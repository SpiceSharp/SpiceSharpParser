using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.Builder;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Measurements;

namespace SpiceSharpParser.Analysis
{
    /// <summary>
    /// A single point in the design space with component values and resulting spec measurements.
    /// </summary>
    public class DesignPoint
    {
        public Dictionary<string, double> ComponentValues { get; set; } = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, double> MeasurementValues { get; set; } = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Weighted objective score (lower is better, 0 = all specs exactly on target).
        /// </summary>
        public double ObjectiveScore { get; set; } = double.MaxValue;

        /// <summary>
        /// Whether all constraints are satisfied.
        /// </summary>
        public bool ConstraintsSatisfied { get; set; }
    }

    /// <summary>
    /// Result of a design space exploration.
    /// </summary>
    public class ExplorationResult
    {
        /// <summary>
        /// All evaluated design points.
        /// </summary>
        public List<DesignPoint> AllPoints { get; } = new List<DesignPoint>();

        /// <summary>
        /// The best feasible design point (lowest objective, all constraints met).
        /// Null if no feasible point found.
        /// </summary>
        public DesignPoint Best { get; set; }

        /// <summary>
        /// Component values for the best point.
        /// </summary>
        public Dictionary<string, double> BestValues => Best?.ComponentValues;

        /// <summary>
        /// Number of feasible (constraint-satisfying) points.
        /// </summary>
        public int FeasibleCount => AllPoints.Count(p => p.ConstraintsSatisfied);
    }

    /// <summary>
    /// Explores a multi-parameter design space by sweeping component values
    /// and evaluating .MEAS results against objectives and constraints.
    /// </summary>
    public class DesignSpaceExplorer
    {
        private readonly string _baseNetlist;
        private readonly List<ParameterDef> _parameters = new List<ParameterDef>();
        private readonly List<ObjectiveDef> _objectives = new List<ObjectiveDef>();
        private readonly List<ConstraintDef> _constraints = new List<ConstraintDef>();

        /// <summary>
        /// Initializes a DesignSpaceExplorer from a netlist string.
        /// </summary>
        /// <param name="netlist">SPICE netlist text.</param>
        public DesignSpaceExplorer(string netlist)
        {
            _baseNetlist = netlist ?? throw new ArgumentNullException(nameof(netlist));
        }

        /// <summary>
        /// Adds a component parameter to sweep.
        /// </summary>
        /// <param name="componentName">Name of the passive component (R, L, or C).</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <param name="steps">Number of steps (default 10).</param>
        public DesignSpaceExplorer AddParameter(string componentName, double min, double max, int steps = 10)
        {
            _parameters.Add(new ParameterDef
            {
                ComponentName = componentName,
                Min = min,
                Max = max,
                Steps = steps,
            });
            return this;
        }

        /// <summary>
        /// Adds an objective to minimize: weighted squared distance from target.
        /// </summary>
        /// <param name="measurementName">.MEAS result name.</param>
        /// <param name="target">Target value.</param>
        /// <param name="weight">Relative weight (default 1.0).</param>
        public DesignSpaceExplorer AddObjective(string measurementName, double target, double weight = 1.0)
        {
            _objectives.Add(new ObjectiveDef
            {
                MeasurementName = measurementName,
                Target = target,
                Weight = weight,
            });
            return this;
        }

        /// <summary>
        /// Adds a constraint: measurement must be within [min, max].
        /// </summary>
        public DesignSpaceExplorer AddConstraint(string measurementName, double min, double max)
        {
            _constraints.Add(new ConstraintDef
            {
                MeasurementName = measurementName,
                Min = min,
                Max = max,
            });
            return this;
        }

        /// <summary>
        /// Performs a grid search over all parameter combinations.
        /// </summary>
        public ExplorationResult Explore()
        {
            var result = new ExplorationResult();

            if (_parameters.Count == 0)
            {
                return result;
            }

            // Generate parameter value grids
            var grids = new List<double[]>();
            foreach (var param in _parameters)
            {
                var values = new double[param.Steps + 1];
                for (int i = 0; i <= param.Steps; i++)
                {
                    // Logarithmic spacing if range spans more than 1 decade
                    if (param.Max / param.Min > 10 && param.Min > 0)
                    {
                        double logMin = Math.Log10(param.Min);
                        double logMax = Math.Log10(param.Max);
                        values[i] = Math.Pow(10, logMin + (logMax - logMin) * i / param.Steps);
                    }
                    else
                    {
                        values[i] = param.Min + (param.Max - param.Min) * i / param.Steps;
                    }
                }

                grids.Add(values);
            }

            // Enumerate all combinations
            var indices = new int[_parameters.Count];
            bool done = false;

            while (!done)
            {
                // Build component value map for this combination
                var componentValues = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
                for (int p = 0; p < _parameters.Count; p++)
                {
                    componentValues[_parameters[p].ComponentName] = grids[p][indices[p]];
                }

                // Evaluate this design point
                var point = EvaluatePoint(componentValues);
                result.AllPoints.Add(point);

                // Update best
                if (point.ConstraintsSatisfied)
                {
                    if (result.Best == null || point.ObjectiveScore < result.Best.ObjectiveScore)
                    {
                        result.Best = point;
                    }
                }

                // Increment indices (odometer pattern)
                int carry = _parameters.Count - 1;
                while (carry >= 0)
                {
                    indices[carry]++;
                    if (indices[carry] <= _parameters[carry].Steps)
                    {
                        break;
                    }

                    indices[carry] = 0;
                    carry--;
                }

                if (carry < 0)
                {
                    done = true;
                }
            }

            return result;
        }

