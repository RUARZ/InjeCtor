using InjeCtor.Core.Creation;
using InjeCtor.Core.Invoke;
using InjeCtor.Core.Scope;
using InjeCtor.Core.TypeInformation;
using InjeCtor.Core.TypeMapping;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace InjeCtor.Core
{
    /// <summary>
    /// The default implementation of <see cref="IInjeCtor"/> and therefore the core / kernel for InjeCtor.
    /// </summary>
    public class InjeCtor : IInjeCtor
    {
        #region Private Fields

        private readonly TypeInformationInjector mTypeInformationInjector = new TypeInformationInjector();
        private readonly IScopeAwareCreator mCreator;

        private ITypeMappingProvider mMappingProvider;
        private IScope mScope;
        private ConcurrentDictionary<IScope, IScope>? mScopes = new ConcurrentDictionary<IScope, IScope>();

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
            : this(typeMapper, new TypeInformationBuilder(), new DefaultCreator())
        {

        }

        /// <summary>
        /// Creates a new instance of <see cref="InjeCtor"/> which takes a <see cref="ITypeMapper"/> and <see cref="ITypeInformationProvider"/>
        /// instance to use.
        /// </summary>
        /// <param name="typeMapper">Implementation of <see cref="ITypeMapper"/> to use.</param>
        /// <param name="typeInfoProvider">Implementation of <see cref="ITypeInformationProvider"/> to use.</param>
        public InjeCtor(ITypeMapper typeMapper, ITypeInformationProvider typeInfoProvider)
            : this(typeMapper, typeInfoProvider, new DefaultCreator())
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

            TypeInformationProvider = typeInfoProvider;
            TypeInformationBuilder = typeInfoProvider as ITypeInformationBuilder;

            if (TypeInformationBuilder != null)
                Mapper.MappingAdded += Mapper_MappingAdded;

            mCreator = creator;
            mCreator.MappingProvider = mMappingProvider;
            mCreator.SetSingletons(mGlobalSingletons);

            AddDefaultMappingsIfNeeded<IScope, Scope.Scope>();
            AddDefaultMappingsIfNeeded<IScopeAwareInvoker, Invoker>();

            // setup the default scope
            mScope = CreateAndSetupScope();

            mGlobalSingletons[typeof(IInjeCtor)] = this;
            // only to add, if not already present, mapping for own interface and as own implementation
            // the mapping type in this case should not realy matter because we already set this instance to
            // global singleton.
            AddDefaultMappingsIfNeeded<IInjeCtor, InjeCtor>(true);
        }

        #endregion

        #region IInjeCtor

        /// <inheritdoc/>
        public ITypeMapper Mapper { get; }

        /// <inheritdoc/>
        public ITypeMappingProvider? MappingProvider { get => mMappingProvider; }

        /// <inheritdoc/>
        public ITypeInformationProvider TypeInformationProvider { get; }

        /// <inheritdoc/>
        public ITypeInformationBuilder? TypeInformationBuilder { get; }

        /// <inheritdoc/>
        public object? Get(Type type)
        {
            return mScope?.Get(type);
        }

        /// <inheritdoc/>
        public T Get<T>()
        {
            return (T)Get(typeof(T));
        }

        /// <inheritdoc/>
        public object? Invoke<TObj>(TObj obj, Expression<Func<TObj, Delegate>> expression, params object?[] parameters)
        {
            return mScope?.Invoke(obj, expression, parameters);
        }

        /// <inheritdoc/>
        public object? Invoke(Expression<Func<Delegate>> expression, params object?[] parameters)
        {
            return mScope?.Invoke(expression, parameters);
        }

        /// <inheritdoc/>
        public IScope CreateScope()
        {
            IScope scope = CreateAndSetupScope();

            mScopes?.TryAdd(scope, scope);
            return scope;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (mScopes != null)
            {
                foreach (IScope scope in mScopes.Values)
                {
                    scope.Dispose();
                }
            }

            // remove the own instance from the singletons to prevent endless call of dispose
            // which will result in stack overflow exception
            mGlobalSingletons.TryRemove(typeof(IInjeCtor), out _);

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

        /// <inheritdoc/>
        public IEnumerable<IScope> GetScopes()
        {
            return mScopes?.Values ?? Enumerable.Empty<IScope>();
        }

        #endregion

        #region Private Methods

        private IScope CreateAndSetupScope()
        {
            IScopeAwareInvoker invoker = mCreator.Create<IScopeAwareInvoker>();
            IScope scope = mCreator.Create<IScope>();

            invoker.Scope = scope;
            scope.Creator = mCreator;
            scope.TypeInformationProvider = TypeInformationProvider;
            scope.MappingProvider = MappingProvider;
            scope.Invoker = invoker;
            scope.Disposing += Scope_Disposing;
            scope.RequestSingletonCreationInstance += Scope_RequestSingletonCreationInstance;

            return scope;
        }

        private object CreateSingletonInstance(Type type, ICreationHistory creationHistory)
        {
            if (!mGlobalSingletons.TryGetValue(type, out var instance))
            {
                ITypeMapping? mapping = Mapper.GetTypeMapping(type);
                if (mapping != null && mapping.CreationInstruction == CreationInstruction.Singleton && mapping.Instance != null)
                    instance = mapping.Instance;
                else
                    instance = mCreator.CreateDirect(type, mScope, creationHistory);

                if (mGlobalSingletons.TryAdd(type, instance))
                {
                    mTypeInformationInjector.InjectProperties(instance, TypeInformationProvider, mCreator, mScope, creationHistory);
                }
                else if (!mGlobalSingletons.TryGetValue(type, out instance))
                {
                    throw new InvalidOperationException($"Failed to add new created instance but also can't get a already registered instance for type '{type}'!");
                }
            }

            return instance;
        }

        private void AddDefaultMappingsIfNeeded<TInterface, TDefaultImplementation>(bool globalSingleton = false) where TDefaultImplementation : TInterface
        {
            ITypeMapping? mapping = Mapper.GetTypeMapping<TInterface>();

            if (mapping != null && mapping.MappedType != null)
                return;

            ITypeMappingBuilder<TInterface> newMapping = Mapper.Add<TInterface>();

            if (globalSingleton)
                newMapping.AsSingleton<TDefaultImplementation>();
            else
                newMapping.AsScopeSingleton<TDefaultImplementation>();
        }

        #endregion

        #region Event handling

        private void Scope_RequestSingletonCreationInstance(object sender, RequestSingletonCreationEventArgs e)
        {
            object instance = CreateSingletonInstance(e.Type, e.CreationHistory);
            e.Instance = instance;
        }

        private void Scope_Disposing(object sender, EventArgs e)
        {
            if (!(sender is IScope scope))
                return;

            scope.Disposing -= Scope_Disposing;
            scope.RequestSingletonCreationInstance -= Scope_RequestSingletonCreationInstance;

            mScopes?.TryRemove(scope, out _);
        }

        private void Mapper_MappingAdded(object sender, MappingAddedEventArgs e)
        {
            if (e?.Mapping?.MappedType != null)
                TypeInformationBuilder?.Add(e.Mapping.MappedType);
        }

        #endregion
    }
}
