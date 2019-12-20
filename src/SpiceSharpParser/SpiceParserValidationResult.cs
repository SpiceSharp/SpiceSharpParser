using SpiceSharpParser.Lexers;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.Parsers;

namespace SpiceSharpParser
{
    public class SpiceParserValidationResult
    {
        public SpiceParserValidationResult()
        {
            ParsingValidationResult = new ParsingValidationResult();
            SpiceNetlistValidationResult = new SpiceNetlistValidationResult();
        }

        public LexerException LexerException { get; set; }

        public bool AreTokensValid => LexerException == null;

        public ParsingValidationResult ParsingValidationResult { get; set; }

        public SpiceNetlistValidationResult SpiceNetlistValidationResult { get; set; }
    }
}
