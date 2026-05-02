namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources
{
    internal sealed class LaplaceSourceInput
    {
        public LaplaceSourceInput(string controlPositiveNode, string controlNegativeNode)
        {
            ControlPositiveNode = controlPositiveNode;
            ControlNegativeNode = controlNegativeNode;
        }

        public string ControlPositiveNode { get; }

        public string ControlNegativeNode { get; }
    }
}
