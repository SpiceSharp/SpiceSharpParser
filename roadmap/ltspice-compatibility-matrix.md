---
title: LTspice Compatibility Matrix
status: P3 Baseline + .FOUR Support Refresh + Custom Passives
scope: SpiceSharpParser + SpiceSharp
last_reviewed: 2026-06-13
---

# LTspice Compatibility Matrix

This matrix tracks LTspice-generated netlist compatibility evidence. It is intentionally conservative: a row is a support claim only when backed by a fixture or existing focused test. P3 adds parser-first model and instance parameter tolerance; it does not claim numeric parity for unsupported LTspice compact models or topology-changing instance parasitics.

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
| `.four` | Accepted as control | Creates transient Fourier post-processing for `.TRAN` | TRAN supported | Analytic integration/unit evidence | Targeted diagnostics for missing `.TRAN`, missing signal, invalid frequency, too-short transient, and missing signal export | Current parser/runtime | Results are exposed through `model.FourierAnalyses`; one result is produced per signal and per stepped transient simulation. Evidence: `FourTests`, `FourierAnalysisCalculatorTests`, README `FourierAnalyses`. |
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
| R/C model `tc=a[,b]` | Accepted | Lowered to `tc1` / `tc2` | Existing R/C model behavior | Read fixture | None expected in LTspice mode | Current parser/runtime | Parser shim only; coefficients beyond two are rejected. |
| LTspice metadata/rating parameters `mfg`, `manufacturer`, `pn`, `part`, `desc`, `description`, `V`, `Irms`, `Ipk` | Accepted | Warning no-op in LTspice mode | No runtime effect | Diagnostic fixture | Default: existing parameter error. LTspice: warning names parameter. | Current parser/runtime | BOM/layout metadata is not used by SpiceSharpParser. |
| R/C/L instance parasitics `Rser`, `Rpar`, `Cpar`, `Lser`, `RLshunt` | Accepted | Rejected in LTspice mode | Not runnable | None | Targeted LTspice instance-parameter error names parameter | Current parser/runtime | Topology-changing passive parasitics are not synthesized. |
| Capacitor `Q=<expr>` and inductor `Flux=<expr>` | Accepted | Core LTspice mode: targeted diagnostic. With `UseCustomComponents()`: produces `NonlinearCapacitor` / `NonlinearInductor`. | Custom components: TRAN and AC supported with operating-point incremental capacitance/inductance. | Focused parser/TRAN/AC fixtures | Core LTspice mode still emits targeted charge/flux diagnostic when custom mappings are not enabled | `SpiceSharpParser.CustomComponents` | `Q` uses `x` as terminal voltage and supports `IC`, `M`, and `N`; `Flux` uses `x` as branch current. |
| LTspice ideal diode parameters `Ron`, `Roff`, `Vfwd`, `Vrev`, `Rrev`, `Ilimit`, `RevIlimit`, `Epsilon`, `RevEpsilon` | Accepted | Rejected in LTspice mode | Not runnable | None | Targeted LTspice ideal-diode diagnostic names parameter | Current parser/runtime | These select LTspice's idealized diode behavior, not Berkeley diode parameters. |
| Switch aliases `von`/`voff` and `ion`/`ioff` | Accepted | Lowered to `vt`/`vh` and `it`/`ih` | Existing switch model behavior | Read fixture | Missing pair or mixed native/alias forms are targeted errors | Current parser/runtime | Midpoint and half-span lowering only; no LTspice numeric parity claim. |
| Switch `Lser`, `Vser`, `Ilimit` | Accepted | Rejected in LTspice mode | Not runnable | None | Targeted LTspice switch diagnostic names parameter | Current parser/runtime | Series elements and current limiting are not synthesized. |
| MOS model levels outside 1, 2, and 3 | Accepted | Rejected in LTspice mode | Not runnable | None | Targeted LTspice MOS-level diagnostic | Current parser/runtime | Legacy MOS levels 1-3 remain the supported parser/runtime subset. |
| Three-terminal LTspice MOS / power-MOS syntax | Accepted | Rejected in LTspice mode | Not runnable | None | Targeted LTspice three-terminal MOS diagnostic | Current parser/runtime | Treated as VDMOS/power-MOS engine-required syntax. |
| `VDMOS` models | Accepted as model syntax | Rejected in LTspice mode | Not runnable | None | Targeted engine-required diagnostic names `VDMOS` | Future engine package | Parser recognizes the gap; no runtime support is claimed. |
| `O` / `LTRA` and `U` / `URC` lines | Accepted as component/model syntax | Rejected in LTspice mode | Not runnable | None | Targeted engine-required diagnostics name line family | Future engine package | Parser recognizes the gap; lossy/distributed-line runtime support is not claimed. |
