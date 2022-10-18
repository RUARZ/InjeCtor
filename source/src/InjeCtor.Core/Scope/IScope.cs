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
        /// Instace which implements <see cref="IScopeAwareCreator"/> which can be used for creation of instances.
        /// </summary>
        IScopeAwareCreator? Creator { get; set; }

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

        /// <summary>
        /// Event which is thrown if a global singleton instance needs to be created which was not already created.
        /// </summary>
        event EventHandler<RequestSingletonCreationEventArgs>? RequestSingletonCreationInstance;

        /// <summary>
        /// Event thrown if the scope is to be disposed to allow correct deregister to <see cref="RequestSingletonCreationInstance"/>.
        /// </summary>
        event EventHandler? Disposing;
    }

    /// <summary>
    /// Event args for requesting a creation of a singleton. Event args also used to get the created instance if successfull!
    /// </summary>
    public class RequestSingletonCreationEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new instance of <see cref="RequestSingletonCreationEventArgs"/>.
        /// </summary>
        /// <param name="type">The type which is requested to create.</param>
        public RequestSingletonCreationEventArgs(Type type)
        {
            Type = type;
        }

        /// <summary>
        /// The type which is requested to be created as global singleton
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// The created instance for the requested type!
        /// </summary>
        public object? Instance { get; set; }
    }
}
