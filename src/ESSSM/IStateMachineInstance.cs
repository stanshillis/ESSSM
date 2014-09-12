using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESSSM
{
    public interface IStateMachineInstance<TState, TContext>
    {
        void Receive(object input, Action<TContext> unhandledCallback);
    }

    public static class IStateMachineInstanceExtensions
    {
        public static void Receive<TState, TContext>(this IStateMachineInstance<TState, TContext> self, object input)
        {
            self.Receive(input, ctx => { });
        }
    }

}
