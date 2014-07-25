using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESSSM.SimpleImpl
{
    public class InProgressMachineData<TState, TContext>
    {
        public InProgressMachineData(StateDefinition<TState, TContext> state)
            : this(state, new object[0])
        {
        }

        public InProgressMachineData(StateDefinition<TState, TContext> state, IEnumerable<object> pendingInput)
        {
            this.CurrentState = state;
            this.PendingInput = new List<object>(pendingInput);
        }

        public StateDefinition<TState, TContext> CurrentState { get; private set; }

        public IList<object> PendingInput { get; private set; }
    }
}
