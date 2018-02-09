using SpiceLex;
using SpiceNetlist.SpiceSharpConnector;
using SpiceParser;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            StringBuilder st = new StringBuilder();
            st.Append(@"Example 3 for interconnect simulation");
            for (var i = 0; i < 10000; i++)
            {
                st.Append("* aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa aaaaaaaaaaaaaaaa aaaaaaa\r\n");
                st.Append("\n");
            }

            Console.WriteLine("L complete");

            var tokensStr = st.ToString();

            Console.WriteLine("L str complete, len=" + tokensStr.Length);

            var s0 = new Stopwatch();
            s0.Start();
            SpiceLexer lexer = new SpiceLexer();
            var tokensEnumerable = lexer.GetTokens(tokensStr);
            var tokens = tokensEnumerable.ToArray();
            Console.WriteLine("Lexer: " + s0.ElapsedMilliseconds + "ms");

            var s1 = new Stopwatch();
            s1.Start();
            var parseTree = new SpiceParser.SpiceParser().GetParseTree(tokens);
            Console.WriteLine("Parse tree generated: " + s1.ElapsedMilliseconds + "ms");

            Console.WriteLine("Translating");
            var s2 = new Stopwatch();
            s2.Start();
            var translator = new ParseTreeTranslator();
            var netList = translator.GetNetList(parseTree);
            Console.WriteLine(s2.ElapsedMilliseconds + "ms");
        }
    }
}
