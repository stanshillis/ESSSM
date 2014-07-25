using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESSSM.Configuration
{
    public interface IConfigureSimpleTransitionDefinition<TState, TContext, TInput> :
        IConfigureTransitionDestination<TState, IConfigureStateDefinition<TState, TContext>>,
        IConfigurePredicatedTransitionInitially<TState, TContext>,
        IRecordTransitionMetadata<TContext, TInput, IConfigureSimpleTransitionDefinition<TState, TContext, TInput>>
    {
        IConfigureSimpleTransitionDefinition<TState, TContext, TInput> OnReceive(Action<TContext, TInput> handler);
    }
}
