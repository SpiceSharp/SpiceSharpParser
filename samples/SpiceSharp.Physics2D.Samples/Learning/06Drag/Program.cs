using SpiceSharp;
using SpiceSharp.Physics2D.Bodies;
using SpiceSharp.Physics2D.Forces;
using SpiceSharp.Physics2D.Mathematics;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

// Drag opposes velocity relative to a surrounding medium. It removes energy
// smoothly; it does not stop the body with an instantaneous impulse.
var body = new RigidBody2D(
    "body",
    mass: 1.0,
    inertia: 0.2,
    initialLinearVelocity: new Vector2D(3.0, 0.0),
    initialAngularVelocity: 2.0);
var linearDrag = new LinearDrag2D(
    "linear-drag",
    body.Name,
    damping: 1.0,
    mediumVelocity: new Vector2D(0.5, 0.0));
var angularDrag = new AngularDrag2D(
    "angular-drag",
    body.Name,
    damping: 0.2,
    mediumAngularVelocity: 0.0);

var simulation = new Transient("tran", new Trapezoidal
{
    InitialStep = 0.01,
    MaxStep = 0.01,
    StopTime = 2.0,
});
var velocityX = new RealPropertyExport(simulation, body.Name, "vx");
var omega = new RealPropertyExport(simulation, body.Name, "omega");
var energy = new RealPropertyExport(simulation, body.Name, "kineticenergy");
double finalVelocityX = 0.0;
double finalOmega = 0.0;
double finalEnergy = 0.0;

foreach (int exportType in simulation.Run(new Circuit(body, linearDrag, angularDrag)))
{
    if (exportType == Transient.ExportTransient)
    {
        finalVelocityX = velocityX.Value;
        finalOmega = omega.Value;
        finalEnergy = energy.Value;
    }
}

Console.WriteLine("The initial vx was 3 and the medium vx is 0.5.");
Console.WriteLine($"final vx             = {finalVelocityX:G6}");
Console.WriteLine($"final angular speed  = {finalOmega:G6}");
Console.WriteLine($"final kinetic energy = {finalEnergy:G6}");
