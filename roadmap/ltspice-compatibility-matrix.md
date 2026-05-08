---
title: LTspice Compatibility Matrix
status: P2 Scalar Expression And Waveform Baseline
scope: SpiceSharpParser + SpiceSharp
last_reviewed: 2026-05-07
---

# LTspice Compatibility Matrix

This matrix tracks LTspice-generated netlist compatibility evidence. It is intentionally conservative: a row is a support claim only when backed by a fixture or existing focused test. P2 adds conservative scalar expression, `EXP(...)` waveform, and source-option evidence; it does not claim full LTspice numeric parity.

Compatibility classes:

| Class | Meaning |
| --- | --- |
| Supported | Parser and runtime can represent the feature with tests. |
| Parser shim | LTspice spelling lowers to existing behavior. |
| Recognized no-op | LTspice display, probing, annotation, or GUI metadata is ignored with a warning in LTspice mode. |
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
| `.backanno` | Accepted as control | Default: rejected. LTspice: warning no-op. | LTspice OP smoke supported | Smoke fixture | Default: targeted error. LTspice: warning names `.backanno`. | Current parser/runtime | Generated annotation metadata is not used by SpiceSharpParser. |
| `.tf` | Accepted as control | Rejected | Not runnable | None | Targeted unsupported LTspice diagnostic | Current parser/runtime | Possible future small-signal feature. |
| `.four` | Accepted as control | Rejected | Not runnable | None | Targeted unsupported LTspice diagnostic | Current parser/runtime | Possible future post-processing feature. |
| `.net` | Accepted as control | Rejected | Not runnable | None | Targeted unsupported LTspice diagnostic | Current parser/runtime | Possible future AC post-processing feature. |
| `.ferret` | Accepted as control | Rejected | Not runnable | None | Targeted unsupported LTspice diagnostic | Current parser/runtime | Intentional unsupported candidate because it downloads external files. |
| `.loadbias` | Accepted as control | Rejected | Not runnable | None | Targeted unsupported LTspice diagnostic | Current parser/runtime | Needs portable state-format design. |
| `.savebias` | Accepted as control | Rejected | Not runnable | None | Targeted unsupported LTspice diagnostic | Current parser/runtime | Needs portable state-format design. |
| `.machine` / `.endmachine` | Accepted as controls | Rejected | Not runnable | None | Targeted unsupported LTspice diagnostic | Current parser/runtime | Engine/runtime feature if ever in scope. |
| `.tran <Tstop>` | Accepted as control | Default: rejected. LTspice: produces transient simulation with derived step. | LTspice TRAN smoke supported | Smoke fixture; no LTspice parity claim | Default: existing `.TRAN` validation error. LTspice: none expected. | Current parser/runtime | P1 compatibility policy derives `step = Tstop / 50.0` and `maxStep = step`. |
| `.tran <Tstop> UIC` | Accepted as control | LTspice mode produces transient simulation with `UseIc` set. | LTspice TRAN smoke supported | Smoke fixture; no LTspice parity claim | None expected in LTspice mode | Current parser/runtime | Uses the same derived-step policy as `.tran <Tstop>`. |
| `.tran <Tstop> startup` / `steady` / `nodiscard` / `step` | Accepted as control | Rejected in LTspice mode | Not runnable | None | Targeted LTspice `.TRAN` modifier diagnostic names the modifier | Current parser/runtime | Behavior-changing modifiers remain unsupported in P1. |
| `.options plotwinsize=<n>` and LTspice output/viewer options | Accepted | Default: assignment options rejected. LTspice: warning no-op. | LTspice OP smoke supported | Smoke fixture | Default: unsupported option error. LTspice: warning names option. | Current parser/runtime | P1 no-op list: `plotwinsize`, `plotreltol`, `plotvntol`, `plotabstol`, `numdgt`, `measdgt`, `meascplxfmt`, `baudrate`, `fastaccess`. |
| `.options cshunt=<value>` and LTspice solver-behavior options | Accepted | Rejected in LTspice mode | Not runnable | None | Targeted LTspice option error names option | Current parser/runtime | P1 behavior-changing list: `cshunt`, `gshunt`, `srcsteps`, `gminsteps`, `trtol`, `chgtol`, `pivrel`, `pivtol`, `ptrantau`. |
| Scalar aliases `arccos`, `arcsin`, `arctan`, `fabs`, `sgn`, `round`, `pwr`, `pwrs`, `hypot` | Accepted | Resolved through SpiceSharpBehavioral/defaults or parser shims | OP supported | Analytic fixture | None expected | Current parser/runtime | P2 adds `fabs` and one-argument `round` shims; other listed functions are fixture-backed existing runtime support. |
| Scalar `table(...)` / `tbl(...)` in expressions | Accepted | Runnable in expression and LTspice fixtures | OP supported | Analytic fixture | None expected | Current parser/runtime | Runtime support comes from SpiceSharpBehavioral defaults; `MathFunctions.CreateTable()` remains a stale parser-owned factory audit item. |
| Expression `**`, single/double `&` and `|`, unary `!` | Accepted | Parsed as power, boolean AND/OR, and NOT | OP/unit supported | Analytic fixture | None expected | Current parser/runtime | `^` remains existing exponent syntax; LTspice boolean XOR is deferred. |
| `uplim(...)`, `dnlim(...)`, unary `~` | Accepted in expression text | Rejected in LTspice mode | Not runnable | None | Targeted LTspice expression diagnostic names construct | Current parser/runtime | Smooth limiting and unary inversion semantics are not mapped yet. |
| `EXP(...)` source waveform | Accepted | Produces internal exponential waveform | TRAN supported | Analytic fixture | Invalid count/tau diagnostics name `EXP` | Current parser/runtime | P2 supports the six-argument form `EXP(v1 v2 td1 tau1 td2 tau2)`. |
| `PULSE(... Ncycles)` | Accepted | Rejected in LTspice mode | Not runnable | None | Targeted LTspice `PULSE` cycle-count diagnostic | Current parser/runtime | Finite-cycle waveform behavior is deferred. |
| `SINE(... Ncycles)` | Accepted | Rejected in LTspice mode | Not runnable | None | Targeted LTspice `SINE` cycle-count diagnostic | Current parser/runtime | Finite-cycle waveform behavior is deferred. |
| Independent source `tbl=(expr,x1,y1,...)` | Accepted in LTspice mode | Lowered to behavioral `table(...)` source | OP supported | Analytic fixture | Invalid form names `tbl` | Current parser/runtime | Parser shim uses the existing source/table expression path. |
| Source `wavefile=<path> chan=<n> [amplitude=<value>]` | Accepted | Produces wave-file waveform | Existing wave-file behavior | Smoke/diagnostic fixture | Missing `chan` and missing file produce targeted validation | Current parser/runtime | Channel defaults are not inferred. |
| Source `Rser`, `Cpar`, `load`, `R=<value>` | Accepted | Rejected in LTspice mode | Not runnable | None | Targeted LTspice source-option error names option | Current parser/runtime | Topology-changing source options are not synthesized. |
| `VDMOS` models | Parser/model support not claimed | Not claimed | Not runnable | None | Not yet fixture-backed | Future engine package | P3/P4 model triage item. |
| `O` / `LTRA` and `URC` lines | Parser/model support not claimed | Not claimed | Not runnable | None | Not yet fixture-backed | Future engine package | P3/P4 distributed-line triage item. |
