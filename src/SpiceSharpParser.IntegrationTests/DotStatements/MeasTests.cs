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

        // =====================================================================
        // Group R: Threshold Crossing Edge Cases
        // =====================================================================

        [Fact]
        public void CrossCountsBothEdges()
        {
            // Sine-like signal (pulse train) with many crossings at 2.5V.
            // CROSS=3 should count both rising and falling edges.
            var model = GetSpiceSharpModel(
                "MEAS CROSS counts both edges",
                "V1 OUT 0 PULSE(0 5 0 1n 1n 5e-6 10e-6)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 50e-6",
                ".MEAS TRAN cross3 WHEN V(OUT)=2.5 CROSS=3",
                ".MEAS TRAN rise2 WHEN V(OUT)=2.5 RISE=2",
                ".MEAS TRAN fall1 WHEN V(OUT)=2.5 FALL=1",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "cross3");
            AssertMeasurementSuccess(model, "rise2");
            AssertMeasurementSuccess(model, "fall1");

            // CROSS=3 should be the 3rd edge (any direction); RISE=2 is 2nd rising.
            // fall1 < rise2 < cross3 (ordered in time)
            double cross3 = model.Measurements["cross3"][0].Value;
            double rise2 = model.Measurements["rise2"][0].Value;
            double fall1 = model.Measurements["fall1"][0].Value;
            Assert.True(fall1 > 0);
            Assert.True(rise2 > fall1);
            Assert.True(cross3 > 0);
        }

        [Fact]
        public void RiseHighEdgeNumber()
        {
            // PULSE train with many cycles. RISE=5 should find the 5th rising crossing.
            var model = GetSpiceSharpModel(
                "MEAS RISE=5 on PULSE train",
                "V1 OUT 0 PULSE(0 5 0 1n 1n 5e-6 10e-6)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 100e-6",
                ".MEAS TRAN r5 WHEN V(OUT)=2.5 RISE=5",
                ".MEAS TRAN r1 WHEN V(OUT)=2.5 RISE=1",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "r5");
            AssertMeasurementSuccess(model, "r1");

            double r5 = model.Measurements["r5"][0].Value;
            double r1 = model.Measurements["r1"][0].Value;

            // 5th rising edge is 4 periods after 1st: r5 ≈ r1 + 4 * 10µs
            Assert.True(EqualsWithTol(r1 + 4 * 10e-6, r5));
        }

        [Fact]
        public void TrigTargTwoNodeVoltages()
        {
            // TRIG on V(IN), TARG on V(OUT) — RC delay measurement
            var model = GetSpiceSharpModel(
                "MEAS TRIG-TARG two nodes",
                "V1 IN 0 PULSE(0 10 0 1n 1n 10e-6 20e-6)",
                "R1 IN OUT 1e3",
                "C1 OUT 0 10e-9",
                ".IC V(OUT)=0",
                ".TRAN 1e-8 50e-6",
                ".MEAS TRAN delay TRIG V(IN) VAL=5 RISE=1 TARG V(OUT) VAL=5 RISE=1",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "delay");
            Assert.True(model.Measurements["delay"][0].Value > 0);
        }

        [Fact]
        public void TrigTargWithTDOnTarg()
        {
            // TD on TARG side independently
            var model = GetSpiceSharpModel(
                "MEAS TRIG/TARG TD on TARG",
                "V1 OUT 0 PULSE(0 5 0 1n 1n 5e-6 10e-6)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 50e-6",
                ".MEAS TRAN no_td TRIG V(OUT) VAL=2.5 RISE=1 TARG V(OUT) VAL=2.5 FALL=1",
                ".MEAS TRAN with_td TRIG V(OUT) VAL=2.5 RISE=1 TARG V(OUT) VAL=2.5 FALL=1 TD=15e-6",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "no_td");
            AssertMeasurementSuccess(model, "with_td");

            // with_td should have a larger result since TARG search starts later
            Assert.True(model.Measurements["with_td"][0].Value > model.Measurements["no_td"][0].Value);
        }

        [Fact]
        public void TrigTargWithTDOnBoth()
        {
            // Separate TD values for TRIG and TARG
            var model = GetSpiceSharpModel(
                "MEAS TRIG/TARG TD on both",
                "V1 OUT 0 PULSE(0 5 0 1n 1n 5e-6 10e-6)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 50e-6",
                ".MEAS TRAN both_td TRIG V(OUT) VAL=2.5 RISE=1 TD=10e-6 TARG V(OUT) VAL=2.5 FALL=1 TD=10e-6",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "both_td");
        }

        [Fact]
        public void WhenWithCrossOnOscillatingSignal()
        {
            // Find the 4th crossing (any direction) of a pulse waveform
            var model = GetSpiceSharpModel(
                "MEAS WHEN CROSS on oscillating signal",
                "V1 OUT 0 PULSE(0 5 0 1n 1n 5e-6 10e-6)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 80e-6",
                ".MEAS TRAN c4 WHEN V(OUT)=2.5 CROSS=4",
                ".MEAS TRAN c1 WHEN V(OUT)=2.5 CROSS=1",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "c4");
            AssertMeasurementSuccess(model, "c1");

            Assert.True(model.Measurements["c4"][0].Value > model.Measurements["c1"][0].Value);
        }

        [Fact]
        public void FindWhenWithFromToWindow()
        {
            // Restrict FIND/WHEN search to a time window
            var model = GetSpiceSharpModel(
                "MEAS FIND/WHEN with FROM/TO",
                "V1 OUT 0 PULSE(0 5 0 1n 1n 5e-6 10e-6)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 50e-6",
                ".MEAS TRAN full WHEN V(OUT)=2.5 RISE=1",
                ".MEAS TRAN windowed WHEN V(OUT)=2.5 RISE=1 FROM=15e-6 TO=30e-6",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "full");
            AssertMeasurementSuccess(model, "windowed");

            // Windowed result should be later in time than unrestricted
            Assert.True(model.Measurements["windowed"][0].Value > model.Measurements["full"][0].Value);
        }

        [Fact]
        public void NegativeThresholdCrossing()
        {
            // Signal crosses a negative threshold
            var model = GetSpiceSharpModel(
                "MEAS Negative threshold crossing",
                "V1 OUT 0 PULSE(-5 5 0 1e-6 1e-6 10e-6 20e-6)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 50e-6",
                ".MEAS TRAN neg_cross WHEN V(OUT)=-2.5 RISE=1",
                ".MEAS TRAN vmin MIN V(OUT)",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "neg_cross");
            AssertMeasurementSuccess(model, "vmin");

            Assert.True(model.Measurements["neg_cross"][0].Value > 0);
            Assert.True(model.Measurements["vmin"][0].Value < 0);
        }

        // =====================================================================
        // Group S: Numeric Precision & Accuracy
        // =====================================================================

        [Fact]
        public void AvgOfSineOverFullPeriod()
        {
            // Average of a symmetric waveform over full periods should be midpoint.
            // PULSE(0 10 ...) has avg ≈ 5 over full periods (50% duty cycle)
            var model = GetSpiceSharpModel(
                "MEAS AVG symmetric pulse",
                "V1 OUT 0 PULSE(0 10 0 1n 1n 5e-6 10e-6)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 100e-6",
                ".MEAS TRAN vavg AVG V(OUT) FROM=0 TO=100e-6",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "vavg");
            // 50% duty cycle: avg ≈ 5.0
            Assert.True(Math.Abs(model.Measurements["vavg"][0].Value - 5.0) < 0.5);
        }

        [Fact]
        public void RmsOfSquareWave()
        {
            // RMS of square wave ±A should equal A.
            // PULSE(0, 10) → RMS over full periods ≈ peak * sqrt(duty)
            // For 50% duty, 0-to-10: RMS = 10*sqrt(0.5) = ~7.07
            var model = GetSpiceSharpModel(
                "MEAS RMS of square wave",
                "V1 OUT 0 PULSE(0 10 0 1n 1n 5e-6 10e-6)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 100e-6",
                ".MEAS TRAN vrms RMS V(OUT) FROM=0 TO=100e-6",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "vrms");
            double expected = 10.0 * Math.Sqrt(0.5); // ≈ 7.07
            Assert.True(Math.Abs(model.Measurements["vrms"][0].Value - expected) < 0.5);
        }

        [Fact]
        public void IntegConstantCurrentAsCharge()
        {
            // Q = I * t. Constant 10V across 1k = 10mA for 10ms → Q = 0.1 V·s
            var model = GetSpiceSharpModel(
                "MEAS INTEG constant voltage as area",
                "V1 OUT 0 10.0",
                "R1 OUT 0 1e3",
                ".TRAN 1e-5 10e-3",
                ".MEAS TRAN area INTEG V(OUT) FROM=0 TO=10e-3",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "area");
            // Integral of constant 10V over 10ms = 10 * 0.01 = 0.1
            Assert.True(EqualsWithTol(0.1, model.Measurements["area"][0].Value));
        }

        [Fact]
        public void DerivOfLinearRamp()
        {
            // PWL-like ramp via RC with long time constant.
            // For a step input through RC, V(out) = V*(1-exp(-t/RC)).
            // At t=0, dV/dt = V/RC.
            // V=10, R=1k, C=1µF → RC = 1ms → dV/dt at t=0 = 10/1ms = 10000 V/s
            var model = GetSpiceSharpModel(
                "MEAS DERIV of RC charge",
                "V1 IN 0 10.0",
                "R1 IN OUT 1e3",
                "C1 OUT 0 1e-6",
                ".IC V(OUT)=0",
                ".TRAN 1e-7 5e-3",
                ".MEAS TRAN slope DERIV V(OUT) AT=0.5e-3",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "slope");
            // At t=0.5ms, dV/dt = (V/RC)*exp(-t/RC) = 10000 * exp(-0.5) ≈ 6065
            double expected = 10000.0 * Math.Exp(-0.5);
            Assert.True(Math.Abs(model.Measurements["slope"][0].Value - expected) / expected < 0.1);
        }

        [Fact]
        public void PpOnConstantSignal()
        {
            // PP of a constant DC source should be ≈ 0
            var model = GetSpiceSharpModel(
                "MEAS PP constant signal",
                "V1 OUT 0 5.0",
                "R1 OUT 0 1e3",
                ".TRAN 1e-7 10e-6",
                ".MEAS TRAN vpp PP V(OUT)",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "vpp");
            Assert.True(Math.Abs(model.Measurements["vpp"][0].Value) < 0.01);
        }

        [Fact]
        public void MinMaxOnConstantSignal()
        {
            // MIN and MAX of DC 5V should both be ≈ 5V
            var model = GetSpiceSharpModel(
                "MEAS MIN/MAX constant signal",
                "V1 OUT 0 5.0",
                "R1 OUT 0 1e3",
                ".TRAN 1e-7 10e-6",
                ".MEAS TRAN vmax MAX V(OUT)",
                ".MEAS TRAN vmin MIN V(OUT)",
                ".END");

            RunSimulations(model);

            AssertMeasurement(model, "vmax", 5.0);
            AssertMeasurement(model, "vmin", 5.0);
        }

        // =====================================================================
        // Group T: AC Analysis Measurements
        // =====================================================================

        [Fact]
        public void AcMaxMinMagnitude()
        {
            // RC low-pass: MAX and MIN of magnitude over frequency
            var model = GetSpiceSharpModel(
                "MEAS AC MAX/MIN magnitude",
                "V1 IN 0 AC 1",
                "R1 IN OUT 1e3",
                "C1 OUT 0 159e-9",
                ".AC DEC 100 1 1e6",
                ".MEAS AC mag_max MAX VM(OUT)",
                ".MEAS AC mag_min MIN VM(OUT)",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "mag_max");
            AssertMeasurementSuccess(model, "mag_min");
            // At DC, gain ≈ 1; at high freq, gain → 0
            Assert.True(EqualsWithTol(1.0, model.Measurements["mag_max"][0].Value));
            Assert.True(model.Measurements["mag_min"][0].Value < 0.01);
        }

        [Fact]
        public void AcMaxGainWithFromTo()
        {
            // MAX VM(OUT) over a restricted frequency range
            var model = GetSpiceSharpModel(
                "MEAS AC MAX with FROM/TO",
                "V1 IN 0 AC 1",
                "R1 IN OUT 1e3",
                "C1 OUT 0 159e-9",
                ".AC DEC 100 1 1e6",
                ".MEAS AC max_full MAX VM(OUT)",
                ".MEAS AC max_low MAX VM(OUT) FROM=1 TO=100",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "max_full");
            AssertMeasurementSuccess(model, "max_low");

            // At low frequencies, passive RC gain ≈ 1
            Assert.True(EqualsWithTol(1.0, model.Measurements["max_low"][0].Value));
        }

        [Fact]
        public void AcAvgMagnitude()
        {
            // AVG of VM(OUT) over a frequency range
            var model = GetSpiceSharpModel(
                "MEAS AC AVG magnitude",
                "V1 IN 0 AC 1",
                "R1 IN OUT 1e3",
                "C1 OUT 0 159e-9",
                ".AC DEC 10 1 1e6",
                ".MEAS AC avg_mag AVG VM(OUT)",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "avg_mag");
            // Average gain is between 0 and 1 (since filter rolls off)
            double avg = model.Measurements["avg_mag"][0].Value;
            Assert.True(avg > 0 && avg <= 1.0);
        }

        [Fact]
        public void AcMinGainAtHighFrequency()
        {
            // MIN VM(OUT) at high frequencies should be small
            var model = GetSpiceSharpModel(
                "MEAS AC MIN gain",
                "V1 IN 0 AC 1",
                "R1 IN OUT 1e3",
                "C1 OUT 0 159e-9",
                ".AC DEC 10 1 1e6",
                ".MEAS AC min_gain MIN VM(OUT)",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "min_gain");
            Assert.True(model.Measurements["min_gain"][0].Value < 0.01);
        }

        [Fact]
        public void AcPeakToPeakGain()
        {
            // PP of VM(OUT) across frequency range
            var model = GetSpiceSharpModel(
                "MEAS AC PP gain",
                "V1 IN 0 AC 1",
                "R1 IN OUT 1e3",
                "C1 OUT 0 159e-9",
                ".AC DEC 10 1 1e6",
                ".MEAS AC pp_gain PP VM(OUT)",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "pp_gain");
            // Should be close to 1.0 (from ~1 at DC to near 0 at high freq)
            Assert.True(model.Measurements["pp_gain"][0].Value > 0.9);
        }

        [Fact]
        public void AcIntegMagnitude()
        {
            // INTEG of VM(OUT) over frequency
            var model = GetSpiceSharpModel(
                "MEAS AC INTEG magnitude",
                "V1 IN 0 AC 1",
                "R1 IN OUT 1e3",
                "C1 OUT 0 159e-9",
                ".AC DEC 10 1 1e6",
                ".MEAS AC integ_mag INTEG VM(OUT)",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "integ_mag");
            Assert.True(model.Measurements["integ_mag"][0].Value > 0);
        }

        // =====================================================================
        // Group U: DC Sweep Measurements
        // =====================================================================

        [Fact]
        public void DcThresholdCrossing()
        {
            // Find voltage where current crosses a threshold
            // V/R = I → V = I*R. For I=5mA, R=1k → V=5V
            var model = GetSpiceSharpModel(
                "MEAS DC threshold crossing",
                "V1 IN 0 10",
                "R1 IN 0 1e3",
                ".DC V1 0 10 0.1",
                ".MEAS DC vth WHEN I(V1)=-5e-3",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "vth");
            Assert.True(EqualsWithTol(5.0, model.Measurements["vth"][0].Value));
        }

        [Fact]
        public void DcMinMaxWithWindow()
        {
            // MIN/MAX of current over restricted voltage range
            var model = GetSpiceSharpModel(
                "MEAS DC MIN/MAX with window",
                "V1 IN 0 10",
                "R1 IN 0 1e3",
                ".DC V1 0 10 0.1",
                ".MEAS DC imin MIN I(V1) FROM=2 TO=5",
                ".MEAS DC imax MAX I(V1) FROM=2 TO=5",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "imin");
            AssertMeasurementSuccess(model, "imax");

            // I(V1) is negative in SPICE convention; with FROM=2, current is -2mA; at TO=5, current is -5mA
            // MAX is the least negative (closest to 0), MIN is most negative
            double imin = model.Measurements["imin"][0].Value;
            double imax = model.Measurements["imax"][0].Value;
            Assert.True(imax > imin); // -2mA > -5mA
            Assert.True(imax < 0);
        }

        [Fact]
        public void DcSweepNegativeValues()
        {
            // Sweep from -5V to 5V, measure crossing at 0V
            var model = GetSpiceSharpModel(
                "MEAS DC negative sweep",
                "V1 IN 0 10",
                "R1 IN OUT 1e3",
                "R2 OUT 0 1e3",
                ".DC V1 -5 5 0.1",
                ".MEAS DC zero_cross WHEN V(OUT)=0 RISE=1",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "zero_cross");
            Assert.True(EqualsWithTol(0.0, model.Measurements["zero_cross"][0].Value));
        }

        [Fact]
        public void DcAvgCurrent()
        {
            // AVG of current over DC sweep
            var model = GetSpiceSharpModel(
                "MEAS DC AVG current",
                "V1 IN 0 10",
                "R1 IN 0 1e3",
                ".DC V1 0 10 0.1",
                ".MEAS DC iavg AVG I(V1)",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "iavg");
        }

        // =====================================================================
        // Group V: Differential & Complex Signal References
        // =====================================================================

        [Fact]
        public void DifferentialVoltageMax()
        {
            // V(A,B) differential voltage — measure MAX
            var model = GetSpiceSharpModel(
                "MEAS Differential V(A,B)",
                "V1 A 0 10.0",
                "V2 B 0 3.0",
                "R1 A 0 1e3",
                "R2 B 0 1e3",
                ".TRAN 1e-7 10e-6",
                ".MEAS TRAN vdiff MAX V(A,B)",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "vdiff");
            Assert.True(EqualsWithTol(7.0, model.Measurements["vdiff"][0].Value));
        }

        [Fact]
        public void DifferentialVoltageAvg()
        {
            // V(A,B) differential voltage — measure AVG
            var model = GetSpiceSharpModel(
                "MEAS AVG with V(A,B)",
                "V1 A 0 10.0",
                "V2 B 0 3.0",
                "R1 A 0 1e3",
                "R2 B 0 1e3",
                ".TRAN 1e-7 10e-6",
                ".MEAS TRAN vdiff_avg AVG V(A,B)",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "vdiff_avg");
            Assert.True(EqualsWithTol(7.0, model.Measurements["vdiff_avg"][0].Value));
        }

        [Fact]
        public void CurrentThroughVoltageSource()
        {
            // Measure current through voltage source
            var model = GetSpiceSharpModel(
                "MEAS current through V source",
                "V1 IN 0 10.0",
                "R1 IN 0 1e3",
                ".TRAN 1e-7 10e-6",
                ".MEAS TRAN isrc MAX I(V1)",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "isrc");
        }

        // =====================================================================
        // Group W: PARAM & Cross-Measurement References
        // =====================================================================

        [Fact]
        public void ParamThreeMeasurements()
        {
            // PARAM expression referencing 3 other measurements
            var model = GetSpiceSharpModel(
                "MEAS PARAM with 3 refs",
                "V1 OUT 0 PULSE(0 10 0 1n 1n 5e-6 10e-6)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 30e-6",
                ".MEAS TRAN vmax MAX V(OUT)",
                ".MEAS TRAN vmin MIN V(OUT)",
                ".MEAS TRAN vavg AVG V(OUT)",
                ".MEAS TRAN combo PARAM='(vmax+vmin)/2-vavg'",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "combo");
            // (10+0)/2 - 5 = 0 approximately
            double combo = model.Measurements["combo"][0].Value;
            Assert.True(Math.Abs(combo) < 1.0);
        }

        [Fact]
        public void ParamWithMathFunctions()
        {
            // PARAM with sqrt, abs
            var model = GetSpiceSharpModel(
                "MEAS PARAM with math funcs",
                "V1 OUT 0 PULSE(0 10 0 1n 1n 5e-6 10e-6)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 30e-6",
                ".MEAS TRAN vmax MAX V(OUT)",
                ".MEAS TRAN root_max PARAM='sqrt(vmax)'",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "root_max");
            Assert.True(EqualsWithTol(Math.Sqrt(10.0), model.Measurements["root_max"][0].Value));
        }

        [Fact]
        public void ParamChain()
        {
            // One PARAM references another PARAM result
            var model = GetSpiceSharpModel(
                "MEAS PARAM chain",
                "V1 OUT 0 PULSE(0 10 0 1n 1n 5e-6 10e-6)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 30e-6",
                ".MEAS TRAN vmax MAX V(OUT)",
                ".MEAS TRAN doubled PARAM='vmax*2'",
                ".MEAS TRAN quadrupled PARAM='doubled*2'",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "doubled");
            AssertMeasurementSuccess(model, "quadrupled");
            Assert.True(EqualsWithTol(20.0, model.Measurements["doubled"][0].Value));
            Assert.True(EqualsWithTol(40.0, model.Measurements["quadrupled"][0].Value));
        }

        // =====================================================================
        // Group X: Windowing Interactions
        // =====================================================================

        [Fact]
        public void AvgWithFromTo()
        {
            // Average over restricted window vs full sim
            var model = GetSpiceSharpModel(
                "MEAS AVG with FROM/TO",
                "V1 OUT 0 PULSE(0 10 0 1n 1n 5e-6 10e-6)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 50e-6",
                ".MEAS TRAN avg_full AVG V(OUT)",
                ".MEAS TRAN avg_half AVG V(OUT) FROM=0 TO=5e-6",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "avg_full");
            AssertMeasurementSuccess(model, "avg_half");

            // In first 5µs the signal is rising, then high — avg_half differs from avg_full
            Assert.True(model.Measurements["avg_full"][0].Value != model.Measurements["avg_half"][0].Value);
        }

        [Fact]
        public void RmsWithFromTo()
        {
            // RMS over restricted window
            var model = GetSpiceSharpModel(
                "MEAS RMS with FROM/TO",
                "V1 OUT 0 PULSE(0 10 0 1n 1n 5e-6 10e-6)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 50e-6",
                ".MEAS TRAN rms_full RMS V(OUT)",
                ".MEAS TRAN rms_high RMS V(OUT) FROM=1e-6 TO=5e-6",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "rms_full");
            AssertMeasurementSuccess(model, "rms_high");
        }

        [Fact]
        public void PpWithRestrictedWindow()
        {
            // PP over a window shorter than one period — should be less than full PP
            var model = GetSpiceSharpModel(
                "MEAS PP with restricted window",
                "V1 OUT 0 PULSE(0 10 0 1n 1n 5e-6 10e-6)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 50e-6",
                ".MEAS TRAN pp_full PP V(OUT)",
                ".MEAS TRAN pp_high PP V(OUT) FROM=1e-6 TO=4e-6",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "pp_full");
            AssertMeasurementSuccess(model, "pp_high");

            // Restricted window during HIGH phase has less PP
            Assert.True(model.Measurements["pp_high"][0].Value <= model.Measurements["pp_full"][0].Value);
        }

        [Fact]
        public void FromToOutsideSimulationRange()
        {
            // Both FROM and TO beyond simulation time → failure
            var model = GetSpiceSharpModel(
                "MEAS FROM/TO outside range",
                "V1 OUT 0 5.0",
                "R1 OUT 0 1e3",
                ".TRAN 1e-7 10e-6",
                ".MEAS TRAN out_of_range MAX V(OUT) FROM=1.0 TO=2.0",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("out_of_range"));
            Assert.False(model.Measurements["out_of_range"][0].Success);
        }

        // =====================================================================
        // Group Y: Failure & Robustness
        // =====================================================================

        [Fact]
        public void TrigUnreachableTargReachable()
        {
            // Trig threshold unreachable → overall measurement fails
            var model = GetSpiceSharpModel(
                "MEAS TRIG unreachable",
                "V1 OUT 0 5.0",
                "R1 OUT 0 1e3",
                ".TRAN 1e-7 10e-6",
                ".MEAS TRAN fail_trig TRIG V(OUT) VAL=100 RISE=1 TARG V(OUT) VAL=3 RISE=1",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("fail_trig"));
            Assert.False(model.Measurements["fail_trig"][0].Success);
        }

        [Fact]
        public void TargUnreachableTrigReachable()
        {
            // Targ threshold unreachable → overall measurement fails
            var model = GetSpiceSharpModel(
                "MEAS TARG unreachable",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 1e-6",
                ".IC V(OUT)=0",
                ".TRAN 1e-5 50e-3",
                ".MEAS TRAN fail_targ TRIG V(OUT) VAL=1 RISE=1 TARG V(OUT) VAL=100 RISE=1",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("fail_targ"));
            Assert.False(model.Measurements["fail_targ"][0].Success);
        }

        [Fact]
        public void VeryLargeAmplitude()
        {
            // kV-level signals
            var model = GetSpiceSharpModel(
                "MEAS Very large amplitude",
                "V1 OUT 0 1000.0",
                "R1 OUT 0 1e6",
                ".TRAN 1e-7 10e-6",
                ".MEAS TRAN vmax MAX V(OUT)",
                ".MEAS TRAN vmin MIN V(OUT)",
                ".END");

            RunSimulations(model);

            AssertMeasurement(model, "vmax", 1000.0);
        }

        [Fact]
        public void VerySmallAmplitude()
        {
            // µV-level signals
            var model = GetSpiceSharpModel(
                "MEAS Very small amplitude",
                "V1 OUT 0 1e-6",
                "R1 OUT 0 1e3",
                ".TRAN 1e-7 10e-6",
                ".MEAS TRAN vmax MAX V(OUT)",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "vmax");
            Assert.True(Math.Abs(model.Measurements["vmax"][0].Value - 1e-6) < 1e-7);
        }

        [Fact]
        public void WhenNoCrossingReturnsFail()
        {
            // Threshold impossible to reach → Success=false, Value=NaN
            var model = GetSpiceSharpModel(
                "MEAS When no cross fail",
                "V1 OUT 0 5.0",
                "R1 OUT 0 1e3",
                ".TRAN 1e-7 10e-6",
                ".MEAS TRAN no_cross WHEN V(OUT)=999.0",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("no_cross"));
            Assert.False(model.Measurements["no_cross"][0].Success);
            Assert.True(double.IsNaN(model.Measurements["no_cross"][0].Value));
        }

        [Fact]
        public void IntegFullPeriodSymmetric()
        {
            // Integral of symmetric PULSE over full period → area = amplitude * duty * period
            var model = GetSpiceSharpModel(
                "MEAS INTEG full period",
                "V1 OUT 0 PULSE(0 10 0 1n 1n 5e-6 10e-6)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 10e-6",
                ".MEAS TRAN area INTEG V(OUT) FROM=0 TO=10e-6",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "area");
            // 10V * 5µs (high) + 0V * 5µs (low) = 50e-6
            double expected = 10.0 * 5e-6;
            Assert.True(Math.Abs(model.Measurements["area"][0].Value - expected) / expected < 0.05);
        }

        // =====================================================================
        // Group Z: Syntax Variants & Compatibility
        // =====================================================================

        [Fact]
        public void MixedCaseKeywords()
        {
            // Mixed case: .Meas, Tran, Max
            var model = GetSpiceSharpModel(
                "MEAS Mixed case keywords",
                "V1 OUT 0 10.0",
                "R1 OUT 0 1e3",
                ".TRAN 1e-7 10e-6",
                ".Meas Tran vmax Max V(OUT)",
                ".END");

            RunSimulations(model);

            AssertMeasurement(model, "vmax", 10.0);
        }

        [Fact]
        public void MeasureAliasAllTypes()
        {
            // .MEASURE (long form) with multiple measurement types
            var model = GetSpiceSharpModel(
                "MEASURE Alias all types",
                "V1 OUT 0 PULSE(0 10 0 1n 1n 5e-6 10e-6)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 30e-6",
                ".MEASURE TRAN vmax MAX V(OUT)",
                ".MEASURE TRAN vmin MIN V(OUT)",
                ".MEASURE TRAN vpp PP V(OUT)",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "vmax");
            AssertMeasurementSuccess(model, "vmin");
            AssertMeasurementSuccess(model, "vpp");
        }

        [Fact]
        public void MeasurementNameWithUnderscores()
        {
            // Names with underscores and numbers
            var model = GetSpiceSharpModel(
                "MEAS name with underscores",
                "V1 OUT 0 PULSE(0 10 0 1n 1n 5e-6 10e-6)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 30e-6",
                ".MEAS TRAN my_rise_time_1 WHEN V(OUT)=5.0 RISE=1",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "my_rise_time_1");
        }

        [Fact]
        public void MultipleMeasAllTypes()
        {
            // All statistical types in one netlist
            var model = GetSpiceSharpModel(
                "MEAS All statistical types together",
                "V1 OUT 0 PULSE(0 10 0 1n 1n 5e-6 10e-6)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 30e-6",
                ".MEAS TRAN m_max MAX V(OUT)",
                ".MEAS TRAN m_min MIN V(OUT)",
                ".MEAS TRAN m_avg AVG V(OUT)",
                ".MEAS TRAN m_rms RMS V(OUT)",
                ".MEAS TRAN m_pp PP V(OUT)",
                ".MEAS TRAN m_integ INTEG V(OUT)",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "m_max");
            AssertMeasurementSuccess(model, "m_min");
            AssertMeasurementSuccess(model, "m_avg");
            AssertMeasurementSuccess(model, "m_rms");
            AssertMeasurementSuccess(model, "m_pp");
            AssertMeasurementSuccess(model, "m_integ");

            // Sanity relationships
            double vmax = model.Measurements["m_max"][0].Value;
            double vmin = model.Measurements["m_min"][0].Value;
            double vpp = model.Measurements["m_pp"][0].Value;
            double vavg = model.Measurements["m_avg"][0].Value;
            double vrms = model.Measurements["m_rms"][0].Value;

            Assert.True(EqualsWithTol(vmax - vmin, vpp));
            Assert.True(vavg >= vmin && vavg <= vmax);
            Assert.True(vrms >= 0);
            Assert.True(vrms >= Math.Abs(vavg)); // RMS ≥ |AVG| always
        }

        // =====================================================================
        // Group AA: .STEP Interactions with Multiple Types
        // =====================================================================

        [Fact]
        public void StepWithStatisticalMeasurements()
        {
            // .STEP with MIN/MAX/AVG producing per-step results
            var model = GetSpiceSharpModel(
                "MEAS .STEP with stats",
                ".PARAM V_val=1",
                "V1 IN 0 {V_val}",
                "R1 IN OUT 10e3",
                "C1 OUT 0 1e-6",
                ".IC V(OUT)=0",
                ".TRAN 1e-5 50e-3",
                ".STEP PARAM V_val LIST 5 10 15",
                ".MEAS TRAN vmax MAX V(OUT)",
                ".MEAS TRAN vavg AVG V(OUT)",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("vmax"));
            Assert.True(model.Measurements.ContainsKey("vavg"));

            var vmaxResults = model.Measurements["vmax"];
            var vavgResults = model.Measurements["vavg"];

            Assert.Equal(3, vmaxResults.Count);
            Assert.Equal(3, vavgResults.Count);
            Assert.All(vmaxResults, r => Assert.True(r.Success));
            Assert.All(vavgResults, r => Assert.True(r.Success));

            // MAX should increase with each step value
            Assert.True(vmaxResults[0].Value < vmaxResults[1].Value);
            Assert.True(vmaxResults[1].Value < vmaxResults[2].Value);
        }

        [Fact]
        public void StepWithTrigTarg()
        {
            // Timing measurement that varies with component value
            // Use smaller capacitor so signals settle within sim time
            var model = GetSpiceSharpModel(
                "MEAS .STEP with TRIG/TARG",
                ".PARAM R_val=10e3",
                "V1 IN 0 10.0",
                "R1 IN OUT {R_val}",
                "C1 OUT 0 10e-9",
                ".IC V(OUT)=0",
                ".TRAN 1e-6 10e-3",
                ".STEP PARAM R_val LIST 1e3 10e3 100e3",
                ".MEAS TRAN rise_t TRIG V(OUT) VAL=1 RISE=1 TARG V(OUT) VAL=9 RISE=1",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("rise_t"));
            var results = model.Measurements["rise_t"];
            Assert.Equal(3, results.Count);
            Assert.All(results, r => Assert.True(r.Success));

            // Higher R → longer rise time
            Assert.True(results[0].Value < results[1].Value);
            Assert.True(results[1].Value < results[2].Value);
        }

        // =====================================================================
        // Group AB: FIND/WHEN Additional Cases
        // =====================================================================

        [Fact]
        public void FindWhenCurrent()
        {
            // FIND current value WHEN voltage crosses threshold
            var model = GetSpiceSharpModel(
                "MEAS FIND I WHEN V",
                "V1 IN 0 10.0",
                "R1 IN OUT 1e3",
                "C1 OUT 0 1e-6",
                ".IC V(OUT)=0",
                ".TRAN 1e-6 10e-3",
                ".MEAS TRAN i_at_5v FIND I(R1) WHEN V(OUT)=5.0",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "i_at_5v");
            // When V(OUT)=5V, I(R1) = (10-5)/1k = 5mA
            Assert.True(EqualsWithTol(5e-3, model.Measurements["i_at_5v"][0].Value));
        }

        [Fact]
        public void FindWhenWithRise2()
        {
            // FIND with RISE=2
            var model = GetSpiceSharpModel(
                "MEAS FIND/WHEN RISE=2",
                "V1 OUT 0 PULSE(0 5 0 1n 1n 5e-6 10e-6)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-8 50e-6",
                ".MEAS TRAN t_r2 WHEN V(OUT)=2.5 RISE=2",
                ".MEAS TRAN t_r1 WHEN V(OUT)=2.5 RISE=1",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "t_r2");
            AssertMeasurementSuccess(model, "t_r1");

            // 2nd rise is exactly one period later
            double period = 10e-6;
            double diff = model.Measurements["t_r2"][0].Value - model.Measurements["t_r1"][0].Value;
            Assert.True(EqualsWithTol(period, diff));
        }

        // =====================================================================
        // Group AC: OP Additional Measurements
        // =====================================================================

        [Fact]
        public void OpMultipleMeasurements()
        {
            // Multiple measurements on OP analysis
            var model = GetSpiceSharpModel(
                "MEAS OP multiple",
                "V1 IN 0 12.0",
                "R1 IN MID 4e3",
                "R2 MID 0 6e3",
                ".OP",
                ".MEAS OP vmid MAX V(MID)",
                ".MEAS OP vin MAX V(IN)",
                ".END");

            RunSimulations(model);

            // Voltage divider: V(MID) = 12 * 6k/(4k+6k) = 7.2V
            AssertMeasurement(model, "vmid", 7.2);
            AssertMeasurement(model, "vin", 12.0);
        }

        // =====================================================================
        // Group AD: DERIV Edge Cases
        // =====================================================================

        [Fact]
        public void DerivAtEarlyTime()
        {
            // DERIV at very early time (near simulation start)
            var model = GetSpiceSharpModel(
                "MEAS DERIV early time",
                "V1 IN 0 10.0",
                "R1 IN OUT 1e3",
                "C1 OUT 0 1e-6",
                ".IC V(OUT)=0",
                ".TRAN 1e-7 5e-3",
                ".MEAS TRAN slope_early DERIV V(OUT) AT=1e-6",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "slope_early");
            // Near t=0, slope should be close to V/(RC) = 10/(1e3*1e-6) = 10000
            Assert.True(model.Measurements["slope_early"][0].Value > 5000);
        }

        [Fact]
        public void DerivAtLateTime()
        {
            // DERIV at late time (signal nearly settled)
            var model = GetSpiceSharpModel(
                "MEAS DERIV late time",
                "V1 IN 0 10.0",
                "R1 IN OUT 1e3",
                "C1 OUT 0 1e-6",
                ".IC V(OUT)=0",
                ".TRAN 1e-6 10e-3",
                ".MEAS TRAN slope_late DERIV V(OUT) AT=9e-3",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "slope_late");
            // After 9 time constants, slope should be very small
            Assert.True(Math.Abs(model.Measurements["slope_late"][0].Value) < 100);
        }

        // =====================================================================
        // Group AE: Combined Analysis Types in One Netlist
        // =====================================================================

        [Fact]
        public void TranAndDcMeasInSameNetlist()
        {
            // Both TRAN and DC measurements in same netlist
            var model = GetSpiceSharpModel(
                "MEAS TRAN + DC in same netlist",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 1e-6",
                ".IC V(OUT)=0",
                ".TRAN 1e-5 50e-3",
                ".DC V1 0 10 0.5",
                ".MEAS TRAN tran_max MAX V(OUT)",
                ".MEAS DC dc_max MAX V(OUT)",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "tran_max");
            AssertMeasurementSuccess(model, "dc_max");
        }

        // =====================================================================
        // Group AF: INTEG Additional Cases
        // =====================================================================

        [Fact]
        public void IntegWithFromToPrecision()
        {
            // INTEG with tight window — verify two windows give proportional areas
            var model = GetSpiceSharpModel(
                "MEAS INTEG with tight FROM/TO",
                "V1 OUT 0 10.0",
                "R1 OUT 0 1e3",
                ".TRAN 1e-7 100e-6",
                ".MEAS TRAN area_small INTEG V(OUT) FROM=10e-6 TO=20e-6",
                ".MEAS TRAN area_large INTEG V(OUT) FROM=10e-6 TO=60e-6",
                ".END");

            RunSimulations(model);

            AssertMeasurementSuccess(model, "area_small");
            AssertMeasurementSuccess(model, "area_large");

            // Both integrals of constant signal are positive
            Assert.True(model.Measurements["area_small"][0].Value > 0);
            Assert.True(model.Measurements["area_large"][0].Value > model.Measurements["area_small"][0].Value);
        }

        // =====================================================================
        // Group AG: Multiple Measurements Stress Test
        // =====================================================================

        [Fact]
        public void FifteenMeasurementsStressTest()
        {
            // 15 measurements of various types — stress test
            var model = GetSpiceSharpModel(
                "MEAS 15 measurements stress",
                "V1 IN 0 PULSE(0 10 0 1e-6 1e-6 5e-6 12e-6)",
                "R1 IN OUT 1e3",
                "C1 OUT 0 100e-9",
                ".IC V(OUT)=0",
                ".TRAN 1e-8 60e-6",
                ".MEAS TRAN m1 MAX V(OUT)",
                ".MEAS TRAN m2 MIN V(OUT)",
                ".MEAS TRAN m3 AVG V(OUT)",
                ".MEAS TRAN m4 RMS V(OUT)",
                ".MEAS TRAN m5 PP V(OUT)",
                ".MEAS TRAN m6 INTEG V(OUT)",
                ".MEAS TRAN m7 MAX V(IN)",
                ".MEAS TRAN m8 MIN V(IN)",
                ".MEAS TRAN m9 PP V(IN)",
                ".MEAS TRAN m10 AVG V(IN)",
                ".MEAS TRAN m11 WHEN V(IN)=5.0 RISE=1",
                ".MEAS TRAN m12 WHEN V(IN)=5.0 FALL=1",
                ".MEAS TRAN m13 TRIG V(IN) VAL=2 RISE=1 TARG V(IN) VAL=8 RISE=1",
                ".MEAS TRAN m14 PARAM='m1-m2'",
                ".MEAS TRAN m15 PARAM='m3/m1'",
                ".END");

            RunSimulations(model);

            for (int i = 1; i <= 15; i++)
            {
                string name = $"m{i}";
                Assert.True(model.Measurements.ContainsKey(name), $"Missing {name}");
                Assert.True(model.Measurements[name][0].Success, $"{name} failed");
            }

            // PP should equal MAX - MIN
            Assert.True(EqualsWithTol(
                model.Measurements["m1"][0].Value - model.Measurements["m2"][0].Value,
                model.Measurements["m5"][0].Value));

            // PARAM m14 = m1 - m2 should equal PP (m5)
            Assert.True(EqualsWithTol(
                model.Measurements["m5"][0].Value,
                model.Measurements["m14"][0].Value));
        }
        // =====================================================================
        // Group O: FIND ... AT= Measurements
        // =====================================================================

        [Fact]
        public void FindAtConstantVoltage()
        {
            // Constant 5V source — FIND V(out) AT=any_time should return 5.0
            var model = GetSpiceSharpModel(
                "MEAS FIND AT Constant Voltage",
                "V1 OUT 0 5.0",
                "R1 OUT 0 1e3",
                ".TRAN 1e-4 10e-3",
                ".MEAS TRAN res1 FIND V(OUT) AT=5m",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("res1"));
            var results = model.Measurements["res1"];
            Assert.Single(results);
            Assert.True(results[0].Success);
            Assert.True(EqualsWithTol(5.0, results[0].Value));
        }

        [Fact]
        public void FindAtRCCharging()
        {
            // RC circuit: V(out) = 10*(1-exp(-t/RC)), RC=10ms
            // At t=10ms: V(out) = 10*(1-exp(-1)) ≈ 6.3212
            double rc = 10e3 * 1e-6; // 10ms
            double expected = 10.0 * (1.0 - Math.Exp(-10e-3 / rc));

            var model = GetSpiceSharpModel(
                "MEAS FIND AT RC Charging",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 1e-6",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-5 50e-3",
                ".MEAS TRAN res1 FIND V(OUT) AT=10m",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("res1"));
            var results = model.Measurements["res1"];
            Assert.Single(results);
            Assert.True(results[0].Success);
            Assert.True(EqualsWithTol(expected, results[0].Value));
        }

        // =====================================================================
        // Group P: TD on WHEN Measurements
        // =====================================================================

        [Fact]
        public void WhenWithTD()
        {
            // Oscillating signal: crosses 0V many times. With TD, skip early crossings.
            // V(out) = sin(2*pi*1000*t) crosses 0 at t=0, 0.5ms, 1ms, 1.5ms, ...
            // With TD=0.8ms, the first crossing after 0.8ms is at t=1ms
            var model = GetSpiceSharpModel(
                "MEAS WHEN with TD",
                "V1 OUT 0 SIN(0 1 1e3)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-6 5e-3",
                ".MEAS TRAN res_notd WHEN V(OUT)=0 CROSS=1",
                ".MEAS TRAN res_td WHEN V(OUT)=0 CROSS=1 TD=0.8m",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("res_notd"));
            Assert.True(model.Measurements.ContainsKey("res_td"));
            Assert.True(model.Measurements["res_notd"][0].Success);
            Assert.True(model.Measurements["res_td"][0].Success);
            // Without TD, first crossing is near t=0.5ms (first positive-to-negative)
            // With TD=0.8ms, first crossing after 0.8ms should be at ~1ms
            Assert.True(model.Measurements["res_td"][0].Value > 0.8e-3);
        }

        // =====================================================================
        // Group Q: RISE/FALL/CROSS=LAST
        // =====================================================================

        [Fact]
        public void WhenCrossLast()
        {
            // Oscillating signal: find the last crossing of 0V
            var model = GetSpiceSharpModel(
                "MEAS WHEN CROSS LAST",
                "V1 OUT 0 SIN(0 1 1e3)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-6 5e-3",
                ".MEAS TRAN res_last WHEN V(OUT)=0 CROSS=LAST",
                ".MEAS TRAN res_first WHEN V(OUT)=0 CROSS=1",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("res_last"));
            Assert.True(model.Measurements.ContainsKey("res_first"));
            Assert.True(model.Measurements["res_last"][0].Success);
            Assert.True(model.Measurements["res_first"][0].Success);
            // Last crossing should be much later than first
            Assert.True(model.Measurements["res_last"][0].Value > model.Measurements["res_first"][0].Value);
            // Last crossing should be near end of simulation (5ms)
            Assert.True(model.Measurements["res_last"][0].Value > 4e-3);
        }

        [Fact]
        public void FindWhenRiseLast()
        {
            // Find V(out) at the last rising crossing of 0V
            var model = GetSpiceSharpModel(
                "MEAS FIND WHEN RISE LAST",
                "V1 OUT 0 SIN(0 1 1e3)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-6 5e-3",
                ".MEAS TRAN res1 FIND V(OUT) WHEN V(OUT)=0 RISE=LAST",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("res1"));
            Assert.True(model.Measurements["res1"][0].Success);
            // At a rising zero crossing, V(OUT) ≈ 0
            Assert.True(Math.Abs(model.Measurements["res1"][0].Value) < 0.1);
        }

        [Fact]
        public void TrigTargCrossLast()
        {
            // Use CROSS=LAST on TARG for TRIG/TARG measurement
            var model = GetSpiceSharpModel(
                "MEAS TRIG/TARG CROSS LAST",
                "V1 OUT 0 SIN(0 1 1e3)",
                "R1 OUT 0 1e3",
                ".TRAN 1e-6 5e-3",
                ".MEAS TRAN res1 TRIG V(OUT) VAL=0 CROSS=1 TARG V(OUT) VAL=0 CROSS=LAST",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("res1"));
            Assert.True(model.Measurements["res1"][0].Success);
            // Should be a large positive value (last crossing - first crossing)
            Assert.True(model.Measurements["res1"][0].Value > 3e-3);
        }

        // =====================================================================
        // Group R: DERIV with WHEN
        // =====================================================================

        [Fact]
        public void DerivWithWhen()
        {
            // RC circuit: V(out) = 10*(1-exp(-t/RC))
            // dV/dt = (10/RC)*exp(-t/RC)
            // Use DERIV WHEN V(OUT)=5 to find the derivative at the point where V=5V
            // V=5 when t = -RC*ln(0.5) = RC*ln(2) ≈ 6.93ms
            // dV/dt at that point = (10/RC)*exp(-ln(2)) = (10/RC)*0.5 = 500 V/s
            double rc = 10e3 * 1e-6; // 10ms

            var model = GetSpiceSharpModel(
                "MEAS DERIV with WHEN",
                "V1 IN 0 10.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 1e-6",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-5 50e-3",
                ".MEAS TRAN res1 DERIV V(OUT) AT=6.93m",
                ".MEAS TRAN res2 DERIV V(OUT) WHEN V(OUT)=5",
                ".END");

            RunSimulations(model);

            Assert.True(model.Measurements.ContainsKey("res1"));
            Assert.True(model.Measurements.ContainsKey("res2"));
            Assert.True(model.Measurements["res1"][0].Success);
            Assert.True(model.Measurements["res2"][0].Success);
            // Both should give approximately the same derivative (around 500 V/s)
            // Use a 5% tolerance since discrete derivative is approximate
            double expected = (10.0 / rc) * 0.5; // 500 V/s
            Assert.True(Math.Abs(model.Measurements["res1"][0].Value - expected) / expected < 0.05,
                $"res1 derivative {model.Measurements["res1"][0].Value} not within 5% of {expected}");
            Assert.True(Math.Abs(model.Measurements["res2"][0].Value - expected) / expected < 0.05,
                $"res2 derivative {model.Measurements["res2"][0].Value} not within 5% of {expected}");
        }
    }
}
