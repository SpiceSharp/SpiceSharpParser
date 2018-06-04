using System.Collections.Generic;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls.Prints
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
        public string Name { get;  }

        /// <summary>
        /// Gets the rows.
        /// </summary>
        public List<Row> Rows { get; }

        /// <summary>
        /// Gets the columns names.
        /// </summary>
        public List<string> ColumnNames { get; }
    }
}
