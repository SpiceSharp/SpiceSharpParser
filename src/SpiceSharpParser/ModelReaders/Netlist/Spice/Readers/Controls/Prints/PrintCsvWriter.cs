using System;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls.Prints;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Prints
{
    public static class PrintCsvWriter
    {
        public static void ToCsv(this Print print, string path, string columnsSeperator, string decimalSeparator = null, bool? addCsvHeader = null)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (columnsSeperator == null)
            {
                throw new ArgumentNullException(nameof(columnsSeperator));
            }

            var raw = print.ToRaw();

            Common.Writers.CsvWriter.Write(path, raw.Item2, raw.Item3, columnsSeperator, decimalSeparator, addCsvHeader ?? false);
        }
    }
}
