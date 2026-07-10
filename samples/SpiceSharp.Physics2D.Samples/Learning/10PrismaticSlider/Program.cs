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

// The guide axis is world X. Motion along X is free because axial stiffness
// and damping use their default value of zero. Y motion and relative angle are
// guided compliantly.
var slider = new RigidBody2D(
    "slider",
    mass: 1.0,
    inertia: 0.2,
    initialPosition: new Vector2D(0.0, 0.02),
    initialAngle: 0.01,
    initialLinearVelocity: new Vector2D(0.5, 0.0));
var guide = new PrismaticJoint2D(
    "guide",
    MechanicalAnchor2D.World(Vector2D.Zero),
    MechanicalAnchor2D.Body(slider.Name, Vector2D.Zero),
    guideAxis: Vector2D.UnitX,
    referenceAngle: 0.0,
    normalStiffness: 3000.0,
    normalDamping: 80.0,
    angularStiffness: 800.0,
    angularDamping: 25.0);
var push = new AppliedForce2D("push", slider.Name, new Vector2D(0.5, 0.0));

var simulation = new Transient("tran", new Trapezoidal
{
    InitialStep = 0.002,
    MaxStep = 0.002,
    StopTime = 1.0,
});
var x = new RealPropertyExport(simulation, slider.Name, "x");
var y = new RealPropertyExport(simulation, slider.Name, "y");
var angle = new RealPropertyExport(simulation, slider.Name, "angle");
var axialTravel = new RealPropertyExport(simulation, guide.Name, "axialtravel");
var normalError = new RealPropertyExport(simulation, guide.Name, "normalerror");
double finalX = 0.0;
double finalY = 0.0;
double finalAngle = 0.0;
double finalAxialTravel = 0.0;
double finalNormalError = 0.0;

foreach (int exportType in simulation.Run(new Circuit(slider, guide, push)))
{
    if (exportType == Transient.ExportTransient)
    {
        finalX = x.Value;
        finalY = y.Value;
        finalAngle = angle.Value;
        finalAxialTravel = axialTravel.Value;
        finalNormalError = normalError.Value;
    }
}

Console.WriteLine($"x / axial travel = {finalX:G6} / {finalAxialTravel:G6} m");
Console.WriteLine($"y / normal error = {finalY:G6} / {finalNormalError:G6} m");
Console.WriteLine($"angle             = {finalAngle:G6} rad");
