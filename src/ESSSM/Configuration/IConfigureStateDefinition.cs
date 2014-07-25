using System;
namespace ESSSM.Configuration
{
    public interface IConfigureStateDefinition<TState, TContext> : IConfigureStateMachine<TState, TContext>
    {
        IConfigureStateDefinition<TState, TContext> OnExit(Action<TContext> onExitDelegate);

        IConfigureStateDefinition<TState, TContext> OnEnter(Action<TContext> onEnterDelegate);

        IConfigureSimpleTransitionDefinition<TState, TContext, TInput> Await<TInput>();

        IConfigureComplexTransitionDefinition<TState, TContext> AwaitAll();
    }
}
