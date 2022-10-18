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

        private readonly object mSingletonCreationLockObject = new object();

        private readonly ITypeInformationProvider mTypeInformationProvider;
        private readonly IScopeAwareCreator mCreator;

        private ITypeMappingProvider mMappingProvider;
        private IScope? mScope;
        private ConcurrentBag<IScope>? mScopes = new ConcurrentBag<IScope>();

        private readonly ConcurrentDictionary<Type, object> mGlobalSingletons = new ConcurrentDictionary<Type, object>();

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a instance of <see cref="InjeCtor"/> and initializes it with the default implementations.
        /// </summary>
        public InjeCtor()
            : this(new SimpleTypeMapper(), new TypeInformationBuilder(), new DefaultCreator())
        {

        }

        /// <summary>
        /// Creates a new instance of <see cref="InjeCtor"/> which takes a <see cref="ITypeMapper"/> instance to use.
        /// </summary>
        /// <param name="typeMapper">Implementation of <see cref="ITypeMapper"/> to use.</param>
        public InjeCtor(ITypeMapper typeMapper)
            :this(typeMapper, new TypeInformationBuilder(), new DefaultCreator())
        {

        }

        /// <summary>
        /// Creates a new instance of <see cref="InjeCtor"/> which takes a <see cref="ITypeMapper"/> and <see cref="ITypeInformationProvider"/>
        /// instance to use.
        /// </summary>
        /// <param name="typeMapper">Implementation of <see cref="ITypeMapper"/> to use.</param>
        /// <param name="typeInfoProvider">Implementation of <see cref="ITypeInformationProvider"/> to use.</param>
        public InjeCtor(ITypeMapper typeMapper, ITypeInformationProvider typeInfoProvider)
            :this(typeMapper, typeInfoProvider, new DefaultCreator())
        {

        }

        /// <summary>
        /// Creates a new instance of <see cref="InjeCtor"/> and takes implementations of interfaces to use for operation.
        /// </summary>
        /// <param name="mapper">Implementation of <see cref="ITypeMapper"/> to use.</param>
        /// <param name="typeInfoProvider">Implementation of <see cref="ITypeInformationProvider"/> to use.</param>
        /// <param name="creator">Implementation of <see cref="IScopeAwareCreator"/> to use.</param>
        public InjeCtor(ITypeMapper mapper, ITypeInformationProvider typeInfoProvider, IScopeAwareCreator creator)
        {
            Mapper = mapper;
            mMappingProvider = mapper;

            mTypeInformationProvider = typeInfoProvider;
            mCreator = creator;
            mCreator.SetSingletons(mGlobalSingletons);

            ITypeMapping? mapping = Mapper?.GetTypeMapping<IScope>();

            if (mapping is null || mapping.MappedType is null)
                Mapper?.Add<IScope>().As<Scope.Scope>(); // in case no other implementation defined then use default one

            // setup the default scope
            mScope = CreateAndSetupScope();
        }

        #endregion

        #region IInjeCtor

        /// <inheritdoc/>
        public ITypeMapper Mapper { get; }

        /// <inheritdoc/>
        public ITypeMappingProvider? MappingProvider
        {
            get => mMappingProvider;
            set
            {
                throw new InvalidOperationException($"Setting of '{nameof(MappingProvider)}' is not allowed since it can only be set through constructor for '{typeof(InjeCtor).FullName}'!");
            }
        }

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
                    return CreateSingletonInstance(type);
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
            IScope scope = CreateAndSetupScope();

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
            throw new InvalidOperationException($"Setting the singletons for '{nameof(InjeCtor)}' is not allowed!");
        }

        #endregion

        #region Private Methods

        private IScope CreateAndSetupScope()
        {
            IScope scope = mCreator.Create<IScope>();
            scope.Creator = mCreator;
            scope.TypeInformationProvider = mTypeInformationProvider;
            scope.MappingProvider = MappingProvider;
            scope.Disposing += Scope_Disposing;
            scope.RequestSingletonCreationInstance += Scope_RequestSingletonCreationInstance;

            return scope;
        }

        private object CreateSingletonInstance(Type type)
        {
            if (!mGlobalSingletons.TryGetValue(type, out var instance))
            {
                lock (mSingletonCreationLockObject)
                {
                    // a second check in case a other thread created in the mean time an instance for this type while waiting for the lock.
                    if (!mGlobalSingletons.TryGetValue(type, out instance))
                    {
                        instance = mCreator.Create(type);
                        mGlobalSingletons[type] = instance;

                        //TODO: furhter processing for injection depending on type informations!
                    }
                }
            }

            return instance;
        }

        #endregion

        #region IScope Event handling

        private void Scope_RequestSingletonCreationInstance(object sender, RequestSingletonCreationEventArgs e)
        {
            object instance = CreateSingletonInstance(e.Type);
            e.Instance = instance;
        }

        private void Scope_Disposing(object sender, EventArgs e)
        {
            if (!(sender is IScope scope))
                return;

            scope.Disposing -= Scope_Disposing;
            scope.RequestSingletonCreationInstance -= Scope_RequestSingletonCreationInstance;
        }

        #endregion
    }
}
