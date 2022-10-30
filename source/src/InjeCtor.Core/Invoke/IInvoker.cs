using InjeCtor.Core.Scope;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace InjeCtor.Core.Invoke
{
    /// <summary>
    /// Interface for invoke of methods with injecting known types.
    /// </summary>
    public interface IInvoker
    {
        /// <summary>
        /// Invoke the method defined in the <paramref name="expression"/> and inject all possible parameters.
        /// </summary>
        /// <typeparam name="TObj">Type of the object on which to invoke the method.</typeparam>
        /// <param name="obj">The object on which the method should be invoked.</param>
        /// <param name="expression">Expression to get the method to invoke.</param>
        /// <param name="parameters">Additional parameters if not all can be determined automatically.</param>
        object? Invoke<TObj>(TObj obj, Expression<Func<TObj, Delegate>> expression, params object?[] parameters);

        /// <summary>
        /// Invoke the method, of a static class, defined in the <paramref name="expression"/> and inject all possible parameters.
        /// </summary>
        /// <param name="expression">Expression to get the method to invoke.</param>
        /// <param name="parameters">Additional parameters if not all can be determined automatically.</param>
        object? Invoke(Expression<Func<Delegate>> expression, params object?[] parameters);
    }

    /// <summary>
    /// <see cref="IInvoker"/> which is aware of <see cref="IScope"/> to use for creations / getting instances.
    /// </summary>
    public interface IScopeAwareInvoker : IInvoker
    {
        /// <summary>
        /// The <see cref="IScope"/> to use for invokations.
        /// </summary>
        IScope? Scope { get; set; }
    }
}
