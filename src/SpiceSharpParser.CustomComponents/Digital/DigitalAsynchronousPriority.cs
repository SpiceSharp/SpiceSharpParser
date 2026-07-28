namespace SpiceSharpParser.CustomComponents.Digital
{
    /// <summary>
    /// Selects the deterministic result when asynchronous preset and clear are
    /// asserted at the same time on a sequential digital component.
    /// </summary>
    public enum DigitalAsynchronousPriority
    {
        /// <summary>Clear wins. This is the built-in default.</summary>
        Clear,

        /// <summary>Preset wins.</summary>
        Preset,
    }
}
