# SpiceSharpParser

[![NuGet](https://img.shields.io/nuget/v/SpiceSharp-Parser.svg)](https://www.nuget.org/packages/SpiceSharp-Parser)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=SpiceSharp_SpiceSharpParser&metric=coverage)](https://sonarcloud.io/summary/new_code?id=SpiceSharp_SpiceSharpParser)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=SpiceSharp_SpiceSharpParser&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=SpiceSharp_SpiceSharpParser)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=SpiceSharp_SpiceSharpParser&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=SpiceSharp_SpiceSharpParser)

A .NET library that parses SPICE netlists and simulates them using [SpiceSharp](https://github.com/SpiceSharp/SpiceSharp). It supports a wide subset of PSpice and LTspice syntax including DC, AC, transient, noise, and operating-point analyses.

**Targets:** .NET Standard 2.0 / .NET 8.0

## Installation

```
dotnet add package SpiceSharp-Parser
```

Or via the NuGet Package Manager:

```
Install-Package SpiceSharp-Parser
```

## Quick Example

```csharp
using System;
using System.Linq;
using SpiceSharpParser;
using SpiceSharpParser.Common;

var netlist = string.Join(Environment.NewLine,
    "Low-pass RC filter",
    "V1 IN 0 AC 1",
    "R1 IN OUT 1k",
    "C1 OUT 0 1u",
    ".AC DEC 10 1 1MEG",
    ".SAVE V(OUT)",
    ".END");

// Parse
var parser = new SpiceNetlistParser();
var parseResult = parser.ParseNetlist(netlist);

// Translate to SpiceSharp objects
var reader = new SpiceSharpReader();
var spiceModel = reader.Read(parseResult.FinalModel);

// Run simulation
var sim = spiceModel.Simulations.Single();
var export = spiceModel.Exports.Find(e => e.Name == "V(OUT)");
sim.EventExportData += (sender, args) => Console.WriteLine(export.Extract());
sim.Execute(spiceModel.Circuit);
```

## Workflow

Using SpiceSharpParser involves three steps:

1. **Parse** — convert a SPICE netlist string into a parse-tree model with `SpiceNetlistParser.ParseNetlist()`.
2. **Read** — translate the parse-tree into SpiceSharp simulation objects with `SpiceSharpReader.Read()`.
3. **Simulate** — run the SpiceSharp simulations and collect results via exports and events.

### SpiceSharpModel

`SpiceSharpReader.Read()` returns a `SpiceSharpModel` containing:

| Property | Description |
|----------|-------------|
| `Circuit` | The SpiceSharp `Circuit` with all components |
| `Simulations` | List of simulation objects (DC, AC, Transient, etc.) |
| `Exports` | Signal exports created by `.SAVE`, `.PRINT`, `.PLOT` |
| `XyPlots` | Plot data from `.PLOT` statements |
| `Prints` | Tabular data from `.PRINT` statements |
| `Measurements` | Results from `.MEAS` / `.MEASURE` statements |
| `Title` | Netlist title (first line) |
| `FourierAnalyses` | Fourier analysis results from .FOUR statements |

## Supported Features

### Analysis

`.AC`, `.DC`, `.TRAN`, `.OP`, `.NOISE`

### Output

`.SAVE`, `.PRINT`, `.PLOT`, `.MEAS` / `.MEASURE` (TRIG/TARG, WHEN, FIND, MAX, MIN, AVG, RMS, PP, INTEG, DERIV, PARAM), and `.FOUR` transient Fourier post-processing with structured `model.FourierAnalyses` results.

### Parameters & Functions

`.PARAM`, `.FUNC`, `.LET`, `.SPARAM`

### Circuit Structure

`.SUBCKT` / `.ENDS`, `X` (subcircuit instances), `.INCLUDE`, `.LIB`, `.GLOBAL`, `.APPENDMODEL`

### Simulation Control

`.STEP`, `.MC` (Monte Carlo), `.TEMP`, `.OPTIONS`, `.IC`, `.NODESET`, `.ST`, `.IF`, `.DISTRIBUTION`

### Devices

| Category | Devices |
|----------|---------|
| Passive | R (resistor), C (capacitor), L (inductor), K (mutual inductance), T (transmission line) |
| Semiconductor | D (diode), Q (BJT), M (MOSFET), J (JFET) |
| Sources | V (voltage), I (current) — DC, AC, PULSE, SIN / SINE, EXP, PWL, SFFM, AM, wave-file input |
| Controlled sources | E (VCVS), F (CCCS), G (VCCS), H (CCVS) |
| Behavioral | B (arbitrary behavioral source with V= or I= expressions) |
| Switches | S (voltage-controlled), W (current-controlled) |

### Dialect Compatibility

Dialect-specific behavior is opt-in. Without it, expressions retain the
SPICE3f5-compatible default operator rules:

```csharp
var compatibility = CompatibilityOptions.None; // SPICE3f5-compatible default
// var compatibility = CompatibilityOptions.PSpice;
// var compatibility = CompatibilityOptions.LTspice;

var parser = new SpiceNetlistParser();
parser.Settings.Compatibility = compatibility;
var parseResult = parser.ParseNetlist(netlist);

var reader = new SpiceSharpReader();
reader.Settings.Compatibility = compatibility;
var model = reader.Read(parseResult.FinalModel);
```

Use the same preset for the parser and reader. Select
`CompatibilityOptions.PSpice` when caret means boolean XOR, or
`CompatibilityOptions.LTspice` for the supported LTspice-specific syntax.

Expression operators are dialect-aware:

| Mode | Boolean NOT | Boolean XOR | Exponentiation |
|------|-------------|-------------|----------------|
| Default / SPICE3f5 | `!` | — | `**` or `^` |
| PSpice compatibility | `!` or `~` | `^` | `**` |
| LTspice compatibility | `!` or `~` | `xor(a,b)` | `**` or `^` |

For PSpice expressions, `~` is unary NOT and binary boolean operators bind in
the order `&`, then `^`, then `|`. In default and LTspice modes, caret remains
an exponent operator. LTspice function-style `xor(a,b)` requires exactly two
arguments.

LTspice mode covers syntax such as:

- `.backanno` and selected output/viewer options as warning no-ops
- one-argument `.TRAN` with a derived step policy
- scalar expression aliases, `table(...)` / `tbl(...)`, smooth limiters `uplim(...)` / `dnlim(...)`, and deterministic behavioral `rand(x)`, `random(x)`, and `white(x)` waveforms
- source waveforms including `EXP(...)`, finite-cycle `PULSE(... Ncycles)`, finite-cycle `SINE(... Ncycles)`, local two-column `PWL file=<path>` data, PWL `TIME_SCALE_FACTOR` / `VALUE_SCALE_FACTOR`, and simple LTspice `PWL REPEAT FOR` / `REPEAT FOREVER` blocks
- independent-source topology options: `Rser`, `Cpar`, `load`, and `R=<value>`
- model parameter aliases: resistor and capacitor models accept `tc=<tc1>[,<tc2>]`; voltage switches accept `von` / `voff`; current switches accept `ion` / `ioff`
- switch model series options `Vser` and `Lser`
- resistor instance parasitics `Rser`, `Rpar`, and `Cpar`
- capacitor instance parasitics `Rser`, `Lser`, `Rpar`, and `Cpar`
- inductor instance parasitics `Rser`, `Lser`, `Rpar`, `RLshunt`, and `Cpar`

Behavioral `rand(x)`, `random(x)`, and `white(x)` reproduce LTspice's value
ranges, integer-interval holding, interpolation timing, and smoothing rules. They
use a parser-owned deterministic hash, so their exact pseudorandom values do not
match LTspice's proprietary sequence. The LTspice-backed transient golden test
is therefore an invariant comparison: it checks the holding and interpolation
relationships while canceling the actual random samples; it is not a
sample-for-sample sequence comparison. The existing zero-argument `random()`
extension is separate from LTspice's one-argument `random(x)` behavior.

Topology-changing LTspice options that can be represented safely are synthesized as helper components in the parser. Behavior-changing constructs that are not represented by SpiceSharp are reported with targeted diagnostics instead of being silently ignored.

See the [LTspice compatibility matrix](roadmap/ltspice-compatibility-matrix.md) and [LTspice netlist compatibility plan](roadmap/ltspice-netlist-compatibility-plan.md) for the current support classes, known gaps, and evidence policy. Selected compatibility surfaces also have optional LTspice-backed golden tests; set `LTSPICE_EXE` to an LTspice executable to enable them. LTspice schematic and symbol import (`.asc` / `.asy`) is out of scope.

### Custom Components

The optional `SpiceSharpParser.CustomComponents` package/project adds opt-in parser mappings for LTspice-style
ideal diode models and nonlinear passive devices:

```spice
.model did D(Ron=0.1 Roff=1e9 Vfwd=0.7 Ilimit=10 Epsilon=10m)
D1 out 0 did

C1 out 0 Q=1u*x+100n*x*x
L1 in out Flux=1m*x+100u*x*x
```

Add `using SpiceSharpParser.CustomComponents;` and enable the mappings with
`reader.Settings.UseCustomComponents()` before calling `Read()`.
Ideal diode models support LTspice-style `Ron`, `Roff`, `Vfwd`, `Vrev`, `Rrev`,
`Ilimit`, `RevIlimit`, `Epsilon`, `RevEpsilon`, `M`, and `N` behavior, while ordinary
diode models still fall back to SpiceSharp's built-in semiconductor diode.

For nonlinear passives, `Q=` describes stored charge as a function of capacitor
terminal voltage, and `Flux=` describes stored flux linkage as a function of inductor
branch current. The local slopes `dQ/dV` and `dFlux/dI` are used for AC small-signal
behavior and transient companion models. `IC=`, `M=`, and `N=` are supported for these
custom passive forms. LTspice natively supports `M` on capacitors and inductors but
not `N`; nonlinear-passive `N` is a parser extension that models explicit series
cells. The LTspice-backed goldens compare it with equivalent native LTspice circuits.

See [LTspice-Style Ideal Diode](src/docs/articles/ideal-diode.md) for syntax, scaling rules, current-law details, and the optional LTspice-backed golden tests for DC, AC, and transient parity.

See [LTspice-Style Nonlinear Passives](src/docs/articles/nonlinear-passives.md) for
`Q=` / `Flux=` syntax, transient behavior, scaling rules, and optional LTspice-backed
golden tests for AC and transient parity.

### Behavioral Modeling

`VALUE={expr}`, `TABLE={expr}`, `POLY(n)`, `B` sources, source-level `E` / `G` / `F` / `H` `LAPLACE` transfer functions, function-style `LAPLACE(input, transfer)` in behavioral expressions, and a full set of built-in math functions including LTspice-style `uplim(...)` and `dnlim(...)` smooth limiters. `LAPLACE` supports voltage-controlled and current-controlled forms with rational polynomials in `s`, including finite constant `M=`, `TD=`, and `DELAY=` options. Function-style calls also support call-local options, mixed-expression helper lowering, and arbitrary scalar input expressions.

## Documentation

See the [documentation articles](src/docs/articles/) for detailed guides on each statement and device type.

## Building from Source

```bash
git clone https://github.com/SpiceSharp/SpiceSharpParser.git
cd SpiceSharpParser/src
dotnet build
dotnet test
```

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
