using System;
using SpiceSharp.Attributes;
using SpiceSharp.ParameterSets;

namespace SpiceSharpMechanical2D.Connections;

/// <summary>
/// Parameters for a translational spring-damper connection.
/// </summary>
public sealed class DistanceSpringDamper2DParameters : ParameterSet<DistanceSpringDamper2DParameters>
{
    private double _restLength;
    private double _stiffness;
    private double _damping;
    private double _lengthRegularization = 1.0e-9;

    /// <summary>
    /// Gets or sets the unloaded distance between the attachment points in meters.
    /// </summary>
    [ParameterName("restlength"), ParameterInfo("Unloaded anchor distance in meters")]
    [GreaterThanOrEquals(0.0)]
    [Finite]
    public double RestLength
    {
        get => _restLength;
        set
        {
            ValidateNonnegativeFinite(value, nameof(value));
            _restLength = value;
        }
    }

    /// <summary>
    /// Gets or sets the spring stiffness in newtons per meter.
    /// </summary>
    [ParameterName("stiffness"), ParameterInfo("Spring stiffness in newtons per meter")]
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
    /// Gets or sets the axial damping coefficient in newton-seconds per meter.
    /// </summary>
    [ParameterName("damping"), ParameterInfo("Axial damping in newton-seconds per meter")]
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

    /// <summary>
    /// Gets or sets the positive length regularization in meters.
    /// </summary>
    [ParameterName("lengthregularization"), ParameterInfo("Positive distance regularization in meters")]
    [GreaterThan(0.0)]
    [Finite]
    public double LengthRegularization
    {
        get => _lengthRegularization;
        set
        {
            if (!IsFinite(value) || value <= 0.0)
                throw new ArgumentOutOfRangeException(nameof(value), "Length regularization must be positive and finite.");
            _lengthRegularization = value;
        }
    }

    private static void ValidateNonnegativeFinite(double value, string parameterName)
    {
        if (!IsFinite(value) || value < 0.0)
            throw new ArgumentOutOfRangeException(parameterName, "The value must be nonnegative and finite.");
    }

    private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
}
