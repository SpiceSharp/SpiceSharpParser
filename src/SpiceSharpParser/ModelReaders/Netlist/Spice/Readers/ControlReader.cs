using System;
using System.Collections.Generic;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers
{
    /// <summary>
    /// Reads all supported <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class ControlReader : StatementReader<Control>, IControlReader
    {
        private static readonly Dictionary<string, string> UnsupportedLtspiceControls = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "BACKANNO", "generated annotation metadata is not supported yet" },
            { "TF", "transfer-function analysis is not supported yet" },
            { "FOUR", "post-transient Fourier reporting is not supported yet" },
            { "NET", "network-parameter post-processing is not supported yet" },
            { "FERRET", "external file download directives are intentionally unsupported" },
            { "LOADBIAS", "solver-state loading is not supported yet" },
            { "SAVEBIAS", "solver-state saving is not supported yet" },
            { "MACHINE", "LTspice state-machine blocks are not supported yet" },
            { "ENDMACHINE", "LTspice state-machine blocks are not supported yet" },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlReader"/> class.
        /// </summary>
        /// <param name="mapper">The base control mapper.</param>
        public ControlReader(IMapper<BaseControl> mapper)
        {
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Gets the base control mapper.
        /// </summary>
        public IMapper<BaseControl> Mapper { get; }

        /// <summary>
        /// Reads a control statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public override void Read(Control statement, IReadingContext context)
        {
            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            string type = statement.Name;

            if (!Mapper.TryGetValue(type, context.ReaderSettings.CaseSensitivity.IsDotStatementNameCaseSensitive, out var controlReader))
            {
                AddUnsupportedControlError(statement, context);
            }
            else
            {
                controlReader.Read(statement, context);
            }
        }

        private static void AddUnsupportedControlError(Control statement, IReadingContext context)
        {
            if (statement.Name.Equals("BACKANNO", StringComparison.OrdinalIgnoreCase)
                && context.ReaderSettings.Compatibility.IsLTspice)
            {
                context.Result.ValidationResult.AddWarning(
                    ValidationEntrySource.Reader,
                    "Ignored LTspice control '.backanno': generated annotation metadata is not used by SpiceSharpParser.",
                    statement.LineInfo);
                return;
            }

            if (UnsupportedLtspiceControls.TryGetValue(statement.Name, out var reason))
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    $"Unsupported LTspice control '.{statement.Name.ToLowerInvariant()}': {reason}.",
                    statement.LineInfo);
                return;
            }

            context.Result.ValidationResult.AddError(
                ValidationEntrySource.Reader,
                $"Unsupported control: {statement.Name}",
                statement.LineInfo);
        }
    }
}
