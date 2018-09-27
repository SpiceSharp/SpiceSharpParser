using System;
using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public class SimulationEventArgs: EventArgs
    {
        public BaseSimulation Simulation { get; set; }
    }

    public class SimulationsParameters : ISimulationsParameters
    {
        protected Dictionary<Simulation, Dictionary<Entity, Dictionary<string, List<EventHandler<LoadStateEventArgs>>>>> BeforeLoads = new Dictionary<Simulation, Dictionary<Entity, Dictionary<string, List<EventHandler<LoadStateEventArgs>>>>>();
        protected Dictionary<Simulation, Dictionary<Entity, Dictionary<string, List<EventHandler<LoadStateEventArgs>>>>> BeforeTemperature = new Dictionary<Simulation, Dictionary<Entity, Dictionary<string, List<EventHandler<LoadStateEventArgs>>>>>();
        protected Dictionary<Simulation, Dictionary<Entity, Dictionary<string, int>>> BeforeLoadsOrder = new Dictionary<Simulation, Dictionary<Entity, Dictionary<string, int>>>();
        protected Dictionary<Simulation, Dictionary<Entity, Dictionary<string, int>>> BeforeTemperatureOrder = new Dictionary<Simulation, Dictionary<Entity, Dictionary<string, int>>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SimulationsParameters"/> class.
        /// </summary>
        /// <param name="evaluators">Simulation evaluators.</param>
        public SimulationsParameters(IEvaluatorsContainer evaluators)
        {
            Evaluators = evaluators;
        }

        protected EventHandler<SimulationEventArgs> SimulationCreated { get; set; }

        protected IEvaluatorsContainer Evaluators { get; }

        public void Prepare(BaseSimulation simulation)
        {
            SimulationCreated?.Invoke(this, new SimulationEventArgs() { Simulation = simulation });
        }

        public void SetICVoltage(string nodeName, string voltageExpression)
        {
            SimulationCreated += (object sender, SimulationEventArgs args) =>
            {
                var evaluator = Evaluators.GetSimulationEvaluator(args.Simulation);
                var value = evaluator.EvaluateDouble(voltageExpression);
                args.Simulation.Nodes.InitialConditions[nodeName] = value;
            };
        }

        /// <summary>
        /// Sets voltage guess condition for node.
        /// </summary>
        /// <param name="nodeName">Name of node.</param>
        /// <param name="expression">Expression.</param>
        public void SetNodeSetVoltage(string nodeName, string expression)
        {
            SimulationCreated += (object sender, SimulationEventArgs args) =>
            {
               args.Simulation.Nodes.NodeSets[nodeName] = Evaluators.GetSimulationEvaluator(args.Simulation).EvaluateDouble(expression);
            };
        }

        public void SetParameter(Entity @object, string paramName, string expression, int order, bool onload = true)
        {
            SimulationCreated += (object sender, SimulationEventArgs args) =>
            {
                if (onload)
                {
                    UpdateParameterBeforeLoad(paramName, @object, expression, args.Simulation, order);
                }

                UpdateParameterBeforeTemperature(paramName, @object, expression, args.Simulation, order);
            };
        }

        public void SetParameter(Entity @object, string paramName, string expression, BaseSimulation simulation, int order, bool onload = true)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            if (onload)
            {
                UpdateParameterBeforeLoad(paramName, @object, expression, simulation, order);
            }

            UpdateParameterBeforeTemperature(paramName, @object, expression, simulation, order);
        }

        public void SetParameter(Entity @object, string paramName, double value, int order)
        {
            SimulationCreated += (object sender, SimulationEventArgs args) =>
            {
                UpdateParameterBeforeLoad(paramName, @object, value, args.Simulation, order);
                UpdateParameterBeforeTemperature(paramName, @object, value, args.Simulation, order);
            };
        }

        public void SetParameter(Entity @object, string paramName, double value, BaseSimulation simulation, int order)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            UpdateParameterBeforeLoad(paramName, @object, value, simulation, order);
            UpdateParameterBeforeTemperature(paramName, @object, value, simulation, order);
        }

        private IEvaluator GetEvaluator(BaseSimulation simulation, string entityName)
        {
            var dotIndex = entityName.LastIndexOf('.');
            var simulationEvaluator = Evaluators.GetSimulationEvaluator(simulation);

            if (dotIndex == -1)
            {
                return simulationEvaluator;
            }

            string subcircuitName = entityName.Substring(0, dotIndex);
            return simulationEvaluator.FindChildEvaluator(subcircuitName);
        }

        private void UpdateParameterBeforeTemperature(string paramName, Entity @object, string expression, BaseSimulation simulation, int order)
        {
            EnsureBeforeTemperature(paramName, @object, simulation);

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
                var parameter = simulation.EntityParameters[@object.Name].GetParameter<double>(paramName.ToLower());

                if (parameter == null)
                {
                    throw new Exception("Parameter " + paramName + " not found in: " + @object.Name);
                }

                parameter.Value = evaluator.EvaluateDouble(expression);

                evaluator.AddAction(
                    @object.Name + "-" + paramName,
                    expression,
                    (newValue) => { parameter.Value = newValue; });
            };

            if (BeforeTemperatureOrder[simulation][@object][paramName] > order)
            {
                return;
            }

            if (BeforeTemperature[simulation][@object][paramName].Count != 0)
            {
                simulation.BeforeTemperature -= BeforeTemperature[simulation][@object][paramName][0];
                BeforeTemperature[simulation][@object][paramName].Clear();
            }

            BeforeTemperatureOrder[simulation][@object][paramName] = order;
            BeforeTemperature[simulation][@object][paramName].Add(handler);
            simulation.BeforeTemperature += handler;
        }

        private void UpdateParameterBeforeTemperature(string paramName, Entity @object, double value, BaseSimulation simulation, int order)
        {
            EnsureBeforeTemperature(paramName, @object, simulation);

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

                var parameter = simulation.EntityParameters[@object.Name].GetParameter<double>(paramName.ToLower());

                if (parameter == null)
                {
                    throw new Exception("Parameter " + paramName + " not found in: " + @object.Name);
                }

                parameter.Value = value;
            };

            if (BeforeTemperatureOrder[simulation][@object][paramName] > order)
            {
                return;
            }

            if (BeforeTemperature[simulation][@object][paramName].Count != 0)
            {
                simulation.BeforeTemperature -= BeforeTemperature[simulation][@object][paramName][0];
                BeforeTemperature[simulation][@object][paramName].Clear();
            }

            BeforeTemperatureOrder[simulation][@object][paramName] = order;
            BeforeTemperature[simulation][@object][paramName].Add(handler);
            simulation.BeforeTemperature += handler;
        }

        private void UpdateParameterBeforeLoad(string paramName, Entity @object, string expression, BaseSimulation simulation, int order)
        {
            EnsureBeforeLoad(paramName, @object, simulation);

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
                var parameter = simulation.EntityParameters[@object.Name].GetParameter<double>(paramName.ToLower());

                if (parameter == null)
                {
                    throw new Exception("Parameter " + paramName + " not found in: " + @object.Name);
                }
                var value = evaluator.EvaluateDouble(expression);
                parameter.Value = value;
            };

            if (BeforeLoadsOrder[simulation][@object][paramName] > order)
            {
                return;
            }

            if (BeforeLoads[simulation][@object][paramName].Count != 0)
            {
                simulation.BeforeLoad -= BeforeLoads[simulation][@object][paramName][0];
                BeforeLoads[simulation][@object][paramName].Clear();
            }

            BeforeLoads[simulation][@object][paramName].Add(handler);
            BeforeLoadsOrder[simulation][@object][paramName] = order;
            simulation.BeforeLoad += handler;
        }

        private void UpdateParameterBeforeLoad(string paramName, Entity @object, double value, BaseSimulation simulation, int order)
        {
            EnsureBeforeLoad(paramName, @object, simulation);

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

                var parameter = simulation.EntityParameters[@object.Name].GetParameter<double>(paramName.ToLower());
                if (parameter == null)
                {
                    throw new Exception("Parameter " + paramName + " not found in: " + @object.Name);
                }
                parameter.Value = value;
            };

            if (BeforeLoadsOrder[simulation][@object][paramName] > order)
            {
                return;
            }

            if (BeforeLoads[simulation][@object][paramName].Count != 0)
            {
                simulation.BeforeLoad -= BeforeLoads[simulation][@object][paramName][0];
                BeforeLoads[simulation][@object][paramName].Clear();
            }

            BeforeLoads[simulation][@object][paramName].Add(handler);
            BeforeLoadsOrder[simulation][@object][paramName] = order;
            simulation.BeforeLoad += handler;
        }

        private void EnsureBeforeLoad(string paramName, Entity @object, BaseSimulation simulation)
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

            if (!BeforeLoadsOrder.ContainsKey(simulation))
            {
                BeforeLoadsOrder[simulation] = new Dictionary<Entity, Dictionary<string, int>>();
            }

            if (!BeforeLoadsOrder[simulation].ContainsKey(@object))
            {
                BeforeLoadsOrder[simulation][@object] = new Dictionary<string, int>();
            }

            if (!BeforeLoadsOrder[simulation][@object].ContainsKey(paramName))
            {
                BeforeLoadsOrder[simulation][@object][paramName] = 0;
            }
        }

        private void EnsureBeforeTemperature(string paramName, Entity @object, BaseSimulation simulation)
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

            if (!BeforeTemperatureOrder.ContainsKey(simulation))
            {
                BeforeTemperatureOrder[simulation] = new Dictionary<Entity, Dictionary<string, int>>();
            }

            if (!BeforeTemperatureOrder[simulation].ContainsKey(@object))
            {
                BeforeTemperatureOrder[simulation][@object] = new Dictionary<string, int>();
            }

            if (!BeforeTemperatureOrder[simulation][@object].ContainsKey(paramName))
            {
                BeforeTemperatureOrder[simulation][@object][paramName] = 0;
            }
        }
    }
}
