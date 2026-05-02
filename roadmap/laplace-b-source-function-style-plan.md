# Function-Style LAPLACE And B-Source LAPLACE

## Summary

Add `LAPLACE(input, transfer)` support inside behavioral source expressions, including `VALUE={...}`, `VALUE {...}`, and `B ... V=/I=` forms. The implementation should be AST-based: parse the behavioral expression, detect `LAPLACE(...)` calls, validate their arguments, and lower them into existing SpiceSharp Laplace source entities.

There are two target shapes:

- A whole-expression `LAPLACE(...)` maps directly to an existing Laplace source entity.
- A mixed expression, such as `1 + 2*LAPLACE(...)`, is lowered by generating internal voltage-output Laplace helper sources and replacing each call with `V(<helperNode>)` in the final behavioral expression.

This keeps runtime behavior on the existing SpiceSharp Laplace components instead of inventing a new behavioral runtime function.

## Goals

- Support function-style Laplace expressions in all expression-based voltage/current source paths.
- Support `B` sources:
  - `B<name> <out+> <out-> V={LAPLACE(...)}`
  - `B<name> <out+> <out-> I={LAPLACE(...)}`
  - mixed `B` expressions containing one or more `LAPLACE(...)` calls.
- Support `VALUE` forms on existing source generators:
  - `E`, `V`, and `H` expression outputs as voltage sources.
  - `G`, `I`, and `F` expression outputs as current sources.
- Keep existing source-level `E/G/F/H ... LAPLACE ...` behavior unchanged.
- Reuse the existing Laplace transfer parser, coefficient normalization, validation messages, and entity types wherever practical.
- Preserve current non-Laplace behavioral source behavior.

## Out Of Scope

- Inline option arguments inside the function call, for example `LAPLACE(V(in), H(s), TD=1n)`.
- Arbitrary Laplace input expressions, for example `LAPLACE(V(a)-V(b), H(s))`.
- Function-style delay expressions such as `exp(-s*td)` or broader PSpice/LTspice dialect forms.
- Supporting `LAPLACE(...)` inside `.FUNC` definitions as a dynamic function.
- Hiding generated helper entities from every low-level circuit inspection API.

## Supported Syntax

The supported function signature is:

```spice
LAPLACE(<input>, <transfer>)
```

Accepted input shapes:

```spice
V(node)
V(node1,node2)
I(source)
```

Accepted output contexts:

```spice
E1 out 0 VALUE={LAPLACE(V(in), 1/(1+s*tau))}
G1 out 0 VALUE={LAPLACE(V(in), gm/(1+s*tau))}
V1 out 0 VALUE={LAPLACE(V(in), wc/(s+wc))}
I1 out 0 VALUE={LAPLACE(V(in), gm*wc/(s+wc))}
B1 out 0 V={LAPLACE(V(in), 1/(1+s*tau))}
B2 out 0 I={LAPLACE(V(in), gm/(1+s*tau))}
B3 out 0 V={1 + 2*LAPLACE(V(in), 1/(1+s))}
B4 out 0 I={LAPLACE(V(a), 1/(1+s*t1)) - LAPLACE(V(b), 1/(1+s*t2))}
```

The function name is case-insensitive. Whitespace inside the expression should follow the existing behavioral expression parser rules.

## Entity Mapping

When the entire expression is exactly one `LAPLACE(...)` call, create the final Laplace entity directly:

| Output kind | Input kind | Entity |
| --- | --- | --- |
| Voltage | Voltage | `LaplaceVoltageControlledVoltageSource` |
| Voltage | Current | `LaplaceCurrentControlledVoltageSource` |
| Current | Voltage | `LaplaceVoltageControlledCurrentSource` |
| Current | Current | `LaplaceCurrentControlledCurrentSource` |

Output kind is determined by the source path:

| Source form | Output kind |
| --- | --- |
| `E ... VALUE=...` | Voltage |
| `H ... VALUE=...` | Voltage |
| `V ... VALUE=...` | Voltage |
| `B ... V=...` | Voltage |
| `G ... VALUE=...` | Current |
| `F ... VALUE=...` | Current |
| `I ... VALUE=...` | Current |
| `B ... I=...` | Current |

For mixed expressions, always lower each `LAPLACE(...)` call to a voltage-output helper:

