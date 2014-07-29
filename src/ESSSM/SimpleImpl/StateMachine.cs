using ESSSM.SimpleImpl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESSSM.Configuration;

namespace ESSSM.SimpleImpl
{
    public class StateMachine<TState, TContext> :
        IStateMachine<TState, TContext>
    {
        private StateDefinition<TState, TContext> initState;
        private Dictionary<TState, StateDefinition<TState, TContext>> stateDefinitions;

        public StateMachine(IEnumerable<StateDefinition<TState, TContext>> machineStates, TState initState)
        {
            this.stateDefinitions = machineStates.ToDictionary(sd => sd.State);
            this.initState = stateDefinitions[initState];
        }

        public InProgressMachineData<TState, TContext> Receive(object input, InProgressMachineData<TState, TContext> inProgressState, TContext context)
        {
            Type inputType = input.GetType();
            StateDefinition<TState, TContext> currentState = inProgressState.CurrentState;
            inProgressState.PendingInput.Add(input);

            IEnumerable<object> inputSequence = inProgressState.PendingInput;
            TState nextStateId;

            while(currentState.TryProcessInput(context, inputSequence, out nextStateId, out inputSequence))
            {
                if (!stateDefinitions.TryGetValue(nextStateId, out currentState))
                {
                    throw new StateUndefinedException(string.Format("Transition to state [{0}] not allowed because it has not been configured", nextStateId));
                }
                currentState.Enter(context);
            }           
            
            return new InProgressMachineData<TState,TContext>(currentState, inputSequence);
        }

        public IStateMachineInstance<TState, TContext> CreateInstance(TContext context)
        {
            return new StateMachineInstance<TState, TContext>(
                this,
                new InProgressMachineData<TState, TContext>(this.initState), 
                context);
        }


        public void VisitTransitions(IVisitStateTransition<TState, TContext> visitor)
        {
            foreach (var state in stateDefinitions.Values)
            {
                state.VisitTransitions(visitor);
            }
        }

        public void VisitInitialTransitions(IVisitStateTransition<TState, TContext> visitor)
        {
            initState.VisitTransitions(visitor);
        }

        public void VisitStates(IVisitState<TState> visitor)
        {
            foreach (var state in stateDefinitions.Values)
            {
                state.VisitState(visitor);
            }
        }       

        public void VisitInitialState(IVisitState<TState> visitor)
        {
            initState.VisitState(visitor);
        }
    }
}
