using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SpiceSharpParser.Common.Writers
{
    /// <summary>
    /// Creates CSV files.
    /// </summary>
    public class CsvWriter
    {
        /// <summary>
        /// Creates a csv file.
        /// </summary>
        /// <param name="path">Path of file to create.</param>
        /// <param name="columns">Columns of csv.</param>
        /// <param name="rows">Rows of csv.</param>
        /// <param name="columnsSeperator">Columns separator.</param>
        /// <param name="decimalSeprator">Decimal seperator.</param>
        /// <param name="addCsvHeader">Specifies whether to add CSV header with seperator value.</param>
        public static void Write(string path, IEnumerable<string> columns, IEnumerable<double[]> rows, string columnsSeperator, string decimalSeprator = null, bool addCsvHeader = false)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (rows == null)
            {
                throw new ArgumentNullException(nameof(rows));
            }

            if (columnsSeperator == null)
            {
                throw new ArgumentNullException(nameof(columnsSeperator));
            }

            string currentCultureDecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            var separators = GetColumnsAndDecimalSeparatorToUse(columnsSeperator, currentCultureDecimalSeparator, decimalSeprator);
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
                    string rowLine = string.Join(columnsSeparatorToUse, row.Select(value => CreateDoubleString(value, separators.Item2)));
                    writer.WriteLine(rowLine);
                }

                writer.Flush();
            }
        }
        
        /// <returns>
        /// First item from tule is columns separator, second is decimal separator
        /// </returns>
        private static Tuple<string, string> GetColumnsAndDecimalSeparatorToUse(string columnsSeperator, string currentCultureDecimalSeparator, string decimalSeprator)
        {
            if (decimalSeprator != null && decimalSeprator != "," && decimalSeprator != ".")
            {
                throw new Exception("Unsupported decimal separator: " + decimalSeprator);
            }

            if (decimalSeprator == null)
            {
                if (currentCultureDecimalSeparator == columnsSeperator)
                {
                    if (currentCultureDecimalSeparator == ".")
                    {
                        return new Tuple<string, string>(columnsSeperator, ",");
                    }
                    else
                    {
                        return new Tuple<string, string>(columnsSeperator, ".");
                    }
                }
                else
                {
                    return new Tuple<string, string>(columnsSeperator, currentCultureDecimalSeparator);
                }
            }
            else
            {
                if (decimalSeprator == columnsSeperator)
                {
                    if (decimalSeprator == ".")
                    {
                        return new Tuple<string, string>(columnsSeperator, ",");
                    }
                    else
                    {
                        return new Tuple<string, string>(columnsSeperator, ".");
                    }
                }
                else
                {
                    return new Tuple<string, string>(columnsSeperator, decimalSeprator);
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
