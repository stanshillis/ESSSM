using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESSSM.Configuration
{
    public interface IConfigurePredicatedTransition<TState, TContext>
        : IConfigurePredicatedTransitionInitially<TState, TContext>
    {
        IConfigureTransitionDestination<TState, IConfigureStateDefinition<TState, TContext>> Otherwise();
    }
}
