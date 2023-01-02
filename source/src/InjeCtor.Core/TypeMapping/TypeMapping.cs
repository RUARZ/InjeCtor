using System.Runtime.CompilerServices;
using System;

namespace InjeCtor.Core.TypeMapping
{
    /// <summary>
    /// Implementation for <see cref="ITypeMappingBuilder"/> to use for type mappings.
    /// </summary>
    internal class TypeMapping : ITypeMappingBuilder, INotifyOnMappingChangedTypeMapping
    {
        #region Constructor

        /// <summary>
        /// Creates a new instance of <see cref="TypeMapping"/> and set's the passed <paramref name="type"/> as <see cref="SourceType"/>.
        /// </summary>
        /// <param name="type">The source type for the mapping.</param>
        public TypeMapping(Type type)
        {
            SourceType = type;

            CreationInstruction = CreationInstruction.Always;
        }

        #endregion

        #region Interface Implementations

        #region ITypeMappingBuilder

        /// <inheritdoc/>
        public Type SourceType { get; }

        /// <inheritdoc/>
        public Type? MappedType { get; protected set; }

        /// <inheritdoc/>
        public object? Instance { get; protected set; }

        /// <inheritdoc/>
        public CreationInstruction CreationInstruction { get; protected set; }

        /// <inheritdoc/>
        public ITypeMappingBuilder As(Type type)
        {
            if (MappedType != null)
                throw new InvalidOperationException("The registration is already set to a target type!");

            ValidateType(type);

            MappedType = type;

            RaiseMappingChanged();
            return this;
        }

        /// <inheritdoc/>
        public ITypeMappingBuilder AsScopeSingleton(Type type)
        {
            CreationInstruction = CreationInstruction.Scope;

            return As(type);
        }

        /// <inheritdoc/>
        public ITypeMappingBuilder AsSingleton(Type type)
        {
            CreationInstruction = CreationInstruction.Singleton;

            return As(type);
        }

        /// <inheritdoc/>
        public ITypeMappingBuilder AsSingleton(object instance)
        {
            if (MappedType != null)
                throw new InvalidOperationException("The registration is already set to a target type!");

            if (instance is null)
                throw new ArgumentNullException(nameof(instance));

            ValidateType(instance.GetType());

            MappedType = instance.GetType();
            CreationInstruction = CreationInstruction.Singleton;
            Instance = instance;

            RaiseMappingChanged();

            return this;
        }

        #endregion

        #region INotifyOnMappingChangedTypeMapping

        /// <inheritdoc/>
        public event EventHandler? MappingChanged;

        #endregion

        #endregion

        #region Protected Methods

        /// <summary>
        /// Raises the <see cref="MappingChanged"/> event.
        /// </summary>
        protected void RaiseMappingChanged()
        {
            MappingChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Validates if the passed <paramref name="type"/> is valid for the mapping to be used for <see cref="SourceType"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to be validated.</param>
        protected void ValidateType(Type type)
        {
            if (!SourceType.IsAssignableFrom(type))
                throw new InvalidOperationException("The type is not valid for the source type!");
        }

        #endregion
    }

    /// <summary>
    /// Implementation for <see cref="ITypeMappingBuilder{TDef}"/> to use for type mappings.
    /// </summary>
    /// <typeparam name="T">The type to which this registration belongs.</typeparam>
    internal class TypeMapping<T> : TypeMapping, ITypeMappingBuilder<T>, INotifyOnMappingChangedTypeMapping
    {

        #region Constructor

        /// <summary>
        /// Creates a new instance of <see cref="TypeMapping{T}"/> and sets the <see cref="SourceType"/>.
        /// </summary>
        public TypeMapping()
            : base(typeof(T))
        {
        }

        #endregion

        #region ITypeMapping<T>

        /// <inheritdoc/>
        public ITypeMappingBuilder<T> As<TMapped>() where TMapped : T
        {
            if (MappedType != null)
                throw new InvalidOperationException("The registration is already set to a target type!");

            MappedType = typeof(TMapped);

            RaiseMappingChanged();
            return this;
        }

        /// <inheritdoc/>
        public ITypeMappingBuilder<T> AsSingleton<TMapped>() where TMapped : T
        {
            CreationInstruction = CreationInstruction.Singleton;

            return As<TMapped>();
        }

        /// <inheritdoc/>
        public ITypeMappingBuilder<T> AsSingleton<TMapped>(TMapped instance) where TMapped : T
        {
            if (MappedType != null)
                throw new InvalidOperationException("The registration is already set to a target type!");

            CreationInstruction = CreationInstruction.Singleton;
            Instance = instance;

            MappedType = typeof(TMapped);

            RaiseMappingChanged();
            return this;
        }

        /// <inheritdoc/>
        public ITypeMappingBuilder<T> AsScopeSingleton<TMapped>() where TMapped : T
        {
            CreationInstruction = CreationInstruction.Scope;

            return As<TMapped>();
        }

        #endregion        
    }
}
