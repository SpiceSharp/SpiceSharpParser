using System;
using SpiceSharp.Attributes;
using SpiceSharp.ParameterSets;

namespace SpiceSharp.Physics2D.Connections;

/// <summary>
/// Parameters for a rotational spring-damper connection.
/// </summary>
public sealed class RotationalSpringDamper2DParameters : ParameterSet<RotationalSpringDamper2DParameters>
{
    private double _referenceAngle;
    private double _stiffness;
    private double _damping;

    /// <summary>
    /// Gets or sets the unloaded relative angle from endpoint A to endpoint B, in radians.
    /// </summary>
    [ParameterName("referenceangle"), ParameterInfo("Unloaded relative angle in radians")]
    [Finite]
    public double ReferenceAngle
    {
        get => _referenceAngle;
        set
        {
            ValidateFinite(value, nameof(value));
            _referenceAngle = value;
        }
    }

    /// <summary>
    /// Gets or sets the rotational stiffness in newton-meters per radian.
    /// </summary>
    [ParameterName("stiffness"), ParameterInfo("Rotational stiffness in newton-meters per radian")]
    [GreaterThanOrEquals(0.0)]
    [Finite]
    public double Stiffness
    {
        get => _stiffness;
        set
        {
            ValidateNonnegativeFinite(value, nameof(value));
            _stiffness = value;
        }
    }

    /// <summary>
    /// Gets or sets the rotational damping in newton-meter-seconds per radian.
    /// </summary>
    [ParameterName("damping"), ParameterInfo("Rotational damping in newton-meter-seconds per radian")]
    [GreaterThanOrEquals(0.0)]
    [Finite]
    public double Damping
    {
        get => _damping;
        set
        {
            ValidateNonnegativeFinite(value, nameof(value));
            _damping = value;
        }
    }

    private static void ValidateNonnegativeFinite(double value, string parameterName)
    {
        ValidateFinite(value, parameterName);
        if (value < 0.0)
            throw new ArgumentOutOfRangeException(parameterName, "The value must be nonnegative.");
    }

    private static void ValidateFinite(double value, string parameterName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentOutOfRangeException(parameterName, "The value must be finite.");
    }
}
