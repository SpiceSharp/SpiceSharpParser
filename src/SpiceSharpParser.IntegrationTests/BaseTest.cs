using SpiceSharp.Simulations;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SpiceSharpParser.IntegrationTests
{
    public class BaseTest
    {
        public BaseTest()
        {
        }

        /// <summary>
        /// Absolute tolerance used
        /// </summary>
        public double AbsTol = 1e-12;

        /// <summary>
        /// Relative tolerance used
        /// </summary>
        public double RelTol = 1e-3;

        public static SpiceNetlistReaderResult ParseNetlistInWorkingDirectory(string workingDirectory, params string[] lines)
        {
            var text = string.Join(Environment.NewLine, lines);
            var parser = new SpiceParser();

            parser.Settings.Parsing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;
            parser.Settings.WorkingDirectory = workingDirectory;

            var parserResult = parser.ParseNetlist(text);

            return parserResult.Result;
        }

        public static SpiceNetlistReaderResult ParseNetlist(params string[] lines)
        {
            var text = string.Join(Environment.NewLine, lines);
            var parser = new SpiceParser();

            parser.Settings.Parsing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;

            return parser.ParseNetlist(text).Result;
        }

        public static SpiceNetlistReaderResult ParseNetlist(int randomSeed, params string[] lines)
        {
            var text = string.Join(Environment.NewLine, lines);
            var parser = new SpiceParser();

            parser.Settings.Parsing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;
            parser.Settings.Reading.Seed = randomSeed;

            return parser.ParseNetlist(text).Result;
        }

        public static SpiceNetlist ParseNetlistToModel(bool isEndRequired, bool hasTitle, params string[] lines)
        {
            var text = string.Join(Environment.NewLine, lines);
            var parser = new SpiceParser();
            parser.Settings.Parsing.HasTitle = hasTitle;
            parser.Settings.Parsing.IsEndRequired = isEndRequired;

            return parser.ParseNetlist(text).PreprocessedNetlistModel;
        }

        public static SpiceNetlist ParseNetlistToPostReadedModel(bool isEndRequired, bool hasTitle, params string[] lines)
        {
            var text = string.Join(Environment.NewLine, lines);
            var parser = new SpiceParser();
            parser.Settings.Parsing.HasTitle = hasTitle;
            parser.Settings.Parsing.IsEndRequired = isEndRequired;

            return parser.ParseNetlist(text).PostprocessedNetlistModel;
        }

        /// <summary>
        /// Runs simulations from <see cref="SpiceNetlistReaderResult.Simulations"/> collection.
        /// </summary>
        /// <param name="readerResult">A reader result</param>
        /// <returns>
        /// A list of exports list
        /// </returns>
        public static List<object> RunSimulationsAndReturnExports(SpiceNetlistReaderResult readerResult)
        {
            var result = new List<object>();

            foreach (var export in readerResult.Exports)
            {
                var simulation = export.Simulation;
                if (simulation is DC)
                {
                    var dcResult = new List<double>();
                    result.Add(dcResult);
                    simulation.ExportSimulationData += (sender, e) =>
                    {
                        dcResult.Add(export.Extract());
                    };
                }

                if (simulation is OP)
                {
                    double opResult = double.NaN;
                    simulation.ExportSimulationData += (sender, e) =>
                    {
                        opResult = export.Extract();
                    };

                    simulation.AfterExecute += (sender, e) =>
                    {
                        result.Add(opResult);
                    };
                }

                if (simulation is Transient)
                {
                    var tranResult = new List<Tuple<double, double>>();
                    result.Add(tranResult);
                    simulation.ExportSimulationData += (sender, e) =>
                    {
                        tranResult.Add(new Tuple<double, double>(e.Time, export.Extract()));
                    };
                }
            }

            foreach (var simulation in readerResult.Simulations)
            {
                simulation.Run(readerResult.Circuit);
            }

            return result;
        }

        /// <summary>
        /// Runs simulations from <see cref="SpiceNetlistReaderResult.Simulations"/> collection.
        /// </summary>
        /// <param name="readerResult">A reader result</param>
        /// <returns>
        /// A list of exports list
        /// </returns>
        public static void RunSimulations(SpiceNetlistReaderResult readerResult)
        {
            foreach (var simulation in readerResult.Simulations)
            {
                simulation.Run(readerResult.Circuit);
            }
        }

        public static double RunOpSimulation(SpiceNetlistReaderResult readerResult, string nameOfExport)
        {
            double result = double.NaN;
            var export = readerResult.Exports.Find(e => e.Name.ToLower() == nameOfExport.ToLower()); //TODO: Remove ToLower someday
            var simulation = readerResult.Simulations.Single();
            simulation.ExportSimulationData += (sender, e) => {

                result = export.Extract();
            };

            simulation.Run(readerResult.Circuit);

            return result;
        }

        public static double[] RunOpSimulation(SpiceNetlistReaderResult readerResult, params string[] nameOfExport)
        {
            var simulation = readerResult.Simulations.Single();
            double[] result = new double[nameOfExport.Length];

            simulation.ExportSimulationData += (sender, e) => {

                for (var i = 0; i < nameOfExport.Length; i++) {
                    var export = readerResult.Exports.Find(exp => exp.Name.ToLower() == nameOfExport[i].ToLower()); //TODO: Remove ToLower someday
                    result[i] = export.Extract();
                }
            };

            simulation.Run(readerResult.Circuit);

            return result;
        }

        public static Tuple<string, double>[] RunOpSimulation(SpiceNetlistReaderResult readerResult)
        {
            var simulation = readerResult.Simulations.Single();
            Tuple<string, double>[] result = new Tuple<string, double>[readerResult.Exports.Count];

            simulation.ExportSimulationData += (sender, e) => {

                for (var i = 0; i < readerResult.Exports.Count; i++)
                {
                    var export = readerResult.Exports[i];
                    try
                    {
                        result[i] = new Tuple<string, double>(export.Name, export.Extract());
                    }
                    catch
                    {
                        result[i] = new Tuple<string, double>(export.Name, double.NaN);
                    }
                }
            };

            simulation.Run(readerResult.Circuit);

            return result;
        }

        public static Tuple<double, double>[] RunTransientSimulation(SpiceNetlistReaderResult readerResult, string nameOfExport)
        {
            var list = new List<Tuple<double,double>>();

            var export = readerResult.Exports.Find(e => e.Name.ToLower() == nameOfExport.ToLower()); //TODO: Remove ToLower someday
            var simulation = readerResult.Simulations.Single();
            simulation.ExportSimulationData += (sender, e) => {

                list.Add(new Tuple<double, double>(e.Time, export.Extract()));
            };

            simulation.Run(readerResult.Circuit);

            return list.ToArray();
        }

        public static Tuple<double, double>[] RunDCSimulation(SpiceNetlistReaderResult readerResult, string nameOfExport)
        {
            var list = new List<Tuple<double, double>>();

            var export = readerResult.Exports.Find(e => e.Name.ToLower() == nameOfExport.ToLower()); //TODO: Remove ToLower someday
            var simulation = readerResult.Simulations.Single();
            simulation.ExportSimulationData += (sender, e) => {

                list.Add(new Tuple<double, double>(e.SweepValue, export.Extract()));
            };

            simulation.Run(readerResult.Circuit);

            return list.ToArray();
        }

        protected void EqualsWithTol(IEnumerable<Tuple<double, double>> exports, IEnumerable<Func<double, double>> references)
        {
            using (var exportIt = exports.GetEnumerator())
            using (var referencesIt = references.GetEnumerator())
            {
                while (exportIt.MoveNext() && referencesIt.MoveNext())
                {
                    double actual = exportIt.Current.Item2;
                    double expected = referencesIt.Current(exportIt.Current.Item1);
                    double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * RelTol + AbsTol;
                    Assert.True(Math.Abs(expected - actual) < tol);
                }
            }
        }

        protected void EqualsWithTol(double actual, double expected)
        {
            double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * RelTol + AbsTol;
            Assert.True(Math.Abs(expected - actual) < tol, $"Actual={actual} expected={expected}");
        }

        protected void EqualsWithTol(IEnumerable<Tuple<double, double>> exports, IEnumerable<double> references)
        {
            using (var exportIt = exports.GetEnumerator())
            using (var referencesIt = references.GetEnumerator())
            {
                while (exportIt.MoveNext() && referencesIt.MoveNext())
                {
                    double actual = exportIt.Current.Item2;
                    double expected = referencesIt.Current;
                    double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * RelTol + AbsTol;
                    Assert.True(Math.Abs(expected - actual) < tol);
                }
            }
        }

        protected void EqualsWithTol(IEnumerable<double> exports, IEnumerable<double> references)
        {
            using (var exportIt = exports.GetEnumerator())
            using (var referencesIt = references.GetEnumerator())
            {
                while (exportIt.MoveNext() && referencesIt.MoveNext())
                {
                    double actual = exportIt.Current;
                    double expected = referencesIt.Current;
                    double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * RelTol + AbsTol;
                    Assert.True(Math.Abs(expected - actual) < tol);
                }
            }
        }
    }
}
