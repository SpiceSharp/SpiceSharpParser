---
title: LTspice Compatibility Matrix
status: P0 Evidence Baseline
scope: SpiceSharpParser + SpiceSharp
last_reviewed: 2026-05-07
---

# LTspice Compatibility Matrix

This matrix tracks LTspice-generated netlist compatibility evidence. It is intentionally conservative: a row is a support claim only when backed by a fixture or existing focused test. P0 does not introduce LTspice compatibility mode, no-op behavior, or numeric parity claims.

Compatibility classes:

| Class | Meaning |
| --- | --- |
| Supported | Parser and runtime can represent the feature with tests. |
| Parser shim | LTspice spelling lowers to existing behavior. |
| Targeted diagnostic | Known unsupported construct fails with a construct-specific validation error. |
| Parse-only evidence | Parser accepts the syntax, but reader/runtime behavior is not claimed. |
| Syntax audit gap | Fixture documents a known gap for later classification. |
| Engine required | Runtime behavior needs SpiceSharp support before runnable compatibility can be claimed. |

| Feature | Parse | Read | Run | Numeric confidence | Diagnostics | Minimum packages | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `B... V={expr}` behavioral voltage source | Accepted | Produces behavioral source | OP supported | Analytic fixture | None expected | Current parser/runtime | Covered by LTspice P0 runnable fixture. |
| `VALUE={expr}` controlled source | Accepted | Produces behavioral source | OP supported | Analytic fixture | None expected | Current parser/runtime | Covered by LTspice P0 runnable fixture. |
| Source-level `TABLE` controlled source | Accepted | Lowered to behavioral expression | OP supported | Analytic fixture | None expected | Current parser/runtime | Existing `TableTests` plus LTspice P0 runnable fixture. |
| `POLY` controlled source | Accepted | Lowered to behavioral expression | OP supported | Analytic fixture | None expected | Current parser/runtime | Existing `PolyTests` plus LTspice P0 runnable fixture. |
| Source-level `LAPLACE` | Accepted | Produces Laplace source | OP/AC/TRAN covered elsewhere | Analytic fixture | None expected | Current parser/runtime | Existing Laplace tests plus LTspice P0 runnable fixture. |
| Function-style `LAPLACE(input, transfer)` | Accepted | Lowered through Laplace source/helper path | OP/AC/TRAN covered elsewhere | Analytic fixture | None expected | Current parser/runtime | Existing Laplace tests plus LTspice P0 runnable fixture. |
| `.param` and `.func` baseline expressions | Accepted | Evaluation context functions/parameters | OP supported | Analytic fixture | None expected | Current parser/runtime | LTspice-specific scoping parity is not claimed. |
| `.tran <step> <stop>` with `.save` and `.meas` | Accepted | Produces transient simulation, exports, measurements | TRAN supported | Smoke/analytic fixture | None expected | Current parser/runtime | LTspice one-argument `.tran` is a separate gap. |
| Quoted `.include` | Accepted | Include processor inserts file content | OP supported | Smoke fixture | None expected | Current parser/runtime | Synthetic local fixture only. |
| Relative `.include` under working directory | Accepted | Include processor resolves relative path | OP supported | Smoke fixture | None expected | Current parser/runtime | Synthetic local fixture only. |
| One-argument `.lib <file>` | Accepted | Lib processor inserts file content | OP supported | Smoke fixture | None expected | Current parser/runtime | Synthetic local fixture only. |
| Selected-section `.lib <file> <section>` | Accepted | Lib processor selects section | OP supported | Smoke fixture | None expected | Current parser/runtime | Synthetic local fixture only. |
| `.backanno` | Accepted as control | Rejected | Not runnable | None | Targeted unsupported LTspice diagnostic | Current parser/runtime | P1 may convert to LTspice-mode warning no-op. |
| `.tf` | Accepted as control | Rejected | Not runnable | None | Targeted unsupported LTspice diagnostic | Current parser/runtime | Possible future small-signal feature. |
| `.four` | Accepted as control | Rejected | Not runnable | None | Targeted unsupported LTspice diagnostic | Current parser/runtime | Possible future post-processing feature. |
| `.net` | Accepted as control | Rejected | Not runnable | None | Targeted unsupported LTspice diagnostic | Current parser/runtime | Possible future AC post-processing feature. |
| `.ferret` | Accepted as control | Rejected | Not runnable | None | Targeted unsupported LTspice diagnostic | Current parser/runtime | Intentional unsupported candidate because it downloads external files. |
| `.loadbias` | Accepted as control | Rejected | Not runnable | None | Targeted unsupported LTspice diagnostic | Current parser/runtime | Needs portable state-format design. |
| `.savebias` | Accepted as control | Rejected | Not runnable | None | Targeted unsupported LTspice diagnostic | Current parser/runtime | Needs portable state-format design. |
| `.machine` / `.endmachine` | Accepted as controls | Rejected | Not runnable | None | Targeted unsupported LTspice diagnostic | Current parser/runtime | Engine/runtime feature if ever in scope. |
| `.tran <Tstop>` | Accepted as control | Rejected | Not runnable | None | Existing `.TRAN` validation error | Current parser/runtime | Syntax audit gap; P1 must decide lowering or targeted diagnostic. |
| `.tran <Tstop> startup` | Accepted as control | Not claimed | Not runnable | None | Parse-only evidence | Current parser/runtime | P1 must classify `startup`, `steady`, `nodiscard`, and `step`. |
| `.options plotwinsize=<n>` | Accepted | Rejected | Not runnable | None | Existing unsupported option validation error | Current parser/runtime | P1 should classify viewer/output no-op options. |
| `EXP(...)` source waveform | Accepted as waveform-like syntax | Rejected | Not runnable | None | Existing unsupported waveform validation error | Current parser/runtime | P2 waveform gap. |
| `PULSE(... Ncycles)` | Accepted | Rejected | Not runnable | None | Existing wrong-argument-count validation error | Current parser/runtime | P2 waveform gap. |
| Scalar `table(...)` in expressions | Accepted in existing fixtures | Runnable in existing expression tests | OP supported | Analytic fixture | None expected | Current parser/runtime | `MathFunctions.CreateTable()` remains a code audit item. |
| `VDMOS` models | Parser/model support not claimed | Not claimed | Not runnable | None | Not yet fixture-backed | Future engine package | P3/P4 model triage item. |
| `O` / `LTRA` and `URC` lines | Parser/model support not claimed | Not claimed | Not runnable | None | Not yet fixture-backed | Future engine package | P3/P4 distributed-line triage item. |
