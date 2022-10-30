using InjeCtor.Core.Expression;
using InjeCtor.Core.Reflection;
using InjeCtor.Core.Scope;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace InjeCtor.Core.Invoke
{
    /// <summary>
    /// The default implementation of <see cref="IInvoker"/>.
    /// </summary>
    internal class Invoker : IScopeAwareInvoker
    {
        #region IInvoker

        /// <inheritdoc/>
        public IScope? Scope { get; set; }

        /// <inheritdoc/>
        public object? Invoke<TObj>(TObj obj, Expression<Func<TObj, Delegate>> expression, params object?[] parameters)
        {
            if (Scope is null)
                throw new InvalidOperationException("Scope is not set and therefore can't invoke methods!");

            MethodInfo? methodInfo = ExpressionParser.GetMethodInfo(expression);

            if (methodInfo == null)
                throw new InvalidOperationException("The passed expression could not be resolved to invoke a method!");

            return ExecuteMethod(methodInfo, obj, parameters);
        }

        /// <inheritdoc/>
        public object? Invoke(Expression<Func<Delegate>> expression, params object?[] parameters)
        {
            if (Scope is null)
                throw new InvalidOperationException("Scope is not set and therefore can't invoke methods!");

            MethodInfo? methodInfo = ExpressionParser.GetMethodInfo(expression);

            if (methodInfo == null)
                throw new InvalidOperationException("The passed expression could not be resolved to invoke a method!");

            return ExecuteMethod(methodInfo, null, parameters);
        }

        #endregion

        #region Private Methods

        private object? ExecuteMethod(MethodInfo method, object? obj, params object?[] parameters)
        {
            List<object?> parameterList = new List<object?>(parameters);
            ParameterInfo[] methodParameters = method.GetParameters();
            object?[] parametersForMethod = new object?[methodParameters.Length];

            for (int i = 0; i < methodParameters.Length; i++)
            {
                if (Scope?.MappingProvider?.GetTypeMapping(methodParameters[i].ParameterType) != null)
                {
                    parametersForMethod[i] = Scope.Create(methodParameters[i].ParameterType);
                }
                else
                {
                    object? para = FindFirstMatchingParameter(methodParameters[i].ParameterType, parameterList);

                    if (para is null && !ReflectionHelper.IsReferenceTypeNullable(methodParameters[i], method))
                    {
                        para = Scope?.Create(methodParameters[i].ParameterType);
                    }
                    
                    parametersForMethod[i] = para;
                }
            }

            return method.Invoke(obj, parametersForMethod);
        }

        private object? FindFirstMatchingParameter(Type type, List<object?> parameters)
        {
            for (int i = 0; i < parameters.Count; i++)
            {
                object? para = parameters[i];
                if (para?.GetType() == type)
                {
                    parameters.RemoveAt(i);
                    return para;
                }
            }

            return null;
        }

        #endregion
    }
}
