using InjeCtor.Core.Resolve;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace InjeCtor.Core.Registration
{
    public class DynamicTypeMapper : IDynamicTypeMapper, ITypeMappingProvider
    {
        #region Private Fields

        private readonly TypeMappingList mMappingList = new TypeMappingList();
        private readonly Func<Assembly[]>? mGetAssemblyFunc;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor for <see cref="DynamicTypeMapper"/>. If this constructor is used
        /// then on <see cref="Resolve"/> it will try to get the assemblies with 
        /// <see cref="AppDomain.Current"/>.<see cref="AppDomain.GetAssemblies"/>. To change the
        /// method how to get the loaded assemblies use the <see cref="DynamicTypeMapper"/> ctor
        /// which takes a <see cref="Func{TResult}"/> parameter!
        /// </summary>
        public DynamicTypeMapper()
            :this(null)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="DynamicTypeMapper"/> and takes a parameter to retrieve
        /// the assemblies to search on <see cref="Resolve"/>.
        /// </summary>
        /// <param name="getAssemblyFunc"><see cref="Func{TResult}"/> to get the assemblies to search for resolving.</param>
        public DynamicTypeMapper(Func<Assembly[]>? getAssemblyFunc)
        {
            mGetAssemblyFunc = getAssemblyFunc;
        }

        #endregion

        #region IDynamicTypeMapper

        /// <inheritdoc/>
        ITypeMapping<T> ITypeMapper.Add<T>()
        {
            return mMappingList.Add(new TypeMapping<T>()); // in this case a normal type mapping is fine.
        }

        /// <inheritdoc/>
        public IDynamicTypeMapping<T> Add<T>()
        {
            return mMappingList.Add(new DynamicTypeMapping<T>());
        }

        /// <inheritdoc/>
        public bool Resolve()
        {
            if (mGetAssemblyFunc != null)
                return Resolve(mGetAssemblyFunc.Invoke() ?? new Assembly[0]);
            
            return Resolve(AppDomain.CurrentDomain.GetAssemblies());
        }

        /// <inheritdoc/>
        public bool Resolve(params Assembly[] assemblies)
        {
            ConcurrentDictionary<Type, ITypeMapping> unfinishedMappings = GetNotFinishedTypeMappings();
            IReadOnlyList<TypeResolver>? resolvers = CreateTypeResolver(unfinishedMappings);
            ConcurrentDictionary<Type, List<Type>> resolvedTypes = ResolveTypes(resolvers, assemblies);        
            return SetMappings(unfinishedMappings, resolvedTypes);
        }

        #endregion

        #region ITypeMappingProvider

        /// <inheritdoc/>
        public ITypeMapping? GetTypeMapping<T>()
        {
            return mMappingList.GetMapping<T>();
        }

        /// <inheritdoc/>
        public ITypeMapping? GetTypeMapping(Type type)
        {
            return mMappingList.GetMapping(type);
        }

        /// <inheritdoc/>
        public IReadOnlyList<ITypeMapping> GetTypeMappings()
        {
            return mMappingList.GetFinishedTypeMappings().ToList().AsReadOnly();
        }

        #endregion

        #region Private Methods

        private ConcurrentDictionary<Type, ITypeMapping> GetNotFinishedTypeMappings()
        {
            ConcurrentDictionary<Type, ITypeMapping> mappings = new ConcurrentDictionary<Type, ITypeMapping>();

            foreach (ITypeMapping unfinishedMapping in mMappingList.GetNotFinishedTypeMappings())
            {
                mappings.TryAdd(unfinishedMapping.SourceType, unfinishedMapping);
            }

            return mappings;
        }

        private IReadOnlyList<TypeResolver> CreateTypeResolver(ConcurrentDictionary<Type, ITypeMapping> mappings)
        {
            List<TypeResolver> resolvers = new List<TypeResolver>();

            foreach (ITypeMapping unfinishedMapping in mappings.Values)
            {
                resolvers.Add(new TypeResolver(unfinishedMapping.SourceType));
            }

            return resolvers;
        }

        private ConcurrentDictionary<Type, List<Type>> ResolveTypes(IEnumerable<TypeResolver> resolvers, Assembly[] assemblies)
        {
            ConcurrentDictionary<Type, List<Type>> resolvedTypes = new ConcurrentDictionary<Type, List<Type>>();

            Parallel.ForEach(resolvers, r =>
            {
                var types = r.Resolve(assemblies);
                if (!types.Any())
                    return;

                if (!resolvedTypes.TryGetValue(r.TypeToResolve, out List<Type> resTypes))
                {
                    resTypes = new List<Type>();
                    resolvedTypes.TryAdd(r.TypeToResolve, resTypes);
                }

                resTypes.AddRange(types);
            });

            return resolvedTypes;
        }

        private bool SetMappings(ConcurrentDictionary<Type, ITypeMapping> unfinishedMappings, ConcurrentDictionary<Type, List<Type>> resolvedTypes)
        {
            bool allMappingsOk = true;
            foreach (ITypeMapping mapping in unfinishedMappings.Values)
            {
                if (!resolvedTypes.TryGetValue(mapping.SourceType, out List<Type> matchingTypes) ||
                    matchingTypes.Count != 1 ||
                    !(mapping is IMappedTypeSetableTypeMapping setableMapping))
                {
                    allMappingsOk = false;
                    continue;
                }

                setableMapping.SetMappedType(matchingTypes.First());
            }
            return allMappingsOk;
        }

        #endregion
    }
}
