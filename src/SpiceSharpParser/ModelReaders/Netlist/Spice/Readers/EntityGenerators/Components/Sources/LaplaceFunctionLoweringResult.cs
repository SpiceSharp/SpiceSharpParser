using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources
{
    internal sealed class LaplaceFunctionLoweringResult
    {
        private LaplaceFunctionLoweringResult(
            bool isHandled,
            bool hasErrors,
            IReadOnlyList<LaplaceFunctionInputHelperDefinition> inputHelperDefinitions,
            LaplaceSourceDefinition directDefinition,
            IReadOnlyList<LaplaceFunctionCallDefinition> helperDefinitions,
            string rewrittenExpression)
        {
            IsHandled = isHandled;
            HasErrors = hasErrors;
            InputHelperDefinitions = inputHelperDefinitions ?? new List<LaplaceFunctionInputHelperDefinition>();
            DirectDefinition = directDefinition;
            HelperDefinitions = helperDefinitions ?? new List<LaplaceFunctionCallDefinition>();
            RewrittenExpression = rewrittenExpression;
        }

        public bool IsHandled { get; }

        public bool HasErrors { get; }

        public bool IsDirect => DirectDefinition != null;

        public IReadOnlyList<LaplaceFunctionInputHelperDefinition> InputHelperDefinitions { get; }

        public LaplaceSourceDefinition DirectDefinition { get; }

        public IReadOnlyList<LaplaceFunctionCallDefinition> HelperDefinitions { get; }

        public string RewrittenExpression { get; }

        public static LaplaceFunctionLoweringResult NoMatch()
        {
            return new LaplaceFunctionLoweringResult(false, false, null, null, null, null);
        }

        public static LaplaceFunctionLoweringResult Error()
        {
            return new LaplaceFunctionLoweringResult(true, true, null, null, null, null);
        }

        public static LaplaceFunctionLoweringResult Direct(
            IReadOnlyList<LaplaceFunctionInputHelperDefinition> inputHelperDefinitions,
            LaplaceSourceDefinition definition)
        {
            return new LaplaceFunctionLoweringResult(true, false, inputHelperDefinitions, definition, null, null);
        }

        public static LaplaceFunctionLoweringResult Mixed(
            IReadOnlyList<LaplaceFunctionInputHelperDefinition> inputHelperDefinitions,
            IReadOnlyList<LaplaceFunctionCallDefinition> helperDefinitions,
            string rewrittenExpression)
        {
            return new LaplaceFunctionLoweringResult(true, false, inputHelperDefinitions, null, helperDefinitions, rewrittenExpression);
        }
    }
}
