
using System;
namespace ESSSM
{
    public interface IStateMachine<TState, TContext>
    {
        IStateMachineInstance<TState, TContext> CreateInstance(TContext context);

        void VisitTransitions(IVisitStateTransition<TState, TContext> visitor);

        void VisitInitialTransitions(IVisitStateTransition<TState, TContext> visitor);

        void VisitStates(IVisitState<TState> visitor);

        void VisitInitialState(IVisitState<TState> visitor);
    }
}
