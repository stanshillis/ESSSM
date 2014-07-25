using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ESSSM.SimpleImpl
{
    public abstract class TransitionDefinition<TState, TContext> : IComparable<TransitionDefinition<TState, TContext>>
    {
        private class PredicatedTransition
        {
            public TState NextState;
            public Func<TContext, bool> Predicate;
            public Expression<Func<TContext, bool>> PredicateExpr;
        }

        private List<PredicatedTransition> predicatedNextStates;

        public TransitionDefinition()
        {
            predicatedNextStates = new List<PredicatedTransition>();
        }
        
        protected TState EvaluateNextState(TContext ctx, TState currentState)
        {
            foreach (var entry in predicatedNextStates)
            {
                if (entry.Predicate(ctx))
                {
                    return entry.NextState;
                }
            }
            return currentState;
        }

        public void AddPredicatedNextState(Expression<Func<TContext, bool>> predicate, TState state)
        {
            predicatedNextStates.Add(new PredicatedTransition() { 
                Predicate = predicate.Compile(), 
                PredicateExpr = predicate, 
                NextState = state 
            });
        }

        public abstract bool TryProcessInput(TContext ctx, TState currentState, IEnumerable<object> inputSequence, out TState nextState, out IEnumerable<object> unprocessedInputSequence);

        public abstract int CompareTo(TransitionDefinition<TState, TContext> other);

        public virtual void VisitTransition(TState state, IEnumerable<Type> inputs, IVisitStateTransition<TState, TContext> visitor)
        {
            foreach(var transition in predicatedNextStates)
            {
                visitor.VisitTransition(state, inputs, transition.PredicateExpr, transition.NextState);
            }
        }
    }
}
