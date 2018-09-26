using System;
using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    /// <summary>
    /// TODO: Add comments. Please refactor me. Please refactor me. Please refactor me.
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

        public bool Prepared { get; protected set; }

        protected Dictionary<Simulation, Dictionary<Entity, Dictionary<string, List<EventHandler<LoadStateEventArgs>>>>> BeforeLoads
            = new Dictionary<Simulation, Dictionary<Entity, Dictionary<string, List<EventHandler<LoadStateEventArgs>>>>>();


        protected Dictionary<Simulation, Dictionary<Entity, Dictionary<string, List<EventHandler<LoadStateEventArgs>>>>> BeforeTemperature
            = new Dictionary<Simulation, Dictionary<Entity, Dictionary<string, List<EventHandler<LoadStateEventArgs>>>>>();

        /// <summary>
        /// Gets the simulation evaluator.
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <returns>
        /// A reference to simulation evaluator.
        /// </returns>
        public IEvaluator GetSimulationEvaluator(Simulation simulation)
        {
            if (!Contexts.ContainsKey(simulation))
            {
                Contexts[simulation] = new SimulationContext()
                {
                    Evaluator = ReadingEvaluator.CreateClonedEvaluator(simulation.Name.ToString(), simulation, ReadingEvaluator.Seed),
                };
            }

            return Contexts[simulation].Evaluator;
        }

        /// <summary>
        /// Prepares the simulation contexts.
        /// </summary>
        public void Prepare()
        {
            if (!Prepared)
            {
                PrepareActions.ForEach(a => a());
                PrepareActions.Clear();
                Prepared = true;
            }
        }

        public void Add(Action action)
        {
            lock (this)
            {
                PrepareActions.Add(action);
            }
        }

        public IDictionary<Simulation, IEvaluator> GetSimulationEvaluators()
        {
            if (!Prepared)
            {
                Prepare();
            }

            var result = new Dictionary<Simulation, IEvaluator>();

            foreach (var simulation in Simulations)
            {
                result[simulation] = GetSimulationEvaluator(simulation);
            }

            return result;
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
                    simulation.Nodes.InitialConditions[nodeName] = GetSimulationEvaluator(simulation).EvaluateDouble(intialVoltageExpression);
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
            GetSimulationEvaluator(simulation).SetParameter(paramName, value);
        }

        /// <summary>
        /// Sets the entity parameter.
        /// </summary>
        /// <param name="paramName">Parameter name.</param>
        /// <param name="object">Entity object.</param>
        /// <param name="expression">Expression.</param>
        /// <param name="simulation">Simulation.</param>
        public void SetEntityParameter(string paramName, Entity @object, string expression, BaseSimulation simulation = null, bool @override = false)
        {
            if (simulation != null)
            {
                AttachBeforeTemperature(paramName, @object, expression, simulation, @override);
                AttachBeforeLoad(paramName, @object, expression, simulation, @override);
            }
            else
            {
                Add(() =>
                {
                    foreach (BaseSimulation s in Simulations)
                    {
                        AttachBeforeLoad(paramName, @object, expression, s, @override);
                        AttachBeforeTemperature(paramName, @object, expression, s, @override);
                    }
                });
            }

            if (Prepared)
            {
                RunPrepareActions();
            }
        }

        private void AttachBeforeTemperature(string paramName, Entity @object, string expression, BaseSimulation simulation, bool @override)
        {
            if (!BeforeTemperature.ContainsKey(simulation))
            {
                BeforeTemperature[simulation] = new Dictionary<Entity, Dictionary<string, List<EventHandler<LoadStateEventArgs>>>>();
            }

            if (!BeforeTemperature[simulation].ContainsKey(@object))
            {
                BeforeTemperature[simulation][@object] = new Dictionary<string, List<EventHandler<LoadStateEventArgs>>>();
            }

            if (!BeforeTemperature[simulation][@object].ContainsKey(paramName))
            {
                BeforeTemperature[simulation][@object][paramName] = new List<EventHandler<LoadStateEventArgs>>();
            }

            EventHandler<LoadStateEventArgs> handler = (object sender, LoadStateEventArgs args) =>
            {
                var evaluator = GetEvaluator(simulation, @object.Name.ToString());
                var parameter = simulation.EntityParameters[@object.Name].GetParameter<double>(paramName);
                parameter.Value = evaluator.EvaluateDouble(expression);

                evaluator.AddAction(
                    @object.Name + "-" + paramName,
                    expression,
                    (newValue) => { parameter.Value = newValue; });
            };

            if (BeforeTemperature[simulation][@object][paramName].Count == 0)
            {
                BeforeTemperature[simulation][@object][paramName].Add(handler);
                simulation.BeforeTemperature += handler;
            }
            else
            {
                if (@override)
                {
                    simulation.BeforeTemperature -= BeforeTemperature[simulation][@object][paramName][0];
                    simulation.BeforeTemperature += handler;
                }
            }
        }

        private void AttachBeforeLoad(string paramName, Entity @object, string expression, BaseSimulation simulation, bool @override)
        {
            if (!BeforeLoads.ContainsKey(simulation))
            {
                BeforeLoads[simulation] = new Dictionary<Entity, Dictionary<string, List<EventHandler<LoadStateEventArgs>>>>();
            }

            if (!BeforeLoads[simulation].ContainsKey(@object))
            {
                BeforeLoads[simulation][@object] = new Dictionary<string, List<EventHandler<LoadStateEventArgs>>>();
            }

            if (!BeforeLoads[simulation][@object].ContainsKey(paramName))
            {
                BeforeLoads[simulation][@object][paramName] = new List<EventHandler<LoadStateEventArgs>>();
            }

            EventHandler<LoadStateEventArgs> handler = (object sender, LoadStateEventArgs args) =>
            {
                //TODO: remove this hack
                if (simulation is DC dc)
                {
                    if (dc.Sweeps[0].Parameter.ToString() == @object.Name.ToString())
                    {
                        return;
                    }
                }

                var evaluator = GetEvaluator(simulation, @object.Name.ToString());
                var parameter = simulation.EntityParameters[@object.Name].GetParameter<double>(paramName);
                var value = evaluator.EvaluateDouble(expression);
                parameter.Value = value;
            };

            if (BeforeLoads[simulation][@object][paramName].Count == 0)
            {
                BeforeLoads[simulation][@object][paramName].Add(handler);
                simulation.BeforeLoad += handler;
            }
            else
            {
                if (@override)
                {
                    simulation.BeforeLoad -= BeforeLoads[simulation][@object][paramName][0];
                    simulation.BeforeLoad += handler;
                }
            }
        }

        /// <summary>
        /// Sets the entity parameter.
        /// </summary>
        /// <param name="paramName">Parameter name.</param>
        /// <param name="object">Entity object.</param>
        /// <param name="paramValue">Expression.</param>
        public void SetEntityParameter(string paramName, Entity @object, Func<object, double> paramValue)
        {
            Add(() =>
            {
                foreach (BaseSimulation s in Simulations)
                {
                    s.BeforeLoad += (object sender, LoadStateEventArgs args) =>
                    {
                        double paramValueUnwrapped = paramValue(s);
                        s.EntityParameters[@object.Name].SetParameter(paramName, paramValueUnwrapped);
                    };
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
        public void SetModelParameter(string paramName, Entity model, string expression, BaseSimulation simulation, bool @override = false)
        {
            SetEntityParameter(paramName, model, expression, simulation, @override);
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
    }
}
