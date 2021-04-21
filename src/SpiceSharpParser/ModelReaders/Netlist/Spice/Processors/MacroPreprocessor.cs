using SpiceSharpParser.Lexers.Netlist.Spice.BusPrefix;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Processors
{
    public class MacroPreprocessor : IProcessor
    {
        public SpiceParserValidationResult Validation { get; set; }

        public Statements Process(Statements statements)
        {
            Statements result = new Statements();
            foreach (var statement in statements)
            {
                result.Add(ResolvePrefixNotation(statement));
            }

            return result;
        }

        private Statement ResolvePrefixNotation(Statement statement)
        {
            if (statement is Component component)
            {
                var clone = statement.Clone() as Component;
                clone.PinsAndParameters.Clear();
                clone.PinsAndParameters = Resolve(component.PinsAndParameters);
                return clone;
            }
            return statement;
        }

        private ParameterCollection Resolve(ParameterCollection pinsAndParameters)
        {
            var result = new ParameterCollection();

            foreach (Parameter param in pinsAndParameters)
            {
                if (param is PrefixParameter prefix)
                {
                    ResolvePrefix(prefix, result);
                }
                else
                {
                    result.Add(param);
                }
            }
            return result;
        }

        private void ResolvePrefix(PrefixParameter prefix, ParameterCollection result)
        {
            var lexer = new SpiceSharpParser.Lexers.Netlist.Spice.BusPrefix.Lexer(prefix.Image);
            var parser = new SpiceSharpParser.Lexers.Netlist.Spice.BusPrefix.Parser();
            var node = parser.Parse(lexer);

            ResolvePrefix(node, result);
        }

        private void ResolvePrefix(Lexers.Netlist.Spice.BusPrefix.Node node, ParameterCollection result)
        {
            if (node is PrefixNodeName prefixNode)
            {
                result.Add(new WordParameter(prefixNode.Name));
            }
            else if (node is Prefix prefix)
            {
                for (var i = 0; i < prefix.Value; i++)
                {
                    foreach (var prefixInsideNode in prefix.Nodes)
                    {
                        ResolvePrefix(prefixInsideNode, result);
                    }
                }
            }
        }
    }
}
