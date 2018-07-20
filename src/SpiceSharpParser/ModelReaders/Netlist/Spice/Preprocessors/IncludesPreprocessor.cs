using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SpiceSharpParser.Common;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Preprocessors
{
    /// <summary>
    /// Preprocess .include statements from netlist file.
    /// </summary>
    public class IncludesPreprocessor : IIncludesPreprocessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IncludesPreprocessor"/> class.
        /// </summary>
        /// <param name="fileReader">File reader</param>
        public IncludesPreprocessor(IFileReader fileReader, ISpiceNetlistParser spiceNetlistParser)
        {
            SpiceNetlistParser = spiceNetlistParser;
            FileReader = fileReader;
        }

        /// <summary>
        /// Gets the file reader.
        /// </summary>
        public IFileReader FileReader { get; }

        /// <summary>
        /// Gets the spice netlist parser.
        /// </summary>
        public ISpiceNetlistParser SpiceNetlistParser { get; }

        /// <summary>
        /// Reads .include statements.
        /// </summary>
        /// <param name="netlistModel">Netlist model to seach for .include statements</param>
        /// <param name="currentDirectoryPath">Current working directory path</param>
        public void Preprocess(SpiceNetlist netlistModel, string currentDirectoryPath = null)
        {
            if (currentDirectoryPath == null)
            {
                currentDirectoryPath = Directory.GetCurrentDirectory();
            }

            var subCircuits = netlistModel.Statements.Where(statement => statement is SubCircuit s);
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

            var includes = netlistModel.Statements.Where(statement => statement is Control c && (c.Name.ToLower() == "include" || c.Name.ToLower() == "inc"));

            if (includes.Any())
            {
                foreach (Control include in includes.ToArray())
                {
                    ReadSingleInclude(netlistModel.Statements, currentDirectoryPath, include);
                }
            }
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
                throw new InvalidOperationException($"Netlist include at { includeFullPath}  is not found");
            }

            // get include content
            string includeContent = FileReader.GetFileContent(includeFullPath);
            if (includeContent != null)
            {
                // parse include 
                SpiceNetlist includeModel = SpiceNetlistParser.Parse(
                    includeContent,
                    new SpiceNetlistParserSettings() { HasTitle = false, IsEndRequired = false, IsNewlineRequired = false });

                // process includes of include netlist
                Preprocess(includeModel, Path.GetDirectoryName(includeFullPath));

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
