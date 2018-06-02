using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SpiceSharpParser.Common
{
    public interface IExpressionParser
    {
        Collection<string> ParametersFoundInLastParse { get; }

        Dictionary<string, double> Parameters { get; }

        Dictionary<string, CustomFunction> CustomFunctions { get; }

        double Parse(string expression, object context = null, IEvaluator evaluator = null);
    }
}
