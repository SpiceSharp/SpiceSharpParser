using SpiceSharp.Attributes;
using SpiceSharp.Entities;
using SpiceSharp.Physics2D.Bodies;
using SpiceSharp.Physics2D.Connections;
using SpiceSharp.Physics2D.Mathematics;
using System;
using System.Diagnostics;

namespace SpiceSharp.Physics2D.Joints;

[GeneratedParameters]
public sealed partial class WeldJoint2DBehavior : JointBehaviorBase, IWeldJoint2DBehavior
{
    private readonly MechanicalAnchor2D _endpointA;
    private readonly MechanicalAnchor2D _endpointB;
    private readonly WeldJoint2DParameters _parameters;
    private WeldJoint2DEvaluation _accepted;
    private WeldJoint2DEvaluation _trial;

    public WeldJoint2DBehavior(
        IBindingContext context,
        IRigidBody2DBehavior bodyA,
        IRigidBody2DBehavior bodyB,
        MechanicalAnchor2D endpointA,
        MechanicalAnchor2D endpointB,
        WeldJoint2DParameters parameters)
        : base(context, bodyA, bodyB)
    {
        _endpointA = endpointA;
        _endpointB = endpointB;
        _parameters = parameters;
        Evaluate(JointValidation.GetInitialState(bodyA), JointValidation.GetInitialState(bodyB));
        if (_trial.AnchorError.Length > 0.1
            || Math.Abs(_trial.RelativeAngleError) > 0.25
            || _trial.ForceOnA.Length > 1.0e4
            || Math.Max(Math.Abs(_trial.TorqueOnA), Math.Abs(_trial.TorqueOnB)) > 1.0e4)
        {
            Trace.TraceWarning(
                "Joint '{0}' has initial anchor/angle preload {1:R} m, {2:R} rad; estimated force {3:R} N.",
                Name,
                _trial.AnchorError.Length,
                _trial.RelativeAngleError,
                _trial.ForceOnA.Length);
        }
    }

    public Vector2D AnchorError => _accepted.AnchorError;

    public Vector2D AnchorVelocityError => _accepted.AnchorVelocityError;

    [ParameterName("relativeangleerror")]
    public double RelativeAngleError => _accepted.RelativeAngleError;

    public Vector2D ForceOnA => _accepted.ForceOnA;

    public Vector2D ForceOnB => -_accepted.ForceOnA;

    [ParameterName("torqueona")]
    public double TorqueOnA => _accepted.TorqueOnA;

    [ParameterName("torqueonb")]
    public double TorqueOnB => _accepted.TorqueOnB;

    [ParameterName("anchorerrorx")]
    public double AnchorErrorX => AnchorError.X;

    [ParameterName("anchorerrory")]
    public double AnchorErrorY => AnchorError.Y;

    [ParameterName("anchorvelocityerrorx")]
    public double AnchorVelocityErrorX => AnchorVelocityError.X;

    [ParameterName("anchorvelocityerrory")]
    public double AnchorVelocityErrorY => AnchorVelocityError.Y;

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
        _trial = WeldJoint2DEquation.Evaluate(
            _endpointA,
            stateA,
            _endpointB,
            stateB,
            _parameters.PositionStiffness,
            _parameters.PositionDamping,
            _parameters.ReferenceAngle,
            _parameters.AngularStiffness,
            _parameters.AngularDamping,
            Loads,
            Jacobian);
    }

    private protected override void AcceptDiagnostics() => _accepted = _trial;
}
