using SpiceSharpParser.Models.Netlist.Spice;
using System;

namespace SpiceSharpParser.Common.Validation
{
    public class ValidationEntry
    {
        public ValidationEntry(ValidationEntrySource source, ValidationEntryLevel level, string message, SpiceLineInfo lineInfo = null, Exception exception = null)
        {
            Source = source;
            Level = level;
            Message = message;
            LineInfo = lineInfo;
            Exception = exception;
        }

        public ValidationEntrySource Source { get; }

        public ValidationEntryLevel Level { get; }

        public string Message { get; }

        public SpiceLineInfo LineInfo { get; }

        public Exception Exception { get; }
    }
}
