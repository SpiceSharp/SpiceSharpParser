using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.CustomComponents;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice;
using System;
using System.Linq;
using System.Numerics;
using Xunit;

namespace SpiceSharpParser.Tests.CustomComponents
{
    public class IdealDiodeTests
    {
        [Fact]
        public void Op_WhenForwardBiased_UsesOnResistance()
        {
            double current = RunOpCurrent(3.0, diode =>
            {
                diode.SetParameter("ron", 2.0);
                diode.SetParameter("roff", 1e9);
                diode.SetParameter("vfwd", 1.0);
            });

            // Above Vfwd the branch uses Ron, so current is (3 V - 1 V) / 2 ohm.
            AssertClose(1.0, current, 1e-9);
        }

        [Fact]
        public void Op_WhenBelowForwardVoltage_UsesOffResistance()
        {
            double current = RunOpCurrent(0.5, diode =>
            {
                diode.Parameters.OnResistance = 2.0;
                diode.Parameters.OffResistance = 1e9;
                diode.Parameters.ForwardVoltage = 1.0;
            });

            // Below Vfwd the branch stays on Roff, so current is 0.5 V / 1e9 ohm.
            AssertClose(0.5e-9, current, 1e-15);
        }

        [Fact]
        public void Op_WhenReverseBreakdown_UsesReverseResistance()
        {
            double current = RunOpCurrent(-6.0, diode =>
            {
                diode.Parameters.OnResistance = 2.0;
                diode.Parameters.OffResistance = 1e9;
                diode.Parameters.ForwardVoltage = 1.0;
                diode.Parameters.ReverseVoltage = 2.0;
                diode.Parameters.ReverseResistance = 4.0;
            });

            // Past -Vrev the reverse branch uses Rrev: (-6 V + 2 V) / 4 ohm.
            AssertClose(-1.0, current, 1e-9);
        }

        [Fact]
        public void Op_WhenMultipliersAreSet_ScalesParallelAndSeries()
        {
            double current = RunOpCurrent(3.0, diode =>
            {
                diode.Parameters.OnResistance = 2.0;
                diode.Parameters.OffResistance = 1e9;
                diode.Parameters.ForwardVoltage = 1.0;
                diode.Parameters.ParallelMultiplier = 2.0;
                diode.Parameters.SeriesMultiplier = 2.0;
            });

            // N=2 halves the local voltage, then M=2 doubles the resulting current.
            AssertClose(0.5, current, 1e-9);
        }

        [Fact]
        public void Op_WhenAreaIsSet_ScalesCurrent()
        {
            double current = RunOpCurrent(3.0, diode =>
            {
                diode.Parameters.OnResistance = 2.0;
                diode.Parameters.OffResistance = 1e9;
                diode.Parameters.ForwardVoltage = 1.0;
                diode.Parameters.Area = 3.0;
            });

            // The unscaled forward current is 1 A, and area=3 scales it to 3 A.
            AssertClose(3.0, current, 1e-9);
        }

        [Fact]
        public void Op_WhenSeriesResistanceIsSet_AddsResistance()
        {
            double current = RunOpCurrent(5.0, diode =>
            {
                diode.Parameters.OnResistance = 2.0;
                diode.Parameters.OffResistance = 1e9;
                diode.Parameters.ForwardVoltage = 1.0;
                diode.Parameters.Resistance = 3.0;
            });

            // The total forward path is Ron + Rs, so current is (5 V - 1 V) / 5 ohm.
            AssertClose(0.8, current, 1e-9);
        }

        [Fact]
        public void Op_WhenReverseResistanceIsOmitted_UsesOnResistance()
        {
            double current = RunOpCurrent(-6.0, diode =>
            {
                diode.Parameters.OnResistance = 2.0;
                diode.Parameters.OffResistance = 1e9;
                diode.Parameters.ForwardVoltage = 1.0;
                diode.Parameters.ReverseVoltage = 2.0;
            });

            // Without Rrev, reverse breakdown falls back to Ron: (-6 V + 2 V) / 2 ohm.
            AssertClose(-2.0, current, 1e-9);
        }

        [Fact]
        public void Op_WhenForwardCurrentLimitIsSet_LimitsCurrentSmoothly()
        {
            double current = RunOpCurrent(5.0, diode =>
            {
                diode.Parameters.OnResistance = 1.0;
                diode.Parameters.OffResistance = 1e9;
                diode.Parameters.ForwardVoltage = 0.0;
                diode.Parameters.ForwardCurrentLimit = 2.0;
            });

            // Current limiting uses limit * tanh(raw / limit), with raw forward current of 5 A.
            AssertClose(2.0 * Math.Tanh(2.5), current, 1e-9);
        }

