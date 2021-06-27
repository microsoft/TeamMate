using System;
using System.Globalization;
using System.Linq.Expressions;

namespace Microsoft.Tools.TeamMate.Foundation.Validation
{
    public static class FluentUtilities
    {
        public static string GetPropertyName<T>(Expression<Func<T>> expression, bool compound = true)
        {
            Expression body = expression.Body;
            return GetMemberName(body, compound);
        }

        public static string GetMemberName(Expression expression, bool compound = true)
        {
            var memberExpression = expression as MemberExpression;

            if (memberExpression != null)
            {
                if (compound && memberExpression.Expression.NodeType == ExpressionType.MemberAccess)
                {
                    return GetMemberName(memberExpression.Expression) + "." + memberExpression.Member.Name;
                }

                return memberExpression.Member.Name;
            }

            var unaryExpression = expression as UnaryExpression;

            if (unaryExpression != null)
            {
                if (unaryExpression.NodeType != ExpressionType.Convert)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Cannot interpret member from {0}",expression));
                }

                return GetMemberName(unaryExpression.Operand);
            }

            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Could not determine member from {0}",expression));
        }

        public static Func<T> Compile<T>(Expression<Func<T>> propertyExpression)
        {
            return Expression.Lambda<Func<T>>(propertyExpression.Body).Compile();
        }
    }
}
