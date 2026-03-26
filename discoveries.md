# Key Discoveries

Lessons learned during circuit design that apply across projects. Read this before starting any new design.

---

## 1. `.TRAN` tmax is required for large-cap diode circuits

**Problem:** Simulation produces wildly wrong output voltages (e.g., 0.8V instead of 22V) and completes suspiciously fast.

**Root Cause:** SpiceSharp's adaptive timestep takes ~18ms steps with large capacitors (millifarad range), completely skipping over the sub-millisecond diode conduction windows. The diodes never conduct, so the capacitors never charge.

**Solution:** Always specify the `tmax` parameter (4th argument) in `.TRAN`:
```spice
.TRAN 100u 2 0 100u UIC
*      ^step ^stop ^start ^tmax
```
Rule of thumb: tmax <= 1/10 of the AC period (for 50Hz: tmax <= 2ms; 100us works well).

---

## 2. Prefer `.MEAS` over `.SAVE` for automated spec verification

**Problem:** Using `.SAVE` to collect raw waveform data requires complex post-processing in test code to extract specs (e.g., finding threshold crossings, computing averages over windows).

**Root Cause:** `.SAVE` gives raw data points; all analysis logic must be written in C#. This is error-prone and verbose.

**Solution:** Use `.MEAS` directives in the netlist to extract specs directly:
```spice
.MEAS TRAN v_out_avg AVG V(out) FROM=1.5 TO=2.0
.MEAS TRAN ripple_pp PARAM='v_out_max - v_out_min'
.MEAS TRAN settle_95 WHEN V(out) = 21.47 RISE=1
```
Then in tests: `var meas = CircuitTestHelper.GetMeasurements(netlist);` and assert on named values.
