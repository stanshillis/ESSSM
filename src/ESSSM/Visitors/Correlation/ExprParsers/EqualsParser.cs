using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ESSSM.Visitors.Correlation.ExprParsers
{
    // Parses basic equality correlation expression of the form
    // 
    //     message.MemberX == context.MemberY
    //     -- OR --
    //     context.MemberY == message.MemberX
    // 
    public class EqualsParser : IParseCorrelationExpr
    {
        public bool TryParse<TContext>(LambdaExpression correlationExpr, out CorrelationData<TContext> parsedCorrelation)
        {
            if (correlationExpr.Body.NodeType == ExpressionType.Equal)
            {
                var equalsExpr = (BinaryExpression)correlationExpr.Body;
                var leftExpr = (MemberExpression)equalsExpr.Left;
                var rightExpr = (MemberExpression)equalsExpr.Right;

                var contextExpr = leftExpr;
                var inputExpr = rightExpr;

                if (rightExpr.Expression.Type == typeof(TContext))
                {
                    contextExpr = rightExpr;
                    inputExpr = leftExpr;
                }

                var contextValDelegate = LambdaExpression.Lambda<Func<TContext, object>>(contextExpr, correlationExpr.Parameters[0]).Compile();

                var delegateType = Expression.GetFuncType(inputExpr.Expression.Type, typeof(object));
                var inputValDelegate = LambdaExpression.Lambda(delegateType, inputExpr, correlationExpr.Parameters[1]).Compile();

                parsedCorrelation = new CorrelationData<TContext>(
                    inputExpr.Expression.Type,
                    contextValDelegate,
                    inputValDelegate,
                    inputExpr.Member.Name,
                    contextExpr.Member.Name);
                return true;
            }
            else
            {
                parsedCorrelation = null;
                return false;
            }
        }
    }
}
