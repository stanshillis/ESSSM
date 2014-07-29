using ESSSM.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace ESSSM.Tests.Visitors
{
    public abstract class StateVisitorTest
    {
        public class StateAlreadyVisitedException : Exception { }

        public class TestStateVisitor : IVisitState<States>
        {
            public Dictionary<States, Dictionary<string, List<object>>> StateAndMetadata = new Dictionary<States, Dictionary<string, List<object>>>();
            
            public void VisitState(States state, IDictionary<string, object> metadata)
            {
                if(StateAndMetadata.ContainsKey(state))
                {
                    
                }
                StateAndMetadata.Add(state, new Dictionary<string, List<object>>());

                var storedMeta = StateAndMetadata[state];
                foreach(var kv in metadata)
                {
                    if(!storedMeta.ContainsKey(kv.Key))
                    {
                        storedMeta.Add(kv.Key, new List<object>());
                    }
                    storedMeta[kv.Key].Add(kv.Value);
                }
            }
        }

        public abstract IConfigureStateMachineInitially<States, Ctx> GetStateMachineConfiguration();

        [Fact]
        public void AllConfiguredStatesAreVisited()
        {
            var testVisitor = new TestStateVisitor();

            var sut = GetStateMachineConfiguration()
                            .Initially(States.One)
                                .Await<MessageA>()
                                    .TransitionTo(States.Three)
                            .During(States.Three)
                                .OnEnter(ctx => { })
                            .Build();
            sut.VisitStates(testVisitor);

            Assert.Equal(2, testVisitor.StateAndMetadata.Keys.Count);
            Assert.True(testVisitor.StateAndMetadata.ContainsKey(States.One));
            Assert.True(testVisitor.StateAndMetadata.ContainsKey(States.Three));
        }

        [Fact]
        public void VisitCorrectInitialState()
        {
            var testVisitor = new TestStateVisitor();

            var sut = GetStateMachineConfiguration()
                            .Initially(States.Three)
                                .Await<MessageA>()
                                    .TransitionTo(States.Five)
                            .During(States.Five)
                                .OnEnter(ctx => { })
                            .Build();
            
            sut.VisitInitialState(testVisitor);

            Assert.Equal(1, testVisitor.StateAndMetadata.Keys.Count);
            Assert.True(testVisitor.StateAndMetadata.ContainsKey(States.Three));
        }

    }
}
