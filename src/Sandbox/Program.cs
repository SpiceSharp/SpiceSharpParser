namespace Sandbox
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using SpiceLexer;
    using SpiceNetlist.SpiceSharpConnector;
    using SpiceParser;
    using SpiceSharp.Parser.Readers;
    using SpiceSharp.Simulations;

    public class Program
    {
        public static void Main(string[] args)
        {
            StringBuilder st = new StringBuilder();
            st.Append(@"BJT Noise Test

vcc 4 0 50
vin 1 0 ac

ccouple 1 2 1

ibias 0 2 100u

rload 4 3 1k

q1 3 2 0 0 test

.model test npn kf=1e-20 af=1 bf=100 rb=10
.noise v(3) vin dec 10 10 100k 1
.save v(1)
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
            var parseTree = new SpiceParser().GetParseTree(tokens);
            Console.WriteLine("Parse tree generated: " + s1.ElapsedMilliseconds + "ms");

            var s2 = new Stopwatch();
            s2.Start();
            var eval = new ParseTreeEvaluator();
            var netlist = eval.Evaluate(parseTree) as SpiceNetlist.NetList;
            Console.WriteLine("Translating to Netlist Object Model:" + s2.ElapsedMilliseconds + "ms");

            var s3 = new Stopwatch();
            s3.Start();
            var connector = new Connector();
            var n = connector.Translate(netlist);
            Console.WriteLine("Translating  Netlist Object Model to SpiceSharp: " + s3.ElapsedMilliseconds + "ms");

            Console.WriteLine("Done");

            var export = n.Exports[0];

            n.Simulations[0].OnExportSimulationData += (object sender, ExportDataEventArgs e) => {
                Console.WriteLine(export.Extract());
            };
            n.Simulations[0].Run(n.Circuit);
        }
    }
}
