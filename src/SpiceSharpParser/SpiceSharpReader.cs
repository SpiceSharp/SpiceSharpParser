using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice;
using System;
using System.Text;
using SpiceSharp;
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
            try
            {
                var reader = new SpiceNetlistReader(Settings);
                return reader.Read(model);
            }
            catch (Exception ex)
            {
                throw new SpiceSharpException("Unhandled exception during reading model", ex);
            }
            
        }
    }
}
