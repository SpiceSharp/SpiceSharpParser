using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Plots;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Common;

    /// <summary>
    /// Reads .SAVE <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class SaveControl : ExportControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SaveControl"/> class.
        /// </summary>
        /// <param name="mapper">The exporter mapper.</param>
        public SaveControl(IMapper<Exporter> mapper, IExportFactory factory)
            : base(mapper, factory)
        {
        }

        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public override void Read(Control statement, IReadingContext context)
        {
            Type simulationType = null;

            if (statement.Parameters.Count == 0)
            {
                CreateExportsForAllVoltageAndCurrents(context);
            }

            for (var i = 0; i < statement.Parameters.Count; i++)
            {
                var parameter = statement.Parameters[i];

                if (i == 0)
                {
                    switch (parameter.Image.ToLower())
                    {
                        case "op":
                            simulationType = typeof(OP);
                            break;
                        case "tran":
                            simulationType = typeof(Transient);
                            break;
                        case "ac":
                            simulationType = typeof(AC);
                            break;
                        case "dc":
                            simulationType = typeof(DC);
                            break;
                    }
                }

                if (parameter is BracketParameter || parameter is ReferenceParameter)
                {
                    AddCommonExport(context, simulationType, parameter);
                }
                else if ((i != 0 || (i == 0 && simulationType == null)) && parameter is SingleParameter s)
                {
                    AddLetExport(context, simulationType, s);
                }
            }

            CreatePlotsForOpParameterSweeps(context);
            CreatePlotsForTranParameterSweeps(context);
            CreatePlotsForAcParameterSweeps(context);
        }

        private void CreatePlotsForTranParameterSweeps(IReadingContext context)
        {
            if (context.Result.SimulationConfiguration.ParameterSweeps.Count > 0)
            {
                // 2. Find all .TRAN exports
                List<Export> tranExports = new List<Export>();
                foreach (var export in context.Result.Exports)
                {
                    if (export.Simulation is Transient)
                    {
                        tranExports.Add(export);
                    }
                }

                // 3. Group them by name (name contains exported variable)
                var groups = tranExports.GroupBy(export => export.Name);

                foreach (var group in groups)
                {
                    string variableName = group.Key;
                    var exports = group.ToList();

                    CreateTranSweepPlot(variableName, exports, context);
                }
            }
        }

        private void CreatePlotsForAcParameterSweeps(IReadingContext context)
        {
            if (context.Result.SimulationConfiguration.ParameterSweeps.Count > 0)
            {
                // 2. Find all .AC exports
                List<Export> acExports = new List<Export>();
                foreach (var export in context.Result.Exports)
                {
                    if (export.Simulation is AC)
                    {
                        acExports.Add(export);
                    }
                }

                // 3. Group them by name (name contains exported variable)
                var groups = acExports.GroupBy(export => export.Name);

                foreach (var group in groups)
                {
                    string variableName = group.Key;
                    var exports = group.ToList();

                    CreateAcSweepPlot(variableName, exports, context);
                }
            }
        }

        private void CreatePlotsForOpParameterSweeps(IReadingContext context)
        {
            if (context.Result.SimulationConfiguration.ParameterSweeps.Count > 0)
            {
                // 1. Find first parametr sweep (it will decide about X-axis scale)
                var firstParameterSweep = context.Result.SimulationConfiguration.ParameterSweeps[0];

                // 2. Find all .OP exports
                List<Export> opExports = new List<Export>();
                foreach (var export in context.Result.Exports)
                {
                    if (export.Simulation is OP)
                    {
                        opExports.Add(export);
                    }
                }

                // 3. Group them by name (name contains exported variable)
                var groups = opExports.GroupBy(export => export.Name);

                foreach (var group in groups)
                {
                    string variableName = group.Key;
                    var exports = group.ToList();

                    CreateOpSweepPlot(firstParameterSweep, variableName, exports, context);
                }
            }
        }

        private void CreateOpSweepPlot(ParameterSweep firstParameterSweep, string variableName, List<Export> exports, IReadingContext context)
        {
            var plot = new XyPlot("OP - Parameter sweep: " + variableName);

            for (var i = 0; i < exports.Count; i++)
            {
                var series = new Series(exports[i].Simulation.Name.ToString()) { XUnit = firstParameterSweep.Parameter.Image, YUnit = exports[i].QuantityUnit };
                AddOpPointToSeries(firstParameterSweep, exports[i], context, series);

                plot.Series.Add(series);
            }

            context.Result.AddPlot(plot);
        }

        private void CreateTranSweepPlot(string variableName, List<Export> exports, IReadingContext context)
        {
            var plot = new XyPlot("Tran - Parameter sweep: " + variableName);

            for (var i = 0; i < exports.Count; i++)
            {
                var series = new Series(exports[i].Simulation.Name.ToString()) { XUnit = "Time (s)", YUnit = exports[i].QuantityUnit };
                AddTranPointsToSeries(exports[i], series);

                plot.Series.Add(series);
            }

            context.Result.AddPlot(plot);
        }

        private void CreateAcSweepPlot(string variableName, List<Export> exports, IReadingContext context)
        {
            var plot = new XyPlot("AC - Parameter sweep: " + variableName);

            for (var i = 0; i < exports.Count; i++)
            {
                var series = new Series(exports[i].Simulation.Name.ToString()) { XUnit = "Freq (s)", YUnit = exports[i].QuantityUnit };
                AddAcPointsToSeries(exports[i], series);

                plot.Series.Add(series);
            }

            context.Result.AddPlot(plot);
        }

        private void AddOpPointToSeries(ParameterSweep firstParameterSweep, Export export, IReadingContext context, Series series)
        {
            export.Simulation.ExportSimulationData += (object sender, ExportDataEventArgs e) =>
            {
                var firstParameterSweepValue = context.Evaluators.GetSimulationEvaluator(export.Simulation).GetParameterValue(firstParameterSweep.Parameter.Image);
                series.Points.Add(new Point() { X = firstParameterSweepValue, Y = export.Extract() });
            };
        }

        private void AddTranPointsToSeries(Export export, Series series)
        {
            export.Simulation.ExportSimulationData += (object sender, ExportDataEventArgs e) =>
            {
                series.Points.Add(new Point() { X = e.Time, Y = export.Extract() });
            };
        }

        private void AddAcPointsToSeries(Export export, Series series)
        {
            export.Simulation.ExportSimulationData += (object sender, ExportDataEventArgs e) =>
            {
                series.Points.Add(new Point() { X = e.Frequency, Y = export.Extract() });
            };
        }

        private void CreateExportsForAllVoltageAndCurrents(IReadingContext context)
        {
            context.Result.Circuit.Objects.BuildOrderedComponentList(); // TODO: Verify with Sven

            // For all simulations add exports for current and voltages
            foreach (var simulation in context.Result.Simulations)
            {
                var nodes = new List<string>();

                foreach (Entity entity in context.Result.Circuit.Objects)
                {
                    if (entity is SpiceSharp.Components.Component c)
                    {
                        string componentName = c.Name.ToString();
                        var @params = new ParameterCollection();
                        @params.Add(new WordParameter(componentName));

                        for (var i = 0; i < c.PinCount; i++)
                        {
                            var node = c.GetNode(i);
                            if (!nodes.Contains(node))
                            {
                                nodes.Add(node);
                            }
                        }

                        // Add current export for component
                        context.Result.AddExport(
                            Mapper
                            .GetValue("I", true)
                            .CreateExport(
                                "I(" + entity.Name + ")",
                                "i",
                                @params,
                                simulation,
                                context.NodeNameGenerator,
                                context.ComponentNameGenerator,
                                context.ModelNameGenerator,
                                context.Result,
                                context.CaseSensitivity));
                    }
                }

                foreach (var node in nodes)
                {
                    var @params = new ParameterCollection();
                    @params.Add(new WordParameter(node.ToString()));

                    context.Result.AddExport(
                        Mapper
                        .GetValue("V", true)
                        .CreateExport(
                            "V(" + node + ")",
                            "v",
                            @params,
                            simulation,
                            context.NodeNameGenerator,
                            context.ComponentNameGenerator,
                            context.ModelNameGenerator,
                            context.Result,
                            context.CaseSensitivity));
                }
            }
        }

        private void AddCommonExport(IReadingContext context, Type simulationType, Parameter parameter)
        {
            foreach (var simulation in Filter(context.Result.Simulations, simulationType))
            {
                context.Result.AddExport(GenerateExport(parameter, context, simulation));
            }
        }

        private void AddLetExport(IReadingContext context, Type simulationType, SingleParameter s)
        {
            string expressionName = s.Image;
            var expressionNames = context.Evaluators.GetExpressionNames();

            if (expressionNames.Contains(expressionName))
            {
                var simulations = Filter(context.Result.Simulations, simulationType);
                foreach (var simulation in simulations)
                {
                    var evaluator = context.Evaluators.GetSimulationEvaluator(simulation);
                    var export = new ExpressionExport(
                            simulation.Name,
                            expressionName,
                            evaluator.GetExpression(expressionName),
                            evaluator,
                            simulation);

                    context.Result.AddExport(export);
                }
            }
            else
            {
                throw new Exception("There is no " + expressionName + " expression");
            }
        }

        private IEnumerable<Simulation> Filter(IEnumerable<Simulation> simulations, Type simulationType)
        {
            if (simulationType == null)
            {
                return simulations;
            }

            return simulations.Where(simulation => simulation.GetType().GetTypeInfo().IsSubclassOf(simulationType) || simulation.GetType() == simulationType);
        }
    }
}
