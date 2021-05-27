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
        protected Export GenerateExport(Parameter parameter, IReadingContext context, Simulation simulation)
        {
            return ExportFactory.Create(parameter, context, simulation, Mapper);
        }

        protected List<Export> CreateExportsForAllVoltageAndCurrents(Simulation simulation, IReadingContext context)
        {
            var result = new List<Export>();
            var nodes = new List<string>();

            if (simulation is Noise)
            {
                result.Add(new OutputNoiseExport(simulation));
                result.Add(new InputNoiseExport(simulation));
                return result;
            }

            foreach (IEntity entity in context.ContextEntities)
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
                            context.EvaluationContext.GetSimulationContext(simulation),
                            context.ReaderSettings.CaseSensitivity));
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
                        context.EvaluationContext.GetSimulationContext(simulation),
                        context.ReaderSettings.CaseSensitivity));
            }

            return result;
        }

        protected List<Export> GenerateExports(ParameterCollection parameterCollection, Simulation simulation, IReadingContext context)
        {
            if (parameterCollection == null || parameterCollection.Count == 0 || (parameterCollection.Count == 1 && parameterCollection[0].Value.ToLower() == "merge"))
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
                    string expressionName = parameter.Value;
                    var expressionNames = context.EvaluationContext.GetExpressionNames();

                    if (expressionNames.Contains(expressionName))
                    {
                        var export = new ExpressionExport(
                            simulation.Name,
                            expressionName,
                            context.EvaluationContext.GetSimulationContext(simulation));

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

            if (simulation is Noise)
            {
                firstColumnName = "Frequency (Hz)";
            }

            return firstColumnName;
        }
    }
}