using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters.Voltage;
using SpiceSharp;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters
{
    class VoltageExporter
    {
        public Export CreateExport(string type, Vector vector, Simulation simulation, ProcessingContext context)
        {
            // Get the nodes
            Identifier node, reference = null;
            switch (vector.Elements.Count)
            {
                case 0:
                    throw new Exception("Node expected");
                case 2:
                    reference = new Identifier(context.GenerateNodeName(vector.Elements[1].RawValue));
                    goto case 1;
                case 1:
                    node = new Identifier(context.GenerateNodeName(vector.Elements[0].RawValue));
                    break;
                default:
                    throw new Exception("Too many nodes specified");
            }

            Export ve = null;
            switch (type.ToLower())
            {
                case "v": ve = new VoltageExport(simulation, node, reference); break;
                case "vr": ve = new VoltageRealExport(simulation, node, reference); break;
                //case "vi": ve = new VoltageImaginaryExport(node, reference); break;
                //case "vdb": ve = new VoltageDecibelExport(node, reference); break;
                //case "vp": ve = new VoltagePhaseExport(node, reference); break;
            }

            return ve;
        }

        public List<string> GetGeneratedTypes()
        {
            return new List<string>() { "v", "vr", "vi", "vdb", "vp" };
        }
    }
}
