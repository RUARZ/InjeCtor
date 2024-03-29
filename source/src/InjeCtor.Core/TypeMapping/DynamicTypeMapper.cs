﻿using InjeCtor.Core.Resolve;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace InjeCtor.Core.TypeMapping
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
            : this(null)
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
        public event EventHandler<MappingAddedEventArgs>? MappingAdded;

        /// <inheritdoc/>
        ITypeMappingBuilder<T> ITypeMapper.Add<T>()
        {
            return Add<T>();
        }

        /// <inheritdoc/>
        public IDynamicTypeMappingBuilder<T> Add<T>()
        {
            DynamicTypeMapping<T> mapping = new DynamicTypeMapping<T>();
            mapping.MappingChanged += Mapping_MappingChanged;
            return mMappingList.Add(mapping);
        }

        /// <inheritdoc/>
        ITypeMappingBuilder ITypeMapper.Add(Type type)
        {
            return Add(type);
        }

        /// <inheritdoc/>
        public IDynamicTypeMappingBuilder Add(Type type)
        {
            DynamicTypeMapping mapping = new DynamicTypeMapping(type);
            mapping.MappingChanged += Mapping_MappingChanged;
            return mMappingList.Add(mapping);
        }

        /// <inheritdoc/>
        public void AddTransient(Type type)
        {
            Add(type).As(type);
        }

        /// <inheritdoc/>
        public void AddScopeSingleton(Type type)
        {
            Add(type).AsScopeSingleton(type);
        }

        /// <inheritdoc/>
        public void AddSingleton(Type type)
        {
            Add(type).AsSingleton(type);
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

        /// <inheritdoc/>
        public void AddTransient<T>() where T : class
        {
            Add<T>().As<T>();
        }

        /// <inheritdoc/>
        public void AddScopeSingleton<T>() where T : class
        {
            Add<T>().AsScopeSingleton<T>();
        }

        /// <inheritdoc/>
        public void AddSingleton<T>() where T : class
        {
            Add<T>().AsSingleton<T>();
        }

        /// <inheritdoc/>
        public void AddSingleton<T>(T instance) where T : class
        {
            Add<T>().AsSingleton(instance);
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

        #region Event Handling

        private void Mapping_MappingChanged(object sender, EventArgs e)
        {
            if (!(sender is INotifyOnMappingChangedTypeMapping mapping))
                return;

            mapping.MappingChanged -= Mapping_MappingChanged;

            MappingAdded?.Invoke(this, new MappingAddedEventArgs(mapping));
        }

        #endregion
    }
}
