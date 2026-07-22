using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SpiceSharpParser.Common.FileSystem;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.Lexers.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Parsers.Netlist.Spice;

namespace SpiceSharpParser.Common.Processors
{
    /// <summary>
    /// Preprocess .lib statements with 2 parameters from netlist file.
    /// </summary>
    public class LibProcessor : IProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LibProcessor"/> class.
        /// </summary>
        /// <param name="fileReader">File reader.</param>
        /// <param name="tokenProviderPool">Token provider.</param>
        /// <param name="spiceNetlistParser">Single spice netlist parser.</param>
        /// <param name="includesPreprocessor">Includes preprocessor.</param>
        /// <param name="initialDirectoryPathProvider">Initial directory path provider.</param>
        /// <param name="lexerSettings">Lexer settings.</param>
        public LibProcessor(
            IFileReader fileReader,
            ISpiceTokenProviderPool tokenProviderPool,
            ISingleSpiceNetlistParser spiceNetlistParser,
            IProcessor includesPreprocessor,
            Func<string> initialDirectoryPathProvider,
            SpiceLexerSettings lexerSettings)
        {
            TokenProviderPool = tokenProviderPool ?? throw new ArgumentNullException(nameof(tokenProviderPool));
            IncludesPreprocessor = includesPreprocessor ?? throw new ArgumentNullException(nameof(includesPreprocessor));
            SpiceNetlistParser = spiceNetlistParser ?? throw new ArgumentNullException(nameof(spiceNetlistParser));
            FileReader = fileReader ?? throw new ArgumentNullException(nameof(fileReader));
            InitialDirectoryPathProvider = initialDirectoryPathProvider ?? throw new ArgumentNullException(nameof(initialDirectoryPathProvider));
            LexerSettings = lexerSettings ?? throw new ArgumentNullException(nameof(lexerSettings));
        }

        public SpiceLexerSettings LexerSettings { get; }

        public ISpiceTokenProviderPool TokenProviderPool { get; }

        /// <summary>
        /// Gets the initial directory path.
        /// </summary>
        public string InitialDirectoryPath => InitialDirectoryPathProvider() ?? Directory.GetCurrentDirectory();

        /// <summary>
        /// Gets the file reader.
        /// </summary>
        public IFileReader FileReader { get; }

        /// <summary>
        /// Gets or sets an optional callback that records dependency resolution attempts.
        /// </summary>
        public Action<SpiceDependency> DependencyRecorder { get; set; }

        /// <summary>
        /// Gets the SPICE netlist parser.
        /// </summary>
        public ISingleSpiceNetlistParser SpiceNetlistParser { get; }

        /// <summary>
        /// Gets the include preprocessor.
        /// </summary>
        public IProcessor IncludesPreprocessor { get; }

        /// <summary>
        /// Gets or sets validation.
        /// </summary>
        public ValidationEntryCollection Validation { get; set; }

        internal Func<
            string,
            string,
            SpiceLexerSettings,
            SingleSpiceNetlistParserSettings,
            SpiceNetlist> SourceParser { get; set; }

        protected Func<string> InitialDirectoryPathProvider { get; }

        /// <summary>
        /// Reads .include statements.
        /// </summary>
        /// <param name="statements">Netlist model to search for .include statements.</param>
        public Statements Process(Statements statements)
        {
            if (statements == null)
            {
                throw new ArgumentNullException(nameof(statements));
            }

            return Process(statements, InitialDirectoryPath);
        }

        private static Statements GetSingleLibStatementsWithOneArgument(List<Statement> allStatements)
        {
            return ToStatements(allStatements);
        }

        private static Statements GetSingleLibStatementsWithTwoArguments(
            Control lib,
            List<Statement> allStatements,
            string libraryPath)
        {
            // Find lib by entry
            var libEntry = allStatements.SingleOrDefault(s => s is Control c && c.Name == "lib" && c.Parameters.Get(0).Value == lib.Parameters.Get(1).Value);
            if (libEntry != null)
            {
                // look for .endl
                int position = allStatements.IndexOf(libEntry);
                int libPosition = position;

                for (; position < allStatements.Count && !(allStatements[position] is Control c && c.Name.Equals("endl", StringComparison.OrdinalIgnoreCase)); position++)
                {
                    // iterate to find position
                }

                if (position == allStatements.Count)
                {
                    throw new SpiceSharpParserException(
                        "No .ENDL found",
                        new SpiceLineInfo { FileName = libraryPath });
                }
                else
                {
                    var libStatements = allStatements.Skip(libPosition + 1).Take(position - libPosition - 1);
                    return ToStatements(libStatements);
                }
            }

            return null;
        }

