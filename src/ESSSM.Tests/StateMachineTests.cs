using ESSSM.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace ESSSM.Tests
{
    public class StateMachineTests
    {
        [Fact]
        public void BuildVerifiesStateAwaitTransitions()
        {
            var sut = StateMachine.Configure<States, Ctx>()
                        .Initially(States.One)
                            .Await<MessageA>()
                                .TransitionTo(States.Three);

            Assert.Throws<TransitionToUndefinedStateException<States>>(() => sut.Build());
        }

        [Fact]
        public void BuildVerifiesStateAwaitAllTransitions()
        {
            var sut = StateMachine.Configure<States, Ctx>()
                        .Initially(States.One)
                            .AwaitAll()
                                .Await<MessageA>()
                                .Await<MessageB>()
                                .TransitionTo(States.Three);

            Assert.Throws<TransitionToUndefinedStateException<States>>(() => sut.Build());
        }
    }
}
