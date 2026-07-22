# SpiceSharpParser diagnostic reference

`SpiceCompiler` reports expected netlist failures as structured diagnostics.
Codes are stable and suitable for tests, suppressions, CI policies, editor
integration, and issue reports. Source positions are one-based and span ends
are exclusive.

Diagnostic code families:

- `SSP1xxx` — lexer and parser syntax
- `SSP2xxx` — preprocessing and source access
- `SSP3xxx` — unsupported constructs
- `SSP4xxx` — semantic translation
- `SSP5xxx` — structural linting
- `SSP6xxx` — compatibility behavior

`SpiceDiagnosticPolicy` may suppress non-error diagnostics or change their
effective severity. Raw errors cannot be suppressed or downgraded. The raw
`AllDiagnostics` collection always determines `Success`, `Model`, and
`Compatibility`; the effective `Diagnostics` collection determines
`PolicySuccess`.

## SSP1000

Lexical error. The source contains text that cannot be tokenized, such as an
invalid character or malformed token. Correct the source at the reported span.

## SSP1100

Syntax error. Tokens do not form a valid SPICE statement, or a required
structural statement such as `.END` is missing. Correct the statement near the
reported span.

## SSP2000

Preprocessing error. An include, library section, macro, or other preprocessing
operation failed. Inspect the message, dependency record, and include stack.

## SSP2001

Root source file not found. Verify the path passed to
`SpiceCompiler.CompileFile`.

## SSP2002

Root source file could not be read. Verify permissions, encoding, and whether
another process has locked the file.

## SSP3001

Unsupported component. Replace it with supported primitives or register a
custom component reader.

## SSP3002

Unsupported parameter. Remove the parameter, translate it to a supported
equivalent, or extend the relevant reader.

## SSP3003

Unsupported model or model level. Use a supported model, provide an equivalent
subcircuit, or add the required SpiceSharp implementation.

## SSP3004

Unsupported control statement. Remove the control or express its behavior
through supported analyses and directives.

## SSP3005

Unsupported source waveform. Rewrite the waveform using supported source
syntax or provide a custom waveform mapping.

## SSP3006

Unsupported option. Remove the option when it only affects a vendor UI, or
translate behavior-changing options to a supported equivalent.

## SSP3007

Unsupported export. Replace the requested output with a supported `.SAVE`,
`.PRINT`, `.PLOT`, or measurement expression.

## SSP3008

Unsupported syntax that does not fit a narrower `SSP3xxx` category. Review the
reported construct and dialect, then replace it or add a custom reader mapping.

## SSP4000

Semantic translation error. The parsed statement could not be converted into
SpiceSharp entities, analyses, or exports. Inspect the source span and message
for the invalid reference or value.

## SSP5001

Floating node. Connect the node to the intended circuit or remove the
unconnected branch.

## SSP5002

Missing DC path. Add an appropriate resistive or source path so the operating
point can be established.

## SSP5003

Missing model. Define the referenced `.MODEL`, include the model library, or
correct the model name.

## SSP5004

Duplicate component name. Rename one of the components so entity names are
unique under the selected case-sensitivity rules.

## SSP5005

Missing AC magnitude. Add an AC magnitude to a source used by an AC analysis.

## SSP5006

Transient maximum step may be too large or unspecified. Supply the fourth
`.TRAN` argument when the circuit needs tighter time resolution.

## SSP5007

Empty circuit. Add at least one simulatable circuit entity.

## SSP5008

No simulation command. Add an analysis such as `.OP`, `.DC`, `.AC`, `.TRAN`,
or `.NOISE`.

## SSP5009

No exports. Add `.SAVE`, `.PRINT`, or another supported output directive when
waveform data is required.

## SSP6001

Recognized construct ignored. The construct is accepted for compatibility but
has no runtime effect. It is safe to suppress only when that omission is
acceptable.

## SSP6002

Numeric compatibility divergence. Simulation can proceed, but an algorithm or
numeric sequence differs from the source simulator. Review the documented
difference before relying on exact parity.

## SSP6003

Compatibility approximation. Vendor syntax was lowered to supported behavior
that may not be perfectly equivalent. Review the suggested fix and validate
the affected behavior.
