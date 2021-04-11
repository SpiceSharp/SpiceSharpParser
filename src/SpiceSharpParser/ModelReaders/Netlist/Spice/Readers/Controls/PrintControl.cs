using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Prints;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .PRINT <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class PrintControl : ExportControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrintControl"/> class.
        /// </summary>
        /// <param name="mapper">The exporter mapper.</param>
        /// <param name="exportFactory">The export factory.</param>
        public PrintControl(
            IMapper<Exporter> mapper,
            IExportFactory exportFactory)
            : base(mapper, exportFactory)
        {
        }

        protected ICollection<string> SupportedPrintTypes { get; } = new List<string>() { "dc", "ac", "tran", "op" };

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
            string printImage = statement.Name + ":" + statement.Parameters.ToString();
            if (type != null && SupportedPrintTypes.Contains(type))
            {
                foreach (Simulation simulation in FilterSimulations(context.Result.Simulations, type))
                {
                    string firstColumnName = GetFirstDimensionLabel(simulation);
                    CreatePrint(printImage, statement.Parameters.Skip(1), context, simulation, firstColumnName, true);
                }
            }
            else
            {
                foreach (Simulation simulation in context.Result.Simulations)
                {
                    string firstColumnName = GetFirstDimensionLabel(simulation);
                    CreatePrint(printImage, statement.Parameters, context, simulation, firstColumnName, false);
                }
            }
        }

        private static void CreateRowInPrint(ref int rowIndex, Simulation simulation, ICircuitContext context, ExportDataEventArgs eventArgs, List<Export> exports, Print print)
        {
            Row row = new Row(rowIndex++);

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
                if (eventArgs.GetSweepValues().Length > 1)
                {
                    // TODO: Add support for DC Sweeps > 1
                    context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, ".PRINT doesn't support sweep count > 1"));
                    return;
                }

                x = eventArgs.GetSweepValues().FirstOrDefault();
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
        }

        private IEnumerable<Simulation> FilterSimulations(IEnumerable<Simulation> simulations, string type)
        {
            var typeLowered = type.ToLower();

            foreach (var simulation in simulations)
            {
                if ((simulation is DC && typeLowered == "dc")
                    || (simulation is Transient && typeLowered == "tran")
                    || (simulation is AC && typeLowered == "ac")
                    || (simulation is OP && typeLowered == "op"))
                {
                    yield return simulation;
                }
            }
        }

        private void CreatePrint(string printImage, ParameterCollection parameters, ICircuitContext context, Simulation simulation, string firstColumnName, bool filterSpecified)
        {
            var print = new Print(simulation.Name.ToString());

            // Create column names
            if (!string.IsNullOrEmpty(firstColumnName))
            {
                print.ColumnNames.Add(firstColumnName);
            }

            List<Export> exports = GenerateExports(parameters, simulation, context);
            for (var i = 0; i < exports.Count; i++)
            {
                print.ColumnNames.Add(exports[i].Name);
            }

            int rowIndex = 0;
            simulation.ExportSimulationData += (sender, args) => CreateRowInPrint(ref rowIndex, simulation, context, args, exports, print);
            simulation.AfterExecute += (sender, args) => AddPrintToResultIfValid(printImage, context, print, simulation, filterSpecified);
        }

        private void AddPrintToResultIfValid(string printImage, ICircuitContext context, Print print, Simulation simulation, bool filterSpecified)
        {
            if (!filterSpecified)
            {
                RemoveNaNColumns(print);

                if (print.Rows.Count == 0 || print.Rows[0].Columns.Count == (simulation is OP ? 0 : 1))
                {
                    context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, $"{printImage} is not valid for: {simulation.Name}"));
                }
                else
                {
                    context.Result.AddPrint(print);
                }
            }
            else
            {
                context.Result.AddPrint(print);
            }
        }

        private void RemoveNaNColumns(Print print)
        {
            for (var columnIndex = print.ColumnNames.Count - 1; columnIndex >= 0; columnIndex--)
            {
                if (print.Rows.All(row => double.IsNaN(row.Columns[columnIndex])))
                {
                    foreach (var row in print.Rows)
                    {
                        row.Columns.RemoveAt(columnIndex);
                    }

                    print.ColumnNames.RemoveAt(columnIndex);
                }
            }
        }
    }
}