        [Fact]
        public void Op_WhenReverseCurrentLimitIsSet_LimitsCurrentSmoothly()
        {
            double current = RunOpCurrent(-6.0, diode =>
            {
                diode.Parameters.OnResistance = 1.0;
                diode.Parameters.OffResistance = 1e9;
                diode.Parameters.ForwardVoltage = 0.0;
                diode.Parameters.ReverseVoltage = 0.0;
                diode.Parameters.ReverseResistance = 1.0;
                diode.Parameters.ReverseCurrentLimit = 2.0;
            });

            // The raw reverse current is -6 A, then the same tanh limiter caps it smoothly.
            AssertClose(-2.0 * Math.Tanh(3.0), current, 1e-9);
        }

        [Fact]
        public void Op_WhenForwardEpsilonIsSet_SmoothsTurnOnKnee()
        {
            double current = RunOpCurrent(0.95, diode =>
            {
                diode.Parameters.OnResistance = 1.0;
                diode.Parameters.OffResistance = 1e12;
                diode.Parameters.ForwardVoltage = 1.0;
                diode.Parameters.ForwardEpsilon = 0.2;
            });

            // At 0.05 V into a 0.2 V smoothing band, the ramp integral is 0.05^2 / (2 * 0.2).
            AssertClose(0.00625, current, 1e-6);
        }

        [Fact]
        public void Op_WhenPropertiesAreExported_ReturnsVoltageConductanceAndPower()
        {
            var values = RunOpProperties(3.0, diode =>
            {
                diode.Parameters.OnResistance = 2.0;
                diode.Parameters.OffResistance = 1e9;
                diode.Parameters.ForwardVoltage = 1.0;
            });

            // The branch is on Ron: current is 1 A, gd is 1 / 2 ohm, and power is 3 V * 1 A.
            AssertClose(1.0, values.Current, 1e-9);
            AssertClose(3.0, values.Voltage, 1e-12);
            AssertClose(0.5, values.Conductance, 1e-12);
            AssertClose(3.0, values.Power, 1e-9);
        }

        [Fact]
        public void Op_WhenSeriesResistanceIsSet_ExportsTerminalProperties()
        {
            var values = RunOpProperties(5.0, diode =>
            {
                diode.Parameters.OnResistance = 2.0;
                diode.Parameters.OffResistance = 1e9;
                diode.Parameters.ForwardVoltage = 1.0;
                diode.Parameters.Resistance = 3.0;
            });

            // The terminal branch includes both the internal ideal diode and Rs.
            AssertClose(0.8, values.Current, 1e-9);
            AssertClose(5.0, values.Voltage, 1e-12);
            AssertClose(2.6, values.JunctionVoltage, 1e-9);
            AssertClose(0.2, values.Conductance, 1e-12);
            AssertClose(4.0, values.Power, 1e-9);
        }

        [Fact]
        public void Op_WhenSeriesMultiplierIsSet_ExportsTerminalConductance()
        {
            var values = RunOpProperties(3.0, diode =>
            {
                diode.Parameters.OnResistance = 2.0;
                diode.Parameters.OffResistance = 1e9;
                diode.Parameters.ForwardVoltage = 1.0;
                diode.Parameters.ParallelMultiplier = 2.0;
                diode.Parameters.SeriesMultiplier = 2.0;
            });

            // Two parallel strings of two series cells behave like 0.5 S at the external terminals.
            AssertClose(0.5, values.Current, 1e-9);
            AssertClose(3.0, values.Voltage, 1e-12);
            AssertClose(3.0, values.JunctionVoltage, 1e-12);
            AssertClose(0.5, values.Conductance, 1e-12);
            AssertClose(1.5, values.Power, 1e-9);
        }

        [Fact]
        public void Ac_WhenForwardBiased_UsesOperatingPointConductance()
        {
            var diode = new IdealDiode("D1", "out", "0");
            diode.Parameters.OnResistance = 2.0;
            diode.Parameters.OffResistance = 1e9;
            diode.Parameters.ForwardVoltage = 1.0;

            var circuit = new Circuit(
                new VoltageSource("V1", "in", "0", 3.0).SetParameter("acmag", 1.0),
                new Resistor("R1", "in", "out", 1.0),
                diode);

            var ac = new AC("ac", new DecadeSweep(1.0, 1e3, 1));
            var export = new ComplexVoltageExport(ac, "out");

            foreach (int ignored in ac.Run(circuit, AC.ExportSmallSignal))
            {
                // The biased diode is a 0.5 S shunt, so the 1 ohm divider gain is 1 / (1 + 0.5).
                AssertClose(2.0 / 3.0, export.Value.Real, 1e-9);
                AssertClose(0.0, export.Value.Imaginary, 1e-12);
            }
        }

