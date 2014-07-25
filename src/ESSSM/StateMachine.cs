using ESSSM.Configuration;
using ESSSM.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESSSM
{
    public static class StateMachine
    {
        /// <summary>
        /// Default entry point for creating and configuring a state machine
        /// </summary>
        public static IConfigureStateMachineInitially<TState, TContext> Configure<TState, TContext>()
        {
            return new SimpleImpl.SimpleStateMachineConfiguration<TState, TContext, object>(
                MachineVerifierVisitor<TState, TContext>.VerifyOnBuild
            );
        }
    }
}
