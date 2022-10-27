using System;
using InjeCtor.Core.Exceptions;
using System.Collections.Generic;
using System.Text;

namespace InjeCtor.Core.Creation
{
    /// <summary>
    /// Interface to handle creation history and check for a possible circular reference on
    /// creations!
    /// </summary>
    public interface ICreationHistory
    {
        /// <summary>
        /// Add's the passed <paramref name="type"/> to the creation history.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to add to the history.</param>
        /// <exception cref="CircularReferenceException">Thrown if the type is still within the history!</exception>
        void Add(Type type);

        /// <summary>
        /// Removes the <paramref name="type"/> from the history.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to remove from the history.</param>
        void Remove(Type type);
    }
}
