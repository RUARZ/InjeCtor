using System;
using System.Collections.Generic;
using System.Text;

namespace InjeCtor.Core.Registration
{
    /// <summary>
    /// Instruction for the creation of a new instance of a type.
    /// </summary>
    public enum CreationInstruction
    {
        /// <summary>
        /// Each request instantiates a new instance.
        /// </summary>
        Always,
        /// <summary>
        /// Only one instance for the scope (-> within a scope it's a 'singleton').
        /// </summary>
        Scope,
        /// <summary>
        /// Singleton for all scopes alive.
        /// </summary>
        Singleton,
    }
}
