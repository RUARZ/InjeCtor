using InjeCtor.Core.Registration;
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
}
