---
title: PSpice LAPLACE Support Feasibility
status: Draft / Roadmap
scope: SpiceSharpParser
dependencies: SpiceSharpBehavioral 3.2.0
last_reviewed: 2026-05-01
---

# PSpice `LAPLACE` Support Feasibility

## TL;DR

- Adding PSpice `LAPLACE` support is **feasible** and **moderate** in difficulty for the common `E`/`G` voltage-controlled forms.
- `SpiceSharpBehavioral 3.2.0` already ships `LaplaceVoltageControlledVoltageSource` and `LaplaceVoltageControlledCurrentSource`, so the runtime is largely solved. The work is **parser + coefficient mapping + diagnostics + tests**.
- Recommended first milestone (MVP): map
  ```spice
  E... LAPLACE {V(node)}        = {H(s)}
  E... LAPLACE {V(node1,node2)} = {H(s)}
  G... LAPLACE {V(node)}        = {H(s)}
  G... LAPLACE {V(node1,node2)} = {H(s)}
  ```
  to the built-in components, where `H(s)` is a rational polynomial in `s` with constant coefficients.
- A short AC spike on this repo's test harness already confirmed `1/(1+s*tau)` produces ~0.7071 magnitude and ~-45° phase at the cutoff frequency.

## Table of Contents

1. [Summary](#summary)
2. [Current Status](#current-status)
3. [Built-In SpiceSharpBehavioral Laplace Support](#built-in-spicesharpbehavioral-laplace-support)
4. [Relevant Existing Architecture](#relevant-existing-architecture)
5. [Why LAPLACE Is Harder Than POLY or TABLE](#why-laplace-is-harder-than-poly-or-table)
6. [Parser & Tokenizer Findings](#parser--tokenizer-findings)
7. [Required Math](#required-math)
8. [Implementation Blueprint](#implementation-blueprint)
9. [Worked Transfer-Function Examples](#worked-transfer-function-examples)
10. [Sample Netlists](#sample-netlists)
11. [Edge Cases, Risks & Compatibility](#edge-cases-risks--compatibility)
12. [Implementation Options & Recommendation](#implementation-options--recommendation)
13. [Tests](#tests)
14. [Suggested PR Breakdown](#suggested-pr-breakdown)
15. [Debugging Guide](#debugging-guide)
16. [Key Files](#key-files)
17. [References](#references)

---

## Summary

Implementing PSpice `LAPLACE` in SpiceSharpParser is feasible. The runtime side is easier than it first appeared because `SpiceSharpBehavioral 3.2.0` already contains Laplace-controlled source components and behaviors. The remaining work is therefore:

1. PSpice syntax recognition and grammar disambiguation.
2. A rational-polynomial parser for the transfer expression.
3. Coefficient generation in ascending powers of `s`.
4. Diagnostics for unsupported variants.
5. Reader / generated-code support.
6. Tests at the lexer, parser, math, reader and integration levels.

Difficulty: **moderate** for the recommended `E`/`G` voltage-controlled MVP. **Moderate-to-high** if the first release also targets arbitrary input expressions, current-controlled `F`/`H` variants, or custom runtime behavior beyond the built-in components.

## Current Status

`LAPLACE` is **not** currently parsed by SpiceSharpParser.

The codebase already supports adjacent features: `VALUE={expr}`, `TABLE {expr} = (...)`, `POLY(n)`, behavioral V/I sources, real-valued math functions, and user `.FUNC`. The expression evaluation path, however, is **purely real-valued** (`CustomRealBuilder` returns `double`), which is insufficient for transfer functions in `s`.

Important dependency discovery: `SpiceSharpBehavioral 3.2.0` exposes built-in Laplace source entities and biasing/frequency/time behaviors. This shifts the preferred plan from *"write custom source behaviors"* to *"parse PSpice syntax and map onto existing entities"* for the simple voltage-controlled cases.

## Built-In SpiceSharpBehavioral Laplace Support

Available types:

- `SpiceSharp.Components.LaplaceVoltageControlledVoltageSource`
- `SpiceSharp.Components.LaplaceVoltageControlledCurrentSource`
- `SpiceSharp.Components.LaplaceBehaviors.Biasing`
- `SpiceSharp.Components.LaplaceBehaviors.Frequency`
- `SpiceSharp.Components.LaplaceBehaviors.Time`

Each entity exposes the same constructor shape and parameter model:

```csharp
new LaplaceVoltageControlledVoltageSource(
    name, pos, neg, controlPos, controlNeg,
    numerator, denominator, delay);

// Parameters: Numerator, Denominator, Delay
```

A short AC probe confirmed coefficient arrays are in **ascending powers of `s`**:

```text
1 + tau*s        -> [1, tau]
s + wc           -> [wc, 1]
s^2 + a*s + b    -> [b, a, 1]
```

For `H(s) = 1 / (1 + s*tau)`:

```csharp
numerator   = new[] { 1.0 };
denominator = new[] { 1.0, tau };
delay       = 0.0;
```

At `f = 1/(2*pi*tau)`, this produced the expected ~0.7071 magnitude and -45° phase.

## Relevant Existing Architecture

Behavioral source parsing currently flows through:

- [src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/VoltageSourceGenerator.cs](src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/VoltageSourceGenerator.cs)
- [src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/CurrentSourceGenerator.cs](src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/CurrentSourceGenerator.cs)
- [src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/ArbitraryBehavioralGenerator.cs](src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/ArbitraryBehavioralGenerator.cs)
- [src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/ExpressionFactory.cs](src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/ExpressionFactory.cs)
- [src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/ExpressionResolver.cs](src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/ExpressionResolver.cs)
- [src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/CustomRealBuilder.cs](src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/CustomRealBuilder.cs)
- [src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/SpiceEvaluationContext.cs](src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/SpiceEvaluationContext.cs)

`VALUE`, `POLY`, and `TABLE` are detected at the source-generator level and translated into expressions evaluated by SpiceSharpBehavioral.

## Why `LAPLACE` Is Harder Than `POLY` or `TABLE`

`POLY` and `TABLE` are **static, real-valued** mappings expressible as `f(double) -> double`.

`LAPLACE` is **dynamic** and analysis-dependent:

```spice
E1 out 0 LAPLACE {V(in)} = {1 / (1 + s * tau)}
```

requires:

| Analysis | Behavior |
|----------|----------|
| OP / DC  | Evaluate at `s = 0` |
| AC       | Evaluate at `s = j*omega` (complex) |
| Transient| Realize as state-space ODE |

This cannot be cleanly modeled as `IFunction<double, double>`.

## Parser & Tokenizer Findings

### Tokenizer needs no new token types

`LAPLACE` can remain a normal `WORD` token, becoming a `WordParameter` — matching how `VALUE`, `POLY`, and `TABLE` are handled. Braced expressions are already lexed as expression tokens, and SPICE continuation lines are supported, so:

```spice
ELOW out 0 LAPLACE {V(in)} =
+ {1/(1+s*tau)}
```

should already tokenize as a single logical statement. Add **regression tests** rather than new tokenizer features.

### Parser has a real grammar gap

The natural shape `LeftExpression = RightExpression`:

```spice
LAPLACE {V(in)} = {1/(1+s*tau)}
```

collides with the existing `Expression = Points` grammar used by `TABLE`:

```spice
TABLE {V(in)} = (0,0) (1,1)
```

Today, `{expr} =` is hard-routed through `ExpressionEqual` / `ReadExpressionEqual`, which expects `Points` on the right. So `{V(in)} = {1/(1+s*tau)}` would be forced into the TABLE grammar.

**Recommended fix:** add a new parameter model and parse symbol rather than overloading `ExpressionEqualParameter`:

```text
ExpressionAssignmentParameter { LeftExpression, RightExpression }
Symbols.ExpressionAssignment :  EXPRESSION "=" EXPRESSION
```

Disambiguate in `ParseTreeGenerator.ReadParameter`:

| Lookahead after `{expr}` | Path |
|--------------------------|------|
| `= (`                    | existing `ExpressionEqual` (TABLE) |
| `(`                      | existing no-equals TABLE point list |
| `= {expr}` or `= 'expr'` | new `ExpressionAssignment`         |

This keeps `TABLE` stable and gives `LAPLACE` a parameter shape that says what it means.

**Files to change:**

- [src/SpiceSharpParser/Parsers/Netlist/Spice/Symbols.cs](src/SpiceSharpParser/Parsers/Netlist/Spice/Symbols.cs) — add `ExpressionAssignment`.
- [src/SpiceSharpParser/Parsers/Netlist/Spice/Internals/ParseTreeGenerator.cs](src/SpiceSharpParser/Parsers/Netlist/Spice/Internals/ParseTreeGenerator.cs) — register symbol, add `ReadExpressionAssignment`, update `ReadParameter`.
- [src/SpiceSharpParser/Parsers/Netlist/Spice/Internals/ParseTreeEvaluator.cs](src/SpiceSharpParser/Parsers/Netlist/Spice/Internals/ParseTreeEvaluator.cs) — register evaluator, construct parameter.
- `src/SpiceSharpParser/Models/Netlist/Spice/Objects/Parameters/ExpressionAssignmentParameter.cs` — new parameter model.
- [src/SpiceSharpParser/Parsers/Netlist/Spice/SpiceGrammarBNF.txt](src/SpiceSharpParser/Parsers/Netlist/Spice/SpiceGrammarBNF.txt) — document the production.

**Parser tests to add before any source-generator work:**

1. Lexer keeps `LAPLACE` as `WORD` and both braced expressions as expression tokens.
2. Continuation-line variant remains one logical statement.
3. Parse tree picks `ExpressionAssignment` for `{V(in)} = {1/(1+s*tau)}`.
4. Evaluator yields `LeftExpression == "V(in)"` and `RightExpression == "1/(1+s*tau)"`.
5. Regression: `TABLE {V(in)} = (0,0)` still produces `ExpressionEqualParameter`.
6. Regression: `TABLE {V(in)} (0,0)` still produces `ExpressionEqualParameter`.

> **Rule:** the tokenizer only identifies the braced expression. Transfer-function semantics belong in the Laplace source parser and rational-polynomial parser, never in the lexer.

## Required Math

Model `LAPLACE` as a transfer-function operation:

$$Y(s) = H(s) \cdot X(s), \qquad H(s) = \frac{N(s)}{D(s)}$$

### Polynomial representation

Coefficient array indexed by power, ascending:

```text
N(s) = b0 + b1*s + b2*s^2 + ... + bm*s^m
D(s) = a0 + a1*s + a2*s^2 + ... + an*s^n
```

Required operations: addition, subtraction, multiplication, scalar scaling, integer power, degree, real and complex evaluation, near-zero high-order trimming.

### Rational polynomial algebra

```csharp
record Polynomial(double[] Coefficients);
record RationalPolynomial(Polynomial Numerator, Polynomial Denominator);
```

Identities for AST conversion:

```text
constant c       => c / 1
s                => s / 1
A + B            => (An*Bd + Bn*Ad) / (Ad*Bd)
A - B            => (An*Bd - Bn*Ad) / (Ad*Bd)
A * B            => (An*Bn) / (Ad*Bd)
A / B            => (An*Bd) / (Ad*Bn)
A ^ n            => (An^n) / (Ad^n)   (n is non-negative integer)
unary -A         => (-An) / Ad
```

Accepted subset inside the transfer function: numeric constants, parameters that evaluate to constants, the symbolic `s`, `+ - * /`, integer `^`, parentheses.

Rejected: `sin(s)`, `exp(s)`, `sqrt(s)`, `abs(s)`, `V(...)`, `I(...)` — none reduce to finite rational polynomials.

### Parameter & constant handling

Resolve parameter values **before** polynomial normalization:

```spice
.PARAM tau=1u
E1 out 0 LAPLACE {V(in)} = {1 / (1 + s*tau)}
```

becomes `N(s)=1, D(s)=1 + 1e-6*s`.

`s` is **scoped**: reserved as the symbolic Laplace variable only inside the transfer expression. Outside it, `s` continues to behave as any other parameter/name so existing netlists are unaffected.

A parameter reference is evaluated to a scalar **only when its expansion does not contain `s`**. If `.PARAM pole = {s+1000}`, reject for the MVP rather than silently expanding a hidden symbolic macro.

### DC / Operating Point

```text
H(0) = N(0) / D(0)
```

If `D(0) = 0` (e.g. `1/s`), the MVP should emit a clear validation error rather than guessing.

### AC

Evaluate at `s = j*omega`, `omega = 2*pi*f`. Use **Horner's method**:

```csharp
Complex result = coefficients[degree];
for (var k = degree - 1; k >= 0; k--)
    result = result * s + coefficients[k];
```

Avoid explicit `Complex.Pow(s, n)` per term — slower and less stable.

The runtime evaluation is performed by SpiceSharpBehavioral; the math layer is needed for unit tests, diagnostics, and DC/improperness checks.

### Transient

Transient cannot use `s = j*omega`. The transfer function must become a time-domain ODE. Direct differentiation of the input is numerically poor; use a **state-space realization** (controllable canonical form):

```text
dx/dt = A*x + B*u
y     = C*x + D*u
```

Requirements: properness check (`deg N <= deg D`), feedthrough extraction when degrees are equal, normalization of leading denominator coefficient, internal state, integration with SpiceSharp's transient stepper.

For the MVP, **delegate transient to the built-in `LaplaceBehaviors.Time`** and validate with step-response tests before claiming support.

### Numerical safeguards

- Reject empty or zero polynomials.
- Reject `NaN` / `±Inf` coefficients.
- Trim near-zero **leading** (highest-power) coefficients only — never interior zeros (`s^2 + 1` is `[1, 0, 1]`, not `[1, 1]`).
- Recommended tolerances: `zeroTolerance = 1e-18`, `relativeTolerance = 1e-12 * max(|c|)`.
- Conservative MVP policy: `deg N <= deg D` (reject ideal differentiators).
- Cap order at ~8–10 initially; recommend cascaded biquads for higher orders.

## Implementation Blueprint

The most important design decision: treat `LAPLACE` as a **source-level transfer-function feature**, not as a normal scalar math function. Existing functions like `poly()`, `if()`, `limit()` produce real-valued expression trees; Laplace requires analysis-dependent behavior.

### MVP scope

Supported syntax:

```spice
Ename out+ out- LAPLACE {V(ctrl+)}        = {transfer_expr}
Ename out+ out- LAPLACE {V(ctrl+,ctrl-)}  = {transfer_expr}
Gname out+ out- LAPLACE {V(ctrl+)}        = {transfer_expr}
Gname out+ out- LAPLACE {V(ctrl+,ctrl-)}  = {transfer_expr}
```

Examples:

```spice
ELOW    out 0 LAPLACE {V(in)} = {1 / (1 + s*tau)}
GHIGH   out 0 LAPLACE {V(in)} = {s / (s + 2*pi*fc)}
EBIQUAD out 0 LAPLACE {V(in)} = {w0*w0 / (s*s + s*w0/q + w0*w0)}
```

Deferred until later: `F`/`H` current-controlled variants, `B` source forms, arbitrary input expressions, exotic PSpice aliases, non-rational transfers, explicit IC syntax for transfer state.

### Semantic contract

```text
Output = H(s) * V(controlPos, controlNeg)
H(s)  = Numerator(s) / Denominator(s)
```

For `E`: `Output` is a controlled voltage between `out+`/`out-`.
For `G`: `Output` is a controlled current using SpiceSharp's existing convention.

The parser is responsible for:

- Recognizing `LAPLACE` without disturbing existing forms.
- Extracting output and control nodes.
- Producing finite `double[]` numerator/denominator arrays in ascending order.
- Preserving line info and source name for diagnostics.
- Constructing the built-in entity.

The parser is **not** responsible for matrix stamping, AC complex evaluation at runtime, or solving state-space equations — all delegated to SpiceSharpBehavioral.

### First development spike (do this first)

Before any grammar work, prove three facts in this repo's test harness:

1. `LaplaceVoltageControlledVoltageSource` can be constructed and run through `.AC` from reader code.
2. Same for `LaplaceVoltageControlledCurrentSource`.
3. `Denominator = [1, tau]` produces the expected `1/(1+s*tau)` response.

Spike circuit:

```spice
Laplace smoke test
.PARAM tau=1u
VIN in 0 AC 1
ELOW out 0 LAPLACE {V(in)} = {1/(1+s*tau)}
RLOAD out 0 1k
.AC LIN 1 {1/(2*pi*tau)} {1/(2*pi*tau)}
.MEAS AC vm_cut FIND VM(out) AT={1/(2*pi*tau)}
.MEAS AC vp_cut FIND VP(out) AT={1/(2*pi*tau)}
.END
```

Expected: `VM(out) ~= 0.70710678`, `VP(out) ~= -0.78539816 rad`.

If this fails with a manually constructed entity, **stop and investigate the dependency** before writing grammar code.

### Proposed file layout

New files:

```text
src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/LaplaceSourceParser.cs
src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/LaplaceSourceDefinition.cs
src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/LaplaceSourceInput.cs
src/SpiceSharpParser/Models/Netlist/Spice/Objects/Parameters/ExpressionAssignmentParameter.cs
src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/Laplace/Polynomial.cs
src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/Laplace/RationalPolynomial.cs
src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/Laplace/LaplaceExpressionParser.cs
src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/Laplace/LaplaceTransferFunction.cs
src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/Laplace/LaplaceValidation.cs
```

**Do not** create parser-owned runtime component classes for the MVP — use the SpiceSharpBehavioral built-ins. Custom components are only justified later if arbitrary behavioral input expressions or unsupported source variants are needed.

### Data model

```csharp
internal sealed class Polynomial
{
    public IReadOnlyList<double> Coefficients { get; }
    public int Degree { get; }

    public static Polynomial Zero { get; }
    public static Polynomial One  { get; }
    public static Polynomial S    { get; }

    public Polynomial Add(Polynomial other);
    public Polynomial Subtract(Polynomial other);
    public Polynomial Multiply(Polynomial other);
    public Polynomial Scale(double factor);
    public Polynomial Pow(int exponent);
    public double     EvaluateReal(double value);
    public Complex    EvaluateComplex(Complex value);
    public Polynomial Normalize(double tolerance);
}

internal sealed class RationalPolynomial
{
    public Polynomial Numerator   { get; }
    public Polynomial Denominator { get; }
    public RationalPolynomial Add(RationalPolynomial other);
    public RationalPolynomial Subtract(RationalPolynomial other);
    public RationalPolynomial Multiply(RationalPolynomial other);
    public RationalPolynomial Divide(RationalPolynomial other);
    public RationalPolynomial Pow(int exponent);
    public Complex            EvaluateComplex(Complex value);
}

internal sealed class LaplaceSourceDefinition
{
    public string SourceName { get; }
    public string SourceKind { get; }                 // "e" or "g"
    public string InputExpression { get; }            // raw, for diagnostics
    public LaplaceSourceInput Input { get; }
    public string TransferExpression { get; }         // raw, for diagnostics
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
    public double[]   NumeratorCoefficients   { get; } // ascending in s
    public double[]   DenominatorCoefficients { get; } // ascending in s
    public Complex    EvaluateForTests(Complex s);
}
```

Avoid full polynomial GCD simplification in v1 — not needed for typical SPICE transfers and adds significant complexity.

### Source-generator integration

Add detection beside existing `POLY`/`TABLE` branches in `VoltageSourceGenerator.CreateCustomVoltageSource` and `CurrentSourceGenerator.CreateCustomCurrentSource`:

```csharp
var laplaceParameter = parameters.FirstOrDefault(p => IsWord(p, "laplace", context));
if (laplaceParameter != null)
{
    var parser = new LaplaceSourceParser(context.EvaluationContext, context.Result.ValidationResult);
    var definition = parser.Parse(name, type, parameters, laplaceParameter);
    if (definition == null) return null;
    return CreateLaplaceSourceEntity(name, parameters, context, definition, isVoltageControlled);
}
```

Place the `LAPLACE` branch **before** `POLY` and `TABLE`, but **after** the simple linear `E out 0 in 0 gain` / `G out 0 in 0 gain` fast path.

Do **not** add `laplace()` to `SpiceEvaluationContext.CreateSpiceFunctions()` — that path is for real scalar functions and would hide analysis-dependent behavior.

### Entity creation

```csharp
private IEntity CreateLaplaceSourceEntity(
    string name, ParameterCollection parameters, IReadingContext context,
    LaplaceSourceDefinition definition, bool isVoltageControlled)
{
    if (definition.SourceKind == "e")
    {
        var entity = new LaplaceVoltageControlledVoltageSource(name);
        context.CreateNodes(entity, CreateLaplaceNodeParameters(parameters, definition));
        entity.Parameters.Numerator   = definition.TransferFunction.NumeratorCoefficients;
        entity.Parameters.Denominator = definition.TransferFunction.DenominatorCoefficients;
        entity.Parameters.Delay       = definition.Delay;
        return entity;
    }
    if (definition.SourceKind == "g")
    {
        var entity = new LaplaceVoltageControlledCurrentSource(name);
        context.CreateNodes(entity, CreateLaplaceNodeParameters(parameters, definition));
        entity.Parameters.Numerator   = definition.TransferFunction.NumeratorCoefficients;
        entity.Parameters.Denominator = definition.TransferFunction.DenominatorCoefficients;
        entity.Parameters.Delay       = definition.Delay;
        return entity;
    }

    context.Result.ValidationResult.AddError(
        ValidationEntrySource.Reader,
        "laplace is not supported for this source type yet",
        parameters.LineInfo);
    return null;
}
```

`CreateLaplaceNodeParameters` builds exactly four node parameters in order `out+, out-, control+, control-` so `context.CreateNodes` performs subcircuit expansion and name generation in one place. **Do not** bypass `CreateNodes` with raw node names.

### Input-probe parsing

Accept only `V(node)` or `V(node1,node2)` for the MVP:

| Input         | controlPos | controlNeg |
|---------------|------------|------------|
| `V(in)`       | `in`       | `0`        |
| `v(in,0)`     | `in`       | `0`        |
| `V(n001,n002)`| `n001`     | `n002`     |

Reject (with specific diagnostics): `V(a)-V(b)`, `I(Vsense)`, `V(a,b,c)`, `V(a+1)`, `V({node})`.

Prefer parsing via the existing expression lexer/parser and accepting only the `V(...)` AST shape — avoid loose regexes that admit malformed input.

### Transfer-expression parser

Reuse `SpiceSharpParser.Lexers.Expressions.Lexer` + `SpiceSharpParser.Parsers.Expression.Parser` for AST creation. Use a **new builder** that returns `RationalPolynomial`, not `double` — this prevents symbolic `s` from leaking into normal scalar evaluation.

Builder rules:

```text
number     -> RationalPolynomial(c, 1)
parameter  -> evaluate (must be s-free) -> RationalPolynomial(c, 1)
s          -> RationalPolynomial(s, 1)
-x         -> -Build(x)
x op y     -> Build(x).Op(Build(y))   for + - * /
x ^ n      -> Build(x).Pow(n)         only non-negative integer n
```

Reject in MVP: non-integer powers, negative powers (write `1/s` instead), functions of `s`, voltage/current/property probes, stochastic functions, conditionals.

### Diagnostics

Specific messages improve user experience and debugging:

```text
laplace expects input expression
laplace input expression must be V(node) or V(node1,node2)
laplace expects transfer expression after =
laplace transfer expression must be a rational polynomial in s
laplace transfer denominator cannot be zero
laplace transfer function is improper; numerator degree exceeds denominator degree
laplace transfer function has singular DC gain
laplace transfer coefficients must be constant expressions
laplace transfer expression reserves symbol 's'; use a different parameter name
laplace delay is not supported yet
F1: laplace source supports only E and G voltage-controlled forms in this version
```

Always attach source name and `SpiceLineInfo`.

### Edge cases in the source generator

- Branch order: `LAPLACE` before `POLY`/`TABLE`, after simple linear path.
- `M=` multiplier: ignore in v1 unless tests pin down semantics; if added, multiply the **numerator coefficients** by `M`, do not wrap in a behavioral expression.
- For `F`/`H`/`V`/`I`/`B` with `LAPLACE`, emit a targeted unsupported diagnostic.

### Generated C# writer

Add **after** runtime support is verified, not before. Emit:

```csharp
var eLow = new LaplaceVoltageControlledVoltageSource("ELOW");
eLow.Connect("out", "0", "in", "0");
eLow.Parameters.Numerator   = new[] { 1.0 };
eLow.Parameters.Denominator = new[] { 1.0, 1e-6 };
eLow.Parameters.Delay       = 0.0;
circuit.Add(eLow);
```

Use **invariant culture** for coefficient formatting (test with `1e-6` to catch comma-decimal regressions). Reuse `LaplaceSourceParser` in writer mode if `Component.PinsAndParameters` is the source of truth, to avoid duplicating parser logic.

### Documentation

Update [src/docs/articles/behavioral-source.md](src/docs/articles/behavioral-source.md) **after** implementation. Document supported syntax, source types, verified analyses (OP/AC, plus TRAN if step-response tests pass), allowed expression subset, allowed input subset, treatment of `s`, known limitations, and worked examples (low-pass, high-pass, biquad). If transient is unverified, **state that explicitly** — silent partial compatibility is painful for users importing PSpice models.

## Worked Transfer-Function Examples

Useful for both implementation tests and validating coefficient normalization.

### First-order low-pass

```spice
.PARAM tau=1u
ELOW out 0 LAPLACE {V(in)} = {1 / (1 + s*tau)}
```

`H(s) = 1/(1+tau*s)` → `Numerator=[1]`, `Denominator=[1, tau]`. DC gain 1; pole at `1/tau`; at `f = 1/(2*pi*tau)`: ~ -3.0103 dB, -45°. **First AC integration test.**

### Differential-input low-pass

```spice
ELOW out 0 LAPLACE {V(inp,inn)} = {1 / (1 + s*tau)}
```

→ `controlPos = inp, controlNeg = inn`. Same coefficients. Verifies that `V(a,b)` is not assumed single-ended.

### First-order high-pass

```spice
.PARAM fc=1k
.PARAM wc={2*pi*fc}
EHIGH out 0 LAPLACE {V(in)} = {s / (s + wc)}
```

→ `Numerator=[0, 1]`, `Denominator=[wc, 1]`. Verifies a numerator with zero constant term is **not** trimmed.

### Inverting / scaled low-pass

```spice
.PARAM gain=-10
.PARAM fc=10k
.PARAM wc={2*pi*fc}
EAMP out 0 LAPLACE {V(in)} = {gain*wc / (s + wc)}
```

→ `Numerator=[gain*wc]`, `Denominator=[wc, 1]`. Verifies parameter expansion, negative coefficients, 180° phase from negative real gain.

### Lead-lag

```spice
.PARAM wz=1k  wp=10k  k=2
ELEAD out 0 LAPLACE {V(in)} = {k*(1 + s/wz) / (1 + s/wp)}
```

→ `Numerator=[k, k/wz]`, `Denominator=[1, 1/wp]`. Verifies division by parameter constants and rational multiplication.

### Second-order low-pass (Butterworth biquad)

```spice
.PARAM f0=10k
.PARAM w0={2*pi*f0}
.PARAM q=0.70710678
E2LP out 0 LAPLACE {V(in)} = {w0*w0 / (s*s + s*w0/q + w0*w0)}
```

→ `Numerator=[w0*w0]`, `Denominator=[w0*w0, w0/q, 1]`. DC gain 1; ~ -3 dB at `f0`.

### Second-order band-pass

```spice
.PARAM f0=10k  q=5
.PARAM w0={2*pi*f0}
EBP out 0 LAPLACE {V(in)} = {(s*w0/q) / (s*s + s*w0/q + w0*w0)}
```

→ `Numerator=[0, w0/q]`, `Denominator=[w0*w0, w0/q, 1]`. Verifies missing powers in both numerator and denominator.

### Voltage-controlled current source

```spice
.PARAM gm=1m  fc=10k
.PARAM wc={2*pi*fc}
GGM out 0 LAPLACE {V(in)} = {gm*wc / (s + wc)}
RLOAD out 0 1k
```

Low-frequency: `I(GGM) ~= gm*V(in)`, `V(out) ~= gm*RLOAD*V(in)`. Validates `LaplaceVoltageControlledCurrentSource`, sign convention, current-source loading.

### Integrator (singular DC)

```spice
EINT out 0 LAPLACE {V(in)} = {1 / s}
```

→ `Numerator=[1]`, `Denominator=[0, 1]`. `D(0) = 0`. **MVP recommendation:** parser accepts the algebra; reader emits a clear validation error for singular DC gain unless a targeted spike proves SpiceSharpBehavioral handles it.

### Differentiator (improper)

```spice
EDIFF out 0 LAPLACE {V(in)} = {s}
```

→ `Numerator=[0, 1]`, `Denominator=[1]`. Improper (`deg N > deg D`), unbounded at high frequency. **Reject** with `laplace transfer function is improper`. Suggested user workaround:

```spice
.PARAM fp=100meg
.PARAM wp={2*pi*fp}
EDIFF out 0 LAPLACE {V(in)} = {s / (1 + s/wp)}
```

### Delay

PSpice syntax for explicit delay is not standardized across examples. Built-in components expose a `Delay` parameter for future mapping. **MVP:** `Delay = 0.0`. Reject `TD=`, `DELAY=`, or extra trailing args until syntax and semantics are confirmed by tests.

### Arbitrary input expression (deferred)

Future lowering for `E1 out 0 LAPLACE {V(a)-V(b)+0.5*I(Vsense)} = {1/(1+s*tau)}`:

```spice
B__laplace_in_E1 n__laplace_E1 0 V={V(a)-V(b)+0.5*I(Vsense)}
E1 out 0 LAPLACE {V(n__laplace_E1)} = {1/(1+s*tau)}
```

Not for the MVP — generated helper names must go through the existing name generator and avoid collisions.

## Sample Netlists

### AC low-pass smoke test

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

Checks: low-f magnitude ~1; at `1/(2*pi*tau)` magnitude ~0.7071; -20 dB/decade rolloff at high f.

### AC high-pass smoke test

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

### Voltage-controlled current source

```spice
* Laplace voltage-controlled current source
.PARAM gm=1m  fc=10k
.PARAM wc={2*pi*fc}
VIN in 0 AC 1
GGM out 0 LAPLACE {V(in)} = {gm*wc / (s + wc)}
RLOAD out 0 1k
.AC DEC 20 10 10meg
.SAVE V(out) I(GGM)
.END
```

### Syntax-error examples (must produce validation errors, not parser exceptions)

```spice
E1 out 0 LAPLACE
E2 out 0 LAPLACE {V(in)}
E3 out 0 LAPLACE {V(in)} = {}
E4 out 0 LAPLACE {V(in)} = {sin(s)}
E5 out 0 LAPLACE {V(in)} = {s^0.5}
E6 out 0 LAPLACE {V(in)} = {1 / 0}
```

## Edge Cases, Risks & Compatibility

### Compatibility decision table

| Feature                                         | MVP behavior                       | Later behavior |
|--|--|--|
| `E ... LAPLACE {V(n)} = {H(s)}`                  | **Support**                        | Keep |
| `E ... LAPLACE {V(n1,n2)} = {H(s)}`              | **Support**                        | Keep |
| `G ... LAPLACE {V(n)} = {H(s)}`                  | **Support**                        | Keep |
| `G ... LAPLACE {V(n1,n2)} = {H(s)}`              | **Support**                        | Keep |
| `F`/`H` current-controlled `LAPLACE`             | Reject with diagnostic             | Custom lowering or current-controlled components |
| `B` source `LAPLACE` syntax                      | Reject with diagnostic             | Investigate PSpice ABM semantics |
| Arbitrary input expression                       | Reject with diagnostic             | Lower through helper source or custom behavior |
| Rational polynomial in `s`                       | **Support**                        | Keep |
| Non-rational functions of `s` (`sin(s)`, …)      | Reject                             | Likely never |
| Singular DC gain (`1/s`, …)                      | Test, then reject or document      | Possible AC-only / IC-aware support |
| Improper transfer (`deg N > deg D`)              | Reject                             | Allow only if proven safe |
| Explicit delay (`TD=`, …)                        | Reject                             | Map to `Delay` after syntax confirmation |
| Transient                                        | Test before claiming               | Document verified subset |

> Good diagnostics are part of compatibility. A rejected feature with a precise message beats falling through to a misleading error.

### Known risks & their mitigations

**Multiple PSpice syntax variants in the wild.**
Implement one documented form first. Add variants only when tests prove the first path works. Emit targeted diagnostics for recognized-but-unsupported variants. Possible later forms (do not guess semantics):

```spice
E1 out 0 LAPLACE {V(in)} {1/(1+s*tau)}
E1 out 0 LAPLACE = {V(in)} {1/(1+s*tau)}
E1 out 0 VALUE = {LAPLACE(V(in), 1/(1+s*tau))}
```

**`s` may collide with a user parameter.**
Treat `s` as symbolic only inside the Laplace transfer expression; normal parameter elsewhere. If a user names a parameter `s` and uses it in a transfer, require renaming. Diagnostic: `laplace transfer expression reserves symbol 's'; use a different parameter name`.

**Coefficients depending on circuit values (`V(...)` inside transfer).**
Reject in v1: `laplace transfer coefficients must be constant expressions`. Truly dynamic coefficients are a much larger nonlinear-dynamic-system feature.

**Singular DC gain.**
Detect `|D(0)| < tolerance`. Spike `1/s` against the built-in component before final policy. Conservative MVP: reject with diagnostic if OP setup is unsafe.

**Improper transfer functions.**
Compare degrees; reject when `deg N > deg D`. Suggest a high-frequency pole.

**Numerically fragile high-order transfers.**
Cap order at ~8–10. Warn on high order. Recommend cascaded biquads.

**Coefficient overflow/underflow.**
Use Horner evaluation, normalize leading denominator coefficient when safe (verify it does not change built-in component behavior). Test both low- and high-frequency evaluation.

**Input expression more complex than `V(node)`/`V(node1,node2)`.**
Reject in v1; later, lower through a generated helper behavioral source.

**`FREQ` parameter is real.**
Updating a real `FREQ` parameter is **not** equivalent to true Laplace behavior. Use the built-in frequency behavior with complex stamping. Don't represent `H(s)` as a real expression of `FREQ`.

**AC needs an operating point.**
For singular-DC transfers the OP may fail. Spike before deciding; reject with a clear message if unreliable.

**Transient initial conditions are ambiguous.**
Verify built-in `LaplaceBehaviors.Time` with step-response tests before documenting transient. Define IC semantics (zero / OP / explicit) based on observed behavior. Future syntax `IC=0` only after PSpice compatibility check.

**Errors hard to trace across layers.**
Always carry source name, raw input/transfer strings, and `SpiceLineInfo` in `LaplaceSourceDefinition`. Prefer validation errors over generic exceptions for user mistakes.

## Implementation Options & Recommendation

| Option | Scope | Effort |
|--------|-------|--------|
| **1. Syntax recognition only** | Parse `LAPLACE`, validate, report unsupported | 0.5–1 day |
| **2. Built-in component MVP** *(recommended)* | `E`/`G` with `V(node)` or `V(node1,node2)` input, rational `H(s)`, OP/AC tests, diagnostics | 2–4 engineering days |
| **3. Full PSpice-compatible AC + TRAN** | Plus broader syntax, possibly arbitrary input expressions, transient validation, generated-code support | 1–3 weeks |

### Recommendation

Implement in phases. Start with the **built-in component MVP** for `E`/`G` voltage-controlled forms. This delivers useful PSpice compatibility while leaning on SpiceSharpBehavioral for analysis-specific runtime behavior. Validate transient, generated C# output, and broader input-expression support as separate follow-up milestones.

### Recommended implementation plan (ordered)

1. Freeze MVP syntax contract (`E`/`G` with `V(node)` / `V(node1,node2)` and rational `H(s)`).
2. Add tokenizer regression tests (`LAPLACE` is `WORD`, braced exprs intact, continuation lines preserved).
3. Add parse-tree gap test: `{V(in)} = {1/(1+s*tau)}` must not be parsed as TABLE points.
4. Add `ExpressionAssignmentParameter` (`LeftExpression`, `RightExpression`).
5. Add `Symbols.ExpressionAssignment` and document in `SpiceGrammarBNF.txt`.
6. Update `ParseTreeGenerator.ReadParameter` so `{expr} = {expr}` routes to `ExpressionAssignment`; `{expr} = (` and `{expr} (` keep the `ExpressionEqual` (TABLE) path.
7. Update `ParseTreeEvaluator` to construct `ExpressionAssignmentParameter`. Add TABLE regression test.
8. Add `LaplaceSourceParser` near source generators. Find `WordParameter("laplace")`, require next parameter to be `ExpressionAssignmentParameter`, preserve line info.
9. Parse input expression with the existing expression parser; accept only `V(node)` and the AST shape produced by `V(node1,node2)`.
10. Reject arbitrary input expressions with a clear validation error.
11. Add Laplace transfer-expression builder reusing the expression lexer/parser but **not** `CustomRealBuilder`.
12. Support: constants, parameters, `s`, unary signs, `+`, `-`, `*`, `/`, non-negative integer `^`.
13. Reject: functions, V/I/property nodes, `TIME`, `FREQ`, non-integer powers, negative powers, non-constant coefficients, zero denominator, improper transfers.
14. Normalize to numerator/denominator coefficient arrays in ascending powers of `s`.
15. Add source-generator detection before `POLY`/`TABLE` branches.
16. Map `E` → `LaplaceVoltageControlledVoltageSource` with node order `out+, out-, control+, control-`.
17. Map `G` → `LaplaceVoltageControlledCurrentSource` with the same order; explicit `M=` decision.
18. Set `Numerator`, `Denominator`, `Delay = 0.0`.
19. Add OP/AC integration tests (low-pass, high-pass, biquad, parameterized, differential input).
20. Add malformed-syntax and unsupported-feature diagnostic tests.
21. Add transient step-response tests **only before** claiming transient compatibility.
22. Update generated C# writer and user docs after runtime is verified.

## Tests

### Math unit tests

- `Polynomial.Add/Subtract/Multiply/Scale/Pow`, trimming, degree, real/complex evaluation.
- Rational add/multiply/divide/power, denominator-zero rejection.
- Worked-example coefficient generation:
  - `1/(1+s*tau)` → `[1]`, `[1, tau]`
  - `s/(s+wc)` → `[0, 1]`, `[wc, 1]`
  - `w0*w0/(s*s+s*w0/q+w0*w0)` → `[w0*w0]`, `[w0*w0, w0/q, 1]`
  - `(1+s/wz)/(1+s/wp)` → rational mult/div with parameters.
- Reject: `s^0.5`, `s^-1`, `sin(s)`, `exp(s)`, `V(x)*s`, `random()*s`.

### Parser tests (place in [src/SpiceSharpParser.Tests/Parsers](src/SpiceSharpParser.Tests/Parsers))

- Tokenizer regression for `LAPLACE` keyword, braced RHS, continuation lines.
- Parse-tree generator picks `ExpressionAssignment`.
- Evaluator preserves both sides intact.
- TABLE regressions unchanged.

### Source-parser tests

- Valid `E` and `G` syntax.
- Missing input expression / equals / transfer expression.
- Malformed braces.
- Unsupported source-type diagnostics for `F`, `H`, `B`.

### Integration tests (place beside existing `PolyTests.cs`, `TableTests.cs` under `AnalogBehavioralModeling/`)

- New `LaplaceTests.cs`.
- `E` low-pass OP gain ~1.
- `G` low-pass transconductance through resistor load.
- Unsupported `V(a)-V(b)` input → validation error.
- Unsupported source type → targeted error.

### AC tests (use `.MEAS AC VM/VP` patterns from [VoltageExportTests.cs](src/SpiceSharpParser.IntegrationTests/VoltageExportTests.cs))

- `1/(1+s*tau)` at cutoff: magnitude ~0.7071, phase ~ -π/4.
- `s/(s+wc)` at `fc`: magnitude ~0.7071, phase ~ +π/4.
- Butterworth biquad at `f0`: magnitude ~ -3 dB.

### Generated-code tests

- Generate C# for a low-pass `E` source; assert emitted code uses `LaplaceVoltageControlledVoltageSource`, ascending coefficient arrays, invariant-culture formatting (e.g. `1e-6`).

### Transient tests

- `1/(1+s*tau)` step response vs. `1 - exp(-t/tau)` at several times.
- Loose tolerances initially. If results don't match, document OP/AC-only support and isolate transient work.

### Test templates (existing files for reference)

- [src/SpiceSharpParser.Tests/Parsers/](src/SpiceSharpParser.Tests/Parsers)
- [src/SpiceSharpParser.IntegrationTests/AnalogBehavioralModeling/](src/SpiceSharpParser.IntegrationTests/AnalogBehavioralModeling)
- [src/SpiceSharpParser.IntegrationTests/VoltageExportTests.cs](src/SpiceSharpParser.IntegrationTests/VoltageExportTests.cs)
- [src/SpiceSharpParser.IntegrationTests/Expressions/](src/SpiceSharpParser.IntegrationTests/Expressions)
- [src/SpiceSharpParser.IntegrationTests/DotStatements/](src/SpiceSharpParser.IntegrationTests/DotStatements)

## Suggested PR Breakdown

| PR | Contents |
|----|----------|
| **1. Grammar & diagnostics** | Lexer tests, expression-to-expression grammar tests, `ExpressionAssignmentParameter`, `Symbols.ExpressionAssignment`, evaluator support, TABLE regression tests, validation error for unsupported `LAPLACE`. |
| **2. Math parser** | `Polynomial`, `RationalPolynomial`, `LaplaceExpressionParser`, normalization & validation tests. |
| **3. `E` source mapping** | Input-probe parser, `LaplaceVoltageControlledVoltageSource` creation, OP/AC tests for low-pass and high-pass. |
| **4. `G` source mapping** | `LaplaceVoltageControlledCurrentSource` creation, current-source sign / loading tests. |
| **5. Compatibility polish** | Singular-DC and improper-transfer diagnostics, transient validation or explicit doc gap, generated C# writer, user docs. |

## Debugging Guide

When a Laplace test fails, narrow by layer.

**Parser:**

- Did the lexer emit `{V(in)}` and `{1/(1+s*tau)}` as expression tokens, or split them?
- Did `ParseTreeGenerator` choose `ExpressionAssignment`, `ExpressionEqual`, `ParameterEqual`, or `ParameterSingle`?
- Did `ParseTreeEvaluator` preserve both sides of `=`?

**Source reader:**

- Inspect `ParameterCollection.ToString()` for the component.
- Confirm `LAPLACE` is a `WordParameter` and was not swallowed by the output pins.
- Confirm source type is `e`/`g` after case handling.
- Confirm `CreateLaplaceNodeParameters` builds exactly four node parameters.

**Math:**

- Print numerator and denominator before entity creation.
- Verify ascending powers of `s`.
- Evaluate the rational polynomial at `s=0` and `s=j*2*pi*fc` in a unit test.
- Check no interior zeros were trimmed (they must be preserved).

**Simulation:**

- Re-run the same circuit with a manually constructed entity.
- If manual works but parser construction fails, inspect node ordering and coefficient arrays.
- If both fail, investigate properness, singular DC, transient assumptions in SpiceSharpBehavioral.

**Generated code:**

- Compare against parser-created entity: same type, node order, coefficients, delay.
- Invariant-culture formatting for coefficients.
- Required namespace included.

## Key Files

- [src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/VoltageSourceGenerator.cs](src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/VoltageSourceGenerator.cs)
- [src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/CurrentSourceGenerator.cs](src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/CurrentSourceGenerator.cs)
- [src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/ExpressionFactory.cs](src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/ExpressionFactory.cs)
- [src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/ExpressionResolver.cs](src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/ExpressionResolver.cs)
- [src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/CustomRealBuilder.cs](src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/CustomRealBuilder.cs)
- [src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/SpiceEvaluationContext.cs](src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/SpiceEvaluationContext.cs)
- [src/SpiceSharpParser/Parsers/Netlist/Spice/Symbols.cs](src/SpiceSharpParser/Parsers/Netlist/Spice/Symbols.cs)
- [src/SpiceSharpParser/Parsers/Netlist/Spice/Internals/ParseTreeGenerator.cs](src/SpiceSharpParser/Parsers/Netlist/Spice/Internals/ParseTreeGenerator.cs)
- [src/SpiceSharpParser/Parsers/Netlist/Spice/Internals/ParseTreeEvaluator.cs](src/SpiceSharpParser/Parsers/Netlist/Spice/Internals/ParseTreeEvaluator.cs)
- [src/SpiceSharpParser/Models/Netlist/Spice/Objects/Parameters/ExpressionEqualParameter.cs](src/SpiceSharpParser/Models/Netlist/Spice/Objects/Parameters/ExpressionEqualParameter.cs)
- [src/SpiceSharpParser/ModelWriters/CSharp/Entities/Components/SourceWriterHelper.cs](src/SpiceSharpParser/ModelWriters/CSharp/Entities/Components/SourceWriterHelper.cs)
- [src/docs/articles/behavioral-source.md](src/docs/articles/behavioral-source.md)

## References

- Cadence PSpice User Guide, LAPLACE ABM semantics: <https://resources.pcb.cadence.com/i/1180526-pspice-user-guide/370>
- Cadence Analog Behavioral Modeling overview: <https://resources.pcb.cadence.com/pspiceuserguide/06-analog-behavioral-modeling>
- Local dependency: `SpiceSharpBehavioral 3.2.0` exposes `LaplaceVoltageControlledVoltageSource`, `LaplaceVoltageControlledCurrentSource`, and shared `Numerator` / `Denominator` / `Delay` parameters, plus `LaplaceBehaviors.Biasing` / `Frequency` / `Time`.
