using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace InjeCtor.Core.Reflection
{
    /// <summary>
    /// Provides Helper methods for reflection.
    /// </summary>
    internal static class ReflectionHelper
    {
        /// <summary>
        /// Checks for the passed <paramref name="pInfo"/> if the type is nullable or not.
        /// </summary>
        /// <param name="pInfo">The <see cref="ParameterInfo"/> to check if the parameter is nullable.</param>
        /// <param name="info">The <see cref="MemberInfo"/> to check for declaring type for nullable check.</param>
        /// <returns><see langword="True"/> if the type is nullable, otherwise <see langword="false"/>.</returns>
        public static bool IsReferenceTypeNullable(ParameterInfo pInfo, MemberInfo info)
        {
            var classNullableContextAttribute = info.DeclaringType.CustomAttributes
                .FirstOrDefault(c => c.AttributeType.Name == "NullableContextAttribute");

            var paramterNullableAttribute = pInfo.CustomAttributes
                .FirstOrDefault(x => x.AttributeType.Name == "NullableAttribute");

            if (classNullableContextAttribute is null && paramterNullableAttribute is null)
                return true;

            var classNullableContext = classNullableContextAttribute?.ConstructorArguments
                ?.First(x => x.ArgumentType.Name == "Byte").Value;

            var nullableParameterContext = paramterNullableAttribute?.ConstructorArguments
                ?.First(ca => ca.ArgumentType.Name == "Byte").Value;

            nullableParameterContext ??= classNullableContext;

            byte? context = nullableParameterContext as byte?;
            return context == 0 || context == 2;
        }
    }
}
