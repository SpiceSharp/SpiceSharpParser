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

---

## 3. Reusable digital gates work well as delayed behavioral subcircuits

**Problem:** Programmatic SpiceSharp circuits need reusable digital logic while
remaining compatible with netlists and optional parser custom components.

**Solution:** Put supply-relative behavioral logic in parameterized
`.SUBCKT` definitions, pass its result through `BVDelay`, and model output
loading with series resistance and shunt capacitance. Load the text once with
`SpiceSubcircuitLibrary` and add instances to any target `Circuit`.

The switching threshold is
`VTH * V(VDD,VSS)`. Keep `VTH`, propagation delay, input resistance, output
resistance, and output capacitance overridable per instance. Verify every truth
table row and use a small transient `tmax` when measuring nanosecond delays.

---

## 4. Behavioral state nodes still need an explicit structural DC path

**Problem:** A behavioral current source and capacitor can implement latch state,
but structural lint reports the memory node as floating.

**Root Cause:** The linter cannot infer a DC conductance from an arbitrary
behavioral expression, and a capacitor is open at DC.

**Solution:** Add an explicit very-high-value hold resistor from the state node
to the local reference. Choose `RHOLD * CMEM` much longer than the simulated
interval, document the retention time, and keep acquisition dynamics on a
separate `RSTATE` parameter.

---

## 5. Ideal behavioral transitions need deliberate edge shaping

**Problem:** A fast behavioral output observed with a timestep larger than its
RC edge can ring outside the supply rails under trapezoidal integration.

**Root Cause:** The solver undersamples a nearly discontinuous source driving a
tiny capacitance.

**Solution:** Give functional digital outputs finite resistance and capacitance,
use Gear for switching-heavy transients when appropriate, and set transient
`tmax` well below both propagation delay and output edge time. The functional
555 uses `ROUT=20` and `COUT=2n` by default (about an 88 ns 10%-to-90% edge).
