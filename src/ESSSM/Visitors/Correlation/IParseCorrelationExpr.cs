using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ESSSM.Visitors.Correlation
{
    public interface IParseCorrelationExpr
    {
        bool TryParse<TContext>(LambdaExpression correlationExpr, out CorrelationData<TContext> parsedCorrelation);
    }
}
