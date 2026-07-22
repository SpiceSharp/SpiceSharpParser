using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.Diagnostics;

namespace SpiceSharpParser
{
    /// <summary>
    /// Represents a source or translation failure while loading or instantiating a subcircuit library.
    /// </summary>
    public sealed class SpiceSubcircuitLibraryException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceSubcircuitLibraryException"/> class.
        /// </summary>
        /// <param name="message">A description of the failed operation.</param>
        /// <param name="diagnostics">Structured diagnostics associated with the failure.</param>
        public SpiceSubcircuitLibraryException(
            string message,
            IEnumerable<SpiceDiagnostic> diagnostics)
            : base(message)
        {
            Diagnostics = (diagnostics ?? Enumerable.Empty<SpiceDiagnostic>()).ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets the structured diagnostics associated with the failure.
        /// </summary>
        public IReadOnlyList<SpiceDiagnostic> Diagnostics { get; }
    }
}
