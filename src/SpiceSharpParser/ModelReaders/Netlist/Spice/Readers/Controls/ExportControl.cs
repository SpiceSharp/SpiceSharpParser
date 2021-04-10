using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    public abstract class ExportControl : BaseControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExportControl"/> class.
        /// </summary>
        /// <param name="mapper">The exporter mapper.</param>
        /// <param name="exportFactory">Export factory.</param>
        protected ExportControl(IMapper<Exporter> mapper, IExportFactory exportFactory)
        {
            Mapper = mapper ?? throw new System.ArgumentNullException(nameof(mapper));
            ExportFactory = exportFactory ?? throw new System.ArgumentNullException(nameof(exportFactory));
        }

        /// <summary>
        /// Gets the exporter mapper.
        /// </summary>
        protected IMapper<Exporter> Mapper { get; }

        /// <summary>
        /// Gets the export factory.
        /// </summary>
        protected IExportFactory ExportFactory { get; }

        /// <summary>
        /// Generates a new export.
        /// </summary>
        protected Export GenerateExport(Parameter parameter, ICircuitContext context, Simulation simulation)
        {
            return ExportFactory.Create(parameter, context, simulation, Mapper);
        }

        protected List<Export> CreateExportsForAllVoltageAndCurrents(Simulation simulation, ICircuitContext context)
        {
            var result = new List<Export>();
            var nodes = new List<string>();

            foreach (IEntity entity in context.Result.Circuit)
            {
                if (entity is SpiceSharp.Components.Component c)
                {
                    string componentName = c.Name;
                    var @params = new ParameterCollection(new List<Parameter>());
                    @params.Add(new WordParameter(componentName, null));

                    for (var i = 0; i < c.Nodes.Count; i++)
                    {
                        var node = c.Nodes[i];
                        if (!nodes.Contains(node))
                        {
                            nodes.Add(node);
                        }
                    }

                    // Add current export for component
                    result.Add(
                        Mapper
                        .GetValue("I", true)
                        .CreateExport(
                            "I(" + componentName + ")",
                            "i",
                            @params,
                            context.Evaluator.GetEvaluationContext(simulation),
                            context.CaseSensitivity));
                }
            }

            foreach (var node in nodes)
            {
                var @params = new ParameterCollection(new List<Parameter>());
                @params.Add(new WordParameter(node, null));

                result.Add(
                    Mapper
                    .GetValue("V", true)
                    .CreateExport(
                        "V(" + node + ")",
                        "v",
                        @params,
                        context.Evaluator.GetEvaluationContext(simulation),
                        context.CaseSensitivity));
            }

            return result;
        }

        protected List<Export> GenerateExports(ParameterCollection parameterCollection, Simulation simulation, ICircuitContext context)
        {
            if (parameterCollection.Count == 0)
            {
                return CreateExportsForAllVoltageAndCurrents(simulation, context);
            }

            List<Export> result = new List<Export>();
            foreach (Parameter parameter in parameterCollection)
            {
                if (parameter is BracketParameter || parameter is ReferenceParameter)
                {
                    result.Add(GenerateExport(parameter, context, simulation));
                }
                else
                {
                    string expressionName = parameter.Image;
                    var expressionNames = context.Evaluator.GetExpressionNames();

                    if (expressionNames.Contains(expressionName))
                    {
                        var export = new ExpressionExport(
                            simulation.Name,
                            expressionName,
                            context.Evaluator.GetEvaluationContext(simulation));

                        result.Add(export);
                    }
                }
            }

            return result;
        }

        protected string GetFirstDimensionLabel(Simulation simulation)
        {
            string firstColumnName = null;

            if (simulation is DC)
            {
                firstColumnName = "Voltage (V) / Current (I)";
            }

            if (simulation is Transient)
            {
                firstColumnName = "Time (s)";
            }

            if (simulation is AC)
            {
                firstColumnName = "Frequency (Hz)";
            }

            if (simulation is OP)
            {
                firstColumnName = string.Empty;
            }

            return firstColumnName;
        }
    }
}