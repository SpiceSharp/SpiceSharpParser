using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation
{
    public interface IExpressionFeaturesReader
    {
        bool HaveSpiceProperties(string expression, EvaluationContext context);

        bool HaveFunctions(string expression, EvaluationContext context);

        bool HaveFunction(string expression, string functionName, EvaluationContext context);

        IEnumerable<string> GetParameters(string expression, EvaluationContext context, bool @throw = true);
    }
}
