using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters.Voltage;
using SpiceSharp;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters
{
    public class VoltageExporter
    {
        public Export CreateExport(string type, ParameterCollection parameters, Simulation simulation, ProcessingContext context)
        {
            if (parameters.Count != 1 || (!(parameters[0] is VectorParameter) && !(parameters[0] is SingleParameter)))
            {
                throw new Exception("Voltage exports should have vector or single parameter");
            }

            // Get the nodes
            Identifier node, reference = null;
            if (parameters[0] is VectorParameter vector)
            {
                switch (vector.Elements.Count)
                {
                    case 0:
                        throw new Exception("Node expected");
                    case 2:
                        reference = new Identifier(context.GenerateNodeName(vector.Elements[1].Image));
                        goto case 1;
                    case 1:
                        node = new Identifier(context.GenerateNodeName(vector.Elements[0].Image));
                        break;
                    default:
                        throw new Exception("Too many nodes specified");
                }
            }
            else
            {
                var singleParameter = parameters[0] as SingleParameter;
                node = new Identifier(context.GenerateNodeName(singleParameter.Image));
            }

            Export ve = null;
            switch (type.ToLower())
            {
                case "v": ve = new VoltageExport(simulation, node, reference); break;
                case "vr": ve = new VoltageRealExport(simulation, node, reference); break;
                case "vi": ve = new VoltageImaginaryExport(simulation, node, reference); break;
                case "vdb": ve = new VoltageDecibelExport(simulation, node, reference); break;
                case "vp": ve = new VoltagePhaseExport(simulation, node, reference); break;
            }

            return ve;
        }

        public List<string> GetGeneratedTypes()
        {
            return new List<string>() { "v", "vr", "vi", "vdb", "vp" };
        }
    }
}
