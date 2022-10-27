using InjeCtor.Core.Registration;
using InjeCtor.Core.Scope;
using System;
using System.Collections.Generic;

namespace InjeCtor.Core.Creation
{
    /// <summary>
    /// Defines methods for creating of types.<br/>
    /// <b>NOTE:</b> this is not intended to already take care of <see cref="CreationInstruction"/>s!
    /// This is only for creating types regardless of the set <see cref="CreationInstruction"/> of the mapping!
    /// </summary>
    public interface ICreator
    {
        /// <summary>
        /// Implementation of <see cref="ITypeMappingProvider"/> to resolve the types.
        /// </summary>
        ITypeMappingProvider? MappingProvider { get; set; }

        /// <summary>
        /// Set's a <see cref="IReadOnlyDictionary{TKey, TValue}"/> to retrieve singletons from it!
        /// </summary>
        /// <param name="singletons"><see cref="IReadOnlyDictionary{TKey, TValue}"/> to retrieve singletons from!</param>
        void SetSingletons(IReadOnlyDictionary<Type, object> singletons);

        /// <summary>
        /// Creates a new instance which was mapped for the passed <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> for which the corresponding mapped type should be created.</param>
        /// <returns>The created type.</returns>
        object Create(Type type);

        /// <summary>
        /// Creates a new instance which was mapped for <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type for which the corresponding mapped type should be created.</typeparam>
        /// <returns>The created type.</returns>
        T Create<T>();
    }

    /// <summary>
    /// Interface for <see cref="ICreator"/> which are aware for <see cref="IScope"/>'s to check for singleton instances.
    /// </summary>
    public interface IScopeAwareCreator : ICreator, ISingletonCreationRequester
    {
        /// <summary>
        /// Creates a new instance which was mapped for the passed <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> for which the corresponding mapped type should be created.</param>
        /// <param name="scope">The <see cref="IScope"/> instance to use for checking of singletons.</param>
        /// <param name="creationHistory">The <see cref="ICreationHistory"/> to use to check for circular references.</param>
        /// <returns>The created type.</returns>
        object Create(Type type, IScope? scope, ICreationHistory? creationHistory = null);

        /// <summary>
        /// Creates a new instance which was mapped for <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type for which the corresponding mapped type should be created.</typeparam>
        /// <param name="scope">The <see cref="IScope"/> instance to use for checking of singletons.</param>
        /// <param name="creationHistory">The <see cref="ICreationHistory"/> to use to check for circular references.</param>
        /// <returns>The created type.</returns>
        T Create<T>(IScope? scope, ICreationHistory? creationHistory = null);


        /// <summary>
        /// Creates the object for the passed <paramref name="type"/> directly without checking / requesting singleton instances.
        /// Needed for e.g. the first creation for a singleton which is, at the point of the request, not in existence yet.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to create.</param>
        /// <param name="scope">Instance of <see cref="IScope"/> if the scope is needed for later needed parameters for other creations.</param>
        /// <param name="creationHistory">The <see cref="ICreationHistory"/> to use to check for circular references.</param>
        /// <returns>The created object.</returns>
        object CreateDirect(Type type, IScope? scope, ICreationHistory creationHistory);

        /// <summary>
        /// Creates the object for the passed <typeparamref name="T"/> directly without checking / requesting singleton instances.
        /// Needed for e.g. the first creation for a singleton which is, at the point of the request, not in existence yet.
        /// </summary>
        /// <typeparam name="T">The type to create.</typeparam>
        /// <param name="scope">Instance of <see cref="IScope"/> if the scope is needed for later needed parameters for other creations.</param>
        /// <param name="creationHistory">The <see cref="ICreationHistory"/> to use to check for circular references.</param>
        /// <returns>The created object.</returns>
        T CreateDirect<T>(IScope? scope, ICreationHistory creationHistory);
    }

    /// <summary>
    /// Interface for proventing a event to request creation of singleton instances.
    /// </summary>
    public interface ISingletonCreationRequester
    {
        /// <summary>
        /// Event which is thrown if a global singleton instance needs to be created which was not already created.
        /// </summary>
        event EventHandler<RequestSingletonCreationEventArgs>? RequestSingletonCreationInstance;
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
        /// <param name="creationHistory">Implementation of <see cref="ICreationHistory"/> to use for further requests.</param>
        public RequestSingletonCreationEventArgs(Type type, ICreationHistory creationHistory)
        {
            Type = type;
            CreationHistory = creationHistory;
        }

        /// <summary>
        /// The type which is requested to be created as global singleton
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Instance of <see cref="ICreationHistory"/> to use for further creation requests.
        /// </summary>
        public ICreationHistory CreationHistory { get; }

        /// <summary>
        /// The created instance for the requested type!
        /// </summary>
        public object? Instance { get; set; }
    }
}
