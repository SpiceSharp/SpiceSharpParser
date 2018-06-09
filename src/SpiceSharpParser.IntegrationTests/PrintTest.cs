using Xunit;
using SpiceSharpParser.Common.Writers;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Prints;

namespace SpiceSharpParser.IntegrationTests
{
    public class PrintTest : BaseTest
    {
        [Fact]
        public void WriteToCsvTest()
        {
            var parseResult = ParseNetlist(
               "The initial voltage on capacitor is 0V. The result should be an exponential converging to dcVoltage.",
                "C1 OUT 0 1e-6",
                "R1 IN OUT 10e3",
                "V1 IN 0 10",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-8 10e-6",
                ".PRINT TRAN",
                ".END");

            RunSimulations(parseResult);
            Assert.Single(parseResult.Prints);
            parseResult.Prints[0].ToCsv("data2.csv", ";", addCsvHeader: true);
        }

        [Fact]
        public void TranTest()
        {
            var parseResult = ParseNetlist(
               "The initial voltage on capacitor is 0V. The result should be an exponential converging to dcVoltage.",
                "C1 OUT 0 1e-6",
                "R1 IN OUT 10e3",
                "V1 IN 0 10",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-8 10e-6",
                ".PRINT TRAN",
                ".END");

            RunSimulations(parseResult);
            Assert.Equal(1, parseResult.Prints.Count);
            Assert.Equal("1 - tran", parseResult.Prints[0].Name);
            Assert.Equal(7, parseResult.Prints[0].ColumnNames.Count);
            Assert.Equal(62, parseResult.Prints[0].Rows.Count);
        }

        [Fact]
        public void OpPrintWithoutParametersWithFilterTest()
        {
            var parseResult = ParseNetlist(
                "Lowpass RC circuit - The capacitor should act like an open circuit",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 10e-6",
                ".OP",
                ".PRINT OP",
                ".END");

            RunSimulations(parseResult);
            Assert.Equal(1, parseResult.Prints.Count);
            Assert.Equal("1 - op", parseResult.Prints[0].Name);
            Assert.Equal(7, parseResult.Prints[0].ColumnNames.Count);
            Assert.Equal(1, parseResult.Prints[0].Rows.Count);
        }

        [Fact]
        public void OpPrintWithoutParametersTest()
        {
            var parseResult = ParseNetlist(
                "Lowpass RC circuit - The capacitor should act like an open circuit",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 10e-6",
                ".OP",
                ".PRINT",
                ".END");

            RunSimulations(parseResult);
            Assert.Equal(1, parseResult.Prints.Count);
            Assert.Equal("1 - op", parseResult.Prints[0].Name);
            Assert.Equal(7, parseResult.Prints[0].ColumnNames.Count);
            Assert.Equal(1, parseResult.Prints[0].Rows.Count);
        }

        [Fact]
        public void OpPrintWithParametersTest()
        {
            var parseResult = ParseNetlist(
                "Lowpass RC circuit - The capacitor should act like an open circuit",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 10e-6",
                ".OP",
                ".PRINT V(OUT) I(V1)",
                ".END");
            RunSimulations(parseResult);

            Assert.Equal(1, parseResult.Prints.Count);
            Assert.Equal("1 - op", parseResult.Prints[0].Name);
            Assert.Equal(3, parseResult.Prints[0].ColumnNames.Count);
            Assert.Equal(1, parseResult.Prints[0].Rows.Count);
        }

        [Fact]
        public void OpPrintWithParametersWithFilterTest()
        {
            var parseResult = ParseNetlist(
                "Lowpass RC circuit - The capacitor should act like an open circuit",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 10e-6",
                ".OP",
                ".PRINT OP V(OUT) I(V1)",
                ".END");
            RunSimulations(parseResult);

            Assert.Equal(1, parseResult.Prints.Count);
            Assert.Equal("1 - op", parseResult.Prints[0].Name);
            Assert.Equal(3, parseResult.Prints[0].ColumnNames.Count);
            Assert.Equal(1, parseResult.Prints[0].Rows.Count);
        }

        [Fact]
        public void DcPrintWithParametersTest()
        {
            var parseResult = ParseNetlist(
              "DC Sweep - Current",
              "I1 0 in 0",
              "R1 in 0 10",
              ".DC I1 -10 10 1e-3",
              ".PRINT V(in) I(R1)",
              ".END");
            RunSimulations(parseResult);

            Assert.Equal(1, parseResult.Prints.Count);
            Assert.Equal("1 - dc", parseResult.Prints[0].Name);
            Assert.Equal(3, parseResult.Prints[0].ColumnNames.Count);
            Assert.Equal(20001, parseResult.Prints[0].Rows.Count);
        }

        [Fact]
        public void DcPrintWithParametersWithFilterTest()
        {
            var parseResult = ParseNetlist(
              "DC Sweep - Current",
              "I1 0 in 0",
              "R1 in 0 10",
              ".DC I1 -10 10 1e-3",
              ".PRINT DC V(in) I(R1)",
              ".END");
            RunSimulations(parseResult);

            Assert.Equal(1, parseResult.Prints.Count);
            Assert.Equal("1 - dc", parseResult.Prints[0].Name);
            Assert.Equal(3, parseResult.Prints[0].ColumnNames.Count);
            Assert.Equal(20001, parseResult.Prints[0].Rows.Count);
        }

        [Fact]
        public void PrintWithLetTest()
        {
            var parseResult = ParseNetlist(
              "DC Sweep - Current",
              "I1 0 in 0",
              "R1 in 0 10",
              ".DC I1 -10 10 1e-3",
              ".PRINT DC V(in) I(R1) V_in_db",
              ".LET V_in_db {log10(V(in))*2}",
              ".END");
            RunSimulations(parseResult);

            Assert.Equal(1, parseResult.Prints.Count);
            Assert.Equal("1 - dc", parseResult.Prints[0].Name);
            Assert.Equal(4, parseResult.Prints[0].ColumnNames.Count);
            Assert.Equal(20001, parseResult.Prints[0].Rows.Count);
        }
    }
}