        [Fact]
        public void Ac_WhenBelowForwardVoltage_UsesOffConductance()
        {
            var diode = new IdealDiode("D1", "out", "0");
            diode.Parameters.OnResistance = 2.0;
            diode.Parameters.OffResistance = 1e9;
            diode.Parameters.ForwardVoltage = 1.0;

            var circuit = new Circuit(
                new VoltageSource("V1", "in", "0", 0.5).SetParameter("acmag", 1.0),
                new Resistor("R1", "in", "out", 1.0),
                diode);

            var ac = new AC("ac", new DecadeSweep(1.0, 1e3, 1));
            var export = new ComplexVoltageExport(ac, "out");

            foreach (int ignored in ac.Run(circuit, AC.ExportSmallSignal))
            {
                // In the off region the shunt conductance is 1 / Roff, giving gain 1 / (1 + 1e-9).
                AssertClose(1.0 / (1.0 + 1e-9), export.Value.Real, 1e-9);
                AssertClose(0.0, export.Value.Imaginary, 1e-12);
            }
        }

        [Fact]
        public void Ac_WhenSeriesResistanceIsSet_ExportsTerminalProperties()
        {
            var values = RunAcProperties(5.0, diode =>
            {
                diode.Parameters.OnResistance = 2.0;
                diode.Parameters.OffResistance = 1e9;
                diode.Parameters.ForwardVoltage = 1.0;
                diode.Parameters.Resistance = 3.0;
            });

            // A 1 V small-signal source sees Ron + Rs = 5 ohm, so the AC current is 0.2 A.
            AssertComplexClose(new Complex(0.2, 0.0), values.Current, 1e-9);
            AssertComplexClose(new Complex(1.0, 0.0), values.Voltage, 1e-12);
            AssertComplexClose(new Complex(0.4, 0.0), values.JunctionVoltage, 1e-9);
            AssertComplexClose(new Complex(0.2, 0.0), values.Power, 1e-9);
        }

        [Fact]
        public void Ac_WhenSeriesMultiplierIsSet_ExportsTerminalPower()
        {
            var values = RunAcProperties(3.0, diode =>
            {
                diode.Parameters.OnResistance = 2.0;
                diode.Parameters.OffResistance = 1e9;
                diode.Parameters.ForwardVoltage = 1.0;
                diode.Parameters.ParallelMultiplier = 2.0;
                diode.Parameters.SeriesMultiplier = 2.0;
            });

            // The exported AC power uses terminal voltage, not the per-cell internal voltage.
            AssertComplexClose(new Complex(0.5, 0.0), values.Current, 1e-9);
            AssertComplexClose(new Complex(1.0, 0.0), values.Voltage, 1e-12);
            AssertComplexClose(new Complex(1.0, 0.0), values.JunctionVoltage, 1e-12);
            AssertComplexClose(new Complex(0.5, 0.0), values.Power, 1e-9);
        }

        [Theory]
        [InlineData(-6.0, -1.0, 0.25)]
        [InlineData(0.5, 0.5e-9, 1e-9)]
        [InlineData(3.0, 1.0, 0.5)]
        public void Op_WhenSweepingLtspiceStyleReferenceModel_MatchesGoldenValues(
            double voltage,
            double expectedCurrent,
            double expectedConductance)
        {
            var values = RunOpProperties(voltage, diode =>
            {
                diode.Parameters.OnResistance = 2.0;
                diode.Parameters.OffResistance = 1e9;
                diode.Parameters.ForwardVoltage = 1.0;
                diode.Parameters.ReverseVoltage = 2.0;
                diode.Parameters.ReverseResistance = 4.0;
            });

            // Fixed compatibility points for LTspice-style Ron/Roff/Vfwd/Vrev/Rrev behavior.
            AssertClose(expectedCurrent, values.Current, 1e-9);
            AssertClose(voltage, values.Voltage, 1e-12);
            AssertClose(expectedConductance, values.Conductance, 1e-12);
            AssertClose(voltage * expectedCurrent, values.Power, 1e-9);
        }

