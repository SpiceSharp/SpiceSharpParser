﻿using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System;
using System.Collections.Concurrent;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Export
{
    public class AtExportFunction : Function<object, double>
    {
        private readonly ConcurrentDictionary<string, Readers.Controls.Exporters.Export> exporters;
        private readonly Exporter exporter;
        private readonly string exportType;
        private readonly INodeNameGenerator nodeNameGenerator;
        private readonly IObjectNameGenerator componentNameGenerator;
        private readonly IObjectNameGenerator modelNameGenerator;
        private readonly IResultService result;
        private readonly SpiceNetlistCaseSensitivitySettings caseSensitivity;

        public AtExportFunction(string name,
            ConcurrentDictionary<string, Readers.Controls.Exporters.Export> exporters,
            Exporter exporter,
            string exportType,
            INodeNameGenerator nodeNameGenerator,
            IObjectNameGenerator componentNameGenerator,
            IObjectNameGenerator modelNameGenerator,
            IResultService result,
            SpiceNetlistCaseSensitivitySettings caseSensitivity)
        {
            Name = name;
            VirtualParameters = true;
            ArgumentsCount = 2;
            this.exporters = exporters;
            this.exporter = exporter;
            this.exportType = exportType;
            this.nodeNameGenerator = nodeNameGenerator;
            this.componentNameGenerator = componentNameGenerator;
            this.modelNameGenerator = modelNameGenerator;
            this.result = result;
            this.caseSensitivity = caseSensitivity;
        }

        public override double Logic(string image, object[] args, IEvaluator evaluator, ExpressionContext context)
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
        }
    }
}