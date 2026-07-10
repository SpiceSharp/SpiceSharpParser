using SpiceSharp;
using SpiceSharpMechanical2D.Bodies;
using SpiceSharpMechanical2D.Connections;
using SpiceSharpMechanical2D.Forces;
using SpiceSharpMechanical2D.Joints;
using SpiceSharpMechanical2D.Mathematics;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

// A revolute joint keeps two anchor points close but leaves rotation free.
// It is compliant, so the anchor error is small rather than exactly zero.
const double length = 0.6;
const double initialAngle = 0.25;
var localPivot = new Vector2D(0.0, length);
var pendulum = new RigidBody2D(
    "pendulum",
    mass: 1.0,
    inertia: 0.12,
    initialPosition: -localPivot.Rotate(initialAngle),
    initialAngle: initialAngle);
var pivot = new RevoluteJoint2D(
    "pivot",
    MechanicalAnchor2D.World(Vector2D.Zero),
    MechanicalAnchor2D.Body(pendulum.Name, localPivot),
    stiffness: 100_000.0,
    damping: 90.0);
var gravity = new Gravity2D("gravity", pendulum.Name, new Vector2D(0.0, -9.81));

var simulation = new Transient("tran", new Trapezoidal
{
    InitialStep = 0.001,
    MaxStep = 0.001,
    StopTime = 1.0,
});
var angle = new RealPropertyExport(simulation, pendulum.Name, "angle");
var errorX = new RealPropertyExport(simulation, pivot.Name, "anchorerrorx");
var errorY = new RealPropertyExport(simulation, pivot.Name, "anchorerrory");
double finalAngle = 0.0;
double finalErrorX = 0.0;
double finalErrorY = 0.0;

foreach (int exportType in simulation.Run(new Circuit(pendulum, pivot, gravity)))
{
    if (exportType == Transient.ExportTransient)
    {
        finalAngle = angle.Value;
        finalErrorX = errorX.Value;
        finalErrorY = errorY.Value;
    }
}

double anchorError = Math.Sqrt((finalErrorX * finalErrorX) + (finalErrorY * finalErrorY));
Console.WriteLine($"final pendulum angle = {finalAngle:G6} rad");
Console.WriteLine($"final anchor error   = {anchorError:G6} m");
