# SpiceSharpParser
Documentation on SpiceSharpParser is available at <https://spicesharp.github.io/SpiceSharpParser/index.html>.

## What is SpiceSharpParser?
SpiceSharpParser is a .NET library that parses Spice netlists and creates an model for SpiceSharp.

## Features
### Supported dot statements
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
* .SUBCKT
* .INCLUDE
* .APPENDMODEL

### Supported components
* RLC
* Switches
* Voltage and current sources
* BJT 
* Diodes
* Mosfets

### Implemented Spice3f5 grammar
<https://github.com/SpiceSharp/SpiceSharpParser/blob/master/src/SpiceSharpParser/Grammar/SpiceBNF.txt>

## Example

```csharp
  string netlist = "your netlist"
  var parserFront = new ParserFacade();
  ParserResult result = parserFront.ParseNetlist(
      netlist, 
      new ParserSettings() { HasTitle = true, IsEndRequired = true });

```

## Build status

|    | Status |
|:---|----------------:|
|**Windows**|[![Build status](https://ci.appveyor.com/api/projects/status/d8tpj2hm3hcullmw/branch/master?svg=true)](https://ci.appveyor.com/project/marcin-golebiowski/spicesharpparser/branch/master)|
|**Linux**|[![Build status](https://travis-ci.org/SpiceSharp/SpiceSharpParser.svg?branch=master)](https://travis-ci.org/SpiceSharp/SpiceSharpParser?branch=master)|


## Installation

SpiceSharpParser is available as NuGet Package 
[<img src="https://img.shields.io/nuget/vpre/SpiceSharp-Parser.svg">]( https://www.nuget.org/packages/SpiceSharp-Parser)







## Currently Supported and Tested Platforms
* Windows
* Linux

## Roadmap
Future features:
* .LIB
* .TEMP
* .PRINT
* .WIDTH
* Better .PARAM
* .IF/.ELSE/.ENDIF

## License
SpiceSharpParser is under MIT License
