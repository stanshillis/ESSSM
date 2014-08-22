using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ESSSM.Visitors.Correlation
{
    public class UnexpectedCorrelationExpressionException : Exception
    {
        public UnexpectedCorrelationExpressionException(object state, object input, Expression epxr, Exception other)
            : base(string.Format("Correlation Expression for state [{0}] input [{1}] was in the wrong format. Expected [context.Member == input.Member], received [{2}]", state, input, epxr.ToString()), other)
        {
        }
    }
}