        private DesignPoint EvaluatePoint(Dictionary<string, double> componentValues)
        {
            var point = new DesignPoint
            {
                ComponentValues = new Dictionary<string, double>(componentValues, StringComparer.OrdinalIgnoreCase),
            };

            try
            {
                // Parse the model and modify component values
                var model = ParseModel(_baseNetlist);
                var inspector = new CircuitInspector(model);

                foreach (var kvp in componentValues)
                {
                    inspector.SetComponentValue(kvp.Key, kvp.Value);
                }

                // Run simulations and collect measurements
                RunSimulations(model);

                // Extract measurement values
                var allMeasNames = _objectives.Select(o => o.MeasurementName)
                    .Concat(_constraints.Select(c => c.MeasurementName))
                    .Distinct(StringComparer.OrdinalIgnoreCase);

                foreach (string measName in allMeasNames)
                {
                    if (model.Measurements.TryGetValue(measName, out var results))
                    {
                        var successResult = results.FirstOrDefault(r => r.Success);
                        if (successResult != null)
                        {
                            point.MeasurementValues[measName] = successResult.Value;
                        }
                    }
                }

                // Evaluate constraints
                point.ConstraintsSatisfied = true;
                foreach (var constraint in _constraints)
                {
                    if (point.MeasurementValues.TryGetValue(constraint.MeasurementName, out double val))
                    {
                        if (val < constraint.Min || val > constraint.Max)
                        {
                            point.ConstraintsSatisfied = false;
                            break;
                        }
                    }
                    else
                    {
                        point.ConstraintsSatisfied = false;
                        break;
                    }
                }

                // Compute objective score (weighted sum of squared relative errors)
                double score = 0;
                foreach (var obj in _objectives)
                {
                    if (point.MeasurementValues.TryGetValue(obj.MeasurementName, out double val))
                    {
                        double error;
                        if (Math.Abs(obj.Target) > 1e-15)
                        {
                            error = (val - obj.Target) / obj.Target;
                        }
                        else
                        {
                            error = val - obj.Target;
                        }

                        score += obj.Weight * error * error;
                    }
                    else
                    {
                        score += obj.Weight * 1e6; // Penalty for missing measurement
                    }
                }

                point.ObjectiveScore = score;
            }
            catch
            {
                point.ConstraintsSatisfied = false;
                point.ObjectiveScore = double.MaxValue;
            }

            return point;
        }

        private static void RunSimulations(SpiceSharpModel model)
        {
            foreach (var simulation in model.Simulations)
            {
                try
                {
                    var exports = model.Exports.Where(ex => ex.Simulation == simulation).ToList();
                    simulation.EventExportData += (sender, e) =>
                    {
                        foreach (var export in exports)
                        {
                            try
                            {
                                export.Extract();
                            }
                            catch
                            {
                                // Ignore export errors
                            }
                        }
                    };

                    var codes = simulation.Run(model.Circuit, -1);
                    codes = simulation.InvokeEvents(codes);
                    codes.ToArray();
                }
                catch
                {
                    // Skip failed simulations
                }
            }
        }

        private static SpiceSharpModel ParseModel(string netlist)
        {
            var trimmed = string.Join(
                Environment.NewLine,
                netlist.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0));

            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;

            var parseResult = parser.ParseNetlist(trimmed);

            var reader = new SpiceSharpReader();
            return reader.Read(parseResult.FinalModel);
        }

        private class ParameterDef
        {
            public string ComponentName { get; set; }

            public double Min { get; set; }

            public double Max { get; set; }

            public int Steps { get; set; }
        }

        private class ObjectiveDef
        {
            public string MeasurementName { get; set; }

            public double Target { get; set; }

            public double Weight { get; set; }
        }

        private class ConstraintDef
        {
            public string MeasurementName { get; set; }

            public double Min { get; set; }

            public double Max { get; set; }
        }
    }
}
