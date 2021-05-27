using Xunit;

namespace SpiceSharpParser.IntegrationTests.AnalogBehavioralModeling
{
    public class PolyTests : BaseTests
    {
        [Fact]
        public void When_OnlyPoly_Expect_Error()
        {
            var netlist = GetSpiceSharpModel(
                "Poly(1) E test circuit - first format",
                "R1 1 0 100",
                "V1 1 0 2",
                "ESource 2 0 POLY",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            Assert.True(netlist.ValidationResult.HasError);
            Assert.False(netlist.ValidationResult.HasWarning);
        }

        [Fact]
        public void VoltageControlledVoltageSourceFormatWithoutDimension()
        {
            var netlist = GetSpiceSharpModel(
                "Poly(1) E test circuit - first format",
                "R1 1 0 100",
                "V1 1 0 2",
                "ESource 2 0 POLY 1 0 2 1", // V(1) + 2
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);
            Assert.False(netlist.ValidationResult.HasWarning);

            double export = RunOpSimulation(netlist, "V(2,0)");
            Assert.Equal(4, export);
        }

        [Fact]
        public void When_PolyWithoutDimension_Expect_Error()
        {
            var result = GetSpiceSharpModel(
                "Poly(1) E test circuit - first format",
                "R1 1 0 100",
                "V1 1 0 2",
                "ESource 2 0 POLY() 1 0 2 1",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.True(result.ValidationResult.HasError);
        }

        [Fact]
        public void VoltageControlledVoltageSourceFirstFormat()
        {
            var netlist = GetSpiceSharpModel(
                "Poly(1) E test circuit - first format",
                "R1 1 0 100",
                "V1 1 0 2",
                "ESource 2 0 POLY(1) 1 0 2 1", // V(1) + 2
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);
            Assert.False(netlist.ValidationResult.HasWarning);

            double export = RunOpSimulation(netlist, "V(2,0)");
            Assert.Equal(4, export);
        }

        [Fact]
        public void VoltageControlledVoltageSourceSecondFormat()
        {
            var netlist = GetSpiceSharpModel(
                "Poly(1) E test circuit - second format",
                "R1 1 0 100",
                "V1 1 0 2",
                "ESource 2 0 POLY(1) (1,0) 2 1", // V(1) + 2
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);
            Assert.False(netlist.ValidationResult.HasWarning);
            double export = RunOpSimulation(netlist, "V(2,0)");
            Assert.Equal(4, export);
        }

        [Fact]
        public void VoltageControlledVoltageSourceFirstFormatSecondDimension()
        {
            var netlist = GetSpiceSharpModel(
                "Poly(1) E test circuit - first format",
                "R1 1 0 100",
                "V1 1 0 2",
                "V3 3 0 3",
                "ESource 2 0 POLY(2) 1 0 3 0 2 1 1", // V(1) + V(3) + 2
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);
            Assert.False(netlist.ValidationResult.HasWarning);

            double export = RunOpSimulation(netlist, "V(2,0)");
            Assert.Equal(7, export);
        }

        [Fact]
        public void VoltageControlledVoltageSourceSecondFormatSecondDimension()
        {
            var netlist = GetSpiceSharpModel(
                "Poly(1) E test circuit - second format",
                "R1 1 0 100",
                "V1 1 0 2",
                "V3 3 0 3",
                "ESource 2 0 POLY(2) (1,0) (3,0) 2 1 1 ", // V(1) + V(3) + 2
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);
            Assert.False(netlist.ValidationResult.HasWarning);

            double export = RunOpSimulation(netlist, "V(2,0)");
            Assert.Equal(7, export);
        }

        [Fact]
        public void When_MissingVariablePoint_Expect_Error()
        {
            var netlist = GetSpiceSharpModel(
                "Poly(1) E test circuit - second format",
                "R1 1 0 100",
                "V1 1 0 2",
                "V3 3 0 3",
                "ESource 2 0 POLY(2) (1,0) 2 1 1 ",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.True(netlist.ValidationResult.HasError);
            Assert.False(netlist.ValidationResult.HasWarning);
        }

        [Fact]
        public void When_MissingVariableDouble_Expect_Error()
        {
            var netlist = GetSpiceSharpModel(
                "Poly(1) E test circuit - second format",
                "R1 1 0 100",
                "V1 1 0 2",
                "V3 3 0 3",
                "ESource 2 0 POLY(2) 1 0 1",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.True(netlist.ValidationResult.HasError);
            Assert.False(netlist.ValidationResult.HasWarning);
        }

        [Fact]
        public void VoltageControlledVoltageSourceThirdFormatSecondDimension()
        {
            var netlist = GetSpiceSharpModel(
                "Poly(1) E test circuit - third format",
                "R1 1 0 100",
                "V1 1 0 2",
                "V3 3 0 3",
                "ESource 2 0 POLY(2) 1 0 3 0 (2, 1, 1)", // V(1) + V(3) + 2
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);
            Assert.False(netlist.ValidationResult.HasWarning);

            double export = RunOpSimulation(netlist, "V(2,0)");
            Assert.Equal(7, export);
        }

        [Fact]
        public void VoltageControlledCurrentSourceFormatWithoutDimension()
        {
            var netlist = GetSpiceSharpModel(
                "Poly(1) G test circuit",
                "R1 1 0 100",
                "V1 2 0 2",
                "GSource 1 0 POLY 2 0 2 1", // V(2) + 2
                ".OP",
                ".SAVE I(GSource)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);
            Assert.False(netlist.ValidationResult.HasWarning);

            double export = RunOpSimulation(netlist, "I(GSource)");
            Assert.Equal(4, export);
        }

        [Fact]
        public void VoltageControlledCurrentSourceFirstFormat()
        {
            var netlist = GetSpiceSharpModel(
                "Poly(1) G test circuit",
                "R1 1 0 100",
                "V1 2 0 2",
                "GSource 1 0 POLY(1) 2 0 2 1", // V(2) + 2
                ".OP",
                ".SAVE I(GSource)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);
            Assert.False(netlist.ValidationResult.HasWarning);

            double export = RunOpSimulation(netlist, "I(GSource)");
            Assert.Equal(4, export);
        }

        [Fact]
        public void VoltageControlledCurrentSourceSecondFormat()
        {
            var netlist = GetSpiceSharpModel(
                "Poly(1) G test circuit",
                "R1 1 0 100",
                "V1 2 0 2",
                "GSource 1 0 POLY(1) (2,0) 2 1", // V(2) + 2
                ".OP",
                ".SAVE I(GSource)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);
            Assert.False(netlist.ValidationResult.HasWarning);

            double export = RunOpSimulation(netlist, "I(GSource)");
            Assert.Equal(4, export);
        }

        [Fact]
        public void VoltageControlledCurrentSourceThirdFormat()
        {
            var netlist = GetSpiceSharpModel(
                "Poly(1) G test circuit",
                "R1 1 0 100",
                "V1 2 0 2",
                "GSource 1 0 POLY(1) 2 0 (2, 1)", // V(2) + 2
                ".OP",
                ".SAVE I(GSource)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);
            Assert.False(netlist.ValidationResult.HasWarning);

            double export = RunOpSimulation(netlist, "I(GSource)");
            Assert.Equal(4, export);
        }

        [Fact]
        public void CurrentControlledCurrentSource()
        {
            var netlist = GetSpiceSharpModel(
                "Poly(1) F test circuit",
                "R1 1 0 100",
                "R2 1 0 200",
                "R3 2 0 100",
                "I1 1 0 2",
                "FSource 2 0 POLY(1) I1 2 1", // I(I1) + 2
                ".OP",
                ".SAVE I(FSource)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);
            Assert.False(netlist.ValidationResult.HasWarning);

            double export = RunOpSimulation(netlist, "I(FSource)");
            Assert.Equal(4, export);
        }

        [Fact]
        public void CurrentControlledCurrentSourceWithoutDimension()
        {
            var netlist = GetSpiceSharpModel(
                "Poly(1) F test circuit",
                "R1 1 0 100",
                "R2 1 0 200",
                "R3 2 0 100",
                "I1 1 0 2",
                "FSource 2 0 POLY I1 2 1", // I(I1) + 2
                ".OP",
                ".SAVE I(FSource)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);
            Assert.False(netlist.ValidationResult.HasWarning);

            double export = RunOpSimulation(netlist, "I(FSource)");
            Assert.Equal(4, export);
        }

        [Fact]
        public void CurrentControlledVoltageSource()
        {
            var netlist = GetSpiceSharpModel(
                "Poly(1) H test circuit",
                "R1 1 0 100",
                "I1 1 0 2",
                "HSource 2 0 POLY(1) I1 2 1", // I(I1) + 2
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);
            Assert.False(netlist.ValidationResult.HasWarning);

            double export = RunOpSimulation(netlist, "V(2,0)");
            Assert.Equal(4, export);
        }

        [Fact]
        public void CurrentControlledVoltageSourceWithoutDimension()
        {
            var netlist = GetSpiceSharpModel(
                "Poly(1) H test circuit",
                "R1 1 0 100",
                "I1 1 0 2",
                "HSource 2 0 POLY I1 2 1", // I(I1) + 2
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);
            Assert.False(netlist.ValidationResult.HasWarning);

            double export = RunOpSimulation(netlist, "V(2,0)");
            Assert.Equal(4, export);
        }
    }
}