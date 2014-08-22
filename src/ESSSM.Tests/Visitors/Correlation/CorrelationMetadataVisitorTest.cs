
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Xunit;
using ESSSM.Visitors.Correlation;
using ESSSM.Configuration;
using ESSSM.Visitors.Correlation.ExprParsers;

namespace ESSSM.Tests.Visitors.Correlation
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

            var sut = sm.GetAllCorrelations();

            Assert.Equal(2, sut.Count());

            var correlations = sut.ToDictionary(cd => cd.InputType);
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
            var sut = GetStateMachineConfiguration()
                        .Initially(States.One)
                            .Await<MessageC>()
                                .CorrelatedBy((ctx, m) => m.Id == ctx.Id)
                                .NoTransition()
                        .Build();

            Assert.DoesNotThrow(() => sut.GetAllCorrelations());
        }

        [Fact]
        public void SupportEqualsCorrelationWithContextOnTheLeft()
        {
            var sut = GetStateMachineConfiguration()
                        .Initially(States.One)
                            .Await<MessageC>()
                                .CorrelatedBy((ctx, m) => ctx.Id == m.Id)
                                .NoTransition()
                        .Build();

            Assert.DoesNotThrow(() => sut.GetAllCorrelations());
        }

        [Fact]
        public void CorrelationVisitorHasStateMachineExtension()
        {
            var sut = GetStateMachineConfiguration()
                        .Initially(States.One)
                            .Await<MessageC>()
                                .CorrelatedBy((ctx, m) => ctx.Id == m.Id)
                                .NoTransition()
                        .Build();

            var visitor = new CorrelationMetadataVisitor<States, Ctx>(new EqualsParser());
            sut.VisitTransitions(visitor);
            var expected = visitor.Correlations;

            var result = sut.GetAllCorrelations();

            // verify that extension call produces same result as manual visitor call
            Assert.True(expected.Count() > 0);
            var resultCorrIterator = result.GetEnumerator();
            foreach(var expectedCorr in expected)
            {
                resultCorrIterator.MoveNext();
                var resultCorr = resultCorrIterator.Current;

                Assert.True(expectedCorr.Equals(resultCorr));                
            }
        }

    }
}
