using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Prints
{
    /// <summary>
    /// Represents a printed data that has columns and rows.
    /// </summary>
    public class Print
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Print"/> class.
        /// </summary>
        /// <param name="name">The name of print.</param>
        public Print(string name)
        {
            Name = name;
            Rows = new List<Row>();
            ColumnNames = new List<string>();
        }

        /// <summary>
        /// Gets the name of the print.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the rows.
        /// </summary>
        public List<Row> Rows { get; }

        /// <summary>
        /// Gets the columns names.
        /// </summary>
        public List<string> ColumnNames { get; }

        /// <summary>
        /// Convert print data to raw data.
        /// </summary>
        public Tuple<string, IEnumerable<string>, IEnumerable<double[]>> ToRaw()
        {
            return new Tuple<string, IEnumerable<string>, IEnumerable<double[]>>(
                Name,
                ColumnNames,
                Rows.Select(row => row.Columns.ToArray()));
        }
    }
}