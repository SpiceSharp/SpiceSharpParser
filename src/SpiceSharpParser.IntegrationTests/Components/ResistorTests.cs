using Xunit;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;

namespace SpiceSharpParser.IntegrationTests.Components
{
    public class ResistorTests : BaseTests
    {
        [Fact]
        public void SimplestFormat()
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
        public void SimplestFormatWithParameterWithoutExpression()
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
        public void ModelFormat()
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
        public void ModelFormatWithoutWidth()
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
        public void ModelFormatWithoutWidthAndLength()
        {
            Assert.Throws<GeneralReaderException>(
                () => ParseNetlist(
                    "Resistor circuit",
                    "V1 1 0 150",
                    "R1 1 0 myresistor",
                    ".MODEL myresistor R RSH=0.1 defw=2u",
                    ".SAVE I(R1)",
                    ".OP",
                    ".END"));
        }

        [Fact]
        public void ModelFormatWithoutWidthAndLengthAndWithResitance()
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
        public void TCParameterFormatWithZeroValues()
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
        public void TCParameterFormatWithNonZeroValues()
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
        public void TCParameterFormatWithZeroValue()
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
        public void ModelFormatWithTC1TC2ZeroValues()
        {
            var netlist = ParseNetlist(
                "Resistor circuit",
                "V1 1 0 150",
                "R1 1 0 myresistor L=10u W=2u",
                ".MODEL myresistor R RSH=0.1 TC1=0 TC2=0",
                ".SAVE I(R1)",
                ".OP",
                ".OPTIONS TEMP=10",
                ".END");

            var export = RunOpSimulation(netlist, "I(R1)");
            Assert.NotNull(netlist);
            EqualsWithTol(300, export);
        }

        [Fact]
        public void ModelFormatWithTC2NonZeroValue()
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
        public void ModelFormatWithTC2NonZeroValueWithTempParameter()
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
        public void ModelFormatWithTC1TC2NonZeroValues()
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
        public void ModelFormatWithTC1TC2NonZeroValuesAndTC1TC2OnResistor()
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
        public void ModelFormatWithTC1TC2NonZeroValuesAndTC1OnResistor()
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
    }
}
