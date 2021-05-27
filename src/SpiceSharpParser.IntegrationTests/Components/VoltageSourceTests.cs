using System.IO;
using System.Linq;
using System.Net;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Components
{
    public class VoltageSourceTests : BaseTests
    {
        [Fact]
        public void When_PulseWithoutBracket_NoException()
        {
            var model = GetSpiceSharpModel(
                "Voltage source",
                "V1 1 0 PULSE 0V 6V 3.68us 41ns 41ns 3.256us 6.52us",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            Assert.NotNull(model);
            var exception = Record.Exception(() => RunTransientSimulation(model, "V(1,0)"));
            Assert.Null(exception);
        }

        [Fact]
        public void When_PulseWithBracket_NoException()
        {
            var model = GetSpiceSharpModel(
                "Voltage source",
                "V1 1 0 PULSE(0V 6V 3.68us 41ns 41ns 3.256us 6.52us)",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");
            Assert.NotNull(model);
            var exception = Record.Exception(() => RunTransientSimulation(model, "V(1,0)"));
            Assert.Null(exception);
        }

        [Fact]
        public void When_SineWithBracket_NoException()
        {
            var model = GetSpiceSharpModel(
                "Voltage source",
                "V1 1 0 SINE(0 5 50 0 0 90)",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            Assert.NotNull(model);
            var exception = Record.Exception(() => RunTransientSimulation(model, "V(1,0)"));
            Assert.Null(exception);
        }

        [Fact]
        public void When_SineWithoutBracket_NoException()
        {
            var model = GetSpiceSharpModel(
                "Voltage source",
                "V1 1 0 SINE 0 5 50 0 0 90",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            Assert.NotNull(model);
            var exception = Record.Exception(() => RunTransientSimulation(model, "V(1,0)"));
            Assert.Null(exception);
        }

        [Fact]
        public void When_SinWithBracket_NoException()
        {
            var model = GetSpiceSharpModel(
                "Voltage source",
                "V1 1 0 SIN(0 5 50 0 0 90)",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            Assert.NotNull(model);
            var exception = Record.Exception(() => RunTransientSimulation(model, "V(1,0)"));
            Assert.Null(exception);
        }

        [Fact]
        public void When_SinWithoutBracket_NoException()
        {
            var model = GetSpiceSharpModel(
                "Voltage source",
                "V1 1 0 SIN 0 5 50 0 0 90",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            Assert.NotNull(model);
            var exception = Record.Exception(() => RunTransientSimulation(model, "V(1,0)"));
            Assert.Null(exception);
        }

        [Fact]
        public void When_PwlWithBracket_NoException()
        {
            var model = GetSpiceSharpModel(
                "Voltage source",
                "V1 1 0 Pwl(0 1 1 2 2 3)",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            Assert.NotNull(model);
            var exception = Record.Exception(() => RunTransientSimulation(model, "V(1,0)"));
            Assert.Null(exception);
        }

        [Fact]
        public void When_PwlFileWithSpaces_Expect_NoException()
        {
            var model = GetSpiceSharpModel(
                "PWL file voltage source",
                "V1 1 0 Pwl file = Resources\\pwl_space.txt",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            Assert.NotNull(model);
            var exception = Record.Exception(() => RunTransientSimulation(model, "V(1,0)"));
            Assert.Null(exception);
        }

        [Fact]
        public void When_PwlFileWithCommas_Expect_NoException()
        {
            var model = GetSpiceSharpModel(
                "PWL file voltage source",
                "V1 1 0 Pwl file = Resources\\pwl_comma.txt",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            Assert.NotNull(model);
            var exception = Record.Exception(() => RunTransientSimulation(model, "V(1,0)"));
            Assert.Null(exception);
        }

        [Fact]
        public void When_PwlFileWithSemicolon_Expect_NoException()
        {
            var model = GetSpiceSharpModel(
                "PWL file voltage source",
                "V1 1 0 Pwl file = Resources\\pwl_semicolon.txt",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            Assert.NotNull(model);
            var exception = Record.Exception(() => RunTransientSimulation(model, "V(1,0)"));
            Assert.Null(exception);
        }

        [Fact]
        public void When_PwlFileWithQuotes_Expect_NoException()
        {
            var model = GetSpiceSharpModel(
                "PWL file voltage source",
                "V1 1 0 Pwl file = \"Resources\\pwl_space.txt\"",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            Assert.NotNull(model);
            var exception = Record.Exception(() => RunTransientSimulation(model, "V(1,0)"));
            Assert.Null(exception);
        }

        [Fact]
        public void When_PwlFile_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Voltage source",
                "V1 1 0 PWL file = \"Resources\\pwl_reference.txt\"",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 0.1 1.5",
                ".END");

            Assert.NotNull(model);
            var exports = RunTransientSimulation(model, "V(1,0)");
            Assert.True(exports.All(export => EqualsWithTol(export.Item2, (export.Item1 < 1.0 ? 2.0 * export.Item1 : 2.0))));
        }

        [Fact]
        public void When_PwlWithoutBracket_NoException()
        {
            var model = GetSpiceSharpModel(
                "Voltage source",
                "V1 1 0 Pwl 0 1 1 2 2 3",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            Assert.NotNull(model);
            var exception = Record.Exception(() => RunTransientSimulation(model, "V(1,0)"));
            Assert.Null(exception);
        }

        [Fact]
        public void When_PwlWithoutBracketWithCommas_NoException()
        {
            var model = GetSpiceSharpModel(
                "Voltage source",
                "V1 1 0 Pwl 0 1, 1 2, 2 3",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            Assert.NotNull(model);
            var exception = Record.Exception(() => RunTransientSimulation(model, "V(1,0)"));
            Assert.Null(exception);
        }

        [Fact]
        public void When_PwlOnePoint_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Voltage source",
                "V1 1 0 Pwl(0 2)",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            Assert.NotNull(model);
            var exports = RunTransientSimulation(model, "V(1,0)");
            Assert.True(exports.All(export => export.Item2 == 2.0));
        }

        [Fact]
        public void When_PwlTwoPoints_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Voltage source",
                "V1 1 0 Pwl(0 0 1.0 2.0)",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 0.1 1.5",
                ".END");

            Assert.NotNull(model);
            var exports = RunTransientSimulation(model, "V(1,0)");
            Assert.True(exports.All(export => EqualsWithTol(export.Item2, (export.Item1 < 1.0 ? 2.0 * export.Item1 : 2.0))));
        }

        [Fact]
        public void When_PwlTwoPointsWithAc_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Voltage source",
                "V1 1 0 Pwl(0 0 1.0 2.0)",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 0.1 1.5",
                ".AC LIN 1000 1 1000",
                ".END");

            Assert.NotNull(model);
            var exports = RunTransientSimulation(model, "V(1,0)");
            Assert.True(exports.All(export => EqualsWithTol(export.Item2, (export.Item1 < 1.0 ? 2.0 * export.Item1 : 2.0))));
        }

        [Fact]
        public void When_PwlTwoPointsWithCommas_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Voltage source",
                "V1 1 0 Pwl 0 0, 1.0 2.0",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 0.1 1.5",
                ".END");

            Assert.NotNull(model);
            var exports = RunTransientSimulation(model, "V(1,0)");
            Assert.True(exports.All(export => EqualsWithTol(export.Item2, (export.Item1 < 1.0 ? 2.0 * export.Item1 : 2.0))));
        }

        [Fact]
        public void When_PwlMinusTimePoint_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "Voltage source",
                "V1 1 0 Pwl(-1.0 0 1.0 2.0)",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 0.1 1.5",
                ".END");

            Assert.NotNull(model);
            var exports = RunTransientSimulation(model, "V(1,0)");
            Assert.True(exports.All(export => EqualsWithTol(export.Item2, (export.Item1 < 1.0 ? 1.0 + export.Item1 : 2.0))));
        }

        [Fact]
        public void When_ACWithoutValue_Expect_NoException()
        {
            var model = GetSpiceSharpModel(
                "Voltage source",
                "V1 1 0 AC",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 0.1 1.5",
                ".AC LIN 1000 1 1000",
                ".END");

            Assert.NotNull(model);
        }

        [Fact]
        public void When_ACWithDC_Expect_NoException()
        {
            var model = GetSpiceSharpModel(
                "Voltage source",
                "V1 1 0 AC 1 DC 2",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 0.1 1.5",
                ".AC LIN 1000 1 1000",
                ".END");

            Assert.NotNull(model);
        }

        [Fact]
        public void When_DCWithoutValue_Expect_NoException()
        {
            var model = GetSpiceSharpModel(
                "Voltage source",
                "V1 1 0 DC",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 0.1 1.5",
                ".END");

            Assert.NotNull(model);
        }

        [Fact]
        public void When_ACPlusSin_Expect_NoException()
        {
            var model = GetSpiceSharpModel(
                "Voltage source",
                "V1 1 0 AC 0",
                "+SIN 0 10 1000 0 0 0",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 0.1 1.5",
                ".AC LIN 1000 1 1000",
                ".END");

            Assert.NotNull(model);
        }

        [Fact]
        public void When_ACPlusSine_Expect_NoException()
        {
            var model = GetSpiceSharpModel(
                "Voltage source",
                "V1 1 0 AC 0",
                "+SINE 0 10 1000 0 0 0",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 0.1 1.5",
                ".AC LIN 1000 1 1000",
                ".END");

            Assert.NotNull(model);
        }

        [Fact]
        public void When_ACPlusPulse_Expect_NoException()
        {
            var model = GetSpiceSharpModel(
                "Voltage source",
                "V1 1 0 AC 0",
                "+PULSE 0V 5V 3.61us 41ns 41ns 4.255us 3.51us",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 0.1 1.5",
                ".AC LIN 1000 1 1000",
                ".END");

            Assert.NotNull(model);
        }

        [Fact]
        public void When_ACPlusPwl_Expect_NoException()
        {
            var model = GetSpiceSharpModel(
                "Voltage source",
                "V1 1 0 AC 0",
                "+Pwl -1.0 0 1.0 2.0",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 0.1 1.5",
                ".AC LIN 1000 1 1000",
                ".END");

            Assert.NotNull(model);
        }

        [Fact]
        public void When_DCAndAC_Expect_NoException()
        {
            var model = GetSpiceSharpModel(
                "Voltage source",
                "V1 1 0 DC 1 AC 0",
                "+Pwl -1.0 0 1.0 2.0",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 0.1 1.5",
                ".AC LIN 1000 1 1000",
                ".END");

            Assert.NotNull(model);
        }
    }
}