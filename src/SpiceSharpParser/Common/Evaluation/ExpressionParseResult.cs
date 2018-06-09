using System;
using System.Collections.ObjectModel;

namespace SpiceSharpParser.Common.Evaluation
{
    public class ExpressionParseResult
    {
        public Func<double> Value { get; set; }

        public Collection<string> FoundParameters { get; set; }
    }
}
