using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESSSM.Configuration
{
    public interface IRecordTransitionMetadata<TContext, TInput, TReturn>
    {
        TReturn WithMetadata(string key, object metadata);
    }
}
