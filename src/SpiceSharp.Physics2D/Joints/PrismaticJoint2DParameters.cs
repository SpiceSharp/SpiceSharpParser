using SpiceSharp.Attributes;
using SpiceSharp.ParameterSets;

namespace SpiceSharp.Physics2D.Joints;

public sealed class PrismaticJoint2DParameters : ParameterSet<PrismaticJoint2DParameters>
{
    private double _angularDamping;
    private double _angularStiffness;
    private double _axialDamping;
    private double _axialStiffness;
    private double _normalDamping;
    private double _normalStiffness;
    private double _referenceAngle;
    private double _referenceAxialTravel;

    [ParameterName("normalstiffness"), ParameterInfo("Guide-normal stiffness in newtons per meter")]
    [GreaterThanOrEquals(0.0)]
    [Finite]
    public double NormalStiffness
    {
        get => _normalStiffness;
        set
        {
            JointValidation.ValidateNonnegativeFinite(value, nameof(value));
            _normalStiffness = value;
        }
    }

    [ParameterName("normaldamping"), ParameterInfo("Guide-normal damping in newton-seconds per meter")]
    [GreaterThanOrEquals(0.0)]
    [Finite]
    public double NormalDamping
    {
        get => _normalDamping;
        set
        {
            JointValidation.ValidateNonnegativeFinite(value, nameof(value));
            _normalDamping = value;
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

    [ParameterName("referenceaxialtravel"), ParameterInfo("Unloaded axial travel in meters")]
    [Finite]
    public double ReferenceAxialTravel
    {
        get => _referenceAxialTravel;
        set
        {
            JointValidation.ValidateFinite(value, nameof(value));
            _referenceAxialTravel = value;
        }
    }

    [ParameterName("axialstiffness"), ParameterInfo("Optional axial stiffness in newtons per meter")]
    [GreaterThanOrEquals(0.0)]
    [Finite]
    public double AxialStiffness
    {
        get => _axialStiffness;
        set
        {
            JointValidation.ValidateNonnegativeFinite(value, nameof(value));
            _axialStiffness = value;
        }
    }

    [ParameterName("axialdamping"), ParameterInfo("Optional axial damping in newton-seconds per meter")]
    [GreaterThanOrEquals(0.0)]
    [Finite]
    public double AxialDamping
    {
        get => _axialDamping;
        set
        {
            JointValidation.ValidateNonnegativeFinite(value, nameof(value));
            _axialDamping = value;
        }
    }
}
