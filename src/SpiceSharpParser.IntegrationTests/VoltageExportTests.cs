using System;
using System.Linq;
using Xunit;

namespace SpiceSharpParser.IntegrationTests
{
    public class VoltageExportTests : BaseTests
    {
        /// <summary>
        /// RC low-pass filter: R=1k, C=159nF => fc ≈ 1 kHz.
        /// At DC (1 Hz), gain ≈ 1 so VDB ≈ 0 dB.
        /// At high frequency (1 MHz), gain << 1 so VDB << 0 dB.
        /// Verifies VDB uses 20*log10 (not bare log10).
        /// </summary>
        [Fact]
        public void VDB_ReturnsCorrect20Log10_ForRCFilter()
        {
            var model = GetSpiceSharpModel(
                "VDB test - RC low-pass",
                "V1 IN 0 AC 1",
                "R1 IN OUT 1e3",
                "C1 OUT 0 159e-9",
                ".AC DEC 10 1 1e6",
                ".MEAS AC vdb_at_dc FIND VDB(OUT) AT=1",
                ".MEAS AC vdb_at_fc FIND VDB(OUT) AT=1e3",
                ".END");

            RunSimulations(model);

            // At 1 Hz (essentially DC), magnitude ≈ 1, so VDB ≈ 0 dB
            AssertMeasurementSuccess(model, "vdb_at_dc");
            double vdbDc = model.Measurements["vdb_at_dc"][0].Value;
            Assert.True(Math.Abs(vdbDc) < 0.1, $"VDB at DC should be ~0 dB, got {vdbDc}");

            // At cutoff (1 kHz), magnitude ≈ 1/sqrt(2), so VDB ≈ -3.01 dB
            AssertMeasurementSuccess(model, "vdb_at_fc");
            double vdbFc = model.Measurements["vdb_at_fc"][0].Value;
            Assert.True(Math.Abs(vdbFc - (-3.01)) < 0.5,
                $"VDB at cutoff should be ~-3 dB, got {vdbFc}");
        }

        /// <summary>
        /// Without the 20x multiplier, log10(1) = 0 would still pass,
        /// but log10(0.707) = -0.15 which is NOT -3 dB.
        /// This test catches the missing multiplier by checking magnitude well below unity.
        /// </summary>
        [Fact]
        public void VDB_HighFrequency_NotBareLog10()
        {
            var model = GetSpiceSharpModel(
                "VDB multiplier test",
                "V1 IN 0 AC 1",
                "R1 IN OUT 1e3",
                "C1 OUT 0 159e-9",
                ".AC DEC 10 1 1e6",
                ".MEAS AC vdb_high FIND VDB(OUT) AT=100e3",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "vdb_high");
            double vdbHigh = model.Measurements["vdb_high"][0].Value;

            // At 100 kHz (100x fc), gain ≈ 1/100, VDB ≈ -40 dB
            // Without 20x multiplier, bare log10(0.01) = -2, which is > -10
            Assert.True(vdbHigh < -30,
                $"VDB at 100kHz should be << -30 dB (around -40), got {vdbHigh}. " +
                "If ~-2, the 20*log10 multiplier is missing.");
        }

        /// <summary>
        /// At DC, phase should be ~0.
        /// At frequencies well above cutoff, phase should approach -pi/2 (-90°).
        /// Verifies VP returns phase (radians), not magnitude.
        /// </summary>
        [Fact]
        public void VP_ReturnsPhase_NotMagnitude()
        {
            var model = GetSpiceSharpModel(
                "VP test - RC low-pass",
                "V1 IN 0 AC 1",
                "R1 IN OUT 1e3",
                "C1 OUT 0 159e-9",
                ".AC DEC 10 1 1e6",
                ".MEAS AC vp_at_dc FIND VP(OUT) AT=1",
                ".MEAS AC vp_at_fc FIND VP(OUT) AT=1e3",
                ".MEAS AC vp_at_high FIND VP(OUT) AT=100e3",
                ".END");

            RunSimulations(model);

            // At DC: phase ≈ 0
            AssertMeasurementSuccess(model, "vp_at_dc");
            double vpDc = model.Measurements["vp_at_dc"][0].Value;
            Assert.True(Math.Abs(vpDc) < 0.01,
                $"VP at DC should be ~0 radians, got {vpDc}");

            // At cutoff: phase ≈ -pi/4 (-0.785 rad)
            AssertMeasurementSuccess(model, "vp_at_fc");
            double vpFc = model.Measurements["vp_at_fc"][0].Value;
            Assert.True(Math.Abs(vpFc - (-Math.PI / 4)) < 0.1,
                $"VP at cutoff should be ~-0.785 rad, got {vpFc}");

            // At high freq: phase ≈ -pi/2 (-1.571 rad)
            // If VP returned magnitude instead, it would be a small positive number (~0.01)
            AssertMeasurementSuccess(model, "vp_at_high");
            double vpHigh = model.Measurements["vp_at_high"][0].Value;
            Assert.True(vpHigh < -1.0,
                $"VP at high freq should be near -pi/2 (~-1.57), got {vpHigh}. " +
                "If positive, VP is returning magnitude instead of phase.");
        }

