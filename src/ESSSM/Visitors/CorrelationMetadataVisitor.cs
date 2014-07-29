using ESSSM.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ESSSM
{

    public static class StateMachineCorrelationExtensions
    {
        public const string CORRELATION_METADATA_KEY = "CorrelatedBy";

        public static TReturn CorrelatedBy<TContext, TInput, TReturn>(this IRecordTransitionMetadata<TContext, TInput, TReturn> self,
            Expression<Func<TContext, TInput, bool>> correlationExpr)
        {
            return self.WithMetadata(CORRELATION_METADATA_KEY, correlationExpr);
        }
    }
}

namespace ESSSM.Visitors
{   
    public class CorrelationMetadataVisitor<TState, TContext> : IVisitStateTransition<TState,TContext>
    {
        private const string CORRELATION_METADATA_KEY = StateMachineCorrelationExtensions.CORRELATION_METADATA_KEY;

        public class CorrelationData
        {
            private Func<TContext, object> contextDelegate;
            private Delegate inputDelegate;
            
            public CorrelationData(Type inputType, Func<TContext, object> contextDelegate, Delegate inputDelegate, string id)
            {
                this.contextDelegate = contextDelegate;
                this.inputDelegate = inputDelegate;
                this.InputType = inputType;
                this.Id = id;
            }

            public object EvalUnderContext(TContext context)
            {
                return contextDelegate(context);
            }

            public object EvalUnderInput(object input)
            {
                return inputDelegate.DynamicInvoke(input);
            }

            public string Id { get; private set; }

            public Type InputType { get; private set; }
        }

        private IList<CorrelationData> correlationData = new List<CorrelationData>();

        public IEnumerable<CorrelationData> Correlations { get { return correlationData; } }

        public void VisitTransition(TState sourceState, IEnumerable<Type> inputTypes, System.Linq.Expressions.Expression<Func<TContext, bool>> transitionPredicate, TState destinationState) { }

        public void VisitTransition(TState sourceState, Type inputType, IDictionary<string, object> metadata)
        {
            object valueObj;
            if(!metadata.TryGetValue(CORRELATION_METADATA_KEY, out valueObj))
            {
                return;
            }
            var correlationExpr = (LambdaExpression)valueObj;
            try
            {
                if (correlationExpr.Body.NodeType == ExpressionType.Equal)
                {
                    var equalsExpr = (BinaryExpression)correlationExpr.Body;
                    var leftExpr = (MemberExpression)equalsExpr.Left;
                    var rightExpr = (MemberExpression)equalsExpr.Right;

                    var contextExpr = leftExpr;
                    var inputExpr = rightExpr;

                    if (rightExpr.Expression.Type == typeof(TContext))
                    {
                        contextExpr = rightExpr;
                        inputExpr = leftExpr;
                    } 
                    
                    var contextValDelegate = LambdaExpression.Lambda<Func<TContext, object>>(contextExpr, correlationExpr.Parameters[0]).Compile();

                    var delegateType = Expression.GetFuncType(inputExpr.Expression.Type, typeof(object));
                    var inputValDelegate = LambdaExpression.Lambda(delegateType, inputExpr, correlationExpr.Parameters[1]).Compile();

                    this.correlationData.Add(new CorrelationData(
                        inputExpr.Expression.Type, 
                        contextValDelegate, 
                        inputValDelegate, 
                        inputExpr.Member.Name));
                }
                else
                {
                    throw new Exception("Unpexpected expression node type");
                }
            }
            catch(Exception e)
            {
                throw new UnexpectedCorrelationExpressionExcpetion(sourceState, inputType, correlationExpr, e);
            }
        }
    }
}