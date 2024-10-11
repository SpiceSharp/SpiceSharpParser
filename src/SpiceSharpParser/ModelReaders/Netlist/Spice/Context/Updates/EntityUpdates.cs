using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation.Expressions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Updates
{
    public class EntityUpdates
    {
        public EntityUpdates(bool isParameterNameCaseSensitive, EvaluationContext context)
        {
            IsParameterNameCaseSensitive = isParameterNameCaseSensitive;
            Context = context;
            CommonUpdates = new ConcurrentDictionary<IEntity, EntityUpdate>();
            SimulationSpecificUpdates = new ConcurrentDictionary<ISimulationWithEvents, Dictionary<IEntity, EntityUpdate>>();
            SimulationEntityParametersCache = new ConcurrentDictionary<string, double>();
        }

        public EvaluationContext Context { get; }

        protected bool IsParameterNameCaseSensitive { get; }

        protected ConcurrentDictionary<IEntity, EntityUpdate> CommonUpdates { get; set; }

        protected ConcurrentDictionary<ISimulationWithEvents, Dictionary<IEntity, EntityUpdate>> SimulationSpecificUpdates { get; set; }

        protected ConcurrentDictionary<string, double> SimulationEntityParametersCache { get; }

        public void Apply(ISimulationWithEvents simulation)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            if (simulation is ISimulationWithEvents biasingSimulation && biasingSimulation is BiasingSimulation)
            {
                biasingSimulation.EventBeforeTemperature += (_, _) =>
                {
                    foreach (var entity in CommonUpdates.Keys)
                    {
                        var beforeTemperature = CommonUpdates[entity].ParameterUpdatesBeforeTemperature;

                        foreach (var entityUpdate in beforeTemperature)
                        {
                            EvaluationContext context = GetEntityContext(biasingSimulation, entity.Name);
                            if (context != null)
                            {
                                var value = entityUpdate.GetValue(context);
                                if (!double.IsNaN(value))
                                {
                                    entity.CreateParameterSetter<double>(entityUpdate.ParameterName)?.Invoke(value);
                                }
                            }
                        }
                    }
                };

                biasingSimulation.EventBeforeTemperature += (_, _) =>
                {
                    if (SimulationSpecificUpdates.ContainsKey(simulation))
                    {
                        foreach (var entityPair in SimulationSpecificUpdates[simulation])
                        {
                            var beforeTemperature = entityPair.Value.ParameterUpdatesBeforeTemperature;

                            foreach (var entityUpdate in beforeTemperature)
                            {
                                EvaluationContext context = GetEntityContext(biasingSimulation, entityPair.Key.Name);
                                if (context != null)
                                {
                                    var value = entityUpdate.GetValue(context);
                                    if (!double.IsNaN(value))
                                    {
                                        entityPair.Key.CreateParameterSetter<double>(entityUpdate.ParameterName)?.Invoke(value);
                                    }
                                }
                            }
                        }
                    }
                };
            }
        }

        public void Add(IEntity entity, string parameterName, string expression, bool beforeTemperature, ISimulationWithEvents simulation)
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

            if (beforeTemperature)
            {
                SimulationSpecificUpdates[simulation][entity].ParameterUpdatesBeforeTemperature.Add(new EntityParameterExpressionValueUpdate()
                {
                    Expression = new DynamicExpression(expression),
                    ParameterName = parameterName,
                });
            }
        }

        public void Add(IEntity entity, string parameterName, string expression, bool beforeTemperature)
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

            if (beforeTemperature)
            {
                CommonUpdates[entity].ParameterUpdatesBeforeTemperature.Add(new EntityParameterExpressionValueUpdate()
                {
                    Expression = new DynamicExpression(expression),
                    ParameterName = parameterName,
                });
            }
        }

        public void Add(IEntity entity, string parameterName, double value, bool beforeTemperature)
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

            if (beforeTemperature)
            {
                CommonUpdates[entity].ParameterUpdatesBeforeTemperature.Add(
                    new EntityParameterDoubleValueUpdate() { ParameterName = parameterName, Value = value });
            }
        }

        public void Add(IEntity entity, string parameterName, double value, bool beforeTemperature, ISimulationWithEvents simulation)
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

            if (beforeTemperature)
            {
                SimulationSpecificUpdates[simulation][entity].ParameterUpdatesBeforeTemperature.Add(new EntityParameterDoubleValueUpdate { ParameterName = parameterName, Value = value });
            }
        }

        private EvaluationContext GetEntityContext(ISimulationWithEvents simulation, string entityName)
        {
            var context = Context.GetSimulationContext(simulation).Find(entityName);
            return context;
        }
    }
}