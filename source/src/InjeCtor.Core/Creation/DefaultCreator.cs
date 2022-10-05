using InjeCtor.Core.Registration;
using InjeCtor.Core.Resolve;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace InjeCtor.Core.Creation
{
    /// <summary>
    /// The default implementation for <see cref="ICreator"/> which uses <see cref="Activator.CreateInstance(Type)"/>
    /// for the creation of the objects.
    /// </summary>
    public class DefaultCreator : ICreator
    {
        #region Private Fields

        private readonly ConstructorResolver mCtorResolver = new ConstructorResolver();
        private IReadOnlyDictionary<Type, object>? mSingletons;

        #endregion

        #region ICreator

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
            ITypeMappingProvider providerToUse = GetTypeMappingProvider();
            return Create(type, providerToUse);
        }

        /// <inheritdoc/>
        public T Create<T>()
        {
            return (T)Create(typeof(T));
        }

        #endregion

        #region Private Methods

        private object Create(Type type, ITypeMappingProvider providerToUse)
        {
            Type creationType = providerToUse.GetTypeMapping(type)?.MappedType ?? type;

            if (mSingletons != null && mSingletons.TryGetValue(creationType, out object singleton))
                return singleton;

            ConstructorInfo? constructorInfo = mCtorResolver.ResolveConstructorInfo(creationType, providerToUse);
            object createdObject = CreateFromConstructor(constructorInfo, providerToUse);
            return createdObject;
        }

        private object CreateFromConstructor(ConstructorInfo? info, ITypeMappingProvider providerToUse)
        {
            if (info is null)
                throw new ArgumentNullException(nameof(info), "The constructor info was not set / resolved!");

            ParameterInfo[] parameterInfos = info.GetParameters();
            object?[] parameters = new object[parameterInfos.Length];

            for (int i = 0; i < parameterInfos.Length; i++)
            {
                ParameterInfo pInfo = parameterInfos[i];
                ITypeMapping? mapping = providerToUse.GetTypeMapping(pInfo.ParameterType);

                if (mapping is null || mapping.MappedType is null)
                {
                    if (pInfo.HasDefaultValue)
                        parameters[i] = pInfo.DefaultValue;
                    else if (pInfo.ParameterType.IsValueType && Nullable.GetUnderlyingType(pInfo.ParameterType) is null)
                        parameters[i] = Activator.CreateInstance(pInfo.ParameterType);
                    else if (!pInfo.ParameterType.IsValueType && !IsReferenceTypeNullable(pInfo, info))
                        parameters[i] = Activator.CreateInstance(pInfo.ParameterType);
                    else
                        parameters[i] = null;
                }
                else
                {
                    parameters[i] = Create(mapping.MappedType, providerToUse);
                }
            }

            return info.Invoke(parameters);
        }

        private bool IsReferenceTypeNullable(ParameterInfo pInfo, ConstructorInfo ctorInfo)
        {
            var classNullableContextAttribute = ctorInfo.DeclaringType.CustomAttributes
                .FirstOrDefault(c => c.AttributeType.Name == "NullableContextAttribute");

            var paramterNullableAttribute = pInfo.CustomAttributes
                .FirstOrDefault(x => x.AttributeType.Name == "NullableAttribute");

            if (classNullableContextAttribute is null && paramterNullableAttribute is null)
                return true;

            var classNullableContext = classNullableContextAttribute?.ConstructorArguments
                ?.First(x => x.ArgumentType.Name == "Byte").Value;

            var nullableParameterContext = paramterNullableAttribute?.ConstructorArguments
                ?.First(ca => ca.ArgumentType.Name == "Byte").Value;

            nullableParameterContext ??= classNullableContext;

            byte? context = nullableParameterContext as byte?;
            return context == 0 || context == 2;
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
