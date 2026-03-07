# .NOISE Statement

The `.NOISE` statement requests a noise analysis. It computes the noise contributions from each device and the total output noise spectral density over a range of frequencies.

## Syntax

```
.NOISE V(<output_node>[, <ref_node>]) <input_source> <sweep_type> <points> <fstart> <fstop>
```

| Parameter | Description |
|-----------|-------------|
| `V(<output_node>)` | Output node for noise measurement |
| `<ref_node>` | Optional reference node (default: ground) |
| `<input_source>` | Input source for input-referred noise |
| `<sweep_type>` | `DEC`, `OCT`, or `LIN` |
| `<points>` | Number of frequency points (per decade/octave, or total) |
| `<fstart>` | Starting frequency (Hz) |
| `<fstop>` | Ending frequency (Hz) |

## Example

```spice
Amplifier noise
V1 IN 0 AC 1
R1 IN OUT 10k
R2 OUT 0 10k
.NOISE V(OUT) V1 DEC 10 1 1MEG
.END
```

## C# API

```csharp
var sim = model.Simulations.Single(); // Noise simulation
sim.EventExportData += (s, args) =>
{
    // Noise data available through exports
    Console.WriteLine(export.Extract());
};
```
