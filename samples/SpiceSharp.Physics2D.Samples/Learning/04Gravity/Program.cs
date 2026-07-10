using SpiceSharp;
using SpiceSharp.Physics2D.Bodies;
using SpiceSharp.Physics2D.Forces;
using SpiceSharp.Physics2D.Mathematics;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

// The body owns mass and state. Gravity is a separate entity that stamps
// F = mass * acceleration into the body's equations.
var body = new RigidBody2D(
    "falling-body",
    mass: 2.0,
    inertia: 0.2,
    initialPosition: new Vector2D(0.0, 10.0));
var gravity = new Gravity2D(
    "gravity",
    body.Name,
    new Vector2D(0.0, -9.81));

var simulation = new Transient("tran", new Trapezoidal
{
    InitialStep = 0.01,
    MaxStep = 0.01,
    StopTime = 1.0,
});
var y = new RealPropertyExport(simulation, body.Name, "y");
var velocityY = new RealPropertyExport(simulation, body.Name, "vy");
double finalY = 0.0;
double finalVelocityY = 0.0;

foreach (int exportType in simulation.Run(new Circuit(body, gravity)))
{
    if (exportType == Transient.ExportTransient)
    {
        finalY = y.Value;
        finalVelocityY = velocityY.Value;
    }
}

Console.WriteLine($"final y  = {finalY:G6} (expected about 5.095)");
Console.WriteLine($"final vy = {finalVelocityY:G6} (expected -9.81)");
