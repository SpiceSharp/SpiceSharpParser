using SpiceSharp;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Physics2D.Bodies;
using SpiceSharp.Physics2D.Mathematics;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Base;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Physics2D.Connections;

/// <summary>
/// Transfers nonlinear axial spring and damping forces between two body-local or world anchors.
/// </summary>
/// <remarks>
/// The connection uses <c>sqrt(d dot d + epsilon_L^2)</c> for its working length.
/// This keeps the force and Jacobian finite at coincident anchors, but intentionally
/// changes the constitutive response when the anchor separation is comparable to
/// <see cref="LengthRegularization"/>.
/// </remarks>
public sealed class DistanceSpringDamper2D : Entity<DistanceSpringDamper2DParameters>
{
    /// <summary>
    /// Initializes a distance spring-damper connection.
    /// </summary>
    public DistanceSpringDamper2D(
        string name,
        MechanicalAnchor2D endpointA,
        MechanicalAnchor2D endpointB,
        double restLength,
        double stiffness,
        double damping = 0.0,
        double lengthRegularization = 1.0e-9)
        : base(name)
    {
        endpointA.Validate(nameof(endpointA));
        endpointB.Validate(nameof(endpointB));
        if (endpointA.IsWorld && endpointB.IsWorld)
            throw new ArgumentException("At least one endpoint must reference a rigid body.");

        EndpointA = endpointA;
        EndpointB = endpointB;
        Parameters.RestLength = restLength;
        Parameters.Stiffness = stiffness;
        Parameters.Damping = damping;
        Parameters.LengthRegularization = lengthRegularization;
    }

    /// <summary>Gets endpoint A.</summary>
    public MechanicalAnchor2D EndpointA { get; }

    /// <summary>Gets endpoint B.</summary>
    public MechanicalAnchor2D EndpointB { get; }

    /// <summary>Gets or sets the unloaded distance in meters.</summary>
    public double RestLength
    {
        get => Parameters.RestLength;
        set => Parameters.RestLength = value;
    }

    /// <summary>Gets or sets the spring stiffness in newtons per meter.</summary>
    public double Stiffness
    {
        get => Parameters.Stiffness;
        set => Parameters.Stiffness = value;
    }

    /// <summary>Gets or sets the axial damping in newton-seconds per meter.</summary>
    public double Damping
    {
        get => Parameters.Damping;
        set => Parameters.Damping = value;
    }

    /// <summary>Gets or sets the positive distance regularization in meters.</summary>
    public double LengthRegularization
    {
        get => Parameters.LengthRegularization;
        set => Parameters.LengthRegularization = value;
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
        private readonly DistanceSpringDamper2D _connection;
        private readonly ElementSet<double> _elements;
        private readonly double[,] _jacobian = new double[
            DistanceSpringDamper2DEquation.LoadCount,
            DistanceSpringDamper2DEquation.StateCount];
        private readonly double[] _loads = new double[DistanceSpringDamper2DEquation.LoadCount];
        private readonly double[] _state = new double[DistanceSpringDamper2DEquation.StateCount];
        private readonly ITimeSimulationState _time;
        private readonly double[] _values;

        public LoadBehavior(
            IBindingContext context,
            IRigidBody2DBehavior bodyA,
            IRigidBody2DBehavior bodyB,
            DistanceSpringDamper2D connection)
            : base(context)
        {
            _bodyA = bodyA;
            _bodyB = bodyB;
            _connection = connection;
            _time = context.GetState<ITimeSimulationState>();
            var biasing = context.GetState<IBiasingSimulationState>();
            var loadIndices = new List<int>(6);
            var rows = new List<int>(6);
            var stateIndices = new List<int>(12);
            var columns = new List<int>(12);
            AddBodyTopology(biasing, bodyA, 0, 0, loadIndices, rows, stateIndices, columns);
            AddBodyTopology(biasing, bodyB, 3, 6, loadIndices, rows, stateIndices, columns);
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

            CaptureState(_bodyA, 0);
            CaptureState(_bodyB, 6);
            ConnectionBodyState2D stateA = ToBodyState(0);
            ConnectionBodyState2D stateB = ToBodyState(6);
            DistanceSpringDamper2DParameters parameters = _connection.Parameters;
            DistanceSpringDamper2DEquation.Evaluate(
                _connection.EndpointA,
                stateA,
                _connection.EndpointB,
                stateB,
                parameters.RestLength,
                parameters.Stiffness,
                parameters.Damping,
                parameters.LengthRegularization,
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
            int loadOffset,
            int stateOffset,
            ICollection<int> loadIndices,
            ICollection<int> rows,
            ICollection<int> stateIndices,
            ICollection<int> columns)
        {
            if (body == null)
                return;

            loadIndices.Add(loadOffset);
            loadIndices.Add(loadOffset + 1);
            loadIndices.Add(loadOffset + 2);
            rows.Add(biasing.Map[body.VelocityXVariable]);
            rows.Add(biasing.Map[body.VelocityYVariable]);
            rows.Add(biasing.Map[body.AngularVelocityVariable]);
            AddState(biasing, body.PositionXVariable, stateOffset, stateIndices, columns);
            AddState(biasing, body.PositionYVariable, stateOffset + 1, stateIndices, columns);
            AddState(biasing, body.AngleVariable, stateOffset + 2, stateIndices, columns);
            AddState(biasing, body.VelocityXVariable, stateOffset + 3, stateIndices, columns);
            AddState(biasing, body.VelocityYVariable, stateOffset + 4, stateIndices, columns);
            AddState(biasing, body.AngularVelocityVariable, stateOffset + 5, stateIndices, columns);
        }

        private static void AddState(
            IBiasingSimulationState biasing,
            IVariable<double> variable,
            int stateIndex,
            ICollection<int> stateIndices,
            ICollection<int> columns)
        {
            stateIndices.Add(stateIndex);
            columns.Add(biasing.Map[variable]);
        }

        private void CaptureState(IRigidBody2DBehavior body, int offset)
        {
            if (body == null)
                return;

            _state[offset] = body.PositionX;
            _state[offset + 1] = body.PositionY;
            _state[offset + 2] = body.Angle;
            _state[offset + 3] = body.VelocityX;
            _state[offset + 4] = body.VelocityY;
            _state[offset + 5] = body.AngularVelocity;
        }

        private ConnectionBodyState2D ToBodyState(int offset) =>
            new ConnectionBodyState2D(
                new Vector2D(_state[offset], _state[offset + 1]),
                _state[offset + 2],
                new Vector2D(_state[offset + 3], _state[offset + 4]),
                _state[offset + 5]);
    }
}
