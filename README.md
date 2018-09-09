# SpiceSharpParser (in development)

## What is SpiceSharpParser?
SpiceSharpParser is a .NET library that parses Spice netlists and creates a model of  Spice simulation using SpiceSharp.

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
* .MODEL (with DEV and LOT support)
* .APPENDMODEL
* .TEMP
* .LIB
* .IF/.ELSE/.ENDIF
* .ST
* .FUNC
* .STEP
* .PRINT
* .MC

### Supported components
* RLC
* Switches
* Voltage and current sources
* BJT 
* Diodes
* Mosfets

### Supported Spice grammar
<https://github.com/SpiceSharp/SpiceSharpParser/blob/master/src/SpiceSharpParser/Parsers/Netlist/Spice/SpiceGrammarBNF.txt>

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

## License
SpiceSharpParser is under MIT License
