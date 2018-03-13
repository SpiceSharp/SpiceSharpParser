using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Registries;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    /// <summary>
    /// Processes all supported <see cref="Control"/> from spice netlist object model.
    /// </summary>
    public class ControlProcessor : StatementProcessor<Control>
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
        /// Processes a control statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modifify</param>
        public override void Process(Control statement, IProcessingContext context)
        {
            string type = statement.Name.ToLower();

            if (!Registry.Supports(type))
            {
                context.AddWarning("Unsupported control: " + statement.Name + " at " + statement.LineNumber + " line");
            }
            else
            {
                Registry.Get(type).Process(statement, context);
            }
        }

        internal int GetSubOrder(Control statement)
        {
            string type = statement.Name.ToLower();
            return Registry.IndexOf(type);
        }
    }
}
