# PSpice LAPLACE Support Feasibility

## Summary

Implementing PSpice `LAPLACE` support in SpiceSharpParser is feasible, and the runtime side is easier than it first looked because `SpiceSharpBehavioral 3.2.0` already contains Laplace-controlled source components. The main implementation work is therefore parser integration, PSpice syntax normalization, transfer-expression parsing, coefficient generation, diagnostics, generated-code support, and tests.

Recommended difficulty estimate: **moderate** for the common `E` and `G` voltage-controlled forms where the input is `V(node)` or `V(node1,node2)`. It becomes **moderate to high** only if the first release also tries to support arbitrary input expressions, current-controlled `F`/`H` variants, or custom runtime behavior beyond the built-in SpiceSharpBehavioral components.

## Current Status

`LAPLACE` is not currently parsed by SpiceSharpParser.

The codebase already supports:

- `VALUE={expr}`
- `TABLE {expr} = (...)`
- `POLY(n)`
- Behavioral voltage and current sources
- Real-valued math functions
- User-defined `.FUNC` expressions

Important dependency discovery: `SpiceSharpBehavioral 3.2.0`, which this parser already uses for behavioral sources, includes built-in Laplace source entities and biasing/frequency/time behaviors. That changes the preferred plan from "write custom source behaviors" to "parse PSpice syntax and map it onto existing SpiceSharpBehavioral entities" for the simple voltage-controlled cases.

## Built-In SpiceSharpBehavioral Laplace Support

The package already exposes these entities:

- `SpiceSharp.Components.LaplaceVoltageControlledVoltageSource`
- `SpiceSharp.Components.LaplaceVoltageControlledCurrentSource`

Each entity has a constructor with this shape:

```csharp
new LaplaceVoltageControlledVoltageSource(
    name,
    pos,
    neg,
    controlPos,
    controlNeg,
    numerator,
    denominator,
    delay);

new LaplaceVoltageControlledCurrentSource(
    name,
    pos,
    neg,
    controlPos,
    controlNeg,
    numerator,
    denominator,
    delay);
```

The shared parameter model exposes:

- `Numerator`
- `Denominator`
- `Delay`

The shared behavior layer includes:

- `SpiceSharp.Components.LaplaceBehaviors.Biasing`
- `SpiceSharp.Components.LaplaceBehaviors.Frequency`
- `SpiceSharp.Components.LaplaceBehaviors.Time`

A small AC probe confirmed that the coefficient arrays are in **ascending powers of `s`**:

```text
1 + tau*s -> [1, tau]
s + wc    -> [wc, 1]
s^2 + a*s + b -> [b, a, 1]
```

For example, `H(s) = 1 / (1 + s*tau)` maps to:

```csharp
numerator = new[] { 1.0 };
denominator = new[] { 1.0, tau };
delay = 0.0;
```

At `f = 1 / (2*pi*tau)`, that mapping produced the expected AC result of about `0.7071` magnitude and `-45` degrees phase.

## Relevant Existing Architecture

Behavioral source parsing currently flows through these areas:

- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/VoltageSourceGenerator.cs`
- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/CurrentSourceGenerator.cs`
- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/ArbitraryBehavioralGenerator.cs`
- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/ExpressionFactory.cs`
- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/ExpressionResolver.cs`
- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/CustomRealBuilder.cs`
- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/SpiceEvaluationContext.cs`

`VALUE`, `POLY`, and `TABLE` are detected at the source-generator level and converted into expressions that SpiceSharpBehavioral can evaluate.

However, the expression evaluation path is currently real-valued. `CustomRealBuilder` evaluates expressions as `double`, which is not enough for Laplace-domain transfer functions.

## Why LAPLACE Is Harder Than POLY Or TABLE

`POLY` and `TABLE` are static nonlinear mappings. They can be expressed as real-valued functions of voltage, current, or parameters.

`LAPLACE` is dynamic. It requires behavior such as:

```spice
E1 out 0 LAPLACE {V(in)} = {1 / (1 + s * tau)}
```

That means the implementation must understand:

- The input expression, such as `V(in)`
- A transfer function in `s`
- AC evaluation at `s = jw`
- DC/OP behavior at `s = 0`
- Transient behavior, if full PSpice compatibility is desired

This cannot be cleanly implemented as a simple `IFunction<double, double>`.

## Math Required For LAPLACE Support

The implementation needs to model `LAPLACE` as a transfer-function operation:

```text
Y(s) = H(s) * X(s)
H(s) = N(s) / D(s)
```

where `X(s)` is the Laplace transform of the input expression, `Y(s)` is the output contribution, and `H(s)` is the transfer function supplied by the netlist.

### Polynomial Representation

Use a polynomial representation in powers of `s`:

```text
N(s) = b0 + b1*s + b2*s^2 + ... + bm*s^m
D(s) = a0 + a1*s + a2*s^2 + ... + an*s^n
```

A practical internal representation is a coefficient array indexed by power:

```text
coefficients[0] = constant term
coefficients[1] = s term
coefficients[2] = s^2 term
```

The implementation needs:

- Polynomial normalization and trimming of near-zero highest-order coefficients
- Polynomial addition and subtraction
- Polynomial multiplication
- Polynomial scaling by a real coefficient
- Polynomial integer powers for expressions such as `s^2`
- Polynomial degree calculation
- Constant extraction from parameter-only expressions

### Rational Polynomial Algebra

The Laplace expression should be parsed into a rational polynomial:

```text
R(s) = P(s) / Q(s)
```

A useful model is:

```csharp
record Polynomial(double[] Coefficients);
record RationalPolynomial(Polynomial Numerator, Polynomial Denominator);
```

Each supported expression node can be converted using these identities:

```text
constant c       => c / 1
s                => s / 1
A + B            => (An*Bd + Bn*Ad) / (Ad*Bd)
A - B            => (An*Bd - Bn*Ad) / (Ad*Bd)
A * B            => (An*Bn) / (Ad*Bd)
A / B            => (An*Bd) / (Ad*Bn)
A ^ n            => (An^n) / (Ad^n), where n is a non-negative integer
unary -A         => (-An) / Ad
```

Only a limited expression subset should be accepted inside the transfer function at first:

- Numeric constants
- Parameters that evaluate to numeric constants
- The symbolic variable `s`
- Operators `+`, `-`, `*`, `/`, and integer `^`
- Parentheses

Functions such as `sin(s)`, `exp(s)`, `sqrt(s)`, `abs(s)`, `V(...)`, and `I(...)` inside the transfer function should be rejected unless there is a deliberate later design for them. They do not generally reduce to finite rational polynomials.

### Parameter And Constant Handling

Parameter names in the transfer expression must be evaluated before polynomial normalization:

```spice
.PARAM tau=1u
E1 out 0 LAPLACE {V(in)} = {1 / (1 + s*tau)}
```

This becomes:

```text
N(s) = 1
D(s) = 1 + 1e-6*s
```

The symbol `s` should be reserved only inside the Laplace transfer expression. Outside that expression, `s` should continue to behave according to normal parameter/name rules so existing netlists are not broken.

### DC And Operating Point Math

For DC and operating point behavior, the basic evaluation is:

```text
s = 0
H(0) = N(0) / D(0)
```

This gives the DC gain. For example:

```text
H(s) = 1 / (1 + s*tau)
H(0) = 1
```

The implementation must handle singular DC cases clearly:

```text
H(s) = 1 / s
D(0) = 0
```

For a first implementation, singular DC gain should probably produce a validation error or an explicit unsupported diagnostic. A more advanced implementation could compute limits as `s -> 0`, but that adds extra algebra and still leaves difficult operating-point behavior for pure integrators.

### AC Analysis Math

For AC analysis, evaluate the transfer function at:

```text
s = j * omega
omega = 2 * pi * frequency
```

Then:

```text
H(j*omega) = N(j*omega) / D(j*omega)
Y(j*omega) = H(j*omega) * X(j*omega)
```

This requires:

- Complex-number arithmetic
- Complex polynomial evaluation
- Complex division
- Near-zero denominator checks
- Magnitude and phase extraction for tests and exports

Horner's method is the preferred way to evaluate polynomials:

```text
P(s) = c0 + c1*s + c2*s^2 + ... + cn*s^n

