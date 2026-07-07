using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;

namespace SpiceSharpParser.Testing
{
    /// <summary>
    /// Shared simulation runners for tests that use prepared SpiceSharpParser models.
    /// </summary>
    public static class SpiceSimulationTestHelper
    {
        /// <summary>
        /// Runs every simulation in the model.
        /// </summary>
        /// <param name="model">The parsed SpiceSharp model.</param>
        public static void RunSimulations(SpiceSharpModel model)
        {
            foreach (var simulation in model.Simulations)
            {
                Evaluate(simulation.InvokeEvents(simulation.Run(model.Circuit, -1)));
            }
        }

        /// <summary>
        /// Runs every simulation and returns captured export values grouped by export.
        /// </summary>
        /// <param name="model">The parsed SpiceSharp model.</param>
        /// <returns>A list of export result buckets.</returns>
        public static List<object> RunSimulationsAndReturnExports(SpiceSharpModel model)
        {
            var result = new List<object>();

            foreach (var export in model.Exports)
            {
                var simulation = export.Simulation;
                if (simulation is DC)
                {
                    var dcResult = new List<double>();
                    result.Add(dcResult);
                    simulation.EventExportData += (sender, e) => dcResult.Add(export.Extract());
                }

                if (simulation is OP)
                {
                    simulation.EventExportData += (sender, e) => result.Add(export.Extract());
                }

                if (simulation is AC ac)
                {
                    var acResult = new List<Tuple<double, double>>();
                    result.Add(acResult);
                    simulation.EventExportData += (sender, e) =>
                    {
                        acResult.Add(new Tuple<double, double>(ac.Frequency, export.Extract()));
                    };
                }

                if (simulation is Transient transient)
                {
                    var tranResult = new List<Tuple<double, double>>();
                    result.Add(tranResult);
                    simulation.EventExportData += (sender, e) =>
                    {
                        tranResult.Add(new Tuple<double, double>(transient.Time, export.Extract()));
                    };
                }
            }

            RunSimulations(model);
            return result;
        }

        /// <summary>
        /// Runs the single OP simulation and returns one named export.
        /// </summary>
        /// <param name="model">The parsed SpiceSharp model.</param>
        /// <param name="exportName">The export name.</param>
        /// <returns>The exported value.</returns>
        public static double RunOp(SpiceSharpModel model, string exportName)
        {
            double result = double.NaN;
            var simulation = model.Simulations.Single();
            var export = FindExport(model, simulation, exportName);

            simulation.EventExportData += (sender, e) => result = export.Extract();
            Evaluate(simulation.InvokeEvents(simulation.Run(model.Circuit, -1)));

            return result;
        }

        /// <summary>
        /// Runs the single OP simulation and returns multiple named exports.
        /// </summary>
        /// <param name="model">The parsed SpiceSharp model.</param>
        /// <param name="exportNames">The export names.</param>
        /// <returns>The exported values in the requested order.</returns>
        public static double[] RunOp(SpiceSharpModel model, params string[] exportNames)
        {
            var simulation = model.Simulations.Single();
            var exports = exportNames.Select(exportName => FindExport(model, simulation, exportName)).ToArray();
            var result = new double[exports.Length];

            simulation.EventExportData += (sender, e) =>
            {
                for (var i = 0; i < exports.Length; i++)
                {
                    result[i] = exports[i].Extract();
                }
            };

            Evaluate(simulation.InvokeEvents(simulation.Run(model.Circuit, -1)));
            return result;
        }

        /// <summary>
        /// Runs the first OP simulation and returns all model exports.
        /// </summary>
        /// <param name="model">The parsed SpiceSharp model.</param>
        /// <returns>Export names and values.</returns>
        public static Tuple<string, double>[] RunOp(SpiceSharpModel model)
        {
            var simulation = model.Simulations.First(s => s is OP);
            var result = new Tuple<string, double>[model.Exports.Count];

            simulation.EventExportData += (sender, e) =>
            {
                for (var i = 0; i < model.Exports.Count; i++)
                {
                    var export = model.Exports[i];
                    result[i] = new Tuple<string, double>(export.Name, TryExtract(export));
                }
            };

            Evaluate(simulation.InvokeEvents(simulation.Run(model.Circuit, -1)));
            return result;
        }

        /// <summary>
        /// Runs each OP simulation in a stepped model and returns the requested export value per simulation.
        /// </summary>
        /// <param name="model">The parsed SpiceSharp model.</param>
        /// <param name="exportName">The export name.</param>
        /// <returns>The exported values.</returns>
        public static double[] RunOpSweep(SpiceSharpModel model, string exportName)
        {
            return model.Simulations
                .Where(simulation => simulation is OP)
                .Select(simulation => RunScalarExport(model, simulation, exportName, OP.ExportOperatingPoint))
                .ToArray();
        }

