using ESSSM.Configuration;
using ESSSM.Visitors;
using ESSSM.Visitors.Correlation;
using ESSSM.Visitors.Correlation.ExprParsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ESSSM
{
    public static class StateMachineCorrelationExtensions
    {
        public static TReturn CorrelatedBy<TContext, TInput, TReturn>(this IRecordTransitionMetadata<TContext, TInput, TReturn> self,
            Expression<Func<TContext, TInput, bool>> correlationExpr)
        {
            return self.WithMetadata(CorrelationKey.VALUE, correlationExpr);
        }

        public static IEnumerable<CorrelationData<TContext>> GetAllCorrelations<TState, TContext>(this IStateMachine<TState, TContext> self)
        {
            var correlationVisitor = new CorrelationMetadataVisitor<TState, TContext>(new EqualsParser());
            self.VisitInitialTransitions(correlationVisitor);
            return correlationVisitor.Correlations;
        }

        public static IEnumerable<Type> GetStartedByMessageTypes<TState, TContext>(this IStateMachine<TState, TContext> self)
        {
            var correlationVisitor = new StartedByMessageTypesVisitor<TState, TContext>();
            self.VisitInitialTransitions(correlationVisitor);
            return correlationVisitor.MessageTypes;
        }
    }
}
