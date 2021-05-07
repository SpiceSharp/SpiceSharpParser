using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Configurations
{
    public class TransientConfiguration
    {
        public double? MaxStep { get; set; }

        public double? Step { get; set; }

        public double? Final { get; set; }

        public double? Start { get; set; }

        public bool? UseIc { get; set; }

        public int? TranMaxIterations { get; set; }

        public Type Type { get; internal set; }
    }
}
