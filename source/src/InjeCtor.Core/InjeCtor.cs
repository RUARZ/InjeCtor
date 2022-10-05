using InjeCtor.Core.Creation;
using InjeCtor.Core.Registration;
using InjeCtor.Core.Scope;
using InjeCtor.Core.TypeInformation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InjeCtor.Core
{
    /// <summary>
    /// The default implementation of <see cref="IInjeCtor"/> and therefore the core / kernel for InjeCtor.
    /// </summary>
    public class InjeCtor : IInjeCtor
    {
        #region Private Fields

        private readonly ITypeInformationProvider mTypeInformationProvider;
        private readonly IScopeAwareCreator mCreator;
        
        private IScope? mScope;
        private ConcurrentBag<IScope>? mScopes = new ConcurrentBag<IScope>();

        private readonly ConcurrentDictionary<Type, object> mGlobalSingletons = new ConcurrentDictionary<Type, object>();

        #endregion

        #region Constructor

        public InjeCtor()
            : this(new SimpleTypeMapper(), new TypeInformationBuilder(), new DefaultCreator())
        {

        }

        public InjeCtor(ITypeMapper mapper, ITypeInformationProvider typeInfoProvider, IScopeAwareCreator creator)
        {
            Mapper = mapper;
            if (mapper is ITypeMappingProvider mappingProvider)
                MappingProvider = mappingProvider;

            mTypeInformationProvider = typeInfoProvider;
            mCreator = creator;
            mCreator.SetSingletons(mGlobalSingletons);
        }

        #endregion

        #region IInjeCtor

        /// <inheritdoc/>
        public ITypeMapper Mapper { get; }

        /// <inheritdoc/>
        public ITypeMappingProvider? MappingProvider { get; set; }

        /// <inheritdoc/>
        public object Create(Type type)
        {
            ITypeMapping? mapping = MappingProvider?.GetTypeMapping(type);

            if (mapping is null)
                throw new InvalidOperationException($"No mapping found for the type '{type.FullName}'!");

            if (mapping.MappedType is null)
                throw new InvalidOperationException($"The mapped type is null for '{mapping.SourceType.FullName}'!");

            object instance;
            switch (mapping.CreationInstruction)
            {
                case CreationInstruction.Always:
                    instance = mCreator.Create(type);
                    break;
                case CreationInstruction.Scope:
                    instance = mCreator.Create(type, mScope);
                    break;
                case CreationInstruction.Singleton:
                    if (!mGlobalSingletons.TryGetValue(type, out instance))
                    {
                        instance = mCreator.Create(type);
                        mGlobalSingletons[type] = instance;
                    }
                    break;
                default:
                    throw new NotSupportedException($"The creation type '{mapping.CreationInstruction}' seems to not be implemented yet!");
            }

            //TODO: furhter processing for injection depending on type informations!
            return instance;
        }

        /// <inheritdoc/>
        public T Create<T>()
        {
            return (T)Create(typeof(T));
        }

        /// <inheritdoc/>
        public IScope CreateScope()
        {
            IScope scope = new Scope.Scope(mCreator);
            scope.TypeInformationProvider = mTypeInformationProvider;
            scope.MappingProvider = MappingProvider;
            
            //TODO: update to get informations if the scope was disposed already to remove it from the
            // mScopes bag!
            mScopes?.Add(scope);
            return scope;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (mScopes != null)
            {
                foreach (IScope scope in mScopes)
                {
                    scope.Dispose();
                }
            }

            foreach (IDisposable disposable in mGlobalSingletons.Values.Where(x => x is IDisposable))
            {
                disposable.Dispose();
            }
            mGlobalSingletons.Clear();

            mScope?.Dispose();

            mScope = null;
            mScopes = null;
        }

        /// <inheritdoc/>
        public void SetSingletons(IReadOnlyDictionary<Type, object> singletons)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