| Laplace input kind | Helper entity |
| --- | --- |
| Voltage input | `LaplaceVoltageControlledVoltageSource` connected from helper node to `0` |
| Current input | `LaplaceCurrentControlledVoltageSource` connected from helper node to `0` |

Then replace the original `LAPLACE(...)` call in the final behavioral expression with:

```spice
V(<helperNode>)
```

The final behavioral source remains a `BehavioralVoltageSource` or `BehavioralCurrentSource` according to the output kind.

## Lowering Algorithm

1. Identify the expression-bearing parameter:
   - `AssignmentParameter` named `VALUE`, `V`, or `I`.
   - `WordParameter` `VALUE` followed by an `ExpressionParameter`.
   - bare `ExpressionParameter` paths that are already treated as behavioral expressions.
2. Parse the raw expression with the existing expression parser into an AST.
3. Traverse the AST and find case-insensitive `FunctionNode` calls named `laplace`.
4. Do not traverse inside a `laplace` call's arguments for further lowering. Nested calls inside input or transfer arguments are invalid through normal input/transfer validation.
5. Validate each `laplace` call:
   - exactly two arguments;
   - first argument is a supported voltage or current probe shape;
   - second argument is a proper rational polynomial in `s`;
   - finite coefficients;
   - finite, non-singular DC gain;
   - denominator is non-zero;
   - transfer order stays within existing `LaplaceExpressionOptions.MaxOrder`.
6. If zero calls are found, return "not handled" and let existing behavioral logic run unchanged.
7. If the root AST node is exactly a single `laplace` call:
   - create the direct final Laplace entity from the mapping table above;
   - no helper node is needed.
8. If the expression is mixed:
   - allocate one helper source and helper node per call;
   - replace the call node with a voltage probe node for the helper node;
   - serialize the modified AST back to a behavioral expression string;
   - create helper entities first;
   - create and return the final behavioral source using the modified expression.

## AST And Formatting Requirements

- Do not locate or replace `LAPLACE(...)` using string slicing or regular expressions.
- Add internal APIs so existing Laplace parsers can work from parsed `Node` instances:
  - `LaplaceExpressionParser.Parse(Node node)`
  - input parsing from a `Node` for `V(...)` and `I(...)`.
- Add a small internal behavioral expression formatter for modified ASTs. It should emit syntax accepted by the existing behavioral parser:
  - binary operators with parentheses;
  - `**` for power if that is what existing expressions expect;
  - `V(node)` and `I(source)` probes;
  - normal function calls for non-Laplace functions;
  - ternary and unary operators where already supported.
- Keep reader and writer formatting code separate from C# interpolation formatting. The reader needs a SPICE expression string; the writer needs generated C# statements.

## Options And Multipliers

Existing source-level `E/G/F/H ... LAPLACE ...` option behavior remains unchanged.

For function-style `LAPLACE(...)` expressions:

- `TD=` and `DELAY=` are accepted as trailing source-level parameters only if exactly one `LAPLACE(...)` call is present.
- The delay option applies to that direct Laplace entity or helper source.
- If more than one `LAPLACE(...)` call is present and a trailing `TD=` or `DELAY=` option is present, emit a reader validation error.
- `M=` must preserve existing current-source behavior:
  - for source-level Laplace syntax, keep folding `M` into the Laplace numerator as today;
  - for direct whole-expression function-style Laplace, folding `M` into the numerator is acceptable when it is semantically equivalent;
  - for mixed current-source behavioral expressions, apply existing `M` behavior to the final current source expression, not to every helper;
  - for voltage-output `VALUE`/`B V=` mixed expressions, reject trailing `M=` unless an existing source path already supports it.

This avoids changing existing `B ... I={expr} M=...` behavior while still allowing direct function-style Laplace to use the current Laplace coefficient path.

## Naming And Collision Rules

Generated helpers should use deterministic, reserved names:

```text
__ssp_laplace_<sanitizedSourceName>_<index>
__ssp_laplace_<sanitizedSourceName>_<index>_src
```

Rules:

- Sanitize to ASCII letters, digits, and underscores.
- Prefix with `__ssp_laplace_`.
- Use a stable index based on traversal order.
- If the generated entity name already exists in `context.ContextEntities`, append an incrementing suffix.
- Helper node names should be local names and go through the same name-generation/scoping path as ordinary node names when subcircuits are expanded.
- The final rewritten behavioral expression should reference the local helper node name. Existing expression resolution should scope it during subcircuit expansion.

