# SpiceSharpParser

[![NuGet](https://img.shields.io/nuget/v/SpiceSharp-Parser.svg)](https://www.nuget.org/packages/SpiceSharp-Parser)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=SpiceSharp_SpiceSharpParser&metric=coverage)](https://sonarcloud.io/summary/new_code?id=SpiceSharp_SpiceSharpParser)

SpiceSharpParser parses SPICE netlists and translates them into ordinary
[SpiceSharp](https://github.com/SpiceSharp/SpiceSharp) circuits and simulations.
It supports broad SPICE, PSpice, and LTspice syntax for .NET Standard 2.0 and
.NET 8.0.

**Current release:** `3.4.1` is a hardening-only release for experimental
LTspice A-device integration. It adds fixes and regression coverage without
adding new A-device families.

For the complete feature and API reference, see the
[detailed project guide](PROJECT_GUIDE.md).

## Highlights

- Parse `.AC`, `.DC`, `.TRAN`, `.OP`, and `.NOISE` analyses.
- Translate netlists into normal SpiceSharp circuits and simulation objects.
- Load recursive `.INCLUDE`/`.LIB` dependencies with source-located diagnostics.
- Use `.SAVE`, `.PRINT`, `.PLOT`, `.MEAS`, and `.FOUR` results from the netlist.
- Load reusable `.SUBCKT` libraries into programmatically assembled circuits.
- Opt into LTspice A-devices, ideal diodes, nonlinear passives, digital logic,
  analog functional models, and a functional 555 with the companion package.

## Installation

Install the core parser:

```shell
dotnet add package SpiceSharp-Parser
```

Install the optional models only when they are needed:

```shell
dotnet add package SpiceSharpParser.CustomComponents
```

| Requirement | Package |
| --- | --- |
| Parse and simulate ordinary SPICE netlists | `SpiceSharp-Parser` |
| Load user-authored `.SUBCKT` libraries | `SpiceSharp-Parser` |
| Parse supported LTspice A-devices and ideal/nonlinear devices | `SpiceSharpParser.CustomComponents` |
| Use packaged digital, analog, and functional 555 models | `SpiceSharpParser.CustomComponents` |

See the
[CustomComponents README](src/SpiceSharpParser.CustomComponents/README.md) for
setup, model explanations, examples, and compatibility limits.

## Quick Start

The original API exposes parsing, translation, and simulation as separate
steps:

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

// Parse the SPICE text.
var parser = new SpiceNetlistParser();
var parsed = parser.ParseNetlist(netlist);

// Translate it into SpiceSharp objects.
var reader = new SpiceSharpReader();
var model = reader.Read(parsed.FinalModel);

// Run the analysis and read the requested export.
var simulation = model.Simulations.Single();
var output = model.Exports.Find(export => export.Name == "V(OUT)");
simulation.EventExportData += (_, _) => Console.WriteLine(output.Extract());
simulation.Execute(model.Circuit);
```

## Choose an API

| Goal | API |
| --- | --- |
| Direct control over an in-memory netlist | `SpiceNetlistParser`, then `SpiceSharpReader` |
| Compile user or vendor files with stable diagnostics | `SpiceCompiler.CompileFile(...)` |
| Load selected `.SUBCKT` definitions into a circuit | `SpiceSubcircuitLibrary.LoadFile(...)` or `LoadText(...)` |
| Enable optional netlist components in the original API | `reader.Settings.UseCustomComponents()` before `Read()` |
| Enable optional components with `SpiceCompiler` | `SpiceCompileOptions.ConfigureReader` |

`SpiceCompiler` adds recursive dependency loading, recovery, compatibility
classification, structural linting, and JSON/SARIF diagnostic output. See the
[detailed project guide](PROJECT_GUIDE.md#structured-compilation) and
[diagnostic reference](docs/diagnostics.md).

## Optional LTspice A-devices

`SpiceSharpParser.CustomComponents` supports these native eight-terminal
A-device families:

| Digital | Analog and mixed-signal |
| --- | --- |
| `SRFLOP`, `DFLOP`, `PHASEDET`, `COUNTER` | `SAMPLEHOLD`, `OTA`, `VARISTOR`, `MODULATOR`/`MODULATE` |

Terminal 8 is common. A terminal is unused only when it repeats that common
node; if common is not ground, a literal `0` remains an active global-ground
connection. Unused outputs are detached, and an unused `MODULATOR` amplitude
input selects the native amplitude of 1.

Complete netlists, `.SAVE`/`.PLOT` waveforms, `.MEAS` checks, and guides are in
the [runnable A-device example pack](circuits/a-devices/README.md).


## Documentation

The former long README reference is split across these focused documents:

| Topic | Document |
| --- | --- |
| Full project reference | [Detailed Project Guide](PROJECT_GUIDE.md) |
| Practical, measured designs | [Circuit Cookbook](circuits/README.md) |
| Documentation by statement and device | [Documentation Index](src/docs/index.md) |
| Parser introduction | [Introduction](src/docs/articles/intro.md) |
| Compiler diagnostics | [Diagnostic Reference](docs/diagnostics.md) |
| LTspice compatibility status | [Compatibility Matrix](roadmap/ltspice-compatibility-matrix.md) |
| Programmatic subcircuit libraries | [Subcircuit Library Guide](src/docs/articles/subcircuit-library.md) |
| LTspice A-devices | [A-device Guide](src/docs/articles/a-devices.md) |
| Digital logic and functional 555 | [Digital Subcircuits](src/docs/articles/digital-subcircuits.md) |
| Analog special functions | [Analog Subcircuits](src/docs/articles/analog-subcircuits.md) |
| LTspice-style ideal diode | [Ideal Diode](src/docs/articles/ideal-diode.md) |
| Nonlinear capacitors and inductors | [Nonlinear Passives](src/docs/articles/nonlinear-passives.md) |
| Measurements and output | [.MEAS](src/docs/articles/meas.md), [.PRINT](src/docs/articles/print.md), [.PLOT](src/docs/articles/plot.md) |

## Build from Source

```shell
git clone https://github.com/SpiceSharp/SpiceSharpParser.git
cd SpiceSharpParser

dotnet restore src/SpiceSharp-Parser.sln
dotnet build src/SpiceSharp-Parser.sln
dotnet test src/SpiceSharpParser.Tests/SpiceSharpParser.Tests.csproj
dotnet test src/SpiceSharpParser.IntegrationTests/SpiceSharpParser.IntegrationTests.csproj
```

## License

SpiceSharpParser is licensed under the [MIT License](LICENSE).
