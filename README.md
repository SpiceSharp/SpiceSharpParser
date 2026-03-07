# <img src="https://spicesharp.github.io/SpiceSharp/api/images/logo_full.svg" width="45px" /> SpiceSharpParser

[<img src="https://img.shields.io/nuget/vpre/SpiceSharp-Parser.svg">](https://www.nuget.org/packages/SpiceSharp-Parser)

SpiceSharpParser is a .NET library that parses SPICE netlists and simulates them using [SpiceSharp](https://github.com/SpiceSharp/SpiceSharp). It targets **netstandard2.0** and **net8.0**, so it works on .NET Framework 4.6.1+, .NET Core, and .NET 5–8+.

## Features

- Parses industry-standard SPICE netlists (PSpice / LTspice dialect)
- Runs DC, AC, transient, operating-point, and noise analyses via SpiceSharp
- Parameter sweeps (`.STEP`), Monte Carlo (`.MC`), and multi-temperature runs
- Post-simulation measurements (`.MEAS` / `.MEASURE`) with TRIG/TARG, FIND, WHEN, AVG, RMS, MIN, MAX, INTEG, and PARAM
- Analog behavioral modeling: `VALUE`, `TABLE`, `POLY(n)`
- Parameterized designs with `.PARAM`, `.FUNC`, `.LET`, `.SUBCKT`
- Conditional netlist sections with `.IF` / `.ELSE` / `.ENDIF`
- Structured output via `.SAVE`, `.PRINT`, `.PLOT`, `.WAVE`

## Installation

```
dotnet add package SpiceSharp-Parser
```

Or via the Package Manager Console:

```
Install-Package SpiceSharp-Parser
```

## Quick Start

```csharp
using System;
using System.Linq;
using SpiceSharpParser;

var netlist = string.Join(Environment.NewLine,
    "Diode circuit",
    "D1 OUT 0 1N914",
    "V1 OUT 0 0",
    ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
    ".DC V1 -1 1 10e-3",
    ".SAVE i(V1)",
    ".END");

// 1. Parse the netlist
var parser = new SpiceNetlistParser();
var parseResult = parser.ParseNetlist(netlist);

// 2. Translate to SpiceSharp objects
var reader = new SpiceSharpReader();
var model = reader.Read(parseResult.FinalModel);

// 3. Run the simulation
var simulation = model.Simulations.Single();
var export = model.Exports.Find(e => e.Name == "i(V1)");
simulation.EventExportData += (sender, args) => Console.WriteLine(export.Extract());
var codes = simulation.Run(model.Circuit, -1);
codes = simulation.InvokeEvents(codes);
codes.ToArray();
```

### How It Works

| Step | API | Result |
|------|-----|--------|
| **Parse** | `SpiceNetlistParser.ParseNetlist()` | Parse-tree model of the netlist |
| **Read** | `SpiceSharpReader.Read()` | `SpiceSharpModel` with Circuit, Simulations, Exports, Measurements |
| **Simulate** | `simulation.Run()` / `InvokeEvents()` | Data available via exports and event callbacks |

## Compatibility

SpiceSharpParser handles a broad subset of **PSpice** and **LTspice** syntax. Unsupported features that may cause parse or simulation errors include `LAPLACE`, `FREQ` (analog behavioral modeling), and some advanced PSpice-only constructs.

## Supported Statements

### Dot Statements

| Statement | Description | Docs |
|-----------|-------------|------|
| `.AC` | AC small-signal frequency sweep | [Docs](src/docs/articles/ac.md) |
| `.DC` | DC sweep analysis | [Docs](src/docs/articles/dc.md) |
| `.TRAN` | Transient (time-domain) analysis | [Docs](src/docs/articles/tran.md) |
| `.OP` | DC operating point | [Docs](src/docs/articles/op.md) |
| `.NOISE` | Noise analysis | [Docs](src/docs/articles/noise.md) |
| `.SAVE` | Save signals for export | [Docs](src/docs/articles/save.md) |
| `.PRINT` | Tabular output | [Docs](src/docs/articles/print.md) |
| `.PLOT` | XY plot output | [Docs](src/docs/articles/plot.md) |
| `.MEAS` / `.MEASURE` | Post-simulation measurements | [Docs](src/docs/articles/meas.md) |
| `.PARAM` | Define parameters | [Docs](src/docs/articles/param.md) |
| `.FUNC` | Define functions | [Docs](src/docs/articles/func.md) |
| `.LET` | Define named expressions | [Docs](src/docs/articles/let.md) |
| `.SPARAM` | Scalar (eagerly evaluated) parameters | [Docs](src/docs/articles/sparam.md) |
| `.SUBCKT` / `.ENDS` | Subcircuit definition | [Docs](src/docs/articles/subckt.md) |
| `.INCLUDE` | Include external file | [Docs](src/docs/articles/include.md) |
| `.LIB` | Include library section | [Docs](src/docs/articles/lib.md) |
| `.GLOBAL` | Declare global nodes | [Docs](src/docs/articles/global.md) |
| `.STEP` | Parameter sweep | [Docs](src/docs/articles/step.md) |
| `.ST` | Parameter sweep (PSpice alias) | [Docs](src/docs/articles/st.md) |
| `.MC` | Monte Carlo analysis | [Docs](src/docs/articles/mc.md) |
| `.TEMP` | Temperature sweep | [Docs](src/docs/articles/temp.md) |
| `.OPTIONS` | Simulator options | [Docs](src/docs/articles/options.md) |
| `.IC` | Initial conditions | [Docs](src/docs/articles/ic.md) |
| `.NODESET` | DC convergence hints | [Docs](src/docs/articles/nodeset.md) |
| `.DISTRIBUTION` | Custom PDF for Monte Carlo | [Docs](src/docs/articles/distribution.md) |
| `.IF` / `.ELSE` / `.ENDIF` | Conditional netlist sections | [Docs](src/docs/articles/if.md) |
| `.APPENDMODEL` | Append model parameters | [Docs](src/docs/articles/appendmodel.md) |

### Device Statements

| Prefix | Device | Docs |
|--------|--------|------|
| **R** | Resistor | [Docs](src/docs/articles/resistor.md) |
| **C** | Capacitor | [Docs](src/docs/articles/capacitor.md) |
| **L** | Inductor | [Docs](src/docs/articles/inductor.md) |
| **K** | Mutual Inductance | [Docs](src/docs/articles/mutual-inductance.md) |
| **V** | Independent Voltage Source | [Docs](src/docs/articles/voltage-source.md) |
| **I** | Independent Current Source | [Docs](src/docs/articles/current-source.md) |
| **E** | Voltage-Controlled Voltage Source (VCVS) | [Docs](src/docs/articles/vcvs.md) |
| **F** | Current-Controlled Current Source (CCCS) | [Docs](src/docs/articles/cccs.md) |
| **G** | Voltage-Controlled Current Source (VCCS) | [Docs](src/docs/articles/vccs.md) |
| **H** | Current-Controlled Voltage Source (CCVS) | [Docs](src/docs/articles/ccvs.md) |
| **B** | Arbitrary Behavioral Source | [Docs](src/docs/articles/behavioral-source.md) |
| **D** | Diode | [Docs](src/docs/articles/diode.md) |
| **Q** | Bipolar Junction Transistor (BJT) | [Docs](src/docs/articles/bjt.md) |
| **M** | MOSFET | [Docs](src/docs/articles/mosfet.md) |
| **J** | JFET | [Docs](src/docs/articles/jfet.md) |
| **S** | Voltage Switch | [Docs](src/docs/articles/voltage-switch.md) |
| **W** | Current Switch | [Docs](src/docs/articles/current-switch.md) |
| **T** | Lossless Transmission Line | [Docs](src/docs/articles/transmission-line.md) |
| **X** | Subcircuit Instance | [Docs](src/docs/articles/subcircuit-instance.md) |

### Analog Behavioral Modeling

- `POLY(n)` — polynomial transfer functions
- `TABLE` — piecewise-linear lookup tables
- `VALUE` — arbitrary expression-based sources

## Documentation

Full documentation is available in [src/docs/articles](src/docs/articles), including a [Getting Started](src/docs/articles/intro.md) guide.

API reference: <https://spicesharp.github.io/SpiceSharpParser/api/index.html>

## License

SpiceSharpParser is licensed under the [MIT License](LICENSE).

[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FSpiceSharp%2FSpiceSharpParser.svg?type=large)](https://app.fossa.com/projects/git%2Bgithub.com%2FSpiceSharp%2FSpiceSharpParser?ref=badge_large)