        [Fact]
        public void Op_WhenLtspiceStyleScalingAndSeriesResistanceAreSet_MatchesGoldenTerminalValues()
        {
            var values = RunOpProperties(10.0, diode =>
            {
                diode.Parameters.OnResistance = 2.0;
                diode.Parameters.OffResistance = 1e9;
                diode.Parameters.ForwardVoltage = 1.0;
                diode.Parameters.Resistance = 3.0;
                diode.Parameters.Area = 2.0;
                diode.Parameters.ParallelMultiplier = 3.0;
                diode.Parameters.SeriesMultiplier = 2.0;
            });

            // Effective threshold is N*Vfwd = 2 V and effective resistance is (Ron + Rs)*N/(area*M).
            AssertClose(4.8, values.Current, 1e-9);
            AssertClose(10.0, values.Voltage, 1e-12);
            AssertClose(5.2, values.JunctionVoltage, 1e-9);
            AssertClose(0.6, values.Conductance, 1e-12);
            AssertClose(48.0, values.Power, 1e-9);
        }

        [Fact]
        public void Parser_WhenCustomComponentsEnabled_MapsIdealDiodeModel()
        {
            var model = ReadWithCustomComponents(
                "Ideal diode parser",
                "V1 in 0 3",
                "D1 in 0 ideal",
                ".model ideal D(Ron=2 Roff=1e9 Vfwd=1)",
                ".op",
                ".end");

            // Ideal-only model parameters should switch the diode reader to the custom ideal-diode entity.
            AssertNoValidationErrors(model);
            Assert.Single(model.Circuit.OfType<IdealDiode>());

            // Ron, Roff, and Vfwd make this a custom ideal diode; the OP current is (3 V - 1 V) / 2 ohm.
            AssertClose(1.0, RunOpCurrent(model), 1e-9);
        }

        [Fact]
        public void Parser_WhenIdealModelHasClassicParameters_IgnoresThem()
        {
            var model = ReadWithCustomComponents(
                "Ideal diode parser",
                "V1 in 0 3",
                "D1 in 0 ideal",
                ".model ideal D(Is=1e-12 N=2 M=0.3 Ron=2 Roff=1e9 Vfwd=1)",
                ".op",
                ".end");

            // Classic diode parameters remain accepted metadata for this path and must not block ideal mapping.
            AssertNoValidationErrors(model);
            Assert.Single(model.Circuit.OfType<IdealDiode>());

            // Classic diode model parameters are ignored once an ideal-only parameter selects this model.
            AssertClose(1.0, RunOpCurrent(model), 1e-9);
        }

        [Fact]
        public void Parser_WhenInstanceOverridesModelParameters_UsesOverride()
        {
            var model = ReadWithCustomComponents(
                "Ideal diode parser",
                "V1 in 0 3",
                "D1 in 0 ideal Ron=2",
                ".model ideal D(Ron=10 Roff=1e9 Vfwd=1)",
                ".op",
                ".end");

            // The model still maps to IdealDiode; the instance parameter is what changes the effective Ron.
            AssertNoValidationErrors(model);
            Assert.Single(model.Circuit.OfType<IdealDiode>());

            // The instance Ron=2 overrides the model Ron=10, so the forward current stays 1 A.
            AssertClose(1.0, RunOpCurrent(model), 1e-9);
        }

        [Fact]
        public void Parser_WhenInvalidInstanceOverrideIsSet_DoesNotShadowModelParameter()
        {
            var model = ReadWithCustomComponents(
                "Ideal diode parser",
                "V1 in 0 5",
                "D1 in 0 ideal Ron=0",
                ".model ideal D(Ron=2 Roff=1e9 Vfwd=1)",
                ".op",
                ".end");

            // Ron=0 violates the ideal-diode parameter constraints and should be reported with its source name.
            Assert.True(model.ValidationResult.HasError);
            Assert.Contains("Ron", GetValidationMessages(model));
            Assert.Single(model.Circuit.OfType<IdealDiode>());

            // The invalid instance Ron must not mark an override; the model Ron=2 remains effective.
            AssertClose(2.0, RunOpCurrent(model), 1e-9);
        }

        [Fact]
        public void Parser_WhenAreaValueIsPositional_ScalesIdealDiode()
        {
            var model = ReadWithCustomComponents(
                "Ideal diode parser",
                "V1 in 0 3",
                "D1 in 0 ideal 3",
                ".model ideal D(Ron=2 Roff=1e9 Vfwd=1)",
                ".op",
                ".end");

            AssertNoValidationErrors(model);
            Assert.Single(model.Circuit.OfType<IdealDiode>());

            // The positional value is diode area, which scales the base 1 A branch current by 3.
            AssertClose(3.0, RunOpCurrent(model), 1e-9);
        }

