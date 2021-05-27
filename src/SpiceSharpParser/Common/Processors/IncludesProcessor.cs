using System;
using System.IO;
using System.Linq;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpParser.Common.FileSystem;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.Lexers.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Parsers.Netlist.Spice;

namespace SpiceSharpParser.Common.Processors
{
    /// <summary>
    /// Preprocess .include statements from netlist file.
    /// </summary>
    public class IncludesProcessor : IProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IncludesProcessor"/> class.
        /// </summary>
        /// <param name="fileReader">File reader.</param>
        /// <param name="tokenProviderPool">Token provider pool.</param>
        /// <param name="spiceNetlistParser">Parser.</param>
        /// <param name="lexerSettings">Lexer settings.</param>
        /// <param name="initialDirectoryPathProvider">Directory provider.</param>
        public IncludesProcessor(IFileReader fileReader, ISpiceTokenProviderPool tokenProviderPool, ISingleSpiceNetlistParser spiceNetlistParser, Func<string> initialDirectoryPathProvider, SpiceLexerSettings lexerSettings)
        {
            TokenProviderPool = tokenProviderPool;
            SpiceNetlistParser = spiceNetlistParser;
            FileReader = fileReader;
            InitialDirectoryPathProvider = initialDirectoryPathProvider;
            LexerSettings = lexerSettings;
        }

        /// <summary>
        /// Gets the lexer settings.
        /// </summary>
        public SpiceLexerSettings LexerSettings { get; }

        /// <summary>
        /// Gets the initial directory path.
        /// </summary>
        public string InitialDirectoryPath => InitialDirectoryPathProvider() ?? Directory.GetCurrentDirectory();

        /// <summary>
        /// Gets the file reader.
        /// </summary>
        public IFileReader FileReader { get; }

        /// <summary>
        /// Gets the token provider.
        /// </summary>
        public ISpiceTokenProviderPool TokenProviderPool { get; }

        /// <summary>
        /// Gets the SPICE netlist parser.
        /// </summary>
        public ISingleSpiceNetlistParser SpiceNetlistParser { get; }

        public ValidationEntryCollection Validation { get; set; }

        protected Func<string> InitialDirectoryPathProvider { get; }

        /// <summary>
        /// Reads .include statements.
        /// </summary>
        /// <param name="statements">Statements.</param>
        public Statements Process(Statements statements)
        {
            if (statements == null)
            {
                throw new ArgumentNullException(nameof(statements));
            }

            return Process(statements, InitialDirectoryPath);
        }

        /// <summary>
        /// Reads .include statements.
        /// </summary>
        /// <param name="statements">Statements.</param>
        /// <param name="currentDirectoryPath">Current directory path.</param>
        protected Statements Process(Statements statements, string currentDirectoryPath)
        {
            var subCircuits = statements.OfType<SubCircuit>().ToList();

            if (subCircuits.Any())
            {
                foreach (SubCircuit subCircuit in subCircuits)
                {
                    var subCircuitIncludes = subCircuit.Statements.OfType<Control>()
                        .Where(statement => statement.Name == "include" || statement.Name.ToLower() == "inc").ToList();

                    foreach (Control include in subCircuitIncludes)
                    {
                        ReadSingleInclude(subCircuit.Statements, currentDirectoryPath, include);
                    }
                }
            }

            var includes = statements.OfType<Control>().Where(statement =>
                statement.Name.ToLower() == "include" || statement.Name.ToLower() == "inc").ToList();

            if (includes.Any())
            {
                foreach (Control include in includes)
                {
                    ReadSingleInclude(statements, currentDirectoryPath, include);
                }
            }

            return statements;
        }

        private void ReadSingleInclude(Statements statements, string currentDirectoryPath, Control include)
        {
            // get full path of .include
            string includePath = include.Parameters.Get(0).Value;

            includePath = PathConverter.Convert(includePath);

            bool isAbsolutePath = Path.IsPathRooted(includePath);
            string includeFullPath = isAbsolutePath ? includePath : Path.Combine(currentDirectoryPath, includePath);

            // check if file exists
            if (!File.Exists(includeFullPath))
            {
                Validation.AddError(
                    ValidationEntrySource.Processor,
                    $"Netlist include at {includeFullPath} is not found",
                    include.LineInfo);
                return;
            }

            // get include content
            string includeContent = FileReader.ReadAll(includeFullPath);
            if (includeContent != null)
            {
                var lexerSettings = new SpiceLexerSettings(LexerSettings.IsDotStatementNameCaseSensitive)
                {
                    HasTitle = false,
                };

                var tokens = TokenProviderPool.GetSpiceTokenProvider(lexerSettings).GetTokens(includeContent);

                foreach (var token in tokens)
                {
                    token.FileName = includeFullPath;
                }

                SpiceNetlistParser.Settings = new SingleSpiceNetlistParserSettings(lexerSettings)
                {
                    IsNewlineRequired = false,
                    IsEndRequired = false,
                };

                SpiceNetlist includeModel = SpiceNetlistParser.Parse(tokens);

                // process includes of include netlist
                includeModel.Statements = Process(includeModel.Statements, Path.GetDirectoryName(includeFullPath));

                // replace statement by the content of the include
                statements.Replace(include, includeModel.Statements);
            }
            else
            {
                Validation.AddError(
                    ValidationEntrySource.Processor,
                    $"Netlist include at {includeFullPath} could not be read",
                    include.LineInfo);
            }
        }
    }
}