        /// <summary>
        /// Runs a transient simulation and captures an export over time.
        /// </summary>
        /// <param name="model">The parsed SpiceSharp model.</param>
        /// <param name="exportName">The export name.</param>
        /// <returns>Time/value samples.</returns>
        public static Tuple<double, double>[] RunTransient(SpiceSharpModel model, string exportName)
        {
            var simulation = model.Simulations.First(s => s is Transient);
            var export = FindExport(model, simulation, exportName);
            var result = new List<Tuple<double, double>>();

            simulation.EventExportData += (sender, e) =>
            {
                result.Add(new Tuple<double, double>(((Transient)simulation).Time, export.Extract()));
            };

            Evaluate(simulation.InvokeEvents(simulation.Run(model.Circuit, -1)));
            return result.ToArray();
        }

        /// <summary>
        /// Runs a transient simulation and captures two exports over time.
        /// </summary>
        /// <param name="model">The parsed SpiceSharp model.</param>
        /// <param name="firstExportName">The first export name.</param>
        /// <param name="secondExportName">The second export name.</param>
        /// <returns>Time/value/value samples.</returns>
        public static Tuple<double, double, double>[] RunTransientPair(
            SpiceSharpModel model,
            string firstExportName,
            string secondExportName)
        {
            var simulation = model.Simulations.First(s => s is Transient);
            var firstExport = FindExport(model, simulation, firstExportName);
            var secondExport = FindExport(model, simulation, secondExportName);
            var result = new List<Tuple<double, double, double>>();

            simulation.EventExportData += (sender, e) =>
            {
                result.Add(new Tuple<double, double, double>(
                    ((Transient)simulation).Time,
                    firstExport.Extract(),
                    secondExport.Extract()));
            };

            Evaluate(simulation.InvokeEvents(simulation.Run(model.Circuit, -1)));
            return result.ToArray();
        }

        /// <summary>
        /// Runs a DC simulation and captures an export over the active sweep value.
        /// </summary>
        /// <param name="model">The parsed SpiceSharp model.</param>
        /// <param name="exportName">The export name.</param>
        /// <returns>Sweep/value samples.</returns>
        public static Tuple<double, double>[] RunDc(SpiceSharpModel model, string exportName)
        {
            var simulation = model.Simulations.First(s => s is DC);
            var export = FindExport(model, simulation, exportName);
            var result = new List<Tuple<double, double>>();

            simulation.EventExportData += (sender, e) =>
            {
                result.Add(new Tuple<double, double>(((DC)simulation).GetCurrentSweepValue().Last(), export.Extract()));
            };

            Evaluate(simulation.InvokeEvents(simulation.Run(model.Circuit, -1)));
            return result.ToArray();
        }

        /// <summary>
        /// Runs an AC simulation and captures an export over frequency.
        /// </summary>
        /// <param name="model">The parsed SpiceSharp model.</param>
        /// <param name="exportName">The export name.</param>
        /// <returns>Frequency/value samples.</returns>
        public static Tuple<double, double>[] RunAc(SpiceSharpModel model, string exportName)
        {
            var simulation = model.Simulations.First(s => s is AC);
            var export = FindExport(model, simulation, exportName);
            var result = new List<Tuple<double, double>>();

            simulation.EventExportData += (sender, e) =>
            {
                result.Add(new Tuple<double, double>(((AC)simulation).Frequency, export.Extract()));
            };

            Evaluate(simulation.InvokeEvents(simulation.Run(model.Circuit, -1)));
            return result.ToArray();
        }

        private static double RunScalarExport(
            SpiceSharpModel model,
            ISimulationWithEvents simulation,
            string exportName,
            int exportCode)
        {
            var export = FindExport(model, simulation, exportName);
            double result = double.NaN;

            foreach (var code in simulation.InvokeEvents(simulation.Run(model.Circuit, -1)))
            {
                if (code == exportCode)
                {
                    result = export.Extract();
                }
            }

            return result;
        }

        private static Export FindExport(SpiceSharpModel model, ISimulationWithEvents simulation, string exportName)
        {
            var export = model.Exports.Find(e => e.Name == exportName && e.Simulation == simulation);
            if (export != null)
            {
                return export;
            }

            export = model.Exports.Find(e => e.Name == exportName && e.Simulation.GetType() == simulation.GetType());
            if (export != null)
            {
                return export;
            }

            throw new InvalidOperationException("Export '" + exportName + "' was not found for simulation type '" + simulation.GetType().Name + "'.");
        }

        private static double TryExtract(Export export)
        {
            try
            {
                return export.Extract();
            }
            catch
            {
                return double.NaN;
            }
        }

        private static void Evaluate(IEnumerable<int> codes)
        {
            codes.ToArray();
        }
    }
}
