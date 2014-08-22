using ESSSM.SimpleImpl;
using ESSSM.Tests.Visitors.Correlation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESSSM.Tests.SimpleImpl
{
    public class SimpleStartedByMessageTypesVisitorTest : StartedByMessageTypesVisitorTest
    {
        public override Configuration.IConfigureStateMachineInitially<States, Ctx> GetStateMachineConfiguration()
        {
            return new SimpleStateMachineConfiguration<States, Ctx, object>();
        }
    }
}