## Reader Implementation Plan

1. Refactor Laplace transfer parsing.
   - Add `LaplaceExpressionParser.Parse(Node node)`.
   - Keep `Parse(string expression)` as a wrapper that parses the string then calls `Parse(Node)`.
   - Keep current validation messages unless the function-style path needs a more specific "laplace function expects two arguments" message.

2. Refactor Laplace input parsing.
   - Move voltage/current input parsing out of private string-only helpers or add parallel `Node`-based helpers.
   - Preserve existing string parser behavior for source-level forms.
   - Node-based validation should accept the same shapes as existing source-level validation.

3. Add lowerer types in the source generator area.
   - `LaplaceFunctionExpressionLowerer`
   - `LaplaceFunctionLoweringResult`
   - `LaplaceFunctionCallDefinition`
   - `LaplaceOutputKind`
   - Keep them internal.

4. Add entity factory helpers.
   - Share direct entity creation between `VoltageSourceGenerator`, `CurrentSourceGenerator`, `ArbitraryBehavioralGenerator`, and the lowerer.
   - Avoid duplicating numerator/denominator/delay assignment logic.
   - Do not change the public `IComponentGenerator.Generate(...)` interface.

5. Update generators.
   - Remove the current early rejection for `LAPLACE(...)`.
   - Before normal behavioral source creation, invoke the lowerer.
   - If the lowerer returns a direct final entity, return it.
   - If the lowerer returns helpers and a final behavioral expression, add helpers to `context.ContextEntities` and return the final behavioral source.
   - If the lowerer reports validation errors, return `null`.
   - If the lowerer finds no `LAPLACE(...)`, continue current behavior unchanged.

6. Keep parse actions correct.
   - Final behavioral sources created after mixed lowering must still set `Parameters.Expression`.
   - `Parameters.ParseAction` must use the existing simulation-aware resolver setup.
   - If the rewritten expression has functions or spice properties, retain the current before-setup parse-action refresh behavior.

## Writer Implementation Plan

1. Add writer-side lowering support in `SourceWriterHelper`.
   - Reuse the same AST parser and validation logic.
   - Use the writer evaluation context for parameters.
   - Emit comments when lowering fails, matching existing writer error style.

2. Update `ArbitraryBehavioralWriter`.
   - Route `V=` and `I=` expressions through the new writer helper before plain behavioral output.

3. Update `VoltageSourceWriter` and `CurrentSourceWriter`.
   - Route `VALUE` and expression-style behavioral paths through the writer helper before plain behavioral output.

4. Emit helper C# statements before final behavioral source statements for mixed expressions.

5. Ensure generated C# uses the same helper names and final expression shape as the reader path.

## Validation And Diagnostics

Use reader validation errors, not runtime exceptions, for expected invalid netlist forms.

Add or preserve clear messages for:

- `laplace function expects exactly two arguments`
- `laplace input expression must be V(node), V(node1,node2), or I(source)`
- `laplace transfer expression must be a rational polynomial in s`
- `laplace transfer function is improper; numerator degree exceeds denominator degree`
- `laplace transfer function has singular DC gain`
- `laplace transfer coefficients must be finite`
- `laplace delay can be specified only once`
- `laplace delay must be non-negative`
- `laplace source-level delay options can be used only when one LAPLACE call is present`
- `laplace option arguments inside LAPLACE(...) are not supported`

For mixed expressions, include the parent source line info when possible. For transfer-specific errors, keep the source expression line info if individual argument line info is not available.

## Test Plan

### Unit Tests

Add tests around the lowerer and updated source generators:

