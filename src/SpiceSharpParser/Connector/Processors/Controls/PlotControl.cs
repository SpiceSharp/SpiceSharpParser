using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Simulations;
using SpiceSharpParser.Connector.Context;
using SpiceSharpParser.Connector.Exceptions;
using SpiceSharpParser.Connector.Processors.Controls.Exporters;
using SpiceSharpParser.Connector.Processors.Controls.Plots;
using SpiceSharpParser.Connector.Registries;
using SpiceSharpParser.Model.SpiceObjects;
using SpiceSharpParser.Model.SpiceObjects.Parameters;

namespace SpiceSharpParser.Connector.Processors.Controls
{
    /// <summary>
    /// Processes .PLOT <see cref="Control"/> from spice netlist object model.
    /// It supports DC, AC, TRAN type of .PLOT
    /// </summary>
    public class PlotControl : ExportControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlotControl"/> class.
        /// </summary>
        /// <param name="registry">The exporter registry</param>
        public PlotControl(IExporterRegistry registry)
            : base(registry)
        {
        }

        /// <summary>
        /// Gets the type of genereator
        /// </summary>
        public override string TypeName => "plot";

        /// <summary>
        /// Gets the supported plot types
        /// </summary>
        protected ICollection<string> SupportedPlotTypes { get; } = new List<string>() { "dc", "ac", "tran" };

        /// <summary>
        /// Processes <see cref="Control"/> statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modify</param>
        public override void Process(Control statement, IProcessingContext context)
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
                throw new GeneralConnectorException(".plot supports only dc, ac, tran plots");
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

        private void CreatePlot(Control statement, IProcessingContext context, Simulation simulationToPlot, string xUnit)
        {
            var plot = new Plot(simulationToPlot.Name.ToString());
            List<Export> exports = GenerateExports(statement.Parameters.Skip(1), simulationToPlot, context);

            for (var i = 0; i < exports.Count; i++)
            {
                plot.Series.Add(new Series(exports[i].Name) { XUnit = xUnit, YUnit = exports[i].QuantityUnit });
            }

            simulationToPlot.OnExportSimulationData += (object sender, ExportDataEventArgs e) =>
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
                        throw new GeneralConnectorException(".plot dc doesn't support sweep count > 1");
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

        private List<Export> GenerateExports(ParameterCollection parameterCollection, Simulation simulationToPlot, IProcessingContext context)
        {
            List<Export> result = new List<Export>();
            foreach (Parameter parameter in parameterCollection)
            {
                if (parameter is BracketParameter || parameter is ReferenceParameter)
                {
                    result.Add(GenerateExport(parameter, simulationToPlot, context));
                }
                else
                {
                    string expressionName = parameter.Image;
                    var expressionNames = context.Evaluator.GetExpressionNames();

                    if (expressionNames.Contains(expressionName))
                    {
                        result.Add(new ExpressionExport(simulationToPlot.Name.ToString(), expressionName, context.Evaluator.GetExpression(expressionName), context.Evaluator, simulationToPlot));
                    }
                }
            }

            return result;
        }
    }
}
