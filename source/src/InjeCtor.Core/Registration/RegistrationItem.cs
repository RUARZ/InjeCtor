using System;

namespace InjeCtor.Core.Registration
{
    /// <summary>
    /// Implementation for <see cref="IRegistrationItem{TDef}"/> to use for setup of the registrations.
    /// </summary>
    /// <typeparam name="T">The type to which this registration belongs.</typeparam>
    internal class RegistrationItem<T> : IRegistrationItem<T>
    {
        #region Constructor

        /// <summary>
        /// Creates a new instance of <see cref="RegistrationItem{T}"/> and sets the <see cref="RegistrationType"/>.
        /// </summary>
        public RegistrationItem()
        {
            RegistrationType = typeof(T);
        }

        #endregion

        #region IRegistrationItem

        /// <inheritdoc/>
        public Type RegistrationType { get; }

        /// <inheritdoc/>
        public Type? TargetType { get; private set; }

        /// <inheritdoc/>
        public void As<T1>() where T1 : T
        {
            if (TargetType != null)
                throw new InvalidOperationException("The registration is already set to a target type!");

            TargetType = typeof(T1);
        }

        #endregion
    }
}
