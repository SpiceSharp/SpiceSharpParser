using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.Parsers.Expression
{
    /// <summary>
    /// @author: Sven Boulanger
    /// @author: Marcin Gołębiowski (functions, lazy evaluation)
    /// A very light-weight and fast expression parser made for parsing SPICE expressions
    /// It is based on Dijkstra's Shunting Yard algorithm. It is very fast for parsing expressions only once.
    /// The parser is also not very expressive for errors, so only use it for relatively simple expressions.
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
        private readonly Stack<Func<ExpressionEvaluationContext, double>> outputStack = new Stack<Func<ExpressionEvaluationContext, double>>();
        private readonly Stack<object> virtualParametersStack = new Stack<object>();
        private readonly Stack<Operator> operatorStack = new Stack<Operator>();
        private readonly StringBuilder sb = new StringBuilder();
        private string input;
        private bool infixPostfix;
        private int count;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceExpressionParser"/> class.
        /// </summary>
        /// <param name="isNegationAssociative">Specifies whether negation is associative.</param>
        public SpiceExpressionParser(bool isNegationAssociative = false)
        {
            OperatorNegative.LeftAssociative = isNegationAssociative;
        }

        /// <summary>
        /// Parses an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="context">The expression context.</param>
        /// <returns>Returns the result of parse.</returns>
        public ExpressionParseResult Parse(string expression, ExpressionParserContext context)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var foundParameters = new HashSet<string>();
            var foundFunctions = new HashSet<string>();

            // Initialize for parsing the expression
            int index = 0;
            input = expression ?? throw new ArgumentNullException(nameof(expression));
            infixPostfix = false;
            outputStack.Clear();
            operatorStack.Clear();
            count = input.Length;

            // Parse the expression
            while (index < count)
            {
                // Skip spaces and curly brackets
                while (index < count && (input[index] == ' ' || (input[index] == '{' || (input[index] == '}'))))
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
                        case '+': PushOperator(expression, OperatorAdd); break;
                        case '-': PushOperator(expression, OperatorSubtract); break;
                        case '*':
                            if ((index + 1 < count) && input[index + 1] == '*')
                            {
                                PushOperator(expression, CreateOperatorForFunction("**", index, context));
                                index++;
                                break;
                            }

                            PushOperator(expression, OperatorMultiply);
                            break;
                        case '/': PushOperator(expression, OperatorDivide); break;
                        case '%': PushOperator(expression, OperatorModulo); break;
                        case '=':
                            index++;
                            if (index < count && input[index] == '=')
                            {
                                PushOperator(expression, OperatorEquals);
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
                                PushOperator(expression, OperatorInequals);
                            }
                            else
                            {
                                goto default;
                            }

                            break;
                        case '?': PushOperator(expression, OperatorOpenConditional); break;
                        case ':':
                            // Evaluate to an open conditional
                            while (operatorStack.Count > 0)
                            {
                                if (operatorStack.Peek().Id == IdOpenConditional)
                                {
                                    break;
                                }

                                EvaluateOperator(expression, operatorStack.Pop());
                            }

                            var x = operatorStack.Pop();
                            operatorStack.Push(OperatorClosedConditional);
                            break;
                        case '|':
                            index++;
                            if (index < count && input[index] == '|')
                            {
                                PushOperator(expression, OperatorConditionalOr);
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
                                PushOperator(expression, OperatorConditionalAnd);
                            }
                            else
                            {
                                goto default;
                            }

                            break;
                        case '#':
                            int startIndex = index;
                            index++;
                            while (index < count && input[index] != '#')
                            {
                                index++;
                            }

                            int endIndex = index;
                            if (index != count)
                            {
                                virtualParametersStack
                                    .Push(input.Substring(startIndex + 1, endIndex - startIndex - 1));
                                index++;
                            }

                            break;
                        case '<':
                            if (index + 1 < count && input[index + 1] == '=')
                            {
                                PushOperator(expression, OperatorLessOrEqual);
                                index++;
                            }
                            else
                            {
                                PushOperator(expression, OperatorLess);
                            }

                            break;
                        case '>':
                            if (index + 1 < count && input[index + 1] == '=')
                            {
                                PushOperator(expression, OperatorGreaterOrEqual);
                                index++;
                            }
                            else
                            {
                                PushOperator(expression, OperatorGreater);
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
                                    FunctionOperator op2 = (FunctionOperator)operatorStack.Pop();
                                    EvaluateFunction(expression, op2, index);
                                    break;
                                }

                                EvaluateOperator(expression, operatorStack.Pop());
                            }

                            infixPostfix = true;
                            break;

                        case '[':
                            break;
                        case ']':
                            EvaluateFunction(expression, (FunctionOperator)operatorStack.Pop(), index);
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

                                EvaluateOperator(expression, operatorStack.Pop());
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

                        int tmpIndex = index;
                        while (tmpIndex < count && input[tmpIndex] != '[')
                        {
                            tmpIndex++;
                        }

                        if (tmpIndex != count)
                        {
                            if (context.Functions.ContainsKey("@"))
                            {
                                operatorStack.Push(CreateOperatorForFunction("@", index, context));
                            }
                            else
                            {
                                throw new FunctionNotFoundException("@");
                            }

                            infixPostfix = false;
                        }
                    }
                    else if (c == '.' || (c >= '0' && c <= '9'))
                    {
                        if (operatorStack.Count > 0
                            && operatorStack.FirstOrDefault(o => o.Id == IdFunction) != null)
                        {
                            if (operatorStack.Peek() is FunctionOperator fo && fo.VirtualParameters)
                            {
                                int startIndex = index;

                                while (index < expression.Length)
                                {
                                    if (expression[index] == ',')
                                    {
                                        break;
                                    }

                                    if (expression[index] == ')')
                                    {
                                        break;
                                    }

                                    index++;
                                }

                                var virtualParameter = expression.Substring(startIndex, index - startIndex);
                                virtualParametersStack.Push(virtualParameter);
                            }
                            else
                            {
                                double parseResult = ParseDouble(expression, ref index);
                                outputStack.Push((evalContext) => parseResult);
                            }
                        }
                        else
                        {
                            double parseResult = ParseDouble(expression, ref index);
                            outputStack.Push((evalContext) => parseResult);
                        }

                        infixPostfix = true;
                    }

                    // Parse a parameter or a function
                    else if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c == '_') || (c == '.'))
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
                            var functionName = sb.ToString();

                            if (context.Functions.ContainsKey(functionName))
                            {
                                foundFunctions.Add(functionName);
                                operatorStack.Push(CreateOperatorForFunction(functionName, index, context));
                            }
                            else
                            {
                                throw new FunctionNotFoundException(functionName);
                            }
                        }
                        else
                        {
                            string parameterName = sb.ToString();

                            if (operatorStack.Count > 0 && operatorStack.Peek().Id == IdFunction
                                                        && ((FunctionOperator)operatorStack.Peek()).VirtualParameters)
                            {
                                virtualParametersStack.Push(parameterName);
                            }
                            else
                            {
                                foundParameters.Add(parameterName);
                                outputStack.Push((evalContext) =>
                                {
                                    if (!evalContext.ExpressionContext.Parameters.ContainsKey(parameterName))
                                    {
                                        throw new UnknownParameterException() { Name = parameterName };
                                    }

                                    return evalContext.ExpressionContext.Parameters[parameterName].Evaluate(evalContext.Evaluator, evalContext.ExpressionContext);
                                });
                            }

                            infixPostfix = true;
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
                                EvaluateFunction(expression, (FunctionOperator)operatorStack.Pop(), index);
                                break;
                            }

                            EvaluateOperator(expression, operatorStack.Pop());
                        }

                        infixPostfix = true;
                        index++;
                    }
                    else if (input[index] == '#')
                    {
                        int startIndex = index;
                        index++;
                        while (index < count && input[index] != '#')
                        {
                            index++;
                        }

                        int endIndex = index;
                        if (index != count)
                        {
                            virtualParametersStack.Push(input.Substring(startIndex + 1, endIndex - startIndex - 1));
                            index++;
                            infixPostfix = true;
                        }
                    }

                    // Prefix operators
                    else
                    {
                        switch (c)
                        {
                            case '+': PushOperator(expression, OperatorPositive); break;
                            case '-': PushOperator(expression, OperatorNegative); break;
                            case '!': PushOperator(expression, OperatorNot); break;
                            case '(': PushOperator(expression, OperatorLeftBracket); break;
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
                EvaluateOperator(expression, operatorStack.Pop());
            }

            if (outputStack.Count > 1)
            {
                throw new Exception("Invalid expression");
            }

            return new ExpressionParseResult()
            {
                Value = outputStack.Pop(),
                FoundFunctions = foundFunctions,
                FoundParameters = foundParameters,
            };
        }

        private FunctionOperator CreateOperatorForFunction(
            string functionName,
            int startIndex,
            ExpressionParserContext context)
        {
            if (!context.Functions.ContainsKey(functionName))
            {
                throw new FunctionNotFoundException(functionName);
            }

            var functions = context.Functions[functionName];

            var cfo = new FunctionOperator();
            cfo.Name = functionName;
            cfo.StartIndex = startIndex;
            cfo.VirtualParameters = functions.First().VirtualParameters;
            cfo.ArgumentsStackCount = outputStack.Count;
            cfo.LeftAssociative = functions.First().Infix || cfo.LeftAssociative;
            cfo.Precedence = functions.First().Infix ? PrecedenceMultiplicative : cfo.Precedence;
            cfo.Functions = functions;
            return cfo;
        }

        private void EvaluateFunction(string expression, FunctionOperator op, int? endIndex = null)
        {
            if (op.VirtualParameters)
            {
                if (op.Functions.Count > 1)
                {
                    throw new InvalidOperationException("Cannot have more than one virtual function with same name");
                }

                var function = op.Functions.Single();

                if (!(function is IFunction<object, double> df))
                {
                    throw new InvalidOperationException("Virtual function needs to have objects as arguments");
                }

                var argCount = function.ArgumentsCount != -1 ? function.ArgumentsCount : virtualParametersStack.Count;
                var args = PopAndReturnArguments(virtualParametersStack, argCount);

                outputStack.Push((evalContext) =>
                {
                    if (endIndex != null)
                    {
                        return df.Logic(
                            op.Name + expression.Substring(op.StartIndex - 1, endIndex.Value - op.StartIndex + 2),
                            args,
                            evalContext.Evaluator,
                            evalContext.ExpressionContext);
                    }

                    return df.Logic(null, args, evalContext.Evaluator, evalContext.ExpressionContext);
                });
            }
            else
            {
                var functionArguments = outputStack.Count - op.ArgumentsStackCount;

                IFunction function;

                if (op.Functions.Count == 1 && op.Functions.First().Infix)
                {
                    functionArguments++;
                }

                if (op.Functions.Count == 1 && op.Functions.First().ArgumentsCount == -1)
                {
                    function = op.Functions.First();
                }
                else
                {
                    function = op.Functions.SingleOrDefault(f => f.ArgumentsCount == functionArguments);

                    if (function == null)
                    {
                        throw new InvalidOperationException(
                            $"Function {op.Name} has been called with unsupported number of arguments");
                    }
                }

                if (!(function is IFunction<double, double> df))
                {
                    throw new InvalidOperationException("Virtual function needs to have double as arguments");
                }

                var args = PopAndReturnArguments(outputStack, functionArguments);

                if (endIndex != null)
                {
                    outputStack.Push((evalContext) =>
                    {
                        var evaluatedArgs = Evaluate(args, evalContext);

                        return df.Logic(
                            op.Name
                            + expression.Substring(op.StartIndex - 1, endIndex.Value - op.StartIndex + 2),
                            evaluatedArgs,
                            evalContext.Evaluator,
                            evalContext.ExpressionContext);
                    });
                }
                else
                {
                    outputStack.Push((evalContext) =>
                    {
                        var evaluatedArgs = Evaluate(args, evalContext);
                        return df.Logic(null, evaluatedArgs, evalContext.Evaluator, evalContext.ExpressionContext);
                    });
                }
            }
        }

        private double[] Evaluate(Func<ExpressionEvaluationContext, double>[] args, ExpressionEvaluationContext evalContext)
        {
            var values = new List<double>(args.Length);
            foreach (var arg in args)
            {
                values.Add(arg(evalContext));
            }

            return values.ToArray();
        }

        /// <summary>
        /// Evaluate operators with precedence.
        /// </summary>
        /// <param name="op">Operator.</param>
        private void PushOperator(string expression, Operator op)
        {
            while (operatorStack.Count > 0)
            {
                // Stop evaluation
                Operator o = operatorStack.Peek();
                if (o.Precedence < op.Precedence || !o.LeftAssociative)
                {
                    break;
                }

                EvaluateOperator(expression, operatorStack.Pop());
            }

            operatorStack.Push(op);
        }

        /// <summary>
        /// Evaluate an operator.
        /// </summary>
        /// <param name="op">Operator.</param>
        private void EvaluateOperator(string expression, Operator op)
        {
            Func<ExpressionEvaluationContext, double> a, b, c;
            switch (op.Id)
            {
                case IdPositive: break;
                case IdNegative:
                    a = outputStack.Pop();
                    outputStack.Push((context) => -a(context)); break;
                case IdNot:
                    a = outputStack.Pop();
                    outputStack.Push((context) => a(context).Equals(0.0) ? 1.0 : 0.0);
                    break;
                case IdAdd:
                    a = outputStack.Pop();
                    b = outputStack.Pop();
                    outputStack.Push((context) => a(context) + b(context)); break;
                case IdSubtract:
                    b = outputStack.Pop();
                    a = outputStack.Pop();
                    outputStack.Push((context) => a(context) - b(context));
                    break;
                case IdMultiply:
                    a = outputStack.Pop();
                    b = outputStack.Pop();
                    outputStack.Push((context) => a(context) * b(context)); break;
                case IdDivide:
                    b = outputStack.Pop();
                    a = outputStack.Pop();
                    outputStack.Push((context) => a(context) / b(context));
                    break;
                case IdModulo:
                    b = outputStack.Pop();
                    a = outputStack.Pop();
                    outputStack.Push((context) => a(context) % b(context));
                    break;
                case IdEquals:
                    a = outputStack.Pop();
                    b = outputStack.Pop();
                    outputStack.Push((context) => (a(context).Equals(b(context)) ? 1.0 : 0.0));
                    break;
                case IdInequals:
                    a = outputStack.Pop();
                    b = outputStack.Pop();
                    outputStack.Push((context) => (!a(context).Equals(b(context)) ? 1.0 : 0.0));
                    break;
                case IdConditionalAnd:
                    b = outputStack.Pop();
                    a = outputStack.Pop();
                    outputStack.Push((context) => (!a(context).Equals(0.0) && !b(context).Equals(0.0) ? 1.0 : 0.0));
                    break;
                case IdConditionalOr:
                    b = outputStack.Pop();
                    a = outputStack.Pop();
                    outputStack.Push((context) => (!a(context).Equals(0.0) || !b(context).Equals(0.0) ? 1.0 : 0.0));
                    break;
                case IdLess:
                    b = outputStack.Pop();
                    a = outputStack.Pop();
                    outputStack.Push((context) => (a(context) < b(context) ? 1.0 : 0.0));
                    break;
                case IdLessOrEqual:
                    b = outputStack.Pop();
                    a = outputStack.Pop();
                    outputStack.Push((context) => (a(context) <= b(context) ? 1.0 : 0.0));
                    break;
                case IdGreater:
                    b = outputStack.Pop();
                    a = outputStack.Pop();
                    outputStack.Push((context) => (a(context) > b(context) ? 1.0 : 0.0));
                    break;
                case IdGreaterOrEqual:
                    b = outputStack.Pop();
                    a = outputStack.Pop();
                    outputStack.Push((context) => (a(context) >= b(context) ? 1.0 : 0.0));
                    break;
                case IdClosedConditional:
                    c = outputStack.Pop();
                    b = outputStack.Pop();
                    a = outputStack.Pop();
                    outputStack.Push((context) => a(context) > 0.0 ? b(context) : c(context));
                    break;
                case IdOpenConditional:
                    throw new Exception("Unmatched conditional");
                case IdLeftBracket:
                    break;
                case IdFunction:
                    EvaluateFunction(expression, (FunctionOperator)op, null);
                    break;
                default:
                    throw new Exception("Unrecognized operator");
            }
        }

        /// <summary>
        /// Parse a double value.
        /// </summary>
        /// <returns>Parse result.</returns>
        private double ParseDouble(string expression, ref int index)
        {
            // Read integer part
            double value = 0.0;
            int expressionIndex = index;
            int expressionLength = expression.Length;

            while (expressionIndex < expressionLength && (expression[expressionIndex] >= '0' && expression[expressionIndex] <= '9'))
            {
                value = (value * 10.0) + (expression[expressionIndex++] - '0');
            }

            // Read decimal part
            if (expressionIndex < expressionLength
                && (expression[expressionIndex] == '.'))
            {
                expressionIndex++;
                double mult = 1.0;
                while (expressionIndex < expressionLength && (expression[expressionIndex] >= '0' && expression[expressionIndex] <= '9'))
                {
                    value = (value * 10.0) + (expression[expressionIndex++] - '0');
                    mult = mult * 10.0;
                }

                value /= mult;
            }

            if (expressionIndex < expressionLength)
            {
                // Scientific notation
                if (expression[expressionIndex] == 'e' || expression[expressionIndex] == 'E')
                {
                    expressionIndex++;
                    var exponent = 0;
                    var neg = false;
                    if (expressionIndex < expressionLength && (expression[expressionIndex] == '+' || expression[expressionIndex] == '-'))
                    {
                        if (expression[expressionIndex] == '-')
                        {
                            neg = true;
                        }

                        expressionIndex++;
                    }

                    // Get the exponent
                    while (expressionIndex < expressionLength && (expression[expressionIndex] >= '0' && expression[expressionIndex] <= '9'))
                    {
                        exponent = (exponent * 10) + (expression[expressionIndex++] - '0');
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
                    switch (expression[expressionIndex])
                    {
                        case 't':
                        case 'T': value *= 1.0e12; expressionIndex++; break;
                        case 'g':
                        case 'G': value *= 1.0e9; expressionIndex++; break;
                        case 'x':
                        case 'X': value *= 1.0e6; expressionIndex++; break;
                        case 'k':
                        case 'K': value *= 1.0e3; expressionIndex++; break;
                        case 'u':
                        case 'µ':
                        case 'U': value /= 1.0e6; expressionIndex++; break;
                        case 'n':
                        case 'N': value /= 1.0e9; expressionIndex++; break;
                        case 'p':
                        case 'P': value /= 1.0e12; expressionIndex++; break;
                        case 'f':
                        case 'F': value /= 1.0e15; expressionIndex++; break;
                        case 'm':
                        case 'M':
                            if (expressionIndex + 2 < expressionLength &&
                                (expression[expressionIndex + 1] == 'e' || expression[expressionIndex + 1] == 'E') &&
                                (expression[expressionIndex + 2] == 'g' || expression[expressionIndex + 2] == 'G'))
                            {
                                value *= 1.0e6;
                                expressionIndex += 3;
                            }
                            else if (expressionIndex + 2 < expressionLength &&
                                (expression[expressionIndex + 1] == 'i' || expression[expressionIndex + 1] == 'I') &&
                                (expression[expressionIndex + 2] == 'l' || expression[expressionIndex + 2] == 'L'))
                            {
                                value *= 25.4e-6;
                                expressionIndex += 3;
                            }
                            else
                            {
                                value /= 1.0e3;
                                expressionIndex++;
                            }

                            break;
                    }
                }

                // Any trailing letters are ignored
                while (expressionIndex < expressionLength && ((expression[expressionIndex] >= 'a' && expression[expressionIndex] <= 'z') || (expression[expressionIndex] >= 'A' && expression[expressionIndex] <= 'Z')))
                {
                    expressionIndex++;
                }
            }

            index = expressionIndex;
            return value;
        }

        private Func<ExpressionEvaluationContext, double>[] PopAndReturnArguments(Stack<Func<ExpressionEvaluationContext, double>> stack, int count)
        {
            var result = new List<Func<ExpressionEvaluationContext, double>>(count);
            for (var i = 0; i < count; i++)
            {
                var val = stack.Pop();
                result.Add((context) => val(context));
            }

            result.Reverse();
            return result.ToArray();
        }

        private object[] PopAndReturnArguments(Stack<object> stack, int count)
        {
            var result = new List<object>(count);
            for (var i = 0; i < count; i++)
            {
                var val = stack.Pop();
                result.Add(val);
            }

            result.Reverse();
            return result.ToArray();
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
        /// User function operator.
        /// </summary>
        private class FunctionOperator : Operator
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FunctionOperator"/> class.
            /// </summary>
            public FunctionOperator()
                : base(IdFunction, byte.MaxValue, false)
            {
            }

            /// <summary>
            /// Gets or sets the function name.
            /// </summary>
            public string Name { get; set; }

            public bool VirtualParameters { get; set; }

            public List<IFunction> Functions { get; set; }

            public int ArgumentsStackCount { get; set; }

            public int StartIndex { get; internal set; }
        }
    }
}
