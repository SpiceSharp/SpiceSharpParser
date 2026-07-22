using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using SpiceSharp;
using SpiceSharp.Entities;
using SpiceSharpParser.Common;
using SpiceSharpParser.Diagnostics;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser
{
    /// <summary>
    /// Loads reusable .SUBCKT definitions and adds their instances to programmatically built SpiceSharp circuits.
    /// </summary>
    public sealed class SpiceSubcircuitLibrary
    {
        private static readonly HashSet<string> SupportControlNames = new HashSet<string>(
            new[]
            {
                "param",
                "sparam",
                "func",
                "global",
                "connect",
                "distribution",
                "options",
                "let",
            },
            StringComparer.OrdinalIgnoreCase);

        private readonly SpiceNetlist _template;
        private readonly SpiceCompileOptions _options;
        private readonly string _workingDirectory;
        private readonly HashSet<string> _supportEntityNames;
        private readonly IEqualityComparer<string> _entityNameComparer;
        private readonly ConditionalWeakTable<Circuit, InstalledSupportEntities> _installedSupportEntities =
            new ConditionalWeakTable<Circuit, InstalledSupportEntities>();

        private SpiceSubcircuitLibrary(
            SpiceNetlist template,
            SpiceCompileOptions options,
            string workingDirectory,
            string sourceName,
            IDictionary<string, SpiceSubcircuitInfo> subcircuits,
            IEnumerable<string> supportEntityNames,
            IEnumerable<SpiceDiagnostic> diagnostics,
            IEnumerable<SpiceDependency> dependencies)
        {
            _template = template ?? throw new ArgumentNullException(nameof(template));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _workingDirectory = workingDirectory ?? throw new ArgumentNullException(nameof(workingDirectory));
            SourceName = sourceName;
            Subcircuits = new ReadOnlyDictionary<string, SpiceSubcircuitInfo>(subcircuits);
            _entityNameComparer = StringComparerProvider.Get(options.CaseSensitivity.IsEntityNamesCaseSensitive);
            _supportEntityNames = new HashSet<string>(supportEntityNames, _entityNameComparer);
            Diagnostics = new List<SpiceDiagnostic>(diagnostics).AsReadOnly();
            Dependencies = new List<SpiceDependency>(dependencies).AsReadOnly();
        }

        /// <summary>
        /// Gets the source path or display name supplied when the library was loaded.
        /// </summary>
        public string SourceName { get; }

        /// <summary>
        /// Gets the top-level subcircuits indexed using the configured subcircuit-name case rules.
        /// </summary>
        public IReadOnlyDictionary<string, SpiceSubcircuitInfo> Subcircuits { get; }

        /// <summary>
        /// Gets structured diagnostics produced while loading the library.
        /// </summary>
        public IReadOnlyList<SpiceDiagnostic> Diagnostics { get; }

        /// <summary>
        /// Gets all recursively discovered .INCLUDE and file-backed .LIB dependencies.
        /// </summary>
        public IReadOnlyList<SpiceDependency> Dependencies { get; }

        /// <summary>
        /// Gets information about a named subcircuit.
        /// </summary>
        /// <param name="subcircuitName">The .SUBCKT name.</param>
        /// <returns>The loaded subcircuit information.</returns>
        public SpiceSubcircuitInfo this[string subcircuitName] => Subcircuits[subcircuitName];

        /// <summary>
        /// Loads a SPICE include-style file containing one or more .SUBCKT definitions.
        /// The file itself does not need a title or .END statement.
        /// </summary>
        /// <param name="filePath">The library file path.</param>
        /// <param name="options">Compilation and reader options, or null for defaults.</param>
        /// <returns>A reusable subcircuit library.</returns>
        public static SpiceSubcircuitLibrary LoadFile(
            string filePath,
            SpiceCompileOptions options = null)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("A subcircuit library path is required.", nameof(filePath));
            }

            string fullPath = Path.GetFullPath(filePath);
            string workingDirectory = options?.WorkingDirectory
                ?? Path.GetDirectoryName(fullPath)
                ?? Environment.CurrentDirectory;
            SpiceCompileOptions effectiveOptions = CloneOptions(options, workingDirectory);
            string source = Lines(
                "SpiceSharpParser subcircuit library loader",
                $".INCLUDE \"{fullPath}\"",
                ".END");

            SpiceCompilationResult compilation = SpiceCompiler.CompileText(
                source,
                fullPath,
                effectiveOptions);

            return Create(compilation, effectiveOptions, workingDirectory, fullPath);
        }

        /// <summary>
        /// Loads include-style SPICE source containing one or more .SUBCKT definitions.
        /// The source does not need a title or .END statement.
        /// </summary>
        /// <param name="source">The SPICE library source.</param>
        /// <param name="options">Compilation and reader options, or null for defaults.</param>
        /// <returns>A reusable subcircuit library.</returns>
        public static SpiceSubcircuitLibrary LoadText(
            string source,
            SpiceCompileOptions options = null)
        {
            return LoadText(source, null, options);
        }

        /// <summary>
        /// Loads named include-style SPICE source containing one or more .SUBCKT definitions.
        /// The source does not need a title or .END statement.
        /// </summary>
        /// <param name="source">The SPICE library source.</param>
        /// <param name="sourceName">A source path or display name used in diagnostics.</param>
        /// <param name="options">Compilation and reader options, or null for defaults.</param>
        /// <returns>A reusable subcircuit library.</returns>
        public static SpiceSubcircuitLibrary LoadText(
            string source,
            string sourceName,
            SpiceCompileOptions options = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            string workingDirectory = options?.WorkingDirectory ?? Environment.CurrentDirectory;
            SpiceCompileOptions effectiveOptions = CloneOptions(options, workingDirectory);
            string wrappedSource = Lines(
                "SpiceSharpParser subcircuit library loader",
                source,
                ".END");

            SpiceCompilationResult compilation = SpiceCompiler.CompileText(
                wrappedSource,
                sourceName,
                effectiveOptions);

            return Create(compilation, effectiveOptions, workingDirectory, sourceName);
        }

        /// <summary>
        /// Adds an instance to a SpiceSharp circuit using the ordered external node names.
        /// </summary>
        /// <param name="circuit">The target programmatic circuit.</param>
        /// <param name="subcircuitName">The loaded .SUBCKT name.</param>
        /// <param name="instanceName">A unique SPICE X instance name, such as XU1.</param>
        /// <param name="nodes">External nodes in the same order as the .SUBCKT pins.</param>
        /// <returns>All entities added to the target, including newly installed shared model entities.</returns>
        public IReadOnlyList<IEntity> AddInstance(
            Circuit circuit,
            string subcircuitName,
            string instanceName,
            params string[] nodes)
        {
            return AddInstance(circuit, subcircuitName, instanceName, nodes, null);
        }

        /// <summary>
        /// Adds a parameterized instance to a SpiceSharp circuit.
        /// </summary>
        /// <param name="circuit">The target programmatic circuit.</param>
        /// <param name="subcircuitName">The loaded .SUBCKT name.</param>
        /// <param name="instanceName">A unique SPICE X instance name, such as XU1.</param>
        /// <param name="nodes">External nodes in the same order as the .SUBCKT pins.</param>
        /// <param name="parameters">Optional parameter expressions overriding .SUBCKT defaults.</param>
        /// <returns>All entities added to the target, including newly installed shared model entities.</returns>
        public IReadOnlyList<IEntity> AddInstance(
            Circuit circuit,
            string subcircuitName,
            string instanceName,
            IEnumerable<string> nodes,
            IReadOnlyDictionary<string, string> parameters)
        {
            if (circuit == null)
            {
                throw new ArgumentNullException(nameof(circuit));
            }

            SpiceSubcircuitInfo info = GetSubcircuit(subcircuitName);
            ValidateInstanceName(instanceName);
            List<string> nodeList = ValidateNodes(nodes, info);
            ValidateParameters(parameters);

            SpiceNetlist instanceNetlist = CreateInstanceNetlist(
                info.Name,
                instanceName,
                nodeList,
                parameters);
            SpiceNetlistReadResult readResult = new SpiceNetlistReader(CreateReaderSettings()).ReadResult(instanceNetlist);

            if (!readResult.Success)
            {
                throw CreateFailure(
                    $"Could not instantiate subcircuit '{info.Name}' as '{instanceName}'.",
                    readResult.Diagnostics);
            }

            List<IEntity> entities = readResult.Model.Circuit.ToList();
            return AddEntities(circuit, entities);
        }

        private static SpiceSubcircuitLibrary Create(
            SpiceCompilationResult compilation,
            SpiceCompileOptions options,
            string workingDirectory,
            string sourceName)
        {
            var diagnostics = compilation.AllDiagnostics
                .Where(diagnostic => diagnostic.Stage != DiagnosticStage.Reader
                    && diagnostic.Stage != DiagnosticStage.Linter)
                .ToList();

            if (compilation.ExpandedModel == null
                || diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
            {
                throw CreateFailure("Could not load the subcircuit library.", diagnostics);
            }

            SpiceNetlist template = CreateTemplate(compilation.ExpandedModel);
            Dictionary<string, SpiceSubcircuitInfo> subcircuits = CreateSubcircuitIndex(
                template,
                options.CaseSensitivity);

            if (subcircuits.Count == 0)
            {
                diagnostics.Add(new SpiceDiagnostic(
                    SpiceDiagnosticCodes.ReaderError,
                    DiagnosticSeverity.Error,
                    DiagnosticStage.Reader,
                    "The source does not contain any top-level .SUBCKT definitions.",
                    new SourceSpan(sourceName, SourcePosition.Unknown, SourcePosition.Unknown),
                    suggestedFix: "Add at least one .SUBCKT/.ENDS definition to the library source."));
                throw CreateFailure("Could not load the subcircuit library.", diagnostics);
            }

            SpiceNetlistReadResult supportResult =
                new SpiceNetlistReader(CreateReaderSettings(options, workingDirectory)).ReadResult(template);
            diagnostics.AddRange(supportResult.Diagnostics);

            if (diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
            {
                throw CreateFailure("Could not load the subcircuit library.", diagnostics);
            }

            IEnumerable<string> supportEntityNames = supportResult.Model.Circuit.Select(entity => entity.Name);
            return new SpiceSubcircuitLibrary(
                template,
                options,
                workingDirectory,
                sourceName,
                subcircuits,
                supportEntityNames,
                diagnostics,
                compilation.Dependencies);
        }

        private static SpiceNetlist CreateTemplate(SpiceNetlist expandedModel)
        {
            var statements = new Statements();
            foreach (Statement statement in expandedModel.Statements)
            {
                if (statement is SubCircuit
                    || statement is Model
                    || (statement is Control control && SupportControlNames.Contains(control.Name)))
                {
                    statements.Add((Statement)statement.Clone());
                }
            }

            return new SpiceNetlist(expandedModel.Title, statements);
        }

        private static Dictionary<string, SpiceSubcircuitInfo> CreateSubcircuitIndex(
            SpiceNetlist template,
            SpiceNetlistCaseSensitivitySettings caseSensitivity)
        {
            IEqualityComparer<string> subcircuitComparer = StringComparerProvider.Get(
                caseSensitivity.IsSubcircuitNameCaseSensitive);
            IEqualityComparer<string> parameterComparer = StringComparerProvider.Get(
                caseSensitivity.IsParameterNameCaseSensitive);
            var result = new Dictionary<string, SpiceSubcircuitInfo>(subcircuitComparer);

            foreach (SubCircuit definition in template.Statements.OfType<SubCircuit>())
            {
                var defaults = new Dictionary<string, string>(parameterComparer);
                foreach (AssignmentParameter parameter in definition.DefaultParameters)
                {
                    defaults[parameter.Name] = parameter.Value;
                }

                result[definition.Name] = new SpiceSubcircuitInfo(
                    definition.Name,
                    definition.Pins.Select(pin => pin.Value),
                    defaults);
            }

            return result;
        }

        private static void ValidateInstanceName(string instanceName)
        {
            if (instanceName == null)
            {
                throw new ArgumentNullException(nameof(instanceName));
            }

            if (string.IsNullOrWhiteSpace(instanceName))
            {
                throw new ArgumentException("An instance name is required.", nameof(instanceName));
            }

            if (!instanceName.StartsWith("X", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "A subcircuit instance name must start with 'X'.",
                    nameof(instanceName));
            }
        }

        private static List<string> ValidateNodes(
            IEnumerable<string> nodes,
            SpiceSubcircuitInfo info)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException(nameof(nodes));
            }

            List<string> result = nodes.ToList();
            if (result.Count != info.Pins.Count)
            {
                throw new ArgumentException(
                    $"Subcircuit '{info.Name}' requires {info.Pins.Count} nodes "
                    + $"({string.Join(", ", info.Pins)}), but {result.Count} were supplied.",
                    nameof(nodes));
            }

            if (result.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("Subcircuit node names cannot be null or whitespace.", nameof(nodes));
            }

            return result;
        }

        private static void ValidateParameters(IReadOnlyDictionary<string, string> parameters)
        {
            if (parameters == null)
            {
                return;
            }

            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                if (string.IsNullOrWhiteSpace(parameter.Key))
                {
                    throw new ArgumentException(
                        "Subcircuit parameter names cannot be null or whitespace.",
                        nameof(parameters));
                }

                if (string.IsNullOrWhiteSpace(parameter.Value))
                {
                    throw new ArgumentException(
                        $"Subcircuit parameter '{parameter.Key}' requires a value expression.",
                        nameof(parameters));
                }
            }
        }

        private static SpiceNetlistReaderSettings CreateReaderSettings(
            SpiceCompileOptions options,
            string workingDirectory)
        {
            CompatibilityOptions compatibility = GetCompatibility(options.Dialect);
            var settings = new SpiceNetlistReaderSettings(
                options.CaseSensitivity,
                () => workingDirectory,
                options.ExternalFilesEncoding,
                options.Separator,
                options.ExpandSubcircuits)
            {
                Compatibility = compatibility,
                Seed = options.Seed,
            };

            options.ConfigureReader?.Invoke(settings);
            settings.Compatibility = compatibility;
            return settings;
        }

        private static CompatibilityOptions GetCompatibility(SpiceDialect dialect)
        {
            switch (dialect)
            {
                case SpiceDialect.Spice3:
                    return CompatibilityOptions.None;
                case SpiceDialect.PSpice:
                    return CompatibilityOptions.PSpice;
                case SpiceDialect.LTspice:
                    return CompatibilityOptions.LTspice;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dialect), dialect, "Unknown SPICE dialect.");
            }
        }

        private static SpiceCompileOptions CloneOptions(
            SpiceCompileOptions source,
            string workingDirectory)
        {
            source = source ?? new SpiceCompileOptions();
            return new SpiceCompileOptions
            {
                Dialect = source.Dialect,
                WorkingDirectory = workingDirectory,
                ContinueAfterErrors = source.ContinueAfterErrors,
                MaximumSyntaxErrors = source.MaximumSyntaxErrors,
                RunLinter = false,
                DiagnosticPolicy = source.DiagnosticPolicy,
                ThrowOnFileAccessError = source.ThrowOnFileAccessError,
                HasTitle = true,
                IsEndRequired = true,
                IsNewlineRequired = source.IsNewlineRequired,
                EnableBusSyntax = source.EnableBusSyntax,
                ExternalFilesEncoding = source.ExternalFilesEncoding,
                CaseSensitivity = CloneCaseSensitivity(source.CaseSensitivity),
                Seed = source.Seed,
                Separator = source.Separator,
                ExpandSubcircuits = source.ExpandSubcircuits,
                ConfigureParser = source.ConfigureParser,
                ConfigureReader = source.ConfigureReader,
            };
        }

        private static SpiceNetlistCaseSensitivitySettings CloneCaseSensitivity(
            SpiceNetlistCaseSensitivitySettings source)
        {
            if (source == null)
            {
                return null;
            }

            return new SpiceNetlistCaseSensitivitySettings
            {
                IsDistributionNameCaseSensitive = source.IsDistributionNameCaseSensitive,
                IsDotStatementNameCaseSensitive = source.IsDotStatementNameCaseSensitive,
                IsEntityNamesCaseSensitive = source.IsEntityNamesCaseSensitive,
                IsExpressionNameCaseSensitive = source.IsExpressionNameCaseSensitive,
                IsFunctionNameCaseSensitive = source.IsFunctionNameCaseSensitive,
                IsModelTypeCaseSensitive = source.IsModelTypeCaseSensitive,
                IsNodeNameCaseSensitive = source.IsNodeNameCaseSensitive,
                IsParameterNameCaseSensitive = source.IsParameterNameCaseSensitive,
                IsSubcircuitNameCaseSensitive = source.IsSubcircuitNameCaseSensitive,
            };
        }

        private static SpiceSubcircuitLibraryException CreateFailure(
            string operation,
            IEnumerable<SpiceDiagnostic> diagnostics)
        {
            List<SpiceDiagnostic> diagnosticList = diagnostics.ToList();
            string details = string.Join(
                Environment.NewLine,
                diagnosticList
                    .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                    .Select(diagnostic => diagnostic.ToString()));
            string message = string.IsNullOrEmpty(details)
                ? operation
                : operation + Environment.NewLine + details;
            return new SpiceSubcircuitLibraryException(message, diagnosticList);
        }

        private static string Lines(params string[] lines)
        {
            return string.Join(Environment.NewLine, lines) + Environment.NewLine;
        }

        private SpiceSubcircuitInfo GetSubcircuit(string subcircuitName)
        {
            if (subcircuitName == null)
            {
                throw new ArgumentNullException(nameof(subcircuitName));
            }

            if (string.IsNullOrWhiteSpace(subcircuitName))
            {
                throw new ArgumentException("A subcircuit name is required.", nameof(subcircuitName));
            }

            if (!Subcircuits.TryGetValue(subcircuitName, out SpiceSubcircuitInfo info))
            {
                throw new KeyNotFoundException(
                    $"Subcircuit '{subcircuitName}' was not found in the library. "
                    + $"Available definitions: {string.Join(", ", Subcircuits.Keys)}.");
            }

            return info;
        }

        private SpiceNetlist CreateInstanceNetlist(
            string subcircuitName,
            string instanceName,
            IEnumerable<string> nodes,
            IReadOnlyDictionary<string, string> parameters)
        {
            var statements = (Statements)_template.Statements.Clone();
            var lineInfo = new SpiceLineInfo
            {
                FileName = $"<programmatic:{instanceName}>",
                LineNumber = 1,
                StartColumnIndex = 0,
                EndColumnIndex = instanceName.Length,
            };
            var instanceParameters = new ParameterCollection();
            foreach (string node in nodes)
            {
                instanceParameters.Add(new IdentifierParameter(node, lineInfo));
            }

            instanceParameters.Add(new WordParameter(subcircuitName, lineInfo));
            if (parameters != null)
            {
                foreach (KeyValuePair<string, string> parameter in parameters)
                {
                    instanceParameters.Add(new AssignmentParameter(
                        parameter.Key,
                        null,
                        new List<string> { parameter.Value },
                        false,
                        lineInfo));
                }
            }

            var component = new Component(instanceName, instanceParameters, lineInfo)
            {
                NameParameter = new WordParameter(instanceName, lineInfo),
            };
            statements.Add(component);
            return new SpiceNetlist(_template.Title, statements);
        }

        private IReadOnlyList<IEntity> AddEntities(Circuit circuit, IList<IEntity> entities)
        {
            InstalledSupportEntities installed = _installedSupportEntities.GetValue(
                circuit,
                _ => new InstalledSupportEntities(_entityNameComparer));
            var added = new List<IEntity>();

            lock (installed)
            {
                foreach (IEntity entity in entities)
                {
                    if (!circuit.TryGetEntity(entity.Name, out IEntity existing))
                    {
                        continue;
                    }

                    bool isInstalledSupport = _supportEntityNames.Contains(entity.Name)
                        && installed.Entities.TryGetValue(entity.Name, out IEntity installedEntity)
                        && ReferenceEquals(existing, installedEntity);
                    if (!isInstalledSupport)
                    {
                        throw new InvalidOperationException(
                            $"Cannot add the subcircuit instance because entity '{entity.Name}' already exists in the target circuit.");
                    }
                }

                try
                {
                    foreach (IEntity entity in entities)
                    {
                        if (circuit.Contains(entity.Name))
                        {
                            continue;
                        }

                        circuit.Add(entity);
                        added.Add(entity);
                        if (_supportEntityNames.Contains(entity.Name))
                        {
                            installed.Entities[entity.Name] = entity;
                        }
                    }
                }
                catch
                {
                    for (int index = added.Count - 1; index >= 0; index--)
                    {
                        IEntity entity = added[index];
                        if (circuit.TryGetEntity(entity.Name, out IEntity existing)
                            && ReferenceEquals(entity, existing))
                        {
                            circuit.Remove(entity.Name);
                        }

                        if (installed.Entities.TryGetValue(entity.Name, out IEntity installedEntity)
                            && ReferenceEquals(entity, installedEntity))
                        {
                            installed.Entities.Remove(entity.Name);
                        }
                    }

                    throw;
                }
            }

            return added.AsReadOnly();
        }

        private SpiceNetlistReaderSettings CreateReaderSettings()
        {
            return CreateReaderSettings(_options, _workingDirectory);
        }

        private sealed class InstalledSupportEntities
        {
            public InstalledSupportEntities(IEqualityComparer<string> comparer)
            {
                Entities = new Dictionary<string, IEntity>(comparer);
            }

            public Dictionary<string, IEntity> Entities { get; }
        }
    }
}
