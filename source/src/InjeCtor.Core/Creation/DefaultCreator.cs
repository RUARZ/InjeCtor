using InjeCtor.Core.Reflection;
using InjeCtor.Core.Registration;
using InjeCtor.Core.Resolve;
using InjeCtor.Core.Scope;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace InjeCtor.Core.Creation
{
    /// <summary>
    /// The default implementation for <see cref="IScopeAwareCreator"/> which uses <see cref="Activator.CreateInstance(Type)"/>
    /// for the creation of the objects.
    /// </summary>
    public class DefaultCreator : IScopeAwareCreator
    {
        #region Private Fields

        private readonly ConstructorResolver mCtorResolver = new ConstructorResolver();
        private IReadOnlyDictionary<Type, object>? mSingletons;

        #endregion

        #region ICreator

        /// <inheritdoc/>
        public event EventHandler<RequestSingletonCreationEventArgs>? RequestSingletonCreationInstance;

        /// <inheritdoc/>
        public ITypeMappingProvider? MappingProvider { get; set; }

        /// <inheritdoc/>
        public void SetSingletons(IReadOnlyDictionary<Type, object> singletons)
        {
            mSingletons = singletons;
        }

        /// <inheritdoc/>
        public object Create(Type type)
        {
            return Create(type, null, new DefaultCreationHistory(type));
        }

        /// <inheritdoc/>
        public T Create<T>()
        {
            return (T)Create(typeof(T));
        }

        /// <inheritdoc/>
        public object Create(Type type, IScope? scope, ICreationHistory? creationHistory = null)
        {
            return CreateInternal(type, scope, false, creationHistory ?? new DefaultCreationHistory(type));
        }

        /// <inheritdoc/>
        public T Create<T>(IScope? scope, ICreationHistory? creationHistory = null)
        {
            return (T)Create(typeof(T), scope, creationHistory ?? new DefaultCreationHistory(typeof(T)));
        }

        /// <inheritdoc/>
        public object CreateDirect(Type type, IScope? scope, ICreationHistory? creationHistory)
        {
            return CreateInternal(type, scope, true, creationHistory);
        }

        /// <inheritdoc/>
        public T CreateDirect<T>(IScope? scope, ICreationHistory? creationHistory)
        {
            return (T)CreateDirect(typeof(T), scope, creationHistory);
        }

        #endregion

        #region Private Methods

        private object CreateInternal(Type type, IScope? scope, bool createDirect, ICreationHistory creationHistory)
        {
            ITypeMappingProvider providerToUse = GetTypeMappingProvider();
            return Create(type, providerToUse, scope, createDirect, creationHistory);
        }

        private object Create(Type type, ITypeMappingProvider providerToUse, IScope? scope, bool createDirect, ICreationHistory creationHistory)
        {
            ITypeMapping? mapping = providerToUse.GetTypeMapping(type);
            Type creationType = mapping?.MappedType ?? type;

            if (creationType.IsAbstract)
                throw new InvalidOperationException($"Can not create the abstract type '{creationType}'!");
            if (creationType.IsInterface)
                throw new InvalidOperationException($"Can not create a instance of the not mapped interface '{creationType}'!");

            if (!createDirect)
                creationHistory.Add(creationType);
            if (!createDirect && mapping != null && scope != null && mapping.CreationInstruction != CreationInstruction.Always)
            {
                object? singleton = scope.GetSingleton(type);

                if (singleton != null)
                    return singleton;

                if (mSingletons != null && mSingletons.TryGetValue(type, out singleton))
                    return singleton;

                RequestSingletonCreationEventArgs args = new RequestSingletonCreationEventArgs(type, creationHistory);
                RequestSingletonCreationInstance?.Invoke(this, args);

                if (args.Instance != null)
                {
                    creationHistory.Remove(creationType);
                    return args.Instance;
                }
            }

            ConstructorInfo? constructorInfo = mCtorResolver.ResolveConstructorInfo(creationType, providerToUse);
            object createdObject = CreateFromConstructor(constructorInfo, providerToUse, scope, creationHistory);

            if (!createDirect)
                creationHistory.Remove(creationType);

            return createdObject;
        }

        private object CreateFromConstructor(ConstructorInfo? info, ITypeMappingProvider providerToUse, IScope? scope, ICreationHistory creationHistory)
        {
            if (info is null)
                throw new ArgumentNullException(nameof(info), "The constructor info was not set / resolved!");

            ParameterInfo[] parameterInfos = info.GetParameters();
            object?[] parameters = new object[parameterInfos.Length];

            for (int i = 0; i < parameterInfos.Length; i++)
            {
                ParameterInfo pInfo = parameterInfos[i];
                ITypeMapping? mapping = providerToUse.GetTypeMapping(pInfo.ParameterType);
                creationHistory.Add(pInfo.ParameterType);

                if (mapping is null || mapping.MappedType is null)
                {
                    if (pInfo.HasDefaultValue)
                        parameters[i] = pInfo.DefaultValue;
                    else if (pInfo.ParameterType.IsValueType && Nullable.GetUnderlyingType(pInfo.ParameterType) is null)
                        parameters[i] = Activator.CreateInstance(pInfo.ParameterType);
                    else if (!pInfo.ParameterType.IsValueType && !ReflectionHelper.IsReferenceTypeNullable(pInfo, info))
                        parameters[i] = CreateNotMappedType(pInfo.ParameterType, providerToUse, scope, creationHistory);
                    else
                        parameters[i] = null;
                }
                else
                {
                    parameters[i] = Create(pInfo.ParameterType, providerToUse, scope, false, creationHistory);
                }

                creationHistory.Remove(pInfo.ParameterType);
            }

            return info.Invoke(parameters);
        }

        private object CreateNotMappedType(Type type, ITypeMappingProvider providerToUse, IScope? scope, ICreationHistory creationHistory)
        {
            ConstructorInfo[] constructorInfos = type.GetConstructors();

            if (constructorInfos.Length == 1 && !constructorInfos[0].GetParameters().Any())
            {
                creationHistory.Remove(type);
                return Activator.CreateInstance(type); // only default ctor
            }

            return Create(type, providerToUse, scope, true, creationHistory);
        }

        private ITypeMappingProvider GetTypeMappingProvider()
        {
            ITypeMappingProvider? provider = MappingProvider;
            if (provider == null)
                throw new ArgumentNullException(nameof(MappingProvider), $"The {MappingProvider} was not set!");

            return provider;
        }

        #endregion
    }
}
