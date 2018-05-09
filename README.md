# SpiceSharpParser
Documentation on SpiceSharpParser is available at <https://spicesharp.github.io/SpiceSharpParser/index.html>.

## What is SpiceSharpParser?
SpiceSharpParser is a .NET Standard library that parses Spice3f5 netlists and creates an object model of netlist (input data for <https://github.com/SpiceSharp/SpiceSharp>)

It has no external dependency. 

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
* .SUBCKT
* .INCLUDE

### Supported Spice3f5 components
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

AppVeyor CI (Windows): [![Build status](https://ci.appveyor.com/api/projects/status/d8tpj2hm3hcullmw/branch/master?svg=true)](https://ci.appveyor.com/project/marcin-golebiowski/spicesharpparser/branch/master)

Travis CI (Linux): [![Build status](https://travis-ci.org/SpiceSharp/SpiceSharpParser.svg?branch=master)](https://travis-ci.org/SpiceSharp/SpiceSharpParser.svg?branch=master)


## Downloads

SpiceSharpParser is available as NuGet Package

<https://www.nuget.org/packages/SpiceSharp-Parser>



## Currently Supported and Tested Platforms
* Windows
* Linux

## License
SpiceSharpParser is under MIT License
