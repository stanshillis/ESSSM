using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESSSM.Configuration
{
    public interface IConfigureStateMachineInitially<TState, TContext>
    {
        IConfigureStateDefinition<TState, TContext> Initially(TState state);
    }
}