        [Fact]
        public void Parser_WhenInstanceMultipliersAreSet_ScalesIdealDiode()
        {
            var model = ReadWithCustomComponents(
                "Ideal diode parser",
                "V1 in 0 3",
                "D1 in 0 ideal M=2 N=2",
                ".model ideal D(Ron=2 Roff=1e9 Vfwd=1)",
                ".op",
                ".end");

            AssertNoValidationErrors(model);
            Assert.Single(model.Circuit.OfType<IdealDiode>());

            // This mirrors the direct multiplier test: N changes local voltage, M scales current.
            AssertClose(0.5, RunOpCurrent(model), 1e-9);
        }

        [Fact]
        public void Parser_WhenZeroScaleIsSet_ReportsValidationError()
        {
            var areaModel = ReadWithCustomComponents(
                "Ideal diode parser",
                "V1 in 0 3",
                "D1 in 0 ideal Area=0",
                ".model ideal D(Ron=2 Roff=1e9 Vfwd=1 Rs=3)",
                ".op",
                ".end");

            // Area participates in Rs scaling, so zero must fail validation instead of creating an infinite path.
            Assert.True(areaModel.ValidationResult.HasError);
            Assert.Contains("Area", GetValidationMessages(areaModel));

            var multiplierModel = ReadWithCustomComponents(
                "Ideal diode parser",
                "V1 in 0 3",
                "D1 in 0 ideal M=0",
                ".model ideal D(Ron=2 Roff=1e9 Vfwd=1 Rs=3)",
                ".op",
                ".end");

            // M=0 would remove the parallel scale denominator; it should be rejected like zero area.
            Assert.True(multiplierModel.ValidationResult.HasError, GetValidationMessages(multiplierModel));
        }

        [Fact]
        public void Parser_WhenModelHasDimensionBins_SelectsIdealDiodeByLengthAndWidth()
        {
            var model = ReadWithCustomComponents(
                "Ideal diode range parser",
                "V1 in1 0 5",
                "V2 in2 0 5",
                "D1 in1 0 ideal L=0.5 W=5",
                "D2 in2 0 ideal L=5 W=50",
                ".model ideal.fast D(Ron=2 Roff=1e9 Vfwd=1 Lmin=0.1 Lmax=1 Wmin=1 Wmax=10)",
                ".model ideal.slow D(Ron=4 Roff=1e9 Vfwd=1 Lmin=1 Lmax=10 Wmin=10 Wmax=100)",
                ".op",
                ".end");

            // D1 falls into the fast bin and D2 into the slow bin, so both should become custom ideal diodes.
            AssertNoValidationErrors(model);
            Assert.Equal(2, model.Circuit.OfType<IdealDiode>().Count());

            // At 5 V with Vfwd=1, Ron=2 gives 2 A and Ron=4 gives 1 A.
            AssertClose(2.0, RunOpCurrent(model, "D1"), 1e-9);
            AssertClose(1.0, RunOpCurrent(model, "D2"), 1e-9);
        }

        [Fact]
        public void Parser_WhenSelectedBinnedModelParameterIsStepped_UpdatesIdealDiode()
        {
            var model = ReadWithCustomComponents(
                "Ideal diode stepped binned model",
                "V1 in 0 5",
                "D1 in 0 ideal L=0.5 W=5",
                ".model ideal.fast D(Ron=2 Roff=1e9 Vfwd=1 Lmin=0.1 Lmax=1 Wmin=1 Wmax=10)",
                ".model ideal.slow D(Ron=8 Roff=1e9 Vfwd=1 Lmin=1 Lmax=10 Wmin=10 Wmax=100)",
                ".op",
                ".step D ideal.fast(Ron) LIST 2 4",
                ".end");

            // The concrete selected model name, ideal.fast, should be accepted as the stepped target.
            AssertNoValidationErrors(model);
            Assert.Equal(2, model.Simulations.Count);

            // The two OP runs use Ron=2 and Ron=4 respectively.
            var currents = RunOpCurrents(model, "D1");
            AssertClose(2.0, currents[0], 1e-9);
            AssertClose(1.0, currents[1], 1e-9);

            // Model sweep values are simulation-local overlays; the shared model should be restored afterward.
            AssertClose(2.0, model.Circuit.OfType<IdealDiodeModel>().Single(m => m.Name == "ideal.fast").Parameters.OnResistance, 1e-12);
        }

