using ESSSM.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESSSM.SimpleImpl
{
    public class SimpleTransitionDefinition<TState, TContext, TInput> : TransitionDefinition<TState, TContext>
    {
        private IDictionary<string, object> metadata;

        public SimpleTransitionDefinition()
        {
            this.metadata = new Dictionary<string, object>();
            HandlerDelegate = (ctx, m) => { };
        }

        public Action<TContext, TInput> HandlerDelegate { get; private set; }

        public SimpleTransitionDefinition<TState, TContext, TInput> AddOnReceiveHandler(Action<TContext, TInput> handler)
        {
            this.HandlerDelegate = handler;
            return this;
        }

        public override bool TryProcessInput(TContext ctx, TState currentState, IEnumerable<object> inputSequence, out TState nextState, out IEnumerable<object> unprocessedInputSequence)
        {
            if (inputSequence.Any())
            {
                var firstInputEntry = inputSequence.First();
                if (firstInputEntry is TInput)
                {
                    HandlerDelegate(ctx, (TInput)firstInputEntry);
                    unprocessedInputSequence = inputSequence.Skip(1);
                    nextState = EvaluateNextState(ctx, currentState);
                    return true;
                }
            }
            nextState = currentState;
            unprocessedInputSequence = inputSequence;
            return false;
        }

        public override int CompareTo(TransitionDefinition<TState, TContext> other)
        {
            if (other is SimpleTransitionDefinition<TState, TContext, TInput>)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        public void AddMetadata(string key, object metadata)
        {
            if (this.metadata.ContainsKey(key))
            {
                throw new MetadataKeyAlreadyAddedException(key);
            }
            this.metadata.Add(key, metadata);
        }

        public override void VisitTransition(TState state, IEnumerable<Type> inputs, IVisitStateTransition<TState, TContext> visitor)
        {
            base.VisitTransition(state, inputs, visitor);
            if (metadata.Count > 0)
            {
                visitor.VisitTransition(state, typeof(TInput), this.metadata);
            }
        }
    }
}
