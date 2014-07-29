using ESSSM.Configuration;
using ESSSM.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Xunit;

namespace ESSSM.Tests
{
    public abstract class StateTransitionVisitorTest
    {
        public class TransitionData
        {
            public IEnumerable<Type> InputTypes;
            public Expression<Func<Ctx, bool>> TransitionPredicate;
            public States DestinationState;
        }

        public class TestTransitionVisitor : IVisitStateTransition<States, Ctx>
        {
            public Dictionary<Type, Dictionary<string, List<object>>> Metadata = new Dictionary<Type, Dictionary<string, List<object>>>();
            public Dictionary<States, List<TransitionData>> Transitions = new Dictionary<States, List<TransitionData>>();

            public void VisitTransition(States sourceState, Type inputType, IDictionary<string, object> metadata)
            {
                // simply record eveyrthing seen

                Dictionary<string, List<object>> inputM;
                if (!Metadata.TryGetValue(inputType, out inputM))
                {
                    inputM = new Dictionary<string, List<object>>();
                    Metadata.Add(inputType, inputM);
                }
                foreach(var mkv in metadata)
                {
                    List<object> keyData;
                    if(!inputM.TryGetValue(mkv.Key, out keyData))
                    {
                        keyData = new List<object>();
                        inputM.Add(mkv.Key, keyData);
                    }
                    keyData.Add(mkv.Value);
                }
            }

            public void VisitTransition(States sourceState, IEnumerable<Type> inputTypes, System.Linq.Expressions.Expression<Func<Ctx, bool>> transitionPredicate, States destinationState)
            {
                if(!Transitions.ContainsKey(sourceState))
                {
                    Transitions.Add(sourceState, new List<TransitionData>());
                }
                Transitions[sourceState].Add(new TransitionData()
                {
                    DestinationState = destinationState,
                    InputTypes = inputTypes,
                    TransitionPredicate = transitionPredicate
                });
            }
        }

        public abstract IConfigureStateMachineInitially<States, Ctx> GetStateMachineConfiguration();

        [Fact]
        public void CanAddMetadataToOneTransition()
        {
            var sut = GetStateMachineConfiguration()
                        .Initially(States.One)
                            .Await<MessageA>()
                                .NoTransition()
                            .Await<MessageB>()
                                .WithMetadata("b", 2)
                                .NoTransition()
                        .Build();

            var testVisitor = new TestTransitionVisitor();
            sut.VisitTransitions(testVisitor);

            Assert.Equal(1, testVisitor.Metadata.Count);
            Assert.True(testVisitor.Metadata.ContainsKey(typeof(MessageB)));
            
            Assert.Equal(1, testVisitor.Metadata[typeof(MessageB)].Count);
            Assert.True(testVisitor.Metadata[typeof(MessageB)].ContainsKey("b"));
            
            Assert.Equal(1, testVisitor.Metadata[typeof(MessageB)]["b"].Count);
            Assert.Equal((object)2, testVisitor.Metadata[typeof(MessageB)]["b"][0]);
        }

        [Fact]
        public void CanAddMetadataToSeveralTransitions()
        {
            var sut = GetStateMachineConfiguration()
                        .Initially(States.One)
                            .Await<MessageA>()
                                .NoTransition()
                            .Await<MessageB>()
                                .WithMetadata("b", 2)
                                .TransitionTo(States.Three)
                        .During(States.Three)
                            .Await<MessageC>()
                                .WithMetadata("b", 5)
                                .NoTransition()
                        .Build();

            var testVisitor = new TestTransitionVisitor();
            sut.VisitTransitions(testVisitor);

            Assert.Equal(2, testVisitor.Metadata.Count);
            Assert.True(testVisitor.Metadata.ContainsKey(typeof(MessageB)));
            Assert.True(testVisitor.Metadata.ContainsKey(typeof(MessageC)));

            Assert.Equal(1, testVisitor.Metadata[typeof(MessageB)].Count);
            Assert.True(testVisitor.Metadata[typeof(MessageB)].ContainsKey("b"));

            Assert.Equal(1, testVisitor.Metadata[typeof(MessageB)]["b"].Count);
            Assert.Equal((object)2, testVisitor.Metadata[typeof(MessageB)]["b"][0]);


            Assert.Equal(1, testVisitor.Metadata[typeof(MessageC)].Count);
            Assert.True(testVisitor.Metadata[typeof(MessageC)].ContainsKey("b"));

            Assert.Equal(1, testVisitor.Metadata[typeof(MessageC)]["b"].Count);
            Assert.Equal((object)5, testVisitor.Metadata[typeof(MessageC)]["b"][0]);
        }

