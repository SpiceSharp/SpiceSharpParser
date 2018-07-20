using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Registries;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers
{
    /// <summary>
    /// Reads all supported <see cref="Control"/> from spice netlist object model.
    /// </summary>
    public class ControlReader : StatementReader<Control>, IControlReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ControlReader"/> class.
        /// </summary>
        /// <param name="registry">Th registry</param>
        public ControlReader(IControlRegistry registry)
        {
            Registry = registry;
        }

        /// <summary>
        /// Gets the registry
        /// </summary>
        public IControlRegistry Registry { get; }

        /// <summary>
        /// Returns whether reader can process specific statement.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <returns>
        /// True if the reader can process given statement.
        /// </returns>
        public override bool CanRead(Statement statement)
        {
            return statement is Control;
        }

        /// <summary>
        /// Reads a control statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modifify</param>
        public override void Read(Control statement, IReadingContext context)
        {
            string type = statement.Name.ToLower();

            if (!Registry.Supports(type))
            {
                context.Result.AddWarning("Unsupported control: " + statement.Name + " at " + statement.LineNumber + " line");
            }
            else
            {
                Registry.Get(type).Read(statement, context);
            }
        }
    }
}