result = cn
for k = n - 1 down to 0:
    result = result * s + ck
```

For example:

```text
H(s) = 1 / (1 + s*tau)
H(j*omega) = 1 / (1 + j*omega*tau)
```

This is the math needed to verify coefficient generation and AC behavior, even when the simulation runtime is delegated to the built-in SpiceSharpBehavioral Laplace components.

### Transient Analysis Math

Transient support is harder because `s` cannot be replaced by `j*omega`. The transfer function must be converted into a time-domain differential equation or state-space model.

Starting from:

```text
H(s) = (b0 + b1*s + ... + bm*s^m) / (a0 + a1*s + ... + an*s^n)
Y(s) = H(s) * X(s)
```

Move the denominator to the left side:

```text
(a0 + a1*s + ... + an*s^n) * Y(s)
=
(b0 + b1*s + ... + bm*s^m) * X(s)
```

In the time domain, powers of `s` correspond to derivatives:

```text
a0*y(t) + a1*dy/dt + ... + an*d^n y/dt^n
=
b0*x(t) + b1*dx/dt + ... + bm*d^m x/dt^m
```

Directly differentiating the input expression is usually a poor numerical strategy. The better approach is to realize the transfer function as internal state variables:

```text
dx_state/dt = A*x_state + B*u
y          = C*x_state + D*u
```

where `u` is the input expression value and `y` is the source output contribution.

Transient support therefore needs:

- Transfer-function normalization
- Properness checks: numerator degree should be less than or equal to denominator degree
- Polynomial division or feedthrough extraction when numerator degree equals denominator degree
- Rejection or special handling when numerator degree is greater than denominator degree
- Conversion from transfer function coefficients to state-space matrices
- Internal state variables for the source
- Integration with SpiceSharp's transient timestep and numerical integration method
- Initial condition handling for internal states
- Stable handling of repeated or high-frequency poles

### State-Space Realization Notes

For a normalized denominator:

```text
D(s) = s^n + alpha[n-1]*s^(n-1) + ... + alpha[1]*s + alpha[0]
```

the implementation can use a controllable canonical form. The exact matrix layout should be chosen to match SpiceSharp's behavior API, but the required idea is:

```text
state derivative = linear function of state + input
output           = linear function of state + optional direct input feedthrough
```

If `H(s)` has a direct feedthrough term, the output includes both an instantaneous part and a dynamic state part:

```text
y(t) = D*u(t) + C*x_state(t)
```

This matters for convergence and for matching AC gain at high frequency.

### Stability And Numerical Checks

The implementation should validate or guard against numerically dangerous transfer functions:

- Empty numerator or denominator
- Denominator equal to zero
- Non-finite coefficients: `NaN`, positive infinity, negative infinity
- Near-zero leading denominator coefficient
- Improper transfer functions without a supported feedthrough/derivative strategy
- Denominator values near zero during AC evaluation
- Very high transfer-function order
- Unstable poles, if transient support is intended to be robust
- Repeated poles or badly scaled coefficients that can cause ill-conditioned state matrices

Recommended coefficient handling:

- Define a small tolerance for trimming near-zero coefficients
- Normalize denominator leading coefficient to `1` before state-space conversion
- Preserve coefficient scale carefully for AC evaluation to avoid overflow
- Use Horner evaluation rather than explicitly computing powers of `s`

### Minimal Math For Each Milestone

For syntax-only support:

- Detect `LAPLACE`
- Validate the rough expression shape
- Emit a clear unsupported-feature diagnostic

For the recommended built-in-component MVP:

- Detect `LAPLACE` in `E` and `G` custom-source paths
- Parse the input expression and accept only simple voltage probes: `V(node)` and `V(node1,node2)`
- Convert that input probe into `controlPos` and `controlNeg` nodes
- Parse the transfer expression into `N(s) / D(s)`
- Evaluate parameters used as coefficients
- Emit numerator and denominator arrays in ascending powers of `s`
- Instantiate `LaplaceVoltageControlledVoltageSource` or `LaplaceVoltageControlledCurrentSource`
- Set `delay` to `0.0` until delay syntax is intentionally supported

For broader compatibility:

- Add support for alternate PSpice source syntax variants
- Decide whether arbitrary input expressions should be rejected, lowered through an internal helper source, or implemented with custom behavior
- Validate transient behavior of the built-in SpiceSharpBehavioral `Time` behavior before documenting transient compatibility
- Add generated C# writer support for the built-in Laplace entities

## Detailed Implementation Blueprint

This section describes a practical implementation path for adding `LAPLACE` support without trying to force it through the existing scalar expression-function mechanism.

The most important design decision is to treat `LAPLACE` as a **source-level transfer-function feature**, not as a normal math function. Existing functions such as `poly()`, `if()`, and `limit()` produce real-valued expression trees. A Laplace source needs analysis-dependent behavior: DC uses `s = 0`, AC uses `s = j*omega`, and transient needs dynamic internal state.

### Recommended First Scope

Start with this documented subset:

```spice
Ename out+ out- LAPLACE {V(ctrl+)} = {transfer_expr}
Ename out+ out- LAPLACE {V(ctrl+,ctrl-)} = {transfer_expr}
Gname out+ out- LAPLACE {V(ctrl+)} = {transfer_expr}
Gname out+ out- LAPLACE {V(ctrl+,ctrl-)} = {transfer_expr}
```

Examples:

```spice
ELOW out 0 LAPLACE {V(in)} = {1 / (1 + s*tau)}
GHIGH out 0 LAPLACE {V(in)} = {s / (s + 2*pi*fc)}
EBIQUAD out 0 LAPLACE {V(in)} = {w0*w0 / (s*s + s*w0/q + w0*w0)}
```

The initial version should support:

- `E` voltage-controlled voltage output
- `G` voltage-controlled current output
- Input probes that are exactly `V(node)` or `V(node1,node2)`
- Mapping the input probe to `controlPos` and `controlNeg` nodes
- Transfer expression parsed by a new rational-polynomial parser
- Built-in SpiceSharpBehavioral OP/DC, AC, and transient behavior through `LaplaceVoltageControlledVoltageSource` and `LaplaceVoltageControlledCurrentSource`
- `delay = 0.0` until PSpice delay syntax is deliberately designed and tested

Defer these until after the first version works:

- `F` and `H` current-controlled source variants
- `B` source syntax variants
- Arbitrary input expressions such as `V(a)-V(b)+I(Vsense)`
- Exotic PSpice syntax aliases
- Non-rational transfer expressions
- Initial-condition options for transfer-function state

### Proposed File Layout

Keep the source parser integration near existing ABM source code, and put reusable math in a small focused namespace.

Suggested new files:

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

Do not add parser-owned runtime component classes for the MVP. Use the components already provided by `SpiceSharpBehavioral`:

```text
SpiceSharp.Components.LaplaceVoltageControlledVoltageSource
SpiceSharp.Components.LaplaceVoltageControlledCurrentSource
```

Only consider parser-owned custom components if a later requirement needs arbitrary behavioral input expressions or a source variant that the built-in components cannot model.

### Data Model

Use small immutable types for the math layer. The exact syntax can follow the repo's current C# style, but the shape should be close to this:

```csharp
internal sealed class Polynomial
{
    public IReadOnlyList<double> Coefficients { get; }
    public int Degree { get; }

