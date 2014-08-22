using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ESSSM.Visitors
{
    public class StartedByMessageTypesVisitor<TState, TContext> : IVisitStateTransition<TState, TContext>
    {
        public IEnumerable<Type> MessageTypes { get; private set; }

        public void VisitTransition(TState sourceState, IEnumerable<Type> inputTypes, System.Linq.Expressions.Expression<Func<TContext, bool>> transitionPredicate, TState destinationState)
        {
            if (MessageTypes == null)
            {
                MessageTypes = inputTypes;
            }
        }

        public void VisitTransition(TState sourceState, Type inputType, IDictionary<string, object> metadata)
        {
        }
    }
}