        [Fact]
        public void Parser_WhenModelSelectionExpressionCannotBeEvaluated_ReportsValidationError()
        {
            var model = ReadWithCustomComponents(
                "Ideal diode range parser",
                "V1 in 0 5",
                "D1 in 0 ideal L={missing_length}",
                ".model ideal.fast D(Ron=2 Roff=1e9 Vfwd=1 Lmin=0.1 Lmax=1)",
                ".op",
                ".end");

            // The netlist can be read before the L expression is evaluated during model selection.
            Assert.False(model.ValidationResult.HasError, GetValidationMessages(model));

            RunOpCurrent(model);

            // Running the simulation triggers range selection and surfaces the missing L/W expression.
            Assert.True(model.ValidationResult.HasError);
            Assert.Contains("L/W", GetValidationMessages(model));
        }

        [Fact]
        public void Parser_WhenModelParameterIsStepped_UpdatesIdealDiode()
        {
            var model = ReadWithCustomComponents(
                "Ideal diode stepped model",
                "V1 in 0 5",
                "D1 in 0 ideal",
                ".model ideal D(Ron=2 Roff=1e9 Vfwd=1)",
                ".op",
                ".step D ideal(Ron) LIST 2 4",
                ".end");

            // The list sweep produces one OP simulation per Ron value.
            AssertNoValidationErrors(model);
            Assert.Equal(2, model.Simulations.Count);

            // Ron=2 gives 2 A; Ron=4 doubles resistance and halves current to 1 A.
            var currents = RunOpCurrents(model, "D1");
            AssertClose(2.0, currents[0], 1e-9);
            AssertClose(1.0, currents[1], 1e-9);

            // After all stepped runs, the shared model parameter is restored to its original value.
            AssertClose(2.0, Assert.Single(model.Circuit.OfType<IdealDiodeModel>()).Parameters.OnResistance, 1e-12);
        }

        [Fact]
        public void Parser_WhenModelSeriesResistanceIsSteppedAcrossZero_UpdatesIdealDiode()
        {
            var model = ReadWithCustomComponents(
                "Ideal diode stepped Rs",
                "V1 in 0 5",
                "D1 in 0 ideal",
                ".model ideal D(Ron=2 Roff=1e9 Vfwd=1 Rs=0)",
                ".op",
                ".step D ideal(Rs) LIST 0 3",
                ".end");

            // The sweep covers the zero-Rs fast path and a positive series-resistance path.
            AssertNoValidationErrors(model);
            Assert.Equal(2, model.Simulations.Count);

            // With Rs=0 the current is (5 V - 1 V) / 2 ohm; with Rs=3 it is divided by Ron + Rs.
            var currents = RunOpCurrents(model, "D1");
            AssertClose(2.0, currents[0], 1e-9);
            AssertClose(0.8, currents[1], 1e-9);
        }

        [Fact]
        public void Parser_WhenOffFlagIsSet_SetsInitialOffFlag()
        {
            var model = ReadWithCustomComponents(
                "Ideal diode parser",
                "V1 in 0 3",
                "D1 in 0 ideal OFF",
                ".model ideal D(Ron=2 Roff=1e9 Vfwd=1)",
                ".op",
                ".end");

            AssertNoValidationErrors(model);
            var diode = Assert.Single(model.Circuit.OfType<IdealDiode>());

            // OFF is an initial-condition hint, not a steady-state current-law change.
            Assert.True(diode.Parameters.Off);
        }

        [Fact]
        public void Parser_WhenIgnoredInstanceParametersAreSet_DoesNotReportErrors()
        {
            var model = ReadWithCustomComponents(
                "Ideal diode parser",
                "V1 in 0 3",
                "D1 in 0 ideal Temp=50 Ic=0.2",
                ".model ideal D(Ron=2 Roff=1e9 Vfwd=1)",
                ".op",
                ".end");

            AssertNoValidationErrors(model);
            Assert.Single(model.Circuit.OfType<IdealDiode>());

            // Temp and Ic are intentionally accepted but ignored, leaving the same 1 A operating point.
            AssertClose(1.0, RunOpCurrent(model), 1e-9);
        }

        [Fact]
        public void Parser_WhenMetadataParametersAreSet_DoesNotReportErrors()
        {
            var model = ReadWithCustomComponents(
                "Ideal diode parser",
                "V1 in 0 3",
                "D1 in 0 ideal pn=ABC irms=2",
                ".model ideal D(Ron=2 Roff=1e9 Vfwd=1 mfg=ACME desc=FastSwitch)",
                ".op",
                ".end");

            AssertNoValidationErrors(model);
            Assert.Single(model.Circuit.OfType<IdealDiode>());

            // Metadata parameters should not affect simulation, so the ideal diode still conducts 1 A.
            AssertClose(1.0, RunOpCurrent(model), 1e-9);
        }

