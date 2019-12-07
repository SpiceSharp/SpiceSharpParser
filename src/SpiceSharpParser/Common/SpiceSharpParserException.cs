using System;
using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser.Common
{
    public class SpiceSharpParserException : Exception
    {
        public SpiceSharpParserException()
        {
        }

        public SpiceSharpParserException(string message)
            : base(message)
        {
        }

        public SpiceSharpParserException(string message, SpiceLineInfo lineInfo)
            : base(
                lineInfo != null ?
                lineInfo.FileName != null
                    ? $"{message} (at line {lineInfo?.LineNumber} from file {lineInfo.FileName})"
                    : $"{message} (at line {lineInfo?.LineNumber})" : $"{message}")
        {
            LineInfo = lineInfo;
        }

        public SpiceSharpParserException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public SpiceSharpParserException(string message, Exception innerException, SpiceLineInfo lineInfo)
            : base(message, innerException)
        {
            LineInfo = lineInfo;
        }

        public SpiceLineInfo LineInfo { get; }
    }
}
