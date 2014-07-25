using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESSSM.Visitors
{
    public class MachineVerifierVisitor<TState, TContext> : 
        IVisitStateTransition<TState, TContext>,
        IVisitState<TState>
    {
        private HashSet<TState> fromStates = new HashSet<TState>();
        private HashSet<TState> toStates = new HashSet<TState>();

        public void VisitTransition(TState sourceState, IEnumerable<Type> inputTypes, System.Linq.Expressions.Expression<Func<TContext, bool>> transitionPredicate, TState destinationState)
        {
            toStates.Add(destinationState);
        }

        public void VisitTransition(TState sourceState, Type inputType, IDictionary<string, object> metadata) {}
        
        public void VisitState(TState state, IDictionary<string, object> metadata)
        {
            fromStates.Add(state);
        }

        public void GuardBrokenTransitions()
        {
            if (!toStates.IsSubsetOf(fromStates))
            {
                toStates.ExceptWith(fromStates);
                throw new TransitionToUndefinedStateException<TState>(toStates);
            }
        }

        public static IStateMachine<TState, TContext> VerifyOnBuild(IStateMachine<TState, TContext> stateMachine)
        {
            var verifyingVisitor = new MachineVerifierVisitor<TState, TContext>();
            stateMachine.VisitTransitions(verifyingVisitor);
            stateMachine.VisitStates(verifyingVisitor);

            verifyingVisitor.GuardBrokenTransitions();

            return stateMachine;
        }
    }
}
