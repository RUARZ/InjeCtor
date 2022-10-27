using InjeCtor.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InjeCtor.Core.Creation
{
    internal class DefaultCreationHistory : ICreationHistory
    {
        #region Private Fields

        private readonly Type mInitialType;
        private readonly Stack<Type> mCreationStack = new Stack<Type>();


        #endregion

        #region Constructor

        public DefaultCreationHistory(Type initialType)
        {
            mInitialType = initialType;
        }

        #endregion

        #region ICreationHistory

        /// <inheritdoc/>
        public void Add(Type type)
        {
            if (mCreationStack.Contains(type))
                throw new CircularReferenceException(
                    $"The creation of the type '{mInitialType}' resulted in a circular reference! " +
                    $"The requested type for current creation, which was already on creation history, is '{type}'! " +
                    $"The creation current creation stack is: {string.Join(", ", mCreationStack.Select(x => x.FullName))}");

            mCreationStack.Push(type);
        }

        /// <inheritdoc/>
        public void Remove(Type type)
        {
            if (type == mCreationStack.Peek())
                mCreationStack.Pop();

            return;
            throw new InvalidOperationException($"The type '{type}' does not match current expected type '{mCreationStack.Peek()}' to remove!");
        }

        #endregion
    }
}
