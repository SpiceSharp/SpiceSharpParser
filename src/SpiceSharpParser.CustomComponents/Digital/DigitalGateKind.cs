namespace SpiceSharpParser.CustomComponents.Digital
{
    /// <summary>
    /// Identifies a built-in combinational digital gate subcircuit.
    /// </summary>
    public enum DigitalGateKind
    {
        /// <summary>A non-inverting one-input buffer.</summary>
        Buffer,

        /// <summary>An inverting one-input gate.</summary>
        Inverter,

        /// <summary>A two-input AND gate.</summary>
        And2,

        /// <summary>A two-input NAND gate.</summary>
        Nand2,

        /// <summary>A two-input OR gate.</summary>
        Or2,

        /// <summary>A two-input NOR gate.</summary>
        Nor2,

        /// <summary>A two-input exclusive-OR gate.</summary>
        Xor2,

        /// <summary>A two-input exclusive-NOR gate.</summary>
        Xnor2,
    }
}
