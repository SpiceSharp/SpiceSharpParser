using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser.Common.Validation
{
    public class ValidationEntry
    {
        public ValidationEntry(ValidationEntrySource source, ValidationEntryLevel level, string message, SpiceLineInfo lineInfo = null)
        {
            Source = source;
            Level = level;
            Message = message;
            LineInfo = lineInfo;
        }

        public ValidationEntrySource Source { get; }

        public ValidationEntryLevel Level { get; }

        public string Message { get; }

        public SpiceLineInfo LineInfo { get; }
    }
}
