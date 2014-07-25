using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESSSM.SimpleImpl
{
    public class StateUndefinedException : Exception
    {
        public StateUndefinedException(string message)
            :base(message)
        {

        }
    }
}
