using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls.Prints;

namespace SpiceSharpParser.Common.Writers
{
    public static class CsvPrintWriter
    {
        public static void Write(this Print print, string path)
        {
            var raw = print.ToRaw();
            CsvWriter.Write(path, raw.Item2, raw.Item3, ";");
        }
    }
}
