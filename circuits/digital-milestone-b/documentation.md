# Digital Library Milestone B Example Guide

The example in
[`milestone-b-clocked-state.cir`](milestone-b-clocked-state.cir) drives every
Milestone B block from one 20 ns clock. It uses a 5 V local supply, a 2.5 V
logic threshold, 1 ns example delays, and resistive output loads.

`DIG_D_LATCH` sees data high while `latch_enable` is briefly high. When the
enable closes, the data later falls but `latch_q` stays high. This demonstrates
transparent acquisition followed by hold.

`DIG_DFF` sees the same data. The first positive edge captures high; after the
data falls, the second edge captures low. `DIG_TFF` has T tied high, so its Q
output alternates at successive positive edges.

`DIG_REG4` has D3..D0 wired as binary 1010. Its active-high OE is tied to VDD,
so the captured word appears on `reg_q3..reg_q0`. Pull OE low in a derived
experiment to release all four outputs; external bias then determines their
voltages through the finite `ROFF` leakage.

`DIG_COUNTER4_UP` has enable tied high. It advances through 1, 2, 3, and 4 on
the example's four positive edges. `carry` becomes high only when the enabled
stored count is 15 and returns low after rollover.

The `.SAVE` and `.PLOT` lines select the same state signals for waveform
inspection. `.MEAS` records exact golden samples, while `.PRINT` emits the
signals used to understand those measurements. Keep transient `tmax` below
the modeled delay and output edge time when adapting this example.

Run it with the repository CLI or compile it with `SpiceCompiler.CompileFile`.
The relative `.include` deliberately exercises the shipped source library.
