using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESSSM.Configuration
{
    public interface IConfigureComplexTransitionDefinition<TState, TContext> :
        IConfigureTransitionDestination<TState, IConfigureStateDefinition<TState, TContext>>,
        IConfigurePredicatedTransitionInitially<TState, TContext>
    {
        IConfigureComplexAwaitTransitionDefinition<TState, TContext, TInput> Await<TInput>();

        IConfigureComplexTransitionDefinition<TState, TContext> OnReceiveAll(Action<TContext> handler);
    }
}
