using System;

namespace InjeCtor.Core.Registration
{
    /// <summary>
    /// Implementation for <see cref="IRegisteredItem"/> to represent registered items after build of <see cref="IRegistrationBuilder"/>.
    /// </summary>
    internal class RegisteredItem : IRegisteredItem
    {
        #region Constructor

        /// <summary>
        /// Creates a new instance of <see cref="RegisteredItem"/> and sets its registrations.
        /// </summary>
        /// <param name="registeredType">The type for which this registration should be used.</param>
        /// <param name="targetType">The target type for this registration to create / use for <paramref name="registeredType"/>.</param>
        public RegisteredItem(Type registeredType, Type targetType)
        {
            RegisteredType = registeredType;
            TargetType = targetType;
        }

        #endregion

        #region IRegisteredItem

        /// <inheritdoc/>
        public Type RegisteredType { get; }

        /// <inheritdoc/>
        public Type TargetType { get; }

        #endregion
    }
}
