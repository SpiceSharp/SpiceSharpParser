namespace Sandbox
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using SpiceLex;
    using SpiceNetlist.SpiceSharpConnector;
    using SpiceParser;

    public class Program
    {
        public static void Main(string[] args)
        {
            StringBuilder st = new StringBuilder();
            st.Append(@"Lowpass filter

.end");

            var tokensStr = st.ToString();

            var s0 = new Stopwatch();
            s0.Start();
            SpiceLexer lexer = new SpiceLexer();
            var tokensEnumerable = lexer.GetTokens(tokensStr);
            var tokens = tokensEnumerable.ToArray();
            Console.WriteLine("Lexer: " + s0.ElapsedMilliseconds + "ms");

            var s1 = new Stopwatch();
            s1.Start();
            var parseTree = new SpiceParser().GetParseTree(tokens);
            Console.WriteLine("Parse tree generated: " + s1.ElapsedMilliseconds + "ms");

            var s2 = new Stopwatch();
            s2.Start();
            var translator = new ParseTreeTranslator();
            var context = translator.GetNetList(parseTree);
            Console.WriteLine("Translating to NOM (Netlist Object Model):" + s2.ElapsedMilliseconds + "ms");

            var s3 = new Stopwatch();
            s3.Start();
            var connector = new Connector();
            var n = connector.Translate(context);
            Console.WriteLine("Translating NOM to SpiceSharp: " + s3.ElapsedMilliseconds + "ms");
        }
    }
}
