// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace FodyTools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Fody;

    using Mono.Cecil;

    internal static class TypeSystemExtensionMethods
    {
        public static string GetFullName(this Type type)
        {
            // type.FullName may contain extra generic info!
            return type.Namespace + "." + type.Name;
        }

        public static Type GetDeclaringType(this MemberInfo memberInfo)
        {
            return memberInfo.DeclaringType ?? throw new InvalidOperationException("Invalid expression, MemberInfo does not have a declaring type.");

        }

        public static void GetMemberInfo(this LambdaExpression expression, out Type declaringType, out string memberName)
        {
            switch (expression.Body)
            {
                case MemberExpression memberExpression:
                    memberName = memberExpression.Member.Name;
                    declaringType = memberExpression.Member.GetDeclaringType();
                    break;

                default:
                    throw new ArgumentException("Only member expression is supported.", nameof(expression));
            }
        }

        public static void GetMethodInfo(this LambdaExpression expression, out Type declaringType, out string methodName, out IReadOnlyList<Type> argumentTypes)
        {
            switch (expression.Body)
            {
                case NewExpression newExpression:
                    methodName = ".ctor";
                    declaringType = newExpression.Type;
                    argumentTypes = newExpression.Arguments.Select(a => a.Type).ToReadOnlyList();
                    break;

                case MethodCallExpression methodCall:
                    methodName = methodCall.Method.Name;
                    declaringType = methodCall.Method.GetDeclaringType();
                    argumentTypes = methodCall.Arguments.Select(a => a.Type).ToReadOnlyList();
                    break;

                default:
                    throw new ArgumentException("Only method call or new expression is supported.", nameof(expression));
            }
        }

        public static bool ParametersMatch(this IList<ParameterDefinition> parameters, IReadOnlyList<Type> argumentTypes)
        {
            if (parameters.Count != argumentTypes.Count)
                return false;

            var genericParameterMap = new Dictionary<string, string>();

            for (var i = 0; i < parameters.Count; i++)
            {
                var parameterType = parameters[i].ParameterType;
                var argumentType = argumentTypes[i].GetFullName();

                if (parameterType.ContainsGenericParameter)
                {
                    // for generic parameters just verify that every generic type matches to the same placeholder type.
                    var elementTypeName = parameterType.GetElementType().FullName;

                    if (genericParameterMap.TryGetValue(elementTypeName, out var mappedType))
                    {
                        if (mappedType != argumentType)
                            return false;
                    }
                    else
                    {
                        genericParameterMap.Add(elementTypeName, argumentType);
                    }
                }
                else if (parameterType.GetElementType().FullName != argumentType)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
