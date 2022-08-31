using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using System.Text;
using SpiceSharpParser.Common;
using System.Threading;

namespace SpiceSharpParser.IntegrationTests
{
    public class BaseTests
    {
        /// <summary>
        /// Absolute tolerance used
        /// </summary>
        private double AbsTol = 1e-12;

        /// <summary>
        /// Relative tolerance used
        /// </summary>
        private double RelTol = 1e-3;

        public BaseTests()
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
        }

        public static SpiceSharpModel GetSpiceSharpModelWithWorkingDirectoryParameter(string workingDirectory, params string[] lines)
        {
            var text = string.Join(Environment.NewLine, lines);
            var parser = new SpiceNetlistParser();

            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;
            parser.Settings.WorkingDirectory = workingDirectory;

            var parserResult = parser.ParseNetlist(text);

            var spiceSharpSettings = new SpiceNetlistReaderSettings(new SpiceNetlistCaseSensitivitySettings(), () => parser.Settings.WorkingDirectory, Encoding.Default);
            var spiceSharpReader = new SpiceNetlistReader(spiceSharpSettings);

            return spiceSharpReader.Read(parserResult.FinalModel);
        }

        public static SpiceSharpModel GetSpiceSharpModel(params string[] lines)
        {
            var text = string.Join(Environment.NewLine, lines);
            var parser = new SpiceNetlistParser();

            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;

            var parserResult = parser.ParseNetlist(text);

            var spiceSharpSettings = new SpiceNetlistReaderSettings(new SpiceNetlistCaseSensitivitySettings(), () => parser.Settings.WorkingDirectory, Encoding.Default);
            var spiceSharpReader = new SpiceNetlistReader(spiceSharpSettings);

            return spiceSharpReader.Read(parserResult.FinalModel);
        }

        public static SpiceNetlistParseResult ParseNetlistRaw(bool enableBusSyntax = false, params string[] lines)
        {
            var text = string.Join(Environment.NewLine, lines);
            var parser = new SpiceNetlistParser();

            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Lexing.EnableBusSyntax = enableBusSyntax;
            parser.Settings.Parsing.IsEndRequired = true;

            return parser.ParseNetlist(text);
        }

        public static SpiceSharpModel GetSpiceSharpModel(int randomSeed, params string[] lines)
        {
            var text = string.Join(Environment.NewLine, lines);
            var parser = new SpiceNetlistParser();

            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;
            var parserResult = parser.ParseNetlist(text);

            var spiceSharpSettings = new SpiceNetlistReaderSettings(new SpiceNetlistCaseSensitivitySettings(), () => parser.Settings.WorkingDirectory, Encoding.Default)
            {
                Seed = randomSeed
            };

            var spiceSharpReader = new SpiceNetlistReader(spiceSharpSettings);

            return spiceSharpReader.Read(parserResult.FinalModel);
        }

        public static SpiceNetlist ParseNetlist(bool isEndRequired, bool hasTitle, params string[] lines)
        {
            var text = string.Join(Environment.NewLine, lines);
            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = hasTitle;
            parser.Settings.Parsing.IsEndRequired = isEndRequired;

            return parser.ParseNetlist(text).FinalModel;
        }

        public static SpiceNetlist ParseNetlistToModel(bool isEndRequired, bool hasTitle, string text)
        {
            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = hasTitle;
            parser.Settings.Parsing.IsEndRequired = isEndRequired;

            return parser.ParseNetlist(text).FinalModel;
        }

        public static SpiceNetlist ParseNetlistToModel(bool isEndRequired, bool isNewlineRequired, bool hasTitle, params string[] lines)
        {
            var text = string.Join(Environment.NewLine, lines);
            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = hasTitle;
            parser.Settings.Parsing.IsEndRequired = isEndRequired;
            parser.Settings.Parsing.IsNewlineRequired = isNewlineRequired;

            return parser.ParseNetlist(text).FinalModel;
        }

