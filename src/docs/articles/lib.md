# .LIB Statement

The `.LIB` statement includes a specific library section from a file. Unlike `.INCLUDE`, which inserts the entire file, `.LIB` can selectively include only a named section.

## Syntax

```
.LIB "<filepath>" [<section_name>]
```

| Parameter | Description |
|-----------|-------------|
| `filepath` | Path to the library file |
| `section_name` | Optional — name of the section to include |

## Library File Format

Library files can define named sections:

```spice
.LIB section_name
...models and subcircuits...
.ENDL section_name
```

## Examples

```spice
* Include specific section
.LIB "device_models.lib" NPN_MODELS

* Include entire file (behaves like .INCLUDE)
.LIB "all_models.lib"
```

## Library File Example

```spice
* device_models.lib
.LIB NPN_MODELS
.model 2N3904 NPN(Is=6.734f Bf=416.4 Br=.7374)
.model 2N2222 NPN(Is=14.34f Bf=255.9 Br=6.092)
.ENDL NPN_MODELS

.LIB PNP_MODELS
.model 2N3906 PNP(Is=1.41f Bf=180 Br=4)
.ENDL PNP_MODELS
```

## Notes

- When no section name is given, `.LIB` behaves like `.INCLUDE`.
- Library files are commonly used to distribute device models separately from the main netlist.
