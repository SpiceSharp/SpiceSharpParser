# Digital Component Library Roadmap

Status: Active — Milestone A implemented on 2026-07-22

## Purpose

Expand `DigitalSubcircuitLibrary` from its current gate and functional-555
foundation into a reusable mixed-signal digital library for programmatic
SpiceSharp circuits, SpiceSharpParser netlists, and circuits using
`SpiceSharpParser.CustomComponents`.

The roadmap favors generic, parameterized primitives first. Familiar 74HC and
CD4000-style devices should then be composed from those validated primitives.
This keeps the implementation reusable and prevents package-specific models
from duplicating state, delay, and output-stage logic.

## Current Baseline

The embedded `standard-digital.lib` currently provides twenty definitions:

| Category | Existing definitions |
| --- | --- |
| Unary gates | `DIG_BUF`, `DIG_NOT` |
| Binary gates | `DIG_AND2`, `DIG_NAND2`, `DIG_OR2`, `DIG_NOR2`, `DIG_XOR2`, `DIG_XNOR2` |
| Input conditioning | `DIG_SCHMITT_BUF`, `DIG_SCHMITT_NOT` |
| Bus drivers | `DIG_TRI_BUF`, `DIG_TRI_NOT` |
| Routing and arithmetic | `DIG_MUX2`, `DIG_MUX4`, `DIG_FULL_ADDER`, `DIG_DEC2TO4` |
| Mixed-signal primitives | `DIG_COMP`, `DIG_OPEN_DRAIN` |
| Stateful primitive | `DIG_SR_LATCH` |
| Integrated functional model | `TIMER555` |

The facade exposes typed gate, Schmitt, and tri-state parameters plus explicit
methods for routing/arithmetic blocks, the comparator, SR latch, open-drain
stage, and 555 timer.

## Goals

- Cover the primitives needed to construct useful combinational, sequential,
  bus-oriented, and mixed-signal digital systems.
- Keep every model usable from both text netlists and programmatic SpiceSharp.
- Model finite input loading, propagation delay, output impedance, and output
  edge rate instead of ideal instantaneous logic.
- Represent high impedance electrically using finite off resistance and
  leakage.
- Give every stateful model deterministic and documented initialization and
  asynchronous-control semantics.
- Verify behavior with truth tables, transient sequences, `.MEAS`, structural
  linting, smoke tests, and complete regression tests.
- Build standard-logic IC wrappers only after their underlying primitives have
  passed independent tests.

## Non-Goals for the First Versions

- Four-state HDL semantics such as X, Z as a logical value, or strength
  resolution. High impedance will be an electrical state.
- Physically accurate metastability.
- Vendor-specific transistor behavior, protection structures, temperature
  curves, or complete supply-current modeling.
- Gate-level timing back-annotation or SDF support.
- Exact emulation of every historical logic-family tolerance.

## Design Principles

1. Add generic primitives before named IC wrappers.
2. Use supply-relative thresholds referenced to VDD and VSS.
3. Keep propagation delay, input resistance, output resistance, and output
   capacitance overridable.
4. Give behavioral state nodes an explicit high-value DC path.
5. Shape ideal behavioral transitions with finite resistance and capacitance.
6. Document active polarity in both the subcircuit contract and C# API.
7. Treat simultaneous asynchronous controls deterministically and document the
   chosen priority.
8. Do not claim setup/hold or metastability behavior that the model does not
   actually implement.
9. Use a transient maximum step below modeled propagation delay and output
   edge time.
10. Add direct tests for the shipped text netlists, not only programmatic
    facade tests.

## Priority 1: Core Primitives

Milestone A items are complete. The D latch and D flip-flop remain the first
clocked-state work for Milestone B.

| Order | Component | Proposed subcircuit and ordered pins | Proposed facade API |
| ---: | --- | --- | --- |
| 1 | Schmitt buffer | `DIG_SCHMITT_BUF A Y VDD VSS` | `AddSchmittBuffer` |
| 2 | Schmitt inverter | `DIG_SCHMITT_NOT A Y VDD VSS` | `AddSchmittInverter` |
| 3 | Tri-state buffer | `DIG_TRI_BUF A OE Y VDD VSS` | `AddTriStateBuffer` |
| 4 | Inverting tri-state buffer | `DIG_TRI_NOT A OE Y VDD VSS` | `AddTriStateInverter` |
| 5 | 2-to-1 multiplexer | `DIG_MUX2 D0 D1 S Y VDD VSS` | `AddMultiplexer2` |
| 6 | 4-to-1 multiplexer | `DIG_MUX4 D0 D1 D2 D3 S0 S1 Y VDD VSS` | `AddMultiplexer4` |
| 7 | D latch | `DIG_D_LATCH D EN Q QB PRE CLR VDD VSS` | `AddDLatch` |
| 8 | D flip-flop | `DIG_DFF D CLK Q QB PRE CLR VDD VSS` | `AddDFlipFlop` |
| 9 | Full adder | `DIG_FULL_ADDER A B CIN SUM COUT VDD VSS` | `AddFullAdder` |
| 10 | 2-to-4 decoder | `DIG_DEC2TO4 A B EN Y0 Y1 Y2 Y3 VDD VSS` | `AddDecoder2To4` |

