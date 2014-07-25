using ESSSM.SimpleImpl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Extensions;

namespace ESSSM.Tests.SimpleImpl
{
    public class TransitionSortabilityTest
    {
        [Fact]
        public void SimpleTransitionsAreSortEquivalent()
        {
            var e1 = new SimpleTransitionDefinition<int, int, int>();
            var e2 = new SimpleTransitionDefinition<int, int, int>();
            var e3 = new SimpleTransitionDefinition<int, int, int>();

            var sut = new TransitionDefinition<int, int>[] {
                e1, e2, e3
            };
            Array.Sort(sut);

            Assert.True(sut[0] == e1, "First element invalid");
            Assert.True(sut[1] == e2, "Second element invalid");
            Assert.True(sut[2] == e3, "Third element invalid");
        }

        [Fact]
        public void ComplexTransitionsWithSameNumOfAwaitsAreSortEquivalent()
        {
            var e1 = new ComplexTransitionDefinition<int, int>();
            var e2 = new ComplexTransitionDefinition<int, int>();
            var e3 = new ComplexTransitionDefinition<int, int>();

            e1.AddAwait<int>();
            e2.AddAwait<int>();
            e3.AddAwait<int>();

            var sut = new TransitionDefinition<int, int>[] {
                e1, e2, e3
            };
            Array.Sort(sut);

            Assert.True(sut[0] == e1, "First element invalid");
            Assert.True(sut[1] == e2, "Second element invalid");
            Assert.True(sut[2] == e3, "Third element invalid");
        }

        [Fact]
        public void ComplexTransitionsWithMoreAwaitsSortToTop()
        {
            var e1 = new ComplexTransitionDefinition<int, int>();
            var e2 = new ComplexTransitionDefinition<int, int>();
            var e3 = new ComplexTransitionDefinition<int, int>();

            e1.AddAwait<int>();
            
            e2.AddAwait<int>();
            e2.AddAwait<string>();
            e2.AddAwait<bool>();
            
            e3.AddAwait<int>();
            e3.AddAwait<string>();

            var sut = new TransitionDefinition<int, int>[] {
                e1, e2, e3
            };
            Array.Sort(sut);

            Assert.True(sut[0] == e2, "First element invalid");
            Assert.True(sut[1] == e3, "Second element invalid");
            Assert.True(sut[2] == e1, "Third element invalid");
        }

        [Fact]
        public void ComplexTransitionsSortToTopComparedToSimple()
        {
            var e1 = new ComplexTransitionDefinition<int, int>();
            var e2 = new SimpleTransitionDefinition<int, int, int>();
            var e3 = new ComplexTransitionDefinition<int, int>();

            e1.AddAwait<int>();

            e3.AddAwait<int>();
            e3.AddAwait<string>();

            var sut = new TransitionDefinition<int, int>[] {
                e1, e2, e3
            };
            Array.Sort(sut);

            Assert.True(sut[0] == e3, "First element invalid");
            Assert.True(sut[1] == e1, "Second element invalid");
            Assert.True(sut[2] == e2, "Third element invalid");
        }
    }
}
