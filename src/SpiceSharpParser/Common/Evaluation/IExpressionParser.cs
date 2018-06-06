using SpiceSharpParser.Common.Evaluation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SpiceSharpParser.Common
{
    public interface IExpressionParser
    {
        Collection<string> ParametersFoundInLastParse { get; }

        Dictionary<string, LazyExpression> Parameters { get; }

        Dictionary<string, CustomFunction> CustomFunctions { get; }

        Func<double> Parse(string expression, object context = null, IEvaluator evaluator = null);
    }
}
