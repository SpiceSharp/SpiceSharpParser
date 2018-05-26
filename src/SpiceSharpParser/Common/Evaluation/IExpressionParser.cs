using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SpiceSharpParser.Common
{ 
    public interface IExpressionParser
    {
        double Parse(string expression, object context = null);

        Collection<string> Variables { get; }

        Dictionary<string, double> Parameters { get; }

        Dictionary<string, CustomFunction> CustomFunctions { get; } 
    }
}
