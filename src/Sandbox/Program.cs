namespace Sandbox
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using SpiceLex;
    using SpiceNetlist.SpiceSharpConnector;
    using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters.Voltage;
    using SpiceParser;
    using SpiceSharp.Simulations;

    public class Program
    {
        public static void Main(string[] args)
        {
            StringBuilder st = new StringBuilder();
            st.Append(@"FILTER

.SUBCKT filter input output params: C=100 R=1000 V=10
C1 output 0 C
R1 input output R
V1 input 0 V
.ENDS filter

X1 IN OUT filter C=100 R=10 V=1

.TRAN 1e-8 10e-2
.SAVE V(OUT)
.end");

            var tokensStr = st.ToString();

            var s0 = new Stopwatch();
            s0.Start();
            SpiceLexer lexer = new SpiceLexer(new NLexer.SpiceLexerOptions() { HasTitle = true });
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
            Console.WriteLine("Translating to NOM (Netlist Object Model):" + s2.ElapsedMilliseconds + "ms");

            var s3 = new Stopwatch();
            s3.Start();
            var connector = new Connector();
            var n = connector.Translate(netlist);
            Console.WriteLine("Translating NOM to SpiceSharp: " + s3.ElapsedMilliseconds + "ms");

            var sim = n.Simulations[0];
            n.Circuit.Nodes.InitialConditions["out"] = 0.0;

            var voltageExport = n.Exports[0] as VoltageExport;
            sim.OnExportSimulationData += (object sender, ExportDataEventArgs data) =>
            {
                Console.WriteLine(data.Time + ";" + voltageExport.Extract() + ";");
            };

            sim.Run(n.Circuit);
        }

    }
}
