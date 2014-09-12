using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESSSM.Visitors.Correlation
{
    public class CorrelationData<TContext> : IEquatable<CorrelationData<TContext>>
    {
        private Func<TContext, object> contextDelegate;
        private Delegate inputDelegate;

        public CorrelationData(Type inputType, Func<TContext, object> contextDelegate, Delegate inputDelegate, string inputMemberName, string contextMemberName)
        {
            this.contextDelegate = contextDelegate;
            this.inputDelegate = inputDelegate;
            this.InputType = inputType;
            this.InputMemberName = inputMemberName;
            this.ContextMemberName = contextMemberName;
        }

        public object EvalUnderContext(TContext context)
        {
            return contextDelegate(context);
        }

        public object EvalUnderInput(object input)
        {
            return inputDelegate.DynamicInvoke(input);
        }

        public string InputMemberName { get; private set; }

        public string ContextMemberName { get; private set; }

        public Type InputType { get; private set; }

        public bool Equals(CorrelationData<TContext> other)
        {
            return this.InputMemberName == other.InputMemberName
                && this.InputType == other.InputType;
        }
    }
}
