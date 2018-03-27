namespace Sandbox
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using SpiceLexer;
    using SpiceNetlist.SpiceSharpConnector;
    using SpiceParser;
    using SpiceParser.Parsing;
    using SpiceParser.Translation;

    public class Program
    {
        public static void Main(string[] args)
        {
            StringBuilder st = new StringBuilder();
            st.Append(@"Lowpass filter

V1 net2 net1 dc 0 ac 24 0
V2 net1 0 dc 24
X1 net2 net3 lcfilter C=100
L2 net3 out 250m
Rload out 0 1k

.SUBCKT lcfilter IN OUT params: L=100 C=10
L1 IN OUT {L*1m}
C1 OUT 0 {C*1u}
.ENDS lcfilter

* Do Simulation
.ac lin 30 500 15k
.PLOT AC v(out)

.end

");
            var tokensStr = st.ToString();

            var s0 = new Stopwatch();
            s0.Start();
            SpiceLexer lexer = new SpiceLexer(new SpiceLexerOptions() { HasTitle = true });
            var tokensEnumerable = lexer.GetTokens(tokensStr);
            var tokens = tokensEnumerable.ToArray();
            Console.WriteLine("Lexer: " + s0.ElapsedMilliseconds + "ms");

            var s1 = new Stopwatch();
            s1.Start();
            var parseTree = new Parser().GetParseTree(tokens);
            Console.WriteLine("Parse tree generated: " + s1.ElapsedMilliseconds + "ms");

            var s2 = new Stopwatch();
            s2.Start();
            var eval = new ParseTreeTranslator();
            var netlist = eval.Evaluate(parseTree) as SpiceNetlist.Netlist;
            Console.WriteLine("Translating to Netlist Object Model:" + s2.ElapsedMilliseconds + "ms");

            var s3 = new Stopwatch();
            s3.Start();
            var connector = new Connector();
            var n = connector.Translate(netlist);
            Console.WriteLine("Translating  Netlist Object Model to SpiceSharp: " + s3.ElapsedMilliseconds + "ms");

            Console.WriteLine("Warning: " + n.Warnings.Count);

            Console.WriteLine("Done");

            n.Simulations[0].Run(n.Circuit);
            var plot = n.Plots[0];
            var csv = plot.ExportToCSV();

            File.WriteAllText("C:/2.csv", csv);
        }
    }
}
