using System;
using System.Collections.Generic;
using System.Text;

namespace InjeCtor.Core.Exceptions
{
    /// <summary>
    /// Represents an <see cref="Exception"/> thrown for try to creation of a type
    /// which results in a circular reference that could not be resolved.
    /// </summary>
    [Serializable]
    public class CircularReferenceException : Exception
    {
        /// <summary>
        /// Creates a new incstance of <see cref="CircularReferenceException"/>.
        /// </summary>
        public CircularReferenceException() { }
        /// <summary>
        /// Creates a new incstance of <see cref="CircularReferenceException"/> and set's it's message.
        /// </summary>
        /// <param name="message">The message for the <see cref="Exception"/>.</param>
        public CircularReferenceException(string message) : base(message) { }
        /// <summary>
        /// Creates a new incstance of <see cref="CircularReferenceException"/>, set's it's message and inner exception.
        /// </summary>
        /// <param name="message">The message for the <see cref="Exception"/>.</param>
        /// <param name="inner">The inner exception which caued this exception.</param>
        public CircularReferenceException(string message, Exception inner) : base(message, inner) { }
        /// <summary>
        /// Creates a new incstance of <see cref="CircularReferenceException"/>.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized 
        /// object data about the exception being thrown.</param>
        /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual information
        /// about the source or destination.</param>
        protected CircularReferenceException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
