using SpiceSharp;
using SpiceSharp.Physics2D.Core;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

// A MechanicalCoordinate is the smallest mechanical state: position q and
// velocity u. With no applied force, u stays constant and q = q0 + u*t.
var coordinate = new MechanicalCoordinate(
    "coordinate",
    generalizedMass: 3.0,
    initialPosition: 1.0,
    initialVelocity: 2.0);

var simulation = new Transient("tran", new Trapezoidal
{
    InitialStep = 0.02,
    MaxStep = 0.02,
    StopTime = 1.0,
});
var position = new RealPropertyExport(simulation, coordinate.Name, "position");
var velocity = new RealPropertyExport(simulation, coordinate.Name, "velocity");
double finalPosition = 0.0;
double finalVelocity = 0.0;

foreach (int exportType in simulation.Run(new Circuit(coordinate)))
{
    if (exportType == Transient.ExportTransient)
    {
        finalPosition = position.Value;
        finalVelocity = velocity.Value;
    }
}

Console.WriteLine($"final position = {finalPosition:G6} (expected 3)");
Console.WriteLine($"final velocity = {finalVelocity:G6} (expected 2)");
