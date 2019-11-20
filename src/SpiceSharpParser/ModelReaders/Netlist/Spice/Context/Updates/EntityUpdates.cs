using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation.Expressions;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Updates
{
    public class EntityUpdates
    {
        public EntityUpdates(bool isParameterNameCaseSensitive, SimulationExpressionContexts contexts)
        {
            IsParameterNameCaseSensitive = isParameterNameCaseSensitive;
            Contexts = contexts ?? throw new ArgumentNullException(nameof(contexts));
            CommonUpdates = new Dictionary<Entity, EntityUpdate>();
            SimulationSpecificUpdates = new Dictionary<Simulation, Dictionary<Entity, EntityUpdate>>();
            SimulationEntityParametersCache = new ConcurrentDictionary<string, Parameter<double>>();
        }

        protected bool IsParameterNameCaseSensitive { get; }

        protected SimulationExpressionContexts Contexts { get; set; }

        protected Dictionary<Entity, EntityUpdate> CommonUpdates { get; set; }

        protected Dictionary<Simulation, Dictionary<Entity, EntityUpdate>> SimulationSpecificUpdates { get; set; }

        protected ConcurrentDictionary<string, Parameter<double>> SimulationEntityParametersCache { get; }

        public void Apply(BaseSimulation simulation)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            simulation.BeforeLoad += (sender, args) =>
            {
                foreach (var entity in CommonUpdates.Keys)
                {
                    var beforeLoads = CommonUpdates[entity].ParameterUpdatesBeforeLoad;

                    foreach (var entityUpdate in beforeLoads)
                    {
                        var parameter = GetEntitySimulationParameter(entityUpdate.ParameterName, entity, simulation, StringComparerProvider.Get(IsParameterNameCaseSensitive));

                        if (parameter != null)
                        {
                            Common.Evaluation.ExpressionContext context = GetEntityContext(simulation, entity);

                            var value = entityUpdate.GetValue(context);

                            if (!double.IsNaN(value))
                            {
                                parameter.Value = value;
                            }
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
                            var parameter = GetEntitySimulationParameter(entityUpdate.ParameterName, entityPair.Key, simulation, StringComparerProvider.Get(IsParameterNameCaseSensitive));

                            if (parameter != null)
                            {
                                Common.Evaluation.ExpressionContext context = GetEntityContext(simulation, entityPair.Key);

                                var value = entityUpdate.GetValue(context);
                                if (!double.IsNaN(value))
                                {
                                    parameter.Value = value;
                                }
                            }
                        }
                    }
                }
            };

            simulation.BeforeTemperature += (sender, args) =>
            {
                foreach (var entity in CommonUpdates.Keys)
                {
                    var beforeTemperature = CommonUpdates[entity].ParameterUpdatesBeforeTemperature;

                    foreach (var entityUpdate in beforeTemperature)
                    {
                        var parameter = GetEntitySimulationParameter(entityUpdate.ParameterName, entity, simulation, StringComparerProvider.Get(IsParameterNameCaseSensitive));

                        if (parameter != null)
                        {
                            Common.Evaluation.ExpressionContext context = GetEntityContext(simulation, entity);
                            var value = entityUpdate.GetValue(context);

                            if (!double.IsNaN(value))
                            {
                                parameter.Value = value;
                            }
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
                            var parameter = GetEntitySimulationParameter(entityUpdate.ParameterName, entityPair.Key, simulation, StringComparerProvider.Get(IsParameterNameCaseSensitive));

                            if (parameter != null)
                            {
                                Common.Evaluation.ExpressionContext context = GetEntityContext(simulation, entityPair.Key);

                                var value = entityUpdate.GetValue(context);
                                if (!double.IsNaN(value))
                                {
                                    parameter.Value = value;
                                }
                            }
                        }
                    }
                }
            };
        }

        public void Add(Entity entity, string parameterName, string expression, bool beforeTemperature, bool beforeLoad, ICircuitContext circuitContext)
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

            if (CommonUpdates.ContainsKey(entity) == false)
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
                    ParameterName = parameterName
                });
            }
        }

        public void Add(Entity entity, Simulation simulation, string parameterName, string expression, bool beforeTemperature, bool beforeLoad, ICircuitContext circuitContext)
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

            if (SimulationSpecificUpdates.ContainsKey(simulation) == false)
            {
                SimulationSpecificUpdates[simulation] = new Dictionary<Entity, EntityUpdate>();
            }

            if (SimulationSpecificUpdates[simulation].ContainsKey(entity) == false)
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
                    ParameterName = parameterName
                });
            }
        }

        public void Add(Entity entity, string parameterName, double value, bool beforeTemperature, bool beforeLoad)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (parameterName == null)
            {
                throw new ArgumentNullException(nameof(parameterName));
            }

            if (CommonUpdates.ContainsKey(entity) == false)
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

        public void Add(Entity entity, Simulation simulation, string parameterName, double value, bool beforeTemperature, bool beforeLoad)
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

            if (SimulationSpecificUpdates.ContainsKey(simulation) == false)
            {
                SimulationSpecificUpdates[simulation] = new Dictionary<Entity, EntityUpdate>();
            }

            if (SimulationSpecificUpdates[simulation].ContainsKey(entity) == false)
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

        private Common.Evaluation.ExpressionContext GetEntityContext(BaseSimulation simulation, Entity entity)
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

        private Parameter<double> GetEntitySimulationParameter(string paramName, Entity @object, BaseSimulation simulation, IEqualityComparer<string> comparer)
        {
            string key = $"{simulation.Name}_{@object.Name}_{paramName}_{(comparer != null ? comparer.ToString() : "null")}";

            if (!SimulationEntityParametersCache.TryGetValue(key, out var result))
            {
                result = simulation.EntityParameters[@object.Name].GetParameter<Parameter<double>>(paramName, comparer);
                SimulationEntityParametersCache[key] = result;
            }

            return result;
        }
    }
}