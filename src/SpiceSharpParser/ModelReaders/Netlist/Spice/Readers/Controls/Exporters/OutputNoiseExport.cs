using SpiceSharp.Simulations;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters
{
    public class OutputNoiseExport : Export
    {
        public OutputNoiseExport(ISimulationWithEvents simulation)
            : base(simulation)
        {
            ExportImpl = new OutputNoiseDensityExport((Noise)simulation);
            Name = "Output noise density";
        }

        public override string QuantityUnit { get; } = "Density";

        public OutputNoiseDensityExport ExportImpl { get; }

        public override double Extract()
        {
            return ExportImpl.Value;
        }
    }
}
