using SpiceSharp.Simulations;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters
{
    public class OutputNoiseExport : Export
    {
        public OutputNoiseExport(Simulation simulation)
            : base(simulation)
        {
            ExportImpl = new OutputNoiseDensityExport((Noise)simulation);
            Name = "Output noise density";
        }

        public override string QuantityUnit { get; } = "Density";

        public OutputNoiseDensityExport ExportImpl { get; }

        public override double Extract()
        {
            if (!ExportImpl.IsValid)
            {
                if (ExceptionsEnabled)
                {
                    throw new SpiceSharpParserException($"Output noise density export {Name} is invalid");
                }

                return double.NaN;
            }

            return ExportImpl.Value;
        }
    }
}
