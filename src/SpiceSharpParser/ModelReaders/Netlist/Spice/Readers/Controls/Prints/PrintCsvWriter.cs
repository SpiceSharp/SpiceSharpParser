using SpiceSharpParser.Common.FileSystem;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Prints
{
    public static class PrintCsvWriter
    {
        public static void ToCsv(this Print print, string path, string columnsSeparator, string decimalSeparator = null, bool? addCsvHeader = null)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (columnsSeparator == null)
            {
                throw new ArgumentNullException(nameof(columnsSeparator));
            }

            var raw = print.ToRaw();
            CsvFileWriter.Write(path, raw.Item2, raw.Item3, columnsSeparator, decimalSeparator, addCsvHeader ?? false);
        }
    }
}