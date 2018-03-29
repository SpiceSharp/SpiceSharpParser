using System.Linq;
using SpiceSharpParser.Lexer.Spice3f5;
using SpiceSharpParser.Parser.TreeGeneration;
using SpiceSharpParser.Parser.TreeTranslator;

namespace SpiceSharpParser
{
    /// <summary>
    /// SpiceSharpParser front
    /// </summary>
    public class ParserFrontage
    {
        /// <summary>
        /// Parses the netlist
        /// </summary>
        /// <param name="netlist">Netlist to parse</param>
        /// <param name="settings">Setting for parser</param>
        /// <returns>
        /// A parsing result
        /// </returns>
        public ParserResult Parse(string netlist, ParserSettings settings)
        {
            var tokens = GetTokens(netlist, settings.HasTitle);

            CheckTokens(settings, tokens);

            var parseTreeRoot = GetParseTree(tokens);
            var netlistModel = GetNetlistModel(parseTreeRoot);
            var connectorResult = GetConnectorResult(netlistModel);

            return new ParserResult()
            {
                SpiceSharpModel = connectorResult,
                NetlistModel = netlistModel
            };
        }

        private static void CheckTokens(ParserSettings settings, SpiceToken[] tokens)
        {
            if (settings.IsEndRequired)
            {
                if ((tokens.Length >= 2 && tokens[tokens.Length - 2].SpiceTokenType != SpiceTokenType.END
                    && tokens.Length >= 3 && tokens[tokens.Length - 3].SpiceTokenType != SpiceTokenType.END)
                    || (tokens.Length == 1 && tokens[0].SpiceTokenType == SpiceTokenType.EOF))
                {
                    throw new System.Exception("No .END keyword");
                }
            }
        }

        /// <summary>
        /// Gets the tokens
        /// </summary>
        /// <param name="netlist">Netlist to tokenize</param>
        /// <param name="hasTitle">Has a title</param>
        /// <returns>
        /// Array of tokens
        /// </returns>
        private SpiceToken[] GetTokens(string netlist, bool hasTitle)
        {
            var lexer = new SpiceLexer(new SpiceLexerOptions { HasTitle = hasTitle });
            var tokensEnumerable = lexer.GetTokens(netlist);
            return tokensEnumerable.ToArray();
        }

        /// <summary>
        /// Gets the parse tree
        /// </summary>
        /// <param name="tokens">Array of tokens </param>
        /// <returns>
        /// A reference to the root of parse tree
        /// </returns>
        private ParseTreeNonTerminalNode GetParseTree(SpiceToken[] tokens)
        {
            return new ParserTreeGenerator().GetParseTree(tokens);
        }

        /// <summary>
        /// Gets the netlist model
        /// </summary>
        /// <param name="root">A parse tree root</param>
        /// <returns>
        /// A netlist model
        /// </returns>
        private Model.Netlist GetNetlistModel(ParseTreeNonTerminalNode root)
        {
            var translator = new ParseTreeTranslator();
            return translator.Evaluate(root) as SpiceSharpParser.Model.Netlist;
        }

        /// <summary>
        /// Gets the SpiceSharp model for the netlist
        /// </summary>
        /// <param name="netlist">Netlist model</param>
        /// <returns>
        /// A new SpiceSharp model for the netlist
        /// </returns>
        private Connector.ConnectorResult GetConnectorResult(SpiceSharpParser.Model.Netlist netlist)
        {
            var connector = new Connector.Connector();
            return connector.Translate(netlist);
        }
    }
}
