# Cookbook schematic layout format

The renderer treats the SPICE netlist as the electrical source of truth and a
versioned TOML file as presentation data. Paths in `[schematic]` are resolved
relative to the TOML file.

## Minimal example

```toml
version = 1

[schematic]
netlist = "example.cir"
output = "schematic.svg"
title = "RC low-pass filter"
description = "R1 feeds output; C1 shunts output to ground."
subtitle = "First-order low-pass"

[[elements]]
id = "R1"
start = [0, 3]
end = [3, 3]

[[elements]]
id = "C1"
start = [3, 3]
end = [3, 0]
label_location = "left"
value_location = "right"

[[node_labels]]
node = "output"
at = [3, 3.5]

[[symbols]]
kind = "ground"
node = "0"
at = [3, 0]
```

## Tables

### `[schematic]`

- `netlist` (required): SPICE file to parse.
- `description` (required): concise accessible SVG description.
- `output` (default `schematic.svg`): generated SVG path.
- `title` (default netlist title): visible and accessible title.
- `subtitle` (optional): visible explanatory line.

### `[theme]`

Optional keys are `line_color`, `text_color`, `secondary_text_color`,
`signal_color`, `accent_color`, `block_fill`, `background`, `font_family`,
`font_size`, `line_width`, `unit`, and `padding`.

### `[[elements]]`

Each element must reference a real netlist component using `id`. Required
geometry is `start = [x, y]` and `end = [x, y]`; it must be horizontal or
vertical. The first and second coordinates represent the first and second
terminal in `terminals`. Two-terminal parts infer those names from the
netlist. Multi-terminal parts require an explicit two-node projection such as
`terminals = ["pole1", "buffer1"]`.

Optional fields:

- `kind`: renderer symbol override. Run `cookbook-schematic symbols` for the
  authoritative list.
- `label` and `value`: displayed text; both default from the netlist.
- `label_location`, `value_location`: `top`, `bottom`, `left`, `right`, or
  `center`.
- `color`, `fill`: CSS colors.
- `reverse`, `flip`: SchemDraw orientation controls.

Common `kind` values include `bjt-npn`, `bjt-pnp`, `resistor`,
`resistor-iec`, `capacitor`,
`capacitor-polarized`, `inductor`, `diode`, `zener`, `schottky`, `led`,
`source-v`, `source-i`, `source-pulse`, `source-sin`, `buffer`, and `block`.

### `[[wires]]`

Use `node` plus two or more orthogonal `points`. A wire can contain several
right-angle segments. `color` and `arrow = "start" | "end" | "both"` are
optional. Wires participate in connectivity validation.

### `[[junctions]]`, `[[node_labels]]`, `[[symbols]]`, `[[texts]]`

- A junction has `node` and `at`; it is visual and does not create electrical
  connectivity.
- A node label has `node`, `at`, optional `location`, and optional display
  `text`.
- A symbol has `kind`, `at`, optional `node`, and optional `label`. Supported
  kinds are `ground`, `ground-signal`, `port`, and `test-point`.
- Free text has `text`, `at`, and optional `location`, `size`, and `color`.

## Validation behavior

`check` rejects unknown component IDs, unknown nodes, duplicate displayed
components, invalid symbol kinds, diagonal wires, and geometrically
disconnected occurrences of the same electrical node. It warns when a netlist
component is omitted. SPICE `X` subcircuit pins are intentionally rejected
because their count cannot be inferred safely without model resolution.

Junctions and labels never repair topology. Align terminals at the same point
or join them with a `[[wires]]` entry carrying the same node name.