        private static Statements ToStatements(IEnumerable<Statement> statements)
        {
            var result = new Statements();
            foreach (var statement in statements)
            {
                result.Add(statement);
            }

            return result;
        }

        /// <summary>
        /// Reads .include statements.
        /// </summary>
        /// <param name="statements">Netlist model to search for .include statements.</param>
        /// <param name="currentDirectoryPath">Current directory path.</param>
        private Statements Process(Statements statements, string currentDirectoryPath)
        {
            bool libFound = ReadLibs(statements, currentDirectoryPath);

            while (libFound)
            {
                ProcessIncludes(statements, currentDirectoryPath);
                libFound = ReadLibs(statements, currentDirectoryPath);
            }

            return statements;
        }

        private void ProcessIncludes(Statements statements, string currentDirectoryPath)
        {
            if (IncludesPreprocessor is IncludesProcessor includesProcessor)
            {
                includesProcessor.Process(statements, currentDirectoryPath);
            }
            else
            {
                IncludesPreprocessor.Process(statements);
            }
        }

        private bool ReadLibs(Statements statements, string currentDirectoryPath)
        {
            bool result = false;
            var subCircuits = statements.Where(statement => statement is SubCircuit).Cast<SubCircuit>().ToList();
            if (subCircuits.Any())
            {
                foreach (SubCircuit subCircuit in subCircuits)
                {
                    var subCircuitLibs = subCircuit.Statements.Where(statement => statement is Control c && (c.Name.ToLower() == "lib")).Cast<Control>().ToList();

                    foreach (Control include in subCircuitLibs)
                    {
                        result = true;
                        ReadSingleLib(subCircuit.Statements, currentDirectoryPath, include);
                    }
                }
            }

            var libs = statements.Where(statement => statement is Control c && (c.Name.ToLower() == "lib")).Cast<Control>().ToList();

            if (libs.Any())
            {
                result = true;
                foreach (Control include in libs)
                {
                    ReadSingleLib(statements, currentDirectoryPath, include);
                }
            }

            return result;
        }

