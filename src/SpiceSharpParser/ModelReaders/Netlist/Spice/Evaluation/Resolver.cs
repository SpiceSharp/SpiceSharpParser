using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation
{
    public class Resolver
    {
        public event EventHandler<VariableFoundEventArgs<VariableNode>> UnknownVariableFound;

        public Dictionary<string, Node> VariableMap { get; set; }

        public Dictionary<string, ResolverFunction> FunctionMap { get; set; }

        public Node Resolve(Node expression)
        {
            switch (expression)
            {
                case UnaryOperatorNode un:
                    switch (un.NodeType)
                    {
                        case NodeTypes.Plus: return Node.Plus(Resolve(un.Argument));
                        case NodeTypes.Minus: return Node.Minus(Resolve(un.Argument));
                        case NodeTypes.Not: return Node.Not(Resolve(un.Argument));
                    }

                    break;

                case BinaryOperatorNode bn:
                    var left = Resolve(bn.Left);
                    var right = Resolve(bn.Right);
                    switch (bn.NodeType)
                    {
                        case NodeTypes.Add: return Node.Add(left, right);
                        case NodeTypes.Subtract: return Node.Subtract(left, right);
                        case NodeTypes.Multiply: return Node.Multiply(left, right);
                        case NodeTypes.Divide: return Node.Divide(left, right);
                        case NodeTypes.Modulo: return Node.Modulo(left, right);
                        case NodeTypes.LessThan: return Node.LessThan(left, right);
                        case NodeTypes.GreaterThan: return Node.GreaterThan(left, right);
                        case NodeTypes.LessThanOrEqual: return Node.LessThanOrEqual(left, right);
                        case NodeTypes.GreaterThanOrEqual: return Node.GreaterThanOrEqual(left, right);
                        case NodeTypes.Equals: return Node.Equals(left, right);
                        case NodeTypes.NotEquals: return Node.NotEquals(left, right);
                        case NodeTypes.And: return Node.And(left, right);
                        case NodeTypes.Or: return Node.Or(left, right);
                        case NodeTypes.Xor: return Node.Xor(left, right);
                        case NodeTypes.Pow: return Node.Power(left, right);
                    }

                    break;

                case TernaryOperatorNode tn:
                    return Node.Conditional(Resolve(tn.Condition), Resolve(tn.IfTrue), Resolve(tn.IfFalse));

                case FunctionNode fn:
                    var args = new Node[fn.Arguments.Count];
                    for (var i = 0; i < args.Length; i++)
                    {
                        args[i] = Resolve(fn.Arguments[i]);
                    }

                    var funtionUpdated = Node.Function(fn.Name, args);
                    if (FunctionMap != null && FunctionMap.TryGetValue(fn.Name, out var function))
                    {
                        if (function is StaticResolverFunction staticResolverFunction)
                        {
                            var i = 0;
                            foreach (VariableNode argument in staticResolverFunction.Arguments)
                            {
                                VariableMap[argument.Name] = args[i];
                                i++;
                            }

                            var functionBodyResolved = Resolve(staticResolverFunction.GetBody());
                            return functionBodyResolved;
                        }

                        if (function is DynamicResolverFunction dynamicResolverFunction)
                        {
                            var functionBodyResolved = dynamicResolverFunction.GetBody(args);
                            return functionBodyResolved;
                        }
                    }

                    return funtionUpdated;
                case ConstantNode cn:
                    return cn;

                case VariableNode vn:
                    if (VariableMap != null && vn.NodeType == NodeTypes.Variable && VariableMap.TryGetValue(vn.Name, out var mapped))
                    {
                        return Resolve(mapped);
                    }
                    else
                    {
                        var vargs = new VariableFoundEventArgs<VariableNode>(this, vn);
                        OnUnknownVariableFound(vargs);
                        if (vargs.Created)
                        {
                            return vargs.Result;
                        }
                        else
                        {
                            return vn;
                        }
                    }
            }

            return expression;
        }

        /// <summary>
        /// Called when a variable was found.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnUnknownVariableFound(VariableFoundEventArgs<VariableNode> args) => UnknownVariableFound?.Invoke(this, args);

        /// <summary>
        /// Event arguments used when a variable was found.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        public class VariableFoundEventArgs<T> : EventArgs
        {
            private T _result;

            /// <summary>
            /// Initializes a new instance of the <see cref="VariableFoundEventArgs{T}"/> class.
            /// </summary>
            /// <param name="builder">The builder.</param>
            /// <param name="node">The variable node.</param>
            /// <exception cref="ArgumentNullException">Throw if <paramref name="builder"/> or <paramref name="node"/> is <c>null</c>.</exception>
            public VariableFoundEventArgs(Resolver builder, VariableNode node)
            {
                Created = false;
                _result = default;
                Node = node;
                Builder = builder;
            }

            /// <summary>
            /// Gets the builder.
            /// </summary>
            /// <value>
            /// The builder.
            /// </value>
            public Resolver Builder { get; }

            /// <summary>
            /// Gets the variable.
            /// </summary>
            /// <value>
            /// The variable.
            /// </value>
            public VariableNode Node { get; }

            /// <summary>
            /// Gets or sets the result of the variable.
            /// </summary>
            /// <value>
            /// The value of the variable.
            /// </value>
            public T Result
            {
                get => _result;
                set
                {
                    _result = value;
                    Created = true;
                }
            }

            /// <summary>
            /// Gets a value indicating whether gets a valute has received a value.
            /// </summary>
            /// <value>
            ///     <c>true</c> if the variable has received a value; otherwise, <c>false</c>.
            /// </value>
            public bool Created { get; private set; }
        }
    }
}
