using System;
using System.Collections.Generic;
using System.Text;

namespace InjeCtor.Core.Registration
{
    /// <summary>
    /// Builder to add registrations and set definitions for them.
    /// </summary>
    public interface IRegistrationBuilder
    {
        /// <summary>
        /// Add's a new registration for <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type which should be registered.</typeparam>
        /// <returns>A <see cref="IRegistrationItem{TDef}"/> instance for further configuration.</returns>
        IRegistrationItem<T> Add<T>();

        /// <summary>
        /// Builds the list with the registration data for further processing.
        /// </summary>
        void Build();

        /// <summary>
        /// Information if the added registrations where build to valid <see cref="IRegisteredItem"/>'s.
        /// </summary>
        bool IsBuild { get; }

        /// <summary>
        /// The build information for the registrations after calling <see cref="Build"/>.
        /// </summary>
        /// <returns>A <see cref="IReadOnlyList{T}"/> of the created <see cref="IRegisteredItem"/> instances.</returns>
        IReadOnlyList<IRegisteredItem> GetRegisteredItems();

        /// <summary>
        /// Get's the <see cref="IRegisteredItem"/> instance for the type <typeparamref name="T"/> if there was
        /// a registration added and the registrations are already build!
        /// </summary>
        /// <typeparam name="T">The type for which a <see cref="IRegisteredItem"/> entry should be retrieved.</typeparam>
        /// <returns>The <see cref="IRegisteredItem"/> entry if found, otherwise <see langword="null"/>!</returns>
        IRegisteredItem? GetRegistration<T>();
    }

    /// <summary>
    /// Interface for fully registered items to get informations for it.
    /// </summary>
    public interface IRegisteredItem
    {
        /// <summary>
        /// The type which was registered. E.g. the interface or abstract class.
        /// </summary>
        Type RegisteredType { get; }

        /// <summary>
        /// The target type which should be initialized.
        /// </summary>
        Type TargetType { get; }
    }

    /// <summary>
    /// Represents items for registration.
    /// </summary>
    public interface IRegistrationItem
    {
        /// <summary>
        /// The type which should be registered.
        /// </summary>
        Type RegistrationType { get; }

        /// <summary>
        /// The setup target type, if already set.
        /// </summary>
        Type? TargetType { get; }
    }

    /// <summary>
    /// Registration item for further definitions / settings.
    /// </summary>
    public interface IRegistrationItem<TDef> : IRegistrationItem
    {
        /// <summary>
        /// Set's the type which should be used for the registration.
        /// </summary>
        /// <typeparam name="T">The type which should be used.</typeparam>
        void As<T>() where T : TDef;
    }
}
