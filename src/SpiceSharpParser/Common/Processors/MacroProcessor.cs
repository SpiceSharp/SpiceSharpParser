using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.Lexers.BusSuffix;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.Parsers.BusSuffix;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.Common.Processors
{
    public class MacroProcessor : IProcessor
    {
        public ValidationEntryCollection Validation { get; set; }

        public Statements Process(Statements statements)
        {
            Statements result = new Statements();
            foreach (var statement in statements)
            {
                foreach (var resolved in Resolve(statement))
                {
                    result.Add(resolved);
                }
            }

            return result;
        }

        private List<Statement> Resolve(Statement statement)
        {
            if (statement is Component component)
            {
                if (component.NameParameter is SuffixParameter nameSuffixParameter)
                {
                    var lexer = new Lexer(nameSuffixParameter.Value);
                    var parser = new Parser();
                    var suffixNode = parser.Parse(lexer);
                    var dimensionsCount = suffixNode.Dimensions.Count;
                    var totalNumberOfComponents = CalculateTotal(suffixNode.Dimensions);
                    var suffixes = GetSuffixes(suffixNode.Dimensions);

                    var result = new List<Statement>();
                    for (var i = 0; i < totalNumberOfComponents; i++)
                    {
                        var clone = component.Clone() as Component;
                        clone.Name = suffixNode.Name + suffixes[i];
                        clone.PinsAndParameters = ResolveParameters(component.PinsAndParameters, i, totalNumberOfComponents, dimensionsCount);

                        result.Add(clone);
                    }

                    return result;
                }
                else
                {
                    var clone = statement.Clone() as Component;
                    clone.PinsAndParameters.Clear();
                    clone.PinsAndParameters = ResolveParameters(component.PinsAndParameters);
                    return new List<Statement>() { clone };
                }
            }

            if (statement is SubCircuit subCircuit)
            {
                var clone = statement.Clone() as SubCircuit;
                clone.Pins = ResolveParameters(subCircuit.Pins);

                var statementsClone = clone.Statements.Clone();
                clone.Statements.Clear();

                foreach (var subCircuitStatement in statementsClone as Statements)
                {
                    foreach (var resolved in Resolve(subCircuitStatement))
                    {
                        clone.Statements.Add(resolved);
                    }
                }

                return new List<Statement>() { clone };
            }

            return new List<Statement>() { statement };
        }

        private List<string> GetSuffixes(List<SuffixDimension> dimensions)
        {
            var result = new List<string>();

            for (var dimensionIndex = dimensions.Count - 1; dimensionIndex >= 0; dimensionIndex--)
            {
                var dimension = dimensions[dimensionIndex];
                if (dimensionIndex != 0)
                {
                    var newResult = new List<string>();

                    foreach (var resultItem in result)
                    {
                        newResult.AddRange(GetSuffixes(dimension, resultItem));
                    }

                    result = newResult;
                }
                else
                {
                    result.AddRange(GetSuffixes(dimension, string.Empty));
                }
            }

            return result;
        }

        private List<string> GetSuffixes(SuffixDimension dimension, string suffix)
        {
            var result = new List<string>();

            foreach (var node in dimension.Nodes)
            {
                if (node is NumberNode numberNode)
                {
                    result.Add($"<{numberNode.Node}>{suffix}");
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
                                result.Add($"<{i}>{suffix}");
                            }
                        }

                        if (rangeNode.Start > rangeNode.Stop)
                        {
                            for (var i = rangeNode.Start; i >= rangeNode.Stop; i -= rangeNode.Step ?? 1)
                            {
                                result.Add($"<{i}>{suffix}");
                            }
                        }

                        if (rangeNode.Start == rangeNode.Stop)
                        {
                            result.Add($"<{rangeNode.Start}>{suffix}");
                        }
                    }
                }
            }

            return result;
        }

        private int CalculateTotal(List<SuffixDimension> dimensions)
        {
            int result = 1;

            for (var i = 0; i < dimensions.Count; i++)
            {
                var dimensionCount = 0;

                foreach (var node in dimensions[0].Nodes)
                {
                    if (node is RangeNode range)
                    {
                        dimensionCount += ((System.Math.Abs(range.Start - range.Stop) + 1) / (range.Step ?? 1)) * (range.Multiply ?? 1);
                    }
                    else
                    {
                        dimensionCount += 1;
                    }
                }

                result *= dimensionCount;
            }

            return result;
        }

        private ParameterCollection ResolveParameters(ParameterCollection pinsAndParameters, int? componentIndex = null, int? total = null, int? componentDimensions = null)
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
                    ResolveSuffix(suffix, result, componentIndex, total, componentDimensions);
                }
                else
                {
                    result.Add(param);
                }
            }

            return result;
        }

        private void ResolveSuffix(SuffixParameter suffix, ParameterCollection result, int? componentIndex = null, int? total = null, int? componentDimensions = null)
        {
            var lexer = new Lexer(suffix.Value);
            var parser = new Parser();
            var suffixNode = parser.Parse(lexer);

            if (componentIndex == null && total == null && componentDimensions == null)
            {
                var suffixes = GetSuffixes(suffixNode.Dimensions);

                foreach (var suffixEntry in suffixes)
                {
                    result.Add(new WordParameter(suffixEntry));
                }
            }
            else
            {
                if (suffixNode.Dimensions.Count == componentDimensions)
                {
                    // case 1 => return one parameter
                    var totalNumberOfNodes = CalculateTotal(suffixNode.Dimensions);

                    if (totalNumberOfNodes != total)
                    {
                        throw new System.Exception("Wrong syntax for bus nodes. Mismatch.");
                    }

                    var allSuffixes = GetSuffixes(suffixNode.Dimensions);
                    result.Add(new WordParameter(allSuffixes[componentIndex.Value]));
                }
                else
                {
                    if (suffixNode.Dimensions.Count > componentDimensions)
                    {
                        // case 2:
                        // X<1:2><4:2>  input<4:5><6:8><1:10><3:50> my_subckt
                        // X<1><4>  input<4><6><1:10><3:50> => X<1><4> input<4><6><1><3>
                        var allPrefixes = GetSuffixes(suffixNode.Dimensions.Take(componentDimensions.Value).ToList());
                        var prefix = allPrefixes[componentIndex.Value];

                        var suffixesPrefixes = GetSuffixes(suffixNode.Dimensions.Skip(componentDimensions.Value).ToList());

                        foreach (var suffixPrefix in suffixesPrefixes)
                        {
                            result.Add(new WordParameter(prefix + suffixPrefix));
                        }
                    }
                    else
                    {
                        // case 3:
                        // X<1:2><4:3><5:6>  input<1:2><1:4> my_subckt
                        var totalNumberOfNodes = CalculateTotal(suffixNode.Dimensions);
                        if (totalNumberOfNodes != total)
                        {
                            throw new System.Exception("Wrong syntax for bus nodes. Mismatch.");
                        }

                        var allSuffixes = GetSuffixes(suffixNode.Dimensions);
                        result.Add(new WordParameter(allSuffixes[componentIndex.Value]));
                    }
                }
            }
        }

        private void ResolvePrefix(PrefixParameter prefix, ParameterCollection result)
        {
            var lexer = new Lexers.BusPrefix.Lexer(prefix.Value);
            var parser = new Parsers.BusPrefix.Parser();
            var node = parser.Parse(lexer);

            ResolvePrefix(node, result);
        }

        private void ResolvePrefix(Parsers.BusPrefix.Node node, ParameterCollection result)
        {
            if (node is Parsers.BusPrefix.PrefixNodeName prefixNode)
            {
                result.Add(new WordParameter(prefixNode.Name));
            }
            else if (node is Parsers.BusPrefix.Prefix prefix)
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
