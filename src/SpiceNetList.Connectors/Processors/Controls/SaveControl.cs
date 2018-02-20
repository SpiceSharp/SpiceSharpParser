using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls
{
    /// <summary>
    /// Processes .SAVE command from Spice netlist.
    /// </summary>
    public class SaveControl : BaseControl
    {
        /// <summary>
        /// Gets the type of genera
        /// </summary>
        public override string TypeName => "save";

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
                    context.AddExport(GenerateExport(bracketParameter, context.FirstSimulation, context));
                }
            }
        }

        private Export GenerateExport(BracketParameter parameter, Simulation simulation, ProcessingContext context)
        {
            var voltageExport = new VoltageExporter();
            var currentExport = new CurrentExporter();
            string type = parameter.Name.ToLower();

            if (voltageExport.GetGeneratedTypes().Contains(type))
            {
                return voltageExport.CreateExport(parameter.Name, parameter.Parameters, simulation, context);
            }
            else if (currentExport.GetGeneratedTypes().Contains(type))
            {
                return currentExport.CreateExport(parameter.Name, parameter.Parameters, simulation, context);
            }

            throw new System.Exception("Unsuported save");
        }
    }
}
