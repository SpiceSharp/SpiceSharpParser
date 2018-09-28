using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SpiceSharpParser.Common;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Processors
{
    /// <summary>
    /// Preprocess .lib statements with 2 parameters from netlist file.
    /// </summary>
    public class LibPreprocessor : IProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LibPreprocessor"/> class.
        /// </summary>
        /// <param name="fileReader">File reader</param>
        public LibPreprocessor(IFileReader fileReader, ISpiceNetlistParser spiceNetlistParser, IProcessor includesPreReader, Func<string> initialDirectoryPathProvider, SpiceNetlistReaderSettings readerSettings)
        {
            ReaderSettings = readerSettings;
            IncludesPreprocessor = includesPreReader;
            SpiceNetlistParser = spiceNetlistParser;
            FileReader = fileReader;
            InitialDirectoryPathProvider = initialDirectoryPathProvider;
        }

        public SpiceNetlistReaderSettings ReaderSettings { get; }

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

        public string IntialDirectoryPath { get; }

        /// <summary>
        /// Gets the file reader.
        /// </summary>
        public IFileReader FileReader { get; }

        /// <summary>
        /// Gets the SPICE netlist parser.
        /// </summary>
        public ISpiceNetlistParser SpiceNetlistParser { get; }

        /// <summary>
        /// Getst the include preprocessor.
        /// </summary>
        public IProcessor IncludesPreprocessor { get; }

        protected Func<string> InitialDirectoryPathProvider { get; }

        /// <summary>
        /// Reads .include statements.
        /// </summary>
        /// <param name="statements">Netlist model to seach for .include statements</param>
        public Statements Process(Statements statements)
        {
            return Process(statements, InitialDirectoryPath);
        }

        /// <summary>
        /// Reads .include statements.
        /// </summary>
        /// <param name="statements">Netlist model to seach for .include statements</param>
        public Statements Process(Statements statements, string currentDirectoryPath)
        {
            bool libFound = ReadLibs(statements, currentDirectoryPath);

            while (libFound)
            {
                IncludesPreprocessor.Process(statements);
                libFound = ReadLibs(statements, currentDirectoryPath);
            }

            return statements;
        }

        private bool ReadLibs(Statements statements, string currentDirectoryPath)
        {
            bool result = false;
            var subCircuits = statements.Where(statement => statement is SubCircuit s);
            if (subCircuits.Any())
            {
                foreach (SubCircuit subCircuit in subCircuits.ToArray())
                {
                    var subCircuitLibs = subCircuit.Statements.Where(statement => statement is Control c && (c.Name.ToLower() == "lib"));

                    foreach (Control include in subCircuitLibs.ToArray())
                    {
                        result = true;
                        ReadSingleLib(subCircuit.Statements, currentDirectoryPath, include);
                    }
                }
            }

            var libs = statements.Where(statement => statement is Control c && (c.Name.ToLower() == "lib"));

            if (libs.Any())
            {
                result = true;
                foreach (Control include in libs.ToArray())
                {
                    ReadSingleLib(statements, currentDirectoryPath, include);
                }
            }

            return result;
        }

        private void ReadSingleLib(Statements statements, string currentDirectoryPath, Control lib)
        {
            // get full path of .lib
            string libPath = ConvertPath(lib.Parameters.GetString(0));
            bool isAbsolutePath = Path.IsPathRooted(libPath);
            string libFullPath = isAbsolutePath ? libPath : Path.Combine(currentDirectoryPath, libPath);

            // check if file exists
            if (!File.Exists(libFullPath))
            {
                throw new InvalidOperationException($"Netlist include at {libFullPath}  is not found");
            }

            // get lib content
            string libContent = FileReader.GetFileContent(libFullPath);
            if (libContent != null)
            {
                // 1. get lib netlist model
                SpiceNetlist includeModel = SpiceNetlistParser.Parse(
                    libContent,
                    new SpiceNetlistParserSettings(ReaderSettings.CaseSettings) { HasTitle = false, IsEndRequired = false, IsNewlineRequired = false });

                var allStatements = includeModel.Statements.ToList();

                if (lib.Parameters.Count == 2)
                {
                    ReadSingleLibWithTwoArguments(statements, lib, allStatements);
                }
                else if (lib.Parameters.Count == 1)
                {
                    ReadSingleLibWithOneArgument(statements, lib, allStatements);
                }
            }
            else
            {
                throw new InvalidOperationException($"Netlist include at {libFullPath} could not be loaded");
            }
        }

        private static void ReadSingleLibWithOneArgument(Statements statements, Control lib, List<Statement> allStatements)
        {
            var libStatements = allStatements;
            statements.Replace(lib, libStatements);
        }

        private static void ReadSingleLibWithTwoArguments(Statements statements, Control lib, List<Statement> allStatements)
        {
            // Find lib by entry
            var libEntry = allStatements.SingleOrDefault(s => s is Control c && c.Name == "lib" && c.Parameters.GetString(0) == lib.Parameters.GetString(1));
            if (libEntry != null)
            {
                // look for .endl
                int position = allStatements.IndexOf(libEntry);
                int libPosition = position;

                for (; !(allStatements[position] is Control c && c.Name.ToLower() == "endl") && position < allStatements.Count; position++)
                {
                }

                if (position == allStatements.Count)
                {
                    throw new Exception("No .endl found");
                }
                else
                {
                    var libStatements = allStatements.Skip(libPosition + 1).Take(position - libPosition);
                    statements.Replace(lib, libStatements);
                }
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
