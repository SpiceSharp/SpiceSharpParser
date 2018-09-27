namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public class MonteCarloConfiguration
    {
        public bool Enabled { get; set; } = false;

        public int? Runs { get; set; }

        public string SimulationType { get; set; }

        public string OutputVariable { get; set; }

        public string Function { get; set; }

        public int? Bins { get; set; }

        public int? RandomSeed { get; set; }
    }
}
