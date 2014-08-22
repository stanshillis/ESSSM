using ESSSM.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ESSSM.Visitors.Correlation
{   
    public class CorrelationMetadataVisitor<TState, TContext> : IVisitStateTransition<TState,TContext>
    {
        private readonly IParseCorrelationExpr correlationExpressionParser;

        private IList<CorrelationData<TContext>> correlationData = new List<CorrelationData<TContext>>();

        public IEnumerable<CorrelationData<TContext>> Correlations { get { return correlationData; } }
        

        public CorrelationMetadataVisitor(IParseCorrelationExpr correlationExpressionParser)
        {
            this.correlationExpressionParser = correlationExpressionParser;
        }

        public void VisitTransition(TState sourceState, IEnumerable<Type> inputTypes, System.Linq.Expressions.Expression<Func<TContext, bool>> transitionPredicate, TState destinationState) { }

        public void VisitTransition(TState sourceState, Type inputType, IDictionary<string, object> metadata)
        {
            object valueObj;
            if (!metadata.TryGetValue(CorrelationKey.VALUE, out valueObj))
            {
                return;
            }
            var correlationExpr = (LambdaExpression)valueObj;
            try
            {
                CorrelationData<TContext> parsedCorrelation;
                if(correlationExpressionParser.TryParse<TContext>(correlationExpr, out parsedCorrelation))
                {
                    this.correlationData.Add(parsedCorrelation);
                }
                else 
                {
                    throw new Exception("Failed to parse correlation expression");
                }
            }
            catch(Exception e)
            {
                throw new UnexpectedCorrelationExpressionException(sourceState, inputType, correlationExpr, e);
            }
        }
    }
}