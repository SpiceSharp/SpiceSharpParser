using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
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
        public static IEnumerable<KeyValuePair<string, CustomFunction>> Create(IMapper<Exporter> exporterRegistry, INodeNameGenerator nodeNameGenerator, IObjectNameGenerator objectNameGenerator, bool ignoreCaseForNodes)
        {
            if (exporterRegistry == null)
            {
                throw new ArgumentNullException(nameof(exporterRegistry));
            }

            var result = new List<KeyValuePair<string, CustomFunction>>();
            var exporters = new Dictionary<string, Export>();

            foreach (KeyValuePair<string, Exporter> exporter in exporterRegistry)
            {
                CustomFunction spiceFunction;

                if (exporter.Key == "@")
                {
                    spiceFunction = CreateAtExport(exporters, exporter.Value, exporter.Key, nodeNameGenerator, objectNameGenerator);
                }
                else
                {
                    spiceFunction = CreateOrdinaryExport(exporters, exporter.Value, exporter.Key, nodeNameGenerator, objectNameGenerator, ignoreCaseForNodes);
                }

                result.Add(new KeyValuePair<string, CustomFunction>(exporter.Key, spiceFunction));
            }

            return result;
        }

        public static CustomFunction CreateOrdinaryExport(Dictionary<string, Readers.Controls.Exporters.Export> exporters, Readers.Controls.Exporters.Exporter exporter, string exportType, INodeNameGenerator nodeNameGenerator, IObjectNameGenerator objectNameGenerator, bool ignoreCaseForNodes)
        {
            CustomFunction function = new CustomFunction();
            function.VirtualParameters = true;
            function.Name = "Exporter: " + exportType;
            function.ArgumentsCount = -1;

            function.Logic = (args, evaluator) =>
            {
                if (evaluator.Context == null || !(evaluator.Context is Simulation))
                {
                    return double.NaN;
                }

                string exporterKey = string.Format("{0}_{1}_{2}", ((Simulation)evaluator.Context).Name, exportType, string.Join(",", args));

                if (!exporters.ContainsKey(exporterKey))
                {
                    var vectorParameter = new VectorParameter();
                    foreach (var arg in args)
                    {
                        vectorParameter.Elements.Add(new WordParameter(arg.ToString()));
                    }

                    var parameters = new ParameterCollection();
                    parameters.Add(vectorParameter);
                    var export = exporter.CreateExport(exportType, parameters, (Simulation)evaluator.Context, nodeNameGenerator, objectNameGenerator, ignoreCaseForNodes);
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

                    var export = exporter.CreateExport(exportType, parameters, evaluator.Context != null ? (Simulation)evaluator.Context : null, nodeNameGenerator, objectNameGenerator, false);
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
