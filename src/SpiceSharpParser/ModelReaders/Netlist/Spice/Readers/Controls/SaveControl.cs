using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Plots;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .SAVE <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class SaveControl : ExportControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SaveControl"/> class.
        /// </summary>
        /// <param name="mapper">The exporter mapper.</param>
        /// <param name="factory">Export factory.</param>
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
                    switch (parameter.Value.ToLower())
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
            if (context.SimulationConfiguration.ParameterSweeps.Count > 0)
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
            if (context.SimulationConfiguration.ParameterSweeps.Count > 0)
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
            if (context.SimulationConfiguration.ParameterSweeps.Count > 0)
            {
                // 1. Find first parameter sweep (it will decide about X-axis scale)
                var firstParameterSweep = context.SimulationConfiguration.ParameterSweeps[0];

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

        private void CreateOpSweepPlot(Context.Sweeps.ParameterSweep firstParameterSweep, string variableName, List<Export> exports, IReadingContext context)
        {
            var plot = new XyPlot("OP - Parameter sweep: " + variableName);

            foreach (var export in exports)
            {
                var series = new Series(export.Simulation.Name)
                {
                    XUnit = firstParameterSweep.Parameter.Value,
                    YUnit = export.QuantityUnit,
                };
                AddOpPointToSeries(firstParameterSweep, export, context, series);

                plot.Series.Add(series);
            }

            context.Result.XyPlots.Add(plot);
        }

        private void CreateTranSweepPlot(string variableName, List<Export> exports, IReadingContext context)
        {
            var plot = new XyPlot("Tran - Parameter sweep: " + variableName);

            foreach (var export in exports)
            {
                var series = new Series(export.Simulation.Name)
                {
                    XUnit = "Time (parameter)",
                    YUnit = export.QuantityUnit,
                };
                AddTranPointsToSeries(export, series);

                plot.Series.Add(series);
            }

            context.Result.XyPlots.Add(plot);
        }

        private void CreateAcSweepPlot(string variableName, List<Export> exports, IReadingContext context)
        {
            var plot = new XyPlot("AC - Parameter sweep: " + variableName);

            foreach (var export in exports)
            {
                var series = new Series(export.Simulation.Name)
                {
                    XUnit = "Freq (parameter)",
                    YUnit = export.QuantityUnit,
                };
                AddAcPointsToSeries(export, series);

                plot.Series.Add(series);
            }

            context.Result.XyPlots.Add(plot);
        }

        private void AddOpPointToSeries(Context.Sweeps.ParameterSweep firstParameterSweep, Export export, IReadingContext context, Series series)
        {
            export.Simulation.ExportSimulationData += (object sender, ExportDataEventArgs e) =>
            {
                var expressionContext = context.Evaluator.GetEvaluationContext(export.Simulation);
                var firstParameterSweepParameter = expressionContext.Parameters[firstParameterSweep.Parameter.Value];

                var value = context.Evaluator.GetEvaluationContext(export.Simulation).Evaluate(firstParameterSweepParameter);
                series.Points.Add(new Point() { X = value, Y = export.Extract() });
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
            // For all simulations add exports for current and voltages
            foreach (var simulation in context.Result.Simulations)
            {
                var nodes = new List<string>();

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
                        context.Result.Exports.Add(
                            Mapper
                            .GetValue("I", true)
                            .CreateExport(
                                "I(" + entity.Name + ")",
                                "i",
                                @params,
                                context.Evaluator.GetEvaluationContext(simulation),
                                context.ReaderSettings.CaseSensitivity));
                    }
                }

                foreach (var node in nodes)
                {
                    var @params = new ParameterCollection(new List<Parameter>());
                    @params.Add(new WordParameter(node, null));

                    context.Result.Exports.Add(
                        Mapper
                        .GetValue("V", true)
                        .CreateExport(
                            "V(" + node + ")",
                            "v",
                            @params,
                            context.Evaluator.GetEvaluationContext(simulation),
                            context.ReaderSettings.CaseSensitivity));
                }
            }
        }

        private void AddCommonExport(IReadingContext context, Type simulationType, Parameter parameter)
        {
            foreach (var simulation in Filter(context.Result.Simulations, simulationType))
            {
                context.Result.Exports.Add(GenerateExport(parameter, context, simulation));
            }
        }

        private void AddLetExport(IReadingContext context, Type simulationType, SingleParameter parameter)
        {
            string expressionName = parameter.Value;
            var expressionNames = context.Evaluator.GetExpressionNames();

            if (expressionNames.Contains(expressionName))
            {
                var simulations = Filter(context.Result.Simulations, simulationType);
                foreach (var simulation in simulations)
                {
                    var export = new ExpressionExport(simulation.Name, expressionName, context.Evaluator.GetEvaluationContext(simulation));
                    context.Result.Exports.Add(export);
                }
            }
            else
            {
                context.Result.ValidationResult.Add(
                    new ValidationEntry(
                        ValidationEntrySource.Reader,
                        ValidationEntryLevel.Warning,
                        $"There is no {expressionName} expression",
                        parameter.LineInfo));
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