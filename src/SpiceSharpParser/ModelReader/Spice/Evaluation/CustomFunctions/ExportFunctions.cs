using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReader.Spice.Context;
using SpiceSharpParser.ModelReader.Spice.Exceptions;
using SpiceSharpParser.ModelReader.Spice.Processors;
using SpiceSharpParser.Model.Spice.Objects;
using SpiceSharpParser.Model.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReader.Spice.Evaluation.CustomFunctions
{
    public class ExportFunctions
    {
        /// <summary>
        /// Creates export custom functions.
        /// </summary>
        public static IEnumerable<KeyValuePair<string, SpiceFunction>> Create(IProcessingContext processingContext, IStatementsProcessor statementsProcessor)
        {
            var result = new List<KeyValuePair<string, SpiceFunction>>();
            var exporters = new Dictionary<string, Processors.Controls.Exporters.Export>();

            foreach (var exporter in statementsProcessor.ExporterRegistry)
            {
                foreach (var exportType in exporter.GetSupportedTypes())
                {
                    SpiceFunction spiceFunction;

                    if (exportType == "@")
                    {
                        spiceFunction = CreateAtExport(processingContext, exporters, exporter, exportType);
                    }
                    else
                    {
                        spiceFunction = CreateOrdinaryExport(processingContext, exporters, exporter, exportType);
                    }

                    result.Add(new KeyValuePair<string, SpiceFunction>(exportType, spiceFunction));
                    if (exportType != exportType.ToUpper())
                    {
                        result.Add(new KeyValuePair<string, SpiceFunction>(exportType.ToUpper(), spiceFunction));
                    }
                }
            }

            return result;
        }

        public static SpiceFunction CreateOrdinaryExport(IProcessingContext processingContext, Dictionary<string, Processors.Controls.Exporters.Export> exporters, Processors.Controls.Exporters.Exporter exporter, string exportType)
        {
            SpiceFunction function = new SpiceFunction();
            function.VirtualParameters = true;
            function.Name = "Exporter: " + exportType;
            function.ArgumentsCount = 0;

            function.Logic = (args, simulation) =>
            {
                string exporterKey = exportType + string.Join(",", args);

                if (!exporters.ContainsKey(exporterKey))
                {
                    var vectorParameter = new VectorParameter();
                    foreach (var arg in args)
                    {
                        vectorParameter.Elements.Add(new WordParameter(arg.ToString()));
                    }

                    var parameters = new ParameterCollection();
                    parameters.Add(vectorParameter);
                    var export = exporter.CreateExport(exportType, parameters, (Simulation)simulation ?? processingContext.Result.Simulations.First(), processingContext);
                    exporters[exporterKey] = export;
                }

                try
                {
                    return exporters[exporterKey].Extract();
                }
                catch (GeneralReaderException)
                {
                    return double.NaN;
                }
            };

            return function;
        }

        public static SpiceFunction CreateAtExport(IProcessingContext processingContext, Dictionary<string, Processors.Controls.Exporters.Export> exporters, Processors.Controls.Exporters.Exporter exporter, string exportType)
        {
            SpiceFunction function = new SpiceFunction();
            function.VirtualParameters = true;
            function.Name = "Exporter: @";
            function.ArgumentsCount = 2;

            function.Logic = (args, simulation) =>
            {
                string exporterKey = exportType + string.Join(",", args);

                if (!exporters.ContainsKey(exporterKey))
                {
                    var parameters = new ParameterCollection();
                    parameters.Add(new WordParameter(args[1].ToString()));
                    parameters.Add(new WordParameter(args[0].ToString()));

                    var export = exporter.CreateExport(exportType, parameters, (Simulation)simulation ?? processingContext.Result.Simulations.First(), processingContext);
                    exporters[exporterKey] = export;
                }

                try
                {
                    return exporters[exporterKey].Extract();
                }
                catch (GeneralReaderException)
                {
                    return double.NaN;
                }
            };

            return function;
        }
    }
}
