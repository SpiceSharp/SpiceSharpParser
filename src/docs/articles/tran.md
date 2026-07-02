# .TRAN Statement

The `.TRAN` statement defines a transient (time-domain) analysis. The circuit is simulated from time 0 to a specified stop time.

For a tutorial on timestep control, integration methods, companion models, and
the derivatives used by the engine, see
[Transient Integration Methods](transient-integration-methods.md).

## Syntax

Supported numeric forms:

```spice
.TRAN <tstep> <tstop> [UIC]
.TRAN <tstep> <tstop> <tmaxstep> [UIC]
.TRAN <tstep> <tstop> <tstart> <tmaxstep> [UIC]
```

| Parameter | Description |
|-----------|-------------|
| `tstep` | Suggested time step for output |
| `tstop` | End time of the simulation |
| `tmaxstep` | Maximum internal time step; this is the third numeric argument when three numbers are supplied |
| `tstart` | Start time for saving output; available only in the four-number form |
| `UIC` | Use Initial Conditions — skip DC operating point, use `.IC` values |

## Examples

```spice
* Simulate from 0 to 1ms with 1ns output step
.TRAN 1e-9 1e-3

* Simulate 10ms with max internal step 100ns
.TRAN 1e-7 10e-3 100e-9

* Simulate 10ms, output from 5ms, max step 100ns
.TRAN 1e-7 10e-3 5e-3 100e-9

* Use initial conditions
.TRAN 1e-8 1e-5 UIC
```

## Using Initial Conditions

When `UIC` is specified, the DC operating point is skipped and node voltages from `.IC` statements or device `IC=` parameters are used instead:

```spice
Capacitor charge
C1 OUT 0 1e-6 IC=0.0
R1 IN OUT 10e3
V1 IN 0 10
.IC V(OUT)=0
.TRAN 1e-8 1e-5 UIC
.SAVE V(OUT)
.END
```

## Waveform Sources

Transient analysis is typically paired with time-domain sources:

```spice
V1 IN 0 PULSE(0 5 0 10n 10n 500u 1m)
V2 SIG 0 SIN(0 1 1k)
```

See the individual waveform documentation for `PULSE`, `SIN`, `PWL`, `SFFM`, and `AM`.

## MNA View

`.TRAN` solves many MNA systems over time. At each candidate timestep, dynamic
devices are converted into companion models, then the matrix is loaded and
solved.

Conceptually:

```text
choose candidate timestep
build capacitor/inductor companion models
load MNA matrix and RHS
solve Newton iterations
accept or reject the candidate point
commit history only if accepted
```

Capacitor and inductor history terms appear as matrix coefficients and RHS
terms during each candidate solve. If the timestep is rejected, those history
updates are not committed.

For a numeric MNA example of one transient RC timestep, see
[Transient Integration Methods](transient-integration-methods.md#example-rc-step-matrix-for-one-candidate-timestep).

## C# API

```csharp
var sim = model.Simulations.Single(); // Transient simulation
var vout = model.Exports.Find(e => e.Name == "V(OUT)");
sim.EventExportData += (s, args) =>
{
    Console.WriteLine(vout.Extract());
};
```

## Related Articles

- [Transient Integration Methods](transient-integration-methods.md)
- [.OPTIONS](options.md)
- [.IC](ic.md)
