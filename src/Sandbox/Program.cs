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
.save v(out)

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

            var sim = n.Simulations[0];

            var voltageExport = n.Exports[0] as Export;
            sim.OnExportSimulationData += (object sender, ExportDataEventArgs data) =>
            {
                try
                {
                    Console.WriteLine(data.Time + ";" + voltageExport.Extract() + ";");
                }
                catch
                {
                    Console.WriteLine(voltageExport.Extract() + ";");
                }
            };

            sim.Run(n.Circuit);
        }

    }
}