        [Fact]
        public void Parser_WhenUnsupportedInstanceParameterIsSet_ReportsValidationError()
        {
            var model = ReadWithCustomComponents(
                "Ideal diode parser",
                "V1 in 0 3",
                "D1 in 0 ideal Foo=1",
                ".model ideal D(Ron=2 Roff=1e9 Vfwd=1)",
                ".op",
                ".end");

            // Unsupported ideal-diode instance parameters should be rejected and reported with their original name.
            Assert.True(model.ValidationResult.HasError);
            Assert.Contains(
                "Foo",
                string.Join(Environment.NewLine, model.ValidationResult.Errors.Select(error => error.Message)));
        }

        [Fact]
        public void Parser_WhenRoffOnlyModelParameterIsSet_MapsIdealDiodeModel()
        {
            var model = ReadWithCustomComponents(
                "Ideal diode parser",
                "V1 in 0 2",
                "D1 in 0 ideal",
                ".model ideal D(Roff=1e9)",
                ".op",
                ".end");

            // Roff is an ideal-only model parameter, so the custom mapper should consume the diode.
            AssertNoValidationErrors(model);
            Assert.Single(model.Circuit.OfType<IdealDiode>());
            Assert.Empty(model.Circuit.OfType<Diode>());

            // Roff is enough to select the ideal model; defaults Ron=1 and Vfwd=0 make 2 V produce 2 A.
            AssertClose(2.0, RunOpCurrent(model), 1e-9);
        }

        [Fact]
        public void Parser_WhenCustomComponentsEnabled_KeepsClassicDiodeModel()
        {
            var model = ReadWithCustomComponents(
                "Classic diode parser",
                "V1 in 0 0.7",
                "D1 in 0 regular",
                ".model regular D(Is=1e-12 N=1)",
                ".op",
                ".end");

            // A normal diode model with no ideal-only parameters must remain SpiceSharp's built-in diode.
            AssertNoValidationErrors(model);
            Assert.Single(model.Circuit.OfType<Diode>());

            // This guards against accidentally treating every D model as an ideal diode.
            Assert.Empty(model.Circuit.OfType<IdealDiode>());
        }

        [Fact]
        public void Parser_WhenOnlySeriesResistanceModelParameterIsSet_KeepsClassicDiodeModel()
        {
            var model = ReadWithCustomComponents(
                "Classic diode parser",
                "V1 in 0 0.7",
                "D1 in 0 regular",
                ".model regular D(Rs=1)",
                ".op",
                ".end");

            // Rs alone is valid for classic diodes, so it must not trigger the ideal-diode mapper.
            AssertNoValidationErrors(model);
            Assert.Single(model.Circuit.OfType<Diode>());

            // Keeping this classic preserves LTspice compatibility for ordinary diode models.
            Assert.Empty(model.Circuit.OfType<IdealDiode>());
        }

        private static double RunOpCurrent(double voltage, Action<IdealDiode> configure)
        {
            var diode = new IdealDiode("D1", "in", "0");
            configure(diode);

            var circuit = new Circuit(
                new VoltageSource("V1", "in", "0", voltage),
                diode);

            var op = new OP("op");
            var export = new RealPropertyExport(op, "D1", "i");

            double current = double.NaN;
            foreach (int ignored in op.Run(circuit))
            {
                current = export.Value;
            }

            return current;
        }

        private static (double Current, double Voltage, double JunctionVoltage, double Conductance, double Power) RunOpProperties(
            double voltage,
            Action<IdealDiode> configure)
        {
            var diode = new IdealDiode("D1", "in", "0");
            configure(diode);

            var circuit = new Circuit(
                new VoltageSource("V1", "in", "0", voltage),
                diode);

            var op = new OP("op");
            var current = new RealPropertyExport(op, "D1", "i");
            var diodeVoltage = new RealPropertyExport(op, "D1", "v");
            var junctionVoltage = new RealPropertyExport(op, "D1", "vj");
            var conductance = new RealPropertyExport(op, "D1", "gd");
            var power = new RealPropertyExport(op, "D1", "p");

            double currentValue = double.NaN;
            double voltageValue = double.NaN;
            double junctionVoltageValue = double.NaN;
            double conductanceValue = double.NaN;
            double powerValue = double.NaN;
            foreach (int ignored in op.Run(circuit))
            {
                currentValue = current.Value;
                voltageValue = diodeVoltage.Value;
                junctionVoltageValue = junctionVoltage.Value;
                conductanceValue = conductance.Value;
                powerValue = power.Value;
            }

            return (currentValue, voltageValue, junctionVoltageValue, conductanceValue, powerValue);
        }

