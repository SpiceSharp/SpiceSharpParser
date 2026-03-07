# .AC Statement

The `.AC` statement defines an AC small-signal frequency analysis. The circuit is first linearized around the DC operating point, then swept over a range of frequencies.

## Syntax

```
.AC DEC <points> <fstart> <fstop>
.AC OCT <points> <fstart> <fstop>
.AC LIN <points> <fstart> <fstop>
```

| Parameter | Description |
|-----------|-------------|
| `DEC` | Logarithmic sweep — *points* per decade |
| `OCT` | Logarithmic sweep — *points* per octave |
| `LIN` | Linear sweep — *points* total between fstart and fstop |
| `fstart` | Starting frequency (Hz). Must be > 0 for DEC/OCT |
| `fstop` | Ending frequency (Hz) |

## Examples

```spice
* 10 points per decade from 1 Hz to 1 MHz
.AC DEC 10 1 1MEG

* 100 linearly spaced points from 60 Hz to 10 kHz
.AC LIN 100 60 10K

* 5 points per octave from 100 Hz to 100 kHz
.AC OCT 5 100 100K
```

## AC Sources

At least one independent source must have an AC specification:

```spice
V1 IN 0 DC 0 AC 1 0
```

The `AC` keyword is followed by magnitude and optional phase (degrees).

## Typical Usage

```spice
Low-pass filter
V1 IN 0 AC 1
R1 IN OUT 1k
C1 OUT 0 1u
.AC DEC 10 1 1MEG
.SAVE V(OUT)
.END
```

## C# API

```csharp
var parser = new SpiceNetlistParser();
var result = parser.ParseNetlist(netlist);
var reader = new SpiceSharpReader();
var model = reader.Read(result.FinalModel);

var sim = model.Simulations.Single(); // AC simulation
var vout = model.Exports.Find(e => e.Name == "V(OUT)");
sim.EventExportData += (s, args) =>
{
    Console.WriteLine(vout.Extract());
};
```
