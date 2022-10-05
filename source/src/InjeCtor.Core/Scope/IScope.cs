using InjeCtor.Core.Creation;
using InjeCtor.Core.TypeInformation;
using System;
using System.Collections.Generic;
using System.Text;

namespace InjeCtor.Core.Scope
{
    /// <summary>
    /// Interface for different scopes for creating / getting instances.
    /// </summary>
    public interface IScope : ICreator, IDisposable
    {
        /// <summary>
        /// Instance of <see cref="ITypeInformationProvider"/> to get informations for types for further
        /// injections.
        /// </summary>
        ITypeInformationProvider? TypeInformationProvider { get; set; }

        /// <summary>
        /// Tries to get already instantiated singleton instance for <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to get an singleton instance for.</typeparam>
        /// <returns>If an singleton instance for <typeparamref name="T"/> exists then the instance will be returned,
        /// otherwise <see langword="null"/> will be returned.</returns>
        object? GetSingleton<T>();

        /// <summary>
        /// Tries to get already instantiated singleton instance for <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to get an singleton instance for.</param>
        /// <returns>If an singleton instance for <paramref name="type"/> exists then the instance will be returned,
        /// otherwise <see langword="null"/> will be returned.</returns>
        object? GetSingleton(Type type);
    }
}
