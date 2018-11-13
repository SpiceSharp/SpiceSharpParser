using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Plots;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .PLOT <see cref="Control"/> from SPICE netlist object model.
    /// It supports DC, AC, TRAN type of .PLOT.
    /// </summary>
    public class PlotControl : ExportControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlotControl"/> class.
        /// </summary>
        /// <param name="mapper">The exporter mapper.</param>
        public PlotControl(IMapper<Exporter> mapper, IExportFactory exportFactory)
            : base(mapper, exportFactory)
        {
        }

        /// <summary>
        /// Gets the supported plot types.
        /// </summary>
        protected ICollection<string> SupportedPlotTypes { get; } = new List<string>() { "dc", "ac", "tran" };

        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public override void Read(Control statement, IReadingContext context)
        {
            if (context == null)
            {
                throw new System.ArgumentNullException(nameof(context));
            }

            if (statement == null)
            {
                throw new System.ArgumentNullException(nameof(statement));
            }

            string type = statement.Parameters[0].Image.ToLower();

            if (!SupportedPlotTypes.Contains(type))
            {
                throw new GeneralReaderException(".plot supports only dc, ac, tran plots");
            }

            foreach (var simulation in context.Result.Simulations)
            {
                if (type == "dc" && simulation is DC)
                {
                    CreatePlot(statement, context, simulation, "Voltage (V)");
                }

                if (type == "tran" && simulation is Transient)
                {
                    CreatePlot(statement, context, simulation, "Time (s)");
                }

                if (type == "ac" && simulation is AC)
                {
                    CreatePlot(statement, context, simulation, "Frequency (Hz)");
                }
            }
        }

        private void CreatePlot(Control statement, IReadingContext context, Simulation simulationToPlot, string xUnit)
        {
            var plot = new XyPlot(simulationToPlot.Name);
            List<Export> exports = GenerateExports(statement.Parameters.Skip(1), simulationToPlot, context);

            for (var i = 0; i < exports.Count; i++)
            {
                plot.Series.Add(new Series(exports[i].Name) { XUnit = xUnit, YUnit = exports[i].QuantityUnit });
            }

            simulationToPlot.ExportSimulationData += (object sender, ExportDataEventArgs e) =>
            {
                double x = 0;

                if (simulationToPlot is Transient)
                {
                    x = e.Time;
                }

                if (simulationToPlot is AC)
                {
                    x = e.Frequency;
                }

                if (simulationToPlot is DC dc)
                {
                    if (dc.Sweeps.Count > 1)
                    {
                        // TODO: Add support for DC Sweeps > 1
                        throw new GeneralReaderException(".plot dc doesn't support sweep count > 1");
                    }

                    x = e.SweepValue;
                }

                for (var i = 0; i < exports.Count; i++)
                {
                    plot.Series[i].Points.Add(new Point() { X = x, Y = exports[i].Extract() });
                }
            };

            context.Result.AddPlot(plot);
        }

        private List<Export> GenerateExports(ParameterCollection parameterCollection, Simulation simulationToPlot, IReadingContext context)
        {
            List<Export> result = new List<Export>();
            foreach (Parameter parameter in parameterCollection)
            {
                if (parameter is BracketParameter || parameter is ReferenceParameter)
                {
                    result.Add(GenerateExport(parameter, context, simulationToPlot));
                }
                else
                {
                    string expressionName = parameter.Image;
                    var evaluator = context.SimulutionEvaluators.GetEvaluator(simulationToPlot);
                    var expressionNames = context.ReadingExpressionContext.GetExpressionNames();

                    if (expressionNames.Contains(expressionName))
                    {
                        result.Add(
                            new ExpressionExport(
                                simulationToPlot.Name,
                                expressionName,
                                context.ReadingExpressionContext.GetExpression(expressionName),
                                evaluator,
                                context.SimulationExpressionContexts,
                                simulationToPlot));
                    }
                }
            }

            return result;
        }
    }
}
