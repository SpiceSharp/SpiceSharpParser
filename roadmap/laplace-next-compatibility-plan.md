---
title: LAPLACE Next Compatibility Plan
status: Draft / Roadmap
scope: SpiceSharpParser
last_reviewed: 2026-05-02
---

# LAPLACE Next Compatibility Plan

## Summary

SpiceSharpParser already has the core LAPLACE feature: source-level `E` / `G` / `F` / `H` transfer sources, function-style `LAPLACE(input, transfer)` in behavioral expressions, helper lowering for mixed expressions, generated C# writer parity, and focused OP / AC / transient coverage.

The next phase should make that support easier to use with real PSpice / LTspice-style netlists. The recommended roadmap is:

| Priority | Feature | User value | Implementation risk |
| --- | --- | --- | --- |
| P1 | Inline function options | Enables `LAPLACE(V(in), H(s), TD=1n)` and per-call scaling | Medium |
| P2 | Multiple delayed function calls | Removes current source-level delay limitation for mixed expressions | Low after P1 |
| P3 | Arbitrary input-expression lowering | Accepts common forms like `LAPLACE(V(a)-V(b), H(s))` | Medium-high |
| P4 | Broader transient verification and diagnostics | Makes support claims clearer and failures easier to fix | Low |
| P5 | Coefficient-list / state-option investigation | Future dialect compatibility | Unknown |

## Baseline To Preserve

Keep all current supported forms working exactly as they do today:

```spice
E1 out 0 LAPLACE {V(in)} = {H(s)} M=2 TD=1n
G1 out 0 LAPLACE {V(in)} {H(s)}
F1 out 0 LAPLACE = {I(Vsense)} {H(s)}
H1 out 0 LAPLACE {I(Vsense)} = {H(s)}

E1 out 0 VALUE={LAPLACE(V(in), H(s))}
B1 out 0 V={LAPLACE(V(in), H(s))}
B2 out 0 I={1m + LAPLACE(V(in), H(s))}
```

The transfer expression remains a finite, proper rational polynomial in `s` with non-singular DC gain. Existing direct probe inputs remain fast paths:

```spice
V(node)
V(node1,node2)
I(source)
```

Source-level options remain assignment-only finite constants:

```spice
M=<expr>
TD=<expr>
DELAY=<expr>
```

## Shared Design Decisions

- Inline function options are call-local. They affect only the `LAPLACE(...)` call that contains them.
- Inline options are parsed from expression AST arguments. In this parser, `TD=1n` inside a function call should be handled as an equality `BinaryOperatorNode` whose left side is a plain variable name and whose right side is a constant-evaluable expression.
- Do not add `LAPLACE` to the normal scalar function table. It remains a source/lowering feature because its runtime behavior is analysis-dependent.
- Keep transfer parsing centralized in `LaplaceExpressionParser`; do not add a second coefficient builder.
- Keep reader and writer behavior aligned by reusing the same lowerer semantics wherever practical.
- Generated helper names stay deterministic, reserved, and collision-checked.
- If a requested dialect form is recognized but intentionally deferred, emit a targeted validation error instead of falling through to generic source handling.

## Option Semantics

### Inline Options

Support these call-local options:

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

Keep current source-level option behavior, with explicit conflict rules:

| Situation | Behavior |
| --- | --- |
| One `LAPLACE(...)`, source-level `TD=` / `DELAY=`, no inline delay | Apply delay to that call |
| One `LAPLACE(...)`, inline delay plus source-level delay | Validation error |
| Multiple `LAPLACE(...)`, source-level delay | Validation error |
| Direct single call, source-level `M=`, no inline `M=` | Fold source-level `M=` into numerator |
| Direct single call, inline `M=` plus source-level `M=` | Validation error |
| Mixed current-output expression, source-level `M=` | Preserve existing final current-expression scaling |
| Mixed current-output expression, inline `M=` plus source-level `M=` | Allow: inline `M=` scales the call, source-level `M=` scales the final current source |
| Mixed voltage-output expression, source-level `M=` | Keep current validation error |

## Phase 1: Inline Function Options

Goal: support call-local `M=`, `TD=`, and `DELAY=` in function-style LAPLACE without changing source-level syntax.

Implementation:

- Extend `LaplaceFunctionExpressionLowerer.TryParseCall(...)` to accept two or more arguments.
- Treat argument 0 as input and argument 1 as transfer.
- Parse arguments 2..N as option AST nodes.
- Add a small option parser that accepts equality nodes with a variable-name left side: `M`, `TD`, or `DELAY`.
- Reuse existing finite constant option evaluation and error messages where possible.
- Store parsed options with each `ParsedLaplaceCall`.
- Apply call-local multiplier before creating `LaplaceSourceDefinition`.
- Apply call-local delay to the direct entity or generated helper entity.
- Preserve the existing "exactly two arguments" error only for calls with fewer than two arguments; replace it with a clearer option error for invalid third and later arguments.

Reader tests:

- Direct `B ... V={LAPLACE(V(in), 1/(1+s), M=2)}` has OP gain `2`.
- Direct `B ... I={LAPLACE(V(in), gm/(1+s), M=2)}` doubles load current.
- Mixed expression with two calls applies each call's own `M=`.
- Direct call with `TD=1n` sets `Parameters.Delay`.
- `TD=1n` and `DELAY=2n` in the same call fails.
- Unknown inline option, such as `FOO=1`, fails clearly.
- Inline option without assignment syntax fails clearly.
- Source-level delay plus inline delay fails clearly.

Writer tests:

- Direct calls emit scaled numerator and delay.
- Mixed calls emit helper entities with each helper's own numerator and delay.
- Invalid inline options emit the existing writer comment-style error.

Docs:

- Update `src/docs/articles/laplace.md` and `src/docs/articles/behavioral-source.md`.
- Remove the "inline function options are not supported" limitation.
- Add examples for direct and mixed call-local options.

## Phase 2: Multiple Delayed Function Calls

Goal: allow mixed expressions where each `LAPLACE(...)` call owns its own delay.

Supported:

```spice
B1 out 0 V={
  LAPLACE(V(a), 1/(1+s*t1), TD=1n)
  + LAPLACE(V(b), 1/(1+s*t2), TD=2n)
}
```

Implementation:

- This mostly falls out of Phase 1 if each parsed call carries its own delay.
- Keep the current rejection for source-level `TD=` / `DELAY=` when more than one call is present.
- Lower each delayed call to a distinct helper with its own `Parameters.Delay`.
- Preserve traversal order for helper naming and expression replacement.

Tests:

- Two delayed calls create two helpers with distinct delays.
- One delayed call plus one undelayed call succeeds.
- Source-level delay with two calls still fails.
- Direct one-call source-level delay continues to work.
- Add a transient smoke test that runs the delayed mixed expression without runtime exceptions and produces bounded output.

## Phase 3: Arbitrary Input-Expression Lowering

Goal: accept common function-style input expressions that are not direct probes.

Supported examples:

```spice
LAPLACE(V(a)-V(b), H(s))
LAPLACE(2*V(in), H(s))
LAPLACE(V(a)+I(Vsense)*rscale, H(s))
```

Implementation strategy:

- Keep existing direct probe parsing as the preferred fast path.
- Normalize simple `V(a)-V(b)` to the existing differential voltage input when possible.
- For other non-probe input expressions, generate an internal behavioral voltage helper that evaluates the input expression.
- Feed the helper voltage into the Laplace transfer using a normal voltage-controlled Laplace entity.

Lowering shape:

```text
input expression -> behavioral voltage input helper -> Laplace source/helper -> final source or expression
```

Entity ordering:

1. Input behavioral helpers.
2. Laplace entities or Laplace helper entities.
3. Final behavioral source for mixed expressions.

Scope:

- Start with function-style `LAPLACE(...)` only.
- Do not broaden source-level `E/G/F/H ... LAPLACE {input}` until this path is stable.
- Do not support nested `LAPLACE(...)` inside the input expression.
- Keep transfer-expression validation unchanged.

Naming:

```text
__ssp_laplace_input_<sanitizedSourceName>_<index>
__ssp_laplace_input_<sanitizedSourceName>_<index>_src
__ssp_laplace_<sanitizedSourceName>_<index>
__ssp_laplace_<sanitizedSourceName>_<index>_src
```

Validation:

