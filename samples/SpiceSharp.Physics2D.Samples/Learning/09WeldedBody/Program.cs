using SpiceSharp;
using SpiceSharp.Physics2D.Bodies;
using SpiceSharp.Physics2D.Connections;
using SpiceSharp.Physics2D.Forces;
using SpiceSharp.Physics2D.Joints;
using SpiceSharp.Physics2D.Mathematics;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

// A weld combines a compliant position lock with a compliant angle lock.
// A steady force therefore creates a small displacement F/k.
var body = new RigidBody2D("body", mass: 1.0, inertia: 0.3);
var weld = new WeldJoint2D(
    "weld",
    MechanicalAnchor2D.World(Vector2D.Zero),
    MechanicalAnchor2D.Body(body.Name, Vector2D.Zero),
    referenceAngle: 0.0,
    positionStiffness: 4000.0,
    positionDamping: 120.0,
    angularStiffness: 1000.0,
    angularDamping: 30.0);
var force = new AppliedForce2D("force", body.Name, new Vector2D(2.0, 0.0));
var torque = new AppliedTorque2D("torque", body.Name, 0.25);

var simulation = new Transient("tran", new Trapezoidal
{
    InitialStep = 0.001,
    MaxStep = 0.001,
    StopTime = 1.0,
});
var x = new RealPropertyExport(simulation, body.Name, "x");
var angle = new RealPropertyExport(simulation, body.Name, "angle");
var reactionX = new RealPropertyExport(simulation, weld.Name, "forceonbx");
var reactionTorque = new RealPropertyExport(simulation, weld.Name, "torqueonb");
double finalX = 0.0;
double finalAngle = 0.0;
double finalReactionX = 0.0;
double finalReactionTorque = 0.0;

foreach (int exportType in simulation.Run(new Circuit(body, weld, force, torque)))
{
    if (exportType == Transient.ExportTransient)
    {
        finalX = x.Value;
        finalAngle = angle.Value;
        finalReactionX = reactionX.Value;
        finalReactionTorque = reactionTorque.Value;
    }
}

Console.WriteLine($"x displacement = {finalX:G6} m (expected about 0.0005)");
Console.WriteLine($"angle          = {finalAngle:G6} rad (expected about 0.00025)");
Console.WriteLine($"weld force     = {finalReactionX:G6} N");
Console.WriteLine($"weld torque    = {finalReactionTorque:G6} N*m");
