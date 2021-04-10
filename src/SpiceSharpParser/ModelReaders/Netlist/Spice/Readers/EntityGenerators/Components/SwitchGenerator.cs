using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Globalization;
using SpiceSharpParser.Common.Validation;
using SpiceSharp.Entities;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Custom;
using Model = SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models.Model;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components
{
    public class SwitchGenerator : ComponentGenerator
    {
        public override IEntity Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, ICircuitContext context)
        {
            switch (type.ToLower())
            {
                case "s": return GenerateVoltageSwitch(componentIdentifier, parameters, context);
                case "w": return GenerateCurrentSwitch(componentIdentifier, parameters, context);
            }

            return null;
        }

        /// <summary>
        /// Generates a voltage switch.
        /// </summary>
        /// <param name="name">Name of voltage switch to generate.</param>
        /// <param name="parameters">Parameters for voltage switch.</param>
        /// <param name="context">Reading context.</param>
        /// <returns>
        /// A new voltage switch.
        /// </returns>
        protected IEntity GenerateVoltageSwitch(string name, ParameterCollection parameters, ICircuitContext context)
        {
            if (parameters.Count < 5)
            {
                context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, "Wrong parameter count for voltage switch", parameters.LineInfo));
                return null;
            }

            string modelName = parameters.Get(4).Image;

            var model = context.ModelsRegistry.FindModel(modelName);
            if (model.Entity is VSwitchModel)
            {
                Resistor resistor = new Resistor(name);
                Model resistorModel = model;
                context.CreateNodes(resistor, parameters.Take(2));
                context.SimulationPreparations.ExecuteTemperatureBehaviorBeforeLoad(resistor);

                context.SimulationPreparations.ExecuteActionAfterSetup(
                    (simulation) =>
                    {
                        if (context.ModelsRegistry is StochasticModelsRegistry stochasticModelsRegistry)
                        {
                            resistorModel = stochasticModelsRegistry.ProvideStochasticModel(name, simulation, model);

                            if (!context.Result.FindObject(resistorModel.Name, out _))
                            {
                                stochasticModelsRegistry.RegisterModelInstance(resistorModel);
                            }
                        }
                        
                        string resExpression =
                            $"table(v({parameters.Get(2)}, {parameters.Get(3)}), @{resistorModel.Name}[voff], @{resistorModel.Name}[roff] , @{resistorModel.Name}[von], @{resistorModel.Name}[ron])";
                        context.SetParameter(resistor, "resistance", resExpression, beforeTemperature: true, onload: true, simulation);
                    });
                return resistor;
            }

            VoltageSwitch vsw = new VoltageSwitch(name);
            context.CreateNodes(vsw, parameters);

            context.SimulationPreparations.ExecuteActionBeforeSetup((simulation) =>
            {
                context.ModelsRegistry.SetModel(
                    vsw,
                    simulation,
                    parameters.Get(4),
                    $"Could not find model {parameters.Get(4)} for voltage switch {name}",
                    (Context.Models.Model model2) => { vsw.Model = model2.Name; },
                    context.Result);
            });

            // Optional ON or OFF
            if (parameters.Count == 6)
            {
                switch (parameters.Get(5).Image.ToLower())
                {
                    case "on":
                        vsw.SetParameter("on", true);
                        break;

                    case "off":
                        vsw.SetParameter("off", true);
                        break;

                    default:
                        context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, "ON or OFF expected", parameters.LineInfo));
                        return vsw;
                }
            }
            else if (parameters.Count > 6)
            {
                context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, "Too many parameters for voltage switch", parameters.LineInfo));
                return vsw;
            }

            return vsw;
        }

        /// <summary>
        /// Generates a current switch.
        /// </summary>
        /// <param name="name">Name of current switch.</param>
        /// <param name="parameters">Parameters of current switch.</param>
        /// <param name="context">Reading context.</param>
        /// <returns>
        /// A new instance of current switch.
        /// </returns>
        protected IEntity GenerateCurrentSwitch(string name, ParameterCollection parameters, ICircuitContext context)
        {
            if (parameters.Count < 4)
            {
                context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, "Wrong parameter count for current switch", parameters.LineInfo));
                return null;
            }

            string modelName = parameters.Get(3).Image;
            var model = context.ModelsRegistry.FindModel(modelName);
            if (model.Entity is ISwitchModel)
            {
                Resistor resistor = new Resistor(name);
                Model resistorModel = model;
                context.CreateNodes(resistor, parameters.Take(2));
                context.SimulationPreparations.ExecuteTemperatureBehaviorBeforeLoad(resistor);

                context.SimulationPreparations.ExecuteActionAfterSetup(
                    (simulation) =>
                    {
                        if (context.ModelsRegistry is StochasticModelsRegistry stochasticModelsRegistry)
                        {
                            resistorModel = stochasticModelsRegistry.ProvideStochasticModel(name, simulation, model);

                            if (!context.Result.FindObject(resistorModel.Name, out _))
                            {
                                stochasticModelsRegistry.RegisterModelInstance(resistorModel);
                            }
                        }
                        double rOff = resistorModel.Parameters.GetProperty<double>("roff");
                        string resExpression = $"table(i({parameters.Get(2)}), @{resistorModel.Name}[ioff], @{resistorModel.Name}[roff] , @{resistorModel.Name}[ion], @{resistorModel.Name}[ron])";
                        context.SetParameter(resistor, "resistance", resExpression, beforeTemperature: true, onload: true, simulation);
                    });
                return resistor;
            }

            CurrentSwitch csw = new CurrentSwitch(name);
            context.CreateNodes(csw, parameters);

            // Get the controlling voltage source
            if (parameters[2] is WordParameter || parameters[2] is IdentifierParameter)
            {
                csw.ControllingSource = context.NameGenerator.GenerateObjectName(parameters.Get(2).Image);
            }
            else
            {
                context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, "Voltage source name expected", parameters.LineInfo));
                return null;
            }

            // Get the model
            context.SimulationPreparations.ExecuteActionBeforeSetup((simulation) =>
            {
                context.ModelsRegistry.SetModel(
                    csw,
                    simulation,
                    parameters.Get(3),
                    $"Could not find model {parameters.Get(3)} for current switch {name}",
                    (Context.Models.Model switchModel) => csw.Model = switchModel.Name,
                    context.Result);
            });

            // Optional on or off
            if (parameters.Count > 4)
            {
                switch (parameters.Get(4).Image.ToLower())
                {
                    case "on":
                        csw.SetParameter("on", true);
                        break;

                    case "off":
                        csw.SetParameter("off", true);
                        break;

                    default:
                        context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, "ON or OFF expected", parameters.LineInfo));
                        return csw;
                }
            }

            return csw;
        }
    }
}