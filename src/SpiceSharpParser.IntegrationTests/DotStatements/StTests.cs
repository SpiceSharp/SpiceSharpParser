using System;
using System.Collections.Generic;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.DotStatements
{
    public class StTests : BaseTests
    {
        [Fact]
        public void ListModelParamCount()
        {
            var model = GetSpiceSharpModel(
                "St - Diode circuit",
                "D1 OUT 0 1N914",
                "R1 OUT 1 100",
                "V1 1 0 -1",
                ".model 1N914 D(Is=2.52e-9 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE i(V1)",
                ".ST LIST 1N914(N) 1.752 1.234 1.2 1.0 0.1",
                ".END");

            Assert.Equal(5, model.Exports.Count);
            Assert.Equal(5, model.Simulations.Count);
        }

        [Fact]
        public void ListVoltageCount()
        {
            var model = GetSpiceSharpModel(
                "St - Diode circuit",
                "D1 OUT 0 1N914",
                "R1 OUT 1 100",
                "V1 1 0 -1",
                ".model 1N914 D(Is=2.52e-9 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE i(V1)",
                ".ST LIST V1 1.752 1.234 1.2 1.0 0.1",
                ".END");

            Assert.Equal(5, model.Exports.Count);
            Assert.Equal(5, model.Simulations.Count);
        }

        [Fact]
        public void ListParamWithoutDeclarationCount()
        {
            var model = GetSpiceSharpModel(
                "St - Diode circuit",
                "D1 OUT 0 1N914",
                "R1 OUT 1 100",
                "V1 1 0 {X}",
                ".model 1N914 D(Is=2.52e-9 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE i(V1)",
                ".ST LIST X 1 2 3 4 5",
                ".END");

            Assert.Equal(5, model.Exports.Count);
            Assert.Equal(5, model.Simulations.Count);
        }

        [Fact]
        public void ListParamCount()
        {
            var model = GetSpiceSharpModel(
                "St - Diode circuit",
                "D1 OUT 0 1N914",
                "R1 OUT 1 100",
                "V1 1 0 {X}",
                ".model 1N914 D(Is=2.52e-9 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE i(V1)",
                ".PARAM X=1",
                ".ST LIST X 1 2 3 4 5",
                ".END");

            Assert.Equal(5, model.Exports.Count);
            Assert.Equal(5, model.Simulations.Count);
        }

        [Fact]
        public void ListParamMultipleStCount()
        {
            var model = GetSpiceSharpModel(
                "St - Diode circuit",
                "D1 OUT 0 1N914",
                "R1 OUT 1 100",
                "V1 1 0 {X}",
                ".model 1N914 D(Is=2.52e-9 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE i(V1)",
                ".PARAM X=1",
                ".ST LIST X 1 2 3 4 5",
                ".ST LIST Y 10 20",
                ".END");

            Assert.Equal(10, model.Exports.Count);
            Assert.Equal(10, model.Simulations.Count);
        }

        [Fact]
        public void ListParam()
        {
            var model = GetSpiceSharpModel(
                "St - Test circuit",
                "V1 0 1 {X}",
                "R1 1 0 100",
                ".OP",
                ".SAVE i(R1)",
                ".PARAM X=0",
                ".ST LIST X 1 2",
                ".END");

            Assert.Equal(2, model.Exports.Count);
            Assert.Equal(2, model.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(model);

            Assert.Equal(-0.01, exports[0]);
            Assert.Equal(-0.02, exports[1]);
        }

        [Fact]
        public void LinParam()
        {
            var model = GetSpiceSharpModel(
                "St - Test circuit",
                "V1 0 1 {X}",
                "R1 1 0 100",
                ".OP",
                ".SAVE i(R1)",
                ".PARAM X=0",
                ".ST X 1 100 1",
                ".END");

            Assert.Equal(100, model.Exports.Count);
            Assert.Equal(100, model.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(model);

            for (var i = 0; i < exports.Count; i++)
            {
                Assert.True(EqualsWithTol((double)-0.01 * (i + 1), (double)exports[i]));
            }
        }

        [Fact]
        public void ListVoltage()
        {
            var model = GetSpiceSharpModel(
                "St - Test circuit",
                "V1 0 1",
                "R1 1 0 100",
                ".OP",
                ".SAVE i(R1)",
                ".ST LIST V1 1 2",
                ".END");

            Assert.Equal(2, model.Exports.Count);
            Assert.Equal(2, model.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(model);

            Assert.Equal(-0.01, exports[0]);
            Assert.Equal(-0.02, exports[1]);
        }

        [Fact]
        public void ListModelParameter()
        {
            var model = GetSpiceSharpModel(
                "St - Diode circuit",
                "D1 OUT 0 1N914",
                "R1 OUT 1 100",
                "V1 1 0 10",
                ".model 1N914 D(Is=2.52e-9 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE i(V1)",
                ".ST LIST 1N914(N) 1.752 1.753 1.754",
                ".END");

            Assert.Equal(3, model.Exports.Count);
            Assert.Equal(3, model.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(model);

            Assert.Equal(-0.092108909328949162, exports[0]);
            Assert.Equal(-0.09210442734453031, exports[1]);
            Assert.Equal(-0.09209994538622851, exports[2]);
        }

        [Fact]
        public void ListDeviceParameter()
        {
            var model = GetSpiceSharpModel(
                "St - Test circuit",
                "V1 0 1 1",
                "R1 1 0 100",
                ".OP",
                ".SAVE i(R1)",
                ".ST LIST @V1[dc] 100 200 400",
                ".END");

            Assert.Equal(3, model.Exports.Count);
            Assert.Equal(3, model.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(model);

            Assert.Equal(-1.0, exports[0]);
            Assert.Equal(-2.0, exports[1]);
            Assert.Equal(-4.0, exports[2]);
        }

        [Fact]
        public void ListDeviceParameterResitance()
        {
            var model = GetSpiceSharpModel(
                "St - Test circuit",
                "V1 0 1 1",
                "R1 1 0 100",
                ".OP",
                ".SAVE i(R1)",
                ".ST LIST @R1[resistance] 100 200 400",
                ".END");

            Assert.Equal(3, model.Exports.Count);
            Assert.Equal(3, model.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(model);

            Assert.Equal(-0.01, exports[0]);
            Assert.Equal(-0.005, exports[1]);
            Assert.Equal(-0.0025, exports[2]);
        }

        [Fact]
        public void LinParamWithTable()
        {
            var model = GetSpiceSharpModel(
                "St - Test circuit",
                "V1 0 1 100",
                "R1 1 0 {R}",
                ".OP",
                ".SAVE i(R1)",
                ".PARAM N=0",
                ".PARAM R={table(N, 1, 10, 2, 20, 3, 30, 4, 40)}",
                ".ST N 1 4 1",
                ".END");

            Assert.Equal(4, model.Exports.Count);
            Assert.Equal(4, model.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(model);

            for (var i = 0; i < exports.Count; i++)
            {
                Assert.Equal(-100 / (10.00 * (i + 1)), (double)exports[i]);
            }
        }

        [Fact]
        public void LinParamWithAdvancedTable()
        {
            var model = GetSpiceSharpModel(
                "St - Test circuit",
                "V1 0 1 100",
                "R1 1 0 {R}",
                ".OP",
                ".SAVE i(R1)",
                ".PARAM N=0",
                ".PARAM R={table(N, 1, pow(10, 1), 2*0+2-0, 20 + 0, 3, 30, 4, 40)}",
                ".ST N 1 4 1",
                ".END");

            Assert.Equal(4, model.Exports.Count);
            Assert.Equal(4, model.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(model);

            for (var i = 0; i < exports.Count; i++)
            {
                Assert.True(EqualsWithTol(-100 / (10.00 * (i + 1)), (double)exports[i]));
            }
        }

        [Fact]
        public void LinParamWithDependedTable()
        {
            var model = GetSpiceSharpModel(
                "St - Test circuit",
                "V1 0 1 100",
                "R1 1 0 {R}",
                ".OP",
                ".SAVE i(R1)",
                ".PARAM N=0",
                ".PARAM M=N",
                ".PARAM R={table(M, 1, 10, 2, 20, 3, 30, 4, 40)}",
                ".ST N 1 4 1",
                ".END");

            Assert.Equal(4, model.Exports.Count);
            Assert.Equal(4, model.Simulations.Count);

            var exports = RunSimulationsAndReturnExports(model);

            for (var i = 0; i < exports.Count; i++)
            {
                Assert.True(EqualsWithTol(-100 / (10.00 * (i + 1)), (double)exports[i]));
            }
        }

        [Fact(Skip = "Will be fixed with new version of SpiceSharp")]
        public void DCParameterSweepWithSt()
        {
            var model = GetSpiceSharpModel(
                "St - Sweeping parameters",
                "V1 in 0 11",
                "R1 in out {a}",
                "R2 out 0 {R}",
                ".param R = 0",
                ".param a = 0",
                ".DC a 1 2 0.5",
                ".st LIST R 10 100",
                ".SAVE v(out)",
                ".END");

            var exports = RunSimulationsAndReturnExports(model);

            var r10 = ((List<Double>)exports[0]);
            Assert.True(EqualsWithTol(10.0 / (10.0 + 1) * 11, r10[0]));
            Assert.True(EqualsWithTol(10.0 / (10.0 + 1.5) * 11, r10[1]));
            Assert.True(EqualsWithTol(10.0 / (10.0 + 2) * 11, r10[2]));

            var r100 = ((List<Double>)exports[1]);
            Assert.True(EqualsWithTol(100.0 / (100.0 + 1) * 11, r100[0]));
            Assert.True(EqualsWithTol(100.0 / (100.0 + 1.5) * 11, r100[1]));
            Assert.True(EqualsWithTol(100.0 / (100.0 + 2) * 11, r100[2]));
        }

        [Fact]
        public void ListIcParameter()
        {
            double dcVoltage = 10;
            double resistorResistance = 10e3; // 10000;
            double capacitance = 1e-6; // 0.000001;
            double tau = resistorResistance * capacitance;

            var model = GetSpiceSharpModel(
                "St - The initial voltage on capacitor is {X} V. The result should be an exponential converging to dcVoltage.",
                "C1 OUT 0 1e-6",
                "R1 IN OUT 10e3",
                "V1 IN 0 10",
                ".IC V(OUT)={X}",
                ".TRAN 1e-8 10e-6",
                ".SAVE V(OUT)",
                ".PARAM X = 0",
                ".ST LIST X 0.0 1.0 2.0",
                ".END");

            var exports = RunSimulationsAndReturnExports(model);

            for (var i = 0; i < exports.Count; i++)
            {
                Func<double, double> reference = t => i + dcVoltage * (1.0 - Math.Exp(-t / tau));
                Assert.True(EqualsWithTol((IEnumerable<Tuple<double, double>>)exports[i], reference));
            }
        }

        [Fact]
        public void StTempCount()
        {
            var model = GetSpiceSharpModel(
                "St - Diode circuit",
                "D1 OUT 0 1N914",
                "R1 OUT 1 100",
                "V1 1 0 -1",
                ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".OP",
                ".SAVE i(V1)",
                ".ST TEMP 30 100 1",
                ".END");

            Assert.Equal(71, model.Simulations.Count);
        }
    }
}