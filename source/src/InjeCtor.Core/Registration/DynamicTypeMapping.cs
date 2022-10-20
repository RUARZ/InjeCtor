using InjeCtor.Core.Resolve;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace InjeCtor.Core.Registration
{

    /// <summary>
    /// Implementation for dynamic type mappings which support auto resolving of interface implemenations in assemblies.
    /// </summary>
    /// <typeparam name="T">The source type which should be mapped.</typeparam>
    internal class DynamicTypeMapping<T> : TypeMapping<T>, IDynamicTypeMapping<T>, IMappedTypeSetableTypeMapping
    {
        #region IMappedTypeSetableTypeMapping

        /// <inheritdoc/>
        public void SetMappedType(Type type)
        {
            if (!SourceType.IsAssignableFrom(type))
                throw new InvalidOperationException($"Could not set '{type.FullName}' because it is not assignable from '{SourceType.FullName}'!");

            if (MappedType != null)
                throw new InvalidOperationException("The registration is already set to a target type!");

            MappedType = type;

            RaiseMappingChanged();
        }

        #endregion

        #region IDynamicTypeMapping<T>

        /// <inheritdoc/>
        public bool Resolve()
        {
            return Resolve(AppDomain.CurrentDomain.GetAssemblies());
        }

        /// <inheritdoc/>
        public bool Resolve(params Assembly[] assemblies)
        {
            if (MappedType != null)
                return true;

            TypeResolver resolver = new TypeResolver(SourceType);
            IEnumerable<Type>? types = resolver.Resolve(assemblies);

            if (types is null || !types.Any() || types.Skip(1).Any())
                return false;

            SetMappedType(types.First());
            return true;
        }

        /// <inheritdoc/>
        public new IDynamicTypeMapping<T> As<TMapped>() where TMapped : T
        {
            return (IDynamicTypeMapping<T>)base.As<TMapped>();
        }

        /// <inheritdoc/>
        IDynamicTypeMapping<T> IDynamicTypeMapping<T>.AsSingleton()
        {
            return (IDynamicTypeMapping<T>)base.AsSingleton();
        }

        /// <inheritdoc/>
        public new IDynamicTypeMapping<T> AsSingleton<TMapped>(TMapped instance) where TMapped : T
        {
            return (IDynamicTypeMapping<T>)base.AsSingleton(instance);
        }

        /// <inheritdoc/>
        IDynamicTypeMapping<T> IDynamicTypeMapping<T>.AsScopeSingleton()
        {
            return (IDynamicTypeMapping<T>)base.AsScopeSingleton();
        }

        /// <inheritdoc/>
        IDynamicTypeMapping<T> IDynamicTypeMapping<T>.AsTransient()
        {
            return (IDynamicTypeMapping<T>)base.AsTransient();
        }

        #endregion
    }
}
