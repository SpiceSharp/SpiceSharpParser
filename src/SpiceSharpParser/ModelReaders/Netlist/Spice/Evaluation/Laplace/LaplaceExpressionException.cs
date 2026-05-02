using System;
using SpiceSharpParser.Common;
using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Laplace
{
    internal class LaplaceExpressionException : SpiceSharpParserException
    {
        public LaplaceExpressionException(string message)
            : base(message)
        {
        }

        public LaplaceExpressionException(string message, SpiceLineInfo lineInfo)
            : base(message, lineInfo)
        {
        }

        public LaplaceExpressionException(string message, Exception innerException, SpiceLineInfo lineInfo)
            : base(message, innerException, lineInfo)
        {
        }
    }
}
