using System.Runtime.CompilerServices;
using System;

namespace InjeCtor.Core.Registration
{
    /// <summary>
    /// Implementation for <see cref="ITypeMapping{TDef}"/> to use for type mappings.
    /// </summary>
    /// <typeparam name="T">The type to which this registration belongs.</typeparam>
    internal class TypeMapping<T> : ITypeMapping<T>, INotifyOnMappingChangedTypeMapping
    {

        #region Constructor

        /// <summary>
        /// Creates a new instance of <see cref="TypeMapping{T}"/> and sets the <see cref="SourceType"/>.
        /// </summary>
        public TypeMapping()
        {
            SourceType = typeof(T);
            CreationInstruction = CreationInstruction.Always;
        }

        #endregion

        #region ITypeMapping<T>

        /// <inheritdoc/>
        public Type SourceType { get; }

        /// <inheritdoc/>
        public Type? MappedType { get; protected set; }

        /// <inheritdoc/>
        public CreationInstruction CreationInstruction { get; protected set; }

        /// <inheritdoc/>
        public object? Instance { get; protected set; }

        /// <inheritdoc/>
        public ITypeMapping<T> As<TMapped>() where TMapped : T
        {
            if (MappedType != null)
                throw new InvalidOperationException("The registration is already set to a target type!");

            MappedType = typeof(TMapped);

            RaiseMappingChanged();
            return this;
        }

        /// <inheritdoc/>
        public ITypeMapping<T> AsSingleton()
        {
            CreationInstruction = CreationInstruction.Singleton;
            return this;
        }

        /// <inheritdoc/>
        public ITypeMapping<T> AsSingleton<TMapped>(TMapped instance) where TMapped : T
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
        public ITypeMapping<T> AsScopeSingleton()
        {
            CreationInstruction = CreationInstruction.Scope;
            return this;
        }

        /// <inheritdoc/>
        public ITypeMapping<T> AsTransient()
        {
            CreationInstruction = CreationInstruction.Always;
            return this;
        }

        #endregion

        #region INotifyOnMappingChangedTypeMapping

        /// <inheritdoc/>
        public event EventHandler? MappingChanged;

        #endregion

        #region Protected Methods

        /// <summary>
        /// Raises the <see cref="MappingChanged"/> event.
        /// </summary>
        protected void RaiseMappingChanged()
        {
            MappingChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
