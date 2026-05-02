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

## Supported Features

### Analysis

`.AC`, `.DC`, `.TRAN`, `.OP`, `.NOISE`

### Output

`.SAVE`, `.PRINT`, `.PLOT`, `.MEAS` / `.MEASURE` (TRIG/TARG, WHEN, FIND, MAX, MIN, AVG, RMS, PP, INTEG, DERIV, PARAM)

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
| Sources | V (voltage), I (current) — DC, AC, PULSE, SIN, PWL, SFFM, AM |
| Controlled sources | E (VCVS), F (CCCS), G (VCCS), H (CCVS) |
| Behavioral | B (arbitrary behavioral source with V= or I= expressions) |
| Switches | S (voltage-controlled), W (current-controlled) |

### Behavioral Modeling

`VALUE={expr}`, `TABLE={expr}`, `POLY(n)`, `B` sources, source-level `E` / `G` / `F` / `H` `LAPLACE` transfer functions, and a full set of built-in math functions. `LAPLACE` supports voltage-controlled and current-controlled forms with rational polynomials in `s`, including finite constant `M=`, `TD=`, and `DELAY=` options.

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
