using System;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Components
{
    public class ResistorTests : BaseTests
    {
        [Fact]
        public void When_NoResistance_Expect_Error()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0",
                ".SAVE I(R1)",
                ".OP",
                ".END");

            Assert.NotNull(model);
            Assert.True(model.ValidationResult.HasError);
        }

        [Fact]
        public void When_SimplestFormat_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 10",
                ".SAVE I(R1)",
                ".OP",
                ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "I(R1)");
            Assert.Equal(15, export);
        }


        [Fact]
        public void When_SimplestFormatWithMultiply_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 100",
                "R1 1 0 10 m=10", // parallel
                ".SAVE I(R1)",
                ".OP",
                ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "I(R1)");
            Assert.Equal(100.0, export);
        }

        [Fact]
        public void When_NgSpiceFirstFormat_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 r=10",
                ".SAVE I(R1)",
                ".OP",
                ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "I(R1)");
            Assert.Equal(15, export);
        }

        [Fact]
        public void When_NgSpiceFirstFormatWithMultiply_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 r=10 m=2",
                ".SAVE I(R1)",
                ".OP",
                ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "I(R1)");
            Assert.Equal(15*2, export);
        }

        [Fact]
        public void When_NgSpiceSecondFormat_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 resistance=10",
                ".SAVE I(R1)",
                ".OP",
                ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "I(R1)");
            Assert.Equal(15, export);
        }


        [Fact]
        public void When_NgSpiceSecondFormatWithMultiply_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 resistance=10 m = 2",
                ".SAVE I(R1)",
                ".OP",
                ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "I(R1)");
            Assert.Equal(15 * 2, export);
        }

        [Fact]
        public void When_SimplestFormatWithParameterWithoutExpression_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 a",
                ".SAVE I(R1)",
                ".PARAM a = 10",
                ".OP",
                ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "I(R1)");
            Assert.Equal(15, export);
        }

        [Fact]
        public void When_SimplestFormatWithParameterWithoutExpressionWithMultiply_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 a m = 2",
                ".SAVE I(R1)",
                ".PARAM a = 10",
                ".OP",
                ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "I(R1)");
            Assert.Equal(15 * 2, export);
        }

        [Fact]
        public void When_SimplestFormatWithParameterWithoutExpressionForNgSpiceFirstFormat_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 r=a",
                ".SAVE I(R1)",
                ".PARAM a = 10",
                ".OP",
                ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "I(R1)");
            Assert.Equal(15, export);
        }

        [Fact]
        public void When_SimplestFormatWithParameterWithoutExpressionForNgSpiceFirstFormatWithMultiply_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 r=a m = 2",
                ".SAVE I(R1)",
                ".PARAM a = 10",
                ".OP",
                ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "I(R1)");
            Assert.Equal(15 * 2, export);
        }


        [Fact]
        public void When_SimplestFormatWithParameterWithoutExpressionForNgSpiceSecondFormat_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 resistance=a",
                ".SAVE I(R1)",
                ".PARAM a = 10",
                ".OP",
                ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "I(R1)");
            Assert.Equal(15, export);
        }

        [Fact]
        public void When_SimplestFormatWithParameterWithoutExpressionForNgSpiceSecondFormatWithMultiply_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 resistance=a m =2",
                ".SAVE I(R1)",
                ".PARAM a = 10",
                ".OP",
                ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "I(R1)");
            Assert.Equal(15*2, export);
        }

        [Fact]
        public void When_ModelFormat_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 myresistor L=10u W=2u",
                ".MODEL myresistor R RSH=0.1",
                ".SAVE I(R1)",
                ".OP",
                ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "I(R1)");
            Assert.True(EqualsWithTol(300.0, export));
        }

        [Fact]
        public void When_ModelFormatWithMultiply_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 myresistor L=10u W=2u m=2",
                ".MODEL myresistor R RSH=0.1",
                ".SAVE I(R1)",
                ".OP",
                ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "I(R1)");
            Assert.True(EqualsWithTol(300.0 * 2.0, export));
        }

        [Fact]
        public void When_ModelFormatWithoutWidth_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 myresistor L=10u",
                ".MODEL myresistor R RSH=0.1 defw=2u",
                ".SAVE I(R1)",
                ".OP",
                ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "I(R1)");
            Assert.True(EqualsWithTol(300.0, export));
        }

        [Fact]
        public void When_ModelFormatWithoutWidthWithMultiply_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 myresistor L=10u m=2",
                ".MODEL myresistor R RSH=0.1 defw=2u",
                ".SAVE I(R1)",
                ".OP",
                ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "I(R1)");
            Assert.True(EqualsWithTol(300.0 * 2, export));
        }

        [Fact]
        public void When_ModelFormatWithoutWidthAndLength_Expect_Reference()
        {
            var result =
                GetSpiceSharpModel(
                    "Resistor circuit",
                    "V1 1 0 150",
                    "R1 1 0 myresistor",
                    ".MODEL myresistor R RSH=0.1 defw=2u",
                    ".SAVE I(R1)",
                    ".OP",
                    ".END");

            Assert.False(result.ValidationResult.HasError);
            var export = RunOpSimulation(result, "I(R1)");
            Assert.True(EqualsWithTol(0.15, export));
        }

        [Fact]
        public void When_ModelFormatWithoutWidthAndLengthWithMultiply_Expect_Reference()
        {
            var result =
                GetSpiceSharpModel(
                    "Resistor circuit",
                    "V1 1 0 150",
                    "R1 1 0 myresistor m = 4",
                    ".MODEL myresistor R RSH=0.1 defw=2u",
                    ".SAVE I(R1)",
                    ".OP",
                    ".END");

            Assert.NotNull(result);
            Assert.False(result.ValidationResult.HasError);
            var export = RunOpSimulation(result, "I(R1)");
            Assert.True(EqualsWithTol(0.6, export));
        }

        [Fact]
        public void When_ModelFormatWithoutWidthAndLengthAndWithResistance_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                    "Resistor circuit",
                    "V1 1 0 150",
                    "R1 1 0 myresistor 0.5",
                    ".MODEL myresistor R RSH=0.1 defw=2u",
                    ".SAVE I(R1)",
                    ".OP",
                    ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "I(R1)");
            Assert.True(EqualsWithTol(300.0, export));
        }

        [Fact]
        public void When_ModelFormatWithoutWidthAndLengthAndWithResistanceWithMultiply_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                    "Resistor circuit",
                    "V1 1 0 150",
                    "R1 1 0 myresistor 0.5 m = 2",
                    ".MODEL myresistor R RSH=0.1 defw=2u",
                    ".SAVE I(R1)",
                    ".OP",
                    ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "I(R1)");
            Assert.True(EqualsWithTol(300.0 * 2.0, export));
        }

        [Fact]
        public void When_TCParameterFormatWithZeroValues_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 10 TC = 0,0",
                ".SAVE I(R1)",
                ".OP",
                ".OPTIONS TEMP=10",
                ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "I(R1)");
            Assert.Equal(15, export);
        }

        [Fact]
        public void When_TCParameterFormatWithZeroValuesWithMultiply_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 10 TC = 0,0 m = 2",
                ".SAVE I(R1)",
                ".OP",
                ".OPTIONS TEMP=10",
                ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "I(R1)");
            Assert.Equal(15*2, export);
        }

        [Fact]
        public void When_TCParameterFormatWithNonZeroValues_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 10 TC = 0,0.01",
                ".SAVE I(R1)",
                ".OP",
                ".OPTIONS TEMP=10",
                ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "I(R1)");
            Assert.NotEqual(15, export);
        }

        [Fact]
        public void When_TCParameterFormatWithNonZeroValuesWithMultiply_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 10 TC = 0,0.01 m = 2",
                ".SAVE I(R1)",
                ".OP",
                ".OPTIONS TEMP=10",
                ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "I(R1)");
            Assert.NotEqual(15*2, export);
        }

        [Fact]
        public void When_TCParameterFormatWithZeroValue_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 10 TC = 0.01",
                ".SAVE I(R1)",
                ".OP",
                ".OPTIONS TEMP=10",
                ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "I(R1)");
            Assert.True(EqualsWithTol(18.0722891566265, export));
        }

        [Fact]
        public void When_TCParameterFormatWithZeroValueWithMultiply_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 10 TC = 0.01 m=2",
                ".SAVE I(R1)",
                ".OP",
                ".OPTIONS TEMP=10",
                ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "I(R1)");
            Assert.True(EqualsWithTol(18.0722891566265 * 2, export));
        }

        [Fact]
        public void When_ModelFormatWithTC1TC2ZeroValues_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 myresistor L=10u W=2u",
                ".MODEL myresistor R rsh=0.1 tc1=0 tc2=0",
                ".SAVE I(R1)",
                ".OP",
                ".OPTIONS TEMP=10",
                ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "I(R1)");
            Assert.True(EqualsWithTol(300, export));
        }

        [Fact]
        public void When_ModelFormatWithTC1TC2ZeroValuesWithMultiply_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 myresistor L=10u W=2u M = 2",
                ".MODEL myresistor R rsh=0.1 tc1=0 tc2=0",
                ".SAVE I(R1)",
                ".OP",
                ".OPTIONS TEMP=10",
                ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "I(R1)");
            Assert.True(EqualsWithTol(300 * 2, export));
        }

        [Fact]
        public void When_ModelFormatWithTC2NonZeroValue_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 myresistor L=10u W=2u",
                ".MODEL myresistor R RSH=0.1 TC1=0.0 TC2=0.1",
                ".SAVE I(R1)",
                ".OP",
                ".OPTIONS TEMP=10",
                ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "I(R1)");
            Assert.True(EqualsWithTol(10.0334448160535, export));
        }

        [Fact]
        public void When_ModelFormatWithTC2NonZeroValueWithMultiply_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 myresistor L=10u W=2u m = 2",
                ".MODEL myresistor R RSH=0.1 TC1=0.0 TC2=0.1",
                ".SAVE I(R1)",
                ".OP",
                ".OPTIONS TEMP=10",
                ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "I(R1)");
            Assert.True(EqualsWithTol(10.0334448160535 * 2, export));
        }

        [Fact]
        public void When_ModelFormatWithTC2NonZeroValueWithTempParameter_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 myresistor L=10u W=2u Temp=10",
                ".MODEL myresistor R RSH=0.1 TC1=0.0 TC2=0.1",
                ".SAVE I(R1)",
                ".OP",
                ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "I(R1)");
            Assert.True(EqualsWithTol(10.0334448160535, export));
        }

        [Fact]
        public void When_ModelFormatWithTC2NonZeroValueWithTempParameterWithMultiply_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 myresistor L=10u W=2u Temp=10 M = 2",
                ".MODEL myresistor R RSH=0.1 TC1=0.0 TC2=0.1",
                ".SAVE I(R1)",
                ".OP",
                ".END");

            Assert.NotNull(model);
            var export = RunOpSimulation(model, "I(R1)");
            Assert.True(EqualsWithTol(10.0334448160535 * 2, export));
        }

        [Fact]
        public void When_ModelFormatWithTC1TC2NonZeroValues_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 myresistor L=10u W=2u",
                ".MODEL myresistor R RSH=0.1 TC1=0.01 TC2=0.1",
                ".SAVE I(R1)",
                ".OP",
                ".OPTIONS TEMP=10",
                ".END");

            var export = RunOpSimulation(model, "I(R1)");
            Assert.NotNull(model);
            Assert.True(EqualsWithTol(10.0908173562059, export));
        }

        [Fact]
        public void When_ModelFormatWithTC1TC2NonZeroValuesAndTC1TC2OnResistor_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 myresistor L=10u W=2u tc=0.2, 0.3",
                ".MODEL myresistor R RSH=0.1 TC1=0.01 TC2=0.1",
                ".SAVE I(R1)",
                ".OP",
                ".OPTIONS TEMP=10",
                ".END");

            var export = RunOpSimulation(model, "I(R1)");
            Assert.NotNull(model);
            Assert.True(EqualsWithTol(10.0908173562059, export));
        }


        [Fact]
        public void When_ModelFormatWithTC1TC2NonZeroValuesAndTC1TC2OnResistorWithMultiply_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 myresistor L=10u W=2u tc=0.2, 0.3 m = 2",
                ".MODEL myresistor R RSH=0.1 TC1=0.01 TC2=0.1",
                ".SAVE I(R1)",
                ".OP",
                ".OPTIONS TEMP=10",
                ".END");

            var export = RunOpSimulation(model, "I(R1)");
            Assert.NotNull(model);
            Assert.True(EqualsWithTol(10.0908173562059 * 2.0, export));
        }

        [Fact]
        public void When_ModelFormatWithTC1TC2NonZeroValuesAndTC1OnResistor_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 myresistor L=10u W=2u tc=.1",
                ".MODEL myresistor R RSH=0.1 TC1=0.01 TC2=0.1",
                ".SAVE I(R1)",
                ".OP",
                ".OPTIONS TEMP=10",
                ".END");

            var export = RunOpSimulation(model, "I(R1)");
            Assert.NotNull(model);
            Assert.True(EqualsWithTol(10.0908173562059, export));
        }

        [Fact]
        public void When_ModelFormatWithTC1TC2NonZeroValuesAndTC1OnResistorWithMultiply_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 myresistor L=10u W=2u tc=.1 m = 2",
                ".MODEL myresistor R RSH=0.1 TC1=0.01 TC2=0.1",
                ".SAVE I(R1)",
                ".OP",
                ".OPTIONS TEMP=10",
                ".END");

            var export = RunOpSimulation(model, "I(R1)");
            Assert.NotNull(model);
            Assert.True(EqualsWithTol(10.0908173562059 * 2.0, export));
        }

        [Fact]
        public void When_DynamicResistorsIsSpecified_Expect_DynamicResistors()
        {
            var model = GetSpiceSharpModel(
                "DC Sweep - dynamic resistors",
                "V1 in 0 0",
                "V2 out 0 10",
                "R1 out 0 {max(V(in), 1e-3)}",
                ".DC V1 0 10 1e-3",
                ".SAVE I(R1)",
                ".END");

            var exports = RunDCSimulation(model, "I(R1)");

            // Get references
            Func<double, double> reference = sweep => 10.0 / Math.Max(1e-3, (sweep));
            Assert.True(EqualsWithTol(exports, reference));
        }

        [Fact]
        public void When_DynamicResistorsIsSpecifiedWithMultiply_Expect_DynamicResistors()
        {
            var model = GetSpiceSharpModel(
                "DC Sweep - dynamic resistors",
                "V1 in 0 0",
                "V2 out 0 10",
                "R1 out 0 {max(V(in), 1e-3)} m = 2",
                ".DC V1 0 10 1e-3",
                ".SAVE I(R1)",
                ".END");

            var exports = RunDCSimulation(model, "I(R1)");

            // Get references
            Func<double, double> reference = sweep => 10.0 / ((0.5) * Math.Max(1e-3, (sweep)));
            Assert.True(EqualsWithTol(exports, reference));
        }


        [Fact]
        public void When_StaticResistorsIsSpecifiedWithMultiply_Expect_DynamicResistors()
        {
            var model = GetSpiceSharpModel(
                "DC Sweep - dynamic resistors",
                "V1 in 0 0",
                "V2 out 0 10",
                "R1 out 0 {max(1e-4, 1e-3)} m = 2",
                ".DC V1 0 10 1e-3",
                ".SAVE I(R1)",
                ".END");

            var exports = RunDCSimulation(model, "I(R1)");

            // Get references
            Func<double, double> reference = sweep => 10.0 / ((0.5) * Math.Max(1e-3, 1e-4));
            Assert.True(EqualsWithTol(exports, reference));
        }

        [Fact]
        public void When_StaticResistorsIsSpecifiedWithMultiplyMandN_Expect_DynamicResistors()
        {
            var model = GetSpiceSharpModel(
                "DC Sweep - dynamic resistors",
                "V1 in 0 0",
                "V2 out 0 10",
                "R1 out 0 {max(1e-4, 1e-3)} m = 2 n = 3",
                ".DC V1 0 10 1e-3",
                ".SAVE I(R1)",
                ".END");

            var exports = RunDCSimulation(model, "I(R1)");

            // Get references
            Func<double, double> reference = sweep => 10.0 / ((0.5) * 3.0 * Math.Max(1e-3, 1e-4));
            Assert.True(EqualsWithTol(exports, reference));
        }
    }
}