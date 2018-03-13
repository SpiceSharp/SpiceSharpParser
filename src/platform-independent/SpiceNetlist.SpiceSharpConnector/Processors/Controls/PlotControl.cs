using System.Collections.Generic;
using System.Linq;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Plots;
using SpiceNetlist.SpiceSharpConnector.Registries;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls
{
    /// <summary>
    /// Processes .PLOT <see cref="Control"/> from spice netlist object model.
    /// It supports DC, AC, TRAN type of .PLOT
    /// </summary>
    public class PlotControl : BaseControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlotControl"/> class.
        /// </summary>
        /// <param name="registry">The exporter registry</param>
        public PlotControl(IExporterRegistry registry)
        {
            Registry = registry;
        }

        /// <summary>
        /// Gets the type of genera
        /// </summary>
        public override string TypeName => "plot";

        /// <summary>
        /// Gets the registry
        /// </summary>
        protected IExporterRegistry Registry { get; }

        /// <summary>
        /// Processes <see cref="Control"/> statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modify</param>
        public override void Process(Control statement, ProcessingContext context)
        {
            string type = statement.Parameters[0].Image.ToLower();

            foreach (var simulation in context.Simulations)
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

        private void CreatePlot(Control statement, ProcessingContext context, Simulation simulationToPlot, string xUnit)
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
                        throw new System.Exception("DC Sweeps > 1");
                    }

                    x = e.SweepValue;
                }

                for (var i = 0; i < exports.Count; i++)
                {
                    plot.Series[i].Points.Add(new Point() { X = x, Y = exports[i].Extract() });
                }
            };

            context.AddPlot(plot);
        }

        private List<Export> GenerateExports(ParameterCollection parameterCollection, Simulation simulationToPlot, ProcessingContext context)
        {
            List<Export> result = new List<Export>();
            foreach (var parameter in parameterCollection)
            {
                if (parameter is BracketParameter bp)
                {
                    result.Add(GenerateExport(bp, simulationToPlot, context));
                }
            }

            return result;
        }

        private Export GenerateExport(BracketParameter parameter, Simulation simulation, ProcessingContext context)
        {
            string type = parameter.Name.ToLower();

            if (Registry.Supports(type))
            {
                return Registry.Get(type).CreateExport(type, parameter.Parameters, simulation, context);
            }

            throw new System.Exception("Unsuported save");
        }
    }
}
