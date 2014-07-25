using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESSSM.Configuration
{
    public class MetadataKeyAlreadyAddedException : Exception
    {
        public MetadataKeyAlreadyAddedException(string key)
            : base(string.Format("StateMachine metadata with key [{0}] has already been added", key))
        {
        }
    }
}
