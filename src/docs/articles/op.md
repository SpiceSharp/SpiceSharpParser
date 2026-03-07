# .OP Statement

The `.OP` statement requests a DC operating point analysis. The simulator computes the quiescent (steady-state) voltages and currents of the circuit with no time-varying or frequency-domain stimuli.

## Syntax

```
.OP
```

No parameters are required.

## Example

```spice
Voltage divider
V1 VCC 0 12
R1 VCC OUT 1k
R2 OUT 0 2k
.OP
.SAVE V(OUT)
.END
```

The operating point of `V(OUT)` will be computed as 8V (voltage divider: 12 × 2k / (1k + 2k)).

## Typical Usage

`.OP` is commonly used to:

- Verify bias points in amplifier circuits
- Check quiescent currents through components
- Debug circuit connectivity before running transient or AC simulations

## C# API

```csharp
var sim = model.Simulations.Single(); // OP simulation
var vout = model.Exports.Find(e => e.Name == "V(OUT)");
sim.EventExportData += (s, args) =>
{
    Console.WriteLine($"V(OUT) = {vout.Extract()}");
};
```
