using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.ModelReader.Netlist.Spice.Registries;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Processors
{
    /// <summary>
    /// Processes all supported <see cref="Control"/> from spice netlist object model.
    /// </summary>
    public class ControlProcessor : StatementProcessor<Control>, IControlProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ControlProcessor"/> class.
        /// </summary>
        /// <param name="registry">Th registry</param>
        public ControlProcessor(IControlRegistry registry)
        {
            Registry = registry;
        }

        /// <summary>
        /// Gets the registry
        /// </summary>
        public IControlRegistry Registry { get; }

        /// <summary>
        /// Returns whether processor can process specific statement.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <returns>
        /// True if the processor can process given statement.
        /// </returns>
        public override bool CanProcess(Statement statement)
        {
            return statement is Control;
        }

        /// <summary>
        /// Processes a control statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modifify</param>
        public override void Process(Control statement, IProcessingContext context)
        {
            string type = statement.Name.ToLower();

            if (!Registry.Supports(type))
            {
                context.Result.AddWarning("Unsupported control: " + statement.Name + " at " + statement.LineNumber + " line");
            }
            else
            {
                Registry.Get(type).Process(statement, context);
            }
        }
    }
}