        [Fact]
        public void CanAddMetadataMultipleTimes()
        {
            var sut = GetStateMachineConfiguration()
                        .Initially(States.One)
                            .Await<MessageA>()
                                .NoTransition()
                            .Await<MessageB>()
                                .WithMetadata("b", 2)
                                .WithMetadata("c", 25)
                                .NoTransition()
                        .Build();

            var testVisitor = new TestTransitionVisitor();
            sut.VisitTransitions(testVisitor);

            Assert.Equal(1, testVisitor.Metadata.Count);
            Assert.True(testVisitor.Metadata.ContainsKey(typeof(MessageB)));

            Assert.Equal(2, testVisitor.Metadata[typeof(MessageB)].Count);
            Assert.True(testVisitor.Metadata[typeof(MessageB)].ContainsKey("b"));
            Assert.True(testVisitor.Metadata[typeof(MessageB)].ContainsKey("c"));

            Assert.Equal(1, testVisitor.Metadata[typeof(MessageB)]["b"].Count);
            Assert.Equal((object)2, testVisitor.Metadata[typeof(MessageB)]["b"][0]);

            Assert.Equal(1, testVisitor.Metadata[typeof(MessageB)]["c"].Count);
            Assert.Equal((object)25, testVisitor.Metadata[typeof(MessageB)]["c"][0]);
        }

        [Fact]
        public void CanAddMetadataToAwaitAllTransitions()
        {
            var sut = GetStateMachineConfiguration()
                        .Initially(States.One)
                            .AwaitAll()
                                .Await<MessageA>()
                                    .NoTransition()
                                .Await<MessageB>()
                                    .WithMetadata("b", 2)
                                    .WithMetadata("c", 25)
                                .NoTransition()
                        .Build();

            var testVisitor = new TestTransitionVisitor();
            sut.VisitTransitions(testVisitor);

            Assert.Equal(1, testVisitor.Metadata.Count);
            Assert.True(testVisitor.Metadata.ContainsKey(typeof(MessageB)));

            Assert.Equal(2, testVisitor.Metadata[typeof(MessageB)].Count);
            Assert.True(testVisitor.Metadata[typeof(MessageB)].ContainsKey("b"));
            Assert.True(testVisitor.Metadata[typeof(MessageB)].ContainsKey("c"));

            Assert.Equal(1, testVisitor.Metadata[typeof(MessageB)]["b"].Count);
            Assert.Equal((object)2, testVisitor.Metadata[typeof(MessageB)]["b"][0]);

            Assert.Equal(1, testVisitor.Metadata[typeof(MessageB)]["c"].Count);
            Assert.Equal((object)25, testVisitor.Metadata[typeof(MessageB)]["c"][0]);
        }

