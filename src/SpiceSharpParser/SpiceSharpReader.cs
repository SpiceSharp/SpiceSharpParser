using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice;
using System;
using System.Text;

namespace SpiceSharpParser
{
    public class SpiceSharpReader
    {
        public SpiceSharpReader(SpiceNetlistReaderSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public SpiceSharpReader()
        {
            Settings = new SpiceNetlistReaderSettings(new SpiceNetlistCaseSensitivitySettings(), () => Environment.CurrentDirectory, Encoding.Default);
        }

        public SpiceNetlistReaderSettings Settings { get; }

        public SpiceSharpModel Read(SpiceNetlist model)
        {
            return ReadResult(model).PartialModel;
        }

        /// <summary>
        /// Translates a parsed netlist and returns structured diagnostics and the partial model.
        /// </summary>
        /// <param name="model">The parsed netlist.</param>
        /// <returns>A structured translation result.</returns>
        public SpiceNetlistReadResult ReadResult(SpiceNetlist model)
        {
            var reader = new SpiceNetlistReader(Settings);
            return reader.ReadResult(model);
        }
    }
}
