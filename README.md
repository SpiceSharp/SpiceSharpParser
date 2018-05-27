# SpiceSharpParser

## What is SpiceSharpParser?
SpiceSharpParser is a .NET library that parses Spice netlists and creates a model for SpiceSharp.

## Features
### Supported dot statements
* .GLOBAL
* .LET
* .NODESET 
* .PARAM (with user functions)
* .OPTION
* .SAVE
* .PLOT
* .IC
* .TRAN
* .AC
* .OP
* .NOISE
* .DC
* .SUBCKT
* .INCLUDE
* .APPENDMODEL
* .TEMP
* .LIB
* .IF/.ELSE/.ENDIF (in next release)
* .ST (in next release)

### Supported components
* RLC
* Switches
* Voltage and current sources
* BJT 
* Diodes
* Mosfets

### Implemented grammar
<https://github.com/SpiceSharp/SpiceSharpParser/blob/master/src/SpiceSharpParser/Parser/Netlist/Spice/SpiceGrammarBNF.txt>

## Build status

|    | Status |
|:---|----------------:|
|**Windows**|[![Build status](https://ci.appveyor.com/api/projects/status/d8tpj2hm3hcullmw/branch/master?svg=true)](https://ci.appveyor.com/project/marcin-golebiowski/spicesharpparser/branch/master)|
|**Linux**|[![Build status](https://travis-ci.org/SpiceSharp/SpiceSharpParser.svg?branch=master)](https://travis-ci.org/SpiceSharp/SpiceSharpParser?branch=master)|


## Installation

SpiceSharpParser is available as NuGet Package 
[<img src="https://img.shields.io/nuget/vpre/SpiceSharp-Parser.svg">]( https://www.nuget.org/packages/SpiceSharp-Parser)


## Documentation
Documentation for API on SpiceSharpParser is available at <https://spicesharp.github.io/SpiceSharpParser/api/index.html>.


## Currently Supported and Tested Platforms
* Windows
* Linux

## Roadmap
Future features:
* .PRINT
* saving output to files

## License
SpiceSharpParser is under MIT License
