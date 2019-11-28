using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SpiceSharpParser.Common.FileSystem
{
    /// <summary>
    /// Creates CSV files.
    /// </summary>
    public static class CsvFileWriter
    {
        /// <summary>
        /// Writes a CSV file into a file system.
        /// </summary>
        /// <param name="path">Path of file to create.</param>
        /// <param name="columns">Columns of csv.</param>
        /// <param name="rows">Rows of csv.</param>
        /// <param name="columnsSeparator">Columns separator.</param>
        /// <param name="decimalSeparator">Decimal separator.</param>
        /// <param name="addCsvHeader">Specifies whether to add CSV header with separator value.</param>
        public static void Write(string path, IEnumerable<string> columns, IEnumerable<double[]> rows, string columnsSeparator, string decimalSeparator = null, bool addCsvHeader = false)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (rows == null)
            {
                throw new ArgumentNullException(nameof(rows));
            }

            if (columnsSeparator == null)
            {
                throw new ArgumentNullException(nameof(columnsSeparator));
            }

            string currentCultureDecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            var separators = GetColumnsAndDecimalSeparatorToUse(columnsSeparator, currentCultureDecimalSeparator, decimalSeparator);
            string columnsSeparatorToUse = separators.Item1;
            string decimalSeparatorToUse = separators.Item2;

            string columnsLine = string.Join(columnsSeparatorToUse, columns.ToArray());
            using (var writer = new StreamWriter(new FileStream(path, FileMode.Create)))
            {
                if (addCsvHeader)
                {
                    writer.WriteLine("sep=" + columnsSeparatorToUse);
                }

                writer.WriteLine(columnsLine);

                foreach (var row in rows)
                {
                    string rowLine = string.Join(columnsSeparatorToUse, row.Select(value => CreateDoubleString(value, decimalSeparatorToUse)));
                    writer.WriteLine(rowLine);
                }

                writer.Flush();
            }
        }

        /// <summary>
        /// Gets a column and decimal separator to use.
        /// </summary>
        /// <param name="columnsSeparator">Columns separator to try.</param>
        /// <param name="currentCultureDecimalSeparator">Current culture decimal separator.</param>
        /// <param name="decimalSeparator">Decimal separator to try.</param>
        /// <returns>
        /// First item from tuple is columns separator, second is decimal separator.
        /// </returns>
        private static Tuple<string, string> GetColumnsAndDecimalSeparatorToUse(string columnsSeparator, string currentCultureDecimalSeparator, string decimalSeparator)
        {
            if (decimalSeparator != null && decimalSeparator != "," && decimalSeparator != ".")
            {
                throw new SpiceSharpParserException($"Unsupported decimal separator: {decimalSeparator}");
            }

            if (decimalSeparator == null)
            {
                if (currentCultureDecimalSeparator == columnsSeparator)
                {
                    if (currentCultureDecimalSeparator == ".")
                    {
                        return new Tuple<string, string>(columnsSeparator, ",");
                    }
                    else
                    {
                        return new Tuple<string, string>(columnsSeparator, ".");
                    }
                }
                else
                {
                    return new Tuple<string, string>(columnsSeparator, currentCultureDecimalSeparator);
                }
            }
            else
            {
                if (decimalSeparator == columnsSeparator)
                {
                    if (decimalSeparator == ".")
                    {
                        return new Tuple<string, string>(columnsSeparator, ",");
                    }
                    else
                    {
                        return new Tuple<string, string>(columnsSeparator, ".");
                    }
                }
                else
                {
                    return new Tuple<string, string>(columnsSeparator, decimalSeparator);
                }
            }
        }

        private static string CreateDoubleString(double value, string decimalSeparator)
        {
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = decimalSeparator;
            return value.ToString(nfi);
        }
    }
}