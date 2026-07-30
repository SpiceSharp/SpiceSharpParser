using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SpiceSharp.Entities;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.CustomComponents
{
    /// <summary>
    /// Coordinates the translation of supported LTspice A-devices into portable
    /// subcircuit entities.
    /// </summary>
    /// <remarks>
    /// The translation is deliberately split into five readable stages:
    /// syntax reading, catalog lookup, validation, terminal resolution, and
    /// expansion. Definitions contain the wiring table; the expander contains
    /// the mechanics.
    /// </remarks>
    public sealed class LTspiceADeviceGenerator : IComponentGenerator
    {
        private readonly LTspiceADeviceExpander _expander;

        /// <summary>
        /// Initializes a new instance of the <see cref="LTspiceADeviceGenerator"/> class.
        /// </summary>
        public LTspiceADeviceGenerator()
        {
            _expander = new LTspiceADeviceExpander();
        }

        /// <inheritdoc />
        public IEntity Generate(
            string componentIdentifier,
            string originalName,
            string type,
            ParameterCollection parameters,
            IReadingContext context)
        {
            // Stage 1: turn the positional netlist syntax into a named object.
            if (!LTspiceADeviceInstanceReader.TryRead(
                    originalName,
                    parameters,
                    context,
                    out LTspiceADeviceInstance instance))
            {
                return null;
            }

            // Stage 2: select one declarative native-to-portable translation.
            if (!LTspiceADeviceCatalog.TryGet(
                    instance.Model,
                    out LTspiceADeviceDefinition definition))
            {
                LTspiceADeviceDiagnostics.AddError(
                    context,
                    $"Unsupported LTspice A-device model '{instance.Model}' on component "
                    + $"'{originalName}'. Supported models are "
                    + LTspiceADeviceCatalog.SupportedModelList
                    + ".",
                    instance.ModelLineInfo);
                return null;
            }

            // Stage 3: reject invalid values before expansion changes the circuit.
            if (!LTspiceADeviceValidator.TryValidate(
                    originalName,
                    instance.Model,
                    instance.Parameters,
                    context))
            {
                return null;
            }

            // Stage 4: resolve terminal-8 common/unused semantics.
            string portableInstanceName = CreatePortableInstanceName(
                componentIdentifier,
                context);
            var connections = LTspiceADeviceConnections.Create(
                instance,
                definition,
                portableInstanceName,
                context.ReaderSettings.CaseSensitivity.IsNodeNameCaseSensitive);

            // Stage 5: expansion can add many entities, so treat it as one atomic
            // operation from the reader's point of view.
            List<IEntity> originalEntities = context.ContextEntities.ToList();
            try
            {
                if (!_expander.TryExpand(
                        context,
                        originalName,
                        portableInstanceName,
                        instance,
                        definition,
                        connections))
                {
                    return null;
                }
            }
            catch (Exception exception) when (
                exception is ArgumentException
                || exception is InvalidOperationException
                || exception is SpiceSubcircuitLibraryException)
            {
                RollBackExpansion(context, originalEntities);
                LTspiceADeviceDiagnostics.AddError(
                    context,
                    $"Could not expand LTspice A-device '{originalName}' "
                    + $"({instance.Model}): {exception.Message}",
                    parameters.LineInfo);
            }

            // A-device expansion adds child entities directly to ContextEntities.
            // There is intentionally no single wrapper entity to return.
            return null;
        }

        private static string CreatePortableInstanceName(
            string componentIdentifier,
            IReadingContext context)
        {
            string preferredName = "X" + componentIdentifier;
            if (!HasGeneratedNameCollision(context, preferredName))
            {
                return preferredName;
            }

            string baseName = "X__a_" + componentIdentifier;
            string candidate = baseName;
            int suffix = 1;
            while (HasGeneratedNameCollision(context, candidate))
            {
                candidate = baseName
                    + "_"
                    + suffix.ToString(CultureInfo.InvariantCulture);
                suffix++;
            }

            return candidate;
        }

        private static bool HasGeneratedNameCollision(
            IReadingContext context,
            string instanceName)
        {
            StringComparison comparison =
                context.ReaderSettings.CaseSensitivity.IsEntityNamesCaseSensitive
                    ? StringComparison.Ordinal
                    : StringComparison.OrdinalIgnoreCase;
            string childPrefix = instanceName + context.NameGenerator.Separator;
            return context.ContextEntities.Contains(instanceName)
                || context.ContextEntities.Contains("V" + instanceName + ".__a_vhigh")
                || context.ContextEntities.Contains("V" + instanceName + ".__a_vlow")
                || context.ContextEntities.Any(
                    entity => entity.Name.StartsWith(childPrefix, comparison));
        }

        private static void RollBackExpansion(
            IReadingContext context,
            IReadOnlyCollection<IEntity> originalEntities)
        {
            List<IEntity> added = context.ContextEntities
                .Where(entity => !originalEntities.Any(
                    original => ReferenceEquals(original, entity)))
                .ToList();
            for (int index = added.Count - 1; index >= 0; index--)
            {
                context.ContextEntities.Remove(added[index].Name);
            }
        }
    }
}
