using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Evaluation
{
    /// <summary>
    /// @author: Sven Boulanger
    /// A very light-weight and fast expression parser made for parsing Spice expressions
    /// It is based on Dijkstra's Shunting Yard algorithm. It is very fast for parsing expressions only once.
    /// The parser is also not very expressive for errors, so only use it for relatively simple expressions.
    /// <list type="bullet">
    ///     <listheader><description>Supported operators</description></listheader>
    ///     <item><description>Positive and negative ('+', '-')</description></item>
    ///     <item><description>Addition and subtraction ('+', '-')</description></item>
    ///     <item><description>Multiplication and division ('*', '/')</description></item>
    ///     <item><description>Modulo ('%')</description></item>
    ///     <item><description>Logical ('&amp;&amp;', '||', '!')</description></item>
    ///     <item><description>Relational ('==', '!=', '&lt;', '&gt;', '&lt;=', '&gt;=')</description></item>
    /// </list>
    /// <list type="bullet">
    ///     <listheader><description>Supported functions</description></listheader>
    ///     <item><description>Minimum and maximum ('min(a,b)', 'max(a,b)')</description></item>
    ///     <item><description>Square root ('sqrt(a)')</description></item>
    ///     <item><description>Absolute value ('abs(a)')</description></item>
    ///     <item><description>Exponent and logarithms ('exp(a)', 'log(a)', 'log10(a)')</description></item>
    ///     <item><description>Powers ('pow(a, b)')</description></item>
    ///     <item><description>Trigonometry ('sin(a)', 'cos(a)', 'tan(a)', 'asin(a)', 'acos(a)', 'atan(a)', 'sinh(a)', 'cosh(a)', 'tanh(a)', 'atan2(a, b)')</description></item>
    /// </list>
    /// </summary>
    public class SpiceExpression
    {
        /// <summary>
        /// Precedence levels
        /// </summary>
        private const byte PrecedenceConditional = 1;
        private const byte PrecedenceConditionalOr = 2;
        private const byte PrecedenceConditionalAnd = 3;
        private const byte PrecedenceLogicalOr = 4;
        private const byte PrecedenceLogicalXor = 5;
        private const byte PrecedenceLogicalAnd = 6;
        private const byte PrecedenceEquality = 7;
        private const byte PrecedenceRelational = 8;
        private const byte PrecedenceShift = 9;
        private const byte PrecedenceAdditive = 10;
        private const byte PrecedenceMultiplicative = 11;
        private const byte PrecedenceUnary = 12;
        private const byte PrecedencePrimary = 13;

        /// <summary>
        /// Operator ID's
        /// </summary>
        private const byte IdPositive = 0;
        private const byte IdNegative = 1;
        private const byte IdNot = 2;
        private const byte IdAdd = 3;
        private const byte IdSubtract = 4;
        private const byte IdMultiply = 5;
        private const byte IdDivide = 6;
        private const byte IdModulo = 7;
        private const byte IdEquals = 8;
        private const byte IdInequals = 9;
        private const byte IdOpenConditional = 10;
        private const byte IdClosedConditional = 11;
        private const byte IdConditionalOr = 12;
        private const byte IdConditionalAnd = 13;
        private const byte IdLess = 14;
        private const byte IdLessOrEqual = 15;
        private const byte IdGreater = 16;
        private const byte IdGreaterOrEqual = 17;
        private const byte IdLeftBracket = 18;
        private const byte IdFunction = 19;

        /// <summary>
        /// Operators
        /// </summary>
        private static readonly Operator OperatorPositive = new Operator(IdPositive, PrecedenceUnary, false);
        private static readonly Operator OperatorNegative = new Operator(IdNegative, PrecedenceUnary, false);
        private static readonly Operator OperatorNot = new Operator(IdNot, PrecedenceUnary, false);
        private static readonly Operator OperatorAdd = new Operator(IdAdd, PrecedenceAdditive, true);
        private static readonly Operator OperatorSubtract = new Operator(IdSubtract, PrecedenceAdditive, true);
        private static readonly Operator OperatorMultiply = new Operator(IdMultiply, PrecedenceMultiplicative, true);
        private static readonly Operator OperatorDivide = new Operator(IdDivide, PrecedenceMultiplicative, true);
        private static readonly Operator OperatorModulo = new Operator(IdModulo, PrecedenceMultiplicative, true);
        private static readonly Operator OperatorEquals = new Operator(IdEquals, PrecedenceEquality, true);
        private static readonly Operator OperatorInequals = new Operator(IdInequals, PrecedenceEquality, true);
        private static readonly Operator OperatorOpenConditional = new Operator(IdOpenConditional, PrecedenceConditional, false);
        private static readonly Operator OperatorClosedConditional = new Operator(IdClosedConditional, PrecedenceConditional, false);
        private static readonly Operator OperatorConditionalOr = new Operator(IdConditionalOr, PrecedenceConditionalOr, true);
        private static readonly Operator OperatorConditionalAnd = new Operator(IdConditionalAnd, PrecedenceConditionalAnd, true);
        private static readonly Operator OperatorLess = new Operator(IdLess, PrecedenceRelational, true);
        private static readonly Operator OperatorLessOrEqual = new Operator(IdLessOrEqual, PrecedenceRelational, true);
        private static readonly Operator OperatorGreater = new Operator(IdGreater, PrecedenceRelational, true);
        private static readonly Operator OperatorGreaterOrEqual = new Operator(IdGreaterOrEqual, PrecedenceRelational, true);
        private static readonly Operator OperatorLeftBracket = new Operator(IdLeftBracket, byte.MaxValue, false);

        /// <summary>
        /// Private variables
        /// </summary>
        private readonly Stack<double> outputStack = new Stack<double>();
        private readonly Stack<Operator> operatorStack = new Stack<Operator>();
        private readonly StringBuilder sb = new StringBuilder();
        private int index;
        private string input;
        private bool infixPostfix;
        private int count;

        /// <summary>
        /// Gets or sets the parameters used for expressions
        /// </summary>
        public Dictionary<string, double> Parameters { get; set; }

        /// <summary>
        /// Gets all supported functions
        /// </summary>
        private Dictionary<string, FunctionOperator> Functions { get; } = new Dictionary<string, FunctionOperator>
        {
            { "min", new FunctionOperator(stack => Math.Min(stack.Pop(), stack.Pop())) },
            { "max", new FunctionOperator(stack => Math.Max(stack.Pop(), stack.Pop())) },
            { "abs", new FunctionOperator(stack => Math.Abs(stack.Pop())) },
            { "sqrt", new FunctionOperator(stack => Math.Sqrt(stack.Pop())) },
            { "exp", new FunctionOperator(stack => Math.Exp(stack.Pop())) },
            { "log", new FunctionOperator(stack => Math.Log(stack.Pop())) },
            { "log10", new FunctionOperator(stack => Math.Log10(stack.Pop())) },
            {
                "pow", new FunctionOperator(stack =>
                {
                    var b = stack.Pop();
                    var a = stack.Pop();
                    return Math.Pow(a, b);
                })
            },
            { "cos", new FunctionOperator(stack => Math.Cos(stack.Pop())) },
            { "sin", new FunctionOperator(stack => Math.Sin(stack.Pop())) },
            { "tan", new FunctionOperator(stack => Math.Tan(stack.Pop())) },
            { "cosh", new FunctionOperator(stack => Math.Cosh(stack.Pop())) },
            { "sinh", new FunctionOperator(stack => Math.Sinh(stack.Pop())) },
            { "tanh", new FunctionOperator(stack => Math.Tanh(stack.Pop())) },
            { "acos", new FunctionOperator(stack => Math.Acos(stack.Pop())) },
            { "asin", new FunctionOperator(stack => Math.Asin(stack.Pop())) },
            { "atan", new FunctionOperator(stack => Math.Atan(stack.Pop())) },
            {
                "atan2", new FunctionOperator(stack =>
                {
                    var b = stack.Pop();
                    var a = stack.Pop();
                    return Math.Atan2(a, b);
                })
            }
        };

        /// <summary>
        /// Parse an expression
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <param name="expressionParameters">Parameters appearing in the expression</param>
        /// <returns>Returns the result of the expression</returns>
        public double Parse(string expression, out List<string> expressionParameters)
        {
            expressionParameters = new List<string>();

            // Initialize for parsing the expression
            index = 0;
            input = expression ?? throw new ArgumentNullException(nameof(expression));
            infixPostfix = false;
            outputStack.Clear();
            operatorStack.Clear();
            count = input.Length;

            // Parse the expression
            while (index < count)
            {
                // Skip spaces
                while (index < count && input[index] == ' ')
                {
                    index++;
                }

                // Parse a double
                char c = input[index];

                // Parse a binary operator
                if (infixPostfix)
                {
                    // Test for infix and postfix operators
                    infixPostfix = false;
                    switch (c)
                    {
                        case '+': PushOperator(OperatorAdd); break;
                        case '-': PushOperator(OperatorSubtract); break;
                        case '*': PushOperator(OperatorMultiply); break;
                        case '/': PushOperator(OperatorDivide); break;
                        case '%': PushOperator(OperatorModulo); break;
                        case '=':
                            index++;
                            if (index < count && input[index] == '=')
                            {
                                PushOperator(OperatorEquals);
                            }
                            else
                            {
                                goto default;
                            }

                            break;
                        case '!':
                            index++;
                            if (index < count && input[index] == '=')
                            {
                                PushOperator(OperatorInequals);
                            }
                            else
                            {
                                goto default;
                            }

                            break;
                        case '?': PushOperator(OperatorOpenConditional); break;
                        case ':':
                            // Evaluate to an open conditional
                            while (operatorStack.Count > 0)
                            {
                                if (operatorStack.Peek().Id == IdOpenConditional)
                                {
                                    break;
                                }

                                Evaluate(operatorStack.Pop());
                            }

                            operatorStack.Pop();
                            operatorStack.Push(OperatorClosedConditional);
                            break;
                        case '|':
                            index++;
                            if (index < count && input[index] == '|')
                            {
                                PushOperator(OperatorConditionalOr);
                            }
                            else
                            {
                                goto default;
                            }

                            break;
                        case '&':
                            index++;
                            if (index < count && input[index] == '&')
                            {
                                PushOperator(OperatorConditionalAnd);
                            }
                            else
                            {
                                goto default;
                            }

                            break;
                        case '<':
                            if (index + 1 < count && input[index + 1] == '=')
                            {
                                PushOperator(OperatorLessOrEqual);
                                index++;
                            }
                            else
                            {
                                PushOperator(OperatorLess);
                            }

                            break;
                        case '>':
                            if (index + 1 < count && input[index + 1] == '=')
                            {
                                PushOperator(OperatorGreaterOrEqual);
                                index++;
                            }
                            else
                            {
                                PushOperator(OperatorGreater);
                            }

                            break;

                        case ')':
                            // Evaluate until the matching opening bracket
                            while (operatorStack.Count > 0)
                            {
                                if (operatorStack.Peek().Id == IdLeftBracket)
                                {
                                    operatorStack.Pop();
                                    break;
                                }

                                if (operatorStack.Peek().Id == IdFunction)
                                {
                                    FunctionOperator op = (FunctionOperator)operatorStack.Pop();
                                    outputStack.Push(op.Function(outputStack));
                                    break;
                                }

                                Evaluate(operatorStack.Pop());
                            }

                            infixPostfix = true;
                            break;

                        case ',':
                            // Function argument
                            while (operatorStack.Count > 0)
                            {
                                if (operatorStack.Peek().Id == IdFunction)
                                {
                                    break;
                                }

                                Evaluate(operatorStack.Pop());
                            }

                            break;
                        default:
                            throw new Exception("Unrecognized operator");
                    }

                    index++;
                }

                // Parse a unary operator
                else
                {
                    if (c == '.' || (c >= '0' && c <= '9'))
                    {
                        outputStack.Push(ParseDouble());
                        infixPostfix = true;
                    }

                    // Parse a parameter or a function
                    else if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                    {
                        sb.Clear();
                        sb.Append(input[index++]);
                        while (index < count)
                        {
                            c = input[index];
                            if ((c >= '0' && c <= '9') ||
                                (c >= 'a' && c <= 'z') ||
                                (c >= 'A' && c <= 'Z') ||
                                c == '_')
                            {
                                sb.Append(c);
                                index++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (index < count && input[index] == '(')
                        {
                            index++;

                            // "cos(10)+sin(-1)*max(tanh(1),sinh(1))"
                            // Benchmark switch statements on function name: 1,000,000 -> 2400ms
                            // Benchmark switch on first character + if/else function name: 1,000,000 -> 2200ms
                            // Benchmark using Dictionary<>: 1,000,000 -> 2200ms -- I chose this option
                            operatorStack.Push(Functions[sb.ToString()]);
                        }
                        else if (Parameters != null)
                        {
                            string id = sb.ToString();
                            expressionParameters.Add(id);
                            outputStack.Push(Parameters[id]);
                            infixPostfix = true;
                        }
                        else
                        {
                            throw new Exception("No parameters");
                        }
                    }

                    // Prefix operators
                    else
                    {
                        switch (c)
                        {
                            case '+': PushOperator(OperatorPositive); break;
                            case '-': PushOperator(OperatorNegative); break;
                            case '!': PushOperator(OperatorNot); break;
                            case '(': PushOperator(OperatorLeftBracket); break;
                            default:
                                throw new Exception("Unrecognized unary operator");
                        }

                        index++;
                    }
                }
            }

            // Evaluate all that is left on the stack
            while (operatorStack.Count > 0)
            {
                Evaluate(operatorStack.Pop());
            }

            if (outputStack.Count > 1)
            {
                throw new Exception("Invalid expression");
            }

            return outputStack.Pop();
        }

        /// <summary>
        /// Evaluate operators with precedence
        /// </summary>
        /// <param name="op">Operator</param>
        private void PushOperator(Operator op)
        {
            while (operatorStack.Count > 0)
            {
                // Stop evaluation
                Operator o = operatorStack.Peek();
                if (o.Precedence < op.Precedence || !o.LeftAssociative)
                {
                    break;
                }

                Evaluate(operatorStack.Pop());
            }

            operatorStack.Push(op);
        }

        /// <summary>
        /// Evaluate an operator
        /// </summary>
        /// <param name="op">Operator</param>
        private void Evaluate(Operator op)
        {
            double a, b;
            switch (op.Id)
            {
                case IdPositive: break;
                case IdNegative: outputStack.Push(-outputStack.Pop()); break;
                case IdNot:
                    a = outputStack.Pop();
                    outputStack.Push(a.Equals(0.0) ? 1.0 : 0.0);
                    break;
                case IdAdd: outputStack.Push(outputStack.Pop() + outputStack.Pop()); break;
                case IdSubtract:
                    b = outputStack.Pop();
                    a = outputStack.Pop();
                    outputStack.Push(a - b);
                    break;
                case IdMultiply: outputStack.Push(outputStack.Pop() * outputStack.Pop()); break;
                case IdDivide:
                    b = outputStack.Pop();
                    a = outputStack.Pop();
                    outputStack.Push(a / b);
                    break;
                case IdModulo:
                    b = outputStack.Pop();
                    a = outputStack.Pop();
                    outputStack.Push(a % b);
                    break;
                case IdEquals: outputStack.Push(outputStack.Pop().Equals(outputStack.Pop()) ? 1.0 : 0.0); break;
                case IdInequals: outputStack.Push(!outputStack.Pop().Equals(outputStack.Pop()) ? 1.0 : 0.0); break;
                case IdConditionalAnd:
                    b = outputStack.Pop();
                    a = outputStack.Pop();
                    outputStack.Push(!a.Equals(0.0) && !b.Equals(0.0) ? 1.0 : 0.0); break;
                case IdConditionalOr:
                    b = outputStack.Pop();
                    a = outputStack.Pop();
                    outputStack.Push(!a.Equals(0.0) || !b.Equals(0.0) ? 1.0 : 0.0); break;
                case IdLess:
                    b = outputStack.Pop();
                    a = outputStack.Pop();
                    outputStack.Push(a < b ? 1.0 : 0.0);
                    break;
                case IdLessOrEqual:
                    b = outputStack.Pop();
                    a = outputStack.Pop();
                    outputStack.Push(a <= b ? 1.0 : 0.0);
                    break;
                case IdGreater:
                    b = outputStack.Pop();
                    a = outputStack.Pop();
                    outputStack.Push(a > b ? 1.0 : 0.0);
                    break;
                case IdGreaterOrEqual:
                    b = outputStack.Pop();
                    a = outputStack.Pop();
                    outputStack.Push(a >= b ? 1.0 : 0.0); break;
                case IdClosedConditional:
                    var c = outputStack.Pop();
                    b = outputStack.Pop();
                    a = outputStack.Pop();
                    outputStack.Push(a > 0.0 ? b : c);
                    break;
                case IdOpenConditional: throw new Exception("Unmatched conditional");
                default:
                   throw new Exception("Unrecognized operator");
            }
        }

        /// <summary>
        /// Parse a double value at the current position
        /// </summary>
        /// <returns>Parse result</returns>
        private double ParseDouble()
        {
            // Read integer part
            double value = 0.0;
            while (index < count && (input[index] >= '0' && input[index] <= '9'))
            {
                value = (value * 10.0) + (input[index++] - '0');
            }

            // Read decimal part
            if (index < count && input[index] == '.')
            {
                index++;
                double mult = 1.0;
                while (index < count && (input[index] >= '0' && input[index] <= '9'))
                {
                    value = (value * 10.0) + (input[index++] - '0');
                    mult = mult * 10.0;
                }

                value /= mult;
            }

            if (index < count)
            {
                // Scientific notation
                if (input[index] == 'e' || input[index] == 'E')
                {
                    index++;
                    var exponent = 0;
                    var neg = false;
                    if (index < count && (input[index] == '+' || input[index] == '-'))
                    {
                        if (input[index] == '-')
                        {
                            neg = true;
                        }

                        index++;
                    }

                    // Get the exponent
                    while (index < count && (input[index] >= '0' && input[index] <= '9'))
                    {
                        exponent = (exponent * 10) + (input[index++] - '0');
                    }

                    // Integer exponentation
                    var mult = 1.0;
                    var b = 10.0;
                    while (exponent != 0)
                    {
                        if ((exponent & 0x01) == 0x01)
                        {
                            mult *= b;
                        }

                        b *= b;
                        exponent >>= 1;
                    }

                    if (neg)
                    {
                        value /= mult;
                    }
                    else
                    {
                        value *= mult;
                    }
                }
                else
                {
                    // Spice modifiers
                    switch (input[index])
                    {
                        case 't':
                        case 'T': value *= 1.0e12; index++; break;
                        case 'g':
                        case 'G': value *= 1.0e9; index++; break;
                        case 'x':
                        case 'X': value *= 1.0e6; index++; break;
                        case 'k':
                        case 'K': value *= 1.0e3; index++; break;
                        case 'u':
                        case 'U': value /= 1.0e6; index++; break;
                        case 'n':
                        case 'N': value /= 1.0e9; index++; break;
                        case 'p':
                        case 'P': value /= 1.0e12; index++; break;
                        case 'f':
                        case 'F': value /= 1.0e15; index++; break;
                        case 'm':
                        case 'M':
                            if (index + 2 < count &&
                                (input[index + 1] == 'e' || input[index + 1] == 'E') &&
                                (input[index + 2] == 'g' || input[index + 2] == 'G'))
                            {
                                value *= 1.0e6;
                                index += 3;
                            }
                            else if (index + 2 < count &&
                                (input[index + 1] == 'i' || input[index + 1] == 'I') &&
                                (input[index + 2] == 'l' || input[index + 2] == 'L'))
                            {
                                value *= 25.4e-6;
                                index += 3;
                            }
                            else
                            {
                                value /= 1.0e3;
                                index++;
                            }

                            break;
                    }
                }

                // Any trailing letters are ignored
                while (index < count && ((input[index] >= 'a' && input[index] <= 'z') || (input[index] >= 'A' && input[index] <= 'Z')))
                {
                    index++;
                }
            }

            return value;
        }

        /// <summary>
        /// Operator description
        /// </summary>
        private class Operator
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Operator"/> class.
            /// </summary>
            /// <param name="id">The operator ID</param>
            /// <param name="precedence">The operator precedence</param>
            /// <param name="la">Is the operator left-associative?</param>
            public Operator(byte id, byte precedence, bool la)
            {
                Id = id;
                Precedence = precedence;
                LeftAssociative = la;
            }

            /// <summary>
            /// Gets operator identifier
            /// </summary>
            public byte Id { get; }

            /// <summary>
            /// Gets operator precedence
            /// </summary>
            public byte Precedence { get; }

            /// <summary>
            /// Gets a value indicating whether the operator is left-associative or not
            /// </summary>
            public bool LeftAssociative { get; }
        }

        /// <summary>
        /// Function description
        /// </summary>
        private class FunctionOperator : Operator
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FunctionOperator"/> class.
            /// </summary>
            /// <param name="func">The function</param>
            public FunctionOperator(Func<Stack<double>, double> func)
                : base(IdFunction, byte.MaxValue, false)
            {
                Function = func;
            }

            /// <summary>
            /// Gets the function evaluation
            /// </summary>
            public Func<Stack<double>, double> Function { get; }
        }
    }
}
