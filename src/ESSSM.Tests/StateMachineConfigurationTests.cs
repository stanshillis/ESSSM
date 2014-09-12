using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using ESSSM.Configuration;

namespace ESSSM.Tests
{
    public abstract class StateMachineConfigurationTests
    {
        public abstract IConfigureStateMachineInitially<States, Ctx> GetStateMachineConfiguration();

        private IStateMachine<States, Ctx> BuildBasicMachine()
        {
            return GetStateMachineConfiguration()
                        .Initially(States.One)
                            .OnEnter(ctx => { ctx.CurrentState = States.One; ctx.Count -= 3; })
                            .OnExit(ctx => ctx.Count += 3)
                            .Await<MessageA>()
                                .TransitionTo(States.Two)
                        .During(States.Two)
                            .OnEnter(ctx => { ctx.Count += 10; ctx.CurrentState = States.Two; })
                            .OnExit(ctx => ctx.Count -= 10)
                        .Build();
        }

        [Fact]
        public void CanBuildBasicFromConfiguration()
        {
            var sut = BuildBasicMachine();
            Assert.NotNull(sut);
        }

        [Fact]
        public void TransitionToStateTwoWhenMessageAIsReceived()
        {
            var ctx = new Ctx();
            var sut = BuildBasicMachine().CreateInstance(ctx);

            sut.Receive(new MessageA());
            Assert.Equal(States.Two, ctx.CurrentState);
            Assert.Equal<int>(10, ctx.Count);
        }

        [Fact]
        public void MachineInstancesAreIndependent()
        {
            var ctx1 = new Ctx();
            var ctx2 = new Ctx() { Count = 100 };
            var sut = BuildBasicMachine();

            var m1 = sut.CreateInstance(ctx1);
            m1.Receive(new MessageA());

            var m2 = sut.CreateInstance(ctx2);            
            m2.Receive(new MessageA());

            Assert.Equal<int>(10, ctx1.Count);
            Assert.Equal<int>(110, ctx2.Count);
        }

        [Fact]
        public void InvalidInputIsIgnored()
        {
            var ctx = new Ctx();
            var sut = BuildBasicMachine().CreateInstance(ctx);


            sut.Receive(new MessageB()); // no transition on MessageB in state One is defined
            sut.Receive(new MessageA());

            Assert.Equal(States.Two, ctx.CurrentState);
        }

        [Fact]
        public void InitialStateIsEnteredWhenInstanceIsCreated()
        {
            var ctx = new Ctx();

            var sut = BuildBasicMachine().CreateInstance(ctx);

            Assert.Equal(States.One, ctx.CurrentState);
            Assert.Equal(-3, ctx.Count);
        }

        [Fact]
        public void SupportMultipleTransitionDefinitions()
        {
            var context = new Ctx();

            var sut = GetStateMachineConfiguration()
                        .Initially(States.One)
                            .OnEnter(ctx => ctx.CurrentState = States.One)
                            .Await<MessageA>()
                                .TransitionTo(States.Two)
                            .Await<MessageB>()
                                .TransitionTo(States.Three)
                        .During(States.Two)
                            .OnEnter(ctx => ctx.CurrentState = States.Two)
                            .Await<MessageA>()
                                .TransitionTo(States.Two)
                            .Await<MessageB>()
                                .TransitionTo(States.One)
                        .During(States.Three)
                            .OnEnter(ctx => ctx.CurrentState = States.Three)
                        .Build().CreateInstance(context);

            Assert.Equal(States.One, context.CurrentState);
            sut.Receive(new MessageA());
            
            Assert.Equal(States.Two, context.CurrentState);
            sut.Receive(new MessageA());
            Assert.Equal(States.Two, context.CurrentState);
            sut.Receive(new MessageB());
            Assert.Equal(States.One, context.CurrentState);

            sut.Receive(new MessageB());
            Assert.Equal(States.Three, context.CurrentState);
        }

