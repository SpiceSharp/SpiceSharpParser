using SpiceSharpParser.Common;

namespace SpiceSharpParser.Parsers
{
    public class ParsingValidationResult
    {
        public SpiceSharpParserException ParsingException { get; set; }

        public bool IsValid => ParsingException == null;
    }
}
