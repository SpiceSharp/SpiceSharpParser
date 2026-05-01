---
title: PSpice LAPLACE Support Feasibility
status: Draft / Roadmap
scope: SpiceSharpParser
dependencies: SpiceSharpBehavioral 3.2.0
last_reviewed: 2026-05-01
---

# PSpice `LAPLACE` Support Feasibility

## TL;DR

- PSpice `LAPLACE` support is feasible for SpiceSharpParser.
- The preferred first implementation is a built-in component mapping, not a custom simulation behavior.
- `SpiceSharpBehavioral 3.2.0` already contains Laplace source entities, shared numerator / denominator / delay parameters, and biasing / frequency / time behaviors.
- The MVP should support `E` and `G` sources with `V(node)` or `V(node1,node2)` input and a rational transfer expression in `s`.
- The hard work is parser disambiguation, rational-polynomial coefficient extraction, validation, diagnostics, and tests.
- A short AC spike already confirmed that `1/(1+s*tau)` maps to `Numerator=[1]`, `Denominator=[1,tau]`, and produces the expected cutoff response.

Recommended first supported forms:

```spice
Ename out+ out- LAPLACE {V(ctrl+)}       = {H(s)}
Ename out+ out- LAPLACE {V(ctrl+,ctrl-)} = {H(s)}
Gname out+ out- LAPLACE {V(ctrl+)}       = {H(s)}
Gname out+ out- LAPLACE {V(ctrl+,ctrl-)} = {H(s)}
```

Where `H(s)` is a finite rational polynomial in `s` whose coefficients reduce to constants after parameter evaluation.

## Table of Contents

