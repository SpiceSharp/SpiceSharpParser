# Getting Started with SpiceSharpParser

SpiceSharpParser is a .NET library that parses SPICE netlists and simulates them using [SpiceSharp](https://github.com/SpiceSharp/SpiceSharp). It supports a wide subset of PSpice and LTspice syntax including DC, AC, transient, noise, and operating-point analyses.

## Installation

Install from NuGet:

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

## Workflow Overview

Using SpiceSharpParser involves three steps:

1. **Parse** — convert a SPICE netlist string into a parse-tree model with `SpiceNetlistParser.ParseNetlist()`.
2. **Read** — translate the parse-tree model into SpiceSharp simulation objects (circuit, simulations, exports) with `SpiceSharpReader.Read()`.
3. **Simulate** — run the SpiceSharp simulations and collect results via exports and events.

### The SpiceSharpModel

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

### Collecting Results

Export data during simulation using the `EventExportData` event:

```csharp
var export = spiceModel.Exports.Find(e => e.Name == "V(OUT)");
sim.EventExportData += (sender, args) =>
{
    double value = (double)export.Extract();
    // process value
};
```

### Parameter Sweeps (.STEP)

When `.STEP` is used, the simulation runs multiple times. Each sweep iteration fires `EventExportData` with appropriate parameter values:

```csharp
sim.EventExportData += (sender, args) =>
{
    Console.WriteLine($"{export.Extract()}");
};
```

## Supported Features

SpiceSharpParser supports a comprehensive set of SPICE statements and devices. See the individual documentation articles for details on each:

- **Analysis**: `.AC`, `.DC`, `.TRAN`, `.OP`, `.NOISE`
- **Output**: `.SAVE`, `.PRINT`, `.PLOT`, `.MEAS`
- **Parameters**: `.PARAM`, `.FUNC`, `.LET`, `.SPARAM`
- **Circuit structure**: `.SUBCKT`, `.INCLUDE`, `.LIB`, `.GLOBAL`
- **Simulation control**: `.STEP`, `.MC`, `.TEMP`, `.OPTIONS`, `.IC`, `.NODESET`
- **Behavioral modeling**: `VALUE`, `TABLE`, `POLY(n)`, `B` sources, canonical `E`-source `LAPLACE`
- **Devices**: R, L, C, K, D, Q, M, J, V, I, E, F, G, H, B, S, W, T, X
