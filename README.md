# SpiceSharpParser

[![NuGet](https://img.shields.io/nuget/v/SpiceSharp-Parser.svg)](https://www.nuget.org/packages/SpiceSharp-Parser)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=SpiceSharp_SpiceSharpParser&metric=coverage)](https://sonarcloud.io/summary/new_code?id=SpiceSharp_SpiceSharpParser)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=SpiceSharp_SpiceSharpParser&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=SpiceSharp_SpiceSharpParser)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=SpiceSharp_SpiceSharpParser&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=SpiceSharp_SpiceSharpParser)

A .NET library that parses SPICE netlists and simulates them using [SpiceSharp](https://github.com/SpiceSharp/SpiceSharp). It supports a wide subset of PSpice and LTspice syntax including DC, AC, transient, noise, and operating-point analyses.

**Targets:** .NET Standard 2.0 / .NET 8.0

## Highlights

- Parse complete SPICE netlists into ordinary SpiceSharp circuits and
  simulations.
- Compile user and vendor files with structured diagnostics, recursive
  `.INCLUDE`/`.LIB` discovery, source locations, and optional structural linting.
- Load reusable `.SUBCKT` libraries from text files and instantiate selected
  definitions in circuits assembled directly with the SpiceSharp API.
- Mix parsed netlists, programmatically created SpiceSharp components, and the
  optional `SpiceSharpParser.CustomComponents` models in one circuit.
- Use twenty built-in digital and mixed-signal models: gates, Schmitt inputs,
  tri-state drivers, multiplexers, arithmetic/routing blocks, a comparator,
  an SR latch, an open-drain stage, and a functional 555 timer.
- Drive acceptance tests from netlist-native `.MEAS`, `.PRINT`, `.PLOT`, and
  `.FOUR` results instead of reimplementing every calculation in C#.

## Installation

Install the parser for ordinary netlist parsing and compilation:

```bash
dotnet add package SpiceSharp-Parser
```

Add the optional custom-components package when you need the ideal diode,
nonlinear passive, or built-in digital/555 models:

```bash
dotnet add package SpiceSharpParser.CustomComponents
```

NuGet Package Manager equivalents:

```powershell
Install-Package SpiceSharp-Parser
Install-Package SpiceSharpParser.CustomComponents
```

| Requirement | Package |
| --- | --- |
| Parse and compile SPICE netlists | `SpiceSharp-Parser` |
| Load user-authored `.SUBCKT` libraries into SpiceSharp | `SpiceSharp-Parser` |
| LTspice-style ideal diode and nonlinear `Q=`/`Flux=` passives | `SpiceSharpParser.CustomComponents` |
| Embedded digital gates, routing, bus drivers, comparator, latch, open-drain, and 555 timer | `SpiceSharpParser.CustomComponents` |

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

## Structured Compilation

For user-supplied or vendor netlists, use `SpiceCompiler` to run parsing,
preprocessing, translation, and structural linting as one operation. Expected
input failures are returned as stable diagnostics instead of being thrown:

```csharp
using System;
using SpiceSharpParser;
using SpiceSharpParser.Diagnostics;

var options = new SpiceCompileOptions
{
    Dialect = SpiceDialect.LTspice,
    WorkingDirectory = @"C:\Models",
    ContinueAfterErrors = true,
    MaximumSyntaxErrors = 25,
    RunLinter = true,
    DiagnosticPolicy = new SpiceDiagnosticPolicy
    {
        WarningsAsErrors = true,
        SuppressedCodes = { SpiceDiagnosticCodes.IgnoredConstruct },
        SeverityOverrides =
        {
            [SpiceDiagnosticCodes.NoSimulation] = DiagnosticSeverity.Info,
        },
    },
};

SpiceCompilationResult result =
    SpiceCompiler.CompileFile(@"C:\Models\amplifier.net", options);

foreach (var diagnostic in result.Diagnostics)
{
    Console.WriteLine(
        $"{diagnostic.Code}: {diagnostic.Span.FilePath}" +
        $"({diagnostic.Span.Start}): {diagnostic.Message}");
}

Console.WriteLine(
    $"Compatibility: {result.Compatibility.BlockerCount} blockers, " +
    $"{result.Compatibility.Unsupported.Count} unsupported constructs, " +
    $"{result.Compatibility.Ignored.Count} ignored metadata items.");

if (result.Model != null)
{
    // No raw error diagnostics remain; simulation can proceed.
}

// Effective diagnostics can be emitted directly to CI and editor tooling.
string json = SpiceDiagnosticFormatter.ToJson(result);
string sarif = SpiceDiagnosticFormatter.ToSarif(result);
```

`InputModel`, `ExpandedModel`, and `TranslatedModel` retain successfully
produced intermediate artifacts for inspection. `TranslatedModel` may be
partial and must not be simulated when errors are present. `Model` is exposed
only when compilation has no error diagnostics. With `ContinueAfterErrors`
enabled, lexer and parser failures in the root source and recursively loaded
`.INCLUDE`/`.LIB` files recover at the next safe statement. The whole
compilation shares one `MaximumSyntaxErrors` budget (25 by default). A
dependency with a structural failure that cannot be synchronized is removed
individually so valid sibling dependencies can still be expanded. The direct
`SpiceNetlistParser` API preserves strict first-error behavior unless recovery
is explicitly enabled in its settings.

`CompileFile` resolves relative includes from the source file's directory by
default; source-file access failures use `SSP2001`/`SSP2002` unless
`ThrowOnFileAccessError` is enabled.

`Dependencies` contains every `.INCLUDE`/`.INC` and file-backed `.LIB`
occurrence in discovery order, including missing and unreadable files. Each
`SpiceDependency` reports the requested and resolved paths, directive span,
dependency kind, resolution status, and selected library section. Diagnostics
inside loaded files expose a root-to-leaf `IncludeStack`; the same directive
spans are also available through `RelatedLocations` for IDE integrations.

`Compatibility` summarizes every diagnostic deterministically by stable code,
construct, and source file. It separates simulation blockers from unsupported
constructs, compatibility shims, ignored metadata, and numeric divergences.
Reader compatibility diagnostics use specific `SSP3001`-`SSP3008` codes;
ignored or divergent behavior uses `SSP6001`-`SSP6003`. Generic semantic reader
failures remain `SSP4000`. Every built-in diagnostic has a stable `HelpLink`
into the [diagnostic reference](docs/diagnostics.md).

`DiagnosticPolicy` creates a CI/editor view without changing compilation
safety. `AllDiagnostics` contains raw diagnostics; `Diagnostics` contains
effective non-suppressed diagnostics; and `SuppressedDiagnostics` records
non-error diagnostics hidden by policy. `Success` and `Model` are based only
on raw errors, while `PolicySuccess` also honors severity overrides and
`WarningsAsErrors`. Raw errors cannot be suppressed or downgraded.

`SpiceDiagnosticFormatter.ToJson` preserves effective and raw diagnostics,
precise spans, related locations, include stacks, suggested fixes, help links,
and compatibility classes. `ToSarif` emits deterministic SARIF 2.1.0 with
effective diagnostics for code-scanning and CI consumers.

## Workflow

Using SpiceSharpParser involves three steps:

1. **Parse** — convert a SPICE netlist string into a parse-tree model with `SpiceNetlistParser.ParseNetlist()`.
2. **Read** — translate the parse-tree into SpiceSharp simulation objects with `SpiceSharpReader.Read()`.
3. **Simulate** — run the SpiceSharp simulations and collect results via exports and events.

### Which API Should I Use?

| Goal | Recommended API |
| --- | --- |
| Compile an application- or user-supplied file with diagnostics | `SpiceCompiler.CompileFile(...)` |
| Parse an in-memory netlist with direct control over parser and reader settings | `SpiceNetlistParser` followed by `SpiceSharpReader` |
| Load selected `.SUBCKT` definitions into a programmatic SpiceSharp circuit | `SpiceSubcircuitLibrary.LoadFile(...)` or `LoadText(...)` |
| Use the packaged digital and functional 555 models | `DigitalSubcircuitLibrary.LoadBuiltIn()` |
| Parse LTspice ideal diodes or nonlinear passives | `reader.Settings.UseCustomComponents()` |
| Load a user library containing those custom components | `SpiceCompileOptions.ConfigureReader` with `UseCustomComponents()` |

These paths are composable. For example, a netlist can be parsed with custom
component mappings, then extended with normal SpiceSharp entities and built-in
digital subcircuits before its simulations are executed.

For direct reader use, `SpiceSharpReader.ReadResult()` returns a
`SpiceNetlistReadResult`. Its `Diagnostics` are immutable and source-located,
`PartialModel` preserves successfully translated objects for inspection, and
`Model` is non-null only when translation has no errors. `Read()` remains
available for compatibility and returns the partial model. Netlist input
failures become reader diagnostics; cancellation, API misuse, and unexpected
reader failures still throw.

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

### Reusable Text Subcircuit Libraries

`SpiceSubcircuitLibrary` loads include-style text files containing `.SUBCKT`
definitions and adds selected instances to an ordinary programmatic SpiceSharp
`Circuit`. Library files do not need a title or `.END` statement.

A library can contain several related definitions and shared models:

```spice
* digital.lib
.SUBCKT NAND2 A B Y VDD VSS PARAMS: VTH=0.5
RINA A VSS 1G
RINB B VSS 1G
BY Y VSS V={if(V(A,VSS)>VTH*V(VDD,VSS),if(V(B,VSS)>VTH*V(VDD,VSS),0,V(VDD,VSS)),V(VDD,VSS))}
.ENDS NAND2

.SUBCKT RC_FILTER IN OUT GND PARAMS: R=10k C=100p
R1 IN OUT {R}
C1 OUT GND {C}
.ENDS RC_FILTER
```

The pin order after each `.SUBCKT` name is the instance contract. Defaults
declared after `PARAMS:` can be overridden independently for every instance.

```csharp
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharpParser;
using System.Collections.Generic;

var options = new SpiceCompileOptions
{
    Dialect = SpiceDialect.LTspice,
    ExpandSubcircuits = true,
};

var library = SpiceSubcircuitLibrary.LoadFile("digital.lib", options);
var circuit = new Circuit(
    new VoltageSource("VDD", "vdd", "0", 5.0));

library.AddInstance(
    circuit,
    subcircuitName: "NAND2",
    instanceName: "XU1",
    nodes: new[] { "a", "b", "y", "vdd", "0" });

library.AddInstance(
    circuit,
    "RC_FILTER",
    "XFILTER",
    new[] { "y", "filtered", "0" },
    new Dictionary<string, string>
    {
        ["R"] = "10k",
        ["C"] = "100p",
    });

// Run any normal SpiceSharp simulation against circuit.
```

For generated or embedded library text, use `LoadText` instead of writing a
temporary file:

```csharp
using System.IO;

string libraryText = File.ReadAllText("digital.lib");
var library = SpiceSubcircuitLibrary.LoadText(
    libraryText,
    sourceName: "embedded/digital.lib");
```

The returned entities are ordinary SpiceSharp entities. You can connect them
to resistors, sources, semiconductor devices, custom components, or other
expanded subcircuits and then choose any normal SpiceSharp simulation.

The loader exposes pin and default-parameter metadata through
`library.Subcircuits`. Nested `.INCLUDE` files, root model definitions, nested
subcircuits, parameter overrides, case-sensitivity settings, and configured
custom reader mappings are retained. Set `ExpandSubcircuits = false` to produce
a native SpiceSharp `Subcircuit` when the instance does not require parameter
expansion. Instance names must start with `X`.

For netlists that use `SpiceSharpParser.CustomComponents`, install that optional
package and set `ConfigureReader = settings => settings.UseCustomComponents()`
on the compile options.

```csharp
using SpiceSharpParser.CustomComponents;

var customOptions = new SpiceCompileOptions
{
    Dialect = SpiceDialect.LTspice,
    ExpandSubcircuits = true,
    ConfigureReader = settings => settings.UseCustomComponents(),
};

var customLibrary = SpiceSubcircuitLibrary.LoadFile(
    "power-and-digital.lib",
    customOptions);
```

#### Library Metadata and Failure Handling

| Property | Purpose |
| --- | --- |
| `Subcircuits` | Ordered pins and default parameters for every top-level definition |
| `Dependencies` | Recursively resolved `.INCLUDE` and `.LIB` files |
| `Diagnostics` | Non-fatal parser, compatibility, and reader diagnostics |

Loading or translating an invalid library throws
`SpiceSubcircuitLibraryException` with structured diagnostics. API mistakes
such as an unknown definition, wrong node count, an instance name without the
`X` prefix, or a generated-name collision are rejected before the target
circuit is mutated.

#### What Is Copied into the Target Circuit?

- The selected subcircuit implementation and any nested definitions it uses.
- Required shared `.MODEL`, parameter, function, and global context.
- Custom component entities when the reader configuration enables them.
- No top-level analysis, output, or unrelated circuit components from the
  library file.

See [Loading Subcircuits into Programmatic SpiceSharp Circuits](src/docs/articles/subcircuit-library.md).

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

#### Digital Component and Functional 555 Library

The custom-components package ships a parameterized mixed-signal library built
on `SpiceSubcircuitLibrary`. It is available in two forms:

- `DigitalSubcircuitLibrary.LoadBuiltIn()` loads the assembly-embedded copy.
- The NuGet package includes `standard-digital.lib` under
  `contentFiles/any/any/SpiceSharpParser.CustomComponents/Digital` for direct
  `.INCLUDE` use or inspection.

The embedded facade is the simplest path for programmatic SpiceSharp circuits.
The text file is useful when the same definition must also be instantiated from
a conventional netlist.

##### Included Models

| C# API or kind | SPICE subcircuit | Ordered pins | Behavior |
| --- | --- | --- | --- |
| `DigitalGateKind.Buffer` | `DIG_BUF` | A, Y, VDD, VSS | Non-inverting unary gate |
| `DigitalGateKind.Inverter` | `DIG_NOT` | A, Y, VDD, VSS | Inverting unary gate |
| `DigitalGateKind.And2` | `DIG_AND2` | A, B, Y, VDD, VSS | Two-input AND |
| `DigitalGateKind.Nand2` | `DIG_NAND2` | A, B, Y, VDD, VSS | Two-input NAND |
| `DigitalGateKind.Or2` | `DIG_OR2` | A, B, Y, VDD, VSS | Two-input OR |
| `DigitalGateKind.Nor2` | `DIG_NOR2` | A, B, Y, VDD, VSS | Two-input NOR |
| `DigitalGateKind.Xor2` | `DIG_XOR2` | A, B, Y, VDD, VSS | Two-input XOR |
| `DigitalGateKind.Xnor2` | `DIG_XNOR2` | A, B, Y, VDD, VSS | Two-input XNOR |
| `AddSchmittBuffer` | `DIG_SCHMITT_BUF` | A, Y, VDD, VSS | Non-inverting input with hysteresis |
| `AddSchmittInverter` | `DIG_SCHMITT_NOT` | A, Y, VDD, VSS | Inverting input with hysteresis |
| `AddTriStateBuffer` | `DIG_TRI_BUF` | A, OE, Y, VDD, VSS | Active-high output enable |
| `AddTriStateInverter` | `DIG_TRI_NOT` | A, OE, Y, VDD, VSS | Active-high enable, inverted data |
| `AddMultiplexer2` | `DIG_MUX2` | D0, D1, S, Y, VDD, VSS | S=0 selects D0; S=1 selects D1 |
| `AddMultiplexer4` | `DIG_MUX4` | D0, D1, D2, D3, S0, S1, Y, VDD, VSS | S0 is the least-significant select bit |
| `AddFullAdder` | `DIG_FULL_ADDER` | A, B, CIN, SUM, COUT, VDD, VSS | One-bit sum and carry |
| `AddDecoder2To4` | `DIG_DEC2TO4` | A, B, EN, Y0, Y1, Y2, Y3, VDD, VSS | Active-high enable and one-hot outputs |
| `AddComparator` | `DIG_COMP` | P, N, Y, VDD, VSS | High when P exceeds N plus `VOFF` |
| `AddOpenDrain` | `DIG_OPEN_DRAIN` | A, Y, VDD, VSS | Active-high low-side pull-down |
| `AddSetResetLatch` | `DIG_SR_LATCH` | S, R, Q, QB, VDD, VSS | Active-high, reset-dominant state |
| `AddTimer555` | `TIMER555` | GND, TRIG, OUT, RESET, CTRL, THRESH, DISCH, VCC | Functional eight-pin 555 |

`TIMER555` uses the standard package order from pin 1 through pin 8. Instance
names still follow SPICE rules and must begin with `X`.

##### Logic at a Glance

For ordinary inputs, logic high means
`V(pin,VSS) > VTH * V(VDD,VSS)`; equality is low. A high output targets VDD
and a low output targets VSS after transport delay and through a finite output
stage. Floating inputs tend low through `RIN`.

| Family | Implemented logic |
| --- | --- |
| Buffer / inverter | `Y=A` / `Y=NOT A` |
| AND / NAND | `Y=A AND B` / its complement |
| OR / NOR | `Y=A OR B` / its complement |
| XOR / XNOR | High for unequal inputs / high for equal inputs |
| Schmitt pair | Set above `VTH_RISE`, reset below `VTH_FALL`, otherwise retain the previous state |
| Tri-state pair | Active-high OE; drive A or `NOT A` through `RON`, otherwise release through `ROFF` |
| 2:1 mux | S=0 selects D0; S=1 selects D1 |
| 4:1 mux | Select index is `2*S1 + S0` |
| Full adder | `SUM=A XOR B XOR CIN`; COUT is high when at least two inputs are high |
| 2-to-4 decoder | Active-high EN; output index is `2*B + A`; disabled outputs are all low |
| Comparator | High only when `V(P,N) > VOFF`; it has no hysteresis |
| Open drain | A=1 pulls Y low; A=0 releases Y; an external pull-up establishes high |
| SR latch | Active-high S/R; reset wins when both are high; otherwise set, reset, or hold |
| 555 timer | Priority is active-low RESET, then low TRIG, then high THRESH, then hold |

`TPD` is transport delay before the output RC response; it is not the complete
threshold-to-threshold delay under load. Tri-state Z is an electrical
approximation (`ROFF` and `COUT`), not an HDL logic value. The 555 applies
`TPD` to multiple internal stages, so it is not a single pin-to-pin delay.

The full component-by-component truth tables, Boolean equations, pin
semantics, startup rules, timing model, loading formulas, bus contention, and
limitations are in the
[Digital and 555 Subcircuit Library logic reference](src/docs/articles/digital-subcircuits.md#logic-and-electrical-conventions).

##### Pure SpiceSharp Gate Example

```csharp
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharpParser.CustomComponents.Digital;

var circuit = new Circuit(
    new VoltageSource("VDD", "vdd", "0", 5.0),
    new VoltageSource("VA", "a", "0", 5.0),
    new VoltageSource("VB", "b", "0", 5.0),
    new Resistor("RLOAD", "y", "0", 10_000.0));

var digital = DigitalSubcircuitLibrary.LoadBuiltIn();
digital.AddBinaryGate(
    circuit,
    DigitalGateKind.Nand2,
    instanceName: "XU1",
    firstInputNode: "a",
    secondInputNode: "b",
    outputNode: "y",
    positiveSupplyNode: "vdd",
    negativeSupplyNode: "0");

// circuit is still a normal SpiceSharp Circuit.
var op = new SpiceSharp.Simulations.OP("op");
var output = new SpiceSharp.Simulations.RealVoltageExport(op, "y");
foreach (int _ in op.Run(circuit))
{
    Console.WriteLine(output.Value);
}
```

`AddBuffer` and `AddInverter` are unary shortcuts. `AddGate` accepts an ordered
input collection when the kind is selected dynamically. `AddBinaryGate`
handles the remaining six `DigitalGateKind` values.

##### Input Conditioning and Tri-State Routing

Milestone A adds explicit APIs for analog/digital boundaries and data routing:

```csharp
var digital = DigitalSubcircuitLibrary.LoadBuiltIn();

digital.AddSchmittBuffer(
    circuit,
    "XINPUT",
    inputNode: "sensor",
    outputNode: "conditioned",
    positiveSupplyNode: "vdd",
    negativeSupplyNode: "0",
    new DigitalSchmittParameters
    {
        RisingThresholdRatio = 0.65,
        FallingThresholdRatio = 0.35,
    });

digital.AddMultiplexer2(
    circuit,
    "XROUTE",
    data0Node: "conditioned",
    data1Node: "alternate",
    selectNode: "select",
    outputNode: "routed",
    positiveSupplyNode: "vdd",
    negativeSupplyNode: "0");

digital.AddTriStateBuffer(
    circuit,
    "XBUS",
    inputNode: "routed",
    outputEnableNode: "enable",
    outputNode: "bus",
    positiveSupplyNode: "vdd",
    negativeSupplyNode: "0",
    new DigitalTriStateParameters
    {
        OnResistance = 50.0,
        OffResistance = 1e12,
    });
```

OE is active high. When disabled, a tri-state output is an electrical high-Z
node with `ROFF` to VSS and `COUT`; there is no separate HDL-style `Z` value.
Multiple enabled drivers therefore produce a finite contention voltage rather
than an ideal-source singularity.

The checked text-netlist equivalent is
[`circuits/digital-milestone-a/milestone-a-routing.cir`](circuits/digital-milestone-a/milestone-a-routing.cir).

##### Comparator, Latch, and Open-Drain Examples

```csharp
digital.AddComparator(
    circuit,
    "XCMP",
    positiveInputNode: "sense",
    negativeInputNode: "reference",
    outputNode: "comparison",
    positiveSupplyNode: "vdd",
    negativeSupplyNode: "0");

digital.AddSetResetLatch(
    circuit,
    "XLATCH",
    setNode: "set",
    resetNode: "reset",
    outputNode: "q",
    invertedOutputNode: "qb",
    positiveSupplyNode: "vdd",
    negativeSupplyNode: "0");

digital.AddOpenDrain(
    circuit,
    "XDRAIN",
    inputNode: "enable",
    outputNode: "openDrain",
    positiveSupplyNode: "vdd",
    negativeSupplyNode: "0");

// An external pull-up defines the released open-drain level.
circuit.Add(new Resistor("RPULL", "vdd", "openDrain", 10_000.0));
```

##### Functional 555 Astable in Pure SpiceSharp

```csharp
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharpParser.CustomComponents.Digital;

var timerCircuit = new Circuit(
    new VoltageSource("VCC", "vcc", "0", 5.0),
    new Resistor("RA", "vcc", "discharge", 10_000.0),
    new Resistor("RB", "discharge", "timing", 10_000.0),
    new Capacitor("CT", "timing", "0", 10e-9),
    new Capacitor("CCTRL", "control", "0", 10e-9),
    new Resistor("RLOAD", "out", "0", 10_000.0));

var digital = DigitalSubcircuitLibrary.LoadBuiltIn();
digital.AddTimer555(
    timerCircuit,
    instanceName: "XU1",
    groundNode: "0",
    triggerNode: "timing",
    outputNode: "out",
    resetNode: "vcc",
    controlNode: "control",
    thresholdNode: "timing",
    dischargeNode: "discharge",
    positiveSupplyNode: "vcc");
```

TRIG and THRESH share the timing-capacitor node in the astable topology. RESET
is tied to VCC. The discharge output connects to the RA/RB junction.

##### The Same 555 from a Text Netlist

```spice
Functional 555 astable
.include "standard-digital.lib"

VCC vcc 0 5
RA vcc discharge 10k
RB discharge timing 10k
CT timing 0 10n IC=0
CCTRL control 0 10n IC=3.333333333
RLOAD out 0 10k

* Standard order: GND TRIG OUT RESET CTRL THRESH DISCH VCC
XU1 0 timing out vcc control timing discharge vcc TIMER555

.OPTIONS method=gear
.TRAN 1u 1m 0 10n UIC
.MEAS TRAN period TRIG V(out) VAL=2.5 RISE=2 TARG V(out) VAL=2.5 RISE=3
.MEAS TRAN high_time TRIG V(out) VAL=2.5 RISE=2 TARG V(out) VAL=2.5 FALL=2
.MEAS TRAN low_time TRIG V(out) VAL=2.5 FALL=2 TARG V(out) VAL=2.5 RISE=3
.SAVE V(out) V(timing) V(discharge)
.END
```

Compile this form with `SpiceCompiler.CompileFile(...)`; relative includes are
resolved from the root netlist's directory. A complete checked example lives at
[`circuits/timer555/timer555-astable.cir`](circuits/timer555/timer555-astable.cir).

##### Mixing Parsed Netlists and Programmatic Digital Blocks

The built-in digital library itself needs no custom reader mapping. It can be
added after any netlist is read. Enable custom mappings only when the parsed
netlist contains the optional ideal diode or nonlinear passive syntax:

```csharp
using SpiceSharpParser;
using SpiceSharpParser.CustomComponents;
using SpiceSharpParser.CustomComponents.Digital;

var parser = new SpiceNetlistParser();
var parsed = parser.ParseNetlist(netlistText);

var reader = new SpiceSharpReader();
reader.Settings.UseCustomComponents();
var model = reader.Read(parsed.FinalModel);

var digital = DigitalSubcircuitLibrary.LoadBuiltIn();
digital.AddBuffer(
    model.Circuit,
    "XBUF",
    inputNode: "logicIn",
    outputNode: "logicOut",
    positiveSupplyNode: "vdd",
    negativeSupplyNode: "0");

// Run the simulations already created by the parsed netlist, or add a normal
// SpiceSharp simulation programmatically.
```

This is the central composition pattern: parse the parts that are convenient
to express as SPICE text, add reusable subcircuits, and then drive or inspect
the final circuit with the normal SpiceSharp API.

##### Per-Instance Electrical Parameters

Gate methods accept `DigitalGateParameters`:

```csharp
digital.AddBuffer(
    circuit,
    "XBUF",
    "input",
    "output",
    "vdd",
    "0",
    new DigitalGateParameters
    {
        LogicThresholdRatio = 0.7,
        PropagationDelay = 6e-9,
        InputResistance = 1e9,
        OutputResistance = 25.0,
        OutputCapacitance = 4e-12,
    });
```

| Gate property | SPICE name | Default | Purpose |
| --- | --- | ---: | --- |
| `LogicThresholdRatio` | `VTH` | 0.5 | Switching threshold as a fraction of VDD-VSS |
| `PropagationDelay` | `TPD` | 10 ns | Transport delay |
| `InputResistance` | `RIN` | 1 GOhm | Finite input loading to VSS |
| `OutputResistance` | `ROUT` | 50 ohms | Finite output drive |
| `OutputCapacitance` | `COUT` | 5 pF | Intrinsic output loading |

Multiplexers, the full adder, and the decoder also accept
`DigitalGateParameters`. Schmitt and tri-state models use typed parameter
objects for their additional electrical contracts:

| Schmitt property | SPICE name | Default |
| --- | --- | ---: |
| `RisingThresholdRatio` | `VTH_RISE` | 0.65 |
| `FallingThresholdRatio` | `VTH_FALL` | 0.35 |
| `PropagationDelay` | `TPD` | 10 ns |
| `InputResistance` | `RIN` | 1 GOhm |
| `OutputResistance` | `ROUT` | 50 ohms |
| `OutputCapacitance` | `COUT` | 5 pF |
| `StateResistance` | `RSTATE` | 1 kOhm |
| `HoldResistance` | `RHOLD` | 1 TOhm |
| `StateCapacitance` | `CMEM` | 1 pF |

`RisingThresholdRatio` must be greater than `FallingThresholdRatio`. Inside
that band the previous state is retained.

| Tri-state property | SPICE name | Default |
| --- | --- | ---: |
| `LogicThresholdRatio` | `VTH` | 0.5 |
| `PropagationDelay` | `TPD` | 10 ns |
| `InputResistance` | `RIN` | 1 GOhm |
| `OnResistance` | `RON` | 50 ohms |
| `OffResistance` | `ROFF` | 1 TOhm |
| `OutputCapacitance` | `COUT` | 5 pF |

`OffResistance` must be greater than `OnResistance`. Typed values are checked
before any entities are added to the target circuit.

The comparator, latch, open-drain, and timer methods accept raw SPICE parameter
overrides as `IReadOnlyDictionary<string, string>`:

```csharp
digital.AddTimer555(
    timerCircuit,
    "XU1",
    "0", "timing", "out", "vcc",
    "control", "timing", "discharge", "vcc",
    new Dictionary<string, string>
    {
        ["TPD"] = "250n",
        ["ROUT"] = "35",
        ["RDIS"] = "15",
    });
```

| Subcircuit | Default parameters |
| --- | --- |
| `DIG_SCHMITT_BUF`, `DIG_SCHMITT_NOT` | `VTH_RISE=0.65 VTH_FALL=0.35 TPD=10n RIN=1G ROUT=50 COUT=5p RSTATE=1k RHOLD=1T CMEM=1p` |
| `DIG_TRI_BUF`, `DIG_TRI_NOT` | `VTH=0.5 TPD=10n RIN=1G RON=50 ROFF=1T COUT=5p` |
| `DIG_MUX2`, `DIG_MUX4`, `DIG_FULL_ADDER`, `DIG_DEC2TO4` | `VTH=0.5 TPD=10n RIN=1G ROUT=50 COUT=5p` |
| `DIG_COMP` | `VOFF=0 TPD=10n RIN=1G ROUT=50 COUT=5p` |
| `DIG_OPEN_DRAIN` | `VTH=0.5 RIN=1G RON=10 ROFF=1T COUT=5p` |
| `DIG_SR_LATCH` | `VTH=0.5 TPD=10n RIN=1G ROUT=50 COUT=5p RSTATE=1k RHOLD=1T CMEM=1p` |
| `TIMER555` | `TPD=100n RIN=1G ROUT=20 COUT=2n RDIS=10 ROFF=1T RDIV=5k` |

##### 555 Behavior and Verified Timing

The functional timer contains a three-resistor divider, two comparators, a
reset-dominant SR latch, a finite-impedance output, and an open-drain discharge
stage. Its control priority is:

1. Active-low RESET forces OUT low and enables DISCH.
2. Otherwise, TRIG below one-third VCC sets the latch.
3. Otherwise, THRESH above two-thirds VCC resets the latch.
4. Otherwise, the latch retains its previous state.

For the usual astable connection:

```text
t_high = 0.693 * (RA + RB) * C
t_low  = 0.693 * RB * C
period = 0.693 * (RA + 2*RB) * C
```

The checked 5 V example uses RA=10 kohm, RB=10 kohm, and C=10 nF:

| Measurement | Ideal | SpiceSharp result | Error |
| --- | ---: | ---: | ---: |
| High time | 138.600 us | 139.107 us | +0.37% |
| Low time | 69.300 us | 69.781 us | +0.69% |
| Period | 207.900 us | 208.887 us | +0.47% |
| Frequency | 4.810 kHz | 4.787 kHz | -0.47% |

Switching simulations need a maximum timestep below the modeled propagation
delay and output edge time. The example uses Gear integration and a 10 ns
`tmax`. When using `UIC`, initialize the control bypass near two-thirds VCC if
the startup ramp is not itself under test.

##### Model Scope

These are functional mixed-signal models for logic, timing, topology, and
control experiments. They do not model unknown logic states, metastability,
protection structures, detailed supply current, output-current limits,
temperature drift, noise, or family/vendor-specific input and output curves.
`TIMER555` is not a transistor-level model of a particular NE555 or TLC555.
Use a vendor macro-model when those effects are acceptance criteria.

More detail:

- [Digital and 555 Subcircuit Library](src/docs/articles/digital-subcircuits.md)
- [Digital component roadmap](roadmap/digital-component-library-roadmap.md)
- [Milestone A requirements](circuits/digital-milestone-a/requirements.md)
- [Milestone A design notes](circuits/digital-milestone-a/documentation.md)
- [Milestone A verification results](circuits/digital-milestone-a/results.md)
- [555 requirements](circuits/timer555/requirements.md)
- [555 design notes](circuits/timer555/documentation.md)
- [555 measured results](circuits/timer555/results.md)

### Behavioral Modeling

`VALUE={expr}`, `TABLE={expr}`, `POLY(n)`, `B` sources, source-level `E` / `G` / `F` / `H` `LAPLACE` transfer functions, function-style `LAPLACE(input, transfer)` in behavioral expressions, and a full set of built-in math functions including LTspice-style `uplim(...)` and `dnlim(...)` smooth limiters. `LAPLACE` supports voltage-controlled and current-controlled forms with rational polynomials in `s`, including finite constant `M=`, `TD=`, and `DELAY=` options. Function-style calls also support call-local options, mixed-expression helper lowering, and arbitrary scalar input expressions.

## Documentation

Start with the [documentation index](src/docs/index.md) or go directly to a
workflow-specific guide:

| Topic | Guide |
| --- | --- |
| Parser introduction and first simulation | [Introduction](src/docs/articles/intro.md) |
| Loading text subcircuits into C#-built circuits | [Programmatic Subcircuit Libraries](src/docs/articles/subcircuit-library.md) |
| Digital logic, truth tables, routing, buses, and functional 555 | [Digital and 555 Subcircuit Library](src/docs/articles/digital-subcircuits.md) |
| Stable compiler diagnostic codes | [Diagnostic Reference](docs/diagnostics.md) |
| LTspice compatibility status | [Compatibility Matrix](roadmap/ltspice-compatibility-matrix.md) |
| Custom ideal diode | [LTspice-Style Ideal Diode](src/docs/articles/ideal-diode.md) |
| Nonlinear capacitors and inductors | [LTspice-Style Nonlinear Passives](src/docs/articles/nonlinear-passives.md) |
| `.MEAS` syntax and structured results | [Measurements](src/docs/articles/meas.md) |

Repository-level circuit artifacts provide executable examples alongside the
API documentation:

- [Functional 555 astable netlist](circuits/timer555/timer555-astable.cir)
- [Digital Milestone A routing netlist](circuits/digital-milestone-a/milestone-a-routing.cir)
- [Digital component roadmap](roadmap/digital-component-library-roadmap.md)
- [555 quantitative requirements](circuits/timer555/requirements.md)
- [555 design and modeling notes](circuits/timer555/documentation.md)
- [555 measured results and regression evidence](circuits/timer555/results.md)

## Building from Source

```bash
git clone https://github.com/SpiceSharp/SpiceSharpParser.git
cd SpiceSharpParser

dotnet restore src/SpiceSharp-Parser.sln
dotnet build src/SpiceSharp-Parser.sln

dotnet test src/SpiceSharpParser.Tests/SpiceSharpParser.Tests.csproj
dotnet test src/SpiceSharpParser.IntegrationTests/SpiceSharpParser.IntegrationTests.csproj
```

Run only the digital and 555 regression tests while developing those models:

```bash
dotnet test src/SpiceSharpParser.Tests/SpiceSharpParser.Tests.csproj \
  --filter "FullyQualifiedName~DigitalSubcircuitLibraryTests"
```

Build and inspect the optional custom-components package:

```bash
dotnet build src/SpiceSharpParser.CustomComponents/SpiceSharpParser.CustomComponents.csproj \
  --configuration Release
```

That project targets both .NET Standard 2.0 and .NET 8.0. Its generated NuGet
archive includes the compiled assemblies, the embedded digital library, and a
content-file copy of `standard-digital.lib`.

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
