using Xunit;

namespace SpiceSharpParser.IntegrationTests
{
    public class StepTest : BaseTest
    {
        [Fact]
        public void StepWithoutDeclarationTest()
        {
            var result = ParseNetlist(
                "Test circuit",
                "V1 0 1 100",
                "R1 1 0 {R}",
                ".OP",
                ".SAVE i(R1)",
                ".PARAM S=3",
                ".PARAM R={table(N, 1, 10, 2, 20, 3, 30)}",
                ".STEP PARAM N LIST 1 2 {S}",
                ".END");

            Assert.Equal(3, result.Exports.Count);
            Assert.Equal(3, result.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(result);

            for (var i = 0; i < exports.Count; i++)
            {
                Assert.Equal(-100 / (10.00 * (i + 1)), exports[i]);
            }
        }

        [Fact]
        public void ParamListTest()
        {
            var result = ParseNetlist(
                "Test circuit",
                "V1 0 1 100",
                "R1 1 0 {R}",
                ".OP",
                ".SAVE i(R1)",
                ".PARAM N=0",
                ".PARAM R={table(N, 1, 10, 2, 20, 3, 30)}",
                ".STEP PARAM N LIST 1 2 3",
                ".END");

            Assert.Equal(3, result.Exports.Count);
            Assert.Equal(3, result.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(result);

            for (var i = 0; i < exports.Count; i++)
            {
                Assert.Equal(-100 / (10.00 * (i + 1)), exports[i]);
            }
        }

        [Fact]
        public void ParamListWithTableInterpolationTest()
        {
            var result = ParseNetlist(
                "Test circuit",
                "V1 0 1 100",
                "R1 1 0 {R}",
                ".OP",
                ".SAVE i(R1)",
                ".PARAM N=0",
                ".PARAM R={table(N, 1, 10, 3, 30)}",
                ".STEP PARAM N LIST 1 2 3 4 5",
                ".END");

            Assert.Equal(5, result.Exports.Count);
            Assert.Equal(5, result.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(result);

            for (var i = 0; i < exports.Count; i++)
            {
                Assert.Equal(-100 / (10.00 * (i + 1)), exports[i]);
            }
        }

        [Fact]
        public void ParamLinTest()
        {
            var result = ParseNetlist(
                "Test circuit",
                "V1 0 1 100",
                "R1 1 0 {R}",
                ".OP",
                ".SAVE i(R1)",
                ".PARAM N=0",
                ".PARAM R={table(N, 1, 10, 2, 20, 3, 30)}",
                ".STEP PARAM N 1 4 1",
                ".END");

            Assert.Equal(3, result.Exports.Count);
            Assert.Equal(3, result.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(result);

            for (var i = 0; i < exports.Count; i++)
            {
                Assert.Equal(-100 / (10.00 * (i + 1)), exports[i]);
            }
        }

        [Fact]
        public void SourceDefaultLinTest()
        {
            var result = ParseNetlist(
                "Test circuit",
                "V1 0 1",
                "R1 1 0 100",
                ".OP",
                ".SAVE i(R1)",
                ".STEP V1 1 100 1",
                ".END");

            Assert.Equal(99, result.Exports.Count);
            Assert.Equal(99, result.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(result);

            for (var i = 0; i < exports.Count; i++)
            {
                EqualsWithTol(-1.0 * (i+1) / 100.0, (double)exports[i]);
            }
        }

        [Fact]
        public void SourceLinTest()
        {
            var result = ParseNetlist(
                "Test circuit",
                "V1 0 1",
                "R1 1 0 100",
                ".OP",
                ".SAVE i(R1)",
                ".STEP LIN V1 1 100 1",
                ".END");

            Assert.Equal(99, result.Exports.Count);
            Assert.Equal(99, result.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(result);

            for (var i = 0; i < exports.Count; i++)
            {
                EqualsWithTol(-1.0 * (i + 1) / 100.0, (double)exports[i]);
            }
        }

        [Fact]
        public void SourceListTest()
        {
            var result = ParseNetlist(
                "Test circuit",
                "V1 0 1",
                "R1 1 0 100",
                ".OP",
                ".SAVE i(R1)",
                ".STEP V1 LIST 1 2 3 4",
                ".END");

            Assert.Equal(4, result.Exports.Count);
            Assert.Equal(4, result.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(result);

            for (var i = 0; i < exports.Count; i++)
            {
                EqualsWithTol(-1.0 * (i + 1) / 100.0, (double)exports[i]);
            }
        }

        [Fact]
        public void SourceDecTest()
        {
            var result = ParseNetlist(
                "Test circuit",
                "V1 0 1",
                "R1 1 0 100",
                ".OP",
                ".SAVE i(R1)",
                ".STEP DEC V1 1 100 1",
                ".END");

            Assert.Equal(3, result.Exports.Count);
            Assert.Equal(3, result.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(result);
            EqualsWithTol(-1 / 100.0, (double)exports[0]);
            EqualsWithTol(-10.000000000000002 / 100.0, (double)exports[1]);
            EqualsWithTol(-100.00000000000004 / 100.0, (double)exports[2]);
        }

        [Fact]
        public void ModelListTest()
        {
            var result = ParseNetlist(
                "Diode circuit",
                "D1 OUT 0 1N914",
                "R1 OUT 1 100",
                "V1 1 0 -1",
                ".model 1N914 D(Is=2.52e-9 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE i(V1)",
                ".STEP D 1N914(N) LIST 1.752 1.234 1.2 1.0 0.1",
                ".END");

            Assert.Equal(5, result.Exports.Count);
            Assert.Equal(5, result.Simulations.Count);
            var exports = RunSimulationsAndReturnExports(result);

            // values not verified with other simulators
            EqualsWithTol(2.52068480498246E-09, (double)exports[0]);
            EqualsWithTol(2.52088986490984E-09, (double)exports[1]);
            EqualsWithTol(2.52089871893846E-09, (double)exports[2]);
            EqualsWithTol(2.5209413879318E-09, (double)exports[3]);
            EqualsWithTol(2.520999941788E-09, (double)exports[4]);
        }

        [Fact]
        public void ModelLinCountTest()
        {
            var result = ParseNetlist(
                "Diode circuit",
                "D1 OUT 0 1N914",
                "R1 OUT 1 100",
                "V1 1 0 -1",
                ".model 1N914 D(Is=2.52e-9 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE i(V1)",
                ".STEP D 1N914(N) 5 10 0.5",
                ".END");

            Assert.Equal(10, result.Exports.Count);
            Assert.Equal(10, result.Simulations.Count);
        }

        [Fact]
        public void TempLinCountTest()
        {
            var result = ParseNetlist(
                "Diode circuit",
                "D1 OUT 0 1N914",
                "R1 OUT 1 100",
                "V1 1 0 -1",
                ".model 1N914 D(Is=2.52e-9 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE i(V1)",
                ".STEP TEMP 50 100 0.5",
                ".END");

            Assert.Equal(100, result.Exports.Count);
            Assert.Equal(100, result.Simulations.Count);
        }
    }
}
