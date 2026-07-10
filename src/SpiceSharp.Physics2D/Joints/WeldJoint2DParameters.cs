using SpiceSharp.Attributes;
using SpiceSharp.ParameterSets;

namespace SpiceSharp.Physics2D.Joints;

public sealed class WeldJoint2DParameters : ParameterSet<WeldJoint2DParameters>
{
    private double _angularDamping;
    private double _angularStiffness;
    private double _positionDamping;
    private double _positionStiffness;
    private double _referenceAngle;

    [ParameterName("positionstiffness"), ParameterInfo("Isotropic anchor stiffness in newtons per meter")]
    [GreaterThanOrEquals(0.0)]
    [Finite]
    public double PositionStiffness
    {
        get => _positionStiffness;
        set
        {
            JointValidation.ValidateNonnegativeFinite(value, nameof(value));
            _positionStiffness = value;
        }
    }

    [ParameterName("positiondamping"), ParameterInfo("Isotropic anchor damping in newton-seconds per meter")]
    [GreaterThanOrEquals(0.0)]
    [Finite]
    public double PositionDamping
    {
        get => _positionDamping;
        set
        {
            JointValidation.ValidateNonnegativeFinite(value, nameof(value));
            _positionDamping = value;
        }
    }

    [ParameterName("referenceangle"), ParameterInfo("Explicit unloaded relative angle in radians")]
    [Finite]
    public double ReferenceAngle
    {
        get => _referenceAngle;
        set
        {
            JointValidation.ValidateFinite(value, nameof(value));
            _referenceAngle = value;
        }
    }

    [ParameterName("angularstiffness"), ParameterInfo("Reference-angle tangent stiffness in newton-meters per radian")]
    [GreaterThanOrEquals(0.0)]
    [Finite]
    public double AngularStiffness
    {
        get => _angularStiffness;
        set
        {
            JointValidation.ValidateNonnegativeFinite(value, nameof(value));
            _angularStiffness = value;
        }
    }

    [ParameterName("angulardamping"), ParameterInfo("Relative angular damping in newton-meter-seconds per radian")]
    [GreaterThanOrEquals(0.0)]
    [Finite]
    public double AngularDamping
    {
        get => _angularDamping;
        set
        {
            JointValidation.ValidateNonnegativeFinite(value, nameof(value));
            _angularDamping = value;
        }
    }
}
