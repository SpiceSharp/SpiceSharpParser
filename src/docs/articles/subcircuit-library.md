# Loading Subcircuits into Programmatic SpiceSharp Circuits

`SpiceSubcircuitLibrary` bridges text netlists and circuits assembled directly
with the SpiceSharp API. It parses a library once, exposes its top-level
`.SUBCKT` definitions, and translates each requested instance into SpiceSharp
entities before adding them to the target `Circuit`.

## Library File

Library files use the same rules as `.INCLUDE` files. They do not need a title
or `.END` statement.

```spice
.SUBCKT NAND2 A B Y VDD VSS
* transistor, behavioral, or other supported implementation
...
.ENDS NAND2
```

The file may also contain root `.MODEL`, `.PARAM`, `.FUNC`, `.GLOBAL`,
`.CONNECT`, `.DISTRIBUTION`, `.OPTIONS`, and `.LET` statements. Nested
`.INCLUDE` and `.LIB` dependencies are expanded while the library is loaded.
Top-level circuit components and simulation/output controls are not copied
into programmatic circuits.

## Loading and Instantiating

```csharp
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharpParser;
using System.Collections.Generic;

var library = SpiceSubcircuitLibrary.LoadFile("digital.lib");
var circuit = new Circuit(
    new VoltageSource("VDD", "vdd", "0", 5.0));

library.AddInstance(
    circuit,
    subcircuitName: "NAND2",
    instanceName: "XU1",
    nodes: new[] { "a", "b", "y", "vdd", "0" });
```

The target remains an ordinary SpiceSharp `Circuit`, so simulations and
exports can be created entirely in C#. `AddInstance` returns the entities
actually added, including shared model entities installed by the first
instance. Later instances from the same library reuse those shared entities.

Instance names must begin with `X`, and the node count must match the
`.SUBCKT` pin count. Validation occurs before the target circuit is changed.
An existing entity with a generated instance or model name is reported as a
collision.

## Parameters

Pass SPICE expressions as a parameter dictionary:

```csharp
library.AddInstance(
    circuit,
    "RC_FILTER",
    "XFILTER",
    new[] { "input", "output", "0" },
    new Dictionary<string, string>
    {
        ["R"] = "10k",
        ["C"] = "100p",
    });
```

The dictionary overrides defaults declared after `PARAMS:` on `.SUBCKT`.
Parameterized instances are expanded when required by the existing reader.

## Native Hierarchy

Subcircuits are expanded by default. To retain SpiceSharp hierarchy for
non-parameterized instances:

```csharp
var options = new SpiceCompileOptions
{
    ExpandSubcircuits = false,
};

var library = SpiceSubcircuitLibrary.LoadFile("digital.lib", options);
library.AddInstance(circuit, "NAND2", "XU1", "a", "b", "y", "vdd", "0");
```

This adds a native SpiceSharp `Subcircuit`. Parameter overrides may still
require expansion.

## Custom Components

Install the optional `SpiceSharpParser.CustomComponents` package and configure
the same reader mappings at load time:

```csharp
using SpiceSharpParser.CustomComponents;

var options = new SpiceCompileOptions
{
    Dialect = SpiceDialect.LTspice,
    ConfigureReader = settings => settings.UseCustomComponents(),
};

var library = SpiceSubcircuitLibrary.LoadFile("digital.lib", options);
```

The resulting target contains normal SpiceSharp entities plus any custom
entity types used by the selected definitions.

## Metadata and Diagnostics

`library.Subcircuits` is a case-aware dictionary of `SpiceSubcircuitInfo`
objects containing ordered pin names and default parameter expressions.
`library.Dependencies` reports recursively loaded files, and
`library.Diagnostics` retains non-fatal load diagnostics.

Source or translation errors throw `SpiceSubcircuitLibraryException`; its
`Diagnostics` property contains the structured diagnostics. API mistakes such
as an unknown definition, wrong pin count, invalid instance name, or target
name collision use standard argument, key, or operation exceptions.
