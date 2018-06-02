using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.Model.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.ModelReader.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.ModelReader.Netlist.Spice.Registries;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Readers.Controls
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
                            context.Result.AddExport(Registry.Get("i").CreateExport("i", @params, simulation, context));
                        }
                    }

                    foreach (var node in nodes)
                    {
                        var @params = new ParameterCollection();
                        @params.Add(new WordParameter(node.ToString()));

                        context.Result.AddExport(Registry.Get("v").CreateExport("v", @params, simulation, context));
                    }
                }
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
        }

        private void AddCommonExport(IReadingContext context, Type simulationType, Model.Netlist.Spice.Objects.Parameter parameter)
        {
            foreach (var simulation in Filter(context.Result.Simulations, simulationType))
            {
                context.Result.AddExport(GenerateExport(parameter, simulation, context));
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
                    context.Result.AddExport(
                        new ExpressionExport(
                            simulation.Name.ToString(),
                            expressionName,
                            context.Evaluator.GetExpression(expressionName),
                            context.Evaluator,
                            simulation
                    ));
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
