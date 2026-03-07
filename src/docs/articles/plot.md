# .PLOT Statement

The `.PLOT` statement specifies signals to capture as XY plot data during simulation. Results are stored in the `XyPlots` collection of the `SpiceSharpModel`.

## Syntax

```
.PLOT [<type>] <expr1> [<expr2> ...] [merge]
```

| Parameter | Description |
|-----------|-------------|
| `type` | Optional analysis filter: `DC`, `AC`, `TRAN`, `NOISE` |
| `expr` | Signal expressions to plot |
| `merge` | Optional keyword — combine all simulations into one plot |

## Examples

```spice
* Plot output voltage during transient
.PLOT TRAN V(OUT)

* Plot multiple signals during AC
.PLOT AC V(OUT) V(IN)

* Merge all sweep iterations into one plot
.PLOT TRAN V(OUT) merge
```

## Merge Behavior

- **Without `merge`**: A separate `XyPlot` is created for each simulation run (except OP).
- **With `merge`**: All simulation runs are combined into a single `XyPlot` with series labeled by simulation.

## Typical Usage

```spice
RC Step Response
V1 IN 0 PULSE(0 5 0 1n 1n 5m 10m)
R1 IN OUT 1k
C1 OUT 0 100n
.TRAN 1e-6 20e-3
.PLOT TRAN V(OUT) V(IN)
.END
```

## C# API

```csharp
var model = reader.Read(parseResult.FinalModel);

foreach (var plot in model.XyPlots)
{
    Console.WriteLine(plot.Name);
    foreach (var series in plot.Series)
    {
        Console.WriteLine($"  Series: {series.Name}, Points: {series.Points.Count}");
    }
}
```
