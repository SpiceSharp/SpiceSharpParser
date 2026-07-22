# Generic 555 Design Notes

## Architecture

TIMER555 composes three reusable public building blocks from
standard-digital.lib: two DIG_COMP comparators, one reset-dominant DIG_SR_LATCH,
and one DIG_OPEN_DRAIN discharge stage. DIG_BUF supplies a finite-impedance,
finite-edge-rate output. Three RDIV resistors generate nominal one-third and
two-thirds supply references.

The priority logic is explicit:

1. RESET below half the supply span clears the latch.
2. Otherwise, TRIG below the lower divider reference sets the latch.
3. Otherwise, THRESH above CTRL clears the latch.
4. Otherwise, the latch retains its state.

This matches the control priority needed by ordinary 555 astable and monostable
circuits. The public pin order is GND, TRIG, OUT, RESET, CTRL, THRESH, DISCH,
VCC, matching package pins 1 through 8.

## Numerical Choices

The default 5 kohm divider follows the familiar bipolar 555 architecture.
TPD=100 ns prevents ideal instantaneous feedback. ROUT=20 ohms and COUT=2 nF
shape the functional output to roughly an 88 ns 10%-to-90% transition, avoiding
undersampled ideal-source ringing. RDIS=10 ohms models an enabled low-side
discharge path; ROFF=1 teraohm models the released path.

The latch stores state on CMEM with an explicit RHOLD DC path. Its default
RHOLD=1 teraohm and CMEM=1 pF produce a one-second retention time constant,
which is intentionally much longer than the included millisecond-scale test.

## Using the Model

Use DigitalSubcircuitLibrary.AddTimer555 for programmatic SpiceSharp circuits.
Use TIMER555 directly from the shipped standard-digital.lib when writing a
netlist. The astable example in this directory demonstrates the latter and
contains .MEAS directives for period, high time, and low time.

For reliable switching transients, keep tmax below the modeled delay and edge
time. Gear integration is useful for this feedback topology. With UIC, initialize
the control bypass near two-thirds VCC if startup behavior itself is not under
test.

## Model Boundary

This is a functional macro-model, not a transistor-level representation of a
specific vendor part. It is intended for logic, timing, and topology studies.
Use a vendor model when supply current, output saturation versus load, input
bias, temperature, noise, or data-sheet min/max tolerances matter.

See the main digital-subcircuits documentation article for public API examples,
parameter tables, and data-sheet references.
