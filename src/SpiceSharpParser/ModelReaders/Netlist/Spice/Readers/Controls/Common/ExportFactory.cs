using System;
using System.Linq;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Common
{
    public class ExportFactory : IExportFactory
    {
        public Export Create(
            Parameter exportParameter,
            IReadingContext context,
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
                        simulation,
                        context.NodeNameGenerator,
                        context.ComponentNameGenerator,
                        context.ModelNameGenerator,
                        context.Result,
                        context.CaseSensitivity);
                }
            }

            if (exportParameter is ReferenceParameter rp)
            {
                string type = "@";
                var parameters = new ParameterCollection { new WordParameter(rp.Name), new WordParameter(rp.Argument) };

                if (mapper.TryGetValue(type, true, out var exporter))
                {
                    return exporter.CreateExport(
                        exportParameter.Image,
                        type,
                        parameters,
                        simulation,
                        context.NodeNameGenerator,
                        context.ComponentNameGenerator,
                        context.ModelNameGenerator,
                        context.Result,
                        context.CaseSensitivity);
                }
            }

            if (exportParameter is SingleParameter s)
            {
                string expressionName = s.Image;
                var expressionNames = context.ReadingExpressionContext.GetExpressionNames();

                if (expressionNames.Contains(expressionName))
                {
                    var evaluator = context.SimulutionEvaluators.GetEvaluator(simulation);
                    var export = new ExpressionExport(
                        simulation.Name,
                        expressionName,
                        context.ReadingExpressionContext.GetExpression(expressionName),
                        evaluator,
                        context.SimulationExpressionContexts,
                        simulation);

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