- Input expression must parse through the existing behavioral expression parser.
- Input expression must not contain nested `LAPLACE(...)`.
- Generated helper names must be collision-checked against existing entities and other generated helpers.
- If arbitrary input support is not enabled for a path, suggest `V(a,b)` when the rejected expression is exactly `V(a)-V(b)`.

Tests:

- `LAPLACE(V(a)-V(b), H(s))` matches `LAPLACE(V(a,b), H(s))`.
- `LAPLACE(2*V(in), H(s))` doubles OP gain.
- Input helper with a parameterized expression works.
- Mixed expression creates input helper, Laplace helper, and final behavioral source in order.
- Subcircuit smoke test verifies helper names remain scoped and collision-safe.
- Writer emits the same helper sequence as the reader path.

## Phase 4: Verification And Diagnostics

Goal: make supported behavior easier to trust and unsupported behavior easier to fix.

Transient verification:

- Delayed first-order `E` low-pass step response.
- Delayed first-order `G` low-pass response through a grounded load.
- `F` and `H` current-controlled first-order transient smoke tests.
- Function-style direct transient low-pass.
- Function-style mixed transient smoke test with at least one helper.

Diagnostics:

- Improper transfer: include numerator and denominator degree.
- Singular DC gain: suggest adding a low-frequency pole or finite leakage term.
- Unsupported arbitrary input: suggest `V(a,b)` for `V(a)-V(b)`.
- Duplicate options: name the duplicate option family (`M` or delay).
- Source-level delay with multiple calls: suggest moving delay into each `LAPLACE(...)` call.

Docs:

- Update `src/docs/articles/laplace.md` limitations with verified transient paths.
- Add a troubleshooting table for `1/s`, `s`, arbitrary inputs, duplicate options, and source-level delay with multiple calls.
- Update `.claude/skills/spicesharp-circuit-design/SKILL.md` after public docs are correct.

## Phase 5: Compatibility Investigation

These are investigation items, not implementation commitments.

### Coefficient-List Syntax

Possible goal:

```spice
E1 out 0 LAPLACE {V(in)} NUM={1} DEN={1 1u}
```

Questions:

- Which dialect syntax should be accepted?
- Are coefficients ascending or descending in that dialect?
- Should coefficients allow parameter expressions?
- How do coefficient lists combine with `M=`, `TD=`, and `DELAY=`?

### Internal-State Options

Possible goal:

```spice
LAPLACE(V(in), H(s), IC=...)
```

Questions:

- Does SpiceSharpBehavioral expose state initialization hooks for Laplace sources?
- Are options source-level, call-local, or both?
- What behavior is expected in OP, AC, and transient analyses?

### Singular Or Improper Transfers

Currently rejected:

```spice
LAPLACE(V(in), 1/s)
LAPLACE(V(in), s)
```

Only reconsider after proving:

- OP behavior is deterministic.
- AC behavior matches user expectations.
- Transient behavior is numerically stable.
- Unsupported analysis combinations fail clearly.

## Key Files

- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/LaplaceFunctionExpressionLowerer.cs`
- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/LaplaceSourceParser.cs`
- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/SourceGenerator.cs`
- `src/SpiceSharpParser/ModelWriters/CSharp/Entities/Components/SourceWriterHelper.cs`
- `src/SpiceSharpParser.Tests/ModelReaders/Spice/Readers/EntityGenerators/Components/Sources/LaplaceSourceParserTests.cs`
- `src/SpiceSharpParser.Tests/ModelWriters/LaplaceSourceWriterTests.cs`
- `src/SpiceSharpParser.IntegrationTests/AnalogBehavioralModeling/LaplaceTests.cs`
- `src/docs/articles/laplace.md`
- `src/docs/articles/behavioral-source.md`
- `.claude/skills/spicesharp-circuit-design/SKILL.md`

## Acceptance Criteria

- Existing source-level and function-style LAPLACE tests continue passing.
- Inline call-local options work in direct and mixed expressions.
- Multiple delayed function-style calls work when delay is call-local.
- Source-level delay with multiple function calls still fails clearly.
- Arbitrary input expressions are supported through helper lowering for function-style calls.
- Generated C# output matches reader behavior for every newly supported form.
- Public docs and the local circuit-design skill accurately distinguish supported, deferred, and rejected LAPLACE forms.

