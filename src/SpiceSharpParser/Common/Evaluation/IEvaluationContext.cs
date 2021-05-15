using System.Collections.Generic;

namespace SpiceSharpParser.Common.Evaluation
{
    public interface IEvaluationContext
    {
        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        Dictionary<string, Expression> Parameters { get; }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        Dictionary<string, Expression> Arguments { get; }

        double Evaluate(string expression);
        bool HaveFunction(string expression, string functionName);
        bool HaveFunctions(string expression);
        bool HaveSpiceProperties(string expression);
    }
}