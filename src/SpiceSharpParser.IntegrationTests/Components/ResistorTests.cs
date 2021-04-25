using System;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Components
{
    public class ResistorTests : BaseTests
    {
        [Fact]
        public void When_NoResistance_Expect_Exception()
        {
            var result = ParseNetlist(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0",
                ".SAVE I(R1)",
                ".OP",
                ".END");

            Assert.True(result.ValidationResult.HasWarning);
        }

        [Fact]
        public void When_SimplestFormat_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 10",
                ".SAVE I(R1)",
                ".OP",
                ".END");

            var export = RunOpSimulation(netlist, "I(R1)");
            Assert.NotNull(netlist);
            Assert.Equal(15, export);
        }

        [Fact]
        public void When_NgSpiceFirstFormat_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 r=10",
                ".SAVE I(R1)",
                ".OP",
                ".END");

            var export = RunOpSimulation(netlist, "I(R1)");
            Assert.NotNull(netlist);
            Assert.Equal(15, export);
        }

        [Fact]
        public void When_NgSpiceSecondFormat_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 resistance=10",
                ".SAVE I(R1)",
                ".OP",
                ".END");

            var export = RunOpSimulation(netlist, "I(R1)");
            Assert.NotNull(netlist);
            Assert.Equal(15, export);
        }

        [Fact]
        public void When_SimplestFormatWithParameterWithoutExpression_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 a",
                ".SAVE I(R1)",
                ".PARAM a = 10",
                ".OP",
                ".END");

            var export = RunOpSimulation(netlist, "I(R1)");
            Assert.NotNull(netlist);
            Assert.Equal(15, export);
        }

        [Fact]
        public void When_SimplestFormatWithParameterWithoutExpressionForNgSpiceFirstFormat_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 r=a",
                ".SAVE I(R1)",
                ".PARAM a = 10",
                ".OP",
                ".END");

            var export = RunOpSimulation(netlist, "I(R1)");
            Assert.NotNull(netlist);
            Assert.Equal(15, export);
        }

        [Fact]
        public void When_SimplestFormatWithParameterWithoutExpressionForNgSpiceSecondFormat_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 resistance=a",
                ".SAVE I(R1)",
                ".PARAM a = 10",
                ".OP",
                ".END");

            var export = RunOpSimulation(netlist, "I(R1)");
            Assert.NotNull(netlist);
            Assert.Equal(15, export);
        }

        [Fact]
        public void When_ModelFormat_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 myresistor L=10u W=2u",
                ".MODEL myresistor R RSH=0.1",
                ".SAVE I(R1)",
                ".OP",
                ".END");

            var export = RunOpSimulation(netlist, "I(R1)");
            Assert.NotNull(netlist);
            EqualsWithTol(300.0, export);
        }

        [Fact]
        public void When_ModelFormatWithoutWidth_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 myresistor L=10u",
                ".MODEL myresistor R RSH=0.1 defw=2u",
                ".SAVE I(R1)",
                ".OP",
                ".END");

            var export = RunOpSimulation(netlist, "I(R1)");
            Assert.NotNull(netlist);
            EqualsWithTol(300.0, export);
        }

        [Fact]
        public void When_ModelFormatWithoutWidthAndLength_Expect_Reference()
        {
            var result =
                ParseNetlist(
                    "Resistor circuit",
                    "V1 1 0 150",
                    "R1 1 0 myresistor",
                    ".MODEL myresistor R RSH=0.1 defw=2u",
                    ".SAVE I(R1)",
                    ".OP",
                    ".END");

            Assert.False(result.ValidationResult.HasError);
        }

        [Fact]
        public void When_ModelFormatWithoutWidthAndLengthAndWithResistance_Expect_Reference()
        {
            var netlist = ParseNetlist(
                    "Resistor circuit",
                    "V1 1 0 150",
                    "R1 1 0 myresistor 0.5",
                    ".MODEL myresistor R RSH=0.1 defw=2u",
                    ".SAVE I(R1)",
                    ".OP",
                    ".END");

            var export = RunOpSimulation(netlist, "I(R1)");
            Assert.NotNull(netlist);
            EqualsWithTol(300.0, export);
        }

        [Fact]
        public void When_TCParameterFormatWithZeroValues_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 10 TC = 0,0",
                ".SAVE I(R1)",
                ".OP",
                ".OPTIONS TEMP=10",
                ".END");

            var export = RunOpSimulation(netlist, "I(R1)");
            Assert.NotNull(netlist);
            Assert.Equal(15, export);
        }

        [Fact]
        public void When_TCParameterFormatWithNonZeroValues_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 10 TC = 0,0.01",
                ".SAVE I(R1)",
                ".OP",
                ".OPTIONS TEMP=10",
                ".END");

            var export = RunOpSimulation(netlist, "I(R1)");
            Assert.NotNull(netlist);
            Assert.NotEqual(15, export);
        }

        [Fact]
        public void When_TCParameterFormatWithZeroValue_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 10 TC = 0.01",
                ".SAVE I(R1)",
                ".OP",
                ".OPTIONS TEMP=10",
                ".END");

            var export = RunOpSimulation(netlist, "I(R1)");
            Assert.NotNull(netlist);
            EqualsWithTol(18.0722891566265, export);
        }

        [Fact]
        public void When_ModelFormatWithTC1TC2ZeroValues_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 myresistor L=10u W=2u",
                ".MODEL myresistor R rsh=0.1 tc1=0 tc2=0",
                ".SAVE I(R1)",
                ".OP",
                ".OPTIONS TEMP=10",
                ".END");

            var export = RunOpSimulation(netlist, "I(R1)");
            Assert.NotNull(netlist);
            EqualsWithTol(300, export);
        }

        [Fact]
        public void When_ModelFormatWithTC2NonZeroValue_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 myresistor L=10u W=2u",
                ".MODEL myresistor R RSH=0.1 TC1=0.0 TC2=0.1",
                ".SAVE I(R1)",
                ".OP",
                ".OPTIONS TEMP=10",
                ".END");

            var export = RunOpSimulation(netlist, "I(R1)");
            Assert.NotNull(netlist);
            EqualsWithTol(10.0334448160535, export);
        }

        [Fact]
        public void When_ModelFormatWithTC2NonZeroValueWithTempParameter_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 myresistor L=10u W=2u Temp=10",
                ".MODEL myresistor R RSH=0.1 TC1=0.0 TC2=0.1",
                ".SAVE I(R1)",
                ".OP",
                ".END");

            var export = RunOpSimulation(netlist, "I(R1)");
            Assert.NotNull(netlist);
            EqualsWithTol(10.0334448160535, export);
        }

        [Fact]
        public void When_ModelFormatWithTC1TC2NonZeroValues_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 myresistor L=10u W=2u",
                ".MODEL myresistor R RSH=0.1 TC1=0.01 TC2=0.1",
                ".SAVE I(R1)",
                ".OP",
                ".OPTIONS TEMP=10",
                ".END");

            var export = RunOpSimulation(netlist, "I(R1)");
            Assert.NotNull(netlist);
            EqualsWithTol(10.0908173562059, export);
        }

        [Fact]
        public void When_ModelFormatWithTC1TC2NonZeroValuesAndTC1TC2OnResistor_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 myresistor L=10u W=2u tc=0.2, 0.3",
                ".MODEL myresistor R RSH=0.1 TC1=0.01 TC2=0.1",
                ".SAVE I(R1)",
                ".OP",
                ".OPTIONS TEMP=10",
                ".END");

            var export = RunOpSimulation(netlist, "I(R1)");
            Assert.NotNull(netlist);
            EqualsWithTol(10.0908173562059, export);
        }

        [Fact]
        public void When_ModelFormatWithTC1TC2NonZeroValuesAndTC1OnResistor_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 myresistor L=10u W=2u tc=.1",
                ".MODEL myresistor R RSH=0.1 TC1=0.01 TC2=0.1",
                ".SAVE I(R1)",
                ".OP",
                ".OPTIONS TEMP=10",
                ".END");

            var export = RunOpSimulation(netlist, "I(R1)");
            Assert.NotNull(netlist);
            EqualsWithTol(10.0908173562059, export);
        }

        [Fact]
        public void When_DynamicResistorsIsSpecified_Expect_DynamicResistors()
        {
            var netlist = ParseNetlist(
                "DC Sweep - dynamic resistors",
                "V1 in 0 0",
                "V2 out 0 10",
                "R1 out 0 {max(V(in), 1e-3)}",
                ".DC V1 0 10 1e-3",
                ".SAVE I(R1)",
                ".END");

            var exports = RunDCSimulation(netlist, "I(R1)");

            // Get references
            Func<double, double> reference = sweep => 10.0 / Math.Max(1e-3, (sweep));
            EqualsWithTol(exports, reference);
        }
    }
}