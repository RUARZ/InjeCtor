using InjeCtor.Core.Resolve;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace InjeCtor.Core.TypeMapping
{
    internal class DynamicTypeMapping : TypeMapping, IDynamicTypeMappingBuilder, IMappedTypeSetableTypeMapping
    {
        #region Constructor

        /// <summary>
        /// Creates a new instance of <see cref="DynamicTypeMapping"/> and set's the passed <paramref name="type"/> as <see cref="SourceType"/>.
        /// </summary>
        /// <param name="type">The source type for the mapping.</param>
        public DynamicTypeMapping(Type type)
            : base(type)
        {

        }

        #endregion

        #region Interface Implementations

        #region IDynamicTypeMappingBuilder

        /// <inheritdoc/>
        public bool Resolve()
        {
            // TODO: for .NET may be changed to AssemblyLoadContext
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

        #endregion

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

        #endregion
    }

    /// <summary>
    /// Implementation for dynamic type mappings which support auto resolving of interface implemenations in assemblies.
    /// </summary>
    /// <typeparam name="T">The source type which should be mapped.</typeparam>
    internal class DynamicTypeMapping<T> : DynamicTypeMapping, IDynamicTypeMappingBuilder<T>, IMappedTypeSetableTypeMapping
    {
        #region Constructor

        /// <summary>
        /// Creates a new instance of <see cref="DynamicTypeMapping"/> and set's the <see cref="SourceType"/> to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The source type which should be mapped.</typeparam>
        public DynamicTypeMapping()
            : base(typeof(T))
        {

        }

        #endregion

        #region Interface Implementations

        #region IDynamicTypeMappingBuilder<T>

        /// <inheritdoc/>
        public IDynamicTypeMappingBuilder<T> As<TMapped>() where TMapped : T
        {
            if (MappedType != null)
                throw new InvalidOperationException("The registration is already set to a target type!");

            MappedType = typeof(TMapped);

            RaiseMappingChanged();
            return this;
        }

        /// <inheritdoc/>
        public IDynamicTypeMappingBuilder<T> AsScopeSingleton()
        {
            CreationInstruction = CreationInstruction.Scope;

            RaiseMappingChanged();
            return this;
        }

        /// <inheritdoc/>
        public ITypeMappingBuilder<T> AsScopeSingleton<TMapped>() where TMapped : T
        {
            CreationInstruction = CreationInstruction.Scope;

            return As<TMapped>();
        }

        /// <inheritdoc/>
        public IDynamicTypeMappingBuilder<T> AsSingleton()
        {
            CreationInstruction = CreationInstruction.Singleton;

            RaiseMappingChanged();
            return this;
        }

        /// <inheritdoc/>
        public ITypeMappingBuilder<T> AsSingleton<TMapped>(TMapped instance) where TMapped : T
        {
            AsSingleton<TMapped>();

            Instance = Instance;

            return this;
        }

        /// <inheritdoc/>
        public ITypeMappingBuilder<T> AsSingleton<TMappedType>() where TMappedType : T
        {
            CreationInstruction = CreationInstruction.Singleton;

            return As<TMappedType>();
        }

        /// <inheritdoc/>
        ITypeMappingBuilder<T> ITypeMappingBuilder<T>.As<TMappedType>()
        {
            return As<TMappedType>();
        }

        #endregion

        #endregion
    }
}