        private void ReadSingleLib(Statements statements, string currentDirectoryPath, Control lib)
        {
            // get full path of .lib
            string requestedPath = lib.Parameters.Get(0).Value;
            string libPath = PathConverter.Convert(requestedPath);
            bool isAbsolutePath = Path.IsPathRooted(libPath);
            string libFullPath = isAbsolutePath ? libPath : Path.Combine(currentDirectoryPath, libPath);

            // check if file exists
            if (!File.Exists(libFullPath))
            {
                RecordDependency(lib, requestedPath, libFullPath, SpiceDependencyStatus.NotFound);
                Validation.AddError(
                    ValidationEntrySource.Processor,
                    $"Netlist include at {libFullPath} could not be found",
                    lib.LineInfo);
                statements.Replace(lib, Enumerable.Empty<Statement>());
                return;
            }

            // get lib content
            string libContent;
            try
            {
                libContent = FileReader.ReadAll(libFullPath);
            }
            catch (IOException exception)
            {
                RecordUnreadableDependency(lib, requestedPath, libFullPath, exception);
                statements.Replace(lib, Enumerable.Empty<Statement>());
                return;
            }
            catch (UnauthorizedAccessException exception)
            {
                RecordUnreadableDependency(lib, requestedPath, libFullPath, exception);
                statements.Replace(lib, Enumerable.Empty<Statement>());
                return;
            }
            catch (System.Security.SecurityException exception)
            {
                RecordUnreadableDependency(lib, requestedPath, libFullPath, exception);
                statements.Replace(lib, Enumerable.Empty<Statement>());
                return;
            }

            if (libContent != null)
            {
                RecordDependency(lib, requestedPath, libFullPath, SpiceDependencyStatus.Resolved);
                var lexerSettings = new SpiceLexerSettings(LexerSettings.IsDotStatementNameCaseSensitive)
                {
                    HasTitle = false,
                    Compatibility = LexerSettings.Compatibility,
                };
                var parserSettings = new SingleSpiceNetlistParserSettings(lexerSettings)
                {
                    IsNewlineRequired = false,
                    IsEndRequired = false,
                };

                SpiceNetlist includeModel;
                if (SourceParser != null)
                {
                    includeModel = SourceParser(libContent, libFullPath, lexerSettings, parserSettings);
                    if (includeModel == null)
                    {
                        statements.Replace(lib, Enumerable.Empty<Statement>());
                        return;
                    }
                }
                else
                {
                    SpiceToken[] tokens;
                    try
                    {
                        tokens = TokenProviderPool.GetSpiceTokenProvider(lexerSettings).GetTokens(libContent);
                    }
                    catch (global::SpiceSharpParser.Lexers.LexerException exception)
                    {
                        if (exception.LineInfo == null)
                        {
                            throw new global::SpiceSharpParser.Lexers.LexerException(
                                exception.Message,
                                exception,
                                new SpiceLineInfo { FileName = libFullPath });
                        }

                        SetExceptionSource(exception, libFullPath);
                        throw;
                    }

                    foreach (var token in tokens)
                    {
                        token.FileName = libFullPath;
                    }

                    SpiceNetlistParser.Settings = parserSettings;

                    try
                    {
                        includeModel = SpiceNetlistParser.Parse(tokens);
                    }
                    catch (SpiceSharpParserException exception)
                    {
                        SetExceptionSource(exception, libFullPath);
                        throw;
                    }
                }

                var allStatements = includeModel.Statements.ToList();
                Statements libStatements = null;

                try
                {
                    if (lib.Parameters.Count == 2)
                    {
                        libStatements = GetSingleLibStatementsWithTwoArguments(lib, allStatements, libFullPath);
                    }
                    else if (lib.Parameters.Count == 1)
                    {
                        libStatements = GetSingleLibStatementsWithOneArgument(allStatements);
                    }
                }
                catch (SpiceSharpParserException exception) when (SourceParser != null)
                {
                    SetExceptionSource(exception, libFullPath);
                    Validation.AddError(
                        ValidationEntrySource.Processor,
                        exception.Message,
                        exception.LineInfo,
                        exception);
                    statements.Replace(lib, Enumerable.Empty<Statement>());
                    return;
                }

                if (libStatements != null)
                {
                    var libDirectory = Path.GetDirectoryName(libFullPath);
                    ProcessIncludes(libStatements, libDirectory);
                    Process(libStatements, libDirectory);
                    statements.Replace(lib, libStatements);
                }
                else
                {
                    string section = lib.Parameters.Count > 1 ? lib.Parameters.Get(1).Value : null;
                    Validation.AddError(
                        ValidationEntrySource.Processor,
                        $"Library section '{section}' was not found in {libFullPath}",
                        lib.LineInfo);
                    statements.Replace(lib, Enumerable.Empty<Statement>());
                }
            }
            else
            {
                RecordDependency(lib, requestedPath, libFullPath, SpiceDependencyStatus.Unreadable);
                Validation.AddError(
                    ValidationEntrySource.Processor,
                    $"Netlist include at {libFullPath} could not be read",
                    lib.LineInfo);
                statements.Replace(lib, Enumerable.Empty<Statement>());
            }
        }

        private void RecordDependency(
            Control library,
            string requestedPath,
            string resolvedPath,
            SpiceDependencyStatus status)
        {
            string section = library.Parameters.Count > 1 ? library.Parameters.Get(1).Value : null;
            DependencyRecorder?.Invoke(new SpiceDependency(
                SpiceDependencyKind.Library,
                requestedPath,
                resolvedPath,
                status,
                global::SpiceSharpParser.Diagnostics.SourceSpan.FromLineInfo(library.LineInfo),
                section));
        }

        private void SetExceptionSource(SpiceSharpParserException exception, string sourcePath)
        {
            if (exception.LineInfo != null && string.IsNullOrEmpty(exception.LineInfo.FileName))
            {
                exception.LineInfo.FileName = sourcePath;
            }
        }

        private void RecordUnreadableDependency(
            Control library,
            string requestedPath,
            string resolvedPath,
            Exception exception)
        {
            RecordDependency(library, requestedPath, resolvedPath, SpiceDependencyStatus.Unreadable);
            Validation.AddError(
                ValidationEntrySource.Processor,
                $"Netlist library at {resolvedPath} could not be read: {exception.Message}",
                library.LineInfo,
                exception);
        }
    }
}