    public static Polynomial Zero { get; }
    public static Polynomial One { get; }
    public static Polynomial S { get; }

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
    public double EvaluateReal(double value);
    public Complex EvaluateComplex(Complex value);
}
```

The MVP does not need to evaluate the transfer function during simulation because SpiceSharpBehavioral does that. Complex evaluation is still useful for unit tests of the math layer and for diagnostics.

The source-level model should carry the raw syntax, the resolved controlling nodes, and the transfer coefficients:

```csharp
internal sealed class LaplaceSourceDefinition
{
    public string SourceName { get; }
    public string SourceKind { get; }
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

`NumeratorCoefficients` and `DenominatorCoefficients` should be returned in ascending powers of `s`, exactly as required by the built-in SpiceSharpBehavioral Laplace components.

### Parser Integration

Add detection beside the existing `POLY` and `TABLE` branches in these methods:

```text
VoltageSourceGenerator.CreateCustomVoltageSource(...)
CurrentSourceGenerator.CreateCustomCurrentSource(...)
```

The flow should be:

1. Detect a `WordParameter` whose value is `laplace`, using the configured case sensitivity rules where possible.
2. Read the next parameter as the expression/equals payload.
3. Extract `input_expr` and `transfer_expr`.
4. Parse `input_expr` as a simple voltage probe and extract control nodes.
5. Parse `transfer_expr` with the new Laplace parser.
6. Create the appropriate built-in Laplace source entity.

Pseudo-code for the source generator branch:

```csharp
var laplaceParameter = parameters.FirstOrDefault(p => IsWord(p, "laplace", context));
if (laplaceParameter != null)
{
    var parser = new LaplaceSourceParser(context.EvaluationContext, context.Result.ValidationResult);
    var definition = parser.Parse(name, type, parameters, laplaceParameter);

    if (definition == null)
    {
        return null;
    }

    return CreateLaplaceSourceEntity(name, parameters, context, definition, isVoltageControlled);
}
```

Place this branch before `POLY` and `TABLE` handling in both custom-source methods. The normal linear `E out 0 in 0 gain` and `G out 0 in 0 gain` paths should stay untouched.

Do not add `laplace()` to `SpiceEvaluationContext.CreateSpiceFunctions()` as the primary implementation. That path is for real scalar functions and would hide the fact that `LAPLACE` must behave differently in OP, AC, and transient analyses.

### Parser Grammar Gap To Check First

Before writing source-generator code, add a parser-level test for this exact line:

```spice
ELOW out 0 LAPLACE {V(in)} = {1/(1+s*tau)}
```

The existing `ExpressionEqualParameter` is tailored for `TABLE`, where the left side is an expression and the right side is a list of points. Its model is effectively:

```text
Expression = Points
```

PSpice `LAPLACE` needs:

```text
InputExpression = TransferExpression
```

That means the generic netlist grammar may fail before `VoltageSourceGenerator` or `CurrentSourceGenerator` ever sees a usable parameter object. If it fails, add a dedicated expression-to-expression parameter shape, for example:

```csharp
internal sealed class ExpressionAssignmentParameter : Parameter
{
    public string LeftExpression { get; }
    public string RightExpression { get; }
}
```

Then update the parse-tree generation/evaluation path so an expression followed by `=` and another expression can be represented without forcing the right-hand side through `Points`.

Concrete parser areas to inspect:

- `src/SpiceSharpParser/Parsers/Netlist/Spice/Symbols.cs`
- `src/SpiceSharpParser/Parsers/Netlist/Spice/Internals/ParseTreeGenerator.cs`
- `src/SpiceSharpParser/Parsers/Netlist/Spice/Internals/ParseTreeEvaluator.cs`
- `src/SpiceSharpParser/Models/Netlist/Spice/Objects/Parameters/ExpressionEqualParameter.cs`

This grammar spike should be done before the source-generator mapping work. Otherwise the implementation can look correct in `CreateCustomVoltageSource` while real PSpice lines still fail during the earlier parse stage.

### Parsing The Source Syntax

The parser should accept the exact scoped form first:

```spice
LAPLACE {input_expr} = {transfer_expr}
```

Implementation details:

- Require an input expression.
- Require an equals separator.
- Require a transfer expression.
- Preserve source line info for diagnostics.
- Strip only the outer expression braces; do not remove braces inside nested expressions blindly.
- For the MVP, accept only `V(name)` and `V(name1,name2)` as the input expression.
- Treat `V(name)` as `V(name,0)`.
- Reject arbitrary input expressions with a clear unsupported diagnostic.
- Parse the transfer expression with the new rational-polynomial parser.

Validation messages should be specific:

- `laplace expects input expression`
- `laplace input expression must be V(node) or V(node1,node2)`
- `laplace expects transfer expression after =`
- `laplace transfer expression must be a rational polynomial in s`
- `laplace transfer denominator cannot be zero`
- `laplace delay is not supported yet`

### Laplace Transfer Expression Parser

Reuse the existing expression lexer/parser to get an AST if practical, but use a new AST visitor/builder that returns `RationalPolynomial` instead of `double`.

The builder should apply these rules:

```text
number        -> RationalPolynomial(number, 1)
parameter     -> evaluate to double, then RationalPolynomial(value, 1)
s             -> RationalPolynomial(s, 1)
-x            -> -Build(x)
x + y         -> Build(x).Add(Build(y))
x - y         -> Build(x).Subtract(Build(y))
x * y         -> Build(x).Multiply(Build(y))
x / y         -> Build(x).Divide(Build(y))
x ^ n         -> Build(x).Pow(n), only when n is a non-negative integer constant
```

Reject these cases in the first implementation:

- `s` used as a parameter instead of the symbolic variable inside transfer expressions
- non-integer powers such as `s^0.5`
- negative powers such as `s^-1`; users can write `1/s` instead
- functions of `s`
- voltage/current probes inside the transfer expression
- stochastic functions
- conditional expressions involving `s`

Parameter evaluation should use the existing evaluator only for subexpressions that do not contain `s`. For example:

```spice
.PARAM wc={2*pi*1k}
E1 out 0 LAPLACE {V(in)} = {wc/(s+wc)}
```

`wc` can be evaluated by the existing parameter evaluator, while `s+wc` is handled by the rational-polynomial builder.

### Polynomial Implementation Details

Represent coefficients in ascending order by power:

```text
3 + 2*s + 5*s^2 -> [3, 2, 5]
```

Addition:

```text
result[i] = left[i] + right[i]
```

Multiplication:

```text
for i in left powers:
    for j in right powers:
        result[i + j] += left[i] * right[j]
```

Power:

```text
result = 1
repeat exponent times:
    result = result * base
```

Normalization:

```text
while highest-order coefficient is abs(coefficient) < tolerance:
    remove highest-order coefficient
```

Complex evaluation with Horner's method:

```csharp
Complex result = coefficients[degree];
for (var power = degree - 1; power >= 0; power--)
{
    result = result * s + coefficients[power];
}
```

Do not explicitly compute `Complex.Pow(s, n)` for every term unless the order is tiny. Horner evaluation is simpler, faster, and numerically better.

### Rational Polynomial Implementation Details

Build operations from polynomial operations:

```text
(a/b) + (c/d) = (a*d + c*b) / (b*d)
(a/b) - (c/d) = (a*d - c*b) / (b*d)
(a/b) * (c/d) = (a*c) / (b*d)
(a/b) / (c/d) = (a*d) / (b*c)
```

After every operation:

- Normalize numerator and denominator.
- Check denominator is not zero.
- Optionally divide out a common scalar factor, especially if the denominator leading coefficient is not `1`.

Avoid full symbolic polynomial greatest-common-divisor simplification in the first version. It is not needed for most SPICE transfer functions and adds complexity.

### OP/DC Runtime Behavior

Do not implement custom OP/DC stamping for the MVP. The built-in SpiceSharpBehavioral Laplace source has biasing behavior, so the parser should provide nodes and coefficient arrays and let the component stamp itself.

The parser can still perform useful pre-validation:

```text
dc_denominator = denominator[0]
dc_numerator   = numerator[0]
```

For finite DC gain, the built-in source should behave as:

```text
gain = dc_numerator / dc_denominator
```

For singular DC gain, such as `1/s`, avoid guessing. Add a targeted implementation spike and tests before deciding whether to reject at parse time or allow SpiceSharpBehavioral to handle it. A conservative MVP can reject singular DC gain with a clear diagnostic if AC simulations cannot proceed reliably through operating-point setup.

### AC Runtime Behavior

Do not implement custom AC stamping for the MVP. `LaplaceVoltageControlledVoltageSource` and `LaplaceVoltageControlledCurrentSource` include frequency behavior and already use SpiceSharp's complex analysis state.

The parser should only ensure that this PSpice expression:

```spice
ELOW out 0 LAPLACE {V(in)} = {1/(1+s*tau)}
```

is converted into this equivalent component setup:

```csharp
var entity = new LaplaceVoltageControlledVoltageSource("ELOW");
// nodes: out, 0, in, 0
entity.Parameters.Numerator = new[] { 1.0 };
entity.Parameters.Denominator = new[] { 1.0, tau };
entity.Parameters.Delay = 0.0;
```

An AC probe verified that `Denominator = [1, tau]` gives the expected low-pass value at cutoff. That should become an integration test so future maintainers do not accidentally reverse the coefficient order.

The existing commented code in `ACControl.cs` shows that AC has access to `ComplexState.Laplace`, but this feature should not be implemented by updating a scalar `FREQ` parameter. True Laplace behavior needs complex stamping, which the built-in source behavior already supplies.

### Transient Runtime Behavior

SpiceSharpBehavioral exposes `LaplaceBehaviors.Time` and time behavior for both built-in Laplace controlled sources, so transient support may already be available once the parser maps the source correctly.

Treat transient as **supported only after tests prove it**. Add step-response tests for simple first-order transfer functions before documenting it as compatible. For example:

```spice
.PARAM tau=1u
VIN in 0 PULSE(0 1 0 1n 1n 10u 20u)
ELOW out 0 LAPLACE {V(in)} = {1/(1+s*tau)}
.TRAN 0.05u 8u
```

Expected behavior is approximately:

```text
V(out,t) = 1 - exp(-t/tau)
```

Keep the state-space notes above because they are useful if custom behavior is ever needed, but the first implementation should rely on the built-in time behavior and validate its limits.

### Entity Creation Strategy

The existing `CreateBehavioralVoltageSource` and `CreateBehavioralCurrentSource` helpers are built around an expression string plus `ParseAction`. A Laplace source should bypass those helpers and create a built-in Laplace entity.

Suggested creation flow:

```csharp
private IEntity CreateLaplaceSourceEntity(
    string name,
    ParameterCollection parameters,
    IReadingContext context,
    LaplaceSourceDefinition definition,
    bool isVoltageControlled)
{
    if (definition.SourceKind == "e")
    {
        var entity = new LaplaceVoltageControlledVoltageSource(name);
        context.CreateNodes(entity, CreateLaplaceNodeParameters(parameters, definition));
        entity.Parameters.Numerator = definition.TransferFunction.NumeratorCoefficients;
        entity.Parameters.Denominator = definition.TransferFunction.DenominatorCoefficients;
        entity.Parameters.Delay = definition.Delay;
        return entity;
    }

    if (definition.SourceKind == "g")
    {
        var entity = new LaplaceVoltageControlledCurrentSource(name);
        context.CreateNodes(entity, CreateLaplaceNodeParameters(parameters, definition));
        entity.Parameters.Numerator = definition.TransferFunction.NumeratorCoefficients;
        entity.Parameters.Denominator = definition.TransferFunction.DenominatorCoefficients;
        entity.Parameters.Delay = definition.Delay;
        return entity;
    }

    context.Result.ValidationResult.AddError(
        ValidationEntrySource.Reader,
        "laplace is not supported for this source type yet",
        parameters.LineInfo);
    return null;
}
```

`CreateLaplaceNodeParameters` should build the four-node sequence expected by the built-in component:

```text
out+, out-, control+, control-
```

Use the same node creation/name-generation path as the existing source generators. Do not pass raw node names through the full constructor unless they have already been transformed the same way `context.CreateNodes` would transform them, especially inside subcircuits.

For the first scope, prefer only `E` and `G` because the built-in components are voltage-controlled. A PSpice input expression of `V(in)` maps cleanly to `controlPos = in` and `controlNeg = 0`.

### Generated C# Writer Support

After runtime support works, update generated-code support.

The writer must be able to emit the equivalent SpiceSharp entity construction for a parsed Laplace source. That likely means:

- Add writer handling for `LaplaceVoltageControlledVoltageSource` and `LaplaceVoltageControlledCurrentSource`.
- Serialize numerator and denominator coefficients.
- Serialize the four connected nodes.
- Serialize `Delay`.

Do not update writer support first. It should follow the runtime entity shape after that shape is proven by tests.

### Documentation Updates

Update `src/docs/articles/behavioral-source.md` after implementation.

Document:

- Supported syntax
- Supported source types
- Supported analyses proven by tests: OP, AC, and TRAN if transient tests pass
- Expression subset allowed inside the transfer function
- Input expression subset: initially `V(node)` and `V(node1,node2)`
- Treatment of `s`
- Known limitations
- Examples for low-pass, high-pass, and second-order filters

If transient behavior is not verified, explicitly state that the first implementation supports OP/AC only. Silent partial compatibility would be painful for users debugging imported PSpice models.

### Suggested Pull Request Sequence

Implement the feature in small PR-sized slices:

1. Add `Polynomial`, `RationalPolynomial`, and unit tests for algebra and complex evaluation.
2. Add `LaplaceExpressionParser` and tests for supported/unsupported transfer expressions.
3. Add `LaplaceSourceParser` for `LAPLACE {V(...)} = {...}` and diagnostics.
4. Map `E` sources to `LaplaceVoltageControlledVoltageSource`.
5. Map `G` sources to `LaplaceVoltageControlledCurrentSource`.
6. Add OP and AC integration tests for first-order and second-order examples.
7. Add transient step-response tests and document the result.
8. Add generated C# writer support.
9. Add documentation.
10. Add arbitrary input-expression support only if there is a concrete requirement.

This sequence keeps the parser and coefficient work behind tested math while using the proven SpiceSharpBehavioral runtime components as early as possible.

### Acceptance Criteria For The Built-In Component MVP

The first useful implementation should meet these criteria:

- Parses `E` and `G` Laplace sources in the documented syntax.
- Rejects malformed Laplace syntax with useful validation errors.
- Rejects unsupported input expressions outside `V(node)` and `V(node1,node2)`.
- Converts rational expressions in `s` into numerator and denominator coefficients.
- Emits coefficients in ascending powers of `s`.
- Supports parameterized coefficients.
- Maps parsed sources to `LaplaceVoltageControlledVoltageSource` and `LaplaceVoltageControlledCurrentSource`.
- Produces correct OP behavior for finite DC gain.
- Handles or clearly rejects singular DC gain.
- Produces correct AC magnitude and phase through the built-in frequency behavior.
- Matches analytic magnitude and phase for first-order low-pass, first-order high-pass, and second-order low-pass examples.
- Includes transient tests or documents that transient compatibility has not yet been verified.

## Worked Transfer-Function Examples

These examples are useful both for implementation tests and for validating the coefficient normalization logic.

### First-Order Low-Pass

Netlist form:

```spice
.PARAM tau=1u
ELOW out 0 LAPLACE {V(in)} = {1 / (1 + s*tau)}
```

Transfer function:

```text
H(s) = 1 / (1 + tau*s)
```

Expected coefficients:

```text
Numerator   = [1]
Denominator = [1, tau]
```

Expected behavior:

- DC gain is `1`.
- Pole is at `1 / tau` radians per second.
- At `frequency = 1 / (2*pi*tau)`, magnitude is about `-3.0103 dB` and phase is about `-45 degrees`.

Implementation note:

- This should be the first AC integration test because the expected response is simple and easy to verify analytically.

### First-Order High-Pass

Netlist form:

```spice
.PARAM fc=1k
.PARAM wc={2*pi*fc}
EHIGH out 0 LAPLACE {V(in)} = {s / (s + wc)}
```

Transfer function:

```text
H(s) = s / (s + wc)
```

Expected coefficients:

```text
Numerator   = [0, 1]
Denominator = [wc, 1]
```

Expected behavior:

- DC gain is `0`.
- High-frequency gain approaches `1`.
- At `fc`, magnitude is about `-3.0103 dB` and phase is about `45 degrees`.

Implementation note:

- This verifies that a numerator with a zero constant term is not incorrectly trimmed to zero.

### Inverting Or Scaled Low-Pass

Netlist form:

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

Expected behavior:

- DC gain is `gain`.
- High-frequency gain approaches `0`.
- The sign of `gain` contributes a 180 degree phase inversion.

Implementation note:

- This verifies parameter expansion, negative coefficients, and phase handling for negative real gain.

### Lead-Lag Network

Netlist form:

```spice
.PARAM wz=1k
.PARAM wp=10k
.PARAM k=2
ELEAD out 0 LAPLACE {V(in)} = {k*(1 + s/wz) / (1 + s/wp)}
```

Expected coefficients before optional scaling:

```text
Numerator   = [k, k/wz]
Denominator = [1, 1/wp]
```

Equivalent scaled form:

```text
Numerator   = [k*wz*wp, k*wp]
Denominator = [wz*wp, wz]
```

Expected behavior:

- DC gain is `k`.
- High-frequency gain approaches `k*wp/wz`.

Implementation note:

- This verifies division by parameter constants and rational expression multiplication.

### Second-Order Low-Pass

Netlist form:

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

Expected behavior:

- DC gain is `1`.
- For `q = 1 / sqrt(2)`, this is a Butterworth-style second-order low-pass section.
- Magnitude near `f0` should be about `-3 dB` for the Butterworth case.

Implementation note:

- This verifies second-order denominator parsing, multiplication of parameters, and AC response for complex poles.

### Second-Order Band-Pass

Netlist form:

```spice
.PARAM f0=10k
.PARAM w0={2*pi*f0}
.PARAM q=5
EBP out 0 LAPLACE {V(in)} = {(s*w0/q) / (s*s + s*w0/q + w0*w0)}
```

Expected coefficients:

```text
Numerator   = [0, w0/q]
Denominator = [w0*w0, w0/q, 1]
```

Expected behavior:

- DC gain is `0`.
- High-frequency gain approaches `0`.
- Gain peaks near `f0`.

Implementation note:

- This verifies that both numerator and denominator may have missing powers of `s`.

### Integrator

Netlist form:

```spice
EINT out 0 LAPLACE {V(in)} = {1 / s}
```

Expected coefficients:

```text
Numerator   = [1]
Denominator = [0, 1]
```

Problem:

- DC gain is singular because `D(0) = 0`.

Recommended first solution:

- Accept the transfer-expression algebra in parser tests.
- Reject OP/DC simulation with a clear validation error for singular DC gain.
- Allow AC or transient only if targeted tests prove the built-in SpiceSharpBehavioral behavior can handle the singular operating-point case.
- Otherwise require users to provide a finite DC path or a non-singular transfer function.

More advanced solution:

- Support it in transient with an internal state and explicit initial condition semantics.
- Support it in AC as `1 / (j*w)`, while documenting that OP is singular.

### Differentiator

Netlist form:

```spice
EDIFF out 0 LAPLACE {V(in)} = {s}
```

Expected coefficients:

```text
Numerator   = [0, 1]
Denominator = [1]
```

Problem:

- This is an improper transfer function. It has numerator degree greater than denominator degree.
- It is unbounded at high frequency.
- Transient implementation would require differentiating the input or adding a physically meaningful rolloff.

Recommended first solution:

- Allow parsing for diagnostics and coefficient tests.
- Reject simulation with `laplace transfer function is improper` unless AC-only behavior deliberately permits it.

User-facing workaround:

```spice
.PARAM fp=100meg
.PARAM wp={2*pi*fp}
EDIFF out 0 LAPLACE {V(in)} = {s / (1 + s/wp)}
```

This turns the ideal differentiator into a proper high-pass-like differentiator with finite high-frequency gain.

## Sample Netlists For Validation

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

Expected checks:

- At very low frequency, `V(out)` magnitude is near `1`.
- At `1 / (2*pi*tau)`, magnitude is near `0.7071`.
- At high frequency, magnitude trends down at about `-20 dB/decade`.

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

Expected checks:

- At very low frequency, `V(out)` magnitude is near `0`.
- At `fc`, magnitude is near `0.7071`.
- At high frequency, magnitude approaches `1`.

### Current Source Example

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

Expected checks:

- DC transconductance is `gm`.
- Low-frequency output voltage magnitude is roughly `gm * RLOAD` if loading assumptions hold.
- AC response rolls off after `fc`.

### Syntax Error Examples

These should produce validation errors instead of vague parser exceptions:

```spice
E1 out 0 LAPLACE
E2 out 0 LAPLACE {V(in)}
E3 out 0 LAPLACE {V(in)} = {}
E4 out 0 LAPLACE {V(in)} = {sin(s)}
E5 out 0 LAPLACE {V(in)} = {s^0.5}
E6 out 0 LAPLACE {V(in)} = {1 / 0}
```

## Common Problems And Practical Solutions

### Problem: PSpice Has Several LAPLACE Syntax Variants

PSpice examples in the wild may use slightly different spellings, bracing, or coefficient-list forms.

Practical solution:

- Implement one documented expression form first: `LAPLACE {input_expr} = {transfer_expr}`.
- Add syntax variants only when tests prove the first path works.
- Emit diagnostics for recognized but unsupported variants instead of letting them fall through to unrelated source parsing.

Possible later syntax variants to investigate:

```spice
E1 out 0 LAPLACE {V(in)} {1/(1+s*tau)}
E1 out 0 LAPLACE = {V(in)} {1/(1+s*tau)}
E1 out 0 VALUE = {LAPLACE(V(in), 1/(1+s*tau))}
```

Do not support these by guessing. Confirm expected PSpice semantics first.

### Problem: `s` Can Conflict With A User Parameter

Example:

```spice
.PARAM s=10
R1 in out {s}
E1 out 0 LAPLACE {V(in)} = {1 / (1 + s*tau)}
```

Practical solution:

- Treat `s` as symbolic only while parsing the Laplace transfer expression.
- Treat `s` normally everywhere else.
- If a user needs a parameter named `s` inside a transfer expression, require them to rename it. This is consistent with `s` being the Laplace-domain variable.

Diagnostic suggestion:

```text
laplace transfer expression reserves symbol 's'; use a different parameter name
```

### Problem: Parameter Expressions May Depend On Circuit Values

Some expressions might contain voltage probes or functions that are not constants:

```spice
E1 out 0 LAPLACE {V(in)} = {1 / (1 + s*V(ctrl))}
```

Practical solution:

- Reject non-constant coefficient expressions in the first version.
- Coefficients should be computed from constants and `.PARAM` values only.
- If dynamic coefficients are desired later, that is a significantly larger nonlinear dynamic-system feature.

Diagnostic suggestion:

```text
laplace transfer coefficients must be constant expressions
```

### Problem: Singular DC Gain

Examples:

```spice
E1 out 0 LAPLACE {V(in)} = {1 / s}
E2 out 0 LAPLACE {V(in)} = {(s + 1) / s}
```

Practical solution for MVP:

- Detect `abs(D(0)) < tolerance`.
- Reject OP/DC use with a clear validation error.
- If AC simulation requires an operating point first, decide whether this source can be excluded from OP or whether it must be rejected until transient/state support exists.

Possible later solution:

- Implement dynamic state and initial conditions for integrators.
- Allow AC transfer evaluation independently of finite DC gain if SpiceSharp permits the source behavior to participate in AC after OP.

### Problem: Improper Transfer Function

Example:

```spice
E1 out 0 LAPLACE {V(in)} = {s*s / (s + 1)}
```

Practical solution:

- Compare numerator and denominator degree.
- If `degree(N) > degree(D)`, reject by default.
- Suggest adding a high-frequency pole to make the transfer function proper.

Diagnostic suggestion:

```text
laplace transfer function is improper; numerator degree exceeds denominator degree
```

User-facing workaround:

```spice
.PARAM wp=1e9
E1 out 0 LAPLACE {V(in)} = {s*s / ((s + 1)*(1 + s/wp))}
```

### Problem: Very High-Order Transfer Functions Are Numerically Fragile

High-order filters are sensitive to coefficient scaling and can produce poor matrix conditioning.

Practical solution:

- Set a conservative maximum order at first, such as 8 or 10.
- Warn when order is high even if allowed.
- Encourage users to split filters into cascaded first-order and second-order sections.

Preferred user pattern:

```spice
E1 n1 0 LAPLACE {V(in)} = {w0a*w0a / (s*s + s*w0a/qa + w0a*w0a)}
E2 out 0 LAPLACE {V(n1)} = {w0b*w0b / (s*s + s*w0b/qb + w0b*w0b)}
```

### Problem: Coefficient Scaling Can Overflow Or Underflow

Example:

```spice
E1 out 0 LAPLACE {V(in)} = {1e24 / (s*s + 1e12*s + 1e24)}
```

Practical solution:

- Normalize by the leading denominator coefficient for state-space conversion.
- For AC evaluation, use Horner evaluation and avoid explicit powers.
- Consider scaling numerator and denominator by a common factor when values are extremely large or small.
- Add tests for both low-frequency and high-frequency evaluation to catch overflow.

### Problem: AC Input Expression Is Not A Simple Node Voltage

Examples:

```spice
E1 out 0 LAPLACE {V(a,b)} = {1/(1+s*tau)}
E2 out 0 LAPLACE {V(a)-V(b)+0.5*I(Vsense)} = {1/(1+s*tau)}
```

Practical solution:

- Start with simple `V(node)` and `V(node1,node2)` only, because the built-in SpiceSharpBehavioral Laplace components are voltage-controlled sources with explicit control nodes.
- Map `V(node)` to `controlPos = node`, `controlNeg = 0`.
- Map `V(node1,node2)` to `controlPos = node1`, `controlNeg = node2`.
- Reject more complex input expressions with a clear diagnostic.

Possible later expansion:

- Lower arbitrary input expressions through an internally generated behavioral voltage source and use that helper node as the Laplace control input.
- Or implement custom Laplace behavior that can evaluate arbitrary behavioral expressions in each analysis mode.
- Treat this as a later feature, because it changes topology and diagnostics in ways users will notice.

### Problem: Existing `FREQ` Parameter Is Real, But LAPLACE Needs Complex `s`

Updating a `FREQ` parameter is not sufficient for true Laplace behavior.

Practical solution:

- Use the built-in Laplace source frequency behavior, which already works with the complex AC state.
- Do not represent `H(s)` as a real expression of `FREQ`.
- Preserve magnitude and phase through complex stamping.

### Problem: AC Analysis Usually Requires An Operating Point

Some Laplace sources have no finite DC gain, but AC analysis often starts from an operating point.

Practical solution:

- For the built-in-component MVP, test singular-DC cases such as `1/s` before deciding whether to accept them.
- If SpiceSharpBehavioral cannot proceed through operating-point setup for those cases, reject them with a clear diagnostic.
- Document this limitation.
- Add a targeted test for `1/s` that verifies the diagnostic.

### Problem: Transient Initial Conditions Are Ambiguous

PSpice models may rely on implicit or explicit initial conditions for dynamic blocks.

Practical solution:

- Verify the built-in `LaplaceBehaviors.Time` behavior with step-response tests before documenting transient support.
- Define whether internal state starts at zero, from DC operating point, or from explicit user syntax based on observed SpiceSharpBehavioral behavior and PSpice compatibility expectations.
- Add docs and tests for step response with known initial conditions.

Possible future syntax:

```spice
E1 out 0 LAPLACE {V(in)} = {1/(1+s*tau)} IC=0
```

Only add such syntax after checking PSpice compatibility expectations.

### Problem: Error Handling Can Be Hard To Trace

Laplace parsing can fail at several layers: netlist tokenization, source syntax extraction, transfer expression parsing, parameter evaluation, and runtime validation.

Practical solution:

- Attach original line info to `LaplaceSourceDefinition`.
- Keep input and transfer expression strings in diagnostics.
- Prefer validation errors over generic exceptions for user mistakes.
- Include the source name in diagnostics.

Good diagnostic examples:

```text
ELOW: laplace transfer expression must be a rational polynomial in s: sin(s)
EINT: laplace transfer function has singular DC gain: 1 / s
EDIFF: laplace transfer function is improper: s
```

## Additional Test Matrix

Math unit tests should cover:

- `Polynomial.Add`, `Subtract`, `Multiply`, `Scale`, `Pow`
- trimming and degree calculation
- real evaluation at `s = 0`
- complex evaluation at selected frequencies
- rational addition, multiplication, division, and power
- denominator-zero rejection

Parser unit tests should cover:

- constants: `{5}`
- symbol: `{s}`
- first-order denominator: `{1/(1+s*tau)}`
- second-order denominator: `{w0*w0/(s*s+s*w0/q+w0*w0)}`
- equivalent operator forms: `s*s`, `s^2`, `(s + a)*(s + b)`
- invalid powers: `s^0.5`, `s^-1`
- invalid functions: `sin(s)`, `exp(s)`
- invalid probes in transfer expression: `V(x)*s`

Source parser tests should cover:

- raw parsing of `LAPLACE {V(in)} = {1/(1+s*tau)}` into an expression-to-expression parameter shape
- valid `E` source syntax
- valid `G` source syntax
- missing input expression
- missing equals sign
- missing transfer expression
- malformed braces
- unsupported source type diagnostics for `F`, `H`, or `B` if deferred

Simulation tests for the built-in-component MVP should cover:

- first-order low-pass magnitude and phase at several frequencies
- first-order high-pass magnitude and phase at several frequencies
- second-order low-pass response near resonance/cutoff
- parameterized coefficients
- negative gain phase inversion
- singular DC rejection
- improper transfer-function rejection

Regression tests should include imported PSpice-style examples that motivated the feature, once available.

## Implementation Options

### Option 1: Syntax Recognition Only

Implement parser recognition and validation for `LAPLACE`, but report it as unsupported.

Estimated effort: **0.5-1 day**.

This is useful if the goal is better diagnostics and compatibility reporting.

### Option 2: Built-In Component MVP

Support common voltage-controlled Laplace forms by mapping them to the built-in SpiceSharpBehavioral Laplace entities.

Estimated effort: **2-4 engineering days** for parser, coefficient mapping, OP/AC tests, and diagnostics, assuming no generated-code support surprises.

This would likely support syntax such as:

```spice
E1 out 0 LAPLACE {V(in)} = {1 / (1 + s * tau)}
G1 out 0 LAPLACE {V(in)} = {s / (s + 1000)}
```

The implementation parses the transfer function into coefficient arrays and lets `LaplaceVoltageControlledVoltageSource` and `LaplaceVoltageControlledCurrentSource` handle OP/AC runtime behavior.

This is the best first milestone.

### Option 3: Full PSpice-Compatible AC + Transient Support

Support AC, OP, and transient behavior, plus broader PSpice syntax variants and possibly arbitrary input expressions.

Estimated effort: **1-3 weeks**, depending mostly on transient validation, generated-code support, and how far arbitrary input-expression compatibility must go.

Transient may already be covered by the built-in time behavior, but it still needs careful validation. Arbitrary input expressions are likely the expensive part because the built-in components only accept voltage-control nodes.

## Recommended Implementation Plan

1. Define the supported syntax subset: `E`/`G` with `V(node)` or `V(node1,node2)` input.
2. Add a parser grammar test for expression-to-expression syntax after `LAPLACE`.
3. Add or adapt the parameter object needed to represent `{V(in)} = {1/(1+s*tau)}`.
4. Add source-level syntax detection beside the existing `POLY` and `TABLE` handling.
5. Parse the Laplace transfer expression separately from normal real-valued expressions.
6. Treat `s` as a reserved symbolic variable only inside the Laplace transfer expression.
7. Normalize the transfer expression into numerator and denominator coefficient arrays in ascending powers of `s`.
8. Map `E` sources to `LaplaceVoltageControlledVoltageSource`.
9. Map `G` sources to `LaplaceVoltageControlledCurrentSource`.
10. Add OP and AC tests for analytic examples.
11. Add transient step-response tests and document the verified behavior.
12. Update generated C# writer support.
13. Document the supported syntax and limitations.

## Key Files

- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/VoltageSourceGenerator.cs`
- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/CurrentSourceGenerator.cs`
- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/ExpressionFactory.cs`
- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/ExpressionResolver.cs`
- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/CustomRealBuilder.cs`
- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/SpiceEvaluationContext.cs`
- `src/SpiceSharpParser/Parsers/Netlist/Spice/Symbols.cs`
- `src/SpiceSharpParser/Parsers/Netlist/Spice/Internals/ParseTreeGenerator.cs`
- `src/SpiceSharpParser/Parsers/Netlist/Spice/Internals/ParseTreeEvaluator.cs`
- `src/SpiceSharpParser/Models/Netlist/Spice/Objects/Parameters/ExpressionEqualParameter.cs`
- `src/SpiceSharpParser/ModelWriters/CSharp/Entities/Components/SourceWriterHelper.cs`
- `src/docs/articles/behavioral-source.md`

## Tests To Add

Add tests similar to the existing `POLY` and `TABLE` ABM tests.

Recommended new test areas:

- Valid `LAPLACE` syntax parsing
- Invalid syntax diagnostics
- Simple `V(node)` and differential `V(node1,node2)` input parsing
- Rejection of unsupported arbitrary input expressions
- Coefficient order mapping into built-in SpiceSharpBehavioral arrays
- OP/DC gain behavior
- AC response for first-order transfer functions
- AC response for second-order transfer functions
- Case-insensitive keyword handling
- Parameterized transfer-function coefficients
- Singular DC gain diagnostics
- Improper transfer-function diagnostics
- Transient step-response tests to decide whether built-in time behavior can be documented as supported

Likely test templates:

- `src/SpiceSharpParser.IntegrationTests/AnalogBehavioralModeling/PolyTests.cs`
- `src/SpiceSharpParser.IntegrationTests/AnalogBehavioralModeling/TableTests.cs`
- `src/SpiceSharpParser.IntegrationTests/Expressions/PolyExpressionTests.cs`
- `src/SpiceSharpParser.IntegrationTests/DotStatements/FuncTests.cs`

## Risk Areas

- PSpice supports multiple historical ABM syntax variants.
- `s` may conflict with parameter names unless scoped carefully.
- High-order transfer functions may create numerical stability issues.
- Improper transfer functions need clear validation.
- The built-in Laplace components only handle voltage-controlled inputs, so arbitrary behavioral input expressions need a separate design.
- Transient support exists in the dependency but still needs compatibility tests before being promised.

## Recommendation

Implement `LAPLACE` in phases.

Start with a **built-in component MVP** for `E` and `G` sources whose input is `V(node)` or `V(node1,node2)`. That gives useful PSpice compatibility by adding parser and coefficient support while relying on SpiceSharpBehavioral for the analysis-specific runtime behavior. Then validate transient behavior, generated C# output, and broader input-expression support as separate follow-up milestones.
