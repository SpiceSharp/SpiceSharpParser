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

AppVeyor CI (Windows): <img src="https://ci.appveyor.com/api/projects/status/d8tpj2hm3hcullmw/branch/master?svg=true"></img>

## Currently Supported and Tested Platforms
* Windows

## License
SpiceSharpParser is under MIT License
