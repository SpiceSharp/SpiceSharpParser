using System;
using System.Linq;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls
{
    class SaveControl : SingleControlProcessor
    {
        public override void Process(Control statement, ProcessingContext context)
        {
            foreach (var parameter in statement.Parameters)
            {
                if (parameter is BracketParameter b)
                {
                    context.AddExport(GenerateExport(b, context.GetSimulation(), context));
                }
            }
        }

        private Export GenerateExport(BracketParameter b, Simulation simulation, ProcessingContext context)
        {
            var vE = new VoltageExporter();
            return vE.CreateExport(b.Name, b.Content.Parameters, simulation, context);
        }
    }
}
