using System;
using System.Collections.Generic;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters.CurrentExports;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters
{
    public class CurrentExporter : Exporter
    {
        public virtual ICollection<string> CreatedTypes => new List<string> { "i", "ir", "ii", "im", "idb", "ip" };

        public override Export CreateExport(string name, string type, ParameterCollection parameters, Simulation simulation, INodeNameGenerator nodeNameGenerator, IObjectNameGenerator componentNameGenerator, IObjectNameGenerator modelNameGenerator, IResultService result, SpiceNetlistCaseSensitivitySettings caseSettings)
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
                        componentIdentifier = componentNameGenerator.Generate(vector.Elements[0].Image);
                        break;
                    default:
                        throw new Exception("Too many nodes specified");
                }
            }
            else
            {
                componentIdentifier = componentNameGenerator.Generate(parameters.Get(0).Image);
            }

            Export export = null;
            switch (type.ToLower())
            {
                case "i": export = new CurrentExport(name, simulation, componentIdentifier); break;
                case "ir": export = new CurrentRealExport(name, simulation, componentIdentifier); break;
                case "ii": export = new CurrentImaginaryExport(name, simulation, componentIdentifier); break;
                case "im": export = new CurrentMagnitudeExport(name, simulation, componentIdentifier); break;
                case "idb": export = new CurrentDecibelExport(name, simulation, componentIdentifier); break;
                case "ip": export = new CurrentPhaseExport(name, simulation, componentIdentifier); break;
            }

            return export;
        }
    }
}