        [Fact]
        public void NoTransitionShouldStayInCurrentState()
        {
            var context = new Ctx();

            var sut = GetStateMachineConfiguration()
                        .Initially(States.One)
                            .OnEnter(ctx => ctx.CurrentState = States.One)
                            .Await<MessageA>()
                                .TransitionTo(States.Two)
                        .During(States.Two)
                            .OnEnter(ctx => ctx.CurrentState = States.Two)
                            .Await<MessageB>()
                                .NoTransition()
                        .Build().CreateInstance(context);

            Assert.Equal(States.One, context.CurrentState);
            
            sut.Receive(new MessageA());
            Assert.Equal(States.Two, context.CurrentState);

            sut.Receive(new MessageB());
            Assert.Equal(States.Two, context.CurrentState);
        }

        [Fact]
        public void ConditionalTransitionsAreEvaluatedInTheOrderRegistered()
        {
            var context = new Ctx();

            var sut = GetStateMachineConfiguration()
                        .Initially(States.One)
                            .OnEnter(ctx => ctx.CurrentState = States.One)
                            .Await<MessageA>()
                                .If(ctx => ctx.Count == 0)
                                    .TransitionTo(States.Two)
                                .If(ctx => ctx.Count >= 0)
                                    .TransitionTo(States.Three)
                                .Otherwise()
                                    .NoTransition()
                        .During(States.Two)
                            .OnEnter(ctx => ctx.CurrentState = States.Two)
                            .Await<MessageA>()
                                .TransitionTo(States.One)
                        .During(States.Three)
                            .OnEnter(ctx => ctx.CurrentState = States.Three)
                            .Await<MessageA>()
                                .TransitionTo(States.One)
                        .Build().CreateInstance(context);

            context.Count = 0;
            sut.Receive(new MessageA());
            Assert.Equal(States.Two, context.CurrentState);

            sut.Receive(new MessageA());
            Assert.Equal(States.One, context.CurrentState);
            context.Count = 1;
            sut.Receive(new MessageA());
            Assert.Equal(States.Three, context.CurrentState);

            sut.Receive(new MessageA());
            Assert.Equal(States.One, context.CurrentState);
            context.Count = -1;
            sut.Receive(new MessageA());
            Assert.Equal(States.One, context.CurrentState);
        }

        private IStateMachine<States, Ctx> BuildComplexMachine()
        {
            return GetStateMachineConfiguration()
                        .Initially(States.One)
                            .OnEnter(ctx => ctx.CurrentState = States.One )
                            .AwaitAll()
                                .Await<MessageA>()
                                    .OnReceive((ctx, m) => ctx.Count += 1)
                                .Await<MessageB>()
                                    .OnReceive((ctx, m) => ctx.Count += 2)
                                .OnReceiveAll(ctx => ctx.Count += 3) // total by the end should be 1 + 2 + 3
                                .TransitionTo(States.Two)                                    
                        .During(States.Two)
                            .OnEnter(ctx => ctx.CurrentState = States.Two)
                            .Await<MessageB>()
                                .TransitionTo(States.Three)
                        .During(States.Three)
                            .OnEnter(ctx => ctx.CurrentState = States.Three)
                        .Build();
        }

        [Fact]
        public void WaitForAllInputBeforeExecutingReceiveHandlers()
        {
            var context = new Ctx();
            var sut = BuildComplexMachine().CreateInstance(context);

            sut.Receive(new MessageA());
            sut.Receive(new MessageB());

            Assert.Equal(States.Two, context.CurrentState);
            Assert.Equal(6, context.Count);            
        }

        [Fact]
        public void WaitAllReceiveOrderDoesNotMatter()
        {
            var context1 = new Ctx();
            var sut1 = BuildComplexMachine().CreateInstance(context1);

            var context2 = new Ctx();
            var sut2 = BuildComplexMachine().CreateInstance(context2);

            sut1.Receive(new MessageB());
            sut1.Receive(new MessageA());

            sut2.Receive(new MessageA());
            sut2.Receive(new MessageB());            

            Assert.Equal(States.Two, context1.CurrentState);
            Assert.Equal(6, context1.Count);

            Assert.Equal(States.Two, context2.CurrentState);
            Assert.Equal(6, context2.Count);
        }

