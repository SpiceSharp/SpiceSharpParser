# .PRINT Statement

The `.PRINT` statement specifies signals to print as tabular data during simulation. Results are stored in the `Prints` collection of the `SpiceSharpModel`.

## Syntax

```
.PRINT [<type>] <expr1> [<expr2> ...]
```

| Parameter | Description |
|-----------|-------------|
| `type` | Optional analysis filter: `DC`, `AC`, `TRAN`, `OP` |
| `expr` | Signal expressions to output |

## Examples

```spice
* Print voltages for all analysis types
.PRINT V(OUT) V(IN)

* Print only during transient analysis
.PRINT TRAN V(OUT) I(V1)

* Print during AC analysis
.PRINT AC V(OUT)
```

## Output Format

Each `.PRINT` creates a `Print` object in `model.Prints`. The first column contains the independent variable (time for TRAN, frequency for AC, sweep value for DC). Subsequent columns contain the requested signals.

## Typical Usage

```spice
Resistor IV
V1 IN 0 0
R1 IN 0 10
.DC V1 -5 5 0.5
.PRINT DC V(IN) I(V1)
.END
```

## C# API

```csharp
var model = reader.Read(parseResult.FinalModel);

foreach (var print in model.Prints)
{
    Console.WriteLine(print.Name);
    foreach (var row in print.Rows)
    {
        Console.WriteLine(string.Join(", ", row.Columns.Select(c => c.Value)));
    }
}
```
