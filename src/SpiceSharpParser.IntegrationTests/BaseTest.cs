using SpiceSharpParser.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SpiceSharpParser.IntegrationTests
{
    public class BaseTest
    {
        /// <summary>
        /// Absolute tolerance used
        /// </summary>
        public double AbsTol = 1e-12;

        /// <summary>
        /// Relative tolerance used
        /// </summary>
        public double RelTol = 1e-3;

        public static ConnectorResult ParseNetlist(params string[] lines)
        {
            var text = string.Join(Environment.NewLine, lines);
            var parserFront = new ParserFrontage();
            return parserFront.Parse(text, new ParserSettings() { HasTitle = true, IsEndRequired = true }).SpiceSharpModel;
        }

        public static Model.Netlist ParseNetlistToModel(bool isEndRequired, bool hasTitle, params string[] lines)
        {
            var text = string.Join(Environment.NewLine, lines);
            var parserFront = new ParserFrontage();
            return parserFront.Parse(text, new ParserSettings() { HasTitle = hasTitle, IsEndRequired = isEndRequired }).NetlistModel;
        }

        public static double RunOpSimulation(ConnectorResult connectorResult, string nameOfExport)
        {
            double result = double.NaN;
            var export = connectorResult.Exports.Find(e => e.Name.ToLower() == nameOfExport.ToLower()); //TODO: Remove ToLower someday
            var simulation = connectorResult.Simulations.Single();
            simulation.OnExportSimulationData += (sender, e) => {

                result = export.Extract();
            };

            simulation.Run(connectorResult.Circuit);

            return result;
        }

        public static Tuple<double, double>[] RunTransientSimulation(ConnectorResult connectorResult, string nameOfExport)
        {
            var list = new List<Tuple<double,double>>();

            var export = connectorResult.Exports.Find(e => e.Name.ToLower() == nameOfExport.ToLower()); //TODO: Remove ToLower someday
            var simulation = connectorResult.Simulations.Single();
            simulation.OnExportSimulationData += (sender, e) => {

                list.Add(new Tuple<double, double>(e.Time, export.Extract()));
            };

            simulation.Run(connectorResult.Circuit);

            return list.ToArray();
        }

        public static Tuple<double, double>[] RunDCSimulation(ConnectorResult connectorResult, string nameOfExport)
        {
            var list = new List<Tuple<double, double>>();

            var export = connectorResult.Exports.Find(e => e.Name.ToLower() == nameOfExport.ToLower()); //TODO: Remove ToLower someday
            var simulation = connectorResult.Simulations.Single();
            simulation.OnExportSimulationData += (sender, e) => {

                list.Add(new Tuple<double, double>(e.SweepValue, export.Extract()));
            };

            simulation.Run(connectorResult.Circuit);

            return list.ToArray();
        }

        protected void Compare(IEnumerable<Tuple<double, double>> exports, IEnumerable<Func<double, double>> references)
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

        protected void Compare(IEnumerable<Tuple<double, double>> exports, IEnumerable<double> references)
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

        protected void Compare(IEnumerable<double> exports, IEnumerable<double> references)
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
