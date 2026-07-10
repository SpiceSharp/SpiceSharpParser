using SpiceSharp;
using SpiceSharp.Physics2D.Bodies;
using SpiceSharp.Physics2D.Connections;
using SpiceSharp.Physics2D.Mathematics;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

// The bodies begin 1.2 m apart, while the spring rest length is 1.0 m.
// The spring pulls them toward each other with equal-and-opposite forces.
var bodyA = new RigidBody2D(
    "body-a",
    mass: 1.0,
    inertia: 0.1,
    initialPosition: new Vector2D(-0.6, 0.0));
var bodyB = new RigidBody2D(
    "body-b",
    mass: 2.0,
    inertia: 0.1,
    initialPosition: new Vector2D(0.6, 0.0));
var spring = new DistanceSpringDamper2D(
    "spring",
    MechanicalAnchor2D.Body(bodyA.Name, Vector2D.Zero),
    MechanicalAnchor2D.Body(bodyB.Name, Vector2D.Zero),
    restLength: 1.0,
    stiffness: 20.0,
    damping: 0.5,
    lengthRegularization: 1e-9);

var simulation = new Transient("tran", new Trapezoidal
{
    InitialStep = 0.005,
    MaxStep = 0.005,
    StopTime = 1.0,
});
var xA = new RealPropertyExport(simulation, bodyA.Name, "x");
var xB = new RealPropertyExport(simulation, bodyB.Name, "x");
var velocityA = new RealPropertyExport(simulation, bodyA.Name, "vx");
var velocityB = new RealPropertyExport(simulation, bodyB.Name, "vx");
double finalXA = 0.0;
double finalXB = 0.0;
double finalVelocityA = 0.0;
double finalVelocityB = 0.0;

foreach (int exportType in simulation.Run(new Circuit(bodyA, bodyB, spring)))
{
    if (exportType == Transient.ExportTransient)
    {
        finalXA = xA.Value;
        finalXB = xB.Value;
        finalVelocityA = velocityA.Value;
        finalVelocityB = velocityB.Value;
    }
}

Console.WriteLine($"body A: x={finalXA:G6}, vx={finalVelocityA:G6}");
Console.WriteLine($"body B: x={finalXB:G6}, vx={finalVelocityB:G6}");
Console.WriteLine($"distance between centers = {(finalXB - finalXA):G6}");
