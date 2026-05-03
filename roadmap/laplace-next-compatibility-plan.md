---
title: LAPLACE Compatibility Status And Research Plan
status: Draft / Roadmap
scope: SpiceSharpParser
last_reviewed: 2026-05-03
---

# LAPLACE Compatibility Status And Research Plan

## Summary

SpiceSharpParser now has the main LAPLACE compatibility surface in place:

- Source-level `E` / `G` / `F` / `H` transfer sources.
- Function-style `LAPLACE(input, transfer)` in `VALUE`, `B ... V=`, and `B ... I=` expressions.
- Mixed-expression lowering through deterministic internal helpers.
- Function-style inline `M=`, `TD=`, and `DELAY=` options.
- Function-style arbitrary scalar input lowering through input helpers.
- Generated C# writer parity for the implemented reader paths.
- Focused OP, AC, and transient coverage.

This document is therefore no longer an implementation plan for inline options or arbitrary-input lowering. It is the current compatibility contract plus the remaining research and verification roadmap for broader PSpice / LTspice-style netlists.

## Current Status

| Area | Status | Evidence | Remaining work |
| --- | --- | --- | --- |
| Source-level `E` / `G` / `F` / `H` `LAPLACE` | Implemented | `LaplaceSourceParser`, `LaplaceTests` | Keep source-level input subset stable |
| Alternate source-level spellings | Implemented | `LAPLACE {input} = {H(s)}`, `LAPLACE {input} {H(s)}`, `LAPLACE = {input} {H(s)}` tests | Keep diagnostics targeted for unsupported variants |
| Source-level `M=`, `TD=`, `DELAY=` | Implemented | Reader, writer, and integration tests | No bare option syntax planned |
| Function-style direct calls | Implemented | `VALUE={LAPLACE(...)}`, `B ... V=`, `B ... I=` tests | Keep `LAPLACE` out of the scalar function table |
| Function-style mixed expressions | Implemented | Helper lowering tests and writer tests | Keep helper names deterministic and collision-checked |
| Inline call-local options | Implemented | `M=`, `TD=`, `DELAY=` reader, writer, and OP tests | Keep conflict rules documented |
| Multiple delayed function-style calls | Implemented when delay is inline | Mixed delayed transient smoke test | Source-level delay with multiple calls stays rejected |
| Arbitrary function-style input expressions | Implemented | Input helper and `V(a)-V(b)` normalization tests | Source-level arbitrary input remains deferred |
| Transient confidence | Partially verified | First-order shape tests plus delayed smoke tests | Add more response-shape and current-controlled coverage |
| Diagnostics | Partially implemented | Targeted option, input, and transfer errors | Improve degree/singularity/action hints |
| Coefficient-list syntax | Research only | No committed dialect decision | Investigate `NUM=` / `DEN=` forms and coefficient order |
| Internal-state / IC options | Research only | No confirmed runtime API contract | Investigate SpiceSharpBehavioral state initialization |
| Singular or improper transfers | Rejected | Transfer validation tests | Reconsider only with runtime proof |

## Compatibility Contract To Preserve

These forms are supported behavior and should remain stable:

```spice
E1 out 0 LAPLACE {V(in)} = {H(s)} M=2 TD=1n
G1 out 0 LAPLACE {V(in)} {H(s)}
F1 out 0 LAPLACE = {I(Vsense)} {H(s)}
H1 out 0 LAPLACE {I(Vsense)} = {H(s)}

E1 out 0 VALUE={LAPLACE(V(in), H(s))}
B1 out 0 V={LAPLACE(V(in), H(s))}
B2 out 0 I={1m + LAPLACE(V(in), H(s))}
B3 out 0 V={LAPLACE(2*V(in), H(s), M=2, TD=1n)}
B4 out 0 V={LAPLACE(V(a), H1(s), TD=1n) + LAPLACE(V(b), H2(s), DELAY=2n)}
```

The transfer expression remains a finite, proper rational polynomial in `s` with finite coefficients and non-singular DC gain.

Source-level inputs remain limited to direct probes:

```spice
V(node)
V(node1,node2)
I(source)
```

Function-style inputs accept those direct probes and arbitrary scalar input expressions. Direct probes stay fast paths. `V(a)-V(b)` is normalized to the existing differential voltage input when possible. Other scalar inputs are lowered through an internal behavioral voltage helper before the Laplace transfer.

