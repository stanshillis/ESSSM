using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ESSSM
{
    public interface IVisitStateTransition<TState, TContext>
    {
        void VisitTransition(TState sourceState, IEnumerable<Type> inputTypes, Expression<Func<TContext, bool>> transitionPredicate, TState destinationState);

        void VisitTransition(TState sourceState, Type inputType, IDictionary<string, object> metadata);
    }
}