- Direct `E ... VALUE={LAPLACE(V(in), 1/(1+s*tau))}` creates `LaplaceVoltageControlledVoltageSource`.
- Direct `G ... VALUE={LAPLACE(V(in), gm/(1+s*tau))}` creates `LaplaceVoltageControlledCurrentSource`.
- Direct `H ... VALUE={LAPLACE(I(VSENSE), 1000/(s+1000))}` creates `LaplaceCurrentControlledVoltageSource`.
- Direct `F ... VALUE={LAPLACE(I(VSENSE), 1/(1+s))}` creates `LaplaceCurrentControlledCurrentSource`.
- `B ... V={LAPLACE(V(in), 1/(1+s))}` creates a voltage-output Laplace entity.
- `B ... I={LAPLACE(V(in), gm/(1+s))}` creates a current-output Laplace entity.
- Mixed `B ... V={1 + 2*LAPLACE(V(in), 1/(1+s))}` creates one helper plus final behavioral voltage source.
- Mixed `B ... I={LAPLACE(V(a), 1/(1+s*t1)) - LAPLACE(V(b), 1/(1+s*t2))}` creates two helpers plus final behavioral current source.
- Mixed expressions preserve non-Laplace functions such as `if()`, `abs()`, and parameters.
- Source-level `TD=` and `DELAY=` apply with one function call and fail with two calls.
- Current-source `M=` behavior is preserved for mixed expressions.
- Invalid function argument count fails.
- Invalid input shape fails.
- Invalid transfer fails.
- User-defined `.FUNC LAPLACE(...)` does not override built-in detection.

### Integration Tests

Add OP, AC, and focused transient coverage:

- OP parity: `E VALUE={LAPLACE(...)}` equals source-level `E ... LAPLACE ...`.
- OP parity: `B V={LAPLACE(...)}` equals equivalent `E ... LAPLACE ...`.
- OP parity: `B I={LAPLACE(...)}` through a grounded resistor matches expected current-source sign.
- AC low-pass cutoff magnitude and phase for `VALUE={LAPLACE(...)}`.
- AC mixed expression: DC offset plus low-pass term has expected low-frequency and cutoff behavior.
- Multiple-call AC expression combines two Laplace helper outputs correctly.
- Transient smoke test for a mixed first-order low-pass expression.
- Subcircuit expansion smoke test to verify helper node/entity names are scoped and do not collide.

### Writer Tests

- Direct function-style VALUE emits the matching Laplace constructor and parameters.
- Direct B source LAPLACE emits the matching Laplace constructor and parameters.
- Mixed expression emits helper Laplace source statements before the final behavioral source statement.
- Writer emits useful comments for invalid function-style Laplace expressions.

### Regression Tests

- Existing source-level `E/G/F/H ... LAPLACE ...` tests continue passing.
- Existing non-Laplace `VALUE`, `B V=`, and `B I=` tests continue passing.
- Existing unsupported forms still fail if they are still out of scope.

## Documentation Updates

Update:

- `src/docs/articles/laplace.md`
- `src/docs/articles/laplace-basics.md`
- `src/docs/articles/behavioral-source.md`
- `src/docs/articles/vcvs.md`
- `src/docs/articles/vccs.md`
- `src/docs/articles/voltage-source.md`
- `src/docs/articles/current-source.md`
- `src/docs/articles/intro.md`

Docs should cover:

- Function-style syntax.
- B source voltage and current examples.
- Mixed expression examples.
- Input and transfer limitations.
- `TD=` / `DELAY=` limitation for multiple calls.
- The fact that helper entities are an implementation detail and may appear during low-level circuit inspection.

Remove "not supported yet" statements for `VALUE={LAPLACE(...)}` and `B` source LAPLACE forms once implementation and tests are complete.

## Acceptance Criteria

- Supported netlists read without validation errors.
- Unsupported function-style forms fail during reading with clear validation errors.
- OP, AC, and transient tests pass for direct and mixed forms.
- Generated C# output matches reader behavior for direct and mixed forms.
- Existing source-level Laplace and non-Laplace behavioral source behavior is unchanged.
- Documentation accurately reflects the supported subset.

## Risks

- Mixed-expression lowering creates extra entities and nodes. The names must be collision-resistant and stable.
- AST formatting must produce expressions that the existing behavioral parser can parse again.
- Current-source `M=` has existing semantics. The implementation must not accidentally apply it twice or move it from the final behavioral expression to every helper.
- Subcircuit expansion may expose naming bugs if helper nodes are generated with already-scoped names instead of local names.
- Writer support can drift from reader behavior unless both share the parser/lowering model.

## Assumptions

- The existing SpiceSharp Laplace source components are the correct runtime implementation for all supported function-style forms.
- Helper voltage outputs are a valid numeric representation of each `LAPLACE(...)` term inside mixed expressions.
- Internal helper entities may be visible during low-level circuit inspection, but their names are reserved and deterministic.
- Inline function options and arbitrary input expressions remain future work.
