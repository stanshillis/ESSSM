using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESSSM
{
    public interface IVisitState<TState>
    {
        void VisitState(TState state, IDictionary<string, object> metadata);
    }
}
