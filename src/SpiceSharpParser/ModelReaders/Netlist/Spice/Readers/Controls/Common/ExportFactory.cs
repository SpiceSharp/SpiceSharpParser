using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Common
{
    public class ExportFactory : IExportFactory
    {
        public Export Create(
            Parameter exportParameter,
            ICircuitContext context,
            Simulation simulation,
            IMapper<Exporter> mapper)
        {
            if (exportParameter is BracketParameter bp)
            {
                string type = bp.Name;

                if (mapper.TryGetValue(type, context.CaseSensitivity.IsFunctionNameCaseSensitive, out var exporter))
                {
                    return exporter.CreateExport(
                        exportParameter.Image,
                        type,
                        bp.Parameters,
                        context.CircuitEvaluator.GetContext(simulation),
                        context.CaseSensitivity);
                }
            }

            if (exportParameter is ReferenceParameter rp)
            {
                string type = "@";
                var parameters = new ParameterCollection { new VectorParameter() { Elements = new List<SingleParameter>() { new WordParameter(rp.Name), new WordParameter(rp.Argument) } } };

                if (mapper.TryGetValue(type, true, out var exporter))
                {
                    return exporter.CreateExport(
                        exportParameter.Image,
                        type,
                        parameters,
                        context.CircuitEvaluator.GetContext(simulation),
                        context.CaseSensitivity);
                }
            }

            if (exportParameter is SingleParameter s)
            {
                string expressionName = s.Image;
                var expressionNames = context.CircuitEvaluator.GetExpressionNames();

                if (expressionNames.Any(e => e == expressionName))
                {
                    var export = new ExpressionExport(
                        simulation.Name,
                        expressionName,
                        context.CircuitEvaluator.GetContext(simulation));

                    return export;
                }
                else
                {
                    throw new Exception("There is no " + expressionName + " expression");
                }
            }

            throw new Exception("Unsupported export: " + exportParameter.Image);
        }
    }
}