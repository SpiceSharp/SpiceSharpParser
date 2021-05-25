using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice;
using System;
using System.Text;
using SpiceSharpParser.Common;

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
            var reader = new SpiceNetlistReader(Settings);
            return reader.Read(model);
        }
    }
}
