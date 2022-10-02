using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace InjeCtor.Core.TypeInformation
{
    /// <summary>
    /// Interface for providing / adding special type informations to mark what should be injected.
    /// </summary>
    public interface ITypeInformationProvider
    {
        /// <summary>
        /// Get's a <see cref="ITypeInformation"/> for <typeparamref name="T"/> and returns it. If no information found
        /// then <see langword="null"/> will be returned.
        /// </summary>
        /// <typeparam name="T">The type for which a information should be retrieved.</typeparam>
        /// <returns>The found <see cref="ITypeInformation"/> or <see langword="null"/> if no info could be found.</returns>
        ITypeInformation? Get<T>();

        /// <summary>
        /// Get's a <see cref="ITypeInformation"/> for <paramref name="type"/> and returns it. If no information found
        /// then <see langword="null"/> will be returned.
        /// </summary>
        /// <returns>The found <see cref="ITypeInformation"/> or <see langword="null"/> if no info could be found.</returns>
        ITypeInformation? Get(Type type);
    }

    /// <summary>
    /// Interface for adding type Informations.
    /// </summary>
    public interface ITypeInformationBuilder
    {
        /// <summary>
        /// Add's information for the passed <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type for which informations should be added.</typeparam>
        /// <returns><see langword="True"/> on success, otherwise <see langword="false"/>.</returns>
        bool Add<T>();

        /// <summary>
        /// Add's information for the passed <paramref name="type"/>.
        /// </summary>
        /// <returns><see langword="True"/> on success, otherwise <see langword="false"/>.</returns>
        bool Add(Type type);
    }

    /// <summary>
    /// Informations gathered or set for specific types about injections.
    /// </summary>
    public interface ITypeInformation
    {
        /// <summary>
        /// The type to which this information belongs.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// A dictionary of the Properties which are expecting a type to be injected.
        /// </summary>
        IReadOnlyDictionary<Type, IReadOnlyList<PropertyInfo>> PropertiesToInject { get; }
    }
}
