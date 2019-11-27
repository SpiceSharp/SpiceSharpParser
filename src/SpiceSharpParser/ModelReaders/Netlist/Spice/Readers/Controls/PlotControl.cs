using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Plots;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .PLOT <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class PlotControl : ExportControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlotControl"/> class.
        /// </summary>
        /// <param name="mapper">The exporter mapper.</param>
        /// <param name="exportFactory">The export factory.</param>
        public PlotControl(
            IMapper<Exporter> mapper,
            IExportFactory exportFactory)
            : base(mapper, exportFactory)
        {
        }

        protected ICollection<string> SupportedPlotTypes { get; } = new List<string>() { "dc", "ac", "tran" };

        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A reading context.</param>
        public override void Read(Control statement, ICircuitContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            string type = statement.Parameters.Count > 0 ? statement.Parameters[0].Image.ToLower() : null;
            string plotImage = statement.Name + ":" + statement.Parameters;
            bool merge = statement.Parameters.Any(p => p.Image == "merge");

            if (merge)
            {
                if (type != null && SupportedPlotTypes.Contains(type))
                {
                    CreatePlot(plotImage, statement.Parameters.Skip(1), context, FilterSimulations(context.Result.Simulations, type), true);
                }
                else
                {
                    CreatePlot(plotImage, statement.Parameters, context, context.Result.Simulations, false);
                }
            }
            else
            {
                if (type != null && SupportedPlotTypes.Contains(type))
                {
                    foreach (var simulation in FilterSimulations(context.Result.Simulations, type))
                    {
                        CreatePlot(plotImage, statement.Parameters.Skip(1), context, simulation, true);
                    }
                }
                else
                {
                    foreach (var simulation in context.Result.Simulations)
                    {
                        if (simulation is OP)
                        {
                            continue;
                        }

                        CreatePlot(plotImage, statement.Parameters, context, simulation, false);
                    }
                }
            }
        }

        private IEnumerable<Simulation> FilterSimulations(IEnumerable<Simulation> simulations, string type)
        {
            var typeLowered = type.ToLower();

            foreach (var simulation in simulations)
            {
                if ((simulation is DC && typeLowered == "dc")
                    || (simulation is Transient && typeLowered == "tran")
                    || (simulation is AC && typeLowered == "ac"))
                {
                    yield return simulation;
                }
            }
        }

        private void CreatePlot(string plotImage, ParameterCollection parameters, ICircuitContext context, Simulation simulation, bool filterSpecified)
        {
            var plot = new XyPlot(simulation.Name);
            List<Export> exports = GenerateExports(parameters, simulation, context);

            for (int i = 0; i < exports.Count; i++)
            {
                Export export = exports[i];
                plot.Series.Add(new Series(export.Name));
            }

            simulation.ExportSimulationData += (sender, args) => CreatePointForSeries(simulation, args, exports, plot.Series, plot);
            simulation.AfterExecute += (sender, args) => AddPlotToResultIfValid(plotImage, context, plot, simulation, filterSpecified);
        }

        private void CreatePlot(string plotImage, ParameterCollection parameters, ICircuitContext context, IEnumerable<Simulation> simulations, bool filterSpecified)
        {
            var plot = new XyPlot($"Merged: {plotImage}");
            foreach (var simulation in simulations)
            {
                List<Export> exports = GenerateExports(parameters, simulation, context);

                List<Series> series = new List<Series>();
                for (int i = 0; i < exports.Count; i++)
                {
                    Export export = exports[i];
                    series.Add(new Series($"{simulation.Name} {export.Name}"));
                }

                plot.Series.AddRange(series);
                simulation.ExportSimulationData += (sender, args) => CreatePointForSeries(simulation, args, exports, series, plot);
            }

            context.Result.AddPlot(plot);
        }

        private void AddPlotToResultIfValid(string plotImage, ICircuitContext context, XyPlot plot, Simulation simulation, bool filterSpecified)
        {
            for (int i = plot.Series.Count - 1; i >= 0; i--)
            {
                Series series = plot.Series[i];
                if (series.Points.Count == 0)
                {
                    plot.Series.RemoveAt(i);
                }
            }

            if (plot.Series.Count > 0)
            {
                context.Result.AddPlot(plot);
            }
            else
            {
                context.Result.AddWarning($"{plotImage} is not valid for: {simulation.Name}");
            }
        }

        private void CreatePointForSeries(Simulation simulation, ExportDataEventArgs eventArgs, List<Export> exports, List<Series> series, XyPlot plot)
        {
            double x = 0;

            if (simulation is Transient)
            {
                x = eventArgs.Time;
            }

            if (simulation is AC)
            {
                x = eventArgs.Frequency;
            }

            if (simulation is DC dc)
            {
                if (dc.Sweeps.Count > 1)
                {
                    // TODO: Add support for DC Sweeps > 1
                    throw new ReadingException(".print doesn't support sweep count > 1");
                }

                x = eventArgs.SweepValue;
            }

            for (var i = 0; i < exports.Count; i++)
            {
                try
                {
                    double val = exports[i].Extract();
                    if (!double.IsNaN(val))
                    {
                        series[i].Points.Add(new Point() { X = x, Y = val });
                    }
                }
                catch (Exception)
                {
                    // ignore exception
                }
            }
        }
    }
}