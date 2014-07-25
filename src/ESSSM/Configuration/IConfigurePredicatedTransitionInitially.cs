using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ESSSM.Configuration
{
    public interface IConfigurePredicatedTransitionInitially<TState, TContext>
    {
        IConfigureTransitionDestination<TState, IConfigurePredicatedTransition<TState, TContext>> If(Expression<Func<TContext, bool>> predicate);
    }
}
