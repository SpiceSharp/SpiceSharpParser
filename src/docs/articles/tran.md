# .TRAN Statement

The `.TRAN` statement defines a transient (time-domain) analysis. The circuit is simulated from time 0 to a specified stop time.

## Syntax

```
.TRAN <tstep> <tstop> [<tstart> [<tmaxstep>]] [UIC]
```

| Parameter | Description |
|-----------|-------------|
| `tstep` | Suggested time step for output |
| `tstop` | End time of the simulation |
| `tstart` | Start time for saving output (default: 0) |
| `tmaxstep` | Maximum internal time step |
| `UIC` | Use Initial Conditions — skip DC operating point, use `.IC` values |

## Examples

```spice
* Simulate from 0 to 1ms with 1ns output step
.TRAN 1e-9 1e-3

* Simulate 10ms, output from 5ms, max step 100ns
.TRAN 1e-7 10e-3 5e-3

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

## C# API

```csharp
var sim = model.Simulations.Single(); // Transient simulation
var vout = model.Exports.Find(e => e.Name == "V(OUT)");
sim.EventExportData += (s, args) =>
{
    Console.WriteLine(vout.Extract());
};
```
