using SpiceSharp;
using SpiceSharpMechanical2D.Bodies;
using SpiceSharpMechanical2D.Forces;
using SpiceSharpMechanical2D.Mathematics;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

// A center force changes linear motion. A torque changes angular motion.
// Because they are separate entities, they superpose naturally.
var body = new RigidBody2D("body", mass: 2.0, inertia: 0.1);
var push = new AppliedForce2D("push", body.Name, new Vector2D(2.0, 0.0));
var spin = new AppliedTorque2D("spin", body.Name, torque: 0.3);

var simulation = new Transient("tran", new Trapezoidal
{
    InitialStep = 0.01,
    MaxStep = 0.01,
    StopTime = 1.0,
});
var x = new RealPropertyExport(simulation, body.Name, "x");
var velocityX = new RealPropertyExport(simulation, body.Name, "vx");
var angle = new RealPropertyExport(simulation, body.Name, "angle");
var omega = new RealPropertyExport(simulation, body.Name, "omega");
double finalX = 0.0;
double finalVelocityX = 0.0;
double finalAngle = 0.0;
double finalOmega = 0.0;

foreach (int exportType in simulation.Run(new Circuit(body, push, spin)))
{
    if (exportType == Transient.ExportTransient)
    {
        finalX = x.Value;
        finalVelocityX = velocityX.Value;
        finalAngle = angle.Value;
        finalOmega = omega.Value;
    }
}

Console.WriteLine($"x     = {finalX:G6} (expected about 0.5)");
Console.WriteLine($"vx    = {finalVelocityX:G6} (expected 1)");
Console.WriteLine($"angle = {finalAngle:G6} (expected about 1.5)");
Console.WriteLine($"omega = {finalOmega:G6} (expected 3)");
