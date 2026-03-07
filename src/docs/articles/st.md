# .ST Statement

The `.ST` statement defines a parameter sweep, identical in function to `.STEP`. It is an alias supported for PSpice compatibility.

## Syntax

```
.ST [LIN] <variable> <start> <stop> <step>
.ST DEC <variable> <start> <stop> <points_per_decade>
.ST OCT <variable> <start> <stop> <points_per_octave>
.ST <variable> LIST <val1> <val2> [<val3> ...]
```

## Example

```spice
.ST LIN R1 100 10k 100
.ST PARAM gain LIST 1 10 100
```

See the [.STEP](.step) documentation for full details and examples. The syntax and behavior are identical.
