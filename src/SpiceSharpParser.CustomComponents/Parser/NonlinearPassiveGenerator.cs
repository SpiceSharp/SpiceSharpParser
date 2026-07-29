using SpiceSharp.Entities;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.CustomComponents
{
    /// <summary>
    /// Creates LTspice-style nonlinear passive components when custom components are enabled.
    /// </summary>
    public class NonlinearPassiveGenerator : IComponentGenerator
    {
        private static readonly ISet<string> OptionalParameterNames =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "ic",
                "m",
                "n",
            };

        private readonly RLCKGenerator _fallback = new RLCKGenerator();

        /// <inheritdoc />
        public IEntity Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, IReadingContext context)
        {
            switch (type.ToLowerInvariant())
            {
                case "c":
                    return GenerateCapacitor(componentIdentifier, originalName, parameters, context);
                case "l":
                    return GenerateInductor(componentIdentifier, originalName, parameters, context);
                default:
                    return _fallback.Generate(componentIdentifier, originalName, type, parameters, context);
            }
        }

        private IEntity GenerateCapacitor(string name, string originalName, ParameterCollection parameters, IReadingContext context)
        {
            var charge = GetAssignment(parameters, 2, "q");
            if (charge == null)
            {
                return _fallback.Generate(name, originalName, "c", parameters, context);
            }

            if (!ValidateTwoTerminal(name, "capacitor", parameters, context))
            {
                return null;
            }

            if (!TryReadOptionalParameters(
                    originalName,
                    "capacitor",
                    parameters,
                    context,
                    out IReadOnlyList<AssignmentParameter> optionalParameters))
            {
                return null;
            }

            var capacitor = new NonlinearCapacitor(name);
            context.CreateNodes(capacitor, parameters.Take(NonlinearCapacitor.PinCount));
            capacitor.Parameters.Expression = charge.Value;
            SimulationExpressionResolver.Configure(
                context,
                charge.Value,
                parser => capacitor.Parameters.ParseAction = parser);

            foreach (AssignmentParameter parameter in optionalParameters)
            {
                context.SetParameter(capacitor, parameter.Name, parameter.Value);
            }

            return capacitor;
        }

        private IEntity GenerateInductor(string name, string originalName, ParameterCollection parameters, IReadingContext context)
        {
            var flux = GetAssignment(parameters, 2, "flux");
            if (flux == null)
            {
                return _fallback.Generate(name, originalName, "l", parameters, context);
            }

            if (!ValidateTwoTerminal(name, "inductor", parameters, context))
            {
                return null;
            }

            if (!TryReadOptionalParameters(
                    originalName,
                    "inductor",
                    parameters,
                    context,
                    out IReadOnlyList<AssignmentParameter> optionalParameters))
            {
                return null;
            }

            var inductor = new NonlinearInductor(name);
            context.CreateNodes(inductor, parameters.Take(NonlinearInductor.PinCount));
            inductor.Parameters.Expression = flux.Value;
            SimulationExpressionResolver.Configure(
                context,
                flux.Value,
                parser => inductor.Parameters.ParseAction = parser);

            foreach (AssignmentParameter parameter in optionalParameters)
            {
                context.SetParameter(inductor, parameter.Name, parameter.Value);
            }

            return inductor;
        }

        private static bool TryReadOptionalParameters(
            string originalName,
            string kind,
            ParameterCollection parameters,
            IReadingContext context,
            out IReadOnlyList<AssignmentParameter> result)
        {
            var assignments = new List<AssignmentParameter>();
            foreach (Parameter parameter in parameters.Skip(3))
            {
                if (!(parameter is AssignmentParameter assignment))
                {
                    context.Result.ValidationResult.AddError(
                        ValidationEntrySource.Reader,
                        $"Invalid parameter for nonlinear {kind} '{originalName}': '{parameter}'.",
                        parameter.LineInfo);
                    result = null;
                    return false;
                }

                if (!OptionalParameterNames.Contains(assignment.Name))
                {
                    context.Result.ValidationResult.AddError(
                        ValidationEntrySource.Reader,
                        $"Unsupported LTspice nonlinear {kind} parameter '{assignment.Name}' on component '{originalName}'.",
                        assignment.LineInfo);
                    result = null;
                    return false;
                }

                assignments.Add(assignment);
            }

            result = assignments;
            return true;
        }

        private static bool ValidateTwoTerminal(string name, string kind, ParameterCollection parameters, IReadingContext context)
        {
            if (parameters.Count < 3)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    $"Nonlinear {kind} '{name}' expects two nodes and an expression.",
                    parameters.LineInfo);
                return false;
            }

            return true;
        }

        private static AssignmentParameter GetAssignment(ParameterCollection parameters, int index, string name)
        {
            if (parameters.Count <= index)
            {
                return null;
            }

            return parameters[index] is AssignmentParameter assignment
                && assignment.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                    ? assignment
                    : null;
        }
    }
}
