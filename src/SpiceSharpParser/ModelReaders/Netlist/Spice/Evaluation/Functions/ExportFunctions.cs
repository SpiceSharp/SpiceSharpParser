using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions
{
    public class ExportFunctions
    {
        /// <summary>
        /// Creates export functions.
        /// </summary>
        public static IEnumerable<KeyValuePair<string, Function>> Create(
            IMapper<Exporter> exporterRegistry,
            INodeNameGenerator nodeNameGenerator,
            IObjectNameGenerator componentNameGenerator,
            IObjectNameGenerator modelNameGenerator,
            IResultService resultService,
            SpiceNetlistCaseSensitivitySettings caseSettings)
        {
            if (exporterRegistry == null)
            {
                throw new ArgumentNullException(nameof(exporterRegistry));
            }

            var result = new List<KeyValuePair<string, Function>>();
            var exporters = new ConcurrentDictionary<string, Export>();

            foreach (KeyValuePair<string, Exporter> exporter in exporterRegistry)
            {
                Function spiceFunction;

                if (exporter.Key == "@")
                {
                    spiceFunction = CreateAtExport(
                        exporters,
                        exporter.Value,
                        exporter.Key,
                        nodeNameGenerator,
                        componentNameGenerator,
                        modelNameGenerator,
                        resultService,
                        caseSettings);
                }
                else
                {
                    spiceFunction = CreateOrdinaryExport(
                        exporters,
                        exporter.Value,
                        exporter.Key,
                        nodeNameGenerator,
                        componentNameGenerator,
                        modelNameGenerator,
                        resultService,
                        caseSettings);
                }

                result.Add(new KeyValuePair<string, Function>(exporter.Key, spiceFunction));
            }

            return result;
        }

        public static Function CreateOrdinaryExport(
            ConcurrentDictionary<string, Export> exporters,
            Exporter exporter,
            string exportType,
            INodeNameGenerator nodeNameGenerator,
            IObjectNameGenerator componentNameGenerator,
            IObjectNameGenerator modelNameGenerator,
            IResultService result,
            SpiceNetlistCaseSensitivitySettings caseSensitivity)
        {
            Function function = new Function();
            function.VirtualParameters = true;
            function.Name = "Exporter: " + exportType;
            function.ArgumentsCount = -1;

            function.ObjectArgsLogic = (image, args, evaluator, context) =>
            {
                if (context.Data == null || !(context.Data is Simulation))
                {
                    return 0.0;
                }

                string exporterKey = string.Format("{0}_{1}_{2}_{3}", ((Simulation)context.Data).Name, context.Name, exportType, string.Join(",", args));

                if (!exporters.ContainsKey(exporterKey))
                {
                    var vectorParameter = new VectorParameter();
                    foreach (var arg in args)
                    {
                        vectorParameter.Elements.Add(new WordParameter(arg.ToString()));
                    }

                    var parameters = new ParameterCollection();
                    parameters.Add(vectorParameter);
                    var export = exporter.CreateExport(
                        image,
                        exportType,
                        parameters,
                        (Simulation)context.Data,
                        nodeNameGenerator,
                        componentNameGenerator,
                        modelNameGenerator,
                        result,
                        caseSensitivity);
                    exporters[exporterKey] = export;
                }

                try
                {
                    double exportValue = exporters[exporterKey].Extract();

                    return exportValue;
                }
                catch (Exception ex)
                {
                    return double.NaN;
                }
            };

            return function;
        }

        public static Function CreateAtExport(
            ConcurrentDictionary<string, Export> exporters,
            Exporter exporter,
            string exportType,
            INodeNameGenerator nodeNameGenerator,
            IObjectNameGenerator componentNameGenerator,
            IObjectNameGenerator modelNameGenerator,
            IResultService result,
            SpiceNetlistCaseSensitivitySettings caseSettings)
        {
            Function function = new Function();
            function.VirtualParameters = true;
            function.Name = "Exporter: @";
            function.ArgumentsCount = 2;

            function.ObjectArgsLogic = (image, args, evaluator, context) =>
            {
                string exporterKey = string.Format(
                    "{0}_{1}_{2}_{3}",
                    context.Data != null ? ((Simulation)context.Data).Name : "no_simulation",
                    context.Name,
                    exportType,
                    string.Join(",", args));

                if (!exporters.ContainsKey(exporterKey))
                {
                    var parameters = new ParameterCollection();
                    parameters.Add(new WordParameter(args[0].ToString()));
                    parameters.Add(new WordParameter(args[1].ToString()));

                    var export = exporter.CreateExport(
                        image,
                        exportType,
                        parameters,
                        context.Data != null ? (Simulation)context.Data : null,
                        nodeNameGenerator,
                        componentNameGenerator,
                        modelNameGenerator,
                        result,
                        caseSettings);

                    exporters[exporterKey] = export;
                }

                try
                {
                    double exportValue = exporters[exporterKey].Extract();

                    return exportValue;
                }
                catch (Exception ex)
                {
                    return double.NaN;
                }
            };

            return function;
        }
    }
}