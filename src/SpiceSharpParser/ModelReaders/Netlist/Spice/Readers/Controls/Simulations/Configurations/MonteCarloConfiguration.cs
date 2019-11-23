using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Configurations
{
    public class MonteCarloConfiguration
    {
        public bool Enabled { get; set; } = false;

        public int? Runs { get; set; }

        public string SimulationType { get; set; }

        public Parameter OutputVariable { get; set; }

        public string Function { get; set; }

        public int? Seed { get; set; }
    }
}