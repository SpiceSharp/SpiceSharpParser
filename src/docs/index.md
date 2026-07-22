# SpiceSharpParser Documentation

SpiceSharpParser is a .NET library that parses SPICE netlists and simulates them using [SpiceSharp](https://github.com/SpiceSharp/SpiceSharp).

## Getting Started

See the [Introduction](articles/intro.md) for installation instructions, a quick example, and an overview of the API.
For a deeper explanation of matrices, sparse solving, and the simulation algorithm, see [How SpiceSharp Solves Circuits](articles/spicesharp-architecture.md).

Math formulas in these articles use KaTeX-compatible Markdown delimiters: inline formulas use `$...$`, and display formulas use `$$...$$`.

## Articles

Browse the documentation by category:

- **Analysis**: [.AC](articles/ac.md), [.DC](articles/dc.md), [.TRAN](articles/tran.md), [.OP](articles/op.md), [.NOISE](articles/noise.md)
- **Architecture**: [How SpiceSharp Solves Circuits](articles/spicesharp-architecture.md), [Transient Integration Methods](articles/transient-integration-methods.md)
- **Output**: [.SAVE](articles/save.md), [.PRINT](articles/print.md), [.PLOT](articles/plot.md), [.MEAS](articles/meas.md), [.FOUR](articles/four.md)
- **Parameters**: [.PARAM](articles/param.md), [.FUNC](articles/func.md), [.LET](articles/let.md), [.SPARAM](articles/sparam.md)
- **Structure**: [.SUBCKT](articles/subckt.md), [X (Subcircuit Instance)](articles/subcircuit-instance.md), [Programmatic Subcircuit Libraries](articles/subcircuit-library.md), [Digital and 555 Subcircuit Library](articles/digital-subcircuits.md), [.INCLUDE](articles/include.md), [.LIB](articles/lib.md), [.GLOBAL](articles/global.md), [.APPENDMODEL](articles/appendmodel.md)
- **Control**: [.STEP](articles/step.md), [.MC](articles/mc.md), [.TEMP](articles/temp.md), [.OPTIONS](articles/options.md), [.IC](articles/ic.md), [.NODESET](articles/nodeset.md), [.ST](articles/st.md), [.IF](articles/if.md), [.DISTRIBUTION](articles/distribution.md)
- **Devices**: [R](articles/resistor.md), [C](articles/capacitor.md), [L](articles/inductor.md), [LTspice-Style Nonlinear Passives](articles/nonlinear-passives.md), [K (Mutual Inductance)](articles/mutual-inductance.md), [D](articles/diode.md), [LTspice-Style Ideal Diode](articles/ideal-diode.md), [Q](articles/bjt.md), [J](articles/jfet.md), [M](articles/mosfet.md), [V](articles/voltage-source.md), [I](articles/current-source.md), [B](articles/behavioral-source.md), [E (VCVS)](articles/vcvs.md), [F (CCCS)](articles/cccs.md), [G (VCCS)](articles/vccs.md), [H (CCVS)](articles/ccvs.md), [Laplace Transform Basics](articles/laplace-basics.md), [LAPLACE Transfer Sources](articles/laplace.md), [S (Voltage Switch)](articles/voltage-switch.md), [W (Current Switch)](articles/current-switch.md), [T (Transmission Line)](articles/transmission-line.md)

## API Reference

The [API documentation](api/index.html) provides detailed reference for all public classes and methods.
