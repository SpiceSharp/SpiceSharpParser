using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Prints;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Common;

    /// <summary>
    /// Reads .PRINT <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class PrintControl : ExportControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrintControl"/> class.
        /// </summary>
        /// <param name="mapper">The exporter mapper</param>
        public PrintControl(IMapper<Exporter> mapper, IExportFactory exportFactory)
            : base(mapper, exportFactory)
        {
        }

        protected ICollection<string> SupportedPrintTypes { get; } = new List<string>() { "dc", "ac", "tran", "op" };

        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modify</param>
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

            string type = statement.Parameters.Count > 0 ? statement.Parameters[0].Image.ToLower() : null;

            if (type != null && SupportedPrintTypes.Contains(type))
            {
                foreach (var simulation in context.Result.Simulations)
                {
                    if (type == "dc" && simulation is DC)
                    {
                        CreatePrint(statement.Parameters.Skip(1), context, simulation, "Voltage (V)");
                    }

                    if (type == "tran" && simulation is Transient)
                    {
                        CreatePrint(statement.Parameters.Skip(1), context, simulation, "Time (s)");
                    }

                    if (type == "ac" && simulation is AC)
                    {
                        CreatePrint(statement.Parameters.Skip(1), context, simulation, "Frequency (Hz)");
                    }

                    if (type == "op" && simulation is OP)
                    {
                        CreatePrint(statement.Parameters.Skip(1), context, simulation, "Final result");
                    }
                }
            }
            else
            {
                foreach (var simulation in context.Result.Simulations)
                {
                    if (simulation is DC)
                    {
                        CreatePrint(statement.Parameters, context, simulation, "Voltage (V)");
                    }

                    if (simulation is Transient)
                    {
                        CreatePrint(statement.Parameters, context, simulation, "Time (s)");
                    }

                    if (simulation is AC)
                    {
                        CreatePrint(statement.Parameters, context, simulation, "Frequency (Hz)");
                    }

                    if (simulation is OP)
                    {
                        CreatePrint(statement.Parameters, context, simulation, null);
                    }
                }
            }
        }

        private List<Export> CreateExportsForAllVoltageAndCurrents(Simulation simulation, IReadingContext context)
        {
            var result = new List<Export>();
            context.Result.Circuit.Entities.BuildOrderedComponentList(); // TODO: Verify with Sven

            var nodes = new List<string>();

            foreach (Entity entity in context.Result.Circuit.Entities)
            {
                if (entity is SpiceSharp.Components.Component c)
                {
                    string componentName = c.Name;
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
                    result.Add(
                        Mapper
                        .GetValue("I", true)
                        .CreateExport(
                            "I(" + componentName + ")",
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
                @params.Add(new WordParameter(node));

                result.Add(
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

            return result;
        }

        private void CreatePrint(ParameterCollection parameters, IReadingContext context, Simulation simulation, string firstColumnName)
        {
            var print = new Print(simulation.Name.ToString());

            if (firstColumnName != null)
            {
                print.ColumnNames.Add(firstColumnName);
            }

            List<Export> exports = GenerateExports(parameters, simulation, context);
            for (var i = 0; i < exports.Count; i++)
            {
                print.ColumnNames.Add(exports[i].Name);
            }

            int index = 1;

            simulation.ExportSimulationData += (object sender, ExportDataEventArgs e) =>
            {
                Row row = new Row(index);
                index++;

                double x = 0;

                if (simulation is Transient)
                {
                    x = e.Time;
                }

                if (simulation is AC)
                {
                    x = e.Frequency;
                }

                if (simulation is DC dc)
                {
                    if (dc.Sweeps.Count > 1)
                    {
                        // TODO: Add support for DC Sweeps > 1
                        throw new GeneralReaderException(".print doesn't support sweep count > 1");
                    }

                    x = e.SweepValue;
                }

                if (!(simulation is OP))
                {
                    row.Columns.Add(x);
                }

                for (var i = 0; i < exports.Count; i++)
                {
                    try
                    {
                        double val = exports[i].Extract();
                        row.Columns.Add(val);
                    }
                    catch (Exception)
                    {
                        row.Columns.Add(double.NaN);
                    }
                }

                print.Rows.Add(row);
            };

            context.Result.AddPrint(print);
        }

        private List<Export> GenerateExports(ParameterCollection parameterCollection, Simulation simulation, IReadingContext context)
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
                    var evaluator = context.SimulutionEvaluators.GetEvaluator(simulation);
                    var expressionNames = context.ReadingExpressionContext.GetExpressionNames();

                    if (expressionNames.Contains(expressionName))
                    {
                        var export = new ExpressionExport(
                            simulation.Name,
                            expressionName,
                            context.ReadingExpressionContext.GetExpression(expressionName),
                            evaluator,
                            context.SimulationExpressionContexts,
                            simulation);

                        export.Extract();
                        result.Add(export);
                    }
                }
            }

            return result;
        }
    }
}
