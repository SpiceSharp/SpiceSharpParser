using SpiceLexer;
using SpiceNetlist.SpiceSharpConnector;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Plots;
using SpiceParser;
using System;
using System.Linq;

namespace SpiceNetlist.Runner
{
    class SpiceHelper
    {
        public static SpiceSharpConnector.Netlist GetNetList(string text)
        {
            var lexer = new SpiceLexer.SpiceLexer(new SpiceLexerOptions { HasTitle = true });
            var tokensEnumerable = lexer.GetTokens(text);
            var tokens = tokensEnumerable.ToArray();

            var parseTree = new SpiceParser.SpiceParser().GetParseTree(tokens);

            var eval = new ParseTreeEvaluator();
            var netlistObjectModel = eval.Evaluate(parseTree) as SpiceNetlist.Netlist;

            var connector = new Connector();
            var netlist = connector.Translate(netlistObjectModel);

            return netlist;
        }

        public static void RunAllSimulations(SpiceSharpConnector.Netlist netlist)
        {
            foreach (var simulation in netlist.Simulations)
            {
                simulation.Run(netlist.Circuit);
            }
        }

        internal static bool IsPlotPositive(Plot plot)
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
 