using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESSSM.Configuration
{
    public interface IConfigureComplexAwaitTransitionDefinition<TState, TContext, TInput> :
        IConfigureComplexTransitionDefinition<TState, TContext>,
        IRecordTransitionMetadata<TContext, TInput, IConfigureComplexAwaitTransitionDefinition<TState, TContext, TInput>>
    {
        IConfigureComplexTransitionDefinition<TState, TContext> OnReceive(Action<TContext, TInput> handler);
    }
}