1. [Decision](#decision)
2. [Current Status](#current-status)
3. [Dependency Facts](#dependency-facts)
4. [MVP Scope](#mvp-scope)
5. [Existing Architecture](#existing-architecture)
6. [Parser Gap](#parser-gap)
7. [Transfer-Function Math](#transfer-function-math)
8. [Reader Integration](#reader-integration)
9. [Diagnostics](#diagnostics)
10. [Compatibility Policy](#compatibility-policy)
11. [Multiple Syntax Strategy](#multiple-syntax-strategy)
12. [Implementation Plan](#implementation-plan)
13. [Acceptance Criteria](#acceptance-criteria)
14. [Test Plan](#test-plan)
15. [Worked Examples](#worked-examples)
16. [Sample Netlists](#sample-netlists)
17. [Debugging Guide](#debugging-guide)
18. [Key Files](#key-files)
19. [References](#references)

---

## Decision

Implement `LAPLACE` as a source-level transfer-function feature. Do not model it as a scalar behavioral function such as `laplace(...)`, and do not add it to the normal real-valued expression function set.

Recommended path:

1. Add parser support for an expression-to-expression assignment parameter.
2. Parse and validate `LAPLACE {input_expr} = {transfer_expr}` at source-reader level.
3. Convert `transfer_expr` into numerator and denominator coefficient arrays in ascending powers of `s`.
4. Map the MVP directly to `SpiceSharpBehavioral` Laplace source entities.
5. Defer broader PSpice forms until the narrow path is covered by OP, AC, diagnostic, and transient-verification tests.

This approach keeps the implementation small, aligns with existing source-generator patterns, and avoids duplicating simulation behaviors that already exist in the dependency.

## Current Status

`LAPLACE` is not currently parsed by SpiceSharpParser.

Adjacent features already exist:

- `VALUE={expr}` behavioral sources.
- `TABLE {expr} = (...)` translation.
- `POLY(n)` translation.
- Arbitrary `B` voltage/current behavioral sources.
- Real-valued math functions and user `.FUNC` definitions.

Important limitation: the existing expression evaluation path is scalar and real-valued. `CustomRealBuilder` returns `double`, which is correct for `VALUE`, `TABLE`, and `POLY`, but not enough for transfer functions that must be evaluated at `s=0`, `s=j*omega`, or as a transient state-space system.

## Dependency Facts

Local package references use `SpiceSharpBehavioral 3.2.0`.

The installed package documentation exposes these Laplace entities:

- `SpiceSharp.Components.LaplaceVoltageControlledVoltageSource`
- `SpiceSharp.Components.LaplaceVoltageControlledCurrentSource`
- `SpiceSharp.Components.LaplaceCurrentControlledVoltageSource`
- `SpiceSharp.Components.LaplaceCurrentControlledCurrentSource`

Each supports:

- A constructor taking `name` only.
- A full constructor taking source nodes / controlling source or control nodes / numerator / denominator / delay.
- Shared Laplace parameters: `Numerator`, `Denominator`, `Delay`.
- Biasing, frequency, and time behaviors.

For the parser MVP, prefer the `name` constructor plus `context.CreateNodes(...)` / existing naming helpers. That preserves subcircuit expansion and generated object naming conventions already used by the source generators.

Coefficient order is ascending powers of `s`:

```text
1 + tau*s      -> [1, tau]
s + wc         -> [wc, 1]
s^2 + a*s + b  -> [b, a, 1]
```

For:

```text
H(s) = 1 / (1 + s*tau)
```

Use:

```csharp
Numerator   = new[] { 1.0 };
Denominator = new[] { 1.0, tau };
Delay       = 0.0;
```

An AC probe at `f = 1/(2*pi*tau)` produced the expected magnitude near `0.70710678` and phase near `-pi/4`.

## MVP Scope

### Supported

```spice
Ename out+ out- LAPLACE {V(ctrl+)}       = {transfer_expr}
Ename out+ out- LAPLACE {V(ctrl+,ctrl-)} = {transfer_expr}
Gname out+ out- LAPLACE {V(ctrl+)}       = {transfer_expr}
Gname out+ out- LAPLACE {V(ctrl+,ctrl-)} = {transfer_expr}
```

Transfer-expression subset:

- Numeric constants.
- Parameters that evaluate to constants.
- Symbolic `s`.
- Parentheses.
- Unary `+` / `-`.
- Binary `+`, `-`, `*`, `/`.
- Non-negative integer powers.

Input-expression subset:

- `V(node)` mapped to control nodes `node`, `0`.
- `V(node1,node2)` mapped to control nodes `node1`, `node2`.

### Deferred

- `F` and `H` current-controlled Laplace sources.
- `B` source Laplace forms.
- Arbitrary input expressions such as `V(a)-V(b)+I(Vsense)`.
- Alternative `E` / `G` Laplace syntax variants that omit `=` or put `=` directly after `LAPLACE`.
- `VALUE={LAPLACE(...)}` / function-like ABM forms.
- Non-rational transfer expressions.
- Explicit delay syntax such as `TD=` or `DELAY=`.
- Initial-condition syntax for Laplace internal state.
- Generated C# writer support, unless the MVP delivery explicitly includes writer parity.

`F` and `H` are lower-risk than originally assumed because the dependency has matching entities. They still deserve a later milestone because their PSpice syntax and controlling-source name handling are different from the voltage-controlled `E` / `G` path.

## Existing Architecture

Controlled-source parsing currently flows through:

- [src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/VoltageSourceGenerator.cs](../src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/VoltageSourceGenerator.cs)
- [src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/CurrentSourceGenerator.cs](../src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/CurrentSourceGenerator.cs)
- [src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/ArbitraryBehavioralGenerator.cs](../src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/ArbitraryBehavioralGenerator.cs)
- [src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/ExpressionFactory.cs](../src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/ExpressionFactory.cs)
- [src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/ExpressionResolver.cs](../src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/ExpressionResolver.cs)
- [src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/CustomRealBuilder.cs](../src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/CustomRealBuilder.cs)
- [src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/SpiceEvaluationContext.cs](../src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/SpiceEvaluationContext.cs)

Relevant existing behavior:

- Simple linear `E` / `G` sources are handled before custom behavioral branches.
- `VALUE`, `POLY`, and `TABLE` are recognized in source generators.
- `TABLE` is lowered into a scalar behavioral expression via `ExpressionFactory.CreateTableExpression(...)`.
- Behavioral source entities use parameterless construction, `context.CreateNodes(...)`, and `ParseAction` callbacks.

`LAPLACE` should follow the same source-generator entry point pattern, but it should produce a Laplace source entity rather than a scalar behavioral expression.

## Parser Gap

The tokenizer probably does not need a new token type. `LAPLACE` can remain a `WORD`, matching the current handling of `VALUE`, `POLY`, and `TABLE`.

The parser does need a new grammar shape.

Current `TABLE` syntax uses an expression followed by points:

```spice
TABLE {V(in)} = (0,0) (1,1)
TABLE {V(in)}   (0,0) (1,1)
```

PSpice Laplace uses expression assignment:

```spice
LAPLACE {V(in)} = {1/(1+s*tau)}
```

Today `{expr} = ...` enters `ExpressionEqual` / `ReadExpressionEqual`, whose right-hand side is `Points`. That is correct for `TABLE`, but wrong for `LAPLACE {expr} = {expr}`.

Add a dedicated parameter model:

```csharp
public sealed class ExpressionAssignmentParameter : Parameter
{
    public string LeftExpression { get; }
    public string RightExpression { get; }
}
```

Add a new parse symbol:

```text
ExpressionAssignment : EXPRESSION "=" EXPRESSION
```

Recommended lookahead in `ReadParameter`:

| Token pattern after `{expr}` | Parser path |
|------------------------------|-------------|
| `= (`                        | Existing `ExpressionEqual` for `TABLE` |
| `(`                          | Existing `ExpressionEqual` for no-equals `TABLE` |
| `= {expr}`                   | New `ExpressionAssignment` |
| `= 'expr'`                   | New `ExpressionAssignment` |

Files to change:

- [src/SpiceSharpParser/Parsers/Netlist/Spice/Symbols.cs](../src/SpiceSharpParser/Parsers/Netlist/Spice/Symbols.cs)
- [src/SpiceSharpParser/Parsers/Netlist/Spice/Internals/ParseTreeGenerator.cs](../src/SpiceSharpParser/Parsers/Netlist/Spice/Internals/ParseTreeGenerator.cs)
- [src/SpiceSharpParser/Parsers/Netlist/Spice/Internals/ParseTreeEvaluator.cs](../src/SpiceSharpParser/Parsers/Netlist/Spice/Internals/ParseTreeEvaluator.cs)
- [src/SpiceSharpParser/Parsers/Netlist/Spice/SpiceGrammarBNF.txt](../src/SpiceSharpParser/Parsers/Netlist/Spice/SpiceGrammarBNF.txt)
- `src/SpiceSharpParser/Models/Netlist/Spice/Objects/Parameters/ExpressionAssignmentParameter.cs` (new)

Regression rules:

- `TABLE {V(in)} = (0,0)` must still produce `ExpressionEqualParameter`.
- `TABLE {V(in)} (0,0)` must still produce `ExpressionEqualParameter`.
- `LAPLACE {V(in)} = {1/(1+s*tau)}` should produce `ExpressionAssignmentParameter`.
- Continuation lines must remain a single logical statement.

## Transfer-Function Math

Model Laplace as:

```text
Y(s) = H(s) * X(s)
H(s) = N(s) / D(s)
```

Represent polynomials in ascending powers of `s`:

```text
N(s) = b0 + b1*s + b2*s^2 + ... + bm*s^m
D(s) = a0 + a1*s + a2*s^2 + ... + an*s^n
```

Minimal data types:

```csharp
internal sealed class Polynomial
{
    public IReadOnlyList<double> Coefficients { get; }
    public int Degree { get; }

    public Polynomial Add(Polynomial other);
    public Polynomial Subtract(Polynomial other);
    public Polynomial Multiply(Polynomial other);
    public Polynomial Scale(double factor);
    public Polynomial Pow(int exponent);
    public double EvaluateReal(double value);
    public Complex EvaluateComplex(Complex value);
    public Polynomial Normalize(double tolerance);
}

internal sealed class RationalPolynomial
{
    public Polynomial Numerator { get; }
    public Polynomial Denominator { get; }

    public RationalPolynomial Add(RationalPolynomial other);
    public RationalPolynomial Subtract(RationalPolynomial other);
    public RationalPolynomial Multiply(RationalPolynomial other);
    public RationalPolynomial Divide(RationalPolynomial other);
    public RationalPolynomial Pow(int exponent);
    public Complex EvaluateComplex(Complex value);
}
```

Builder identities:

```text
constant c -> c / 1
s          -> s / 1
A + B      -> (An*Bd + Bn*Ad) / (Ad*Bd)
A - B      -> (An*Bd - Bn*Ad) / (Ad*Bd)
A * B      -> (An*Bn) / (Ad*Bd)
A / B      -> (An*Bd) / (Ad*Bn)
A ^ n      -> (An^n) / (Ad^n), where n is a non-negative integer
-A         -> (-An) / Ad
```

Recommended expression-parser strategy:

1. Reuse the existing expression lexer/parser to get an AST.
2. Build a new `RationalPolynomial` builder rather than using `CustomRealBuilder`.
3. Treat `s` as symbolic only while building the Laplace transfer expression.
4. Evaluate all other parameters to constants before coefficient normalization.
5. Reject any expression that cannot reduce to a rational polynomial with finite coefficients.

Do not perform polynomial GCD simplification in v1. It adds complexity and is not needed for the usual PSpice transfer-function forms.

### Validation Rules

- Reject an empty numerator or denominator.
- Reject a zero denominator polynomial.
- Reject `NaN` and infinite coefficients.
- Reject non-integer powers.
- Reject negative powers; users can write `1/s` instead.
- Reject functions of `s`, including `sin(s)`, `exp(s)`, `sqrt(s)`, and `abs(s)`.
- Reject voltage/current/property probes inside `H(s)`.
- Reject hidden symbolic parameter expansion such as `.PARAM pole={s+1000}`.
- Reject improper transfers (`deg N > deg D`) in the MVP.
- Reject singular DC gain (`D(0) == 0`) unless a focused spike proves the runtime handles the target scenario safely.
- Preserve interior zeros: `s^2 + 1` is `[1, 0, 1]`, not `[1, 1]`.
- Trim only leading near-zero high-order coefficients.

Suggested tolerances:

```text
zeroTolerance = 1e-18
relativeTolerance = 1e-12 * max(abs(coefficient))
```

Cap transfer-function order around 8-10 initially. Higher-order filters should usually be expressed as cascaded biquads.

### AC Evaluation for Tests

Runtime AC behavior belongs to SpiceSharpBehavioral, but unit tests should still evaluate the generated transfer function independently.

Use Horner's method:

```csharp
Complex Evaluate(IReadOnlyList<double> coefficients, Complex s)
{
    var result = new Complex(coefficients[coefficients.Count - 1], 0.0);
    for (var index = coefficients.Count - 2; index >= 0; index--)
    {
        result = (result * s) + coefficients[index];
    }

    return result;
}
```

Avoid per-term `Complex.Pow`; it is slower and easier to destabilize numerically.

### Transient Policy

Transient cannot be implemented by substituting `s=j*omega`. It requires a time-domain realization.

For the MVP:

- Delegate transient runtime behavior to the built-in `Laplace...Time` behaviors.
- Do not claim transient support until step-response tests pass.
- Require proper transfer functions (`deg N <= deg D`).
- Document any observed initial-condition behavior.

## Reader Integration

Add a focused `LaplaceSourceParser` near the source generators:

```text
src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/LaplaceSourceParser.cs
src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/LaplaceSourceDefinition.cs
src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/LaplaceSourceInput.cs
```

Suggested definition model:

```csharp
internal sealed class LaplaceSourceDefinition
{
    public string SourceName { get; }
    public string SourceKind { get; } // "e" or "g" for MVP
    public string InputExpression { get; }
    public LaplaceSourceInput Input { get; }
    public string TransferExpression { get; }
    public LaplaceTransferFunction TransferFunction { get; }
    public double Delay { get; }
    public SpiceLineInfo LineInfo { get; }
}

internal sealed class LaplaceSourceInput
{
    public string ControlPositiveNode { get; }
    public string ControlNegativeNode { get; }
}

internal sealed class LaplaceTransferFunction
{
    public Polynomial Numerator { get; }
    public Polynomial Denominator { get; }
    public double[] NumeratorCoefficients { get; }
    public double[] DenominatorCoefficients { get; }
    public Complex EvaluateForTests(Complex s);
}
```

Detection belongs in `CreateCustomVoltageSource(...)` and `CreateCustomCurrentSource(...)`.

Branch order:

1. Existing simple linear source fast path.
2. `LAPLACE` custom branch.
3. Existing `VALUE` branch.
4. Existing `POLY` branch.
5. Existing `TABLE` branch.

This avoids interpreting Laplace source parameters as scalar behavioral syntax.

Entity creation should keep existing reader conventions:

```csharp
var entity = new LaplaceVoltageControlledVoltageSource(name);
context.CreateNodes(entity, laplaceNodes); // out+, out-, control+, control-
entity.Parameters.Numerator = definition.TransferFunction.NumeratorCoefficients;
entity.Parameters.Denominator = definition.TransferFunction.DenominatorCoefficients;
entity.Parameters.Delay = definition.Delay;
return entity;
```

For `G` sources, use `LaplaceVoltageControlledCurrentSource` with the same node order.

Do not bypass `context.CreateNodes(...)` with raw node names. That would risk incorrect subcircuit scoping, aliases, and generated object names.

### Input Parsing

Accept only these MVP inputs:

| Input | control+ | control- |
|-------|----------|----------|
| `V(in)` | `in` | `0` |
| `v(in,0)` | `in` | `0` |
| `V(n001,n002)` | `n001` | `n002` |

Reject with targeted diagnostics:

- `V(a)-V(b)`
- `I(Vsense)`
- `V(a,b,c)`
- `V(a+1)`
- `V({node})`
- Any arbitrary expression not shaped exactly like a voltage probe.

Prefer parsing with the existing expression parser and accepting only the expected AST shape. Avoid loose regex parsing of electrical expressions.

### `M=` Policy

Do not silently ignore `M=`.

Recommended MVP behavior: reject `M=` on Laplace sources with a clear diagnostic until tests define the intended semantics. If support is added, multiply the numerator coefficients by `M` after evaluating `M` to a constant.

### Delay Policy

The runtime supports `Delay`, but PSpice delay syntax for Laplace sources needs confirmation.

MVP behavior:

- Set `Delay = 0.0`.
- Reject `TD=`, `DELAY=`, or trailing delay-like parameters with a clear diagnostic.

## Diagnostics

Diagnostics should be validation entries, not generic parser exceptions, when the source line is syntactically parseable but semantically unsupported.

Recommended messages:

```text
laplace expects input expression
laplace expects transfer expression after '='
laplace input expression must be V(node) or V(node1,node2)
laplace transfer expression must be a rational polynomial in s
laplace transfer denominator cannot be zero
laplace transfer function is improper; numerator degree exceeds denominator degree
laplace transfer function has singular DC gain
laplace transfer coefficients must be constant expressions
laplace transfer expression reserves symbol 's'; use a different parameter name
laplace delay syntax is not supported yet
laplace multiplier M is not supported yet
laplace syntax variant is recognized but not supported yet
laplace function-like VALUE form is not supported yet; use E/G LAPLACE {V(...)} = {H(s)}
laplace source supports only E and G voltage-controlled forms in this version
```

Each diagnostic should carry:

- Source name.
- Raw input expression.
- Raw transfer expression when available.
- `SpiceLineInfo`.

Good diagnostics are part of compatibility. A precise rejection is better than falling through to misleading `TABLE`, `VALUE`, or generic parameter-count errors.

## Compatibility Policy

| Feature | MVP behavior | Later behavior |
|---------|--------------|----------------|
| `E ... LAPLACE {V(n)} = {H(s)}` | Support | Keep |
| `E ... LAPLACE {V(n1,n2)} = {H(s)}` | Support | Keep |
| `G ... LAPLACE {V(n)} = {H(s)}` | Support | Keep |
| `G ... LAPLACE {V(n1,n2)} = {H(s)}` | Support | Keep |
| `E` / `G ... LAPLACE {V(n)} {H(s)}` | Targeted unsupported diagnostic until canonical form is stable | Add recognizer that lowers to the same model |
| `E` / `G ... LAPLACE = {V(n)} {H(s)}` | Targeted unsupported diagnostic until canonical form is stable | Add recognizer that lowers to the same model |
| `F` / `H` current-controlled forms | Targeted unsupported diagnostic | Map to built-in current-controlled Laplace entities |
| `B` source Laplace forms | Targeted unsupported diagnostic | Investigate PSpice ABM syntax |
| `VALUE={LAPLACE(...)}` function-like form | Targeted unsupported diagnostic | Investigate as source-level special form |
| Arbitrary input expression | Reject | Lower through helper behavioral source or custom behavior |
| Rational polynomial in `s` | Support | Keep |
| Non-rational functions of `s` | Reject | Likely keep rejected |
| Singular DC gain (`1/s`) | Reject unless verified safe | Possible AC-only or IC-aware mode |
| Improper transfer (`deg N > deg D`) | Reject | Allow only if proven safe for claimed analyses |
| Explicit delay | Reject | Map after syntax and behavior tests |
| Transient | Verify before documenting | Document verified subset |
| Generated C# writer | Defer or include as final polish | Emit built-in Laplace entities |

### PSpice Variants to Investigate Later

Do not guess semantics for these until there are compatibility examples and tests:

```spice
E1 out 0 LAPLACE {V(in)} {1/(1+s*tau)}
E1 out 0 LAPLACE = {V(in)} {1/(1+s*tau)}
E1 out 0 VALUE = {LAPLACE(V(in), 1/(1+s*tau))}
```

## Multiple Syntax Strategy

Treat multiple PSpice spellings as a compatibility layer over one internal model. The runtime and coefficient builder should not care which syntax variant was used.

All supported forms should normalize into the same `LaplaceSourceDefinition`:

```csharp
internal sealed class LaplaceSourceDefinition
{
    public string SourceName { get; }
    public string SourceKind { get; }
    public LaplaceSourceInput Input { get; }
    public string InputExpression { get; }
    public string TransferExpression { get; }
    public LaplaceTransferFunction TransferFunction { get; }
    public double Delay { get; }
    public SpiceLineInfo LineInfo { get; }
}
```

Use small syntax recognizers rather than one large parser branch:

```csharp
internal interface ILaplaceSyntaxRecognizer
{
    bool TryParse(
        string sourceName,
        string sourceKind,
        ParameterCollection parameters,
        IReadingContext context,
        out LaplaceSourceDefinition definition);
}
```

Recognizer priority should be explicit and tested:

1. `CanonicalExpressionAssignmentRecognizer` for `LAPLACE {V(in)} = {H(s)}`.
2. `NoEqualsExpressionPairRecognizer` for `LAPLACE {V(in)} {H(s)}`.
3. `EqualsExpressionPairRecognizer` for `LAPLACE = {V(in)} {H(s)}`.
4. `CurrentControlledRecognizer` for later `F` / `H` forms using `I(Vsense)`.
5. `ValueLaplaceFunctionRecognizer` for later `VALUE={LAPLACE(...)}` and `B`-source ABM forms.
6. `UnsupportedKnownVariantRecognizer` for recognized-but-deferred forms.

The last recognizer is deliberate. If a user writes a known PSpice Laplace variant that is not implemented yet, emit a targeted diagnostic instead of letting the line fall through to `VALUE`, `TABLE`, or generic parameter-count handling.

### Syntax Families

| Family | Example | Near-term behavior | Notes |
|--------|---------|--------------------|-------|
| Canonical assignment | `E1 out 0 LAPLACE {V(in)} = {1/(1+s*tau)}` | Support first | Exercises grammar gap and runtime mapping. |
| No-equals expression pair | `E1 out 0 LAPLACE {V(in)} {1/(1+s*tau)}` | Add after canonical | Should normalize to the same definition and coefficients. |
| Equals-after-keyword pair | `E1 out 0 LAPLACE = {V(in)} {1/(1+s*tau)}` | Add after canonical | Likely parses differently; keep syntax handling isolated in a recognizer. |
| Current-controlled | `F1 out 0 LAPLACE {I(Vsense)} = {H(s)}` | Later milestone | Runtime entities exist, but controlling-source parsing and PSpice compatibility need tests. |
| Function-like `VALUE` | `E1 out 0 VALUE = {LAPLACE(V(in), H(s))}` | Investigate later | Must be handled as source-level Laplace, not a scalar expression function. |
| `B` source ABM | `B1 out 0 V = {LAPLACE(V(in), H(s))}` | Investigate later | May require arbitrary input-expression lowering or custom behavior. |
| Delay/options | `TD=...`, `DELAY=...` | Reject initially | Runtime has `Delay`; PSpice syntax and semantics need confirmation. |

### Normalization Rules

- Equivalent `E` / `G` syntax variants must produce identical input nodes, transfer coefficients, delay, and entity type.
- Syntax recognizers should only parse surface shape. They should call the same input parser and transfer-expression builder.
- The canonical recognizer is the only recognizer needed for the MVP.
- Later recognizers should be added one at a time with compatibility matrix tests.
- Function-like forms must not be registered in the normal scalar function table. They are analysis-dependent source constructs.

## Implementation Plan

### Phase 0: Runtime Spike

Before grammar work, prove the runtime path manually in the repo's test harness:

1. Construct `LaplaceVoltageControlledVoltageSource` directly and run `.OP` / `.AC`.
2. Construct `LaplaceVoltageControlledCurrentSource` directly and run `.OP` / `.AC` through a load resistor.
3. Verify `Denominator=[1,tau]` produces the expected first-order low-pass response.
4. Run a transient step-response smoke test and record whether behavior is ready to claim.

If manual entity construction fails, stop and investigate the dependency before writing parser code.

### Phase 1: Grammar

1. Add tokenizer regression tests for `LAPLACE`, braced expressions, quoted expressions, and continuation lines.
2. Add a failing parser test for `{V(in)} = {1/(1+s*tau)}`.
3. Add `ExpressionAssignmentParameter`.
4. Add `Symbols.ExpressionAssignment`.
5. Add `ReadExpressionAssignment(...)`.
6. Update `ReadParameter(...)` lookahead to route expression-to-expression assignments correctly.
7. Update `ParseTreeEvaluator`.
8. Add `TABLE` regression tests.
9. Update `SpiceGrammarBNF.txt`.

### Phase 2: Math Builder

1. Add `Polynomial` and `RationalPolynomial`.
2. Add `LaplaceExpressionParser` or equivalent builder.
3. Reuse existing expression AST creation.
4. Implement constant parameter evaluation.
5. Implement symbolic `s` handling.
6. Add normalization and validation.
7. Add coefficient-generation tests for worked examples.

### Phase 3: `E` Source Mapping

1. Add `LaplaceSourceParser` and source definition models.
2. Add `CanonicalExpressionAssignmentRecognizer` as the first syntax recognizer.
3. Detect `LAPLACE` in `VoltageSourceGenerator.CreateCustomVoltageSource(...)`.
4. Parse `V(node)` / `V(node1,node2)` input.
5. Map to `LaplaceVoltageControlledVoltageSource`.
6. Add OP and AC integration tests.
7. Add malformed syntax and unsupported-feature diagnostics.

### Phase 4: `G` Source Mapping

1. Detect `LAPLACE` in `CurrentSourceGenerator.CreateCustomCurrentSource(...)`.
2. Reuse the same source parser and transfer-function builder.
3. Map to `LaplaceVoltageControlledCurrentSource`.
4. Add sign-convention and load-resistor tests.

### Phase 5: Syntax Compatibility Layer

1. Add `UnsupportedKnownVariantRecognizer` so common deferred forms receive targeted diagnostics.
2. Add `NoEqualsExpressionPairRecognizer` for `LAPLACE {input} {transfer}` only after canonical tests pass.
3. Add `EqualsExpressionPairRecognizer` for `LAPLACE = {input} {transfer}` only after no-equals tests pass.
4. Verify supported variants normalize to identical `LaplaceSourceDefinition` values.
5. Keep function-like `VALUE={LAPLACE(...)}` forms diagnostic-only until their PSpice semantics are confirmed.
6. Keep `F` / `H` recognizers diagnostic-only until current-controlled tests are planned.

### Phase 6: Polish

1. Decide and document transient support based on step-response results.
2. Add generated C# writer support if parity is required for the release.
3. Update [src/docs/articles/behavioral-source.md](../src/docs/articles/behavioral-source.md).
4. Add release-note text listing supported and unsupported Laplace forms.

## Acceptance Criteria

The MVP is ready when all of these are true:

- `E` and `G` Laplace sources parse without disturbing existing `VALUE`, `POLY`, or `TABLE` behavior.
- Transfer coefficients are generated in ascending powers of `s`.
- Parameterized low-pass, high-pass, and biquad examples produce expected coefficients.
- Any supported alternate PSpice syntax variants normalize to the same definitions and coefficients as the canonical form.
- AC integration tests match expected magnitude and phase at key frequencies.
- Unsupported syntax produces targeted validation errors with line info.
- Recognized-but-deferred PSpice variants produce targeted diagnostics rather than falling through to unrelated source handling.
- No supported user mistake falls through to a generic parser exception.
- Transient support is either verified by tests or explicitly documented as not yet claimed.
- User documentation states syntax, supported analyses, limitations, and examples.

## Test Plan

### Parser Tests

Place near [src/SpiceSharpParser.Tests/Parsers](../src/SpiceSharpParser.Tests/Parsers).

- `LAPLACE` remains a word token.
- `{V(in)}` and `{1/(1+s*tau)}` remain intact expression tokens.
- Continuation-line form parses as one statement:

  ```spice
  ELOW out 0 LAPLACE {V(in)} =
  + {1/(1+s*tau)}
  ```

- `{V(in)} = {1/(1+s*tau)}` produces `ExpressionAssignmentParameter`.
- `ExpressionAssignmentParameter.LeftExpression == "V(in)"`.
- `ExpressionAssignmentParameter.RightExpression == "1/(1+s*tau)"`.
- `TABLE {V(in)} = (0,0)` still produces `ExpressionEqualParameter`.
- `TABLE {V(in)} (0,0)` still produces `ExpressionEqualParameter`.

### Math Unit Tests

- Polynomial add, subtract, multiply, scale, power, trimming, degree.
- Polynomial real and complex evaluation.
- Rational add, subtract, multiply, divide, power.
- Zero denominator rejection.
- Coefficients for all worked examples.
- Rejection of `s^0.5`, `s^-1`, `sin(s)`, `exp(s)`, `V(x)*s`, stochastic functions, and hidden symbolic parameters.

### Source Parser Tests

- Valid `E` single-ended input.
- Valid `E` differential input.
- Valid `G` single-ended input.
- Valid `G` differential input.
- Missing input expression.
- Missing `=`.
- Missing transfer expression.
- Unsupported `F`, `H`, and `B` forms.
- Unsupported arbitrary input expression.
- Unsupported delay and `M=` parameters.

### Compatibility Matrix Tests

For every supported syntax variant, verify:

- It maps to the same `LaplaceSourceDefinition` as the canonical form.
- It produces the same numerator and denominator arrays.
- It creates the same SpiceSharpBehavioral entity type.
- It uses the same output and control-node ordering.
- It keeps existing `VALUE`, `POLY`, and `TABLE` behavior unchanged.

For every recognized but deferred syntax variant, verify:

- It reports a targeted validation error.
- It does not fall through to unrelated source generation.
- It includes source name and line information.

### Integration Tests

Place beside existing analog behavioral tests under [src/SpiceSharpParser.IntegrationTests/AnalogBehavioralModeling](../src/SpiceSharpParser.IntegrationTests/AnalogBehavioralModeling).

- `E` low-pass OP gain near `1`.
- `E` low-pass AC magnitude near `0.70710678` at cutoff.
- `E` low-pass AC phase near `-pi/4` at cutoff.
- `E` high-pass AC magnitude near `0.70710678` at cutoff.
- `E` high-pass AC phase near `+pi/4` at cutoff.
- Differential-input low-pass validates control-node ordering.
- Butterworth biquad is near `-3 dB` at `f0`.
- `G` low-pass drives expected current into a resistor load.
- Unsupported syntax produces validation errors, not test-process exceptions.

Use `.MEAS AC VM/VP` patterns from [src/SpiceSharpParser.IntegrationTests/VoltageExportTests.cs](../src/SpiceSharpParser.IntegrationTests/VoltageExportTests.cs) where practical.

### Transient Tests

- Low-pass step response against `1 - exp(-t/tau)` at several times.
- Zero-input behavior after OP setup.
- Loose tolerance initially, tightened after runtime behavior is understood.

If these fail, document OP/AC-only support and split transient into a follow-up issue.

### Generated-Code Tests

Only add when writer support is in scope.

- Low-pass `E` source emits `LaplaceVoltageControlledVoltageSource`.
- Low-pass `G` source emits `LaplaceVoltageControlledCurrentSource`.
- Coefficients are emitted in ascending order.
- Numeric formatting uses invariant culture, including values such as `1e-6`.
- Generated entity uses the same node order as the reader path.

## Worked Examples

### First-Order Low-Pass

```spice
.PARAM tau=1u
ELOW out 0 LAPLACE {V(in)} = {1 / (1 + s*tau)}
```

Expected coefficients:

```text
Numerator   = [1]
Denominator = [1, tau]
```

DC gain is `1`. At `f = 1/(2*pi*tau)`, expected magnitude is about `0.70710678` and phase is about `-pi/4`.

### Differential-Input Low-Pass

```spice
ELOW out 0 LAPLACE {V(inp,inn)} = {1 / (1 + s*tau)}
```

Expected control nodes: `inp`, `inn`. Same transfer coefficients as the single-ended low-pass.

### First-Order High-Pass

```spice
.PARAM fc=1k
.PARAM wc={2*pi*fc}
EHIGH out 0 LAPLACE {V(in)} = {s / (s + wc)}
```

Expected coefficients:

```text
Numerator   = [0, 1]
Denominator = [wc, 1]
```

The zero constant term in the numerator must be preserved.

### Inverting Low-Pass

```spice
.PARAM gain=-10
.PARAM fc=10k
.PARAM wc={2*pi*fc}
EAMP out 0 LAPLACE {V(in)} = {gain*wc / (s + wc)}
```

Expected coefficients:

```text
Numerator   = [gain*wc]
Denominator = [wc, 1]
```

Verifies parameter expansion, negative coefficients, and phase inversion.

### Lead-Lag

```spice
.PARAM wz=1k
.PARAM wp=10k
.PARAM k=2
ELEAD out 0 LAPLACE {V(in)} = {k*(1 + s/wz) / (1 + s/wp)}
```

Expected coefficients:

```text
Numerator   = [k, k/wz]
Denominator = [1, 1/wp]
```

### Butterworth Biquad

```spice
.PARAM f0=10k
.PARAM w0={2*pi*f0}
.PARAM q=0.70710678
E2LP out 0 LAPLACE {V(in)} = {w0*w0 / (s*s + s*w0/q + w0*w0)}
```

Expected coefficients:

```text
Numerator   = [w0*w0]
Denominator = [w0*w0, w0/q, 1]
```

### Band-Pass Biquad

```spice
.PARAM f0=10k
.PARAM q=5
.PARAM w0={2*pi*f0}
EBP out 0 LAPLACE {V(in)} = {(s*w0/q) / (s*s + s*w0/q + w0*w0)}
```

Expected coefficients:

```text
Numerator   = [0, w0/q]
Denominator = [w0*w0, w0/q, 1]
```

### Voltage-Controlled Current Source

```spice
.PARAM gm=1m
.PARAM fc=10k
.PARAM wc={2*pi*fc}
GGM out 0 LAPLACE {V(in)} = {gm*wc / (s + wc)}
RLOAD out 0 1k
```

Low frequency expectation:

```text
I(GGM) ~= gm * V(in)
V(out) ~= gm * RLOAD * V(in)
```

This validates `LaplaceVoltageControlledCurrentSource` and source sign convention.

### Singular DC Gain

```spice
EINT out 0 LAPLACE {V(in)} = {1 / s}
```

Expected coefficients:

```text
Numerator   = [1]
Denominator = [0, 1]
```

MVP policy: reject unless a targeted runtime spike proves the supported analyses handle the singular operating-point path safely.

### Improper Transfer

```spice
EDIFF out 0 LAPLACE {V(in)} = {s}
```

Expected coefficients:

```text
Numerator   = [0, 1]
Denominator = [1]
```

MVP policy: reject because `deg N > deg D`. Suggested user workaround:

```spice
.PARAM fp=100meg
.PARAM wp={2*pi*fp}
EDIFF out 0 LAPLACE {V(in)} = {s / (1 + s/wp)}
```

## Sample Netlists

### AC Low-Pass Smoke Test

```spice
* Laplace low-pass AC smoke test
.PARAM tau=1u
VIN in 0 AC 1
ELOW out 0 LAPLACE {V(in)} = {1 / (1 + s*tau)}
RLOAD out 0 1k
.AC DEC 20 10 10meg
.SAVE V(out)
.END
```

Checks:

- Low-frequency magnitude near `1`.
- Cutoff magnitude near `0.70710678`.
- High-frequency rolloff near `-20 dB/decade`.

### AC High-Pass Smoke Test

```spice
* Laplace high-pass AC smoke test
.PARAM fc=1k
.PARAM wc={2*pi*fc}
VIN in 0 AC 1
EHIGH out 0 LAPLACE {V(in)} = {s / (s + wc)}
RLOAD out 0 1k
.AC DEC 20 1 10meg
.SAVE V(out)
.END
```

### Voltage-Controlled Current Source Smoke Test

```spice
* Laplace voltage-controlled current source
.PARAM gm=1m
.PARAM fc=10k
.PARAM wc={2*pi*fc}
VIN in 0 AC 1
GGM out 0 LAPLACE {V(in)} = {gm*wc / (s + wc)}
RLOAD out 0 1k
.AC DEC 20 10 10meg
.SAVE V(out) I(GGM)
.END
```

### Syntax Errors That Should Become Validation Errors

```spice
E1 out 0 LAPLACE
E2 out 0 LAPLACE {V(in)}
E3 out 0 LAPLACE {V(in)} = {}
E4 out 0 LAPLACE {V(in)} = {sin(s)}
E5 out 0 LAPLACE {V(in)} = {s^0.5}
E6 out 0 LAPLACE {V(in)} = {1 / 0}
E7 out 0 LAPLACE {V(a)-V(b)} = {1/(1+s)}
```

## Debugging Guide

When a Laplace test fails, narrow the problem by layer.

### Parser

- Did the lexer keep `{V(in)}` and `{1/(1+s*tau)}` as expression tokens?
- Did continuation-line handling produce one logical statement?
- Did `ParseTreeGenerator` choose `ExpressionAssignment`, `ExpressionEqual`, `ParameterEqual`, or `ParameterSingle`?
- Did `ParseTreeEvaluator` preserve both sides of `=`?

### Source Reader

- Is `LAPLACE` a `WordParameter`?
- Which `ILaplaceSyntaxRecognizer` matched?
- Did a recognized-but-deferred syntax go through `UnsupportedKnownVariantRecognizer`?
- Is the next parameter an `ExpressionAssignmentParameter`?
- Did source type normalize to `e` or `g`?
- Did input parsing produce exactly two control nodes?
- Did entity node creation receive exactly `out+`, `out-`, `control+`, `control-`?

### Math

- Print numerator and denominator before entity creation.
- Confirm coefficient arrays are ascending in `s`.
- Evaluate `H(0)` and `H(j*2*pi*f)` independently in unit tests.
- Confirm interior zeros are preserved.
- Check parameter expansion for hidden `s`.

### Simulation

- Re-run the same circuit with a manually constructed Laplace entity.
- If manual construction works and parser construction fails, inspect node order and coefficient arrays.
- If both fail, inspect properness, singular DC gain, and transient assumptions.

### Generated Code

- Compare generated entities against reader-created entities.
- Confirm same type, node order, coefficients, and delay.
- Confirm invariant-culture coefficient formatting.
- Confirm required namespaces are emitted.

## Key Files

- [src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/VoltageSourceGenerator.cs](../src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/VoltageSourceGenerator.cs)
- [src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/CurrentSourceGenerator.cs](../src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/CurrentSourceGenerator.cs)
- [src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/ExpressionFactory.cs](../src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/ExpressionFactory.cs)
- [src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/ExpressionResolver.cs](../src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/ExpressionResolver.cs)
- [src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/CustomRealBuilder.cs](../src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/CustomRealBuilder.cs)
- [src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/SpiceEvaluationContext.cs](../src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/SpiceEvaluationContext.cs)
- [src/SpiceSharpParser/Parsers/Netlist/Spice/Symbols.cs](../src/SpiceSharpParser/Parsers/Netlist/Spice/Symbols.cs)
- [src/SpiceSharpParser/Parsers/Netlist/Spice/Internals/ParseTreeGenerator.cs](../src/SpiceSharpParser/Parsers/Netlist/Spice/Internals/ParseTreeGenerator.cs)
- [src/SpiceSharpParser/Parsers/Netlist/Spice/Internals/ParseTreeEvaluator.cs](../src/SpiceSharpParser/Parsers/Netlist/Spice/Internals/ParseTreeEvaluator.cs)
- [src/SpiceSharpParser/Parsers/Netlist/Spice/SpiceGrammarBNF.txt](../src/SpiceSharpParser/Parsers/Netlist/Spice/SpiceGrammarBNF.txt)
- [src/SpiceSharpParser/Models/Netlist/Spice/Objects/Parameters/ExpressionEqualParameter.cs](../src/SpiceSharpParser/Models/Netlist/Spice/Objects/Parameters/ExpressionEqualParameter.cs)
- [src/SpiceSharpParser/ModelWriters/CSharp/Entities/Components/SourceWriterHelper.cs](../src/SpiceSharpParser/ModelWriters/CSharp/Entities/Components/SourceWriterHelper.cs)
- [src/docs/articles/behavioral-source.md](../src/docs/articles/behavioral-source.md)

## References

- Cadence PSpice User Guide, LAPLACE ABM semantics: <https://resources.pcb.cadence.com/i/1180526-pspice-user-guide/370>
- Cadence Analog Behavioral Modeling overview: <https://resources.pcb.cadence.com/pspiceuserguide/06-analog-behavioral-modeling>
- Local dependency: `SpiceSharpBehavioral 3.2.0`, including Laplace voltage-controlled and current-controlled source entities plus shared `Numerator`, `Denominator`, and `Delay` parameters.
