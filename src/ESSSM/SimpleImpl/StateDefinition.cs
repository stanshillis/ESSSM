using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESSSM.SimpleImpl
{
    public class StateDefinition<TState, TContext>
    {
        private readonly IDictionary<Type, IList<TransitionDefinition<TState, TContext>>> inputTotransitionsLookup;
        private readonly IDictionary<TransitionDefinition<TState, TContext>, IList<Type>> transitionToInputLookup;
        
        public StateDefinition(TState state)
        {
            OnEnterDelegate = NoOp;
            OnExitDelegate = NoOp;
            inputTotransitionsLookup = new Dictionary<Type, IList<TransitionDefinition<TState, TContext>>>();
            transitionToInputLookup = new Dictionary<TransitionDefinition<TState, TContext>, IList<Type>>();
            this.State = state;
        }

        public Action<TContext> OnEnterDelegate { get; set; }

        public Action<TContext> OnExitDelegate { get; set; }

        private void NoOp(TContext ctx) { }
        
        public TState State { get; private set; }

        public StateDefinition<TState, TContext> RegisterInputHandler(Type inputType, TransitionDefinition<TState, TContext> transition)
        {
            IList<TransitionDefinition<TState, TContext>> transitionList;
            if (!inputTotransitionsLookup.TryGetValue(inputType, out transitionList))
            {
                transitionList = new List<TransitionDefinition<TState, TContext>>();
                inputTotransitionsLookup.Add(inputType, transitionList);
            }
            transitionList.Add(transition);

            IList<Type> associatedInputs;
            if(!transitionToInputLookup.TryGetValue(transition, out associatedInputs))
            {
                associatedInputs = new List<Type>();
                transitionToInputLookup.Add(transition, associatedInputs);
            }
            associatedInputs.Add(inputType);

            return this;
        }

        public void Enter(TContext context)
        {
            OnEnterDelegate(context);
        }

        public bool TryProcessInput(TContext ctx, IEnumerable<object> inputSequence, out TState nextState, out IEnumerable<object> unprocessedInputSequence)
        {
            if (!inputSequence.Any())
            {
                nextState = State;
                unprocessedInputSequence = inputSequence;
                return false;
            }
            Type inputType = inputSequence.First().GetType();

            IList<TransitionDefinition<TState, TContext>> transitionDefs;
            if (!inputTotransitionsLookup.TryGetValue(inputType, out transitionDefs))
            {
                // no transition defined for this input, skip!
                nextState = State;
                unprocessedInputSequence = inputSequence.Skip(1);
                return false;
            }

            nextState = State;

            unprocessedInputSequence = inputSequence;
            foreach(var transition in transitionDefs)
            {
                if(transition.TryProcessInput(
                    ctx,
                    State,
                    unprocessedInputSequence,
                    out nextState,
                    out unprocessedInputSequence)) 
                {
                    if (!State.Equals(nextState))
                    {
                        OnExitDelegate(ctx);
                        return true;
                    }
                }
            }
            
            return false;
        }

        public void VisitTransitions(IVisitStateTransition<TState, TContext> visitor)
        {
            foreach(var kv in transitionToInputLookup)
            {
                kv.Key.VisitTransition(this.State, kv.Value, visitor);
            }
        }

        public void VisitState(IVisitState<TState> visitor)
        {
            visitor.VisitState(this.State, /* no state level metadata available yet */ new Dictionary<string, object>(1));
        }
    }
}
