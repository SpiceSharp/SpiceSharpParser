using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Registries;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Evaluation.CustomFunctions
{
    public class ExportFunctions
    {
        /// <summary>
        /// Creates export custom functions.
        /// </summary>
        public static IEnumerable<KeyValuePair<string, CustomFunction>> Create(IReadingContext readingContext, IExporterRegistry exporterRegistry)
        {
            if (exporterRegistry == null)
            {
                throw new ArgumentNullException(nameof(exporterRegistry));
            }

            var result = new List<KeyValuePair<string, CustomFunction>>();
            var exporters = new Dictionary<string, Readers.Controls.Exporters.Export>();

            foreach (var exporter in exporterRegistry)
            {
                foreach (var exportType in exporter.GetSupportedTypes())
                {
                    CustomFunction spiceFunction;

                    if (exportType == "@")
                    {
                        spiceFunction = CreateAtExport(readingContext, exporters, exporter, exportType);
                    }
                    else
                    {
                        spiceFunction = CreateOrdinaryExport(readingContext, exporters, exporter, exportType);
                    }

                    result.Add(new KeyValuePair<string, CustomFunction>(exportType, spiceFunction));
                    if (exportType != exportType.ToUpper())
                    {
                        result.Add(new KeyValuePair<string, CustomFunction>(exportType.ToUpper(), spiceFunction));
                    }
                }
            }

            return result;
        }

        public static CustomFunction CreateOrdinaryExport(IReadingContext readingContext, Dictionary<string, Readers.Controls.Exporters.Export> exporters, Readers.Controls.Exporters.Exporter exporter, string exportType)
        {
            CustomFunction function = new CustomFunction();
            function.VirtualParameters = true;
            function.Name = "Exporter: " + exportType;
            function.ArgumentsCount = -1;

            function.Logic = (args, simulation, evaluator) =>
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
                    var export = exporter.CreateExport(exportType, parameters, (Simulation)simulation ?? readingContext.Result.Simulations.First(), readingContext);
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

        public static void Add(Dictionary<string, CustomFunction> customFunctions, IReadingContext readingContext, IExporterRegistry exporterRegistry)
        {
            foreach (var func in Create(readingContext, exporterRegistry))
            {
                customFunctions[func.Key] = func.Value;
            }
        }

        public static CustomFunction CreateAtExport(IReadingContext readingContext, Dictionary<string, Readers.Controls.Exporters.Export> exporters, Readers.Controls.Exporters.Exporter exporter, string exportType)
        {
            CustomFunction function = new CustomFunction();
            function.VirtualParameters = true;
            function.Name = "Exporter: @";
            function.ArgumentsCount = 2;

            function.Logic = (args, simulation, evaluator) =>
            {
                string exporterKey = exportType + string.Join(",", args);

                if (!exporters.ContainsKey(exporterKey))
                {
                    var parameters = new ParameterCollection();
                    parameters.Add(new WordParameter(args[1].ToString()));
                    parameters.Add(new WordParameter(args[0].ToString()));

                    var export = exporter.CreateExport(exportType, parameters, (Simulation)simulation ?? readingContext.Result.Simulations.First(), readingContext);
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
