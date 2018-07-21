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
            ReadingEvaluator = readingEvaluator;
            Result = result;

            Contexts = new Dictionary<Simulation, SimulationContext>();
            PrepareActions = new List<Action>();
        }

        protected IEvaluator ReadingEvaluator { get; }

        protected IEnumerable<Simulation> Simulations { get { return Result.Simulations; } }

        protected IResultService Result { get; }

        protected IDictionary<Simulation, SimulationContext> Contexts { get; set; }

        protected List<Action> PrepareActions { get; set; }

        protected bool Prepared { get; set; }

        public IEvaluator GetSimulationEvaluator(Simulation simulation)
        {
            if (Contexts.ContainsKey(simulation))
            {
                return Contexts[simulation].Evaluator;
            }

            throw new Exception("Missing context for simulation");
        }

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
                Run();
            }
        }

        public void Prepare()
        {
            CreateContexts();
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

        public void Run()
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

        public void SetParameter(string paramName, double value, BaseSimulation simulation)
        {
            Add(() => { GetSimulationEvaluator(simulation).SetParameter(paramName, value, simulation); });

            if (Prepared)
            {
                Run();
            }
        }

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
                Run();
            }
        }

        public void SetModelParameter(string paramName, Entity model, string expression, BaseSimulation simulation)
        {
            Add(() =>
            {
                simulation.OnBeforeTemperatureCalculations += (object sender, LoadStateEventArgs args) =>
                {
                    var evaluator = GetEvaluator(simulation, model.Name.ToString());
                    var parameter = simulation.EntityParameters.GetEntityParameters(model.Name).GetParameter(paramName);
                    parameter.Value = evaluator.EvaluateDouble(expression, simulation);

                    evaluator.AddAction(model.Name + "-" + paramName, expression, (newValue) => {
                        parameter.Value = newValue;
                    });
                };
            });

            if (Prepared)
            {
                Run();
            }
        }

        private IEvaluator GetEvaluator(BaseSimulation simulation, string entityName)
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

        private void CreateContexts()
        {
            foreach (var simulation in Simulations)
            {
                Contexts[simulation] = new SimulationContext()
                {
                    Evaluator = ReadingEvaluator.CreateClonedEvaluator(simulation.Name.ToString()),
                    Simulation = simulation,
                };
            }

            Prepared = true;
        }
    }
}
