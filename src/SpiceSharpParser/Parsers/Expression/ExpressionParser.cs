using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using SpiceSharp;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpBehavioral.Parsers.Helper;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice;

namespace SpiceSharpParser.Parsers.Expression
{
    public class ExpressionParser : SimpleDerivativeParser
    {
        static ExpressionParser()
        {
            DefaultFunctionsCaseSensitive =
                new ConcurrentDictionary<string, SimpleDerivativeParserHelper.DoubleDerivativesFunction>(
                    StringComparerProvider.Get(true));

            DefaultFunctionsCaseInSensitive =
                new ConcurrentDictionary<string, SimpleDerivativeParserHelper.DoubleDerivativesFunction>(
                    StringComparerProvider.Get(false));

            var dict = new Dictionary<string, SimpleDerivativeParserHelper.DoubleDerivativesFunction>()
            {
                { "Exp", SimpleDerivativeParserHelper.ApplyExp },
                { "Log", SimpleDerivativeParserHelper.ApplyLog },
                { "Pow", SimpleDerivativeParserHelper.ApplyPow },
                { "Log10", SimpleDerivativeParserHelper.ApplyLog10 },
                { "Sqrt", SimpleDerivativeParserHelper.ApplySqrt },
                { "Sin", SimpleDerivativeParserHelper.ApplySin },
                { "Cos", ApplyCos },
                { "Tan", ApplyTan },
                { "Asin", SimpleDerivativeParserHelper.ApplyAsin },
                { "Acos", SimpleDerivativeParserHelper.ApplyAcos },
                { "Atan", SimpleDerivativeParserHelper.ApplyAtan },
                { "Abs", SimpleDerivativeParserHelper.ApplyAbs },
                { "Round", SimpleDerivativeParserHelper.ApplyRound },
                { "Min", SimpleDerivativeParserHelper.ApplyMin },
                { "Max", SimpleDerivativeParserHelper.ApplyMax },
            };

            foreach (var function in dict)
            {
                DefaultFunctionsCaseSensitive.TryAdd(function.Key, function.Value);
                DefaultFunctionsCaseInSensitive.TryAdd(function.Key, function.Value);
            }
        }

        /// <summary>
        /// The default functions.
        /// </summary>
        protected static ConcurrentDictionary<string, SimpleDerivativeParserHelper.DoubleDerivativesFunction> DefaultFunctionsCaseSensitive
        {
            get;
        }

        protected static ConcurrentDictionary<string, SimpleDerivativeParserHelper.DoubleDerivativesFunction> DefaultFunctionsCaseInSensitive
        {
            get;
        }

        protected ConcurrentDictionary<string, SimpleDerivativeParserHelper.DoubleDerivativesFunction> DefaultFunctions
        {
            get;
        }

        public ExpressionParser(SpiceNetlistCaseSensitivitySettings caseSensitivitySettings)
        {
            bool isFunctionCaseSensitive = caseSensitivitySettings?.IsFunctionNameCaseSensitive ?? false;
            DefaultFunctions = isFunctionCaseSensitive ? DefaultFunctionsCaseSensitive : DefaultFunctionsCaseInSensitive;
            FunctionFound += ExpressionParser_FunctionFound;
        }

        private void ExpressionParser_FunctionFound(object sender, FunctionFoundEventArgs<Derivatives<Func<double>>> e)
        {
            if (DefaultFunctions.TryGetValue(e.Name, out var function))
            {
                var arguments = new Derivatives<Func<double>>[e.ArgumentCount];
                for (var i = 0; i < e.ArgumentCount; i++)
                {
                    arguments[i] = e[i];
                }

                e.Result = function?.Invoke(arguments);
            }
        }

        /// <summary>
        /// Applies the cosine.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The cosine result.</returns>
        private static Derivatives<Func<double>> ApplyCos(Derivatives<Func<double>>[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            var arg = arguments[0];
            var result = new DoubleDerivatives(arg.Count);
            var a0 = arg[0];
            result[0] = () => Math.Cos(a0());

            // Apply the chain rule
            for (var i = 1; i < arg.Count; i++)
            {
                if (arg[i] != null)
                {
                    var ai = arg[i];
                    result[i] = () => -Math.Sin(a0()) * ai();
                }
            }
            return result;
        }


        /// <summary>
        /// Applies the tangent.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The tangent result.</returns>
        private static Derivatives<Func<double>> ApplyTan(Derivatives<Func<double>>[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            var arg = arguments[0];
            var result = new DoubleDerivatives(arg.Count);
            var a0 = arg[0];
            result[0] = () => Math.Tan(a0());

            // Apply the chain rule
            for (var i = 1; i < arg.Count; i++)
            {
                if (arg[i] != null)
                {
                    var ai = arg[i];
                    result[i] = () =>
                    {
                        var tmp = Math.Cos(a0());
                        return ai() / tmp / tmp;
                    };
                }
            }
            return result;
        }
    }
}
