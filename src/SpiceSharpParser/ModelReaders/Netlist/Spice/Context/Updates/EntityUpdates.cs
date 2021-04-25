using SpiceSharp;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation.Expressions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Updates
{
    public class EntityUpdates
    {
        public EntityUpdates(bool isParameterNameCaseSensitive, SimulationEvaluationContexts contexts)
        {
            IsParameterNameCaseSensitive = isParameterNameCaseSensitive;
            Contexts = contexts ?? throw new ArgumentNullException(nameof(contexts));
            CommonUpdates = new Dictionary<IEntity, EntityUpdate>();
            SimulationSpecificUpdates = new Dictionary<Simulation, Dictionary<IEntity, EntityUpdate>>();
            SimulationEntityParametersCache = new ConcurrentDictionary<string, double>();
        }

        protected bool IsParameterNameCaseSensitive { get; }

        protected SimulationEvaluationContexts Contexts { get; set; }

        protected Dictionary<IEntity, EntityUpdate> CommonUpdates { get; set; }

        protected Dictionary<Simulation, Dictionary<IEntity, EntityUpdate>> SimulationSpecificUpdates { get; set; }

        protected ConcurrentDictionary<string, double> SimulationEntityParametersCache { get; }

        public void Apply(Simulation simulation)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            if (simulation is BiasingSimulation biasingSimulation)
            {
                biasingSimulation.BeforeLoad += (sender, args) =>
                {
                    foreach (var entity in CommonUpdates.Keys)
                    {
                        var beforeLoads = CommonUpdates[entity].ParameterUpdatesBeforeLoad;

                        foreach (var entityUpdate in beforeLoads)
                        {
                            Common.Evaluation.EvaluationContext context = GetEntityContext(simulation, entity);

                            var value = entityUpdate.GetValue(context);
                            if (!double.IsNaN(value))
                            {
                                entity.CreateParameterSetter<double>(entityUpdate.ParameterName)?.Invoke(value);
                            }
                        }
                    }

                    if (SimulationSpecificUpdates.ContainsKey(simulation))
                    {
                        foreach (var entityPair in SimulationSpecificUpdates[simulation])
                        {
                            var beforeLoads = entityPair.Value.ParameterUpdatesBeforeLoad;

                            foreach (var entityUpdate in beforeLoads)
                            {
                                Common.Evaluation.EvaluationContext context = GetEntityContext(simulation, entityPair.Key);

                                var value = entityUpdate.GetValue(context);
                                if (!double.IsNaN(value))
                                {
                                    entityPair.Key.CreateParameterSetter<double>(entityUpdate.ParameterName)?.Invoke(value);
                                }
                            }
                        }
                    }
                };

                biasingSimulation.BeforeTemperature += (sender, args) =>
                {
                    foreach (var entity in CommonUpdates.Keys)
                    {
                        var beforeTemperature = CommonUpdates[entity].ParameterUpdatesBeforeTemperature;

                        foreach (var entityUpdate in beforeTemperature)
                        {
                            Common.Evaluation.EvaluationContext context = GetEntityContext(simulation, entity);

                            var value = entityUpdate.GetValue(context);
                            if (!double.IsNaN(value))
                            {
                                entity.CreateParameterSetter<double>(entityUpdate.ParameterName)?.Invoke(value);
                            }
                        }
                    }

                    if (SimulationSpecificUpdates.ContainsKey(simulation))
                    {
                        foreach (var entityPair in SimulationSpecificUpdates[simulation])
                        {
                            var beforeTemperature = entityPair.Value.ParameterUpdatesBeforeTemperature;

                            foreach (var entityUpdate in beforeTemperature)
                            {
                                Common.Evaluation.EvaluationContext context = GetEntityContext(simulation, entityPair.Key);

                                var value = entityUpdate.GetValue(context);
                                if (!double.IsNaN(value))
                                {
                                    entityPair.Key.CreateParameterSetter<double>(entityUpdate.ParameterName)?.Invoke(value);
                                }
                            }
                        }
                    }
                };
            }
        }

        public void Add(IEntity entity, Simulation simulation, string parameterName, string expression, bool beforeTemperature, bool beforeLoad)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (parameterName == null)
            {
                throw new ArgumentNullException(nameof(parameterName));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (!SimulationSpecificUpdates.ContainsKey(simulation))
            {
                SimulationSpecificUpdates[simulation] = new Dictionary<IEntity, EntityUpdate>();
            }

            if (!SimulationSpecificUpdates[simulation].ContainsKey(entity))
            {
                SimulationSpecificUpdates[simulation][entity] = new EntityUpdate();
            }

            if (beforeLoad)
            {
                SimulationSpecificUpdates[simulation][entity].ParameterUpdatesBeforeLoad.Add(
                    new EntityParameterExpressionValueUpdate()
                    {
                        ParameterName = parameterName,
                        Expression = new DynamicExpression(expression),
                    });
            }

            if (beforeTemperature)
            {
                SimulationSpecificUpdates[simulation][entity].ParameterUpdatesBeforeTemperature.Add(new EntityParameterExpressionValueUpdate()
                {
                    Expression = new DynamicExpression(expression),
                    ParameterName = parameterName,
                });
            }
        }

        public void Add(IEntity entity, string parameterName, double value, bool beforeTemperature, bool beforeLoad)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (parameterName == null)
            {
                throw new ArgumentNullException(nameof(parameterName));
            }

            if (!CommonUpdates.ContainsKey(entity))
            {
                CommonUpdates[entity] = new EntityUpdate();
            }

            if (beforeLoad)
            {
                CommonUpdates[entity].ParameterUpdatesBeforeLoad.Add(
                    new EntityParameterDoubleValueUpdate()
                    {
                        ParameterName = parameterName,
                        Value = value,
                    });
            }

            if (beforeTemperature)
            {
                CommonUpdates[entity].ParameterUpdatesBeforeTemperature.Add(
                    new EntityParameterDoubleValueUpdate() { ParameterName = parameterName, Value = value });
            }
        }

        public void Add(IEntity entity, string parameterName, string expression, bool beforeTemperature, bool beforeLoad)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (parameterName == null)
            {
                throw new ArgumentNullException(nameof(parameterName));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (!CommonUpdates.ContainsKey(entity))
            {
                CommonUpdates[entity] = new EntityUpdate();
            }

            if (beforeLoad)
            {
                CommonUpdates[entity].ParameterUpdatesBeforeLoad.Add(
                    new EntityParameterExpressionValueUpdate()
                    {
                        ParameterName = parameterName,
                        Expression = new DynamicExpression(expression),
                    });
            }

            if (beforeTemperature)
            {
                CommonUpdates[entity].ParameterUpdatesBeforeTemperature.Add(new EntityParameterExpressionValueUpdate()
                {
                    Expression = new DynamicExpression(expression),
                    ParameterName = parameterName,
                });
            }
        }

        public void Add(IEntity entity, Simulation simulation, string parameterName, double value, bool beforeTemperature, bool beforeLoad)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            if (parameterName == null)
            {
                throw new ArgumentNullException(nameof(parameterName));
            }

            if (!SimulationSpecificUpdates.ContainsKey(simulation))
            {
                SimulationSpecificUpdates[simulation] = new Dictionary<IEntity, EntityUpdate>();
            }

            if (!SimulationSpecificUpdates[simulation].ContainsKey(entity))
            {
                SimulationSpecificUpdates[simulation][entity] = new EntityUpdate();
            }

            if (beforeLoad)
            {
                SimulationSpecificUpdates[simulation][entity].ParameterUpdatesBeforeLoad.Add(new EntityParameterDoubleValueUpdate { ParameterName = parameterName, Value = value });
            }

            if (beforeTemperature)
            {
                SimulationSpecificUpdates[simulation][entity].ParameterUpdatesBeforeTemperature.Add(new EntityParameterDoubleValueUpdate { ParameterName = parameterName, Value = value });
            }
        }

        private Common.Evaluation.EvaluationContext GetEntityContext(Simulation simulation, IEntity entity)
        {
            var contextName = string.Empty;
            var dotIndex = entity.Name.LastIndexOf('.');
            if (dotIndex >= 0)
            {
                contextName = entity.Name.Substring(0, dotIndex);
            }

            var context = Contexts.GetContext(simulation).Find(contextName);
            return context;
        }
    }
}