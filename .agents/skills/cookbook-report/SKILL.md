---
name: cookbook-report
description: Audit SpiceSharp circuit cookbook recipes, SPICE directives, documentation sections, SchemDraw layout topology, accessible SVG metadata, and generated schematic freshness. Use when Codex needs to verify cookbook quality, produce a deterministic JSON health report, diagnose a stale schematic.svg, or check a recipe before committing it.
---

# Cookbook Report

Run the repository-local audit before and after changing cookbook recipes.
Treat it as a read-only quality gate; use `$cookbook-schematic` for fixes to
`schematic.toml` or `schematic.svg`.

## Workflow

1. Install both local tools into the schematic environment if needed:

   ```powershell
   .\tools\cookbook-schematic\.venv\Scripts\python -m pip install `
     -e tools\cookbook-schematic -e tools\cookbook-report
   ```

2. Run the complete audit from the repository root:

   ```powershell
   .\tools\cookbook-schematic\.venv\Scripts\cookbook-report
   ```

3. Use JSON when another tool or agent will consume the result:

   ```powershell
   .\tools\cookbook-schematic\.venv\Scripts\cookbook-report `
     --format json --output artifacts\cookbook-report.json
   ```

4. Use `--skip-freshness` only when SchemDraw is unavailable or a quick
   structural pass is sufficient. This still validates layout topology and
   SVG accessibility but reports schematic freshness as `skipped`.
5. Resolve every error. Review warnings about omitted netlist components or
   measurement-table row counts; make intentional omissions clear in the
   recipe documentation.
6. Run the cookbook simulation regression suite after the report passes:

   ```powershell
   dotnet test src\SpiceSharpParser.Tests\SpiceSharpParser.Tests.csproj `
     --filter 'FullyQualifiedName~CircuitCookbookTests'
   ```

## Exit behavior

- `0`: no errors; warnings are allowed.
- `1`: audit errors, or warnings with `--warnings-as-errors`.
- `2`: invalid command usage or a missing tool dependency.

Do not hand-edit a stale generated schematic. Invoke `$cookbook-schematic`,
review its visual output, and render from `schematic.toml`.
