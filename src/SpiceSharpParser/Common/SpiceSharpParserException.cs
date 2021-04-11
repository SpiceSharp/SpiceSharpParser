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
            : base(CreateExceptionMessage(message, lineInfo))
        {
            LineInfo = lineInfo;
        }

        public SpiceSharpParserException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public SpiceSharpParserException(string message, Exception innerException, SpiceLineInfo lineInfo)
            : base(CreateExceptionMessage(message, lineInfo), innerException)
        {
            LineInfo = lineInfo;
        }

        public SpiceLineInfo LineInfo { get; }

        private static string CreateExceptionMessage(string message, SpiceLineInfo lineInfo)
        {
            return lineInfo != null ?
                (lineInfo.FileName != null
                    ? $"{message} (at line {lineInfo?.LineNumber}, start column = {lineInfo?.StartColumnIndex}, end column = {lineInfo?.EndColumnIndex} from file {lineInfo.FileName})"
                    : $"{message} (at line {lineInfo?.LineNumber}, start column = {lineInfo?.StartColumnIndex}, end column = {lineInfo?.EndColumnIndex})") : $"{message}";
        }
    }
}
