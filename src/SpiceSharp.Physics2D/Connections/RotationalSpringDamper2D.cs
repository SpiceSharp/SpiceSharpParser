using SpiceSharp;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Physics2D.Bodies;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Base;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Physics2D.Connections;

/// <summary>
/// Transfers restoring and damping torque between two body or world rotations.
/// </summary>
/// <remarks>
/// Relative angle error is wrapped to the half-open interval [-pi, pi). The
/// analytic derivative is valid on each branch; the exact +/-pi branch seam is
/// discontinuous and should not be used as a Newton linearization point.
/// </remarks>
public sealed class RotationalSpringDamper2D : Entity<RotationalSpringDamper2DParameters>
{
    /// <summary>
    /// Initializes a rotational spring-damper connection.
    /// </summary>
    public RotationalSpringDamper2D(
        string name,
        RotationalEndpoint2D endpointA,
        RotationalEndpoint2D endpointB,
        double referenceAngle,
        double stiffness,
        double damping = 0.0)
        : base(name)
    {
        endpointA.Validate(nameof(endpointA));
        endpointB.Validate(nameof(endpointB));
        if (endpointA.IsWorld && endpointB.IsWorld)
            throw new ArgumentException("At least one endpoint must reference a rigid body.");

        EndpointA = endpointA;
        EndpointB = endpointB;
        Parameters.ReferenceAngle = referenceAngle;
        Parameters.Stiffness = stiffness;
        Parameters.Damping = damping;
    }

    /// <summary>Gets endpoint A.</summary>
    public RotationalEndpoint2D EndpointA { get; }

    /// <summary>Gets endpoint B.</summary>
    public RotationalEndpoint2D EndpointB { get; }

    /// <summary>Gets or sets the unloaded relative angle in radians.</summary>
    public double ReferenceAngle
    {
        get => Parameters.ReferenceAngle;
        set => Parameters.ReferenceAngle = value;
    }

    /// <summary>Gets or sets the rotational stiffness in newton-meters per radian.</summary>
    public double Stiffness
    {
        get => Parameters.Stiffness;
        set => Parameters.Stiffness = value;
    }

    /// <summary>Gets or sets the rotational damping in newton-meter-seconds per radian.</summary>
    public double Damping
    {
        get => Parameters.Damping;
        set => Parameters.Damping = value;
    }

    /// <inheritdoc/>
    public override void CreateBehaviors(ISimulation simulation)
    {
        if (simulation == null)
            throw new ArgumentNullException(nameof(simulation));
        if (!simulation.UsesBehaviors<ITimeBehavior>())
            return;

        IRigidBody2DBehavior bodyA = EndpointA.IsWorld
            ? null
            : ConnectionBehaviorSupport.ResolveBody(EndpointA.BodyName, simulation);
        IRigidBody2DBehavior bodyB = EndpointB.IsWorld
            ? null
            : ConnectionBehaviorSupport.ResolveBody(EndpointB.BodyName, simulation);
        var behaviors = new BehaviorContainer(Name);
        var context = new BindingContext(this, simulation, behaviors);
        behaviors.Add(new LoadBehavior(context, bodyA, bodyB, this));
        simulation.EntityBehaviors.Add(behaviors);
    }

    private sealed class LoadBehavior : Behavior, IBiasingBehavior
    {
        private readonly int[] _activeLoadIndices;
        private readonly int[] _activeStateIndices;
        private readonly IRigidBody2DBehavior _bodyA;
        private readonly IRigidBody2DBehavior _bodyB;
        private readonly RotationalSpringDamper2D _connection;
        private readonly ElementSet<double> _elements;
        private readonly double[,] _jacobian = new double[
            RotationalSpringDamper2DEquation.LoadCount,
            RotationalSpringDamper2DEquation.StateCount];
        private readonly double[] _loads = new double[RotationalSpringDamper2DEquation.LoadCount];
        private readonly double[] _state = new double[RotationalSpringDamper2DEquation.StateCount];
        private readonly ITimeSimulationState _time;
        private readonly double[] _values;

