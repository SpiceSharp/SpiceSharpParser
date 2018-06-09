using NSubstitute;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using Xunit;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Evaluation;
using System.Linq;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Readers.Controls.Simulations
{
    public class StTest
    {
        [Fact]
        public void LinDefaultTest()
        {
            // prepare
            var control = new Control()
            {
                Name = "st",
                Parameters = new ParameterCollection()
                {
                    new WordParameter("v1"),
                    new ValueParameter("1"),
                    new ValueParameter("5"),
                    new ValueParameter("1"),
                }
            };

            var evaluator = Substitute.For<ISpiceEvaluator>();
            evaluator.EvaluateDouble("1").Returns(1.0);
            evaluator.EvaluateDouble("5").Returns(5.0);

            var resultService = new ResultService(
                new ModelsReaders.Netlist.Spice.SpiceNetlistReaderResult(new SpiceSharp.Circuit(), "title"));

            var readingContext = new ReadingContext(
                string.Empty,
                evaluator,
                resultService,
                new MainCircuitNodeNameGenerator(new string[] { }),
                new ObjectNameGenerator(string.Empty));

            // act
            var stControl = new StControl();
            stControl.Read(control, readingContext);

            // assert
            Assert.Single(resultService.SimulationConfiguration.ParameterSweeps);
            Assert.True(resultService.SimulationConfiguration.ParameterSweeps[0].Sweep is LinearSweep);
            Assert.Equal(4, ((LinearSweep)resultService.SimulationConfiguration.ParameterSweeps[0].Sweep).Points.Count());
        }

        [Fact]
        public void LinTest()
        {
            // prepare
            var control = new Control()
            {
                Name = "st",
                Parameters = new ParameterCollection()
                {
                    new WordParameter("LIN"),
                    new WordParameter("v1"),
                    new ValueParameter("1"),
                    new ValueParameter("5"),
                    new ValueParameter("1"),
                }
            };

            var evaluator = Substitute.For<ISpiceEvaluator>();
            evaluator.EvaluateDouble("1").Returns(1.0);
            evaluator.EvaluateDouble("5").Returns(5.0);

            var resultService = new ResultService(
                new ModelsReaders.Netlist.Spice.SpiceNetlistReaderResult(new SpiceSharp.Circuit(), "title"));

            var readingContext = new ReadingContext(
                string.Empty,
                evaluator,
                resultService,
                new MainCircuitNodeNameGenerator(new string[] { }),
                new ObjectNameGenerator(string.Empty));

            // act
            var stControl = new StControl();
            stControl.Read(control, readingContext);

            // assert
            Assert.Single(resultService.SimulationConfiguration.ParameterSweeps);
            Assert.True(resultService.SimulationConfiguration.ParameterSweeps[0].Sweep is LinearSweep);
            Assert.Equal(4, ((LinearSweep)resultService.SimulationConfiguration.ParameterSweeps[0].Sweep).Points.Count());
        }

        [Fact]
        public void DecTest()
        {
            // prepare
            var control = new Control()
            {
                Name = "st",
                Parameters = new ParameterCollection()
                {
                    new WordParameter("DEC"),
                    new WordParameter("v1"),
                    new ValueParameter("1"),
                    new ValueParameter("16"),
                    new ValueParameter("1"),
                }
            };

            var evaluator = Substitute.For<ISpiceEvaluator>();
            evaluator.EvaluateDouble("1").Returns(1.0);
            evaluator.EvaluateDouble("16").Returns(16);

            var resultService = new ResultService(
                new ModelsReaders.Netlist.Spice.SpiceNetlistReaderResult(new SpiceSharp.Circuit(), "title"));

            var readingContext = new ReadingContext(
                string.Empty,
                evaluator,
                resultService,
                new MainCircuitNodeNameGenerator(new string[] { }),
                new ObjectNameGenerator(string.Empty));

            // act
            var stControl = new StControl();
            stControl.Read(control, readingContext);

            // assert
            Assert.Single(resultService.SimulationConfiguration.ParameterSweeps);
            Assert.True(resultService.SimulationConfiguration.ParameterSweeps[0].Sweep is DecadeSweep);
            Assert.Equal(2, ((DecadeSweep)resultService.SimulationConfiguration.ParameterSweeps[0].Sweep).Points.Count());
        }

        [Fact]
        public void OctTest()
        {
            // prepare
            var control = new Control()
            {
                Name = "st",
                Parameters = new ParameterCollection()
                {
                    new WordParameter("OCT"),
                    new WordParameter("v1"),
                    new ValueParameter("1"),
                    new ValueParameter("16"),
                    new ValueParameter("1"),
                }
            };

            var evaluator = Substitute.For<ISpiceEvaluator>();
            evaluator.EvaluateDouble("1").Returns(1.0);
            evaluator.EvaluateDouble("16").Returns(16);

            var resultService = new ResultService(
                new ModelsReaders.Netlist.Spice.SpiceNetlistReaderResult(new SpiceSharp.Circuit(), "title"));

            var readingContext = new ReadingContext(
                string.Empty,
                evaluator,
                resultService,
                new MainCircuitNodeNameGenerator(new string[] { }),
                new ObjectNameGenerator(string.Empty));

            // act
            var stControl = new StControl();
            stControl.Read(control, readingContext);

            // assert
            Assert.Single(resultService.SimulationConfiguration.ParameterSweeps);
            Assert.True(resultService.SimulationConfiguration.ParameterSweeps[0].Sweep is OctaveSweep);
            Assert.Equal(5, ((OctaveSweep)resultService.SimulationConfiguration.ParameterSweeps[0].Sweep).Points.Count());
        }

        [Fact]
        public void ListTest()
        {
            // prepare
            var control = new Control()
            {
                Name = "st",
                Parameters = new ParameterCollection()
                {
                    new WordParameter("LIST"),
                    new WordParameter("v1"),
                    new ValueParameter("1.0"),
                    new ValueParameter("1.0"),
                    new ValueParameter("1.0"),
                }
            };

            var evaluator = Substitute.For<ISpiceEvaluator>();
            evaluator.EvaluateDouble("1.0").Returns(1.0);

            var resultService = new ResultService(
                new ModelsReaders.Netlist.Spice.SpiceNetlistReaderResult(new SpiceSharp.Circuit(), "title"));

            var readingContext = new ReadingContext(
                string.Empty,
                evaluator,
                resultService,
                new MainCircuitNodeNameGenerator(new string[] { }),
                new ObjectNameGenerator(string.Empty));

            // act
            var stControl = new StControl();
            stControl.Read(control, readingContext);

            // assert
            Assert.Single(resultService.SimulationConfiguration.ParameterSweeps);
            Assert.True(resultService.SimulationConfiguration.ParameterSweeps[0].Sweep is ListSweep);
            Assert.Equal(3, ((ListSweep)resultService.SimulationConfiguration.ParameterSweeps[0].Sweep).Points.Count());
        }
    }
}
