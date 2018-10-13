using System;
using System.Linq.Expressions;

namespace SpiceSharpParser.Parsers.Expression
{
    public static class ExpressionExtensions
    {
        public static Expression<Func<T, double>> Negate<T>(this Expression<Func<T, double>> exp)
        {
            var param = exp.Parameters[0];
            var body = System.Linq.Expressions.Expression.Negate(exp.Body);
            return System.Linq.Expressions.Expression.Lambda<Func<T, double>>(body, param);
        }
    }
}
