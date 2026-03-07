# .PARAM Statement

The `.PARAM` statement defines named parameters (variables) that can be used in expressions throughout the netlist.

## Syntax

```
.PARAM <name>=<expression> [<name2>=<expression2> ...]
```

Parameters can reference other previously defined parameters, numeric literals, and built-in math functions.

## Examples

```spice
* Simple value
.PARAM vdd=3.3

* Expression referencing other params
.PARAM r_val=1k
.PARAM c_val=1/(2*3.14159*1k*r_val)

* Multiple parameters on one line
.PARAM gain=100 bandwidth=1MEG

* User-defined function syntax
.PARAM divider(a,b)=a/(a+b)
```

## Using Parameters

Parameters can be used in component values, source values, and other expressions by enclosing them in curly braces:

```spice
.PARAM res=10k
R1 IN OUT {res}
R2 OUT 0 {2*res}
V1 IN 0 {vdd}
```

## Typical Usage

```spice
Parameterized filter
.PARAM fc=1k
.PARAM r_val=1k
.PARAM c_val={1/(2*3.14159*fc*r_val)}

V1 IN 0 AC 1
R1 IN OUT {r_val}
C1 OUT 0 {c_val}
.AC DEC 10 1 1MEG
.SAVE V(OUT)
.END
```

## Scope

- Parameters defined at the top level are visible in the entire netlist.
- Parameters defined inside a `.SUBCKT` are local to that subcircuit.
- Subcircuit parameters can be overridden during instantiation.
