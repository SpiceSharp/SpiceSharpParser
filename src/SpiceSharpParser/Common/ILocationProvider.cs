using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharpParser.Common
{
    public interface ILocationProvider
    {
        /// <summary>
        /// Gets token line number.
        /// </summary>
        int LineNumber { get; }

        /// <summary>
        /// Gets start column index.
        /// </summary>
        int StartColumnIndex { get; }

        /// <summary>
        /// Gets end column index.
        /// </summary>
        int EndColumnIndex { get; }

        /// <summary>
        /// Gets token file name.
        /// </summary>
        string FileName { get; }
    }
}