Implemented in Milestone A: orders 1–6, 9, and 10. Deferred to Milestone B:
orders 7 and 8.

### Schmitt-Trigger Requirements

Suggested parameters:

- `VTH_RISE`: rising threshold as a fraction of VDD-VSS.
- `VTH_FALL`: falling threshold as a fraction of VDD-VSS.
- `TPD`, `RIN`, `ROUT`, and `COUT`.
- Optional `INITIAL` state for otherwise ambiguous startup.

Acceptance criteria:

- Output changes at the rising threshold during an upward ramp.
- Output changes at the falling threshold during a downward ramp.
- `VTH_RISE` is greater than `VTH_FALL`.
- A noisy input inside the hysteresis band does not cause output chatter.
- Slow ramps converge without rail overshoot.
- Buffer and inverter polarity are both verified.

### Tri-State Requirements

Suggested parameters:

- `VTH`, `TPD`, `RIN`, `RON`, `ROFF`, and `COUT`.
- Explicit enable polarity, preferably reflected by separate active-high and
  active-low methods or a typed option.

Acceptance criteria:

- Enabled-low and enabled-high logic values are correct.
- Disabled output follows an external pull-up and an external pull-down.
- Disabled leakage matches the configured `ROFF` order of magnitude.
- Two enabled opposing drivers produce a finite contention voltage/current and
  do not cause a singular matrix.
- Enable and disable propagation delay are measured separately.

### D-Latch and D-Flip-Flop Requirements

Version-one semantics should be deliberately functional:

- D latch is transparent while EN is active and holds while EN is inactive.
- D flip-flop samples on one documented clock edge.
- PRE and CLR are asynchronous.
- Simultaneous PRE and CLR use a documented deterministic priority or a clearly
  documented invalid-state approximation.
- Version one does not claim physical metastability.
- Setup and hold behavior is documented as ideal sampling unless separately
  implemented and tested.

Suggested parameters:

- `VTH`, `TPD_DATA`, `TPD_CLOCK`, `TPD_ASYNC`, `RIN`, `ROUT`, `COUT`.
- `RSTATE`, `RHOLD`, `CMEM`, and optional `INITIAL`.
- Optional later parameters `TSETUP` and `THOLD` only after violation behavior
  is explicitly designed.

Acceptance criteria:

- Transparent, hold, rising-edge, and non-triggering-edge cases pass.
- Q and QB remain complementary in every valid state.
- Asynchronous preset and clear override clock/data activity.
- Startup is deterministic under explicit reset or explicit initialization.
- Multiple clock cycles pass without state drift.

## Priority 2: Sequential Building Blocks

Implement after D latch and D flip-flop validation.

| Component | Proposed role |
| --- | --- |
| `DIG_TFF` | Toggle flip-flop and divide-by-two primitive |
| `DIG_JKFF` | General set/reset/toggle flip-flop |
| `DIG_REG4` | Four-bit edge-triggered register with clear and output enable |
| `DIG_REG8` | Eight-bit register for buses and package wrappers |
| `DIG_COUNTER4_UP` | Four-bit synchronous up counter |
| `DIG_COUNTER4_UPDOWN` | Loadable up/down counter with carry/borrow |
| `DIG_SHIFT_SIPO8` | Serial-in, parallel-out shift register |
| `DIG_SHIFT_PISO8` | Parallel-in, serial-out shift register |
| `DIG_RING_COUNTER8` | One-hot ring counter |
| `DIG_DECADE_COUNTER` | Ten-state decoded counter |
| `DIG_FREQ_DIVIDER` | Configurable or fixed-width clock divider |

Sequential acceptance tests should cover reset, load, enable, terminal count,
rollover, clock polarity, cascaded carry, and at least two complete state
cycles. Shift-register tests should verify bit ordering and serial cascading.

## Priority 3: Combinational Building Blocks

