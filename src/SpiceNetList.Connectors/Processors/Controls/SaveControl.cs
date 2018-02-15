using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls
{
    public class SaveControl : SingleControlProcessor
    {
        public override void Process(Control statement, ProcessingContext context)
        {
            foreach (var parameter in statement.Parameters)
            {
                if (parameter is BracketParameter bracketParameter)
                {
                    context.AddExport(GenerateExport(bracketParameter, context.GetCurrentSimulation(), context));
                }
            }
        }

        private Export GenerateExport(BracketParameter parameter, Simulation simulation, ProcessingContext context)
        {
            var vE = new VoltageExporter();
            return vE.CreateExport(parameter.Name, parameter.Content.Parameters, simulation, context);
        }
    }
}
