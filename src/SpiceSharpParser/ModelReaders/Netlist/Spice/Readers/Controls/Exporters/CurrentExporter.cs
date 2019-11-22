using System;
using System.Collections.Generic;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters.CurrentExports;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters
{
    public class CurrentExporter : Exporter
    {
        public virtual ICollection<string> CreatedTypes => new List<string> { "i", "ir", "ii", "im", "idb", "ip" };

        public override Export CreateExport(string name, string type, ParameterCollection parameters, EvaluationContext context, ISpiceNetlistCaseSensitivitySettings caseSettings)
        {
            if (parameters.Count != 1 || (!(parameters[0] is VectorParameter) && !(parameters[0] is SingleParameter)))
            {
                throw new Exception("Current exports should have a single parameter or vector parameter");
            }

            // Get the nodes
            string componentIdentifier = null;
            if (parameters[0] is VectorParameter vector)
            {
                switch (vector.Elements.Count)
                {
                    case 0:
                        throw new Exception("Node expected");
                    case 1:
                        componentIdentifier = context.NameGenerator.GenerateObjectName(vector.Elements[0].Image);
                        break;
                    default:
                        throw new Exception("Too many nodes specified");
                }
            }
            else
            {
                componentIdentifier = context.NameGenerator.GenerateObjectName(parameters.Get(0).Image);
            }

            Export export = null;
            switch (type.ToLower())
            {
                case "i": export = new CurrentExport(name, context.Simulation, componentIdentifier); break;
                case "ir": export = new CurrentRealExport(name, context.Simulation, componentIdentifier); break;
                case "ii": export = new CurrentImaginaryExport(name, context.Simulation, componentIdentifier); break;
                case "im": export = new CurrentMagnitudeExport(name, context.Simulation, componentIdentifier); break;
                case "idb": export = new CurrentDecibelExport(name, context.Simulation, componentIdentifier); break;
                case "ip": export = new CurrentPhaseExport(name, context.Simulation, componentIdentifier); break;
            }

            return export;
        }
    }
}
