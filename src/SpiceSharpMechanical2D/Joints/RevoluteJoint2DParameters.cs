using SpiceSharp.Attributes;
using SpiceSharp.ParameterSets;

namespace SpiceSharpMechanical2D.Joints;

public sealed class RevoluteJoint2DParameters : ParameterSet<RevoluteJoint2DParameters>
{
    private double _damping;
    private double _stiffness;

    [ParameterName("stiffness"), ParameterInfo("Isotropic anchor stiffness in newtons per meter")]
    [GreaterThanOrEquals(0.0)]
    [Finite]
    public double Stiffness
    {
        get => _stiffness;
        set
        {
            JointValidation.ValidateNonnegativeFinite(value, nameof(value));
            _stiffness = value;
        }
    }

    [ParameterName("damping"), ParameterInfo("Isotropic anchor damping in newton-seconds per meter")]
    [GreaterThanOrEquals(0.0)]
    [Finite]
    public double Damping
    {
        get => _damping;
        set
        {
            JointValidation.ValidateNonnegativeFinite(value, nameof(value));
            _damping = value;
        }
    }
}
