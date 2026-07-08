using System;
using System.Linq;
using System.Text;
using SpiceSharp.Components;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.LTspiceCompatibility
{
    public class LTspiceCompatibilityP3Tests : BaseTests
    {
        [Fact]
        public void When_RlcModelTcAliasIsRead_Expect_Tc1AndTc2AreMapped()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - R/C tc aliases",
                ".model rmod R(RSH=1 tc=0.01,0.02)",
                ".model cmod C(CJ=1n tc=0.03,0.04)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);

            var resistorModel = Assert.IsType<ResistorModel>(model.Circuit["rmod"]);
            Assert.Equal(0.01, resistorModel.Parameters.TemperatureCoefficient1);
            Assert.Equal(0.02, resistorModel.Parameters.TemperatureCoefficient2);

            var capacitorModel = Assert.IsType<CapacitorModel>(model.Circuit["cmod"]);
            Assert.Equal(0.03, capacitorModel.Parameters.TemperatureCoefficient1);
            Assert.Equal(0.04, capacitorModel.Parameters.TemperatureCoefficient2);
        }

        [Fact]
        public void When_SwitchThresholdAliasesAreRead_Expect_MidpointAndHysteresisAreMapped()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - switch aliases",
                ".model vsw SW(Ron=10 Roff=1Meg von=2 voff=0)",
                ".model isw CSW(Ron=10 Roff=1Meg ion=2m ioff=0)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);

            var voltageSwitch = Assert.IsType<VoltageSwitchModel>(model.Circuit["vsw"]);
            Assert.Equal(1.0, voltageSwitch.Parameters.Threshold);
            Assert.Equal(1.0, voltageSwitch.Parameters.Hysteresis);

            var currentSwitch = Assert.IsType<CurrentSwitchModel>(model.Circuit["isw"]);
            Assert.Equal(1e-3, currentSwitch.Parameters.Threshold);
            Assert.Equal(1e-3, currentSwitch.Parameters.Hysteresis);
        }

        [Fact]
        public void When_LtspiceSwitchSeriesVoltageIsRead_Expect_SynthesizedVoltageSource()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - switch Vser",
                ".param vf=0.2",
                "V1 in 0 1",
                "VCTRL ctrl 0 1",
                "RLOAD in out 100",
                "S1 out 0 ctrl 0 smod",
                ".model smod SW(Ron=1m Roff=1Meg Vt=0.5 Vh=0 Vser={vf})",
                ".op",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.IsType<VoltageSource>(model.Circuit["S1_vser"]);
            Assert.InRange(RunOpSimulation(model, "V(out)"), 0.199, 0.201);
        }

        [Fact]
        public void When_LtspiceSwitchSeriesInductanceIsRead_Expect_SynthesizedInductor()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - switch Lser",
                ".param ls=1u",
                "S1 out 0 ctrl 0 smod",
                ".model smod SW(Ron=1 Roff=1Meg Vt=0.5 Vh=0 Lser={ls})",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.IsType<Inductor>(model.Circuit["S1_lser"]);
        }

        [Fact]
        public void When_LtspiceCurrentSwitchSeriesExtrasAreRead_Expect_SynthesizedHelpers()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - current switch series extras",
                "W1 out 0 VCTRL smod",
                ".model smod CSW(Ron=1 Roff=1Meg It=1m Ih=0 Vser=0.1 Lser=1u)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.IsType<VoltageSource>(model.Circuit["W1_vser"]);
            Assert.IsType<Inductor>(model.Circuit["W1_lser"]);
        }

        [Fact]
        public void When_LtspiceMetadataParametersAreRead_Expect_WarningsOnly()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - metadata no-ops",
                "R1 out 0 1k mfg=Acme pn=R123",
                ".model rmod R(RSH=1 mfg=Acme desc=thinfilm)",
                ".end");

            Assert.False(model.ValidationResult.HasError, ValidationMessages(model.ValidationResult));
            Assert.True(model.ValidationResult.HasWarning);
            AssertWarningContains(model.ValidationResult, "mfg");
            AssertWarningContains(model.ValidationResult, "pn");
            AssertWarningContains(model.ValidationResult, "desc");
        }

        [Fact]
        public void When_MetadataParametersAreReadWithoutLtspiceCompatibility_Expect_DefaultErrors()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.None,
                "LTspice P3 - default metadata",
                ".model rmod R(RSH=1 mfg=Acme)",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, "mfg");
        }

        [Fact]
        public void When_LtspiceResistorSeriesParasiticIsRead_Expect_SynthesizedSeriesResistance()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - resistor Rser",
                "V1 in 0 1",
                "R1 in out 90 Rser=10",
                "RLOAD out 0 900",
                ".op",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.True(EqualsWithTol(0.9, RunOpSimulation(model, "V(out)")));
            Assert.Contains(model.Circuit, entity => entity.Name == "R1_rser");
        }

        [Fact]
        public void When_LtspiceResistorParallelParasiticIsRead_Expect_SynthesizedParallelResistance()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - resistor Rpar",
                "V1 in 0 1",
                "RTOP in out 1k",
                "R1 out 0 1k Rpar=1k",
                ".op",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.True(EqualsWithTol(1.0 / 3.0, RunOpSimulation(model, "V(out)")));
            Assert.Contains(model.Circuit, entity => entity.Name == "R1_rpar");
        }

        [Fact]
        public void When_LtspiceResistorParallelCapacitanceIsRead_Expect_SynthesizedRcTransient()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - resistor Cpar",
                "I1 out 0 PULSE(0 -1m 1n 1n 1n 10u 20u)",
                "R1 out 0 1k Cpar=1n",
                ".tran 100n 5u",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.Contains(model.Circuit, entity => entity.Name == "R1_cpar");

            var exports = RunTransientSimulation(model, "V(out)");
            Assert.NotEmpty(exports);

            var oneTau = exports.OrderBy(export => Math.Abs(export.Item1 - 1e-6)).First();
            Assert.InRange(oneTau.Item2, 0.55, 0.72);
            Assert.InRange(exports.Last().Item2, 0.98, 1.01);
        }

        [Fact]
        public void When_LtspiceResistorSeriesParasiticIsInsideRepeatedSubcircuits_Expect_InternalNodesAreScoped()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - resistor Rser subcircuit scoping",
                "V1 in 0 1",
                "XU1 in out1 rcell",
                "XU2 in out2 rcell",
                "RLOAD1 out1 0 900",
                "RLOAD2 out2 0 400",
                ".subckt rcell p out",
                "R1 p out 90 Rser=10",
                ".ends rcell",
                ".op",
                ".save V(out1)",
                ".save V(out2)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);

            var exports = RunOpSimulation(model, "V(out1)", "V(out2)");
            Assert.True(EqualsWithTol(0.9, exports[0]));
            Assert.True(EqualsWithTol(0.8, exports[1]));
        }

        [Fact]
        public void When_LtspiceResistorHasCombinedParasitics_Expect_AllHelpersAndEquivalentOp()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - resistor combined parasitics",
                "V1 in 0 1",
                "R1 in out 90 Rser=10 Rpar=900 Cpar=1n",
                "RLOAD out 0 900",
                ".op",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.True(EqualsWithTol(10.0 / 11.0, RunOpSimulation(model, "V(out)")));
            Assert.Contains(model.Circuit, entity => entity.Name == "R1_rser");
            Assert.Contains(model.Circuit, entity => entity.Name == "R1_rpar");
            Assert.Contains(model.Circuit, entity => entity.Name == "R1_cpar");
        }

        [Fact]
        public void When_LtspiceModelBasedResistorHasSeriesParasitic_Expect_ModelPathUsesInternalNode()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - model-based resistor Rser",
                ".model rmod R(RSH=1)",
                "V1 in 0 1",
                "R1 in out rmod 90 Rser=10",
                "RLOAD out 0 900",
                ".op",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.True(EqualsWithTol(0.9, RunOpSimulation(model, "V(out)")));
            Assert.Contains(model.Circuit, entity => entity.Name == "R1_rser");
        }

        [Fact]
        public void When_LtspiceResistorParasiticValuesUseParameters_Expect_HelperExpressionsResolve()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - resistor parameterized parasitics",
                ".param rs=10",
                ".param rp=900",
                ".param cp=1n",
                "V1 in 0 1",
                "R1 in out 90 Rser={rs} Rpar={rp} Cpar={cp}",
                "RLOAD out 0 900",
                ".op",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.True(EqualsWithTol(10.0 / 11.0, RunOpSimulation(model, "V(out)")));
            Assert.Contains(model.Circuit, entity => entity.Name == "R1_rser");
            Assert.Contains(model.Circuit, entity => entity.Name == "R1_rpar");
            Assert.Contains(model.Circuit, entity => entity.Name == "R1_cpar");
        }

        [Fact]
        public void When_LtspiceCapacitorSeriesResistanceIsRead_Expect_SynthesizedSeriesResistance()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - capacitor Rser",
                "V1 in 0 PULSE(0 1 0 1n 1n 10u 20u)",
                "RDRIVE in out 900",
                "C1 out 0 1n Rser=100",
                ".tran 100n 5u",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.Contains(model.Circuit, entity => entity.Name == "C1_rser");

            var exports = RunTransientSimulation(model, "V(out)");
            Assert.NotEmpty(exports);

            var oneTau = exports.OrderBy(export => Math.Abs(export.Item1 - 1e-6)).First();
            Assert.InRange(oneTau.Item2, 0.60, 0.75);
            Assert.InRange(exports.Last().Item2, 0.98, 1.01);
        }

        [Fact]
        public void When_LtspiceCapacitorParallelResistanceIsRead_Expect_SynthesizedParallelResistance()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - capacitor Rpar",
                "V1 in 0 1",
                "C1 in out 1n Rpar=1k",
                "RLOAD out 0 1k",
                ".op",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.True(EqualsWithTol(0.5, RunOpSimulation(model, "V(out)")));
            Assert.Contains(model.Circuit, entity => entity.Name == "C1_rpar");
        }

        [Fact]
        public void When_LtspiceModelBasedCapacitorHasParallelParasitic_Expect_ModelPathUsesHelper()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - model-based capacitor Rpar",
                ".model cmod C CJ=1e-6",
                "V1 in 0 1",
                "C1 in out cmod L=1u W=1u Rpar=1k",
                "RLOAD out 0 1k",
                ".op",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.True(EqualsWithTol(0.5, RunOpSimulation(model, "V(out)")));
            Assert.Contains(model.Circuit, entity => entity.Name == "C1_rpar");
        }

        [Fact]
        public void When_LtspiceCapacitorParallelCapacitanceIsRead_Expect_SynthesizedParallelCapacitor()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - capacitor Cpar",
                "I1 out 0 PULSE(0 -1m 1n 1n 1n 10u 20u)",
                "C1 out 0 1n Cpar=1n",
                "RLOAD out 0 1k",
                ".tran 100n 8u",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.Contains(model.Circuit, entity => entity.Name == "C1_cpar");

            var exports = RunTransientSimulation(model, "V(out)");
            Assert.NotEmpty(exports);

            var oneTau = exports.OrderBy(export => Math.Abs(export.Item1 - 2e-6)).First();
            Assert.InRange(oneTau.Item2, 0.55, 0.72);
            Assert.InRange(exports.Last().Item2, 0.97, 1.01);
        }

        [Fact]
        public void When_LtspiceCapacitorParallelCapacitanceUsesParameter_Expect_HelperExpressionAffectsTransient()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - capacitor parameterized Cpar",
                ".param cp=1n",
                "I1 out 0 PULSE(0 -1m 1n 1n 1n 10u 20u)",
                "C1 out 0 1n Cpar={cp}",
                "RLOAD out 0 1k",
                ".tran 100n 8u",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.Contains(model.Circuit, entity => entity.Name == "C1_cpar");

            var exports = RunTransientSimulation(model, "V(out)");
            Assert.NotEmpty(exports);

            var oneTau = exports.OrderBy(export => Math.Abs(export.Item1 - 2e-6)).First();
            Assert.InRange(oneTau.Item2, 0.55, 0.72);
            Assert.InRange(exports.Last().Item2, 0.97, 1.01);
        }

        [Fact]
        public void When_LtspiceCapacitorSeriesInductanceIsRead_Expect_SynthesizedInductor()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - capacitor Lser",
                "C1 out 0 1n Lser=1n",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.Contains(model.Circuit, entity => entity.Name == "C1_lser");
        }

        [Fact]
        public void When_LtspiceCapacitorHasCombinedParasitics_Expect_AllHelpersAreSynthesized()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - capacitor combined parasitics",
                "C1 out 0 1n Rser=1 Lser=1n Rpar=1k Cpar=1n",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.Contains(model.Circuit, entity => entity.Name == "C1_rser");
            Assert.Contains(model.Circuit, entity => entity.Name == "C1_lser");
            Assert.Contains(model.Circuit, entity => entity.Name == "C1_rpar");
            Assert.Contains(model.Circuit, entity => entity.Name == "C1_cpar");
        }

        [Fact]
        public void When_LtspiceCapacitorParallelResistanceIsInsideRepeatedSubcircuits_Expect_InternalHelpersAreScoped()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - capacitor Rpar subcircuit scoping",
                "V1 in 0 1",
                "XU1 in out1 ccell",
                "XU2 in out2 ccell",
                "RLOAD1 out1 0 1k",
                "RLOAD2 out2 0 3k",
                ".subckt ccell p out",
                "C1 p out 1n Rpar=1k",
                ".ends ccell",
                ".op",
                ".save V(out1)",
                ".save V(out2)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);

            var exports = RunOpSimulation(model, "V(out1)", "V(out2)");
            Assert.True(EqualsWithTol(0.5, exports[0]));
            Assert.True(EqualsWithTol(0.75, exports[1]));
        }

        [Fact]
        public void When_LtspiceInductorSeriesResistanceIsRead_Expect_SynthesizedSeriesResistance()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - inductor Rser",
                "V1 in 0 1",
                "L1 in out 1m Rser=10",
                "RLOAD out 0 90",
                ".op",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.True(EqualsWithTol(0.9, RunOpSimulation(model, "V(out)")));
            Assert.IsType<Resistor>(model.Circuit["L1_rser"]);
        }

        [Fact]
        public void When_LtspiceInductorParallelResistanceIsRead_Expect_SynthesizedTransientShunt()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - inductor Rpar",
                "I1 out 0 PULSE(0 -1m 1n 1n 1n 10u 20u)",
                "L1 out 0 1m Rpar=1k",
                ".tran 100n 5u",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.IsType<Resistor>(model.Circuit["L1_rpar"]);

            var exports = RunTransientSimulation(model, "V(out)");
            Assert.NotEmpty(exports);

            var oneTau = exports.OrderBy(export => Math.Abs(export.Item1 - 1e-6)).First();
            Assert.InRange(oneTau.Item2, 0.28, 0.45);
            Assert.InRange(exports.Last().Item2, -0.02, 0.02);
        }

        [Fact]
        public void When_LtspiceInductorRlshuntIsRead_Expect_SynthesizedTransientShunt()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - inductor RLshunt",
                "I1 out 0 PULSE(0 -1m 1n 1n 1n 10u 20u)",
                "L1 out 0 1m RLshunt=1k",
                ".tran 100n 5u",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.IsType<Resistor>(model.Circuit["L1_rlshunt"]);

            var exports = RunTransientSimulation(model, "V(out)");
            Assert.NotEmpty(exports);

            var oneTau = exports.OrderBy(export => Math.Abs(export.Item1 - 1e-6)).First();
            Assert.InRange(oneTau.Item2, 0.28, 0.45);
            Assert.InRange(exports.Last().Item2, -0.02, 0.02);
        }

        [Fact]
        public void When_LtspiceInductorParallelCapacitanceIsRead_Expect_SynthesizedParallelCapacitor()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - inductor Cpar",
                "L1 out 0 1m Cpar=1n",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.IsType<Capacitor>(model.Circuit["L1_cpar"]);
        }

        [Fact]
        public void When_LtspiceInductorSeriesInductanceIsRead_Expect_SynthesizedSeriesInductor()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - inductor Lser",
                "L1 out 0 1m Lser=1u",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.IsType<Inductor>(model.Circuit["L1_lser"]);
        }

        [Fact]
        public void When_LtspiceInductorHasCombinedParasitics_Expect_AllHelpersAreSynthesized()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - inductor combined parasitics",
                "L1 out 0 1m Rser=1 Lser=1u Rpar=1k RLshunt=2k Cpar=1n",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.IsType<Resistor>(model.Circuit["L1_rser"]);
            Assert.IsType<Inductor>(model.Circuit["L1_lser"]);
            Assert.IsType<Resistor>(model.Circuit["L1_rpar"]);
            Assert.IsType<Resistor>(model.Circuit["L1_rlshunt"]);
            Assert.IsType<Capacitor>(model.Circuit["L1_cpar"]);
        }

        [Fact]
        public void When_LtspiceInductorParasiticValuesUseParameters_Expect_HelperExpressionsResolve()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - inductor parameterized parasitics",
                ".param rs=10",
                ".param ls=1u",
                ".param rp=1k",
                ".param rls=2k",
                ".param cp=1n",
                "V1 in 0 1",
                "L1 in out 1m Rser={rs} Lser={ls} Rpar={rp} RLshunt={rls} Cpar={cp}",
                "RLOAD out 0 90",
                ".op",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);

            var inToOutResistance = 1.0 / ((1.0 / 10.0) + (1.0 / 1000.0) + (1.0 / 2000.0));
            var expected = 90.0 / (90.0 + inToOutResistance);

            Assert.True(EqualsWithTol(expected, RunOpSimulation(model, "V(out)")));
            Assert.IsType<Resistor>(model.Circuit["L1_rser"]);
            Assert.IsType<Inductor>(model.Circuit["L1_lser"]);
            Assert.IsType<Resistor>(model.Circuit["L1_rpar"]);
            Assert.IsType<Resistor>(model.Circuit["L1_rlshunt"]);
            Assert.IsType<Capacitor>(model.Circuit["L1_cpar"]);
        }

        [Fact]
        public void When_LtspiceInductorSeriesParasiticIsInsideRepeatedSubcircuits_Expect_InternalNodesAreScoped()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - inductor Rser subcircuit scoping",
                "V1 in 0 1",
                "XU1 in out1 lcell",
                "XU2 in out2 lcell",
                "RLOAD1 out1 0 90",
                "RLOAD2 out2 0 40",
                ".subckt lcell p out",
                "L1 p out 1m Rser=10",
                ".ends lcell",
                ".op",
                ".save V(out1)",
                ".save V(out2)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);

            var exports = RunOpSimulation(model, "V(out1)", "V(out2)");
            Assert.True(EqualsWithTol(0.9, exports[0]));
            Assert.True(EqualsWithTol(0.8, exports[1]));
        }

        [Theory]
        [InlineData("R1 out 0 1k Rser=1", "Rser")]
        [InlineData("R1 out 0 1k Rpar=1Meg", "Rpar")]
        [InlineData("C1 out 0 1n Rser=1", "Rser")]
        [InlineData("C1 out 0 1n Lser=1n", "Lser")]
        [InlineData("C1 out 0 1n Rpar=1k", "Rpar")]
        [InlineData("C1 out 0 1n Cpar=1n", "Cpar")]
        [InlineData("L1 out 0 1u Rser=1", "Rser")]
        [InlineData("L1 out 0 1u Lser=1n", "Lser")]
        [InlineData("L1 out 0 1u Rpar=1k", "Rpar")]
        [InlineData("L1 out 0 1u RLshunt=1k", "RLshunt")]
        [InlineData("L1 out 0 1u Cpar=1p", "Cpar")]
        public void When_PassiveParasiticIsReadWithoutLtspiceCompatibility_Expect_DefaultError(
            string componentLine,
            string parameterName)
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.None,
                "LTspice P3 - default passive parasitic",
                componentLine,
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, parameterName);
        }

        [Theory]
        [InlineData("C1 out 0 Q=1n*x", "Q", "charge-defined")]
        [InlineData("C1 out 0 Q=1n*x Rser=1", "Q", "charge-defined")]
        [InlineData("L1 out 0 Flux=1m*tanh(x)", "Flux", "flux-defined")]
        public void When_LtspiceUnsupportedPassiveInstanceParameterChangesTopology_Expect_TargetedError(
            string componentLine,
            string parameterName,
            string expectedReason)
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - passive instance parameter",
                componentLine,
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, "LTspice");
            AssertErrorContains(model.ValidationResult, parameterName);
            AssertErrorContains(model.ValidationResult, expectedReason);
        }

        [Theory]
        [InlineData("Ron")]
        [InlineData("Roff")]
        [InlineData("Vfwd")]
        [InlineData("Ilimit")]
        [InlineData("Epsilon")]
        public void When_LtspiceIdealDiodeModelParameterIsRead_Expect_TargetedError(string parameterName)
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - ideal diode",
                $".model dmod D({parameterName}=1)",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, "LTspice");
            AssertErrorContains(model.ValidationResult, parameterName);
            AssertErrorContains(model.ValidationResult, "ideal-diode");
        }

        [Theory]
        [InlineData("Ilimit")]
        public void When_LtspiceSwitchModelParameterChangesBehavior_Expect_TargetedError(string parameterName)
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - switch unsupported parameter",
                $".model smod SW(Ron=1 Roff=1Meg Vt=0 Vh=0 {parameterName}=1)",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, "LTspice");
            AssertErrorContains(model.ValidationResult, parameterName);
            AssertErrorContains(model.ValidationResult, "switch");
        }

        [Fact]
        public void When_LtspiceVdmosModelIsRead_Expect_EngineRequiredError()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - VDMOS",
                ".model pwr VDMOS(Ron=1 Vto=2)",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, "VDMOS");
            AssertErrorContains(model.ValidationResult, "engine support");
        }

        [Fact]
        public void When_LtspiceHighMosLevelIsRead_Expect_TargetedError()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - MOS level",
                ".model nmod NMOS(level=8)",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, "LTspice");
            AssertErrorContains(model.ValidationResult, "level 8");
        }

        [Fact]
        public void When_LtspiceThreeTerminalMosSyntaxIsRead_Expect_TargetedError()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - three-terminal MOS",
                "M1 d g s pwr",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, "three-terminal MOS");
            AssertErrorContains(model.ValidationResult, "engine support");
        }

        [Fact]
        public void When_LtspiceLossyTransmissionLineIsRead_Expect_EngineRequiredError()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - LTRA",
                "O1 in 0 out 0 lossy",
                ".model lossy LTRA(R=1 L=1u C=1n len=1)",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, "O/LTRA");
            AssertErrorContains(model.ValidationResult, "engine support");
        }

        [Fact]
        public void When_LtspiceUniformRcLineIsRead_Expect_EngineRequiredError()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - URC",
                "U1 a b 0 urc L=1",
                ".model urc URC(Rperl=1 Cperl=1p)",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, "U/URC");
            AssertErrorContains(model.ValidationResult, "engine support");
        }

        private static SpiceSharpModel GetSpiceSharpModelWithCompatibility(CompatibilityOptions compatibility, params string[] lines)
        {
            var text = string.Join(Environment.NewLine, lines);
            var parser = new SpiceNetlistParser();

            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;
            parser.Settings.Compatibility = compatibility;

            var parserResult = parser.ParseNetlist(text);

            var spiceSharpSettings = new SpiceNetlistReaderSettings(
                new SpiceNetlistCaseSensitivitySettings(),
                () => parser.Settings.WorkingDirectory,
                Encoding.Default)
            {
                Compatibility = compatibility,
            };

            var spiceSharpReader = new SpiceNetlistReader(spiceSharpSettings);
            return spiceSharpReader.Read(parserResult.FinalModel);
        }

        private static void AssertNoValidationIssues(ValidationEntryCollection validation)
        {
            var messages = ValidationMessages(validation);
            Assert.False(validation.HasError, "Unexpected validation error: " + messages);
            Assert.False(validation.HasWarning, "Unexpected validation warning: " + messages);
        }

        private static void AssertErrorContains(ValidationEntryCollection validation, string expected)
        {
            var messages = string.Join(Environment.NewLine, validation.Errors.Select(error => error.Message));
            Assert.Contains(expected, messages, StringComparison.OrdinalIgnoreCase);
        }

        private static void AssertWarningContains(ValidationEntryCollection validation, string expected)
        {
            var messages = string.Join(Environment.NewLine, validation.Warnings.Select(warning => warning.Message));
            Assert.Contains(expected, messages, StringComparison.OrdinalIgnoreCase);
        }

        private static string ValidationMessages(ValidationEntryCollection validation)
        {
            return string.Join(Environment.NewLine, validation.Select(entry => entry.Message));
        }
    }
}
