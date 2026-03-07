using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
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
        private static readonly Dictionary<string, string> WrapperSuffixes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "mag", "M" },
            { "db", "DB" },
            { "phase", "P" },
            { "ph", "P" },
            { "real", "R" },
            { "re", "R" },
            { "imag", "I" },
            { "im", "I" },
        };

        private static readonly HashSet<string> BaseExportNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "V", "I",
        };

        public Export Create(
            Parameter exportParameter,
            IReadingContext context,
            ISimulationWithEvents simulation,
            IMapper<Exporter> mapper)
        {
            if (exportParameter is BracketParameter bp)
            {
                string type = bp.Name;
                bool caseSensitive = context.ReaderSettings.CaseSensitivity.IsFunctionNameCaseSensitive;

                if (mapper.TryGetValue(type, caseSensitive, out var exporter))
                {
                    return exporter.CreateExport(
                        exportParameter.Value,
                        type,
                        bp.Parameters,
                        context.EvaluationContext.GetSimulationContext(simulation),
                        context.ReaderSettings.CaseSensitivity);
                }

                // Support nested function syntax: mag(V(out)) → VM(out), db(I(R1)) → IDB(R1), etc.
                if (WrapperSuffixes.TryGetValue(type, out var suffix)
                    && bp.Parameters.Count == 1
                    && bp.Parameters[0] is BracketParameter inner
                    && BaseExportNames.Contains(inner.Name))
                {
                    string flatType = inner.Name.ToUpper() + suffix;

                    if (mapper.TryGetValue(flatType, caseSensitive, out var flatExporter))
                    {
                        return flatExporter.CreateExport(
                            exportParameter.Value,
                            flatType,
                            inner.Parameters,
                            context.EvaluationContext.GetSimulationContext(simulation),
                            context.ReaderSettings.CaseSensitivity);
                    }
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
                        exportParameter.Value,
                        type,
                        parameters,
                        context.EvaluationContext.GetSimulationContext(simulation),
                        context.ReaderSettings.CaseSensitivity);
                }
            }

            if (exportParameter is SingleParameter s)
            {
                string expressionName = s.Value;
                var expressionNames = context.EvaluationContext.GetExpressionNames();

                if (expressionNames.Any(e => e == expressionName))
                {
                    var export = new ExpressionExport(
                        simulation.Name,
                        expressionName,
                        context.EvaluationContext.GetSimulationContext(simulation));

                    return export;
                }
                else
                {
                    context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, $"There is no {expressionName} expression", exportParameter.LineInfo);
                    return null;
                }
            }

            context.Result.ValidationResult.AddError(ValidationEntrySource.Reader,  $"Unsupported export: {exportParameter}", exportParameter.LineInfo);
            return null;
        }
    }
}