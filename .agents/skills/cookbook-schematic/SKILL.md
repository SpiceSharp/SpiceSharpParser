---
name: cookbook-schematic
description: Create, update, validate, and visually review SpiceSharp cookbook schematic.svg files with the repository-local SchemDraw renderer. Use for cookbook circuit diagrams, schematic.toml layout files, SPICE-netlist-to-SVG work, or when a generated electronics schematic must keep component and node names synchronized with a .cir file.
---

# Cookbook Schematic

Generate `schematic.svg` from a SPICE netlist and a compact TOML layout. Treat
the netlist as electrical truth and the TOML file as visual intent.

## Workflow

1. Read the circuit `.cir`, its README, and any existing `schematic.toml` or
   `schematic.svg`.
2. Read [references/layout-format.md](references/layout-format.md) before
   creating or changing layout TOML.
3. If the tool environment is absent, create it and install the local package:

   ```powershell
   python -m venv tools\cookbook-schematic\.venv
   .\tools\cookbook-schematic\.venv\Scripts\python -m pip install -e tools\cookbook-schematic
   ```

4. Validate before rendering:

   ```powershell
   .\tools\cookbook-schematic\.venv\Scripts\cookbook-schematic check <circuit>\schematic.toml
   ```

5. Render to `artifacts/` first. Inspect the SVG in the browser, or rasterize
   it and inspect the PNG when direct SVG viewing is unavailable. Check symbol
   meaning, topology, labels, whitespace, contrast, and clipping.
6. Iterate on `schematic.toml`, then render to its configured
   `schematic.svg`. Do not hand-edit the generated SVG.
7. Run the renderer tests when changing tool code:

   ```powershell
   .\tools\cookbook-schematic\.venv\Scripts\python -m unittest discover -s tools\cookbook-schematic\tests -v
   ```

## Authoring rules

- Keep every displayed component ID and electrical node tied to the parsed
  netlist. Use explicit `terminals` for multi-terminal controlled sources or
  functional blocks.
- Use a left-to-right signal flow, orthogonal wiring, short labels, and a
  shared ground rail where that matches the circuit.
- Show junction dots at meaningful branches. Junctions are visual only;
  terminal coordinates and same-node wires establish checked connectivity.
- Prefer conventional SchemDraw symbols. Use `block` only when the behavior is
  clearer as a named functional stage.
- Preserve the generated `<title>`, `<desc>`, `role="img"`, and deterministic
  text-based SVG output.
- Treat warnings about omitted netlist components as review items. Omission
  can be intentional, but it must not conceal circuit behavior important to
  the recipe.