        /// <summary>
        /// Runs simulations from collection.
        /// </summary>
        /// <param name="readerResult">A reader result</param>
        /// <returns>
        /// A list of exports list
        /// </returns>
        public static List<object> RunSimulationsAndReturnExports(SpiceSharpModel readerResult)
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
                    simulation.ExportSimulationData += (sender, e) =>
                    {
                        var opResult = export.Extract();
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
        /// Runs simulations from collection.
        /// </summary>
        /// <param name="readerResult">A reader result</param>
        /// <returns>
        /// A list of exports list
        /// </returns>
        public static void RunSimulations(SpiceSharpModel readerResult)
        {
            foreach (var simulation in readerResult.Simulations)
            {
                simulation.Run(readerResult.Circuit);
            }
        }

        public static double RunOpSimulation(SpiceSharpModel readerResult, string nameOfExport)
        {
            double result = double.NaN;
            var export = readerResult.Exports.Find(e => e.Name == nameOfExport);
            var simulation = readerResult.Simulations.Single();
            simulation.ExportSimulationData += (sender, e) =>
            {
                result = export.Extract();
            };

            simulation.Run(readerResult.Circuit);

            return result;
        }

        public static double[] RunOpSimulation(SpiceSharpModel readerResult, params string[] nameOfExport)
        {
            var simulation = readerResult.Simulations.Single();
            double[] result = new double[nameOfExport.Length];

            simulation.ExportSimulationData += (sender, e) =>
            {
                for (var i = 0; i < nameOfExport.Length; i++)
                {
                    var export = readerResult.Exports.Find(exp => exp.Name == nameOfExport[i]);
                    result[i] = export.Extract();
                }
            };

            simulation.Run(readerResult.Circuit);

            return result;
        }

        public static Tuple<string, double>[] RunOpSimulation(SpiceSharpModel readerResult)
        {
            var simulation = readerResult.Simulations.First(s => s is OP);
            Tuple<string, double>[] result = new Tuple<string, double>[readerResult.Exports.Count];

            simulation.ExportSimulationData += (sender, e) =>
            {
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

        public static Tuple<double, double>[] RunTransientSimulation(SpiceSharpModel readerResult, string nameOfExport)
        {
            var list = new List<Tuple<double, double>>();

            var export = readerResult.Exports.Find(e => e.Name == nameOfExport && e.Simulation is Transient);
            var simulation = readerResult.Simulations.First(s => s is Transient);
            simulation.ExportSimulationData += (sender, e) =>
            {
                list.Add(new Tuple<double, double>(e.Time, export.Extract()));
            };

            simulation.Run(readerResult.Circuit);

            return list.ToArray();
        }

        public static Tuple<double, double>[] RunDCSimulation(SpiceSharpModel readerResult, string nameOfExport)
        {
            var list = new List<Tuple<double, double>>();

            var export = readerResult.Exports.Find(e => e.Name == nameOfExport && e.Simulation is DC);
            var simulation = readerResult.Simulations.First(s => s is DC);
            simulation.ExportSimulationData += (sender, e) =>
            {
                list.Add(new Tuple<double, double>(e.GetSweepValues().First(), export.Extract()));
            };

            simulation.Run(readerResult.Circuit);

            return list.ToArray();
        }

        protected bool EqualsWithTol(IEnumerable<Tuple<double, double>> exports, Func<double, double> reference)
        {
            using (var exportIt = exports.GetEnumerator())
            {
                while (exportIt.MoveNext())
                {
                    double actual = exportIt.Current.Item2;
                    double expected = reference(exportIt.Current.Item1);
                    double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * RelTol + AbsTol;

                    if (Math.Abs(expected - actual) > tol)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        protected bool EqualsWithTol(double expected, double actual)
        {
            double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * RelTol + AbsTol;
            return Math.Abs(expected - actual) < tol;
        }

        protected bool EqualsWithTol(IEnumerable<Tuple<double, double>> exports, IEnumerable<double> references)
        {
            using (var exportIt = exports.GetEnumerator())
            using (var referencesIt = references.GetEnumerator())
            {
                while (exportIt.MoveNext() && referencesIt.MoveNext())
                {
                    double actual = exportIt.Current.Item2;
                    double expected = referencesIt.Current;
                    double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * RelTol + AbsTol;
                    if (Math.Abs(expected - actual) > tol)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        protected bool EqualsWithTol(IEnumerable<double> exports, IEnumerable<double> references)
        {
            using (var exportIt = exports.GetEnumerator())
            using (var referencesIt = references.GetEnumerator())
            {
                while (exportIt.MoveNext() && referencesIt.MoveNext())
                {
                    double actual = exportIt.Current;
                    double expected = referencesIt.Current;
                    double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * RelTol + AbsTol;
                    if (Math.Abs(expected - actual) > tol)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}