        /// <summary>
        /// VR should return the real part of complex voltage.
        /// At DC, VR ≈ 1 (full voltage, no imaginary component).
        /// At cutoff, VR ≈ 0.5 (real part of 1/(1+j) = 0.5 - 0.5j).
        /// </summary>
        [Fact]
        public void VR_ReturnsRealPart_NotMagnitude()
        {
            var model = GetSpiceSharpModel(
                "VR test - RC low-pass",
                "V1 IN 0 AC 1",
                "R1 IN OUT 1e3",
                "C1 OUT 0 159e-9",
                ".AC DEC 10 1 1e6",
                ".MEAS AC vr_at_dc FIND VR(OUT) AT=1",
                ".MEAS AC vr_at_fc FIND VR(OUT) AT=1e3",
                ".MEAS AC vm_at_fc FIND VM(OUT) AT=1e3",
                ".END");

            RunSimulations(model);

            // At DC: VR ≈ 1
            AssertMeasurementSuccess(model, "vr_at_dc");
            double vrDc = model.Measurements["vr_at_dc"][0].Value;
            Assert.True(Math.Abs(vrDc - 1.0) < 0.01,
                $"VR at DC should be ~1.0, got {vrDc}");

            // At cutoff: VR ≈ 0.5, VM ≈ 0.707
            // VR != VM proves we're getting the real part, not magnitude
            AssertMeasurementSuccess(model, "vr_at_fc");
            AssertMeasurementSuccess(model, "vm_at_fc");
            double vrFc = model.Measurements["vr_at_fc"][0].Value;
            double vmFc = model.Measurements["vm_at_fc"][0].Value;

            Assert.True(Math.Abs(vrFc - 0.5) < 0.05,
                $"VR at cutoff should be ~0.5, got {vrFc}");
            Assert.True(Math.Abs(vmFc - 0.707) < 0.05,
                $"VM at cutoff should be ~0.707, got {vmFc}");
            Assert.True(Math.Abs(vrFc - vmFc) > 0.1,
                $"VR ({vrFc}) should differ from VM ({vmFc}) at cutoff");
        }

        /// <summary>
        /// Cross-check: VR² + VI² should equal VM² (Pythagorean identity).
        /// This validates that VR and VI are the true real/imaginary components.
        /// </summary>
        [Fact]
        public void VR_And_VI_Satisfy_PythagoreanIdentity()
        {
            var model = GetSpiceSharpModel(
                "VR/VI/VM identity test",
                "V1 IN 0 AC 1",
                "R1 IN OUT 1e3",
                "C1 OUT 0 159e-9",
                ".AC DEC 10 1 1e6",
                ".MEAS AC vr_fc FIND VR(OUT) AT=1e3",
                ".MEAS AC vi_fc FIND VI(OUT) AT=1e3",
                ".MEAS AC vm_fc FIND VM(OUT) AT=1e3",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "vr_fc");
            AssertMeasurementSuccess(model, "vi_fc");
            AssertMeasurementSuccess(model, "vm_fc");

            double vr = model.Measurements["vr_fc"][0].Value;
            double vi = model.Measurements["vi_fc"][0].Value;
            double vm = model.Measurements["vm_fc"][0].Value;

            double computedMag = Math.Sqrt(vr * vr + vi * vi);
            Assert.True(Math.Abs(computedMag - vm) < 1e-6,
                $"sqrt(VR²+VI²) = {computedMag} should equal VM = {vm}");
        }

        /// <summary>
        /// VDB should equal 20*log10(VM) — cross-check between two export types.
        /// </summary>
        [Fact]
        public void VDB_Equals_20Log10_VM()
        {
            var model = GetSpiceSharpModel(
                "VDB vs VM cross-check",
                "V1 IN 0 AC 1",
                "R1 IN OUT 1e3",
                "C1 OUT 0 159e-9",
                ".AC DEC 10 1 1e6",
                ".MEAS AC vdb_val FIND VDB(OUT) AT=10e3",
                ".MEAS AC vm_val FIND VM(OUT) AT=10e3",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "vdb_val");
            AssertMeasurementSuccess(model, "vm_val");

            double vdb = model.Measurements["vdb_val"][0].Value;
            double vm = model.Measurements["vm_val"][0].Value;
            double expected = 20.0 * Math.Log10(vm);

            Assert.True(Math.Abs(vdb - expected) < 1e-6,
                $"VDB ({vdb}) should equal 20*log10(VM) ({expected})");
        }
    }
}
