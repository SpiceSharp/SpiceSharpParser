using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Prints
{
    /// <summary>
    /// Represent print row.
    /// </summary>
    public class Row
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Row"/> class.
        /// </summary>
        /// <param name="rowIndex">row index</param>
        public Row(int rowIndex)
        {
            Columns = new List<double>();
            RowIndex = rowIndex;
        }

        /// <summary>
        /// Gets the row columns.
        /// </summary>
        public List<double> Columns { get; }

        /// <summary>
        /// Gets the row index.
        /// </summary>
        public int RowIndex { get; }
    }
}