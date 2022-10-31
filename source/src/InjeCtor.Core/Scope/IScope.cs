using InjeCtor.Core.Creation;
using InjeCtor.Core.Invoke;
using InjeCtor.Core.Registration;
using InjeCtor.Core.TypeInformation;
using System;
using System.Collections.Generic;
using System.Text;

namespace InjeCtor.Core.Scope
{
    /// <summary>
    /// Interface for different scopes for creating / getting instances.
    /// </summary>
    public interface IScope : IInstanceGetter, IInvoker, ISingletonCreationRequester, IDisposable
    {
        /// <summary>
        /// Instance of <see cref="ITypeInformationProvider"/> to get informations for types for further
        /// injections.
        /// </summary>
        ITypeInformationProvider? TypeInformationProvider { get; set; }

        /// <summary>
        /// Implementation of <see cref="ITypeMappingProvider"/> to resolve the types.
        /// </summary>
        ITypeMappingProvider? MappingProvider { get; set; }

        /// <summary>
        /// Instace which implements <see cref="IScopeAwareCreator"/> which can be used for creation of instances.
        /// </summary>
        IScopeAwareCreator? Creator { get; set; }

        /// <summary>
        /// Implementation of <see cref="IScopeAwareInvoker"/> to provide the <see cref="IScopeAwareInvoker"/> methods for the scope.
        /// </summary>
        IInvoker? Invoker { get; set; }

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
        /// Event thrown if the scope is to be disposed to allow correct deregister to <see cref="RequestSingletonCreationInstance"/>.
        /// </summary>
        event EventHandler? Disposing;
    }
}