## Design Decisions

- `LAPLACE` is a source/lowering feature, not a normal scalar function. Do not add it to the scalar math function table.
- Transfer parsing stays centralized in `LaplaceExpressionParser`.
- Reader and generated C# writer behavior stay aligned through shared lowerer semantics.
- Generated helper names stay deterministic, reserved, and collision-checked.
- Function-style helper entities may appear through low-level circuit inspection APIs.
- Source-level arbitrary input expressions are still out of scope. Use `V(a,b)` instead of source-level `V(a)-V(b)`.
- If a dialect form is recognized but intentionally deferred, emit a targeted validation error instead of falling through to generic source handling.

## Option Semantics

### Inline Options

Function-style calls support call-local options:

```spice
LAPLACE(V(in), H(s), M=2)
LAPLACE(V(in), H(s), TD=1n)
LAPLACE(V(in), H(s), DELAY=1n)
LAPLACE(V(in), H(s), M=2, TD=1n)
```

Rules:

- `M=` folds into that call's numerator coefficients.
- `TD=` and `DELAY=` are aliases for that call's `Parameters.Delay`.
- A call may specify at most one multiplier.
- A call may specify at most one delay option total.
- Option values must be finite constant expressions; delay must be non-negative.
- Unknown inline option names are validation errors.
- Inline options are supported for direct whole-expression calls and mixed-expression helper calls.

### Source-Level Options With Function-Style Calls

Source-level options remain assignment-only finite constants:

```spice
M=<expr>
TD=<expr>
DELAY=<expr>
```

Conflict rules:

| Situation | Behavior |
| --- | --- |
| One `LAPLACE(...)`, source-level `TD=` / `DELAY=`, no inline delay | Apply delay to that call |
| One `LAPLACE(...)`, inline delay plus source-level delay | Validation error |
| Multiple `LAPLACE(...)`, source-level delay | Validation error; move delay into each call |
| Direct single call, source-level `M=`, no inline `M=` | Fold source-level `M=` into numerator |
| Direct single call, inline `M=` plus source-level `M=` | Validation error |
| Mixed current-output expression, source-level `M=` | Preserve existing final current-expression scaling |
| Mixed current-output expression, inline `M=` plus source-level `M=` | Allow: inline `M=` scales the call, source-level `M=` scales the final current source |
| Mixed voltage-output expression, source-level `M=` | Validation error |

## Implemented Lowering Model

Direct whole-expression calls create the final Laplace entity directly:

| Output kind | Input kind | Entity |
| --- | --- | --- |
| Voltage | Voltage | `LaplaceVoltageControlledVoltageSource` |
| Voltage | Current | `LaplaceCurrentControlledVoltageSource` |
| Current | Voltage | `LaplaceVoltageControlledCurrentSource` |
| Current | Current | `LaplaceCurrentControlledCurrentSource` |

Mixed expressions lower each `LAPLACE(...)` call to a voltage-output helper and replace the call with `V(<helperNode>)` in the final behavioral expression. If a function-style input is not a direct probe, an input helper is inserted first:

```text
input expression -> behavioral voltage input helper -> Laplace source/helper -> final source or expression
```

Entity ordering is:

1. Input behavioral helpers.
2. Laplace entities or Laplace helper entities.
3. Final behavioral source for mixed expressions.

Generated names use these reserved patterns:

```text
__ssp_laplace_input_<sanitizedSourceName>_<index>
__ssp_laplace_input_<sanitizedSourceName>_<index>_src
__ssp_laplace_<sanitizedSourceName>_<index>
__ssp_laplace_<sanitizedSourceName>_<index>_src
```

## Remaining Roadmap

### P1: Broader Transient Verification

Goal: make transient support claims more precise and less dependent on smoke tests.

Backlog:

- Delayed first-order `E` low-pass step response with shape assertions.
- Delayed first-order `G` low-pass response through a grounded load with sign assertions.
- `F` and `H` current-controlled first-order transient smoke or shape tests.
- Function-style direct transient low-pass.
- Function-style mixed transient response with at least one helper.
- Bounded-output checks for mixed delayed expressions.

Acceptance criteria:

- Public docs distinguish analytical response checks from runtime smoke tests.
- Delayed transient claims are backed by tests, not only by construction-time validation.
- Current-controlled transient paths have at least focused coverage.

### P2: Diagnostics And Troubleshooting

