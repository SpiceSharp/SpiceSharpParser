# .SAVE Statement

The `.SAVE` statement specifies which signals to save during simulation. Saved signals become available as exports in the `SpiceSharpModel`.

## Syntax

```
.SAVE [<type>] <expr1> [<expr2> ...]
```

| Parameter | Description |
|-----------|-------------|
| `type` | Optional analysis filter: `OP`, `TRAN`, `AC`, `DC` |
| `expr` | Signal expressions to save |

If no parameters are given, `.SAVE` saves all node voltages and branch currents.

## Signal Expressions

| Expression | Description |
|------------|-------------|
| `V(node)` | Voltage at node (referenced to ground) |
| `V(node1, node2)` | Voltage difference between two nodes |
| `I(Vsource)` | Current through a voltage source |
| `@device[param]` | Device parameter (e.g., `@R1[resistance]`) |

## Examples

```spice
* Save specific signals
.SAVE V(OUT) V(IN) I(V1)

* Save all node voltages and branch currents
.SAVE

* Save only during transient analysis
.SAVE TRAN V(OUT)

* Save voltage difference
.SAVE V(OUT, IN)
```

## Typical Usage

```spice
RC Circuit
V1 IN 0 PULSE(0 5 0 1n 1n 500u 1m)
R1 IN OUT 1k
C1 OUT 0 1u
.TRAN 1e-6 2e-3
.SAVE V(OUT) V(IN) I(V1)
.END
```

## C# API

```csharp
var model = reader.Read(parseResult.FinalModel);

// Access saved exports
foreach (var export in model.Exports)
{
    Console.WriteLine($"{export.Name}");
}

// Find a specific export
var vout = model.Exports.Find(e => e.Name == "V(OUT)");
```
