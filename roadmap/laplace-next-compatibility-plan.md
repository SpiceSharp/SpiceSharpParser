---
title: LAPLACE Next Compatibility Plan
status: Draft / Roadmap
scope: SpiceSharpParser
last_reviewed: 2026-05-02
---

# LAPLACE Next Compatibility Plan

## Summary

SpiceSharpParser already supports the core LAPLACE feature set: source-level `E` / `G` / `F` / `H` transfer sources, function-style `LAPLACE(input, transfer)` in behavioral expressions, mixed-expression helper lowering, generated C# writer parity, and focused OP / AC / transient coverage.

The next roadmap phase should improve compatibility with common PSpice / LTspice-style netlists and reduce avoidable user rewrites. The recommended order is:

1. Add inline function-call options.
2. Support multiple delayed function-style calls.
3. Accept simple arbitrary input expressions through helper lowering.
4. Broaden transient verification and diagnostics.
5. Investigate coefficient-list and state-option syntax only after the above pieces are stable.

## Current Baseline

Implemented behavior to preserve:

- Source-level forms:

  ```spice
  E1 out 0 LAPLACE {V(in)} = {H(s)} M=2 TD=1n
  G1 out 0 LAPLACE {V(in)} {H(s)}
  F1 out 0 LAPLACE = {I(Vsense)} {H(s)}
  H1 out 0 LAPLACE {I(Vsense)} = {H(s)}
  ```

- Function-style forms:

  ```spice
  E1 out 0 VALUE={LAPLACE(V(in), H(s))}
  B1 out 0 V={LAPLACE(V(in), H(s))}
  B2 out 0 I={1m + LAPLACE(V(in), H(s))}
  ```

- Supported inputs:

  ```spice
  V(node)
  V(node1,node2)
  I(source)
  ```

- Supported transfer expressions are finite, proper rational polynomials in `s` with non-singular DC gain.
- Source-level options `M=`, `TD=`, and `DELAY=` are assignment-only, finite constants.
- Function-style source-level `TD=` / `DELAY=` requires exactly one `LAPLACE(...)` call today.

## Phase 1: Inline Function Options

Add support for call-local options inside function-style LAPLACE:

```spice
LAPLACE(V(in), H(s), M=2)
LAPLACE(V(in), H(s), TD=1n)
LAPLACE(V(in), H(s), DELAY=1n)
LAPLACE(V(in), H(s), M=2, TD=1n)
```

Implementation notes:

- Extend `LaplaceFunctionExpressionLowerer.TryParseCall(...)` to accept two or more arguments.
- Keep the first argument as input and the second argument as transfer.
- Treat remaining arguments as option assignments only.
- Reuse existing finite constant validation for `M=`, `TD=`, and `DELAY=`.
- Fold call-local `M=` into that call's numerator coefficients.
- Apply call-local delay to that direct source or helper source.
- Reject duplicate multiplier or duplicate delay options per call.
- Reject unsupported option names with a targeted validation error.
- If both call-local and source-level options are present, reject overlapping options rather than guessing precedence.

Reader tests:

- Direct voltage-output call with `M=2` scales OP gain.
- Direct current-output call with `M=2` scales output current.
- Mixed expression with two calls can give each call its own `M=`.
- Direct call with `TD=1n` sets entity delay.
- Duplicate `TD=` / `DELAY=` inside one call fails.
- Source-level `TD=` plus call-local `TD=` fails with a clear message.

Writer tests:

- Generated C# emits scaled numerator and delay for direct calls.
- Mixed calls emit helper entities with each helper's own numerator and delay.
- Invalid inline options emit the existing writer error-comment style.

Docs:

- Update `src/docs/articles/laplace.md` and `src/docs/articles/behavioral-source.md`.
- Remove the statement that inline function options are unsupported.
- Document source-level versus call-local option conflict rules.

## Phase 2: Multiple Delayed Function Calls

Once call-local options exist, allow mixed expressions with multiple delayed calls:

```spice
B1 out 0 V={
  LAPLACE(V(a), 1/(1+s*t1), TD=1n)
  + LAPLACE(V(b), 1/(1+s*t2), TD=2n)
}
```

Implementation notes:

- Keep the existing source-level delay restriction for multiple calls.
- Allow multiple calls when each delay is call-local.
- Each call lowers to its own helper entity with its own `Parameters.Delay`.
- Direct whole-expression calls continue to map directly to the final Laplace entity.

Tests:

- Mixed two-call expression creates two helpers with distinct delays.
- Source-level `TD=` with two calls still fails.
- One delayed call plus one undelayed call succeeds.
- OP and AC behavior remain unchanged by delay where the runtime treats delay outside those analyses.
- Add a transient smoke test that verifies the circuit runs and the delayed output stays bounded.

