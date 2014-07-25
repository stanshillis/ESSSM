using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESSSM.Configuration
{
    public interface IConfigureStateMachine<TState, TContext>
    {
        IConfigureStateDefinition<TState, TContext> During(TState state);

        IStateMachine<TState, TContext> Build();
    }
}