        [Fact]
        public void AwaitTransitionsConfiguredAndVisitedEqual()
        {
            var sut = GetStateMachineConfiguration()
                        .Initially(States.One)
                            .Await<MessageA>()
                                .NoTransition()
                            .Await<MessageB>()
                                .TransitionTo(States.Two)
                        .During(States.Two)
                            .Await<MessageC>()
                                .NoTransition()
                        .Build();

            var testVisitor = new TestTransitionVisitor();
            sut.VisitTransitions(testVisitor);

            Assert.True(testVisitor.Transitions.ContainsKey(States.One));
            Assert.True(testVisitor.Transitions.ContainsKey(States.Two));
            Assert.Equal(2, testVisitor.Transitions[States.One].Count);
            Assert.Equal(1, testVisitor.Transitions[States.Two].Count);

            var trans1 = testVisitor.Transitions[States.One][0];
            Assert.Equal(1, trans1.InputTypes.Count());
            Assert.Equal(typeof(MessageA), trans1.InputTypes.First());
            Assert.Equal(States.One, trans1.DestinationState);

            var trans2 = testVisitor.Transitions[States.One][1];
            Assert.Equal(1, trans2.InputTypes.Count());
            Assert.Equal(typeof(MessageB), trans2.InputTypes.First());
            Assert.Equal(States.Two, trans2.DestinationState);

            var trans3 = testVisitor.Transitions[States.Two][0];
            Assert.Equal(1, trans3.InputTypes.Count());
            Assert.Equal(typeof(MessageC), trans3.InputTypes.First());
            Assert.Equal(States.Two, trans3.DestinationState);
        }

        [Fact]
        public void AwaitAllTransitionsConfiguredAndVisitedEqual()
        {
            var sut = GetStateMachineConfiguration()
                        .Initially(States.One)
                            .AwaitAll()
                                .Await<MessageA>()
                                .Await<MessageB>()
                                .TransitionTo(States.Two)
                        .During(States.Two)
                            .Await<MessageC>()
                                .NoTransition()
                        .Build();

            var testVisitor = new TestTransitionVisitor();
            sut.VisitTransitions(testVisitor);

            Assert.True(testVisitor.Transitions.ContainsKey(States.One));
            Assert.True(testVisitor.Transitions.ContainsKey(States.Two));
            Assert.Equal(1, testVisitor.Transitions[States.One].Count);
            Assert.Equal(1, testVisitor.Transitions[States.Two].Count);

            var trans1 = testVisitor.Transitions[States.One][0];
            Assert.Equal(2, trans1.InputTypes.Count());
            Assert.Equal(typeof(MessageA), trans1.InputTypes.First());
            Assert.Equal(typeof(MessageB), trans1.InputTypes.Skip(1).First());
            Assert.Equal(States.Two, trans1.DestinationState);

            var trans2 = testVisitor.Transitions[States.Two][0];
            Assert.Equal(1, trans2.InputTypes.Count());
            Assert.Equal(typeof(MessageC), trans2.InputTypes.First());
            Assert.Equal(States.Two, trans2.DestinationState);
        }

        [Fact]
        public void AddingSameMetadataKeyTwiceGeneratesMetadataKeyAlreadyAddedException()
        {
            Assert.Throws<MetadataKeyAlreadyAddedException>(() => GetStateMachineConfiguration()
                .Initially(States.One)
                    .Await<MessageA>()
                        .WithMetadata("Key", "meta")
                        .WithMetadata("Key", "meta 2")
                        .NoTransition()
                    .Build());
        }

        [Fact]
        public void VisitCorrectInitialTransitions()
        {
            var sut = GetStateMachineConfiguration()
                        .Initially(States.Three)
                            .AwaitAll()
                                .Await<MessageA>()
                                .Await<MessageB>()
                                .TransitionTo(States.Two)
                        .During(States.Two)
                            .Await<MessageC>()
                                .NoTransition()
                        .Build();

            var testVisitor = new TestTransitionVisitor();
            sut.VisitInitialTransitions(testVisitor);

            Assert.Equal(1, testVisitor.Transitions.Keys.Count);
            Assert.True(testVisitor.Transitions.ContainsKey(States.Three));
            Assert.Equal(1, testVisitor.Transitions[States.Three].Count);

            var trans1 = testVisitor.Transitions[States.Three][0];
            Assert.Equal(2, trans1.InputTypes.Count());
            Assert.Equal(typeof(MessageA), trans1.InputTypes.First());
            Assert.Equal(typeof(MessageB), trans1.InputTypes.Skip(1).First());
            Assert.Equal(States.Two, trans1.DestinationState);
        }
    }
}
