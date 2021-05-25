using System.Collections.Generic;

namespace SpiceSharpParser.Common.Evaluation
{
    public interface IEvaluationContext
    {
        /// <summary>
        /// Gets the parameters.
        /// </summary>
        Dictionary<string, Expression> Parameters { get; }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        Dictionary<string, Expression> Arguments { get; }

        IEvaluator Evaluator { get; set; }
    }
}