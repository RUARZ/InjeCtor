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
        }

        #endregion

        #region ITypeMapping<T>

        /// <inheritdoc/>
        public Type SourceType { get; }

        /// <inheritdoc/>
        public Type? MappedType { get; protected set; }

        /// <inheritdoc/>
        public void As<T1>() where T1 : T
        {
            if (MappedType != null)
                throw new InvalidOperationException("The registration is already set to a target type!");

            MappedType = typeof(T1);

            RaiseMappingChanged();
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
