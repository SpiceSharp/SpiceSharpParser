using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.FileSystem;
using SpiceSharpParser.Lexers.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Parsers.Netlist.Spice;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Processors
{
    /// <summary>
    /// Preprocess .include statements from netlist file.
    /// </summary>
    public class IncludesPreprocessor : IProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IncludesPreprocessor"/> class.
        /// </summary>
        /// <param name="fileReader">File reader</param>
        public IncludesPreprocessor(IFileReader fileReader, ISpiceTokenProvider tokenProvider, ISingleSpiceNetlistParser singleSpiceNetlistParser, Func<string> initialDirectoryPathProvider, SpiceNetlistReaderSettings readerSettings)
        {
            ReaderSettings = readerSettings;
            TokenProvider = tokenProvider;
            SingleSpiceNetlistParser = singleSpiceNetlistParser;
            FileReader = fileReader;
            InitialDirectoryPathProvider = initialDirectoryPathProvider;
        }

        /// <summary>
        /// Gets the initial directory path.
        /// </summary>
        public string InitialDirectoryPath
        {
            get
            {
                return InitialDirectoryPathProvider() ?? Directory.GetCurrentDirectory();
            }
        }

        /// <summary>
        /// Gets the file reader.
        /// </summary>
        public IFileReader FileReader { get; }

        /// <summary>
        /// Gets the token provider.
        /// </summary>
        public ISpiceTokenProvider TokenProvider { get; }

        /// <summary>
        /// Gets the SPICE netlist parser.
        /// </summary>
        public ISingleSpiceNetlistParser SingleSpiceNetlistParser { get; }

        protected Func<string> InitialDirectoryPathProvider { get; }

        protected SpiceNetlistReaderSettings ReaderSettings { get; }

        /// <summary>
        /// Reads .include statements.
        /// </summary>
        /// <param name="statements">Statements</param>
        public Statements Process(Statements statements)
        {
            return Process(statements, InitialDirectoryPath);
        }

        /// <summary>
        /// Reads .include statements.
        /// </summary>
        /// <param name="statements">Statements</param>
        /// <param name="currentDirectoryPath">Current directory path.</param>
        protected Statements Process(Statements statements, string currentDirectoryPath)
        {
            var subCircuits = statements.Where(statement => statement is SubCircuit s);
            if (subCircuits.Any())
            {
                foreach (SubCircuit subCircuit in subCircuits.ToArray())
                {
                    var subCircuitIncludes = subCircuit.Statements.Where(statement => statement is Control c && (c.Name.ToLower() == "include" || c.Name.ToLower() == "inc"));

                    foreach (Control include in subCircuitIncludes.ToArray())
                    {
                        ReadSingleInclude(subCircuit.Statements, currentDirectoryPath, include);
                    }
                }
            }

            var includes = statements.Where(statement => statement is Control c && (c.Name.ToLower() == "include" || c.Name.ToLower() == "inc"));

            if (includes.Any())
            {
                foreach (Control include in includes.ToArray())
                {
                    ReadSingleInclude(statements, currentDirectoryPath, include);
                }
            }

            return statements;
        }

        private void ReadSingleInclude(Statements statements, string currentDirectoryPath, Control include)
        {
            // get full path of .include
            string includePath = include.Parameters.GetString(0);

            includePath = ConvertPath(includePath);

            bool isAbsolutePath = Path.IsPathRooted(includePath);
            string includeFullPath = isAbsolutePath ? includePath : Path.Combine(currentDirectoryPath, includePath);

            // check if file exists
            if (!File.Exists(includeFullPath))
            {
                throw new InvalidOperationException($"Netlist include at {includeFullPath}  is not found");
            }

            // get include content
            string includeContent = FileReader.GetFileContent(includeFullPath);
            if (includeContent != null)
            {
                // tokens
                var tokens = TokenProvider.GetTokens(includeContent);
                
                // parse 
                var settings = new SingleSpiceNetlistParserSettings(ReaderSettings.CaseSettings)
                    { HasTitle = false, IsEndRequired = false, IsNewlineRequired = false };

                SingleSpiceNetlistParser.Settings = settings;
                SpiceNetlist includeModel = SingleSpiceNetlistParser.Parse(tokens);
                    

                // process includes of include netlist
                var savedDirectoryPath = InitialDirectoryPath;
                includeModel.Statements = Process(includeModel.Statements, Path.GetDirectoryName(includeFullPath));
                // repelace statement by the content of the include
                statements.Replace(include, includeModel.Statements);
            }
            else
            {
                throw new InvalidOperationException($"Netlist include at {includeFullPath} could not be loaded");
            }
        }

        private string ConvertPath(string includePath)
        {
#if NET45
            return includePath.Replace("/", "\\"); // NET45 can run on Windows
#else
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return includePath.Replace("/", "\\");
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return includePath.Replace("\\", "/");
            }

            return includePath;
#endif
        }
    }
}
