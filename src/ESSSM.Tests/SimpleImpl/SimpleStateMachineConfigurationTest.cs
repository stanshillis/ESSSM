﻿using ESSSM.SimpleImpl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESSSM.Tests.SimpleImpl
{
    public class SimpleStateMachineConfigurationTest : StateMachineConfigurationTests
    {
        public override Configuration.IConfigureStateMachineInitially<States, Ctx> GetStateMachineConfiguration()
        {
            return new SimpleStateMachineConfiguration<States, Ctx, object>();
        }
    }
}
