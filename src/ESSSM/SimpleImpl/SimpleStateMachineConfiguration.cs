using ESSSM.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ESSSM.SimpleImpl
{
    public class ConfigurationData<TState, TContext>
    {
        public Dictionary<TState, StateDefinition<TState, TContext>> stateDefinitions = new Dictionary<TState,StateDefinition<TState,TContext>>();
        public StateDefinition<TState, TContext> initState;
        public StateDefinition<TState, TContext> currentState;
        public ComplexTransitionDefinition<TState, TContext> currentComplexTransition;
        public Expression<Func<TContext, bool>> currentTransitionPredicate;
        public TransitionDefinition<TState, TContext> currentTransition;
    }

    public class SimpleStateMachineConfiguration<TState, TContext, TInput>:
        IConfigureStateMachine<TState, TContext>,
        IConfigureStateMachineInitially<TState, TContext>,
        IConfigureStateDefinition<TState, TContext>,
        IConfigureComplexTransitionDefinition<TState, TContext>,
        IConfigureComplexAwaitTransitionDefinition<TState, TContext, TInput>,
        IConfigureSimpleTransitionDefinition<TState, TContext, TInput>,
        IConfigureTransitionDestination<TState, IConfigurePredicatedTransition<TState, TContext>>,
        IConfigurePredicatedTransition<TState, TContext>
    {
        private readonly ConfigurationData<TState, TContext> config;
        private readonly SimpleTransitionDefinition<TState, TContext, TInput> currentSimpleTransition;
        private readonly Func<IStateMachine<TState, TContext>, IStateMachine<TState, TContext>> buildHook;

        public SimpleStateMachineConfiguration()
            : this(sm => sm)
        {
        }


        public SimpleStateMachineConfiguration(Func<IStateMachine<TState, TContext>, IStateMachine<TState, TContext>> buildHook)
        {
            this.buildHook = buildHook;
            config = new ConfigurationData<TState, TContext>();
        }

        private SimpleStateMachineConfiguration(Func<IStateMachine<TState, TContext>, IStateMachine<TState, TContext>> buildHook, ConfigurationData<TState, TContext> config, SimpleTransitionDefinition<TState, TContext, TInput> currentSimpleTransition)
        {
            this.buildHook = buildHook;
            this.config = config;
            this.currentSimpleTransition = currentSimpleTransition;
        }

        // ----------------------------------------------------------------------------
        // ----------------------------------------------------------------------------
        // ----------------------------------------------------------------------------


        public IStateMachine<TState, TContext> Build()
        {
            return this.buildHook(new StateMachine<TState, TContext>(config.stateDefinitions.Values, config.initState.State));
        }
        
        public IConfigureStateDefinition<TState, TContext> Initially(TState state)
        {
            config.initState = GetCreateStateDefinition(state);
            config.currentState = config.initState;
            return this;
        }

        public IConfigureStateDefinition<TState, TContext> During(TState state)
        {
            config.currentState = GetCreateStateDefinition(state);
            return this;
        }

        private StateDefinition<TState, TContext> GetCreateStateDefinition(TState state)
        {
            StateDefinition<TState, TContext> stateDef;
            if (config.stateDefinitions.ContainsKey(state))
            {
                stateDef = config.stateDefinitions[state];
            }
            else
            {
                stateDef = new StateDefinition<TState, TContext>(state);
                config.stateDefinitions.Add(state, stateDef);
            }
            return stateDef;
        }


        // ----------------------------------------------------------------------------
        // ----------------------------------------------------------------------------
        // ----------------------------------------------------------------------------


        public IConfigureStateDefinition<TState, TContext> OnExit(Action<TContext> onExitDelegate)
        {
            config.currentState.OnExitDelegate = onExitDelegate;
            return this;
        }

        public IConfigureStateDefinition<TState, TContext> OnEnter(Action<TContext> onEnterDelegate)
        {
            config.currentState.OnEnterDelegate = onEnterDelegate;
            return this;
        }

        public IConfigureSimpleTransitionDefinition<TState, TContext, TNewInput> Await<TNewInput>()
        {
            var simpleTrans = new SimpleTransitionDefinition<TState, TContext, TNewInput>();
            config.currentState.RegisterInputHandler(typeof(TNewInput), simpleTrans);
            config.currentTransition = simpleTrans;
            return new SimpleStateMachineConfiguration<TState, TContext, TNewInput>(buildHook, config, simpleTrans);
        }

        public IConfigureComplexTransitionDefinition<TState, TContext> AwaitAll()
        {
            config.currentComplexTransition = new ComplexTransitionDefinition<TState, TContext>();
            config.currentTransition = config.currentComplexTransition;
            return this;
        }

        // ----------------------------------------------------------------------------
        // ----------------------------------------------------------------------------
        // ----------------------------------------------------------------------------


        IConfigureSimpleTransitionDefinition<TState, TContext, TInput> IConfigureSimpleTransitionDefinition<TState, TContext, TInput>.OnReceive(Action<TContext, TInput> handler)
        {
            this.currentSimpleTransition.AddOnReceiveHandler(handler);
            return this;
        }


        // ----------------------------------------------------------------------------
        // ----------------------------------------------------------------------------
        // ----------------------------------------------------------------------------


        IConfigureComplexAwaitTransitionDefinition<TState, TContext, TNewInput> IConfigureComplexTransitionDefinition<TState, TContext>.Await<TNewInput>()
        {
            var simpleTrans = this.config.currentComplexTransition.AddAwait<TNewInput>();
            config.currentState.RegisterInputHandler(typeof(TNewInput), this.config.currentComplexTransition);
            return new SimpleStateMachineConfiguration<TState, TContext, TNewInput>(buildHook, config, simpleTrans);
        }

        public IConfigureComplexTransitionDefinition<TState, TContext> OnReceiveAll(Action<TContext> handler)
        {
            config.currentComplexTransition.AddOnReceiveAllHandler(handler);
            return this;
        }

        public IConfigureComplexTransitionDefinition<TState, TContext> OnReceive(Action<TContext, TInput> handler)
        {
            currentSimpleTransition.AddOnReceiveHandler(handler);
            return this;
        }


        // ----------------------------------------------------------------------------
        // ----------------------------------------------------------------------------
        // ----------------------------------------------------------------------------


        public IConfigureStateDefinition<TState, TContext> NoTransition()
        {
            config.currentTransition.AddPredicatedNextState(ctx => true, config.currentState.State);
            return this;
        }

        public IConfigureStateDefinition<TState, TContext> TransitionTo(TState newState)
        {
            config.currentTransition.AddPredicatedNextState(ctx => true, newState);
            return this;
        }

        public IConfigureTransitionDestination<TState, IConfigurePredicatedTransition<TState, TContext>> If(Expression<Func<TContext, bool>> predicate)
        {
            config.currentTransitionPredicate = predicate;
            return this;
        }


        // ----------------------------------------------------------------------------
        // ----------------------------------------------------------------------------
        // ----------------------------------------------------------------------------


        IConfigurePredicatedTransition<TState, TContext> IConfigureTransitionDestination<TState, IConfigurePredicatedTransition<TState, TContext>>.NoTransition()
        {
            config.currentTransition.AddPredicatedNextState(config.currentTransitionPredicate, config.currentState.State);
            return this;
        }

        IConfigurePredicatedTransition<TState, TContext> IConfigureTransitionDestination<TState, IConfigurePredicatedTransition<TState, TContext>>.TransitionTo(TState newState)
        {
            config.currentTransition.AddPredicatedNextState(config.currentTransitionPredicate, newState);
            return this;
        }


        // ----------------------------------------------------------------------------
        // ----------------------------------------------------------------------------
        // ----------------------------------------------------------------------------


        public IConfigureTransitionDestination<TState, IConfigureStateDefinition<TState, TContext>> Otherwise()
        {
            config.currentTransitionPredicate = (ctx) => true;
            return this;
        }


        // ----------------------------------------------------------------------------
        // ----------------------------------------------------------------------------
        // ----------------------------------------------------------------------------


        public IConfigureSimpleTransitionDefinition<TState, TContext, TInput> WithMetadata(string key, object metadata)
        {
            currentSimpleTransition.AddMetadata(key, metadata);
            return this;
        }

        IConfigureComplexAwaitTransitionDefinition<TState, TContext, TInput> IRecordTransitionMetadata<TContext, TInput, IConfigureComplexAwaitTransitionDefinition<TState, TContext, TInput>>.WithMetadata(string key, object metadata)
        {
            currentSimpleTransition.AddMetadata(key, metadata);
            return this;
        }
    }
}