- Three- and four-input AND, NAND, OR, and NOR gates.
- 3-to-8 decoder/demultiplexer with enable inputs.
- 8-to-3 priority encoder.
- 1-to-4 and 1-to-8 demultiplexers.
- Four-bit magnitude comparator.
- Four-bit ripple-carry adder.
- Four-bit carry-lookahead adder.
- Parity generator/checker.
- BCD-to-seven-segment decoder.
- Binary-to-BCD converter.
- Majority-vote gate.
- One-hot detector.
- Leading-zero detector.
- Small combinational lookup-table block.

Every combinational component should have an exhaustive truth-table test when
the input width makes that practical. Wider arithmetic blocks should use
boundary vectors, randomized deterministic vectors, and carry-chain cases.

## Priority 4: Bus and Interface Models

- Bidirectional transmission gate.
- Analog/digital switch.
- Four-bit and eight-bit bus transceivers.
- Direction-controlled tri-state buffer.
- Open-drain bus line composed from `DIG_OPEN_DRAIN`.
- Wired-AND interrupt line example.
- Unidirectional logic-level translator.
- Bidirectional level translator with enable.
- Clock buffer with enable.
- Reset synchronizer.
- Two-stage input synchronizer.
- Weak bus keeper.
- Contention detector output.

Bus tests must verify disabled leakage, external bias behavior, direction
control, simultaneous-drive contention, propagation in both directions, and
the absence of floating-node lint errors.

## Priority 5: Standard-Logic IC Wrappers

These wrappers should reuse the generic primitives and preserve recognizable
pin behavior and active polarity.

| Wrapper | Required primitives |
| --- | --- |
| HC14-style hex Schmitt inverter | Schmitt inverter |
| HC74-style dual D flip-flop | D flip-flop with asynchronous controls |
| HC125-style quad tri-state buffer | Tri-state buffer |
| HC138-style 3-to-8 decoder | Decoder and enable logic |
| HC157-style quad 2-to-1 multiplexer | Multiplexer and enable logic |
| HC161/HC163-style counter | D flip-flop, register, counter logic |
| HC193-style up/down counter | Counter, load, carry, and borrow logic |
| HC245-style bus transceiver | Bidirectional tri-state bus driver |
| HC283-style four-bit adder | Full-adder chain |
| HC85-style magnitude comparator | Comparator logic and cascade inputs |
| HC165-style PISO register | D flip-flop and parallel-load shift logic |
| HC595-style SIPO/storage register | Shift register, storage register, tri-state outputs |
| CD4017-style decade counter | Counter and one-hot decode |
| CD4040-style ripple counter | Toggle flip-flop chain |

The HC595-style model is a particularly valuable integration target. It tests
two separate positive-edge clock domains, serial cascading, asynchronous clear,
an output storage register, and tri-state parallel outputs.

## Priority 6: Mixed-Signal Digital Helpers

- Switch debouncer.
- Power-on-reset generator.
- Reset supervisor with hysteresis.
- Rising-edge and falling-edge detectors.
- Retriggerable monostable.
- Non-retriggerable monostable.
- Window comparator.
- Pulse-width detector.
- Missing-clock detector.
- Voltage-controlled oscillator.
- Simple XOR and edge-triggered phase detectors.
- PWM generator.
- Frequency-to-voltage block.

The CD4046-style PLL/VCO should be a late milestone. Its VCO transfer function,
phase detectors, loop filter, startup behavior, capture range, lock range, and
solver convergence require a separate quantitative design effort.

## Proposed C# API Evolution

Keep `DigitalGateKind` for simple Boolean gates. Add explicit methods for blocks
whose pins or semantics are not interchangeable.

Suggested parameter types:

- `DigitalGateParameters`: retain for ordinary gates.
- `DigitalSchmittParameters`: rising/falling threshold, initial state, delay,
  and loading.
- `DigitalTriStateParameters`: enable polarity, on/off resistance, delay,
  leakage, and output capacitance.
- `DigitalSequentialParameters`: clock threshold, asynchronous priority,
  initial state, state retention, delay, and loading.
- `DigitalBusParameters`: direction/enable polarity, drive resistance, leakage,
  and bus capacitance.

Continue allowing raw `IReadOnlyDictionary<string, string>` overrides for
advanced SPICE expressions, but validate typed parameter objects before
mutating the target circuit.

If the text library grows substantially, split implementation files by family:

```text
Digital/Netlists/
  digital-combinational.lib
  digital-sequential.lib
  digital-interface.lib
  digital-standard-ics.lib
  digital-mixed-signal.lib
```

`DigitalSubcircuitLibrary.LoadBuiltIn()` can still expose one aggregate facade
by loading a generated or concatenated embedded library.

## Verification Strategy

### Common Tests

