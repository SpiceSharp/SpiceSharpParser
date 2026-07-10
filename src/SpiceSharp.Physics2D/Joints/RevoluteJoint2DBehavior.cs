using SpiceSharp.Attributes;
using SpiceSharp.Entities;
using SpiceSharp.Physics2D.Bodies;
using SpiceSharp.Physics2D.Connections;
using SpiceSharp.Physics2D.Mathematics;
using System.Diagnostics;

namespace SpiceSharp.Physics2D.Joints;

[GeneratedParameters]
public sealed partial class RevoluteJoint2DBehavior : JointBehaviorBase,
    IRevoluteJoint2DBehavior
{
    private readonly MechanicalAnchor2D _endpointA;
    private readonly MechanicalAnchor2D _endpointB;
    private readonly RevoluteJoint2DParameters _parameters;
    private RevoluteJoint2DEvaluation _accepted;
    private RevoluteJoint2DEvaluation _trial;

    public RevoluteJoint2DBehavior(
        IBindingContext context,
        IRigidBody2DBehavior bodyA,
        IRigidBody2DBehavior bodyB,
        MechanicalAnchor2D endpointA,
        MechanicalAnchor2D endpointB,
        RevoluteJoint2DParameters parameters)
        : base(context, bodyA, bodyB)
    {
        _endpointA = endpointA;
        _endpointB = endpointB;
        _parameters = parameters;
        Evaluate(JointValidation.GetInitialState(bodyA), JointValidation.GetInitialState(bodyB));
        if (_trial.AnchorError.Length > 0.1 || _trial.ForceOnA.Length > 1.0e4)
        {
            Trace.TraceWarning(
                "Joint '{0}' has initial anchor preload {1:R} m and estimated force {2:R} N.",
                Name,
                _trial.AnchorError.Length,
                _trial.ForceOnA.Length);
        }
    }

    public Vector2D AnchorError => _accepted.AnchorError;

    public Vector2D AnchorVelocityError => _accepted.AnchorVelocityError;

    public Vector2D ForceOnA => _accepted.ForceOnA;

    public Vector2D ForceOnB => -_accepted.ForceOnA;

    public double TorqueOnA => _accepted.TorqueOnA;

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

    [ParameterName("torqueona")]
    public double TorqueOnAExport => TorqueOnA;

    [ParameterName("torqueonb")]
    public double TorqueOnBExport => TorqueOnB;

    [ParameterName("storedelasticenergy")]
    public double StoredElasticEnergy => _accepted.StoredElasticEnergy;

    [ParameterName("dissipatedpower")]
    public double DissipatedPower => _accepted.DissipatedPower;

    private protected override void Evaluate(
        ConnectionBodyState2D stateA,
        ConnectionBodyState2D stateB)
    {
        _trial = RevoluteJoint2DEquation.Evaluate(
            _endpointA,
            stateA,
            _endpointB,
            stateB,
            _parameters.Stiffness,
            _parameters.Damping,
            Loads,
            Jacobian);
    }

    private protected override void AcceptDiagnostics() => _accepted = _trial;
}
