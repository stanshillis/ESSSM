using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESSSM.Configuration
{
    public interface IConfigureTransitionDestination<TState, TReturn>
    {
        TReturn NoTransition();
        TReturn TransitionTo(TState newState);
    }
}