        private static (Complex Current, Complex Voltage, Complex JunctionVoltage, Complex Power) RunAcProperties(
            double voltage,
            Action<IdealDiode> configure)
        {
            var diode = new IdealDiode("D1", "in", "0");
            configure(diode);

            var circuit = new Circuit(
                new VoltageSource("V1", "in", "0", voltage).SetParameter("acmag", 1.0),
                diode);

            var ac = new AC("ac", new DecadeSweep(1.0, 1e3, 1));
            var current = new ComplexPropertyExport(ac, "D1", "i");
            var diodeVoltage = new ComplexPropertyExport(ac, "D1", "v");
            var junctionVoltage = new ComplexPropertyExport(ac, "D1", "vj");
            var power = new ComplexPropertyExport(ac, "D1", "p");

            Complex currentValue = new Complex(double.NaN, double.NaN);
            Complex voltageValue = new Complex(double.NaN, double.NaN);
            Complex junctionVoltageValue = new Complex(double.NaN, double.NaN);
            Complex powerValue = new Complex(double.NaN, double.NaN);
            foreach (int ignored in ac.Run(circuit, AC.ExportSmallSignal))
            {
                currentValue = current.Value;
                voltageValue = diodeVoltage.Value;
                junctionVoltageValue = junctionVoltage.Value;
                powerValue = power.Value;
            }

            return (currentValue, voltageValue, junctionVoltageValue, powerValue);
        }

        private static double RunOpCurrent(SpiceSharpModel model)
        {
            return RunOpCurrent(model, "D1");
        }

        private static double RunOpCurrent(SpiceSharpModel model, string entityName)
        {
            double current = double.NaN;
            var preparedSimulation = model.Simulations.FirstOrDefault(simulation => simulation is OP);
            if (preparedSimulation != null)
            {
                var export = new RealPropertyExport(preparedSimulation, entityName, "i");
                foreach (int code in preparedSimulation.InvokeEvents(preparedSimulation.Run(model.Circuit, -1)))
                {
                    if (code == OP.ExportOperatingPoint)
                    {
                        current = export.Value;
                    }
                }

                return current;
            }

            var op = new OP("verify");
            var fallbackExport = new RealPropertyExport(op, entityName, "i");
            foreach (int ignored in op.Run(model.Circuit))
            {
                current = fallbackExport.Value;
            }

            return current;
        }

        private static double[] RunOpCurrents(SpiceSharpModel model, string entityName)
        {
            return model.Simulations
                .Where(simulation => simulation is OP)
                .Select(simulation =>
                {
                    var export = new RealPropertyExport(simulation, entityName, "i");
                    double current = double.NaN;
                    foreach (int code in simulation.InvokeEvents(simulation.Run(model.Circuit, -1)))
                    {
                        if (code == OP.ExportOperatingPoint)
                        {
                            current = export.Value;
                        }
                    }

                    return current;
                })
                .ToArray();
        }

        private static SpiceSharpModel ReadWithCustomComponents(params string[] lines)
        {
            var text = string.Join(Environment.NewLine, lines);
            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;

            var parseResult = parser.ParseNetlist(text);
            var reader = new SpiceSharpReader();
            reader.Settings.UseCustomComponents();

            return reader.Read(parseResult.FinalModel);
        }

        private static void AssertNoValidationErrors(SpiceSharpModel model)
        {
            Assert.False(model.ValidationResult.HasError, GetValidationMessages(model));
        }

        private static string GetValidationMessages(SpiceSharpModel model)
        {
            return string.Join(Environment.NewLine, model.ValidationResult.Errors.Select(error => error.Message));
        }

        private static void AssertClose(double expected, double actual, double tolerance)
        {
            Assert.True(
                Math.Abs(expected - actual) <= tolerance,
                $"Expected {expected:R}, got {actual:R}.");
        }

        private static void AssertComplexClose(Complex expected, Complex actual, double tolerance)
        {
            AssertClose(expected.Real, actual.Real, tolerance);
            AssertClose(expected.Imaginary, actual.Imaginary, tolerance);
        }
    }
}
