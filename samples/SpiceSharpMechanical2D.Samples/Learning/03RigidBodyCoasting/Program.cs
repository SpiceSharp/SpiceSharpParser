using SpiceSharp;
using SpiceSharpMechanical2D.Bodies;
using SpiceSharpMechanical2D.Mathematics;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

// A rigid body has x/y translation plus rotation. Without loads, all three
// velocities remain constant.
var body = new RigidBody2D(
    "body",
    mass: 2.0,
    inertia: 0.5,
    initialPosition: new Vector2D(1.0, 2.0),
    initialAngle: 0.1,
    initialLinearVelocity: new Vector2D(0.5, -0.25),
    initialAngularVelocity: 0.4);

var simulation = new Transient("tran", new Trapezoidal
{
    InitialStep = 0.02,
    MaxStep = 0.02,
    StopTime = 2.0,
});
var x = new RealPropertyExport(simulation, body.Name, "x");
var y = new RealPropertyExport(simulation, body.Name, "y");
var angle = new RealPropertyExport(simulation, body.Name, "angle");
double finalX = 0.0;
double finalY = 0.0;
double finalAngle = 0.0;

foreach (int exportType in simulation.Run(new Circuit(body)))
{
    if (exportType == Transient.ExportTransient)
    {
        finalX = x.Value;
        finalY = y.Value;
        finalAngle = angle.Value;
    }
}

Console.WriteLine($"final x     = {finalX:G6} (expected 2)");
Console.WriteLine($"final y     = {finalY:G6} (expected 1.5)");
Console.WriteLine($"final angle = {finalAngle:G6} (expected 0.9)");
