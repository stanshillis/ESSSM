using ESSSM.SimpleImpl;
using ESSSM.Tests.Visitors;
using System;
using System.Linq;
using System.Text;

namespace ESSSM.Tests.SimpleImpl
{
    public class SimpleStateVisitorTest : StateVisitorTest
    {
        public override Configuration.IConfigureStateMachineInitially<States, Ctx> GetStateMachineConfiguration()
        {
            return new SimpleStateMachineConfiguration<States, Ctx, object>();
        }
    }
}
