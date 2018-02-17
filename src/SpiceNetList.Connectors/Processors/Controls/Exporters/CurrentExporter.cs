using System;
using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters.CurrentExports;
using SpiceSharp;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters
{
    public class CurrentExporter
    {
        public Export CreateExport(string type, ParameterCollection parameters, Simulation simulation, ProcessingContext context)
        {
            if (parameters.Count != 1 || !(parameters[0] is SingleParameter))
            {
                throw new Exception("Current exports should single parameter");
            }

            // Get the nodes
            Identifier node, reference = null;
            if (parameters[0] is VectorParameter vector)
            {
                switch (vector.Elements.Count)
                {
                    case 0:
                        throw new Exception("Node expected");
                    case 1:
                        node = new Identifier(context.GenerateNodeName(vector.Elements[0].Image));
                        break;
                    default:
                        throw new Exception("Too many nodes specified");
                }
            }
            else
            {
                node = new Identifier(context.GenerateNodeName(parameters.GetString(0)));
            }

            Export ce = null;
            switch (type.ToLower())
            {
                case "i": ce = new CurrentExport(simulation, node); break;
                case "ir": ce = new CurrentRealExport(simulation, node); break;
                case "ii": ce = new CurrentImaginaryExport(simulation, node); break;
                case "im": ce = new CurrentMagnitudeExport(simulation, node); break;
                case "idb": ce = new CurrentDecibelExport(simulation, node); break;
                case "ip": ce = new CurrentPhaseExport(simulation, node); break;
            }

            return ce;
        }

        public List<string> GetGeneratedTypes()
        {
            return new List<string>() { "i", "ir", "ii", "im", "idb", "ip" };
        }
    }
}
