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
                            ApplyParameterUpdate(entity, entityUpdate, context);
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
                                ApplyParameterUpdate(entityPair.Key, entityUpdate, context);
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

            var simulationUpdates = SimulationSpecificUpdates.GetOrAdd(simulation, _ => new Dictionary<IEntity, EntityUpdate>());
            var entityUpdate = simulationUpdates.ContainsKey(entity)
                ? simulationUpdates[entity]
                : (simulationUpdates[entity] = new EntityUpdate());

            if (beforeTemperature)
            {
                entityUpdate.ParameterUpdatesBeforeTemperature.Add(new EntityParameterExpressionValueUpdate()
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

            var entityUpdate = CommonUpdates.GetOrAdd(entity, _ => new EntityUpdate());

            if (beforeTemperature)
            {
                entityUpdate.ParameterUpdatesBeforeTemperature.Add(new EntityParameterExpressionValueUpdate()
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

            var entityUpdate = CommonUpdates.GetOrAdd(entity, _ => new EntityUpdate());

            if (beforeTemperature)
            {
                entityUpdate.ParameterUpdatesBeforeTemperature.Add(
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

            var simulationUpdates = SimulationSpecificUpdates.GetOrAdd(simulation, _ => new Dictionary<IEntity, EntityUpdate>());
            var entityUpdate = simulationUpdates.ContainsKey(entity)
                ? simulationUpdates[entity]
                : (simulationUpdates[entity] = new EntityUpdate());

            if (beforeTemperature)
            {
                entityUpdate.ParameterUpdatesBeforeTemperature.Add(new EntityParameterDoubleValueUpdate { ParameterName = parameterName, Value = value });
            }
        }

        private EvaluationContext GetEntityContext(ISimulationWithEvents simulation, string entityName)
        {
            var context = Context.GetSimulationContext(simulation).Find(entityName);
            return context;
        }

        private static void ApplyParameterUpdate(IEntity entity, EntityParameterUpdate entityUpdate, EvaluationContext context)
        {
            if (TryGetValue(entityUpdate, context, out double value) && !double.IsNaN(value))
            {
                entity.SetParameter(entityUpdate.ParameterName, value);
            }
        }

        private static bool TryGetValue(EntityParameterUpdate entityUpdate, EvaluationContext context, out double value)
        {
            if (context != null)
            {
                value = entityUpdate.GetValue(context);
                return true;
            }

            if (entityUpdate is EntityParameterDoubleValueUpdate doubleUpdate)
            {
                value = doubleUpdate.Value;
                return true;
            }

            value = double.NaN;
            return false;
        }
    }
}
