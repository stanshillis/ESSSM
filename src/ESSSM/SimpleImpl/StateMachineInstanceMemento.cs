using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace otdih.Sandbox.ES.ProcessManager.ESSSM.SimpleImpl
{
    public class StateMachineInstanceMemento<TState> : IStateMachineInstanceMemento
    {
        public TState CurrentState { get; set; }

        public IEnumerable<object> { get; set; }
    }
}
