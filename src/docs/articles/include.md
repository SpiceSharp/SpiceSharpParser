# .INCLUDE Statement

The `.INCLUDE` statement includes the contents of another file into the current netlist. The included file is parsed as if its contents were inserted at the location of the `.INCLUDE` statement.

## Syntax

```
.INCLUDE "<filepath>"
.INCLUDE <filepath>
```

| Parameter | Description |
|-----------|-------------|
| `filepath` | Path to the file to include (absolute or relative to the current netlist) |

## Examples

```spice
.INCLUDE "models/diodes.lib"
.INCLUDE transistors.mod
```

## Typical Usage

```spice
Main netlist
.INCLUDE "standard_models.lib"
D1 OUT 0 1N914
V1 OUT 0 0
.DC V1 -1 1 10e-3
.SAVE I(V1)
.END
```

Where `standard_models.lib` contains:

```spice
.model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752)
```

## Notes

- Included files can contain any valid SPICE statements (models, subcircuits, parameters, etc.).
- Multiple levels of nesting are supported (an included file can itself contain `.INCLUDE`).
- File paths are resolved relative to the current working directory or the location of the including file.
