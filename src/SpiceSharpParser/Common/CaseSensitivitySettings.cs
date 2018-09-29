namespace SpiceSharpParser.ModelReaders.Netlist.Spice
{
    public class CaseSensitivitySettings
    {
        public bool IgnoreCaseForComponents { get; set; } = true;

        public bool IgnoreCaseForNodes { get; set; } = true;

        public bool IgnoreCaseForDotStatements { get; set; } = true;

        public bool IgnoreCaseForFunctions { get; set; } = true;
    }
}
