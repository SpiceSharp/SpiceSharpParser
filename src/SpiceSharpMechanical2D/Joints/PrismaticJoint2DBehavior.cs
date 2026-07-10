using SpiceSharp.Attributes;
using SpiceSharp.Entities;
using SpiceSharpMechanical2D.Bodies;
using SpiceSharpMechanical2D.Connections;
using SpiceSharpMechanical2D.Mathematics;
using System;
using System.Diagnostics;

namespace SpiceSharpMechanical2D.Joints;

[GeneratedParameters]
public sealed partial class PrismaticJoint2DBehavior : JointBehaviorBase,
    IPrismaticJoint2DBehavior
{
    private readonly MechanicalAnchor2D _endpointA;
    private readonly MechanicalAnchor2D _endpointB;
    private readonly Vector2D _guideAxis;
    private readonly PrismaticJoint2DParameters _parameters;
    private PrismaticJoint2DEvaluation _accepted;
    private PrismaticJoint2DEvaluation _trial;

    public PrismaticJoint2DBehavior(
        IBindingContext context,
        IRigidBody2DBehavior bodyA,
        IRigidBody2DBehavior bodyB,
        MechanicalAnchor2D endpointA,
        MechanicalAnchor2D endpointB,
        Vector2D guideAxis,
        PrismaticJoint2DParameters parameters)
        : base(context, bodyA, bodyB)
    {
        _endpointA = endpointA;
        _endpointB = endpointB;
        _guideAxis = guideAxis;
        _parameters = parameters;
        Evaluate(JointValidation.GetInitialState(bodyA), JointValidation.GetInitialState(bodyB));
        if (Math.Abs(_trial.NormalError) > 0.1
            || Math.Abs(_trial.RelativeAngleError) > 0.25
            || _trial.ForceOnA.Length > 1.0e4
            || Math.Max(Math.Abs(_trial.TorqueOnA), Math.Abs(_trial.TorqueOnB)) > 1.0e4)
        {
            Trace.TraceWarning(
                "Joint '{0}' has initial normal/angle preload {1:R} m, {2:R} rad; estimated force {3:R} N.",
                Name,
                _trial.NormalError,
                _trial.RelativeAngleError,
                _trial.ForceOnA.Length);
        }
    }

    [ParameterName("normalerror")]
    public double NormalError => _accepted.NormalError;

    [ParameterName("normalvelocityerror")]
    public double NormalVelocityError => _accepted.NormalVelocityError;

    [ParameterName("axialtravel")]
    public double AxialTravel => _accepted.AxialTravel;

    [ParameterName("axialvelocity")]
    public double AxialVelocity => _accepted.AxialVelocity;

    [ParameterName("relativeangleerror")]
    public double RelativeAngleError => _accepted.RelativeAngleError;

    public Vector2D ForceOnA => _accepted.ForceOnA;

    public Vector2D ForceOnB => -_accepted.ForceOnA;

    [ParameterName("torqueona")]
    public double TorqueOnA => _accepted.TorqueOnA;

    [ParameterName("torqueonb")]
    public double TorqueOnB => _accepted.TorqueOnB;

    [ParameterName("forceonax")]
    public double ForceOnAX => ForceOnA.X;

    [ParameterName("forceonay")]
    public double ForceOnAY => ForceOnA.Y;

    [ParameterName("forceonbx")]
    public double ForceOnBX => ForceOnB.X;

    [ParameterName("forceonby")]
    public double ForceOnBY => ForceOnB.Y;

    [ParameterName("storedelasticenergy")]
    public double StoredElasticEnergy => _accepted.StoredElasticEnergy;

    [ParameterName("dissipatedpower")]
    public double DissipatedPower => _accepted.DissipatedPower;

    private protected override void Evaluate(
        ConnectionBodyState2D stateA,
        ConnectionBodyState2D stateB)
    {
        _trial = PrismaticJoint2DEquation.Evaluate(
            _endpointA,
            stateA,
            _endpointB,
            stateB,
            _guideAxis,
            _parameters.NormalStiffness,
            _parameters.NormalDamping,
            _parameters.ReferenceAngle,
            _parameters.AngularStiffness,
            _parameters.AngularDamping,
            _parameters.ReferenceAxialTravel,
            _parameters.AxialStiffness,
            _parameters.AxialDamping,
            Loads,
            Jacobian);
    }

    private protected override void AcceptDiagnostics() => _accepted = _trial;
}