Goal: make unsupported or unsafe forms easier to fix.

Backlog:

- Improper transfer: include numerator and denominator degree.
- Singular DC gain: suggest adding a low-frequency pole or finite leakage term when useful.
- Source-level arbitrary input: suggest `V(a,b)` when the rejected expression is exactly `V(a)-V(b)`.
- Duplicate options: name the duplicate option family (`M` or delay).
- Source-level delay with multiple calls: keep the existing suggestion to move delay into each `LAPLACE(...)` call.
- Unknown inline option: list supported options.

Acceptance criteria:

- Expected unsupported forms fail during reading or writing with targeted messages.
- Troubleshooting guidance in `src/docs/articles/laplace.md` matches the actual validation messages.

### P3: Coefficient-List Syntax Investigation

These are investigation items, not implementation commitments.

Possible goal:

```spice
E1 out 0 LAPLACE {V(in)} NUM={1} DEN={1 1u}
```

Questions:

- Which dialect syntax should be accepted?
- Are coefficients ascending or descending in that dialect?
- Should coefficients allow parameter expressions?
- How do coefficient lists combine with `M=`, `TD=`, and `DELAY=`?
- Should generated C# preserve coefficient-list intent or emit normalized coefficients only?

Acceptance criteria before implementation:

- A compatibility matrix row names the dialect and syntax.
- Coefficient order is proven with a small OP/AC fixture.
- Parameter-expression behavior is specified.
- Conflict rules with existing options are documented.

### P4: Internal-State And Initial-Condition Options

These are investigation items, not implementation commitments.

Possible goal:

```spice
LAPLACE(V(in), H(s), IC=...)
```

Questions:

- Does SpiceSharpBehavioral expose state initialization hooks for Laplace sources?
- Are options source-level, call-local, or both?
- What behavior is expected in OP, AC, and transient analyses?
- How do initial conditions interact with `.IC`, `.NODESET`, and `.TRAN UIC`?
- Can generated C# represent the same state setup?

Acceptance criteria before implementation:

- A runtime API spike proves the state can be initialized deterministically.
- OP, AC, and transient behavior are specified separately.
- Unsupported analysis combinations have targeted diagnostics.

### P5: Singular Or Improper Transfer Reconsideration

Currently rejected:

```spice
LAPLACE(V(in), 1/s)
LAPLACE(V(in), s)
```

Only reconsider after proving:

- OP behavior is deterministic.
- AC behavior matches user expectations.
- Transient behavior is numerically stable.
- Runtime failures are not pushed from validation time into simulation time.
- Unsupported analysis combinations fail clearly.

## Documentation Work

Current public docs already describe most implemented behavior. Keep these aligned whenever compatibility changes:

- `src/docs/articles/laplace.md`
- `src/docs/articles/behavioral-source.md`
- `.claude/skills/spicesharp-circuit-design/SKILL.md`
- `README.md`

Older roadmap files may still contain historical pre-implementation wording. Prefer marking them as historical or linking here instead of duplicating this status table.

## Evidence And Key Files

- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/LaplaceFunctionExpressionLowerer.cs`
- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/LaplaceSourceParser.cs`
- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/SourceGenerator.cs`
- `src/SpiceSharpParser/ModelWriters/CSharp/Entities/Components/SourceWriterHelper.cs`
- `src/SpiceSharpParser.Tests/ModelReaders/Spice/Readers/EntityGenerators/Components/Sources/LaplaceSourceParserTests.cs`
- `src/SpiceSharpParser.Tests/ModelWriters/LaplaceSourceWriterTests.cs`
- `src/SpiceSharpParser.IntegrationTests/AnalogBehavioralModeling/LaplaceTests.cs`
- `src/docs/articles/laplace.md`
- `src/docs/articles/behavioral-source.md`

## Acceptance Criteria

- Existing source-level and function-style LAPLACE tests continue passing.
- Inline call-local options work in direct and mixed expressions.
- Multiple delayed function-style calls work when delay is call-local.
- Source-level delay with multiple function calls still fails clearly.
- Arbitrary input expressions are supported through helper lowering for function-style calls.
- Generated C# output matches reader behavior for every implemented form.
- Public docs and the local circuit-design skill accurately distinguish supported, deferred, and rejected LAPLACE forms.
- Future compatibility claims are backed by tests, compatibility matrix rows, or explicit research notes.
