using SpiceSharpParser.Lexers.Netlist.Spice.BusPrefix;
using SpiceSharpParser.Lexers.Netlist.Spice.BusSuffix;
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
                result.Add(Resolve(statement));
            }

            return result;
        }

        private Statement Resolve(Statement statement)
        {
            if (statement is Component component)
            {
                var clone = statement.Clone() as Component;
                clone.PinsAndParameters.Clear();
                clone.PinsAndParameters = Resolve(component.PinsAndParameters);
                return clone;
            }

            if (statement is SubCircuit subCircuit)
            {
                var clone = statement.Clone() as SubCircuit;
                clone.Pins = Resolve(subCircuit.Pins);

                var statementsClone = clone.Statements.Clone();
                clone.Statements.Clear();

                foreach (var subCircuitStatement in statementsClone as Statements)
                {
                    clone.Statements.Add(Resolve(subCircuitStatement));
                }

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
                else if (param is SuffixParameter suffix)
                {
                    ResolveSuffix(suffix, result);
                }
                else
                {
                    result.Add(param);
                }
            }

            return result;
        }

        private void ResolveSuffix(SuffixParameter suffix, ParameterCollection result)
        {
            var lexer = new SpiceSharpParser.Lexers.Netlist.Spice.BusSuffix.Lexer(suffix.Image);
            var parser = new SpiceSharpParser.Lexers.Netlist.Spice.BusSuffix.Parser();
            var suffixNode = parser.Parse(lexer);

            foreach (var node in suffixNode.Nodes)
            {
                if (node is NumberNode numberNode)
                {
                    result.Add(new WordParameter($"{suffixNode.Name}<{numberNode.Node}>"));
                }

                if (node is RangeNode rangeNode)
                {
                    rangeNode.Multiply = rangeNode.Multiply ?? 1;

                    for (var j = 0; j < rangeNode.Multiply; j++)
                    {
                        if (rangeNode.Start < rangeNode.Stop)
                        {
                            for (var i = rangeNode.Start; i <= rangeNode.Stop; i += rangeNode.Step ?? 1)
                            {
                                result.Add(new WordParameter($"{suffixNode.Name}<{i}>"));
                            }
                        }

                        if (rangeNode.Start > rangeNode.Stop)
                        {
                            for (var i = rangeNode.Start; i >= rangeNode.Stop; i -= rangeNode.Step ?? 1)
                            {
                                result.Add(new WordParameter($"{suffixNode.Name}<{i}>"));
                            }
                        }

                        if (rangeNode.Start == rangeNode.Stop)
                        {
                            result.Add(new WordParameter($"{suffixNode.Name}<{rangeNode.Start}>"));
                        }
                    }
                }
            }
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
