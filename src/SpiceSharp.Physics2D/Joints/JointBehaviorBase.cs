using SpiceSharp;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Physics2D.Bodies;
using SpiceSharp.Physics2D.Connections;
using SpiceSharp.Physics2D.Mathematics;
using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceSharp.Physics2D.Joints;

public abstract class JointBehaviorBase : Behavior, IBiasingBehavior, IAcceptBehavior
{
    private readonly int[] _activeLoadIndices;
    private readonly int[] _activeStateIndices;
    private readonly IRigidBody2DBehavior _bodyA;
    private readonly IRigidBody2DBehavior _bodyB;
    private readonly ElementSet<double> _elements;
    private readonly double[] _state = new double[JointEquationSupport.StateCount];
    private readonly ITimeSimulationState _time;
    private readonly double[] _values;

    protected JointBehaviorBase(
        IBindingContext context,
        IRigidBody2DBehavior bodyA,
        IRigidBody2DBehavior bodyB)
        : base(context)
    {
        _bodyA = bodyA;
        _bodyB = bodyB;
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
        _elements = new ElementSet<double>(biasing.Solver, matrixLocations, rows.ToArray());
    }

    protected double[,] Jacobian { get; } =
        new double[JointEquationSupport.LoadCount, JointEquationSupport.StateCount];

    protected double[] Loads { get; } = new double[JointEquationSupport.LoadCount];

    void IBiasingBehavior.Load()
    {
        if (_time.UseDc)
        {
            return;
        }

        CaptureState(_bodyA, 0);
        CaptureState(_bodyB, 6);
        Evaluate(ToBodyState(0), ToBodyState(6));

        int valueIndex = 0;
        for (int rowIndex = 0; rowIndex < _activeLoadIndices.Length; rowIndex++)
        {
            int loadIndex = _activeLoadIndices[rowIndex];
            for (int columnIndex = 0; columnIndex < _activeStateIndices.Length; columnIndex++)
            {
                int stateIndex = _activeStateIndices[columnIndex];
                _values[valueIndex++] = -Jacobian[loadIndex, stateIndex];
            }
        }

        for (int rowIndex = 0; rowIndex < _activeLoadIndices.Length; rowIndex++)
        {
            int loadIndex = _activeLoadIndices[rowIndex];
            double linearizedLoad = Loads[loadIndex];
            for (int columnIndex = 0; columnIndex < _activeStateIndices.Length; columnIndex++)
            {
                int stateIndex = _activeStateIndices[columnIndex];
                linearizedLoad -= Jacobian[loadIndex, stateIndex] * _state[stateIndex];
            }

            _values[valueIndex++] = linearizedLoad;
        }

        _elements.Add(_values);
    }

    void IAcceptBehavior.Probe()
    {
    }

    void IAcceptBehavior.Accept() => AcceptDiagnostics();

    private protected abstract void Evaluate(ConnectionBodyState2D stateA, ConnectionBodyState2D stateB);

    private protected abstract void AcceptDiagnostics();

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
        {
            return;
        }

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
        {
            return;
        }

        _state[offset] = body.PositionX;
        _state[offset + 1] = body.PositionY;
        _state[offset + 2] = body.Angle;
        _state[offset + 3] = body.VelocityX;
        _state[offset + 4] = body.VelocityY;
        _state[offset + 5] = body.AngularVelocity;
    }

    private ConnectionBodyState2D ToBodyState(int offset) =>
        new(
            new Vector2D(_state[offset], _state[offset + 1]),
            _state[offset + 2],
            new Vector2D(_state[offset + 3], _state[offset + 4]),
            _state[offset + 5]);
}
