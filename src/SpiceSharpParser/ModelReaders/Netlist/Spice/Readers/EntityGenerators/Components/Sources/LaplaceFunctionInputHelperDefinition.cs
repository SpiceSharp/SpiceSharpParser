using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources
{
    internal sealed class LaplaceFunctionInputHelperDefinition
    {
        public LaplaceFunctionInputHelperDefinition(
            string helperNodeName,
            string sourceName,
            string expression,
            SpiceLineInfo lineInfo)
        {
            HelperNodeName = helperNodeName;
            SourceName = sourceName;
            Expression = expression;
            LineInfo = lineInfo;
        }

        public string HelperNodeName { get; }

        public string SourceName { get; }

        public string Expression { get; }

        public SpiceLineInfo LineInfo { get; }
    }
}
