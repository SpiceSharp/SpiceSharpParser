# .DC Statement

The `.DC` statement defines a DC sweep analysis. One or more independent source values are swept over a range while the DC operating point is computed at each step.

## Syntax

```
.DC <srcname> <vstart> <vstop> <vincr> [<srcname2> <vstart2> <vstop2> <vincr2>]
```

| Parameter | Description |
|-----------|-------------|
| `srcname` | Name of the independent source to sweep (e.g., `V1`) |
| `vstart` | Starting value |
| `vstop` | Ending value |
| `vincr` | Increment step size |

A second source can be specified for nested (double) sweeps.

## Examples

```spice
* Single sweep: V1 from -5V to 5V in 0.1V steps
.DC V1 -5 5 0.1

* Double sweep: V1 from 0 to 5V, for each value of V2 from 0 to 3V
.DC V1 0 5 0.1 V2 0 3 1
```

## Typical Usage

```spice
IV Characteristic
V1 in 0 0
R1 in 0 10
.DC V1 -10 10 1e-3
.SAVE V(in) I(V1)
.END
```

## Diode IV Curve

```spice
Diode characteristic
D1 OUT 0 1N914
V1 OUT 0 0
.model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752)
.DC V1 -1 1 10e-3
.SAVE I(V1)
.END
```

## C# API

```csharp
var sim = model.Simulations.Single(); // DC simulation
var export = model.Exports.Find(e => e.Name == "I(V1)");
sim.EventExportData += (s, args) =>
{
    Console.WriteLine(export.Extract());
};
```
