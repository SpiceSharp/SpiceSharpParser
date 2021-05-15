using Xunit;

namespace SpiceSharpParser.IntegrationTests.DotStatements
{
    public class StepTests : BaseTests
    {
        [Fact]
        public void StepWithoutDeclaration()
        {
            var model = GetSpiceSharpModel(
                "Step - Test circuit",
                "V1 0 1 100",
                "R1 1 0 {R}",
                ".OP",
                ".SAVE i(R1)",
                ".PARAM S=3",
                ".PARAM R={table(N, 1, 10, 2, 20, 3, 30)}",
                ".STEP PARAM N LIST 1 2 {S}",
                ".END");

            Assert.Equal(3, model.Exports.Count);
            Assert.Equal(3, model.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(model);

            for (var i = 0; i < exports.Count; i++)
            {
                Assert.Equal(-100 / (10.00 * (i + 1)), exports[i]);
            }
        }

        [Fact]
        public void ParamList()
        {
            var model = GetSpiceSharpModel(
                "Step - Test circuit",
                "V1 0 1 100",
                "R1 1 0 {R}",
                ".OP",
                ".SAVE i(R1)",
                ".PARAM N=0",
                ".PARAM R={table(N, 1, 10, 2, 20, 3, 30)}",
                ".STEP PARAM N LIST 1 2 3",
                ".END");

            Assert.Equal(3, model.Exports.Count);
            Assert.Equal(3, model.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(model);

            for (var i = 0; i < exports.Count; i++)
            {
                Assert.Equal(-100 / (10.00 * (i + 1)), exports[i]);
            }
        }

        [Fact]
        public void ParamDependencyList()
        {
            var model = GetSpiceSharpModel(
                "Step - Test circuit",
                "V1 0 1 100",
                "R1 1 0 {R}",
                ".OP",
                ".SAVE i(R1)",
                ".PARAM N=0",
                ".PARAM M={N+0*10}",
                ".PARAM S={M+0*100}",
                ".PARAM R={table(S, 1, 10, 2, 20, 3, 30)}",
                ".STEP PARAM N LIST 1 2 3",
                ".END");

            Assert.Equal(3, model.Exports.Count);
            Assert.Equal(3, model.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(model);

            for (var i = 0; i < exports.Count; i++)
            {
                Assert.Equal(-100 / (10.00 * (i + 1)), exports[i]);
            }
        }

        [Fact]
        public void ParamListWithTableInterpolation()
        {
            var model = GetSpiceSharpModel(
                "Step - Test circuit",
                "V1 0 1 100",
                "R1 1 0 {R}",
                ".OP",
                ".SAVE i(R1)",
                ".PARAM N=0",
                ".PARAM R={table(N, 1, 10, 3, 30)}",
                ".STEP PARAM N LIST 1 2 3",
                ".END");

            Assert.Equal(3, model.Exports.Count);
            Assert.Equal(3, model.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(model);

            for (var i = 0; i < exports.Count; i++)
            {
                Assert.Equal(-100 / (10.00 * (i + 1)), exports[i]);
            }
        }

        [Fact]
        public void ParamLin()
        {
            var model = GetSpiceSharpModel(
                "Step - Test circuit",
                "V1 0 1 100",
                "R1 1 0 {R}",
                ".OP",
                ".SAVE i(R1)",
                ".PARAM N=0",
                ".PARAM R={table(N, 1, 10, 2, 20, 3, 30, 4, 40)}",
                ".STEP PARAM N 1 4 1",
                ".END");

            Assert.Equal(4, model.Exports.Count);
            Assert.Equal(4, model.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(model);

            for (var i = 0; i < exports.Count; i++)
            {
                Assert.Equal(-100 / (10.00 * (i + 1)), exports[i]);
            }
        }

        [Fact]
        public void StepParamSubcktGlobal()
        {
            var model = GetSpiceSharpModel(
                "Step - Subcircuit + STEP",
                "V1 IN 0 4.0",
                "X1 IN OUT twoResistors R1=1 R2=2",
                "RX OUT 0 1",
                ".SUBCKT twoResistors input output params: R1=10 R2=10",
                "R1 input 1 {X*R1}",
                "R2 1 output {R2}",
                ".ENDS twoResistors",
                ".OP",
                ".SAVE V(OUT)",
                ".STEP PARAM X LIST 1 5",
                ".END");

            Assert.Equal(2, model.Exports.Count);
            Assert.Equal(2, model.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(model);

            // Get references
            double[] references = { 1.0, 0.5 };

            for (var i = 0; i < exports.Count; i++)
            {
                Assert.True(EqualsWithTol((double)exports[i], references[i]));
            }
        }

        [Fact]
        public void StepParamSubcktParam()
        {
            var model = GetSpiceSharpModel(
                "Step - Subcircuit + STEP",
                "V1 IN 0 4.0",
                "X1 IN OUT twoResistors R1={X} R2=2",
                "RX OUT 0 1",
                ".SUBCKT twoResistors input output params: R1=10 R2=10",
                "R1 input 1 {R1}",
                "R2 1 output {R2}",
                ".ENDS twoResistors",
                ".OP",
                ".SAVE V(OUT)",
                ".STEP PARAM X LIST 1 5",
                ".END");

            Assert.Equal(2, model.Exports.Count);
            Assert.Equal(2, model.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(model);

            // Get references
            double[] references = { 1.0, 0.5 };

            for (var i = 0; i < exports.Count; i++)
            {
                Assert.True(EqualsWithTol((double)exports[i], references[i]));
            }
        }

        [Fact]
        public void StepParamSubcktParamComplex()
        {
            var model = GetSpiceSharpModel(
                "Step - Subcircuit + STEP",
                "V1 IN 0 4.0",
                "X1 IN OUT twoResistors R1={X} R2=2",
                "RX OUT 0 1",
                ".SUBCKT twoResistors input output params: R1=10 R2=10",
                "X1 input 1 resistor R={R1+0}",
                "X2 1 output resistor R={R2}",
                ".ENDS twoResistors",
                ".SUBCKT resistor input output params: R=1000",
                "R1 input output {R+0}",
                ".ENDS resistor",
                ".OP",
                ".SAVE V(OUT)",
                ".STEP PARAM X LIST 1 5",
                ".END");

            Assert.Equal(2, model.Exports.Count);
            Assert.Equal(2, model.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(model);

            // Get references
            double[] references = { 1.0, 0.5 };

            for (var i = 0; i < exports.Count; i++)
            {
                Assert.True(EqualsWithTol((double)exports[i], references[i]));
            }
        }

        [Fact]
        public void SourceDefaultLin()
        {
            var model = GetSpiceSharpModel(
                "Step - Test circuit",
                "V1 0 1",
                "R1 1 0 100",
                ".OP",
                ".SAVE i(R1)",
                ".STEP V1 1 100 1",
                ".END");

            Assert.Equal(100, model.Exports.Count);
            Assert.Equal(100, model.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(model);

            for (var i = 0; i < exports.Count; i++)
            {
                Assert.True(EqualsWithTol(-1.0 * (i + 1) / 100.0, (double)exports[i]));
            }
        }

        [Fact]
        public void SourceLin()
        {
            var model = GetSpiceSharpModel(
                "Step - Test circuit",
                "V1 0 1",
                "R1 1 0 100",
                ".OP",
                ".SAVE i(R1)",
                ".STEP LIN V1 1 100 1",
                ".END");

            Assert.Equal(100, model.Exports.Count);
            Assert.Equal(100, model.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(model);

            for (var i = 0; i < exports.Count; i++)
            {
                Assert.True(EqualsWithTol(-1.0 * (i + 1) / 100.0, (double)exports[i]));
            }
        }

        [Fact]
        public void SourceList()
        {
            var model = GetSpiceSharpModel(
                "Step - Test circuit",
                "V1 0 1",
                "R1 1 0 100",
                ".OP",
                ".SAVE i(R1)",
                ".STEP V1 LIST 1 2 3 4",
                ".END");

            Assert.Equal(4, model.Exports.Count);
            Assert.Equal(4, model.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(model);

            for (var i = 0; i < exports.Count; i++)
            {
                Assert.True(EqualsWithTol(-1.0 * (i + 1) / 100.0, (double)exports[i]));
            }
        }

        [Fact]
        public void SourceDec()
        {
            var model = GetSpiceSharpModel(
                "Step - Test circuit",
                "V1 0 1",
                "R1 1 0 100",
                ".OP",
                ".SAVE i(R1)",
                ".STEP DEC V1 1 100 1",
                ".END");

            Assert.Equal(3, model.Exports.Count);
            Assert.Equal(3, model.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(model);

            Assert.True(EqualsWithTol(-1 / 100.0, (double)exports[0]));
            Assert.True(EqualsWithTol(-10.000000000000002 / 100.0, (double)exports[1]));
            Assert.True(EqualsWithTol(-100.00000000000004 / 100.0, (double)exports[2]));
        }

        [Fact]
        public void ModelList()
        {
            var model = GetSpiceSharpModel(
                "Step - Diode circuit",
                "D1 OUT 0 1N914",
                "R1 OUT 1 100",
                "V1 1 0 -1",
                ".model 1N914 D(Is=2.52e-9 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE i(V1)",
                ".STEP D 1N914(N) LIST 1.752 1.234 1.2 1.0 0.1",
                ".END");

            Assert.Equal(5, model.Exports.Count);
            Assert.Equal(5, model.Simulations.Count);
            var exports = RunSimulationsAndReturnExports(model);

            // values not verified with other simulators
            Assert.True(EqualsWithTol(2.52068480498246E-09, (double)exports[0]));
            Assert.True(EqualsWithTol(2.52088986490984E-09, (double)exports[1]));
            Assert.True(EqualsWithTol(2.52089871893846E-09, (double)exports[2]));
            Assert.True(EqualsWithTol(2.5209413879318E-09, (double)exports[3]));
            Assert.True(EqualsWithTol(2.520999941788E-09, (double)exports[4]));
        }

        [Fact]
        public void ModelLinCount()
        {
            var model = GetSpiceSharpModel(
                "Step - Diode circuit",
                "D1 OUT 0 1N914",
                "R1 OUT 1 100",
                "V1 1 0 -1",
                ".model 1N914 D(Is=2.52e-9 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE i(V1)",
                ".STEP D 1N914(N) 5 10 0.5",
                ".END");

            Assert.Equal(11, model.Exports.Count);
            Assert.Equal(11, model.Simulations.Count);
        }

        [Fact]
        public void TempLinCount()
        {
            var model = GetSpiceSharpModel(
                "Step - Diode circuit",
                "D1 OUT 0 1N914",
                "R1 OUT 1 100",
                "V1 1 0 -1",
                ".model 1N914 D(Is=2.52e-9 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE i(V1)",
                ".STEP TEMP 50 100 0.5",
                ".END");

            Assert.Equal(101, model.Exports.Count);
            Assert.Equal(101, model.Simulations.Count);
        }
    }
}