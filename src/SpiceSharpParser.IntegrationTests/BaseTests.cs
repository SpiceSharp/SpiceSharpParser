using System;
using System.Collections.Generic;
using System.Threading;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.Testing;

namespace SpiceSharpParser.IntegrationTests
{
    public class BaseTests
    {
        public BaseTests()
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
        }

        public static SpiceSharpModel GetSpiceSharpModelWithWorkingDirectoryParameter(string workingDirectory, params string[] lines)
        {
            return SpiceNetlistTestHelper.ParseAndRead(
                new SpiceNetlistTestOptions { WorkingDirectory = workingDirectory },
                lines);
        }

        public static SpiceSharpModel GetSpiceSharpModel(params string[] lines)
        {
            return SpiceNetlistTestHelper.ParseAndRead(lines);
        }

        public static SpiceNetlistParseResult ParseNetlistRaw(bool enableBusSyntax = false, params string[] lines)
        {
            return SpiceNetlistTestHelper.ParseRaw(
                new SpiceNetlistTestOptions { EnableBusSyntax = enableBusSyntax },
                lines);
        }

        public static SpiceSharpModel GetSpiceSharpModel(int randomSeed, params string[] lines)
        {
            return SpiceNetlistTestHelper.ParseAndRead(
                new SpiceNetlistTestOptions { Seed = randomSeed },
                lines);
        }

        public static SpiceNetlist ParseNetlist(bool isEndRequired, bool hasTitle, params string[] lines)
        {
            return SpiceNetlistTestHelper.Parse(
                new SpiceNetlistTestOptions
                {
                    HasTitle = hasTitle,
                    IsEndRequired = isEndRequired,
                },
                lines);
        }

        public static SpiceNetlist ParseNetlistToModel(bool isEndRequired, bool hasTitle, string text)
        {
            return SpiceNetlistTestHelper.ParseTextRaw(
                new SpiceNetlistTestOptions
                {
                    HasTitle = hasTitle,
                    IsEndRequired = isEndRequired,
                },
                text).FinalModel;
        }

        public static SpiceNetlist ParseNetlistToModel(bool isEndRequired, bool isNewlineRequired, bool hasTitle, params string[] lines)
        {
            return SpiceNetlistTestHelper.Parse(
                new SpiceNetlistTestOptions
                {
                    HasTitle = hasTitle,
                    IsEndRequired = isEndRequired,
                    IsNewlineRequired = isNewlineRequired,
                },
                lines);
        }

        /// <summary>
        /// Runs simulations from collection.
        /// </summary>
        /// <param name="readerResult">A reader result.</param>
        /// <returns>
        /// A list of exports list.
        /// </returns>
        public static List<object> RunSimulationsAndReturnExports(SpiceSharpModel readerResult)
        {
            return SpiceSimulationTestHelper.RunSimulationsAndReturnExports(readerResult);
        }

        /// <summary>
        /// Runs simulations from collection.
        /// </summary>
        /// <param name="readerResult">A reader result.</param>
        public static void RunSimulations(SpiceSharpModel readerResult)
        {
            SpiceSimulationTestHelper.RunSimulations(readerResult);
        }

        public static double RunOpSimulation(SpiceSharpModel readerResult, string nameOfExport)
        {
            return SpiceSimulationTestHelper.RunOp(readerResult, nameOfExport);
        }

        public static double[] RunOpSimulation(SpiceSharpModel readerResult, params string[] nameOfExport)
        {
            return SpiceSimulationTestHelper.RunOp(readerResult, nameOfExport);
        }

        public static Tuple<string, double>[] RunOpSimulation(SpiceSharpModel readerResult)
        {
            return SpiceSimulationTestHelper.RunOp(readerResult);
        }

        public static Tuple<double, double>[] RunTransientSimulation(SpiceSharpModel readerResult, string nameOfExport)
        {
            return SpiceSimulationTestHelper.RunTransient(readerResult, nameOfExport);
        }

        public static Tuple<double, double>[] RunDCSimulation(SpiceSharpModel readerResult, string nameOfExport)
        {
            return SpiceSimulationTestHelper.RunDc(readerResult, nameOfExport);
        }

        protected bool EqualsWithTol(IEnumerable<Tuple<double, double>> exports, Func<double, double> reference)
        {
            return SpiceNetlistAssertions.EqualsWithTolerance(exports, reference);
        }

        protected bool EqualsWithTol(double expected, double actual)
        {
            return TestTolerance.Default.Equals(expected, actual);
        }

        protected bool EqualsWithTol(IEnumerable<Tuple<double, double>> exports, IEnumerable<double> references)
        {
            return SpiceNetlistAssertions.EqualsWithTolerance(exports, references);
        }

        protected bool EqualsWithTol(IEnumerable<double> exports, IEnumerable<double> references)
        {
            return SpiceNetlistAssertions.EqualsWithTolerance(exports, references);
        }

        protected void AssertMeasurement(SpiceSharpModel model, string name, double expectedValue)
        {
            SpiceNetlistAssertions.AssertMeasurement(model, name, expectedValue);
        }

        protected void AssertMeasurementSuccess(SpiceSharpModel model, string name)
        {
            SpiceNetlistAssertions.AssertMeasurementSuccess(model, name);
        }
    }
}
