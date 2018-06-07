using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
        /// <param name="path">Path of file to create</param>
        /// <param name="columns">Columns of csv</param>
        /// <param name="rows">Rows of csv</param>
        public static void Write(string path, IEnumerable<string> columns, IEnumerable<double[]> rows, string separator)
        {
            string columnsLine = string.Join(separator, columns.ToArray());

            using (var writer = new StreamWriter(new FileStream(path, FileMode.Create)))
            {
                writer.WriteLine(columnsLine);

                foreach (var row in rows)
                {
                    string rowLine = string.Join(separator, row.ToArray());
                    writer.WriteLine(rowLine);
                }

                writer.Flush();
            }
        }
    }
}
