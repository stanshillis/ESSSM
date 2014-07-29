using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESSSM.SimpleImpl
{
    public class StateMachineInstance<TState, TContext> : IStateMachineInstance<TState, TContext>
    {
        private InProgressMachineData<TState, TContext> currentState;
        private StateMachine<TState, TContext> stateMachine;
        private TContext context;

        public StateMachineInstance(
            StateMachine<TState, TContext> stateMachine, 
            InProgressMachineData<TState, TContext> initState,
            TContext context)
        {
            this.context = context;
            this.currentState = initState;
            this.stateMachine = stateMachine;

            initState.CurrentState.Enter(context);
        }

        public void Receive(object input)
        {
            currentState = stateMachine.Receive(input, currentState, context);
        }
    }
}
