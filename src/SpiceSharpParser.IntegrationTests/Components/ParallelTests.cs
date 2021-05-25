using System;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Components
{
    public class ParallelTests : BaseTests
    {
        [Fact]
        public void Test01()
        {
            var lines = new string[] {
                "Test01",
                "V1 IN 0 10",
                ".PARALLEL",
                "X1 IN2 IN twoResistorsInSeries R1=1 R2=2",
                "X2 IN3 IN2 twoResistorsInSeries R1=1 R2=2",
                "X3 IN3 0 twoResistorsInSeries R1=1 R2=2",
                ".ENDP",

                ".SUBCKT twoResistorsInSeries input output R1=10 R2=100",
                "R1 input 1 {R1}",
                "R2 1 output {R2}",
                ".ENDS twoResistorsInSeries",

                ".OP",
                ".OPTIONS localsolver = on",
                ".SAVE I(V1)",
                ".END"};

            var text = string.Join(Environment.NewLine, lines);
            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = true;

            var parseResult = parser.ParseNetlist(text);

            var reader = new SpiceSharpReader();
            reader.Settings.ExpandSubcircuits = false;
            var spiceModel = reader.Read(parseResult.FinalModel);

            double export = RunOpSimulation(spiceModel, "I(V1)");

            double[] references = { -10.0 / 9.0 };

            Assert.True(EqualsWithTol(new double[] { export }, references));
        }

        [Fact]
        public void Test02()
        {
            var lines = new string [] {
                "Test02",
                "V1 IN 0 10",
                ".PARALLEL P1",
                "X1 IN2 IN twoResistorsInSeries R1=1 R2=2",
                "X2 IN3 IN2 twoResistorsInSeries R1=1 R2=2",
                "X3 IN3 0 twoResistorsInSeries R1=1 R2=2",
                ".ENDP",

                ".SUBCKT twoResistorsInSeries input output R1=10 R2=100",
                "R1 input 1 {R1}",
                "R2 1 output {R2}",
                ".ENDS twoResistorsInSeries",

                ".OP",
                ".OPTIONS localsolver=on",
                ".SAVE I(V1)",
                ".END"};

            var text = string.Join(Environment.NewLine, lines);
            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = true;

            var parseResult = parser.ParseNetlist(text);
            var reader = new SpiceSharpReader();
            reader.Settings.ExpandSubcircuits = false;
            var spiceModel = reader.Read(parseResult.FinalModel);

            double export = RunOpSimulation(spiceModel, "I(V1)");

            double[] references = { -10.0 / 9.0 };

            Assert.True(EqualsWithTol(new double[] { export }, references));
        }
    }
}