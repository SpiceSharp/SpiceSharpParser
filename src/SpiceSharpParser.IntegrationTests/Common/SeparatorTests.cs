using System;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Common
{
    public class SeparatorTests : BaseTests
    {
        [Fact]
        public void FunnySeparator()
        {
            var lines = new string[] {
                "Funny separator",
                "V1 IN 0 4.0",
                "X1 IN OUT twoResistorsInSeries R1=1 R2=2",
                "R1 OUT 0 1",
                "\n",
                ".SUBCKT resistor input output params: R=1",
                "R1 input output {R}",
                ".ENDS resistor",
                ".SUBCKT twoResistorsInSeries input output params: R1=1 R2=1",
                "X1 input 1 resistor R={R1}",
                "X2 1 output resistor R={R2}",
                ".ENDS twoResistorsInSeries",
                "\n",
                ".OP",
                ".SAVE V(OUT) V(X1--X1--input) I(X1--X1--R1)",
                ".END" };

            var text = string.Join(Environment.NewLine, lines);

            var parser = new SpiceNetlistParser();
            var parseResult = parser.ParseNetlist(text);

            var reader = new SpiceSharpReader();
            reader.Settings.Separator = "--";
            var spiceModel = reader.Read(parseResult.FinalModel);

            double[] export = RunOpSimulation(spiceModel, "V(OUT)", "V(X1--X1--input)", "I(X1--X1--R1)");

            Assert.Equal(1, export[0]);
            Assert.Equal(4, export[1]);
            Assert.Equal(1, export[2]);
        }
    }
}