- Library parses with no diagnostics.
- Pin metadata and default parameters match documentation.
- Every public facade method validates pin count and instance name.
- Invalid typed parameters fail before circuit mutation.
- Pure SpiceSharp circuits and parser-created circuits both work.
- Shipped text examples compile through `SpiceCompiler.CompileFile`.
- `NetlistLinter.Lint` reports no structural errors.
- Stable static fixtures pass `SmokeTester.QuickCheck`.
- Transient tests specify an appropriate `tmax`.

### Combinational Tests

- Exhaustive truth table.
- Per-instance threshold override.
- Propagation delay through rising and falling transitions.
- Output voltage under resistive and capacitive loading.

### Stateful Tests

- Initialization and asynchronous reset/preset.
- Hold behavior across multiple delay constants.
- Correct response to every relevant clock edge.
- No response to the opposite edge.
- Long sequence without state leakage or drift.
- Explicit test for documented simultaneous-control priority.

### Bus Tests

- Enabled logic-low and logic-high drive.
- Disabled external pull-up and pull-down.
- Leakage/off resistance.
- Contention between opposing enabled drivers.
- Direction switching and enable timing.

### Integrated-Device Tests

- Package-level pin order and active polarity.
- Composition behavior beyond primitive tests.
- Cascading between two instances.
- Representative application netlist with `.MEAS` acceptance criteria.

All milestone completion runs should emit TRX evidence and include the focused
suite, full unit suite, full integration suite, dual-target release build, and
NuGet content inspection.

## Recommended Delivery Milestones

### Milestone A: Analog/Digital Boundary and Routing — Complete

1. Schmitt buffer and inverter.
2. Tri-state buffer and inverter.
3. 2-to-1 and 4-to-1 multiplexers.
4. Full adder.
5. 2-to-4 decoder.

### Milestone B: Clocked State

1. D latch.
2. Positive-edge D flip-flop.
3. T flip-flop.
4. Four-bit register.
5. Four-bit counter.

### Milestone C: Data Movement

1. Eight-bit SIPO shift register.
2. Eight-bit PISO shift register.
3. Eight-bit tri-state register.
4. Eight-bit bus transceiver.
5. HC595-style integrated model.

### Milestone D: Standard-Logic Coverage

1. HC14, HC74, HC125.
2. HC138 and HC157.
3. HC161/HC193.
4. HC245, HC283, and HC85.
5. HC165, HC595, CD4017, and CD4040.

### Milestone E: Advanced Mixed-Signal Logic

1. Debouncer and power-on reset.
2. Edge detector and monostable.
3. Window and pulse-width detectors.
4. VCO and phase detector primitives.
5. CD4046-style PLL only after a separate requirements and convergence study.

## Definition of Done for Each Component

- Quantitative requirements and explicit semantic choices are documented.
- Subcircuit is added to an embedded and packaged text library.
- Public facade method and typed parameters are added where appropriate.
- Metadata and parameter defaults are tested.
- Functional, timing, loading, and invalid-input tests pass.
- Smoke and lint checks are recorded where applicable.
- At least one direct text-netlist example compiles and simulates.
- README and detailed digital-library documentation are updated.
- Backlog and discoveries are updated with reusable lessons.
- Focused and complete regression suites pass with TRX output.
- NuGet packaging still contains all libraries and target-framework assemblies.

## Reference Devices

The generic models should not copy vendor transistor behavior, but these
standard-logic devices provide useful functional contracts:

- [TI SN74HC14 Schmitt-trigger inverter](https://www.ti.com/lit/ds/symlink/sn74hc14.pdf)
- [TI SN74HC125 tri-state buffer](https://www.ti.com/lit/ds/symlink/sn74hc125.pdf)
- [TI SN74HC74 D flip-flop](https://www.ti.com/lit/ds/symlink/sn74hc74.pdf)
- [TI SN74HC138 decoder/demultiplexer](https://www.ti.com/product/SN74HC138)
- [TI SN74HC157 multiplexer](https://www.ti.com/product/SN74HC157)
- [TI SN74HC595 shift/storage register](https://www.ti.com/lit/ds/symlink/sn54hc595.pdf)
- [TI CD74HC4046A PLL/VCO](https://www.ti.com/product/CD74HC4046A)

## Recommended Immediate Next Step

Start Milestone B by specifying deterministic initialization and asynchronous
PRE/CLR priority for `DIG_D_LATCH` and positive-edge `DIG_DFF`. Reuse the
Milestone A conventions for explicit state-node DC paths, supply-relative
thresholds, finite output stages, fail-before-mutation typed validation, and
direct text-netlist verification.
