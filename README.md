# SpiceSharpParser
Documentation on SpiceSharpParser is available at <https://spicesharp.github.io/SpiceSharpParser/index.html>.

## What is SpiceSharpParser?
SpiceSharpParser is a .NET Standard library that parses Spice3f5 netlists and creates an object model of netlist for <https://github.com/SpiceSharp/SpiceSharpParser>

## Features
### Supported Spice3f5 controls
* .GLOBAL
* .LET
* .NODESET 
* .PARAM
* .OPTION
* .SAVE
* .PLOT
* .IC
* .TRAN
* .AC
* .OP
* .NOISE
* .DC
* .SUCKT

### Supported Spice3f5 components
* RLC
* Switches
* Voltage and current sources
* BJT 
* Diodes
* Mosfets

### Implemented Spice3f5 grammar
<https://github.com/SpiceSharp/SpiceSharpParser/blob/master/src/SpiceSharpParser/Grammar/SpiceBNF.txt>

## Currently Supported and Tested Platforms
* Windows

## License
SpiceSharpParser is under MIT License
