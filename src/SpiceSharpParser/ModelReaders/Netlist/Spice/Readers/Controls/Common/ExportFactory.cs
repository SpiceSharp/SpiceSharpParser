using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Validation;
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
                        context.Evaluator.GetEvaluationContext(simulation),
                        context.CaseSensitivity);
                }
            }

            if (exportParameter is ReferenceParameter rp)
            {
                string type = "@";
                var parameters = new ParameterCollection(
                    new List<Parameter>()
                    {
                        new VectorParameter(
                            new List<SingleParameter>()
                            {
                                new WordParameter(rp.Name, rp.LineInfo),
                                new WordParameter(rp.Argument, rp.LineInfo),
                            }),
                    });

                if (mapper.TryGetValue(type, true, out var exporter))
                {
                    return exporter.CreateExport(
                        exportParameter.Image,
                        type,
                        parameters,
                        context.Evaluator.GetEvaluationContext(simulation),
                        context.CaseSensitivity);
                }
            }

            if (exportParameter is SingleParameter s)
            {
                string expressionName = s.Image;
                var expressionNames = context.Evaluator.GetExpressionNames();

                if (expressionNames.Any(e => e == expressionName))
                {
                    var export = new ExpressionExport(
                        simulation.Name,
                        expressionName,
                        context.Evaluator.GetEvaluationContext(simulation));

                    return export;
                }
                else
                {
                    context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, $"There is no {expressionName} expression", exportParameter.LineInfo));
                    return null;
                }
            }

            context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, $"Unsupported export: {exportParameter.Image}", exportParameter.LineInfo));
            return null;
        }
    }
}