using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESSSM
{
    public interface IStateMachineInstance<TState, TContext>
    {
        void Receive(object input);
    }
}
