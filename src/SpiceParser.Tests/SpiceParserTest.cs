using SpiceNetlist;
using System;
using Xunit;

namespace SpiceParser.Tests
{
    public class SpiceParserTest
    {
        [Fact]
        public void VectorTest()
        {
            var parser = new SpiceParser();

            var vectorTokens = new NLexer.Token[]
            {
                new NLexer.Token((int)SpiceToken.WORD, "v"),
                new NLexer.Token((int)SpiceToken.DELIMITER, "("),
                new NLexer.Token((int)SpiceToken.WORD, "out"),
                new NLexer.Token((int)SpiceToken.COMMA, ","),
                new NLexer.Token((int)SpiceToken.WORD, "0"),
                new NLexer.Token((int)SpiceToken.DELIMITER, ")")
            };

            try
            {
                ParseTreeNonTerminalNode tree = parser.GetParseTree(vectorTokens, SpiceNetlist.SpiceGrammarSymbol.PARAMETER);
            }
            catch (Exception ex)
            {
                Assert.True(false, "Exception during parsing vector tokens");
            }
        }
    }
}
