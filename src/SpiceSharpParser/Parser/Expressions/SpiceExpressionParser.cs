using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.Parser.Expressions
{
    /// <summary>
    /// @author: Sven Boulanger 
    /// @author: Marcin Gołębiowski (custom functions)
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
    public class SpiceExpressionParser : IExpressionParser
    {
        /// <summary>
        /// Precedence levels.
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
        /// Operator ID's.
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
        private const byte IdUserFunction = 20;

        /// <summary>
        /// Operators.
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
        /// Private variables.
        /// </summary>
        private readonly Stack<double> outputStack = new Stack<double>();
        private readonly Stack<object> virtualParamtersStack = new Stack<object>();
        private readonly Stack<Operator> operatorStack = new Stack<Operator>();
        private readonly StringBuilder sb = new StringBuilder();
        private int index;
        private string input;
        private bool infixPostfix;
        private int count;

        public SpiceExpressionParser(bool isNegationAssociative = false)
        {
            OperatorNegative.LeftAssociative = isNegationAssociative;
        }

        /// <summary>
        /// Gets or sets the parameters used for expressions.
        /// </summary>
        public Dictionary<string, double> Parameters { get; protected set; } = new Dictionary<string, double>();

        /// <summary>
        /// Gets or sets the variables used in last parsed expression.
        /// </summary>
        public Collection<string> Variables { get; protected set; }

        /// <summary>
        /// Gets or sets custom functions.
        /// </summary>
        public Dictionary<string, CustomFunction> CustomFunctions { get; protected set; } = new Dictionary<string, CustomFunction>();

        /// <summary>
        /// Gets all built-in constants.
        /// </summary>
        private Dictionary<string, double> BuiltInConstants { get; } = new Dictionary<string, double>()
        {
            { "PI", Math.PI },
            { "pi", Math.PI },
            { "e", Math.E },
            { "E", Math.E },
            { "false", 0.0 },
            { "true", 1.0 },
            { "yes", 1.0 },
            { "no", 0.0 },
            { "kelvin", -273.15 },
            { "echarge", 1.60219e-19 },
            { "c", 299792500 },
            { "boltz", 1.38062e-23 },
        };

        /// <summary>
        /// Gets all built-in functions (only trigonometric functions)
        /// </summary>
        private Dictionary<string, BuiltInFunctionOperator> BuiltInFunctions { get; } = new Dictionary<string, BuiltInFunctionOperator>
        {
            { "cos", new BuiltInFunctionOperator(stack => Math.Cos(stack.Pop())) },
            { "sin", new BuiltInFunctionOperator(stack => Math.Sin(stack.Pop())) },
            { "tan", new BuiltInFunctionOperator(stack => Math.Tan(stack.Pop())) },
            { "cosh", new BuiltInFunctionOperator(stack => Math.Cosh(stack.Pop())) },
            { "sinh", new BuiltInFunctionOperator(stack => Math.Sinh(stack.Pop())) },
            { "tanh", new BuiltInFunctionOperator(stack => Math.Tanh(stack.Pop())) },
            { "acos", new BuiltInFunctionOperator(stack => Math.Acos(stack.Pop())) },
            { "asin", new BuiltInFunctionOperator(stack => Math.Asin(stack.Pop())) },
            { "atan", new BuiltInFunctionOperator(stack => Math.Atan(stack.Pop())) },
            { "atan2",
                new BuiltInFunctionOperator(
                    stack => {
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
        /// <returns>Returns the result of the expression</returns>
        public double Parse(string expression, object context = null)
        {
            Variables = new Collection<string>();

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

                if (index == count)
                {
                    break;
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
                        case '+': PushOperator(OperatorAdd, context); break;
                        case '-': PushOperator(OperatorSubtract, context); break;
                        case '*':
                            if ((index + 1 < count) && input[index + 1] == '*')
                            {
                                index++;
                                PushOperator(CreateOperatorUserFunction("**"), context);
                                break;
                            }
                            PushOperator(OperatorMultiply, context);
                            break;
                        case '/': PushOperator(OperatorDivide, context); break;
                        case '%': PushOperator(OperatorModulo, context); break;
                        case '=':
                            index++;
                            if (index < count && input[index] == '=')
                            {
                                PushOperator(OperatorEquals, context);
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
                                PushOperator(OperatorInequals, context);
                            }
                            else
                            {
                                goto default;
                            }

                            break;
                        case '?': PushOperator(OperatorOpenConditional, context); break;
                        case ':':
                            // Evaluate to an open conditional
                            while (operatorStack.Count > 0)
                            {
                                if (operatorStack.Peek().Id == IdOpenConditional)
                                {
                                    break;
                                }

                                EvaluateOperator(operatorStack.Pop(), context);
                            }

                            operatorStack.Pop();
                            operatorStack.Push(OperatorClosedConditional);
                            break;
                        case '|':
                            index++;
                            if (index < count && input[index] == '|')
                            {
                                PushOperator(OperatorConditionalOr, context);
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
                                PushOperator(OperatorConditionalAnd, context);
                            }
                            else
                            {
                                goto default;
                            }

                            break;
                        case '<':
                            if (index + 1 < count && input[index + 1] == '=')
                            {
                                PushOperator(OperatorLessOrEqual, context);
                                index++;
                            }
                            else
                            {
                                PushOperator(OperatorLess, context);
                            }

                            break;
                        case '>':
                            if (index + 1 < count && input[index + 1] == '=')
                            {
                                PushOperator(OperatorGreaterOrEqual,context);
                                index++;
                            }
                            else
                            {
                                PushOperator(OperatorGreater, context);
                            }

                            break;

                        case ')':
                            // Evaluate until the matching opening bracket

                            // TODO: verify logic below
                            while (operatorStack.Count > 0)
                            {
                                if (operatorStack.Peek().Id == IdLeftBracket)
                                {
                                    operatorStack.Pop();
                                    break;
                                }

                                if (operatorStack.Peek().Id == IdFunction)
                                {
                                    BuiltInFunctionOperator op = (BuiltInFunctionOperator)operatorStack.Pop();
                                    outputStack.Push(op.Function(outputStack));
                                    break;
                                }

                                if (operatorStack.Peek().Id == IdUserFunction)
                                {
                                    UserFunctionOperator op2 = (UserFunctionOperator)operatorStack.Pop();
                                    EvaluateUserFunction(op2, context);
                                    break;
                                }

                                EvaluateOperator(operatorStack.Pop(), context);
                            }

                            infixPostfix = true;
                            break;

                        case '[':
                            break;
                        case ']':
                            EvaluateUserFunction((UserFunctionOperator)operatorStack.Pop(), context);
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

                                if (operatorStack.Peek().Id == IdUserFunction)
                                {
                                    break;
                                }

                                EvaluateOperator(operatorStack.Pop(), context);
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
                    if (c == '@')
                    {
                        index++;

                        int startIndex = index;
                        int tmpIndex = index;
                        while (tmpIndex < count && input[tmpIndex] != '[')
                        {
                            tmpIndex++;
                        }

                        if (tmpIndex != count)
                        {
                            if (CustomFunctions.ContainsKey("@"))
                            {
                                operatorStack.Push(CreateOperatorUserFunction("@"));
                            }
                            else
                            {
                                throw new Exception("Unknown function: @");
                            }
                            infixPostfix = false;
                        }
                    }
                    else if (c == '.' || (c >= '0' && c <= '9'))
                    {
                        if (operatorStack.Count > 0
                            && operatorStack.Peek().Id == IdUserFunction
                            && ((UserFunctionOperator)operatorStack.Peek()).PureVirtualFunction)
                        {
                            virtualParamtersStack.Push(ParseDouble().ToString());
                        }
                        else
                        {
                            outputStack.Push(ParseDouble());
                        }
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
                                c == '_' ||
                                c == '.')
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
                            if (BuiltInFunctions.TryGetValue(sb.ToString(), out var function))
                            {
                                operatorStack.Push(function);
                            }
                            else
                            {
                                if (CustomFunctions.ContainsKey(sb.ToString()))
                                {
                                    operatorStack.Push(CreateOperatorUserFunction(sb.ToString()));
                                }
                                else
                                {
                                    throw new Exception("Unknown function: " + sb.ToString());
                                }
                            }
                        }
                        else if (Parameters != null)
                        {
                            string id = sb.ToString();

                            if (operatorStack.Count > 0
                                && operatorStack.Peek().Id == IdUserFunction
                                && ((UserFunctionOperator)operatorStack.Peek()).PureVirtualFunction)
                            {
                                if (Parameters.TryGetValue(id, out var parameter))
                                {
                                    Variables.Add(id);
                                }

                                virtualParamtersStack.Push(id);
                            }
                            else if (BuiltInConstants.TryGetValue(id, out var @const))
                            {
                                outputStack.Push(@const);
                            }
                            else if (Parameters.TryGetValue(id, out var parameter))
                            {
                                Variables.Add(id);
                                outputStack.Push(parameter);
                            }
                            else
                            {
                                throw new Exception("Unknown parameter");
                            }

                            infixPostfix = true;
                        }
                        else
                        {
                            throw new Exception("No parameters");
                        }
                    }
                    else if (input[index] == ')' && index >= 1 && input[index - 1] == '(')
                    {
                        while (operatorStack.Count > 0)
                        {
                            if (operatorStack.Peek().Id == IdLeftBracket)
                            {
                                operatorStack.Pop();
                                break;
                            }

                            if (operatorStack.Peek().Id == IdFunction)
                            {
                                BuiltInFunctionOperator op = (BuiltInFunctionOperator)operatorStack.Pop();
                                outputStack.Push(op.Function(outputStack));
                                break;
                            }

                            if (operatorStack.Peek().Id == IdUserFunction)
                            {
                                EvaluateUserFunction((UserFunctionOperator)operatorStack.Pop(), context);
                                break;
                            }

                            EvaluateOperator(operatorStack.Pop(), context);
                        }

                        infixPostfix = true;
                        index++;
                    }
                    // Prefix operators
                    else
                    {
                        switch (c)
                        {
                            case '+': PushOperator(OperatorPositive, context); break;
                            case '-': PushOperator(OperatorNegative, context); break;
                            case '!': PushOperator(OperatorNot, context); break;
                            case '(': PushOperator(OperatorLeftBracket, context); break;
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
                EvaluateOperator(operatorStack.Pop(), context);
            }

            if (outputStack.Count > 1)
            {
                throw new Exception("Invalid expression");
            }

            return outputStack.Pop();
        }

        private UserFunctionOperator CreateOperatorUserFunction(string functionName)
        {
            if (!CustomFunctions.ContainsKey(functionName))
            {
                throw new Exception("Unknown function:" + functionName);
            }

            var userFunc = CustomFunctions[functionName];
            var ufo = new UserFunctionOperator();
            ufo.PureVirtualFunction = userFunc.VirtualParameters;
            ufo.ArgumentsCount = userFunc.ArgumentsCount;
            ufo.ArgumentsStackCount = outputStack.Count;
            ufo.Precedence = userFunc.Infix ? PrecedenceMultiplicative : ufo.Precedence;
            ufo.LeftAssociative = userFunc.Infix ? true : ufo.LeftAssociative;

            ufo.Function = (argumentStack, contextObj) =>
            {
                var result = userFunc.Logic(argumentStack.ToArray(), contextObj);
                argumentStack.Clear();
                return double.Parse(result.ToString()); //TODO: spice expression at the moment evalute only to double ...
            };
            return ufo;
        }

        private void EvaluateUserFunction(UserFunctionOperator op, object context)
        {
            if (op.PureVirtualFunction)
            {
                outputStack.Push(op.Function(virtualParamtersStack, context));
                virtualParamtersStack.Clear();
            }
            else
            {
                var argCount = op.ArgumentsCount != -1 ? op.ArgumentsCount:outputStack.Count - op.ArgumentsStackCount;
                var args = PopAndReturnElements(outputStack, argCount);
                outputStack.Push(op.Function(args, context));
            }
        }

        /// <summary>
        /// Evaluate operators with precedence.
        /// </summary>
        /// <param name="op">Operator</param>
        private void PushOperator(Operator op, object context)
        {
            while (operatorStack.Count > 0)
            {
                // Stop evaluation
                Operator o = operatorStack.Peek();
                if (o.Precedence < op.Precedence || !o.LeftAssociative)
                {
                    break;
                }

                EvaluateOperator(operatorStack.Pop(), context);
            }

            operatorStack.Push(op);
        }

        /// <summary>
        /// Evaluate an operator
        /// </summary>
        /// <param name="op">Operator</param>
        private void EvaluateOperator(Operator op, object context)
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
                case IdOpenConditional:
                    throw new Exception("Unmatched conditional");
                case IdFunction:
                    outputStack.Push(((BuiltInFunctionOperator)op).Function(outputStack));
                    break;
                case IdUserFunction:
                    EvaluateUserFunction((UserFunctionOperator)op, context);
                    break;
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
            if (index < count
                && (input[index] == '.'  || input[index] == ',' && operatorStack.Count == 0))
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
                        case 'μ':
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

        private Stack<object> PopAndReturnElements(Stack<double> stack, int count)
        {
            var result = new List<object>();
            for (var i = 0; i < count; i++)
            {
                result.Add(stack.Pop());
            }

            result.Reverse();
            return new Stack<object>(result);
        }

        /// <summary>
        /// Operator description.
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
            /// Gets operator identifier.
            /// </summary>
            public byte Id { get; }

            /// <summary>
            /// Gets or sets operator precedence.
            /// </summary>
            public byte Precedence { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the operator is left-associative or not.
            /// </summary>
            public bool LeftAssociative { get; set; }
        }

        /// <summary>
        /// Built-in function operator.
        /// </summary>
        private class BuiltInFunctionOperator : Operator
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="BuiltInFunctionOperator"/> class.
            /// </summary>
            /// <param name="func">The function</param>
            public BuiltInFunctionOperator(Func<Stack<double>, double> func)
                : base(IdFunction, byte.MaxValue, false)
            {
                Function = func ?? throw new ArgumentNullException(nameof(func));
            }

            /// <summary>
            /// Gets the function evaluation.
            /// </summary>
            public Func<Stack<double>, double> Function { get; }
        }

        /// <summary>
        /// User function operator.
        /// </summary>
        private class UserFunctionOperator : Operator
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="UserFunctionOperator"/> class.
            /// </summary>
            public UserFunctionOperator()
                : base(IdUserFunction, byte.MaxValue, false)
            {
            }

            /// <summary>
            /// Gets or sets the function evaluation.
            /// </summary>
            public Func<Stack<object>, object, double> Function { get; set; }

            public bool PureVirtualFunction { get; set; }

            public int ArgumentsCount { get; set; }

            public int ArgumentsStackCount { get; set; }
        }
    }
}
