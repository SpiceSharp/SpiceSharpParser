using SpiceSharp.Simulations;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters
{
    public class InputNoiseExport : Export
    {
        public InputNoiseExport(ISimulationWithEvents simulation)
            : base(simulation)
        {
            ExportImpl = new InputNoiseDensityExport((Noise)simulation);
            Name = "Input noise density";
        }

        public override string QuantityUnit { get; } = "Density";

        public InputNoiseDensityExport ExportImpl { get; }

        public override double Extract()
        {
            return ExportImpl.Value;
        }
    }
}
