using System;
using System.Linq;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Components
{
    public class SubcircuitTests : BaseTests
    {
        [Fact]
        public void ComplexSubcircuitWithParamsAndMultiply()
        {
            var model = GetSpiceSharpModel(
                "Subcircuit - ComplexSubcircuitWithParams",
                "V1 IN 0 4.0",
                "X1 IN 0 twoResistorsInSeries M = 10",
                ".SUBCKT resistor input output params: R=1",
                "R1 input output {R}",
                ".ENDS resistor",
                ".SUBCKT twoResistorsInSeries input output params: R1=10 R2=20",
                "X1 input 1 resistor R=R1",
                "X2 1 output resistor R=R2",
                ".ENDS twoResistorsInSeries",
                ".OP",
                ".SAVE I(V1)",
                ".MEAS OP meas_i MAX I(V1)",
                ".END");

            double export = RunOpSimulation(model, "I(V1)");

            // Get references
            double[] references = { -4.0 / ((10.0 + 20.0) / 10.0) };

            Assert.True(EqualsWithTol(new double[] { export }, references));
            AssertMeasurement(model, "meas_i", export);
        }

        [Fact]
        public void ComplexSubcircuitWithParamsAndMultiplyExisting()
        {
            var model = GetSpiceSharpModel(
                "Subcircuit",
                "V1 IN 0 4.0",
                "XX IN 0 twoResistorsInSeries M = 10",
                ".SUBCKT resistor input output params: R=1",
                "R1 input output {R} m = 3",
                ".ENDS resistor",
                ".SUBCKT twoResistorsInSeries input output params: R1=10 R2=20",
                "X1 input 1 resistor R=R1",
                "X2 1 output resistor R=R2",
                ".ENDS twoResistorsInSeries",
                ".OP",
                ".SAVE I(V1)",
                ".MEAS OP meas_i MAX I(V1)",
                ".END");

            double export = RunOpSimulation(model, "I(V1)");

            // Get references
            double[] references = { -4.0 / ( (10.0 + 20.0) / 30.0) };

            Assert.True(EqualsWithTol(new double[] { export }, references));
            AssertMeasurement(model, "meas_i", export);
        }

        [Fact]
        public void SubcircuitWithMParameters()
        {
            var model = GetSpiceSharpModel(
                "Using M for subcircuits",
                
                "V1 IN 0 10",
                "X1 IN 0 twoResistorsInSeries R1=1 R2=2 M = 10",
                
                ".SUBCKT twoResistorsInSeries input output R1=10 R2=100",
                "R1 input 1 {R1}",
                "R2 1 output {R2}",
                ".ENDS twoResistorsInSeries",

                ".OP",
                ".SAVE I(V1)",
                ".MEAS OP meas_i MAX I(V1)",
                ".END");

            double export = RunOpSimulation(model, "I(V1)");

            double[] references = { -10.0 / (((1.0 + 2.0) / 10.0)) };

            Assert.True(EqualsWithTol(new double[] { export }, references));
            AssertMeasurement(model, "meas_i", export);
        }

        [Fact]
        public void SubcircuitWithMParameterAndFewComponents()
        {
            var model = GetSpiceSharpModel(
                "Using M for subcircuits",

                "V1 IN 0 10",
                "X1 IN 0 twoResistorsInSeries R1=1 R2=2 M = 10",

                ".SUBCKT twoResistorsInSeries input output R1=10 R2=100",
                "R1 input 1 {R1}",
                "R2 1 output {R2}",
                "V1 0 11 1",
                "RX 0 12 100",
                "I1 0 12 1",
                ".ENDS twoResistorsInSeries",

                ".OP",
                ".SAVE I(V1)",
                ".MEAS OP meas_i MAX I(V1)",
                ".END");

            double export = RunOpSimulation(model, "I(V1)");

            double[] references = { -10.0 / (((1.0 + 2.0) / 10.0)) };

            Assert.True(EqualsWithTol(new double[] { export }, references));
            AssertMeasurement(model, "meas_i", export);
        }

        [Fact]
        public void SubcircuitEnding()
        {
            var model = GetSpiceSharpModel(
                "Subcircuit - SubcircuitEndingTest",
                "V1 IN 0 4.0",
                "X1 IN OUT twoResistorsInSeries R1=1 R2=2",
                "RX OUT 0 1",
                ".SUBCKT treeResistorsInSeries input output params: R1=10 R2=100 R3=1000",
                "R1 input 1 {R1}",
                "R2 1 2 {R2}",
                "R3 2 output {R3}",
                ".ENDS",
                ".SUBCKT twoResistorsInSeries input output params: R1=10 R2=100",
                "R1 input 1 {R1}",
                "R2 1 output {R2}",
                ".ENDS twoResistorsInSeries",
                ".OP",
                ".SAVE V(OUT)",
                ".MEAS OP meas_v MAX V(OUT)",
                ".END");

            double export = RunOpSimulation(model, "V(OUT)");

            // Get references
            double[] references = {1.0};

            Assert.True(EqualsWithTol(new double[] { export }, references));
            AssertMeasurement(model, "meas_v", export);
        }

        [Fact]
        public void SubcircuitParamsDifferentFormat()
        {
            var model = GetSpiceSharpModel(
                "Subcircuit - SubcircuitEndingTest",
                "V1 IN 0 4.0",
                "X1 IN OUT twoResistorsInSeries PARAMS: R1=1, R2=2",
                "RX OUT 0 1",
                ".SUBCKT treeResistorsInSeries input output params: R1=10, R2=100, R3=1000",
                "R1 input 1 {R1}",
                "R2 1 2 {R2}",
                "R3 2 output {R3}",
                ".ENDS",
                ".SUBCKT twoResistorsInSeries input output params: R1=10, R2=100",
                "R1 input 1 {R1}",
                "R2 1 output {R2}",
                ".ENDS twoResistorsInSeries",
                ".OP",
                ".SAVE V(OUT)",
                ".MEAS OP meas_v MAX V(OUT)",
                ".END");

            double export = RunOpSimulation(model, "V(OUT)");

            // Get references
            double[] references = {1.0};

            Assert.True(EqualsWithTol(new double[] { export }, references));
            AssertMeasurement(model, "meas_v", export);
        }

        [Fact]
        public void SubcircuitParams()
        {
            var model = GetSpiceSharpModel(
                "Subcircuit - SubcircuitEndingTest",
                "V1 IN 0 4.0",
                "X1 IN OUT twoResistorsInSeries PARAMS: R1=1 R2=2",
                "RX OUT 0 1",
                ".SUBCKT treeResistorsInSeries input output params: R1=10 R2=100 R3=1000",
                "R1 input 1 {R1}",
                "R2 1 2 {R2}",
                "R3 2 output {R3}",
                ".ENDS",
                ".SUBCKT twoResistorsInSeries input output params: R1=10 R2=100",
                "R1 input 1 {R1}",
                "R2 1 output {R2}",
                ".ENDS twoResistorsInSeries",
                ".OP",
                ".SAVE V(OUT)",
                ".MEAS OP meas_v MAX V(OUT)",
                ".END");

            double export = RunOpSimulation(model, "V(OUT)");

            // Get references
            double[] references = {1.0};

            Assert.True(EqualsWithTol(new double[] { export }, references));
            AssertMeasurement(model, "meas_v", export);
        }

        [Fact]
        public void SingleSubcircuitWithParams()
        {
            var model = GetSpiceSharpModel(
                "Subcircuit - SingleSubcircuitWithParams",
                "V1 IN 0 4.0",
                "X1 IN OUT twoResistorsInSeries R1=1 R2=2",
                "RX OUT 0 1",
                ".SUBCKT twoResistorsInSeries input output params: R1=10 R2=100",
                "R1 input 1 {R1}",
                "R2 1 output {R2}",
                ".ENDS twoResistorsInSeries",
                ".OP",
                ".SAVE V(OUT)",
                ".MEAS OP meas_v MAX V(OUT)",
                ".END");

            double export = RunOpSimulation(model, "V(OUT)");

            // Get references
            double[] references = {1.0};

            Assert.True(EqualsWithTol(new double[] { export }, references));
            AssertMeasurement(model, "meas_v", export);
        }

        [Fact]
        public void When_Connectors_Are_Used()
        {
            var model = GetSpiceSharpModel(
                "Subcircuit - Voltometr",
                "V1 IN 0 4.0",
                "X1 IN 0 OUT voltometr",
                "X2 IN 0 OUT2 voltometr2",
                ".SUBCKT voltometr measure_pos measure_neg output",
                "B1 output 0 V={0.6 * V(measure_pos,measure_neg)}",
                ".ENDS voltometr",
                ".SUBCKT voltometr2 measure_pos measure_neg output",
                "B1 output 0 V={0.5 * V(measure_pos,measure_neg)}",
                ".ENDS voltometr2",
                ".OP",
                ".SAVE V(OUT) V(OUT2)",
                ".MEAS OP meas_v MAX V(OUT)",
                ".END");

            double[] exports = RunOpSimulation(model, "V(OUT)", "V(OUT2)");

            // Get references
            double[] references = {2.4, 2.0};

            Assert.True(EqualsWithTol(exports, references));
            AssertMeasurement(model, "meas_v", exports[0]);
        }

        [Fact]
        public void When_ConnectorsSame_Are_Used()
        {
            var model = GetSpiceSharpModel(
                "Subcircuit",
                "V1 IN 0 10.0",
                "V2 IN2 0 5.0",
                "X1 OUT 0 IN something",
                "X2 OUT2 0 IN2 something2",
                ".SUBCKT something output base input",
                "B1 output base V={0.5 * V(input)}",
                ".ENDS something",
                ".SUBCKT something2 output base input",
                "B1 output base V={0.1 * V(input)}",
                ".ENDS something2",
                ".OP",
                ".SAVE V(OUT) V(OUT2)",
                ".MEAS OP meas_v MAX V(OUT)",
                ".END");

            double[] exports = RunOpSimulation(model, "V(OUT)", "V(OUT2)");

            // Get references
            double[] references = {5.0, 0.5};

            Assert.True(EqualsWithTol(exports, references));
            AssertMeasurement(model, "meas_v", exports[0]);
        }

        [Fact]
        public void SingleSubcircuitWithoutParamsKeyword()
        {
            var model = GetSpiceSharpModel(
                "Subcircuit - SingleSubcircuitWithoutParamsKeyword",
                "V1 IN 0 4.0",
                "X1 IN OUT twoResistorsInSeries R1=1 R2=2",
                "RX OUT 0 1",
                ".SUBCKT twoResistorsInSeries input output R1=10 R2=100",
                "R1 input 1 {R1}",
                "R2 1 output {R2}",
                ".ENDS twoResistorsInSeries",
                ".OP",
                ".SAVE V(OUT)",
                ".MEAS OP meas_v MAX V(OUT)",
                ".END");

            double export = RunOpSimulation(model, "V(OUT)");

            // Get references
            double[] references = {1.0};

            Assert.True(EqualsWithTol(new double[] { export }, references));
            AssertMeasurement(model, "meas_v", export);
        }

        [Fact]
        public void SingleSubcircuitWithDefaultParams()
        {
            var model = GetSpiceSharpModel(
                "Subcircuit - SingleSubcircuitWithDefaultParams",
                "V1 IN 0 4.0",
                "X1 IN OUT twoResistorsInSeries",
                "RX OUT 0 1",
                ".SUBCKT twoResistorsInSeries input output params: R1=10 R2=20",
                "R1 input 1 {R1}",
                "R2 1 output {R2}",
                ".ENDS twoResistorsInSeries",
                ".OP",
                ".SAVE V(OUT)",
                ".MEAS OP meas_v MAX V(OUT)",
                ".END");

            double export = RunOpSimulation(model, "V(OUT)");

            // Get references
            double[] references = {(1.0 / (10.0 + 20.0 + 1.0)) * 4.0};

            Assert.True(EqualsWithTol(new double[] { export }, references));
            AssertMeasurement(model, "meas_v", export);
        }

        [Fact]
        public void ComplexSubcircuitWithParams()
        {
            var model = GetSpiceSharpModel(
                "Subcircuit - ComplexSubcircuitWithParams",
                "V1 IN 0 4.0",
                "X1 IN OUT twoResistorsInSeries",
                "RX OUT 0 1",
                ".SUBCKT resistor input output params: R=1",
                "R1 input output {R}",
                ".ENDS resistor",
                ".SUBCKT twoResistorsInSeries input output params: R1=10 R2=20",
                "X1 input 1 resistor R=R1",
                "X2 1 output resistor R=R2",
                ".ENDS twoResistorsInSeries",
                ".OP",
                ".SAVE V(OUT)",
                ".MEAS OP meas_v MAX V(OUT)",
                ".END");

            double export = RunOpSimulation(model, "V(OUT)");

            // Get references
            double[] references = {(1.0 / (10.0 + 20.0 + 1.0)) * 4.0};

            Assert.True(EqualsWithTol(new double[] { export }, references));
            AssertMeasurement(model, "meas_v", export);
        }

        [Fact]
        public void ComplexContainedSubcircuitWithParams()
        {
            var model = GetSpiceSharpModel(
                "Subcircuit - ComplexContainedSubcircuitWithParams",
                "V1 IN 0 4.0",
                "X1 IN OUT twoResistorsInSeries",
                "RX OUT 0 1",
                ".SUBCKT twoResistorsInSeries input output params: R1=10 R2=20",
                ".SUBCKT resistor input output params: R=1",
                "R1 input output {R}",
                ".ENDS resistor",
                "X1 input 1 resistor R=R1",
                "X2 1 output resistor R=R2",
                ".ENDS twoResistorsInSeries",
                ".OP",
                ".SAVE V(OUT)",
                ".MEAS OP meas_v MAX V(OUT)",
                ".END");

            double export = RunOpSimulation(model, "V(OUT)");

            // Get references
            double[] references = {(1.0 / (10.0 + 20.0 + 1.0)) * 4.0};

            Assert.True(EqualsWithTol(new double[] { export }, references));
            AssertMeasurement(model, "meas_v", export);
        }

        [Fact]
        public void ComplexContainedSubcircuitWithParamsAndParamControl()
        {
            var model = GetSpiceSharpModel(
                "Subcircuit - ComplexContainedSubcircuitWithParamsAndParamControl",
                "V1 IN 0 4.0",
                "X1 IN OUT twoResistorsInSeries",
                "RX OUT 0 1",
                ".SUBCKT twoResistorsInSeries input output params: R1=10 R2=20",
                ".SUBCKT resistor input output params: R=1",
                "R1 input output {R}",
                ".ENDS resistor",
                "X1 input 1 resistor R=R1",
                "X2 1 output resistor R=R3",
                ".param R3={R2*1}",
                ".ENDS twoResistorsInSeries",
                ".OP",
                ".SAVE V(OUT)",
                ".MEAS OP meas_v MAX V(OUT)",
                ".END");

            double export = RunOpSimulation(model, "V(OUT)");

            // Get references
            double[] references = {(1.0 / (10.0 + 20.0 + 1.0)) * 4.0};

            Assert.True(EqualsWithTol(new double[] { export }, references));
            AssertMeasurement(model, "meas_v", export);
        }

        [Fact]
        public void SubcircuitWithWrongEnding()
        {
            var text = string.Join(Environment.NewLine, "Subcircuit - ComplexContainedSubcircuitWithParams",
                "V1 IN 0 4.0",
                "X1 IN OUT resistor",
                "RX OUT 0 1",
                ".SUBCKT resistor input output params: R=1",
                "R1 input output {R}",
                ".ENDS resistor2",
                ".OP",
                ".SAVE V(OUT)",
                ".END");
            var parser = new SpiceNetlistParser();

            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;

            var model = parser.ParseNetlist(text);
            Assert.True(model.ValidationResult.HasError);
        }

        [Fact]
        public void When_GND_Port_Used_In_Behavioral_Source_With_Tran()
        {
            // Regression test: SUBCKT with GND port name + behavioral source
            // must work in .TRAN, not just .OP. Previously, Generate() bypassed
            // the pin map for "GND", creating a floating node instead of mapping
            // to the external ground node "0".
            var model = GetSpiceSharpModel(
                "Subcircuit GND Port TRAN Test",
                "V1 IN 0 4.0",
                "X1 0 IN OUT test_subckt",
                ".SUBCKT test_subckt GND INPUT OUTPUT",
                "R1 INPUT mid 5k",
                "R2 mid GND 5k",
                "B1 OUTPUT GND V={0.5 * V(INPUT,GND)}",
                ".ENDS test_subckt",
                ".TRAN 1u 100u",
                ".SAVE V(OUT)",
                ".END");

            var simulations = model.Simulations;
            Assert.Single(simulations);

            var tran = simulations[0];
            double lastValue = double.NaN;

            tran.EventExportData += (sender, e) =>
            {
                var exports = model.Exports.Where(ex => ex.Simulation == tran).ToList();
                foreach (var export in exports)
                {
                    lastValue = export.Extract();
                }
            };

            var codes = tran.Run(model.Circuit, -1);
            codes = tran.InvokeEvents(codes);
            codes.ToArray();

            // B1 = 0.5 * V(IN,0) = 0.5 * 4.0 = 2.0V
            Assert.InRange(lastValue, 1.9, 2.1);
        }
    }
}