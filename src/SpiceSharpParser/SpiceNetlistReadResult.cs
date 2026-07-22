using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.Diagnostics;
using SpiceSharpParser.ModelReaders.Netlist.Spice;

namespace SpiceSharpParser
{
    /// <summary>
    /// Contains the outcome of translating a parsed SPICE netlist.
    /// </summary>
    public sealed class SpiceNetlistReadResult
    {
        internal SpiceNetlistReadResult(SpiceSharpModel partialModel)
        {
            PartialModel = partialModel ?? throw new System.ArgumentNullException(nameof(partialModel));
            Diagnostics = partialModel.ValidationResult.ToDiagnostics();
            Model = Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                ? null
                : partialModel;
        }

        /// <summary>
        /// Gets a value indicating whether translation produced a simulation-ready model.
        /// </summary>
        public bool Success => Model != null;

        /// <summary>
        /// Gets the translated model only when no error diagnostics were produced; otherwise, null.
        /// </summary>
        public SpiceSharpModel Model { get; }

        /// <summary>
        /// Gets the partially translated model for inspection, including when translation reported errors.
        /// Do not simulate this model unless <see cref="Success"/> is true.
        /// </summary>
        public SpiceSharpModel PartialModel { get; }

        /// <summary>
        /// Gets immutable structured diagnostics produced by the reader.
        /// </summary>
        public IReadOnlyList<SpiceDiagnostic> Diagnostics { get; }
    }
}
