using SpiceLexer;
using SpiceNetlist.SpiceSharpConnector;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Plots;
using SpiceParser;
using SpiceParser.Parsing;
using SpiceParser.Translation;
using System;
using System.Linq;

namespace SpiceNetlist.Runner
{
    public class SpiceHelper
    {
        public static SpiceToken[] GetTokens(string text)
        {
            var lexer = new SpiceLexer.SpiceLexer(new SpiceLexerOptions { HasTitle = true });
            var tokensEnumerable = lexer.GetTokens(text);
            return tokensEnumerable.ToArray();
        }

        public static ParseTreeNonTerminalNode GetParseTree(SpiceToken[] tokens)
        {
            return new SpiceParser.Parsing.Parser().GetParseTree(tokens); 
        }

        public static SpiceNetlist.Netlist GetNetlist(ParseTreeNonTerminalNode root)
        {
            var translator = new ParseTreeTranslator();
            return translator.Evaluate(root) as SpiceNetlist.Netlist;
        }

        public static SpiceSharpConnector.Netlist GetSpiceSharpNetlist(SpiceNetlist.Netlist netlist)
        {
            var connector = new Connector();
            return connector.Translate(netlist);
        }
        

        public static bool IsPlotPositive(Plot plot)
        {
            for (var i = 0; i < plot.Series.Count; i++)
            {
                for (var j = 0; j < plot.Series[i].Points.Count; j++)
                {
                    var y = plot.Series[i].Points[j].Y;
                    if (y <= 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
 