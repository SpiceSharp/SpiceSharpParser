using SpiceSharpBehavioral.Parsers.Nodes;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Mathematics.Combinatorics;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.ResolverFunctions
{
    public class PolyResolverFunction : DynamicResolverFunction
    {
        public PolyResolverFunction()
        {
            Name = "poly";
        }

        public override Node GetBody(Node[] argumentValues)
        {
            if (argumentValues[0].NodeType != NodeTypes.Constant)
            {
                throw new SpiceSharpParserException("POLY first parameter should be constant");
            }

            int dimension = (int)((ConstantNode)argumentValues[0]).Literal;

            if (argumentValues.Length < dimension + 1)
            {
                throw new SpiceSharpParserException("Too less variables for POLY");
            }

            List<Node> variables = new List<Node>();

            for (var i = 1; i <= dimension; i++)
            {
                variables.Add(argumentValues[i]);
            }

            List<Node> coefficients = new List<Node>();

            for (var i = dimension + 1; i < argumentValues.Length; i++)
            {
                coefficients.Add(argumentValues[i]);
            }

            if (coefficients.Count == 0)
            {
                return 0;
            }

            return GetExpression(coefficients, dimension, variables);
        }

        private static Node GetExpression(List<Node> coefficients, int dimension, List<Node> variables)
        {
            var combinations = CombinationCache.GetCombinations(coefficients.Count, dimension);
            Node sum = Node.Zero;
            sum += coefficients[0];

            for (int i = 1; i < combinations.Count; i++)
            {
                sum += ComputeSumElementValue(variables, coefficients[i], combinations[i]);
            }

            return sum;
        }

        private static Node ComputeSumElementValue(List<Node> variables, Node coefficient, int[] combination)
        {
            Node result = 1.0;

            for (int i = 0; i < combination.Length; i++)
            {
                result *= variables[combination[i] - 1];
            }

            return result * coefficient;
        }
    }
}
