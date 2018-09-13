using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Registries;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.CustomFunctions
{
    public class ExportFunctions
    {
        /// <summary>
        /// Creates export custom functions.
        /// </summary>
        public static IEnumerable<KeyValuePair<string, CustomFunction>> Create(IExporterRegistry exporterRegistry, INodeNameGenerator nodeNameGenerator, IObjectNameGenerator objectNameGenerator)
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
                        spiceFunction = CreateAtExport(exporters, exporter, exportType, nodeNameGenerator, objectNameGenerator);
                    }
                    else
                    {
                        spiceFunction = CreateOrdinaryExport(exporters, exporter, exportType, nodeNameGenerator, objectNameGenerator);
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

        public static CustomFunction CreateOrdinaryExport(Dictionary<string, Readers.Controls.Exporters.Export> exporters, Readers.Controls.Exporters.Exporter exporter, string exportType, INodeNameGenerator nodeNameGenerator, IObjectNameGenerator objectNameGenerator)
        {
            CustomFunction function = new CustomFunction();
            function.VirtualParameters = true;
            function.Name = "Exporter: " + exportType;
            function.ArgumentsCount = -1;

            function.Logic = (args, evaluator) =>
            {
                string exporterKey = string.Format("{0}_{1}_{2}", evaluator.Context != null ? ((Simulation)evaluator.Context).Name : "no_simulation", exportType, string.Join(",", args));

                if (!exporters.ContainsKey(exporterKey))
                {
                    var vectorParameter = new VectorParameter();
                    foreach (var arg in args)
                    {
                        vectorParameter.Elements.Add(new WordParameter(arg.ToString()));
                    }

                    var parameters = new ParameterCollection();
                    parameters.Add(vectorParameter);
                    var export = exporter.CreateExport(exportType, parameters, evaluator.Context != null ? (Simulation)evaluator.Context : null, nodeNameGenerator, objectNameGenerator);
                    exporters[exporterKey] = export;
                }

                try
                {
                    return exporters[exporterKey].Extract();
                }
                catch (Exception ex)
                {
                    return double.NaN;
                }
            };

            return function;
        }

        public static void Add(Dictionary<string, CustomFunction> customFunctions, IExporterRegistry exporterRegistry, INodeNameGenerator nodeNameGenerator, IObjectNameGenerator objectNameGenerator)
        {
            foreach (var func in Create(exporterRegistry, nodeNameGenerator, objectNameGenerator))
            {
                customFunctions[func.Key] = func.Value;
            }
        }

        public static CustomFunction CreateAtExport(Dictionary<string, Readers.Controls.Exporters.Export> exporters, Readers.Controls.Exporters.Exporter exporter, string exportType, INodeNameGenerator nodeNameGenerator, IObjectNameGenerator objectNameGenerator)
        {
            CustomFunction function = new CustomFunction();
            function.VirtualParameters = true;
            function.Name = "Exporter: @";
            function.ArgumentsCount = 2;

            function.Logic = (args, evaluator) =>
            {
                string exporterKey = string.Format("{0}_{1}_{2}", evaluator.Context != null ? ((Simulation)evaluator.Context).Name : "no_simulation", exportType, string.Join(",", args));

                if (!exporters.ContainsKey(exporterKey))
                {
                    var parameters = new ParameterCollection();
                    parameters.Add(new WordParameter(args[0].ToString()));
                    parameters.Add(new WordParameter(args[1].ToString()));

                    var export = exporter.CreateExport(exportType, parameters, evaluator.Context != null ? (Simulation)evaluator.Context : null, nodeNameGenerator, objectNameGenerator);
                    exporters[exporterKey] = export;
                }

                try
                {
                    return exporters[exporterKey].Extract();
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
