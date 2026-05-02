namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources
{
    internal sealed class LaplaceSourceInput
    {
        private LaplaceSourceInput(
            LaplaceSourceInputKind kind,
            string controlPositiveNode,
            string controlNegativeNode,
            string controllingSource)
        {
            Kind = kind;
            ControlPositiveNode = controlPositiveNode;
            ControlNegativeNode = controlNegativeNode;
            ControllingSource = controllingSource;
        }

        public LaplaceSourceInputKind Kind { get; }

        public string ControlPositiveNode { get; }

        public string ControlNegativeNode { get; }

        public string ControllingSource { get; }

        public static LaplaceSourceInput Voltage(string controlPositiveNode, string controlNegativeNode)
        {
            return new LaplaceSourceInput(
                LaplaceSourceInputKind.Voltage,
                controlPositiveNode,
                controlNegativeNode,
                null);
        }

        public static LaplaceSourceInput Current(string controllingSource)
        {
            return new LaplaceSourceInput(
                LaplaceSourceInputKind.Current,
                null,
                null,
                controllingSource);
        }
    }

    internal enum LaplaceSourceInputKind
    {
        Voltage,
        Current,
    }
}
