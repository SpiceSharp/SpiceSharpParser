# <img src="https://spicesharp.github.io/SpiceSharp/api/images/logo_full.svg" width="45px" /> Spice#/SpiceSharpParser
SpiceSharpParser is a .NET library that enables to simulate electronics circuits defined by Spice netlists.

## Documentation
Documentation for API on SpiceSharpParser is available at <https://spicesharp.github.io/SpiceSharpParser/api/index.html>.

## Quickstart

Parsing a netlist and executing a simulation is relatively straightforward. For example:

```csharp
var netlist = string.Join(Environment.NewLine,
                "Diode circuit",
                "D1 OUT 0 1N914",
                "V1 OUT 0 0",
                ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".DC V1 -1 1 10e-3",
                ".SAVE i(V1)",
                ".END");

var parser = new SpiceParser();
var parseResult = parser.ParseNetlist(netlist);
var spiceSharpModel = parseResult.SpiceSharpModel;
var simulation = spiceSharpModel.Simulations.Single();
simulation.Run(spiceSharpModel.Circuit);            
```

## Installation

SpiceSharpParser is available as NuGet Package 
[<img src="https://img.shields.io/nuget/vpre/SpiceSharp-Parser.svg">]( https://www.nuget.org/packages/SpiceSharp-Parser)

## Build status

|    | Status |
|:---|----------------:|
|**Windows**|[![Build status](https://ci.appveyor.com/api/projects/status/d8tpj2hm3hcullmw/branch/master?svg=true)](https://ci.appveyor.com/project/marcin-golebiowski/spicesharpparser/branch/master)|
|**Linux**|[![Build status](https://travis-ci.org/SpiceSharp/SpiceSharpParser.svg?branch=master)](https://travis-ci.org/SpiceSharp/SpiceSharpParser?branch=master)|

## License
SpiceSharpParser is under MIT License
