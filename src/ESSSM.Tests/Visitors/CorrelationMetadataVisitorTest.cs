
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Xunit;
using ESSSM.Visitors;
using ESSSM.Configuration;

namespace ESSSM.Tests
{
    public abstract class CorrelationMetadataVisitorTest
    {
        public abstract IConfigureStateMachineInitially<States, Ctx> GetStateMachineConfiguration();

        [Fact]
        public void CorrelationDataIsExtractedCorrectly()
        {
            var sm = GetStateMachineConfiguration()
                        .Initially(States.One)
                            .Await<MessageC>()
                                .CorrelatedBy((ctx, m) => m.Id == ctx.Id)
                                .NoTransition()
                            .Await<MessageB>()
                                .CorrelatedBy((ctx, m) => ctx.Id == m.MessageId)
                                .WithMetadata("b", 2)
                                .NoTransition()
                        .Build();

            var sut = new CorrelationMetadataVisitor<States, Ctx>();
            sm.VisitTransitions(sut);

            Assert.Equal(2, sut.Correlations.Count());

            var correlations = sut.Correlations.ToDictionary(cd => cd.InputType);
            Assert.True(correlations.ContainsKey(typeof(MessageC)));
            Assert.True(correlations.ContainsKey(typeof(MessageB)));

            var testContext = new Ctx() { Id = "CTX" };
            var testMessageC = new MessageC() { Id = "C" };
            var testMessageB = new MessageB() { MessageId = "B" };

            Assert.Equal(testContext.Id, correlations[typeof(MessageC)].EvalUnderContext(testContext));
            Assert.Equal(testMessageC.Id, correlations[typeof(MessageC)].EvalUnderInput(testMessageC));
            Assert.Equal("Id", correlations[typeof(MessageC)].Id);

            Assert.Equal(testContext.Id, correlations[typeof(MessageB)].EvalUnderContext(testContext));
            Assert.Equal(testMessageB.MessageId, correlations[typeof(MessageB)].EvalUnderInput(testMessageB));
            Assert.Equal("MessageId", correlations[typeof(MessageB)].Id);
        }

        [Fact]
        public void SupportEqualsCorrelationWithInputOnTheLeft()
        {
            var sm = GetStateMachineConfiguration()
                        .Initially(States.One)
                            .Await<MessageC>()
                                .CorrelatedBy((ctx, m) => m.Id == ctx.Id)
                                .NoTransition()
                        .Build();

            var sut = new CorrelationMetadataVisitor<States, Ctx>();
            Assert.DoesNotThrow(() => sm.VisitTransitions(sut));
        }

        [Fact]
        public void SupportEqualsCorrelationWithContextOnTheLeft()
        {
            var sm = GetStateMachineConfiguration()
                        .Initially(States.One)
                            .Await<MessageC>()
                                .CorrelatedBy((ctx, m) => ctx.Id == m.Id)
                                .NoTransition()
                        .Build();

            var sut = new CorrelationMetadataVisitor<States, Ctx>();
            Assert.DoesNotThrow(() => sm.VisitTransitions(sut));
        }

    }
}
