using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESSSM.SimpleImpl
{
    public class ComplexTransitionDefinition<TState, TContext> : TransitionDefinition<TState, TContext>
    {
        private readonly IDictionary<Type, TransitionDefinition<TState, TContext>> unorderedAwaits;
        private Action<TContext> onReceiveAllHandler = ctx => { };

        public ComplexTransitionDefinition()
        {
            this.unorderedAwaits = new Dictionary<Type, TransitionDefinition<TState, TContext>>();
        }

        public SimpleTransitionDefinition<TState, TContext, TInput> AddAwait<TInput>()
        {
            var awaitTrans = new SimpleTransitionDefinition<TState, TContext, TInput>();
            unorderedAwaits.Add(typeof(TInput), awaitTrans);
            return awaitTrans;
        }

        public ComplexTransitionDefinition<TState, TContext> AddOnReceiveAllHandler(Action<TContext> handler)
        {
            this.onReceiveAllHandler = handler;
            return this;
        }

        public override bool TryProcessInput(TContext ctx, TState currentState, IEnumerable<object> inputSequence, out TState nextState, out IEnumerable<object> unprocessedInputSequence)
        {
            var seen = new HashSet<Type>();
            var handled = 0;
            var orderedAwaitHandlers = new List<TransitionDefinition<TState, TContext>>(unorderedAwaits.Count);
            var dedupedInput = new List<object>();

            foreach (var input in inputSequence)
            {
                if (handled == unorderedAwaits.Count)
                {
                    dedupedInput.Add(input);
                    continue;
                }

                var inputType = input.GetType();
                if (!seen.Add(inputType))
                {
                    continue;
                }
                
                dedupedInput.Add(input);

                TransitionDefinition<TState, TContext> awaitHandler;
                if (unorderedAwaits.TryGetValue(inputType, out awaitHandler))
                {
                    handled += 1;
                    orderedAwaitHandlers.Add(awaitHandler);
                }
                else
                {
                    unprocessedInputSequence = inputSequence;
                    nextState = currentState;
                    return false;
                }
            }

            nextState = currentState;
            if (handled == unorderedAwaits.Count)
            {
                inputSequence = dedupedInput;
                foreach (var awaitHandler in orderedAwaitHandlers)
                {
                    TState throwAwayNewState;
                    if (!awaitHandler.TryProcessInput(ctx, currentState, inputSequence, out throwAwayNewState, out inputSequence))
                    {
                        throw new Exception("Something went terribly wrong. Transition that was expected to handle the input did't!");
                    }
                }
                onReceiveAllHandler(ctx);
                nextState = EvaluateNextState(ctx, currentState);
            }
            
            unprocessedInputSequence = inputSequence;            
            return true;
        }

        public override int CompareTo(TransitionDefinition<TState, TContext> other)
        {
            if (other is ComplexTransitionDefinition<TState, TContext>)
            {
                return ((ComplexTransitionDefinition<TState, TContext>)other).unorderedAwaits.Count.CompareTo(this.unorderedAwaits.Count);
            }
            else
            {
                return -1;
            }
        }

        public override void VisitTransition(TState state, IEnumerable<Type> inputs, IVisitStateTransition<TState, TContext> visitor)
        {
            base.VisitTransition(state, inputs, visitor);
            foreach(var trans in unorderedAwaits)
            {
                trans.Value.VisitTransition(state, inputs, visitor);
            }
        }
    }
}
