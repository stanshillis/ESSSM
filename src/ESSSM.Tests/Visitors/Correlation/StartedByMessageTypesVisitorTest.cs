
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
    public abstract class StartedByMessageTypesVisitorTest
    {
        public abstract IConfigureStateMachineInitially<States, Ctx> GetStateMachineConfiguration();

        [Fact]
        public void ReturnsMessageTypesForInitialState()
        {
            var sm = GetStateMachineConfiguration()
                        .Initially(States.One)
                            .Await<MessageC>()
                                .NoTransition()
                        .Build();

            var sut = sm.GetStartedByMessageTypes();

            Assert.Equal(1, sut.Count());

            Assert.Equal(typeof(MessageC), sut.First());
        }

        [Fact]
        public void DoesNotReturnMessageTypesForNonInitialStates()
        {
            var sm = GetStateMachineConfiguration()
                        .Initially(States.One)
                            .Await<MessageC>()
                                .NoTransition()
                        .During(States.Two)
                            .Await<MessageB>()
                                .NoTransition()
                        .Build();

            var sut = sm.GetStartedByMessageTypes();

            Assert.Equal(1, sut.Count());

            Assert.Equal(typeof(MessageC), sut.First());
        }

    }
}
