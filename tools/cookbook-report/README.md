# Cookbook report

This repository-local command audits the circuit cookbook without modifying
it. It checks the documented recipe contract, validates SchemDraw layout
topology, verifies accessible SVG metadata, and detects a stale
`schematic.svg` by comparing it with a fresh deterministic render.

## Setup

Install it in the same environment as the schematic renderer:

```powershell
python -m venv tools\cookbook-schematic\.venv
.\tools\cookbook-schematic\.venv\Scripts\python -m pip install `
  -e tools\cookbook-schematic -e tools\cookbook-report
```

## Commands

Run the complete audit from the repository root:

```powershell
.\tools\cookbook-schematic\.venv\Scripts\cookbook-report
```

Write stable machine-readable output for an AI agent or CI job:

```powershell
.\tools\cookbook-schematic\.venv\Scripts\cookbook-report `
  --format json --output artifacts\cookbook-report.json
```

Use `--skip-freshness` for a faster structural check that does not import
SchemDraw. The command exits with 1 when errors are found and 2 for command or
environment failures. Warnings do not fail by default; pass
`--warnings-as-errors` to make them fail.

## Tests

```powershell
.\tools\cookbook-schematic\.venv\Scripts\python -m unittest discover `
  -s tools\cookbook-report\tests -v
```
