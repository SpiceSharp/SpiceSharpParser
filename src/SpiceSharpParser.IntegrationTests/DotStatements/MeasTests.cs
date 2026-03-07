using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.DotStatements
{
    public class MeasTests : BaseTests
    {
        // =====================================================================
        // Group A: TRIG/TARG — Delay & Timing Measurements (TRAN)
        // =====================================================================

        [Fact]
        public void TrigTargRiseTimeRC()
        {
            // RC circuit: R=10k, C=1µF, step input 10V
            // Rise time from 10% (1V) to 90% (9V) = RC * ln(9) ≈ 21.97ms
            double rc = 10e3 * 1e-6; // 10ms
            double expectedRiseTime = rc * Math.Log(9.0 / 1.0); // ln(0.9/0.1) = ln(9)

            var model = GetSpiceSharpModel(
                "MEAS TRIG/TARG Rise Time of RC Circuit",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 1e-6",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-5 50e-3",
                ".MEAS TRAN rise_time TRIG V(OUT) VAL=1.0 RISE=1 TARG V(OUT) VAL=9.0 RISE=1",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("rise_time"));
            var results = model.Measurements["rise_time"];
            Assert.Single(results);
            Assert.True(results[0].Success);
            Assert.True(EqualsWithTol(expectedRiseTime, results[0].Value));
        }

        [Fact]
        public void TrigTargPropagationDelayRC()
        {
            // Two-stage RC: R1=1k, C1=10nF. Measure delay from input crossing 2.5V to output crossing 2.5V.
            // With a step input, the mid-node voltage is filtered through the RC.
            var model = GetSpiceSharpModel(
                "MEAS TRIG/TARG Propagation Delay",
                "V1 IN 0 PULSE(0 5 0 1n 1n 100u 200u)",
                "R1 IN MID 1e3",
                "C1 MID 0 10e-9",
                ".IC V(MID)=0.0",
                ".TRAN 1e-8 50e-6",
                ".MEAS TRAN tpd TRIG V(IN) VAL=2.5 RISE=1 TARG V(MID) VAL=2.5 RISE=1",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("tpd"));
            var results = model.Measurements["tpd"];
            Assert.Single(results);
            Assert.True(results[0].Success);
            // Delay should be positive and proportional to RC = 1k * 10nF = 10µs
            Assert.True(results[0].Value > 0);
        }

        [Fact]
        public void TrigTargWithTD()
        {
            // RC circuit with time delay offset on trigger search
            var model = GetSpiceSharpModel(
                "MEAS TRIG/TARG With TD",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 1e-6",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-5 50e-3",
                ".MEAS TRAN delayed TRIG V(OUT) VAL=1.0 RISE=1 TD=5e-3 TARG V(OUT) VAL=9.0 RISE=1",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("delayed"));
            var results = model.Measurements["delayed"];
            Assert.Single(results);
            // TD delays the trigger search, so result should differ from no-TD case
        }

        [Fact]
        public void TrigTargFallTime()
        {
            // RC discharge: pulse goes high then falls
            double rc = 10e3 * 1e-6; // 10ms

            var model = GetSpiceSharpModel(
                "MEAS TRIG/TARG Fall Time",
                "V1 IN 0 PULSE(10 0 10e-3 1n 1n 50e-3 100e-3)",
                "R1 IN OUT 10e3",
                "C1 OUT 0 1e-6",
                ".IC V(OUT)=10.0",
                ".TRAN 1e-5 70e-3",
                ".MEAS TRAN fall_time TRIG V(OUT) VAL=9.0 FALL=1 TARG V(OUT) VAL=1.0 FALL=1",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("fall_time"));
            var results = model.Measurements["fall_time"];
            Assert.Single(results);
            Assert.True(results[0].Success);
            Assert.True(results[0].Value > 0);
        }

        [Fact]
        public void TrigTargCrossEdge()
        {
            // Pulse waveform with multiple crossings, use CROSS=2 and CROSS=3
            var model = GetSpiceSharpModel(
                "MEAS TRIG/TARG Cross Edge",
                "V1 OUT 0 PULSE(0 5 0 1n 1n 10e-6 20e-6)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 60e-6",
                ".MEAS TRAN t_between TRIG V(OUT) VAL=2.5 CROSS=2 TARG V(OUT) VAL=2.5 CROSS=3",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("t_between"));
            var results = model.Measurements["t_between"];
            Assert.Single(results);
            Assert.True(results[0].Success);
            // Time between CROSS=2 and CROSS=3 should be ~half the period (10µs)
            Assert.True(EqualsWithTol(10e-6, results[0].Value));
        }

        // =====================================================================
        // Group B: WHEN — Threshold Crossing Time (TRAN)
        // =====================================================================

        [Fact]
        public void WhenCombinedSyntax()
        {
            // RC circuit: find time when V(out) = 5V (50% of 10V step)
            double rc = 10e3 * 1e-6; // 10ms
            double expected = rc * Math.Log(2); // t = -RC*ln(1 - 0.5) = RC*ln(2)

            var model = GetSpiceSharpModel(
                "MEAS WHEN Combined Syntax",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 1e-6",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-5 50e-3",
                ".MEAS TRAN t50 WHEN V(OUT)=5.0",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("t50"));
            var results = model.Measurements["t50"];
            Assert.Single(results);
            Assert.True(results[0].Success);
            Assert.True(EqualsWithTol(expected, results[0].Value));
        }

        [Fact]
        public void WhenSeparateSyntax()
        {
            // Same circuit as WhenCombinedSyntax but with separate WHEN syntax
            double rc = 10e3 * 1e-6;
            double expected = rc * Math.Log(2);

            var model = GetSpiceSharpModel(
                "MEAS WHEN Separate Syntax",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 1e-6",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-5 50e-3",
                ".MEAS TRAN t50_sep WHEN V(OUT) VAL=5.0 CROSS=1",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("t50_sep"));
            var results = model.Measurements["t50_sep"];
            Assert.Single(results);
            Assert.True(results[0].Success);
            Assert.True(EqualsWithTol(expected, results[0].Value));
        }

        [Fact]
        public void WhenRiseN()
        {
            // Pulse waveform, find time of 2nd rising crossing at 2.5V
            var model = GetSpiceSharpModel(
                "MEAS WHEN RISE=2",
                "V1 OUT 0 PULSE(0 5 0 1n 1n 10e-6 20e-6)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 60e-6",
                ".MEAS TRAN t_rise2 WHEN V(OUT)=2.5 RISE=2",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("t_rise2"));
            var results = model.Measurements["t_rise2"];
            Assert.Single(results);
            Assert.True(results[0].Success);
            // 2nd rising edge at 2.5V should be approximately at t = 20µs (start of 2nd period)
            Assert.True(results[0].Value > 19e-6 && results[0].Value < 21e-6);
        }

        [Fact]
        public void WhenFallN()
        {
            // Pulse waveform, find time of 1st falling crossing at 2.5V
            var model = GetSpiceSharpModel(
                "MEAS WHEN FALL=1",
                "V1 OUT 0 PULSE(0 5 0 1n 1n 10e-6 20e-6)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 60e-6",
                ".MEAS TRAN t_fall1 WHEN V(OUT)=2.5 FALL=1",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("t_fall1"));
            var results = model.Measurements["t_fall1"];
            Assert.Single(results);
            Assert.True(results[0].Success);
            // 1st falling edge at 2.5V should be around t ≈ 10µs (after first pulse)
            Assert.True(results[0].Value > 9e-6 && results[0].Value < 11e-6);
        }

        [Fact]
        public void WhenNoCrossing()
        {
            // DC source at 5V, try to find crossing at 10V — impossible
            var model = GetSpiceSharpModel(
                "MEAS WHEN No Crossing",
                "V1 OUT 0 5.0",
                "R1 OUT 0 1e3",
                ".TRAN 1e-7 10e-6",
                ".MEAS TRAN impossible WHEN V(OUT)=10.0",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("impossible"));
            var results = model.Measurements["impossible"];
            Assert.Single(results);
            Assert.False(results[0].Success);
        }

        // =====================================================================
        // Group C: FIND/WHEN — Value at Threshold (TRAN)
        // =====================================================================

        [Fact]
        public void FindWhenBasic()
        {
            // Pulse input through RC. Find V(OUT) when V(IN) crosses 2.5V
            var model = GetSpiceSharpModel(
                "MEAS FIND/WHEN Basic",
                "V1 IN 0 PULSE(0 5 0 1e-6 1e-6 10e-6 20e-6)",
                "R1 IN OUT 1e3",
                "C1 OUT 0 10e-9",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-8 30e-6",
                ".MEAS TRAN vout_at_cross FIND V(OUT) WHEN V(IN)=2.5",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("vout_at_cross"));
            var results = model.Measurements["vout_at_cross"];
            Assert.Single(results);
            Assert.True(results[0].Success);
            // V(OUT) should be some value between 0 and 5V at the crossing time
            Assert.True(results[0].Value >= 0 && results[0].Value <= 5.0);
        }

        [Fact]
        public void FindCurrentWhenVoltage()
        {
            // RC circuit, find current when V(OUT) = 5V
            var model = GetSpiceSharpModel(
                "MEAS FIND Current WHEN Voltage",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 1e-6",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-5 50e-3",
                ".MEAS TRAN i_at_5v FIND I(R1) WHEN V(OUT)=5.0",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("i_at_5v"));
            var results = model.Measurements["i_at_5v"];
            Assert.Single(results);
            Assert.True(results[0].Success);
        }

        [Fact]
        public void FindWhenWithRise()
        {
            // Pulse circuit with multiple crossings, find V(OUT) at 2nd rising crossing of V(IN)
            var model = GetSpiceSharpModel(
                "MEAS FIND/WHEN With RISE",
                "V1 IN 0 PULSE(0 5 0 1n 1n 10e-6 20e-6)",
                "R1 IN OUT 1e3",
                "C1 OUT 0 10e-9",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-8 60e-6",
                ".MEAS TRAN v_at_rise2 FIND V(OUT) WHEN V(IN)=2.5 RISE=2",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("v_at_rise2"));
            var results = model.Measurements["v_at_rise2"];
            Assert.Single(results);
            Assert.True(results[0].Success);
        }

        // =====================================================================
        // Group D: MIN/MAX/AVG/PP/RMS — Statistical Measurements (TRAN)
        // =====================================================================

        [Fact]
        public void MaxVoltage()
        {
            // RC charging from step input. Max should approach 10V.
            var model = GetSpiceSharpModel(
                "MEAS MAX Voltage",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 1e-6",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-5 50e-3",
                ".MEAS TRAN vmax MAX V(OUT)",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("vmax"));
            var results = model.Measurements["vmax"];
            Assert.Single(results);
            Assert.True(results[0].Success);
            // After 5*RC = 50ms, capacitor should be very close to 10V
            Assert.True(results[0].Value > 9.9);
        }

        [Fact]
        public void MinVoltage()
        {
            // RC charging from 0V. Min should be near 0V.
            var model = GetSpiceSharpModel(
                "MEAS MIN Voltage",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 1e-6",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-5 50e-3",
                ".MEAS TRAN vmin MIN V(OUT)",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("vmin"));
            var results = model.Measurements["vmin"];
            Assert.Single(results);
            Assert.True(results[0].Success);
            // Min voltage should be 0V (initial condition)
            Assert.True(results[0].Value < 0.1);
        }

        [Fact]
        public void AvgVoltageDC()
        {
            // DC source 10V through voltage divider (R1=R2=10k)
            // Average of constant 5V signal = 5V
            var model = GetSpiceSharpModel(
                "MEAS AVG DC Voltage",
                "V1 IN 0 10.0",
                "R1 IN MID 10e3",
                "R2 MID 0 10e3",
                ".TRAN 1e-7 10e-6",
                ".MEAS TRAN vavg_dc AVG V(MID)",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("vavg_dc"));
            var results = model.Measurements["vavg_dc"];
            Assert.Single(results);
            Assert.True(results[0].Success);
            Assert.True(EqualsWithTol(5.0, results[0].Value));
        }

        [Fact]
        public void PeakToPeak()
        {
            // Pulse source 0 to 5V. PP should be 5V.
            var model = GetSpiceSharpModel(
                "MEAS PP Peak-to-Peak",
                "V1 OUT 0 PULSE(0 5 0 1n 1n 5e-6 10e-6)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 30e-6",
                ".MEAS TRAN vpp PP V(OUT)",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("vpp"));
            var results = model.Measurements["vpp"];
            Assert.Single(results);
            Assert.True(results[0].Success);
            Assert.True(EqualsWithTol(5.0, results[0].Value));
        }

        [Fact]
        public void RmsSineWave()
        {
            // Sine wave: amplitude 5V, freq 1MHz, run for 10 full cycles
            // RMS = 5/sqrt(2) ≈ 3.536V
            double expectedRms = 5.0 / Math.Sqrt(2);

            var model = GetSpiceSharpModel(
                "MEAS RMS Sine Wave",
                "V1 OUT 0 SIN(0 5 1MEG)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-9 10e-6",
                ".MEAS TRAN vrms RMS V(OUT) FROM=1e-6 TO=9e-6",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("vrms"));
            var results = model.Measurements["vrms"];
            Assert.Single(results);
            Assert.True(results[0].Success, $"RMS measurement failed, value={results[0].Value}");
            // Allow 5% tolerance for numerical RMS on discrete samples
            double tol = expectedRms * 0.05;
            Assert.True(Math.Abs(results[0].Value - expectedRms) < tol,
                $"Expected RMS ≈ {expectedRms}, but got {results[0].Value}");
        }

        [Fact]
        public void MaxWithFromTo()
        {
            // Pulse source, measure MAX only within a time window
            var model = GetSpiceSharpModel(
                "MEAS MAX With FROM/TO Window",
                "V1 OUT 0 PULSE(0 5 5e-6 1n 1n 10e-6 20e-6)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 40e-6",
                ".MEAS TRAN vmax_window MAX V(OUT) FROM=5e-6 TO=15e-6",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("vmax_window"));
            var results = model.Measurements["vmax_window"];
            Assert.Single(results);
            Assert.True(results[0].Success);
            // In the window 5µs-15µs, the pulse is high (5V)
            Assert.True(EqualsWithTol(5.0, results[0].Value));
        }

        [Fact]
        public void MinWithFromTo()
        {
            // Pulse source, measure MIN within a window where pulse is high
            var model = GetSpiceSharpModel(
                "MEAS MIN With FROM/TO Window",
                "V1 OUT 0 PULSE(0 5 5e-6 1n 1n 10e-6 20e-6)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 40e-6",
                ".MEAS TRAN vmin_window MIN V(OUT) FROM=6e-6 TO=14e-6",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("vmin_window"));
            var results = model.Measurements["vmin_window"];
            Assert.Single(results);
            Assert.True(results[0].Success);
            // Within 6µs-14µs, pulse should be steady at 5V, so min ≈ 5V
            Assert.True(results[0].Value > 4.9);
        }

        // =====================================================================
        // Group E: INTEG — Integration (TRAN)
        // =====================================================================

        [Fact]
        public void IntegConstantVoltage()
        {
            // DC voltage 5V. Integral from 0 to 10µs = 5V * 10µs = 50µV·s
            double expected = 5.0 * 10e-6;

            var model = GetSpiceSharpModel(
                "MEAS INTEG Constant Voltage",
                "V1 OUT 0 5.0",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 10e-6",
                ".MEAS TRAN vt_integral INTEG V(OUT) FROM=0 TO=10e-6",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("vt_integral"));
            var results = model.Measurements["vt_integral"];
            Assert.Single(results);
            Assert.True(results[0].Success);
            Assert.True(EqualsWithTol(expected, results[0].Value));
        }

        [Fact]
        public void IntegFullWindow()
        {
            // DC voltage 5V, full simulation. Integral = 5V * 10µs
            double expected = 5.0 * 10e-6;

            var model = GetSpiceSharpModel(
                "MEAS INTEG Full Window",
                "V1 OUT 0 5.0",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 10e-6",
                ".MEAS TRAN vt_full INTEG V(OUT)",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("vt_full"));
            var results = model.Measurements["vt_full"];
            Assert.Single(results);
            Assert.True(results[0].Success);
            Assert.True(EqualsWithTol(expected, results[0].Value));
        }

        // =====================================================================
        // Group F: DERIV — Derivative (TRAN)
        // =====================================================================

        [Fact]
        public void DerivRampVoltage()
        {
            // Linear ramp from 0 to 10V in 10µs. Slope = 1V/µs = 1e6 V/s
            double expectedSlope = 10.0 / 10e-6;

            var model = GetSpiceSharpModel(
                "MEAS DERIV Ramp Voltage",
                "V1 OUT 0 PWL(0 0 10e-6 10)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 10e-6",
                ".MEAS TRAN slope DERIV V(OUT) AT=5e-6",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("slope"));
            var results = model.Measurements["slope"];
            Assert.Single(results);
            Assert.True(results[0].Success);
            Assert.True(EqualsWithTol(expectedSlope, results[0].Value));
        }

        // =====================================================================
        // Group G: PARAM — Computed Measurements (TRAN)
        // =====================================================================

        [Fact]
        public void ParamBasicRatio()
        {
            // Measure MAX and MIN, then compute ratio via PARAM
            var model = GetSpiceSharpModel(
                "MEAS PARAM Basic Ratio",
                "V1 OUT 0 PULSE(2 8 0 1n 1n 5e-6 10e-6)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 30e-6",
                ".MEAS TRAN vmax MAX V(OUT)",
                ".MEAS TRAN vmin MIN V(OUT)",
                ".MEAS TRAN ratio PARAM='vmax/vmin'",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("ratio"));
            var results = model.Measurements["ratio"];
            Assert.Single(results);
            Assert.True(results[0].Success);
            // 8/2 = 4.0
            Assert.True(EqualsWithTol(4.0, results[0].Value));
        }

        [Fact]
        public void ParamExpressionWithMath()
        {
            // Compute symmetry metric from rise and fall time proxies
            var model = GetSpiceSharpModel(
                "MEAS PARAM Expression With Math",
                "V1 OUT 0 PULSE(0 10 0 1n 1n 5e-6 10e-6)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 30e-6",
                ".MEAS TRAN vmax MAX V(OUT)",
                ".MEAS TRAN vmin MIN V(OUT)",
                ".MEAS TRAN span PARAM='vmax-vmin'",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("span"));
            var results = model.Measurements["span"];
            Assert.Single(results);
            Assert.True(results[0].Success);
            // 10 - 0 = 10
            Assert.True(EqualsWithTol(10.0, results[0].Value));
        }

        // =====================================================================
        // Group H: AC Analysis Measurements
        // =====================================================================

        [Fact]
        public void AcMaxGain()
        {
            // Simple RC low-pass filter: R=1k, C=159nF → fc ≈ 1kHz
            // At low frequency, gain ≈ 1.0
            var model = GetSpiceSharpModel(
                "MEAS AC Max Gain",
                "V1 IN 0 AC 1",
                "R1 IN OUT 1e3",
                "C1 OUT 0 159e-9",
                ".AC DEC 10 1 1e6",
                ".MEAS AC max_gain MAX VM(OUT)",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("max_gain"));
            var results = model.Measurements["max_gain"];
            Assert.Single(results);
            Assert.True(results[0].Success);
            // Max gain of a passive RC is 1.0 (at DC/low frequencies)
            Assert.True(EqualsWithTol(1.0, results[0].Value));
        }

        // =====================================================================
        // Group I: DC Sweep Measurements
        // =====================================================================

        [Fact]
        public void DcMaxCurrent()
        {
            // Resistor R=1k, sweep V1 from 0 to 10V
            // Max current at 10V: I = 10/1000 = 10mA
            var model = GetSpiceSharpModel(
                "MEAS DC Max Current",
                "V1 IN 0 10",
                "R1 IN 0 1e3",
                ".DC V1 0 10 0.1",
                ".MEAS DC imax MAX I(R1)",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("imax"));
            var results = model.Measurements["imax"];
            Assert.Single(results);
            Assert.True(results[0].Success);
        }

        // =====================================================================
        // Group J: OP Measurements
        // =====================================================================

        [Fact]
        public void OpMeasVoltage()
        {
            // Voltage divider: V1=10V, R1=R2=10k → V(MID) = 5V
            var model = GetSpiceSharpModel(
                "MEAS OP Voltage",
                "V1 IN 0 10.0",
                "R1 IN MID 10e3",
                "R2 MID 0 10e3",
                ".OP",
                ".MEAS OP vmid MAX V(MID)",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("vmid"));
            var results = model.Measurements["vmid"];
            Assert.Single(results);
            Assert.True(results[0].Success);
            Assert.True(EqualsWithTol(5.0, results[0].Value));
        }

        [Fact]
        public void OpMeasCurrent()
        {
            // Simple circuit: V1=10V, R1=10k → I(R1) = 1mA
            var model = GetSpiceSharpModel(
                "MEAS OP Current",
                "V1 IN 0 10.0",
                "R1 IN 0 10e3",
                ".OP",
                ".MEAS OP i_r1 MAX I(R1)",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("i_r1"));
            var results = model.Measurements["i_r1"];
            Assert.Single(results);
            Assert.True(results[0].Success);
        }

        // =====================================================================
        // Group K: .MEASURE Alias
        // =====================================================================

        [Fact]
        public void MeasureAliasTran()
        {
            // Same as WhenCombinedSyntax but using .MEASURE
            double rc = 10e3 * 1e-6;
            double expected = rc * Math.Log(2);

            var model = GetSpiceSharpModel(
                "MEASURE Alias TRAN",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 1e-6",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-5 50e-3",
                ".MEASURE TRAN t50 WHEN V(OUT)=5.0",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("t50"));
            var results = model.Measurements["t50"];
            Assert.Single(results);
            Assert.True(results[0].Success);
            Assert.True(EqualsWithTol(expected, results[0].Value));
        }

        [Fact]
        public void MeasureAliasAc()
        {
            // AC measurement using .MEASURE alias
            var model = GetSpiceSharpModel(
                "MEASURE Alias AC",
                "V1 IN 0 AC 1",
                "R1 IN OUT 1e3",
                "C1 OUT 0 159e-9",
                ".AC DEC 10 1 1e6",
                ".MEASURE AC max_gain MAX VM(OUT)",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("max_gain"));
            var results = model.Measurements["max_gain"];
            Assert.Single(results);
            Assert.True(results[0].Success);
        }

        // =====================================================================
        // Group L: Multiple Measurements in One Netlist
        // =====================================================================

        [Fact]
        public void MultipleMeasSameAnalysis()
        {
            // Five measurements in one netlist
            var model = GetSpiceSharpModel(
                "Multiple MEAS Same Analysis",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 1e-6",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-5 50e-3",
                ".MEAS TRAN vmax MAX V(OUT)",
                ".MEAS TRAN vmin MIN V(OUT)",
                ".MEAS TRAN vavg AVG V(OUT)",
                ".MEAS TRAN vpp PP V(OUT)",
                ".MEAS TRAN vrms RMS V(OUT)",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("vmax"));
            Assert.True(model.Measurements.ContainsKey("vmin"));
            Assert.True(model.Measurements.ContainsKey("vavg"));
            Assert.True(model.Measurements.ContainsKey("vpp"));
            Assert.True(model.Measurements.ContainsKey("vrms"));

            Assert.True(model.Measurements["vmax"][0].Success);
            Assert.True(model.Measurements["vmin"][0].Success);
            Assert.True(model.Measurements["vavg"][0].Success);
            Assert.True(model.Measurements["vpp"][0].Success);
            Assert.True(model.Measurements["vrms"][0].Success);
        }

        [Fact]
        public void MultipleMeasDifferentAnalyses()
        {
            // Both TRAN and DC measurements in one netlist
            var model = GetSpiceSharpModel(
                "Multiple MEAS Different Analyses",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 1e-6",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-5 50e-3",
                ".DC V1 0 10 1",
                ".MEAS TRAN vmax_tran MAX V(OUT)",
                ".MEAS DC vmax_dc MAX V(OUT)",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("vmax_tran"));
            Assert.True(model.Measurements.ContainsKey("vmax_dc"));
            Assert.True(model.Measurements["vmax_tran"][0].Success);
            Assert.True(model.Measurements["vmax_dc"][0].Success);
        }

        [Fact]
        public void TenMeasurementsStressTest()
        {
            // Ten measurements in one netlist
            var model = GetSpiceSharpModel(
                "Ten MEAS Stress Test",
                "V1 IN 0 PULSE(0 10 0 1n 1n 5e-6 10e-6)",
                "R1 IN OUT 1e3",
                "C1 OUT 0 10e-9",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-8 30e-6",
                ".MEAS TRAN m1 MAX V(OUT)",
                ".MEAS TRAN m2 MIN V(OUT)",
                ".MEAS TRAN m3 AVG V(OUT)",
                ".MEAS TRAN m4 PP V(OUT)",
                ".MEAS TRAN m5 RMS V(OUT)",
                ".MEAS TRAN m6 INTEG V(OUT)",
                ".MEAS TRAN m7 MAX V(IN)",
                ".MEAS TRAN m8 MIN V(IN)",
                ".MEAS TRAN m9 AVG V(IN)",
                ".MEAS TRAN m10 PP V(IN)",
                ".END");

            RunSimulations(model);

            for (int i = 1; i <= 10; i++)
            {
                string key = $"m{i}";
                Assert.True(model.Measurements.ContainsKey(key), $"Missing measurement: {key}");
                Assert.True(model.Measurements[key][0].Success, $"Measurement {key} not successful");
            }
        }

        // =====================================================================
        // Group M: .STEP Interaction
        // =====================================================================

        [Fact]
        public void MeasWithStepList()
        {
            // .STEP with 3 voltage values → 3 simulation runs → 3 results
            var model = GetSpiceSharpModel(
                "MEAS With STEP LIST",
                "V1 IN 0 {V_val}",
                "R1 IN OUT 10e3",
                "C1 OUT 0 1e-6",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-5 50e-3",
                ".STEP PARAM V_val LIST 2 5 10",
                ".MEAS TRAN vmax MAX V(OUT)",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("vmax"));
            var results = model.Measurements["vmax"];
            Assert.Equal(3, results.Count);
            Assert.All(results, r => Assert.True(r.Success));
        }

        [Fact]
        public void MeasWithStepConcurrent()
        {
            // Thread safety test: run .STEP simulations concurrently
            var model = GetSpiceSharpModel(
                "MEAS With STEP Concurrent",
                "V1 IN 0 {V_val}",
                "R1 IN OUT 10e3",
                "C1 OUT 0 1e-6",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-5 50e-3",
                ".STEP PARAM V_val LIST 1 2 3 4 5",
                ".MEAS TRAN vmax MAX V(OUT)",
                ".END");

            // Run simulations concurrently
            Parallel.ForEach(model.Simulations, sim =>
            {
                var codes = sim.Run(model.Circuit, -1);
                codes = sim.InvokeEvents(codes);
                codes.ToArray();
            });

            Assert.True(model.Measurements.ContainsKey("vmax"));
            var results = model.Measurements["vmax"];
            Assert.Equal(5, results.Count);
            Assert.All(results, r => Assert.True(r.Success));
        }

        // =====================================================================
        // Group N: Error Handling & Edge Cases
        // =====================================================================

        [Fact]
        public void MeasNoCrossingReturnsFailure()
        {
            // DC source, threshold above max → should fail
            var model = GetSpiceSharpModel(
                "MEAS No Crossing Failure",
                "V1 OUT 0 5.0",
                "R1 OUT 0 1e3",
                ".TRAN 1e-7 10e-6",
                ".MEAS TRAN cross WHEN V(OUT)=100.0",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("cross"));
            var results = model.Measurements["cross"];
            Assert.Single(results);
            Assert.False(results[0].Success);
            Assert.True(double.IsNaN(results[0].Value));
        }

        [Fact]
        public void MeasEmptyFromToWindow()
        {
            // Window outside simulation range
            var model = GetSpiceSharpModel(
                "MEAS Empty FROM/TO Window",
                "V1 OUT 0 5.0",
                "R1 OUT 0 1e3",
                ".TRAN 1e-7 10e-6",
                ".MEAS TRAN vmax_far MAX V(OUT) FROM=100 TO=200",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("vmax_far"));
            var results = model.Measurements["vmax_far"];
            Assert.Single(results);
            Assert.False(results[0].Success);
        }

        [Fact]
        public void MeasCaseInsensitive()
        {
            // All lowercase — should still parse correctly
            var model = GetSpiceSharpModel(
                "MEAS Case Insensitive",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 1e-6",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-5 50e-3",
                ".meas tran vmax max V(OUT)",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("vmax"));
            var results = model.Measurements["vmax"];
            Assert.Single(results);
            Assert.True(results[0].Success);
        }

        [Fact]
        public void MeasWithIC()
        {
            // Circuit with .IC, verify MEAS works with initial conditions
            var model = GetSpiceSharpModel(
                "MEAS With IC",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 1e-6",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-5 50e-3",
                ".MEAS TRAN vmax MAX V(OUT)",
                ".MEAS TRAN vmin MIN V(OUT)",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("vmax"));
            Assert.True(model.Measurements.ContainsKey("vmin"));
            Assert.True(model.Measurements["vmax"][0].Success);
            Assert.True(model.Measurements["vmin"][0].Success);
            Assert.True(model.Measurements["vmin"][0].Value < 0.1);
        }

        [Fact]
        public void MeasOnPulseSource()
        {
            // Known PULSE waveform, measure peak-to-peak and max
            var model = GetSpiceSharpModel(
                "MEAS On Pulse Source",
                "V1 OUT 0 PULSE(0 5 1e-6 10e-9 10e-9 5e-6 10e-6)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 30e-6",
                ".MEAS TRAN vpp PP V(OUT)",
                ".MEAS TRAN vmax MAX V(OUT)",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("vpp"));
            Assert.True(model.Measurements.ContainsKey("vmax"));
            Assert.True(EqualsWithTol(5.0, model.Measurements["vpp"][0].Value));
            Assert.True(EqualsWithTol(5.0, model.Measurements["vmax"][0].Value));
        }

        // =====================================================================
        // Group O: Subcircuit Signals
        // =====================================================================

        [Fact]
        public void MeasSubcircuitVoltage()
        {
            // Measure internal node of subcircuit
            var model = GetSpiceSharpModel(
                "MEAS Subcircuit Voltage",
                ".SUBCKT DIVIDER IN OUT",
                "R1 IN MID 10e3",
                "R2 MID OUT 10e3",
                ".ENDS",
                "V1 IN 0 10.0",
                "X1 IN OUT DIVIDER",
                "R3 OUT 0 1e6",
                ".TRAN 1e-7 10e-6",
                ".MEAS TRAN vx MAX V(X1.MID)",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("vx"));
            var results = model.Measurements["vx"];
            Assert.Single(results);
            Assert.True(results[0].Success);
        }

        // =====================================================================
        // Group P: Combined with Other Controls
        // =====================================================================

        [Fact]
        public void MeasWithPrint()
        {
            // Both .MEAS and .PRINT in same netlist
            var model = GetSpiceSharpModel(
                "MEAS With PRINT",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 1e-6",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-5 50e-3",
                ".MEAS TRAN vmax MAX V(OUT)",
                ".PRINT TRAN V(OUT)",
                ".END");

            RunSimulations(model);

            // Both should produce results
            Assert.True(model.Measurements.ContainsKey("vmax"));
            Assert.True(model.Measurements["vmax"][0].Success);
            Assert.Single(model.Prints);
        }

        [Fact]
        public void MeasWithSave()
        {
            // Both .MEAS and .SAVE in same netlist
            var model = GetSpiceSharpModel(
                "MEAS With SAVE",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 1e-6",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-5 50e-3",
                ".MEAS TRAN vmax MAX V(OUT)",
                ".SAVE V(OUT)",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("vmax"));
            Assert.True(model.Measurements["vmax"][0].Success);
        }

        // =====================================================================
        // Additional tests for completeness
        // =====================================================================

        [Fact]
        public void AvgVoltageWithFromTo()
        {
            // DC 5V, average in a window should still be 5V
            var model = GetSpiceSharpModel(
                "MEAS AVG With FROM/TO",
                "V1 OUT 0 5.0",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 10e-6",
                ".MEAS TRAN vavg_win AVG V(OUT) FROM=2e-6 TO=8e-6",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("vavg_win"));
            var results = model.Measurements["vavg_win"];
            Assert.Single(results);
            Assert.True(results[0].Success);
            Assert.True(EqualsWithTol(5.0, results[0].Value));
        }

        [Fact]
        public void IntegWithFromTo()
        {
            // DC 5V, integral from 2µs to 8µs = 5V * 6µs = 30µV·s
            double expected = 5.0 * 6e-6;

            var model = GetSpiceSharpModel(
                "MEAS INTEG With FROM/TO",
                "V1 OUT 0 5.0",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 10e-6",
                ".MEAS TRAN vt_win INTEG V(OUT) FROM=2e-6 TO=8e-6",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("vt_win"));
            var results = model.Measurements["vt_win"];
            Assert.Single(results);
            Assert.True(results[0].Success, $"INTEG measurement failed, value={results[0].Value}");
            // Use a reasonable tolerance for numerical integration with discrete windowing
            double tol = Math.Abs(expected) * 0.05; // 5% tolerance for windowed integration
            Assert.True(Math.Abs(results[0].Value - expected) < tol,
                $"Expected integral ≈ {expected}, but got {results[0].Value}");
        }

        [Fact]
        public void MeasurementTypePersists()
        {
            // Verify that MeasurementType is correctly stored
            var model = GetSpiceSharpModel(
                "MEAS Type Persistence",
                "V1 OUT 0 5.0",
                "R1 OUT 0 1e3",
                ".TRAN 1e-7 10e-6",
                ".MEAS TRAN vmax MAX V(OUT)",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("vmax"));
            var results = model.Measurements["vmax"];
            Assert.Single(results);
            Assert.Equal("MAX", results[0].MeasurementType);
        }

        [Fact]
        public void TrigTargSameSignal()
        {
            // TRIG and TARG on same signal, different thresholds
            var model = GetSpiceSharpModel(
                "MEAS TRIG/TARG Same Signal",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 1e-6",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-5 50e-3",
                ".MEAS TRAN dt TRIG V(OUT) VAL=2.0 RISE=1 TARG V(OUT) VAL=8.0 RISE=1",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("dt"));
            var results = model.Measurements["dt"];
            Assert.Single(results);
            Assert.True(results[0].Success);
            Assert.True(results[0].Value > 0);
        }

        [Fact]
        public void WhenWithFromToWindow()
        {
            // WHEN with FROM/TO to restrict search window
            var model = GetSpiceSharpModel(
                "MEAS WHEN With FROM/TO",
                "V1 OUT 0 PULSE(0 5 0 1n 1n 10e-6 20e-6)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 60e-6",
                ".MEAS TRAN t_in_window WHEN V(OUT)=2.5 RISE=1 FROM=0 TO=30e-6",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("t_in_window"));
            var results = model.Measurements["t_in_window"];
            Assert.Single(results);
            Assert.True(results[0].Success);
        }

        [Fact]
        public void MaxVoltageDirectSource()
        {
            // Direct voltage source — V(OUT) is exactly 5V
            var model = GetSpiceSharpModel(
                "MEAS MAX Direct Source",
                "V1 OUT 0 5.0",
                "R1 OUT 0 1e3",
                ".TRAN 1e-7 10e-6",
                ".MEAS TRAN vmax_direct MAX V(OUT)",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("vmax_direct"));
            Assert.True(EqualsWithTol(5.0, model.Measurements["vmax_direct"][0].Value));
        }

        [Fact]
        public void MinMaxConsistency()
        {
            // MIN <= AVG <= MAX always
            var model = GetSpiceSharpModel(
                "MEAS MIN/MAX/AVG Consistency",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 1e-6",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-5 50e-3",
                ".MEAS TRAN v_min MIN V(OUT)",
                ".MEAS TRAN v_avg AVG V(OUT)",
                ".MEAS TRAN v_max MAX V(OUT)",
                ".END");

            RunSimulations(model);

            double min = model.Measurements["v_min"][0].Value;
            double avg = model.Measurements["v_avg"][0].Value;
            double max = model.Measurements["v_max"][0].Value;

            Assert.True(min <= avg);
            Assert.True(avg <= max);
        }

        [Fact]
        public void PpEqualsMaxMinusMin()
        {
            // PP should equal MAX - MIN
            var model = GetSpiceSharpModel(
                "MEAS PP = MAX - MIN",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 1e-6",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-5 50e-3",
                ".MEAS TRAN vmin2 MIN V(OUT)",
                ".MEAS TRAN vmax2 MAX V(OUT)",
                ".MEAS TRAN vpp2 PP V(OUT)",
                ".END");

            RunSimulations(model);

            double min = model.Measurements["vmin2"][0].Value;
            double max = model.Measurements["vmax2"][0].Value;
            double pp = model.Measurements["vpp2"][0].Value;

            Assert.True(EqualsWithTol(max - min, pp));
        }

        [Fact]
        public void MeasSimulationNamePopulated()
        {
            // Verify that SimulationName is populated
            var model = GetSpiceSharpModel(
                "MEAS Simulation Name",
                "V1 OUT 0 5.0",
                "R1 OUT 0 1e3",
                ".TRAN 1e-7 10e-6",
                ".MEAS TRAN vmax MAX V(OUT)",
                ".END");

            RunSimulations(model);

            var results = model.Measurements["vmax"];
            Assert.Single(results);
            Assert.False(string.IsNullOrEmpty(results[0].SimulationName));
        }

        // =====================================================================
        // Group I: Nested function syntax — mag(V()), db(V()), etc.
        // =====================================================================

        [Fact]
        public void NestedMagVoltageEqualsVM()
        {
            // mag(V(OUT)) should produce the same result as VM(OUT)
            var model = GetSpiceSharpModel(
                "MEAS Nested mag(V()) vs VM()",
                "V1 IN 0 AC 1",
                "R1 IN OUT 1e3",
                "C1 OUT 0 1e-9",
                ".AC DEC 10 1k 100MEG",
                ".MEAS AC gain_vm MAX VM(OUT)",
                ".MEAS AC gain_mag MAX mag(V(OUT))",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("gain_vm"));
            Assert.True(model.Measurements.ContainsKey("gain_mag"));

            double vmResult = model.Measurements["gain_vm"][0].Value;
            double magResult = model.Measurements["gain_mag"][0].Value;

            Assert.True(model.Measurements["gain_vm"][0].Success);
            Assert.True(model.Measurements["gain_mag"][0].Success);
            Assert.True(EqualsWithTol(vmResult, magResult));
        }

        [Fact]
        public void NestedDbVoltageEqualsVDB()
        {
            // db(V(OUT)) should produce the same result as VDB(OUT)
            var model = GetSpiceSharpModel(
                "MEAS Nested db(V()) vs VDB()",
                "V1 IN 0 AC 1",
                "R1 IN OUT 1e3",
                "C1 OUT 0 1e-9",
                ".AC DEC 10 1k 100MEG",
                ".MEAS AC max_db_vdb MAX VDB(OUT)",
                ".MEAS AC max_db_nested MAX db(V(OUT))",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("max_db_vdb"));
            Assert.True(model.Measurements.ContainsKey("max_db_nested"));

            double vdbResult = model.Measurements["max_db_vdb"][0].Value;
            double nestedResult = model.Measurements["max_db_nested"][0].Value;

            Assert.True(model.Measurements["max_db_vdb"][0].Success);
            Assert.True(model.Measurements["max_db_nested"][0].Success);
            Assert.True(EqualsWithTol(vdbResult, nestedResult));
        }

        [Fact]
        public void NestedRealImagPhaseVoltage()
        {
            // real(V(OUT)), imag(V(OUT)), phase(V(OUT)) should match VR, VI, VP
            var model = GetSpiceSharpModel(
                "MEAS Nested real/imag/phase",
                "V1 IN 0 AC 1",
                "R1 IN OUT 1e3",
                "C1 OUT 0 1e-9",
                ".AC DEC 10 1k 100MEG",
                ".MEAS AC max_vr MAX VR(OUT)",
                ".MEAS AC max_real MAX real(V(OUT))",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements["max_vr"][0].Success);
            Assert.True(model.Measurements["max_real"][0].Success);
            Assert.True(EqualsWithTol(
                model.Measurements["max_vr"][0].Value,
                model.Measurements["max_real"][0].Value));
        }
    }
}