## Phase 3: Simple Arbitrary Input Expressions

Support common input expressions by lowering them into helper behavioral sources:

```spice
LAPLACE(V(a)-V(b), H(s))
LAPLACE(2*V(in), H(s))
LAPLACE(V(a)+I(Vsense)*rscale, H(s))
```

Implementation strategy:

- Preserve existing direct probe input fast paths.
- For a non-probe input expression, create an internal behavioral voltage helper that evaluates the input expression.
- Feed that helper node into an ordinary voltage-controlled Laplace helper:

  ```text
  input expression -> helper behavioral voltage source -> Laplace voltage input
  ```

- Only allow arbitrary input expressions in function-style `LAPLACE(...)` first.
- Do not expand source-level `E/G/F/H ... LAPLACE {input}` input syntax until the function-style path is stable.
- Reuse the existing behavioral expression resolver so parameters, functions, and probes behave like normal `B` / `VALUE` expressions.

Naming:

- Use deterministic helper names below the existing reserved namespace:

  ```text
  __ssp_laplace_input_<sourceName>_<index>
  __ssp_laplace_input_<sourceName>_<index>_src
  ```

- Continue using the existing `__ssp_laplace_<sourceName>_<index>` names for transfer helper outputs.
- Maintain collision checks against existing generated entities.

Validation:

- Reject nested `LAPLACE(...)` inside the input helper expression.
- Reject input expressions that cannot be parsed by the existing behavioral expression parser.
- Keep transfer expression validation unchanged: it must still be a rational polynomial in `s`.

Tests:

- `LAPLACE(V(a)-V(b), H(s))` matches `LAPLACE(V(a,b), H(s))`.
- `LAPLACE(2*V(in), H(s))` doubles OP gain.
- Mixed expression with arbitrary input creates input helper, Laplace helper, and final behavioral source in the correct order.
- Subcircuit test verifies generated helper names are scoped and collision-safe.

## Phase 4: Broader Verification And Diagnostics

Expand confidence around behavior that already mostly works but is not fully claimed.

Transient tests:

- Delayed first-order `E` low-pass step response.
- Delayed first-order `G` low-pass step response through a grounded load.
- `F` and `H` current-controlled first-order transient smoke tests.
- Mixed function-style transient smoke test with at least one helper.

Diagnostics:

- Include the rejected transfer form in validation messages where practical.
- For improper transfers, report numerator and denominator degree.
- For singular DC gain, suggest adding a low-frequency pole or finite leakage term.
- For arbitrary input rejection, suggest `V(a,b)` when the expression is exactly `V(a)-V(b)`.

Docs:

- Update the limitations section in `laplace.md` to say which transient paths are verified.
- Add a short troubleshooting table for `1/s`, `s`, arbitrary input expressions, duplicate options, and source-level delay with multiple calls.

## Phase 5: Compatibility Investigation

Investigate these forms after phases 1-4 are complete. Do not implement them until runtime semantics and dialect compatibility are clear.

### Coefficient-List Syntax

Potential user-facing goal:

```spice
E1 out 0 LAPLACE {V(in)} NUM={1} DEN={1 1u}
E1 out 0 LAPLACE {V(in)} = [num coefficients] [den coefficients]
```

Open decisions:

- Which dialect syntax should be accepted?
- Are coefficients ascending or descending in the source dialect?
- Should coefficients allow parameters?
- How should this interact with `M=`, `TD=`, and `DELAY=`?

### Internal-State Options

Potential user-facing goal:

```spice
LAPLACE(V(in), H(s), IC=...)
```

Open decisions:

- Does SpiceSharpBehavioral expose enough state initialization support?
- Are these options source-level, call-local, or both?
- How do they behave across OP, AC, and transient analyses?

### Singular Or Improper Transfer Support

Examples:

```spice
LAPLACE(V(in), 1/s)
LAPLACE(V(in), s)
```

Current policy is to reject these. Any relaxation must first prove:

- OP behavior is deterministic.
- AC behavior matches user expectations.
- Transient behavior is numerically stable.
- Diagnostics remain clear for unsupported analysis combinations.

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

## Acceptance Criteria

- Existing source-level and function-style LAPLACE tests continue passing.
- Inline call-local options work in direct and mixed expressions.
- Multiple delayed function-style calls work when delay is call-local.
- Source-level delay with multiple function calls still fails clearly.
- Simple arbitrary input expressions are either supported through helper lowering or rejected with better diagnostics.
- Generated C# output matches reader behavior for all newly supported forms.
- Public docs accurately distinguish supported, deferred, and rejected LAPLACE forms.

