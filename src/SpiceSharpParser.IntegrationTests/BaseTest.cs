using SpiceSharpParser.Connector;
using SpiceSharpParser.Parser.Translation;
using SpiceSharpParser.SpiceLexer;
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

        public static Netlist ParseNetlist(params string[] lines)
        {
            var text = string.Join(Environment.NewLine, lines);
            var lexer = new SpiceLexer.SpiceLexer(new SpiceLexerOptions { HasTitle = true });
            var tokensEnumerable = lexer.GetTokens(text);
            var tokens = tokensEnumerable.ToArray();

            var parseTree = new Parser.Parsing.Parser().GetParseTree(tokens);

            var eval = new ParseTreeTranslator();
            var netlistObjectModel = eval.Evaluate(parseTree) as Model.Netlist;

            var connector = new Connector.Connector();
            var netlist = connector.Translate(netlistObjectModel);

            return netlist;
        }

        public static Model.Netlist ParseNetlistToModel(params string[] lines)
        {
            var text = string.Join(Environment.NewLine, lines);
            var lexer = new SpiceLexer.SpiceLexer(new SpiceLexerOptions { HasTitle = true });
            var tokensEnumerable = lexer.GetTokens(text);
            var tokens = tokensEnumerable.ToArray();

            var parseTree = new Parser.Parsing.Parser().GetParseTree(tokens);

            var eval = new ParseTreeTranslator();
            var netlistObjectModel = eval.Evaluate(parseTree) as Model.Netlist;
            return netlistObjectModel;
        }

        public static double RunOpSimulation(Netlist netlist, string nameOfExport)
        {
            double result = double.NaN;
            var export = netlist.Exports.Find(e => e.Name.ToLower() == nameOfExport.ToLower()); //TODO: Remove ToLower someday
            var simulation = netlist.Simulations.Single();
            simulation.OnExportSimulationData += (sender, e) => {

                result = export.Extract();
            };

            simulation.Run(netlist.Circuit);

            return result;
        }

        public static Tuple<double, double>[] RunTransientSimulation(Netlist netlist, string nameOfExport)
        {
            var list = new List<Tuple<double,double>>();

            var export = netlist.Exports.Find(e => e.Name.ToLower() == nameOfExport.ToLower()); //TODO: Remove ToLower someday
            var simulation = netlist.Simulations.Single();
            simulation.OnExportSimulationData += (sender, e) => {

                list.Add(new Tuple<double, double>(e.Time, export.Extract()));
            };

            simulation.Run(netlist.Circuit);

            return list.ToArray();
        }

        public static Tuple<double, double>[] RunDCSimulation(Netlist netlist, string nameOfExport)
        {
            var list = new List<Tuple<double, double>>();

            var export = netlist.Exports.Find(e => e.Name.ToLower() == nameOfExport.ToLower()); //TODO: Remove ToLower someday
            var simulation = netlist.Simulations.Single();
            simulation.OnExportSimulationData += (sender, e) => {

                list.Add(new Tuple<double, double>(e.SweepValue, export.Extract()));
            };

            simulation.Run(netlist.Circuit);

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
