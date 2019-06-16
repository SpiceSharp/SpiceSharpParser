using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Export;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions
{
    public class ExportFunctions
    {
        /// <summary>
        /// Creates export functions.
        /// </summary>
        public static IEnumerable<KeyValuePair<string, IFunction<string, double>>> Create(
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

            var result = new List<KeyValuePair<string, IFunction<string, double>>>();
            var exporters = new ConcurrentDictionary<string, Readers.Controls.Exporters.Export>();

            foreach (KeyValuePair<string, Exporter> exporter in exporterRegistry)
            {
                IFunction<string, double> spiceFunction;

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

                result.Add(new KeyValuePair<string, IFunction<string, double>>(exporter.Key, spiceFunction));
            }

            return result;
        }

        public static IFunction<string, double> CreateOrdinaryExport(
            ConcurrentDictionary<string, Readers.Controls.Exporters.Export> exporters,
            Exporter exporter,
            string exportType,
            INodeNameGenerator nodeNameGenerator,
            IObjectNameGenerator componentNameGenerator,
            IObjectNameGenerator modelNameGenerator,
            IResultService result,
            SpiceNetlistCaseSensitivitySettings caseSensitivity)
        {
            return new OrdinaryExportFunction(
                "Exporter: " + exportType, 
                exporters,
                exporter,
                exportType, 
                nodeNameGenerator,
                componentNameGenerator,
                modelNameGenerator,
                result, 
                caseSensitivity);
        }

        public static IFunction<object, double> CreateAtExport(
            ConcurrentDictionary<string, Readers.Controls.Exporters.Export> exporters,
            Exporter exporter,
            string exportType,
            INodeNameGenerator nodeNameGenerator,
            IObjectNameGenerator componentNameGenerator,
            IObjectNameGenerator modelNameGenerator,
            IResultService result,
            SpiceNetlistCaseSensitivitySettings caseSensitivity)
        {
            return new AtExportFunction(
                 "Exporter: @",
                 exporters,
                 exporter,
                 exportType,
                 nodeNameGenerator,
                 componentNameGenerator,
                 modelNameGenerator,
                 result,
                 caseSensitivity);
        }
    }
}