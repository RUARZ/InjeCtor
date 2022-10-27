using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace InjeCtor.Core.Expression
{
    /// <summary>
    /// Helper class for resolving expressions to get <see cref="PropertyInfo"/>'s, <see cref="MemberInfo"/>'s, ...
    /// </summary>
    internal static class ExpressionParser
    {
        #region Public Methods

        /// <summary>
        /// Tries to resolve the passed <paramref name="expression"/> and get a <see cref="PropertyInfo"/> from it.
        /// </summary>
        /// <typeparam name="T">The type from which the propertyinfo should be retrieved.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="expression">The expression to resolve.</param>
        /// <returns>The resolved <see cref="PropertyInfo"/> or <see langword="null"/> if the <paramref name="expression"/> could not be
        /// resolved to get a <see cref="PropertyInfo"/>.</returns>
        public static PropertyInfo? GetPropertyInfo<T, TProperty>(Expression<Func<T, TProperty>> expression)
        {
            MemberExpression? memberExpr = expression.Body as MemberExpression;
            PropertyInfo? pInfo = memberExpr?.Member as PropertyInfo;

            if (pInfo != null)
                return pInfo;

            return null;
        }

        /// <summary>
        /// Tries to resolve the passed <paramref name="expression"/> and get a <see cref="MethodInfo"/> from it.
        /// </summary>
        /// <typeparam name="TObj">The type from which the method info should be retrieved.</typeparam>
        /// <param name="expression">The expression to resolve.</param>
        /// <returns>The resolved <see cref="MethodInfo"/> or <see langword="null"/> if the <paramref name="expression"/> could not
        /// be resolved to a <see cref="MethodInfo"/>.</returns>
        public static MethodInfo? GetMethodInfo<TObj>(Expression<Func<TObj, Delegate>> expression)
        {
            UnaryExpression? unaryExpression = expression.Body as UnaryExpression;
            MethodCallExpression? methodCallExpression = unaryExpression?.Operand as MethodCallExpression;
            ConstantExpression? methodCallObject = methodCallExpression?.Object as ConstantExpression;
            MethodInfo? methodInfo = methodCallObject?.Value as MethodInfo;

            return methodInfo;
        }

        #endregion
    }
}