        public LoadBehavior(
            IBindingContext context,
            IRigidBody2DBehavior bodyA,
            IRigidBody2DBehavior bodyB,
            RotationalSpringDamper2D connection)
            : base(context)
        {
            _bodyA = bodyA;
            _bodyB = bodyB;
            _connection = connection;
            _time = context.GetState<ITimeSimulationState>();
            var biasing = context.GetState<IBiasingSimulationState>();
            var loadIndices = new List<int>(2);
            var rows = new List<int>(2);
            var stateIndices = new List<int>(4);
            var columns = new List<int>(4);
            AddBodyTopology(biasing, bodyA, 0, 0, loadIndices, rows, stateIndices, columns);
            AddBodyTopology(biasing, bodyB, 1, 2, loadIndices, rows, stateIndices, columns);
            _activeLoadIndices = loadIndices.ToArray();
            _activeStateIndices = stateIndices.ToArray();

            var matrixLocations = new MatrixLocation[rows.Count * columns.Count];
            int locationIndex = 0;
            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < columns.Count; columnIndex++)
                {
                    matrixLocations[locationIndex++] =
                        new MatrixLocation(rows[rowIndex], columns[columnIndex]);
                }
            }

            _values = new double[matrixLocations.Length + rows.Count];
            _elements = new ElementSet<double>(
                biasing.Solver,
                matrixLocations,
                rows.ToArray());
        }

        void IBiasingBehavior.Load()
        {
            if (_time.UseDc)
                return;

            _state[0] = _bodyA == null ? _connection.EndpointA.FixedAngle : _bodyA.Angle;
            _state[1] = _bodyA == null ? 0.0 : _bodyA.AngularVelocity;
            _state[2] = _bodyB == null ? _connection.EndpointB.FixedAngle : _bodyB.Angle;
            _state[3] = _bodyB == null ? 0.0 : _bodyB.AngularVelocity;
            RotationalSpringDamper2DParameters parameters = _connection.Parameters;
            RotationalSpringDamper2DEquation.Evaluate(
                _state[0],
                _state[1],
                _state[2],
                _state[3],
                parameters.ReferenceAngle,
                parameters.Stiffness,
                parameters.Damping,
                _loads,
                _jacobian);

            int valueIndex = 0;
            for (int rowIndex = 0; rowIndex < _activeLoadIndices.Length; rowIndex++)
            {
                int loadIndex = _activeLoadIndices[rowIndex];
                for (int columnIndex = 0; columnIndex < _activeStateIndices.Length; columnIndex++)
                {
                    int stateIndex = _activeStateIndices[columnIndex];
                    _values[valueIndex++] = -_jacobian[loadIndex, stateIndex];
                }
            }

            for (int rowIndex = 0; rowIndex < _activeLoadIndices.Length; rowIndex++)
            {
                int loadIndex = _activeLoadIndices[rowIndex];
                double linearizedLoad = _loads[loadIndex];
                for (int columnIndex = 0; columnIndex < _activeStateIndices.Length; columnIndex++)
                {
                    int stateIndex = _activeStateIndices[columnIndex];
                    linearizedLoad -= _jacobian[loadIndex, stateIndex] * _state[stateIndex];
                }

                _values[valueIndex++] = linearizedLoad;
            }

            _elements.Add(_values);
        }

        private static void AddBodyTopology(
            IBiasingSimulationState biasing,
            IRigidBody2DBehavior body,
            int loadIndex,
            int stateOffset,
            ICollection<int> loadIndices,
            ICollection<int> rows,
            ICollection<int> stateIndices,
            ICollection<int> columns)
        {
            if (body == null)
                return;

            loadIndices.Add(loadIndex);
            rows.Add(biasing.Map[body.AngularVelocityVariable]);
            stateIndices.Add(stateOffset);
            stateIndices.Add(stateOffset + 1);
            columns.Add(biasing.Map[body.AngleVariable]);
            columns.Add(biasing.Map[body.AngularVelocityVariable]);
        }
    }
}
