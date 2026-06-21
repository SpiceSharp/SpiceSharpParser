using System;
using SpiceSharpParser.ModelReaders.Netlist.Spice;

namespace SpiceSharpParser.CustomComponents
{
    /// <summary>
    /// Extensions for enabling custom component parsing.
    /// </summary>
    public static class CustomComponentReaderExtensions
    {
        /// <summary>
        /// Enables parser mappings for the custom components in this assembly.
        /// </summary>
        /// <param name="settings">The reader settings.</param>
        /// <returns>The same settings instance for fluent configuration.</returns>
        public static SpiceNetlistReaderSettings UseCustomComponents(this SpiceNetlistReaderSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            settings.Mappings.Components.Map("C", new NonlinearPassiveGenerator());
            settings.Mappings.Components.Map("L", new NonlinearPassiveGenerator());
            settings.Mappings.Models.Map("D", new IdealDiodeModelGenerator());
            settings.Mappings.Components.Map("D", new IdealDiodeGenerator());
            return settings;
        }
    }
}
