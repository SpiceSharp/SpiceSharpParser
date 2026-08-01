# Cookbook schematic renderer

This repository-local command renders deterministic, accessible SVG circuit
schematics from a SPICE netlist plus a compact TOML layout file. The netlist is
the source of truth for component IDs, nodes, and default values; TOML only
describes presentation.

## Setup

```powershell
python -m venv tools\cookbook-schematic\.venv
.\tools\cookbook-schematic\.venv\Scripts\python -m pip install -e tools\cookbook-schematic
```

SchemDraw is the only runtime dependency. It renders SVG headlessly, so the
normal command does not need a browser, GUI, LaTeX, or native Cairo library.

## Commands

```powershell
.\tools\cookbook-schematic\.venv\Scripts\cookbook-schematic check path\to\schematic.toml
.\tools\cookbook-schematic\.venv\Scripts\cookbook-schematic render path\to\schematic.toml
.\tools\cookbook-schematic\.venv\Scripts\cookbook-schematic symbols
```

Use `render --output artifacts\preview.svg` to review a draft without
replacing the configured SVG. `check` parses and validates without importing
SchemDraw, which makes it suitable for a lightweight CI check.

The renderer verifies that layout component IDs and node names exist in the
netlist, detects disconnected geometry for repeated nodes, requires
orthogonal wiring, reports omitted components, and writes the final SVG with
accessible title and description elements.

See
`.agents/skills/cookbook-schematic/references/layout-format.md` for the layout
schema and `.agents/skills/cookbook-schematic/SKILL.md` for the chat workflow.

## Tests

```powershell
.\tools\cookbook-schematic\.venv\Scripts\python -m unittest discover -s tools\cookbook-schematic\tests -v
```

