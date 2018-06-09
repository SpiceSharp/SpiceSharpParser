using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls.Plots;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Registries;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reades .SAVE <see cref="Control"/> from spice netlist object model.
    /// </summary>
    public class SaveControl : ExportControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SaveControl"/> class.
        /// </summary>
        /// <param name="registry">The exporter registry</param>
        public SaveControl(IExporterRegistry registry) 
            : base(registry)
        {
        }

        /// <summary>
        /// Gets the type of generator.
        /// </summary>
        public override string SpiceName => "save";

        /// <summary>
        /// Reades <see cref="Control"/> statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modify</param>
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

            CreatePlotsForParameterSweeps(context);
        }

        private void CreatePlotsForParameterSweeps(IReadingContext context)
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

                //3. Group them by name (name contains exported variable)
                var groups = opExports.GroupBy(export => export.Name);

                foreach (var group in groups)
                {
                    string variableName = group.Key;
                    var exports = group.ToList();

                    CreateSweepPlot(firstParameterSweep, variableName, exports, context);
                }
            }
        }

        private void CreateSweepPlot(ParameterSweep firstParameterSweep, string variableName, List<Export> exports, IReadingContext context)
        {
            var plot = new Plot("OP - Parameter sweep: " + variableName);

            for (var i = 0; i < exports.Count; i++)
            {
                var series = new Series(exports[i].Simulation.Name.ToString()) { XUnit = firstParameterSweep.Parameter.Image, YUnit = exports[i].QuantityUnit };
                AddPointsToSeries(firstParameterSweep, exports[i], context, series);

                plot.Series.Add(series);
            }

            context.Result.AddPlot(plot);
        }

        private static void AddPointsToSeries(ParameterSweep firstParameterSweep, Export export, IReadingContext context, Series series)
        {
            export.Simulation.OnExportSimulationData += (object sender, ExportDataEventArgs e) =>
            {
                var firstParameterSweepValue = context.Evaluator.GetParameterValue(firstParameterSweep.Parameter.Image, sender);
                series.Points.Add(new Point() { X = firstParameterSweepValue, Y = export.Extract() });
            };
        }

        private void CreateExportsForAllVoltageAndCurrents(IReadingContext context)
        {
            context.Result.Circuit.Objects.BuildOrderedComponentList(); //TODO: Verify with Sven

            // For all simulations add exports for current and voltages
            foreach (var simulation in context.Result.Simulations)
            {
                var nodes = new List<Identifier>();

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
                        context.Result.AddExport(Registry.Get("i").CreateExport("i", @params, simulation, context.NodeNameGenerator, context.ObjectNameGenerator));
                    }
                }

                foreach (var node in nodes)
                {
                    var @params = new ParameterCollection();
                    @params.Add(new WordParameter(node.ToString()));

                    context.Result.AddExport(Registry.Get("v").CreateExport("v", @params, simulation, context.NodeNameGenerator, context.ObjectNameGenerator));
                }
            }
        }

        private void AddCommonExport(IReadingContext context, Type simulationType, Models.Netlist.Spice.Objects.Parameter parameter)
        {
            foreach (var simulation in Filter(context.Result.Simulations, simulationType))
            {
                context.Result.AddExport(GenerateExport(parameter, simulation, context.NodeNameGenerator, context.ObjectNameGenerator));
            }
        }

        private void AddLetExport(IReadingContext context, Type simulationType, SingleParameter s)
        {
            string expressionName = s.Image;
            var expressionNames = context.Evaluator.GetExpressionNames();

            if (expressionNames.Contains(expressionName))
            {
                var simulations = Filter(context.Result.Simulations, simulationType);
                foreach (var simulation in simulations)
                {
                    var export = new ExpressionExport(
                            simulation.Name.ToString(),
                            expressionName,
                            context.Evaluator.GetExpression(expressionName),
                            context.Evaluator,
                            simulation
                    );
                    context.Result.AddExport(export);
                }
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
