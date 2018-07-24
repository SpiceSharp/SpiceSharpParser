using System;
using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    /// <summary>
    /// TODO: Add comments.
    /// </summary>
    public class SimulationContexts : ISimulationContexts
    {
        public SimulationContexts(IResultService result, IEvaluator readingEvaluator)
        {
            Result = result ?? throw new ArgumentNullException(nameof(result));
            ReadingEvaluator = readingEvaluator ?? throw new ArgumentNullException(nameof(readingEvaluator));
            Contexts = new Dictionary<Simulation, SimulationContext>();
            PrepareActions = new List<Action>();
        }

        protected IEvaluator ReadingEvaluator { get; }

        protected IEnumerable<Simulation> Simulations
        {
            get
            {
                return Result.Simulations;
            }
        }

        protected IResultService Result { get; }

        protected IDictionary<Simulation, SimulationContext> Contexts { get; set; }

        protected List<Action> PrepareActions { get; set; }

        protected bool Prepared { get; set; }

        /// <summary>
        /// Gets the simulation evaluators.
        /// </summary>
        public IDictionary<Simulation, IEvaluator> GetSimulationEvaluators()
        {
            var result = new Dictionary<Simulation, IEvaluator>();

            foreach (var simulation in Simulations)
            {
                result[simulation] = GetSimulationEvaluator(simulation);
            }

            return result;
        }

        /// <summary>
        /// Gets the simulation evaluator.
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <returns>
        /// A reference to simulation evaluator.
        /// </returns>
        public IEvaluator GetSimulationEvaluator(Simulation simulation)
        {
            if (Contexts.ContainsKey(simulation))
            {
                return Contexts[simulation].Evaluator;
            }

            throw new Exception("Missing context for simulation");
        }

        /// <summary>
        /// Prepares the simulation contexts.
        /// </summary>
        public void Prepare(int? randomSeed)
        {
            CreateContexts(randomSeed);
            PrepareActions.ForEach(a => a());
            PrepareActions.Clear();
            Prepared = true;
        }

        public void Add(Action action)
        {
            lock (this)
            {
                PrepareActions.Add(action);
            }
        }

        /// <summary>
        /// Sets IC node voltage for every simulation.
        /// </summary>
        /// <param name="nodeName">Name of the node</param>
        /// <param name="intialVoltageExpression">IC voltage expression.</param>
        public void SetICVoltage(string nodeName, string intialVoltageExpression)
        {
            Add(() =>
            {
                foreach (var simulation in Simulations)
                {
                    simulation.Nodes.InitialConditions[nodeName] = GetSimulationEvaluator(simulation).EvaluateDouble(intialVoltageExpression, simulation);
                }
            });

            if (Prepared)
            {
                RunPrepareActions();
            }
        }

        /// <summary>
        /// Sets the parameter for simulation.
        /// </summary>
        /// <param name="paramName">Parameter name.</param>
        /// <param name="value">Value of parameter.</param>
        /// <param name="simulation">Simulation.</param>
        public void SetParameter(string paramName, double value, BaseSimulation simulation)
        {
            Add(() => { GetSimulationEvaluator(simulation).SetParameter(paramName, value, simulation); });

            if (Prepared)
            {
                RunPrepareActions();
            }
        }

        /// <summary>
        /// Sets the entity parameter.
        /// </summary>
        /// <param name="paramName">Parameter name.</param>
        /// <param name="object">Entity object.</param>
        /// <param name="expression">Expression.</param>
        /// <param name="simulation">Simulation.</param>
        public void SetEntityParameter(string paramName, Entity @object, string expression, BaseSimulation simulation = null)
        {
            Add(() =>
            {
                if (simulation != null)
                {
                    simulation.OnBeforeTemperatureCalculations += (object sender, LoadStateEventArgs args) =>
                    {
                        var evaluator = GetEvaluator(simulation, @object.Name.ToString());
                        var parameter = simulation.EntityParameters.GetEntityParameters(@object.Name).GetParameter(paramName);
                        parameter.Value = evaluator.EvaluateDouble(expression, simulation);

                        evaluator.AddAction(@object.Name + "-" + paramName, expression, (newValue) => {
                            parameter.Value = newValue;
                        });
                    };
                }
                else
                {
                    foreach (BaseSimulation s in Simulations)
                    {
                        s.OnBeforeTemperatureCalculations += (object sender, LoadStateEventArgs args) =>
                        {
                            var evaluator = GetEvaluator(s, @object.Name.ToString());
                            var parameter = s.EntityParameters.GetEntityParameters(@object.Name).GetParameter(paramName);
                            parameter.Value = evaluator.EvaluateDouble(expression, s);

                            evaluator.AddAction(@object.Name + "-" + paramName, expression, (newValue) => {
                                parameter.Value = newValue;
                            });
                        };
                    }
                }
            });

            if (Prepared)
            {
                RunPrepareActions();
            }
        }

        /// <summary>
        /// Sets the model parameter.
        /// </summary>
        /// <param name="paramName">Parameter name.</param>
        /// <param name="model">Entity object.</param>
        /// <param name="expression">Expression.</param>
        /// <param name="simulation">Simulation.</param>
        public void SetModelParameter(string paramName, Entity model, string expression, BaseSimulation simulation)
        {
            SetEntityParameter(paramName, model, expression, simulation);
        }

        protected void RunPrepareActions()
        {
            lock (this)
            {
                foreach (var action in PrepareActions.ToArray())
                {
                    action();
                }

                PrepareActions.Clear();
            }
        }

        protected IEvaluator GetEvaluator(BaseSimulation simulation, string entityName)
        {
            var dotIndex = entityName.LastIndexOf('.');
            var simulationEvaluator = GetSimulationEvaluator(simulation);

            if (dotIndex == -1)
            {
                return simulationEvaluator;
            }

            string subcircuitName = entityName.Substring(0, dotIndex);
            return simulationEvaluator.FindChildEvaluator(subcircuitName);
        }

        protected void CreateContexts(int? randomSeed)
        {
            foreach (var simulation in Simulations)
            {
                Contexts[simulation] = new SimulationContext()
                {
                    Evaluator = ReadingEvaluator.CreateClonedEvaluator(simulation.Name.ToString(), randomSeed != null ? randomSeed++ : null),
                    Simulation = simulation,
                };
            }

            Prepared = true;
        }
    }
}
