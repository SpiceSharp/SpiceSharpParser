using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetlist.SpiceSharpConnector.Registries;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Simulations;
using System.Linq;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls
{
    /// <summary>
    /// Processes .SAVE <see cref="Control"/> from spice netlist object model.
    /// </summary>
    public class SaveControl : BaseControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SaveControl"/> class.
        /// </summary>
        /// <param name="registry">The exporter registry</param>
        public SaveControl(IExporterRegistry registry)
        {
            Registry = registry;
        }

        /// <summary>
        /// Gets the type of genera
        /// </summary>
        public override string TypeName => "save";

        protected IExporterRegistry Registry { get; }

        /// <summary>
        /// Processes <see cref="Control"/> statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modify</param>
        public override void Process(Control statement, ProcessingContext context)
        {
            foreach (var parameter in statement.Parameters)
            {
                if (parameter is BracketParameter bracketParameter)
                {
                    context.AddExport(GenerateExport(bracketParameter, context.Simulations.First(), context));
                }
            }
        }

        private Export GenerateExport(BracketParameter parameter, Simulation simulation, ProcessingContext context)
        {
            string type = parameter.Name.ToLower();

            if (Registry.Supports(type))
            {
                return Registry.Get(type).CreateExport(type, parameter.Parameters, simulation, context);
            }

            throw new System.Exception("Unsuported save");
        }
    }
}
