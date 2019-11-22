namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters
{
    public class ZeroExport : Export
    {
        public ZeroExport()
            : base(null)
        {
        }

        public override string QuantityUnit { get; }

        public override double Extract()
        {
            return 0;
        }
    }
}