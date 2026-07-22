# Generic 555 Validation Results

Validation date: 2026-07-22

## Test Circuit

The astable fixture uses VCC=5 V, RA=10 kohm, RB=10 kohm, C=10 nF, a
10 kohm output load, and a 10 nF control bypass. The transient uses Gear
integration, UIC, and a 10 ns maximum timestep.

## Measured Timing

| Measurement | Ideal target | SpiceSharp result | Error |
| --- | ---: | ---: | ---: |
| High time | 138.600 us | 139.107 us | +0.37% |
| Low time | 69.300 us | 69.781 us | +0.69% |
| Period | 207.900 us | 208.887 us | +0.47% |
| Frequency | 4.810 kHz | 4.787 kHz | -0.47% |
| Timing minimum | 1.667 V nominal | 1.663 V | -0.20% |
| Timing maximum | 3.333 V nominal | 3.335 V | +0.05% |

All timing results are inside the 5% acceptance window. The output remained
between 0 V and 4.990 V with the nominal load.

## Functional Checks

- Comparator output polarity passed both differential directions.
- The SR latch passed set, hold, reset, and simultaneous-input reset priority.
- The open-drain block pulled below 0.1 V when enabled and released above
  4.9 V with a 10 kohm pull-up.
- TIMER555 enforced RESET over TRIG and TRIG over THRESH.
- A reset-held static timer passed parsing, NetlistLinter, and SmokeTester OP
  convergence checks.
- The embedded library exposed all twelve definitions with no diagnostics.

## Regression Evidence

- Focused DigitalSubcircuitLibraryTests: 40 passed, 0 failed.
- Full unit suite: 562 passed, 9 skipped, 0 failed.
- Full integration suite: 968 passed, 2 skipped, 0 failed.
- Release builds passed for netstandard2.0 and net8.0.
- The generated NuGet archive contains both framework assemblies and
  contentFiles/any/any/SpiceSharpParser.CustomComponents/Digital/standard-digital.lib.

The focused, unit, and integration runs emitted TRX files under their test
projects' TestResults directories.
