namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources
{
    internal sealed class LaplaceFunctionCallDefinition
    {
        public LaplaceFunctionCallDefinition(
            string helperNodeName,
            LaplaceSourceDefinition definition)
        {
            HelperNodeName = helperNodeName;
            Definition = definition;
        }

        public string HelperNodeName { get; }

        public LaplaceSourceDefinition Definition { get; }
    }
}
