using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESSSM.Visitors
{
    public class TransitionToUndefinedStateException : Exception
    {
        public TransitionToUndefinedStateException(int numInvalidStates)
            : base(string.Format("Configured states machine is inconsistent. Has transition to [{0}] state(s) which have not been configured.", numInvalidStates))
        {

        }
    }

    public class TransitionToUndefinedStateException<TState> : TransitionToUndefinedStateException
    {
        public TransitionToUndefinedStateException(IEnumerable<TState> states)
            : base(states.Count())
        {
            this.States = states;
        }

        public IEnumerable<TState> States { get; private set; }
    }
}