        [Fact]
        public void WaitAllHandledDuplicates()
        {
            var context = new Ctx();
            var sut = BuildComplexMachine().CreateInstance(context);

            sut.Receive(new MessageA());
            sut.Receive(new MessageA());
            sut.Receive(new MessageA());
            sut.Receive(new MessageB());


            Assert.Equal(States.Two, context.CurrentState);
            Assert.Equal(6, context.Count);
        }

        [Fact]
        public void WaitAllIsNotGreedy()
        {
            var context = new Ctx();
            var sut = BuildComplexMachine().CreateInstance(context);

            sut.Receive(new MessageA());
            sut.Receive(new MessageA());
            sut.Receive(new MessageA());
            sut.Receive(new MessageB()); // should go to state Two
            sut.Receive(new MessageB()); // expected to be handled on State Two, which leads to State Three
            

            Assert.Equal(States.Three, context.CurrentState);
        }

        [Fact]
        public void PredicatedTransitionAreRespected()
        {
            var context = new Ctx();

            var sut = GetStateMachineConfiguration()
                            .Initially(States.One)
                                .OnEnter(ctx => ctx.CurrentState = States.One)
                                .Await<MessageA>()
                                    .If(ctx => ctx.Count == 10)
                                        .TransitionTo(States.Two)
                                    .Otherwise().NoTransition()
                            .During(States.Two)
                                .OnEnter(ctx => ctx.CurrentState = States.Two)
                            .Build().CreateInstance(context);

            sut.Receive(new MessageA());
            Assert.Equal(States.One, context.CurrentState);

            context.Count = 10;
            sut.Receive(new MessageA());
            Assert.Equal(States.Two, context.CurrentState);
        }

        [Fact]
        public void PredicatedTransitionAreEvaluatedInOrder()
        {
            var context = new Ctx();

            var sut = GetStateMachineConfiguration()
                            .Initially(States.One)
                                .OnEnter(ctx => ctx.CurrentState = States.One)
                                .Await<MessageA>()
                                    .If(ctx => ctx.Count > 10)
                                        .TransitionTo(States.Two)
                                    .If(ctx => ctx.Count > 0)
                                        .TransitionTo(States.Three)
                                    .Otherwise().NoTransition()
                            .During(States.Two)
                                .OnEnter(ctx => ctx.CurrentState = States.Two)
                                .Await<MessageB>()
                                    .TransitionTo(States.One)
                            .During(States.Three)
                                .OnEnter(ctx => ctx.CurrentState = States.Three)                                
                            .Build().CreateInstance(context);

            context.Count = 11;
            sut.Receive(new MessageA());
            Assert.Equal(States.Two, context.CurrentState);

            sut.Receive(new MessageB());
            Assert.Equal(States.One, context.CurrentState);

            context.Count = 5;
            sut.Receive(new MessageA());
            Assert.Equal(States.Three, context.CurrentState);
        }

        [Fact]
        public void UnhandledHandlerIsNotCalledWhenMessageIsProcessed()
        {
            var context = new Ctx();

            var sut = GetStateMachineConfiguration()
                            .Initially(States.One)
                                .Await<MessageA>().TransitionTo(States.Two)
                            .During(States.Two)
                                .Await<MessageB>().NoTransition()
                            .Build().CreateInstance(context);
            
            var handlerCalled = false;
            sut.Receive(new MessageA(), ctx => handlerCalled = true);

            Assert.False(handlerCalled);
        }

        [Fact]
        public void UnhandledHandlerIsCalledWhenMessageIsNotProcessed()
        {
            var context = new Ctx();

            var sut = GetStateMachineConfiguration()
                            .Initially(States.One)
                                .Await<MessageA>().TransitionTo(States.Two)
                            .During(States.Two)
                                .Await<MessageB>().NoTransition()
                            .Build().CreateInstance(context);

            var handlerCalled = false;
            sut.Receive(new MessageC(), ctx => handlerCalled = true);

            Assert.True(handlerCalled);
        }
    }
}
