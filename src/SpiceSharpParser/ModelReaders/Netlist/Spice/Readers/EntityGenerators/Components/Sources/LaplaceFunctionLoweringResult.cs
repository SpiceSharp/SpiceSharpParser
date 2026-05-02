using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources
{
    internal sealed class LaplaceFunctionLoweringResult
    {
        private LaplaceFunctionLoweringResult(
            bool isHandled,
            bool hasErrors,
            LaplaceSourceDefinition directDefinition,
            IReadOnlyList<LaplaceFunctionCallDefinition> helperDefinitions,
            string rewrittenExpression)
        {
            IsHandled = isHandled;
            HasErrors = hasErrors;
            DirectDefinition = directDefinition;
            HelperDefinitions = helperDefinitions ?? new List<LaplaceFunctionCallDefinition>();
            RewrittenExpression = rewrittenExpression;
        }

        public bool IsHandled { get; }

        public bool HasErrors { get; }

        public bool IsDirect => DirectDefinition != null;

        public LaplaceSourceDefinition DirectDefinition { get; }

        public IReadOnlyList<LaplaceFunctionCallDefinition> HelperDefinitions { get; }

        public string RewrittenExpression { get; }

        public static LaplaceFunctionLoweringResult NoMatch()
        {
            return new LaplaceFunctionLoweringResult(false, false, null, null, null);
        }

        public static LaplaceFunctionLoweringResult Error()
        {
            return new LaplaceFunctionLoweringResult(true, true, null, null, null);
        }

        public static LaplaceFunctionLoweringResult Direct(LaplaceSourceDefinition definition)
        {
            return new LaplaceFunctionLoweringResult(true, false, definition, null, null);
        }

        public static LaplaceFunctionLoweringResult Mixed(
            IReadOnlyList<LaplaceFunctionCallDefinition> helperDefinitions,
            string rewrittenExpression)
        {
            return new LaplaceFunctionLoweringResult(true, false, null, helperDefinitions, rewrittenExpression);
        }
    }
}
