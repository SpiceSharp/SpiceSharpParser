using SpiceSharpParser.Lexers;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.Parsers;

namespace SpiceSharpParser
{
    public class SpiceParserValidationResult
    {
        public SpiceParserValidationResult()
        {
            Lexing = new LexerValidationResult();
            Parsing = new ParsingValidationResult();
            Reading = new SpiceNetlistValidationResult();
        }

        public LexerValidationResult Lexing { get; set; }

        public ParsingValidationResult Parsing { get; set; }

        public SpiceNetlistValidationResult Reading { get; set; }
    }
}
