using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SpiceSharpParser.Model;
using SpiceSharpParser.Model.SpiceObjects;

namespace SpiceSharpParser.Preprocessors
{
    /// <summary>
    /// Processes .include statements from netlist file
    /// </summary>
    public class IncludesPreProcessor : IIncludesPreProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IncludesPreProcessor"/> class.
        /// </summary>
        /// <param name="fileReader">File reader</param>
        public IncludesPreProcessor(IFileReader fileReader, INetlistModelReader netlistModelReader)
        {
            NetlistModelReader = netlistModelReader;
            FileReader = fileReader;
        }

        /// <summary>
        /// Gets the file reader
        /// </summary>
        public IFileReader FileReader { get; }

        /// <summary>
        /// Gets the netlist model reader
        /// </summary>
        public INetlistModelReader NetlistModelReader { get; }

        /// <summary>
        /// Processes .include statements
        /// </summary>
        /// <param name="netlistModel">Netlist model to seach for .include statements</param>
        /// <param name="currentDirectoryPath">Current working directory path</param>
        public void Process(Netlist netlistModel, string currentDirectoryPath = null)
        {
            List<string> loadedFullPaths = new List<string>();
            Process(netlistModel, loadedFullPaths, currentDirectoryPath ?? Directory.GetCurrentDirectory());
        }

        /// <summary>
        /// Processes .include statements
        /// </summary>
        /// <param name="netlistModel">Netlist model to seach for .include statements</param>
        /// <param name="loadedFullPaths">List of paths of loaded netlists</param>
        /// <param name="currentDirectoryPath">Current working directory path</param>
        protected void Process(Netlist netlistModel, List<string> loadedFullPaths, string currentDirectoryPath = null)
        {
            var subCircuits = netlistModel.Statements.Where(statement => statement is SubCircuit s);
            if (subCircuits.Any())
            {
                foreach (SubCircuit subCircuit in subCircuits.ToArray())
                {
                    var subCircuitIncludes = subCircuit.Statements.Where(statement => statement is Control c && (c.Name.ToLower() == "include" || c.Name.ToLower() == "inc"));

                    foreach (Control include in subCircuitIncludes.ToArray())
                    {
                        ProcessSingleInclude(subCircuit.Statements, new List<string>(), currentDirectoryPath, include);
                    }
                }
            }

            var includes = netlistModel.Statements.Where(statement => statement is Control c && (c.Name.ToLower() == "include" || c.Name.ToLower() == "inc"));

            if (includes.Any())
            {
                foreach (Control include in includes.ToArray())
                {
                    ProcessSingleInclude(netlistModel.Statements, loadedFullPaths, currentDirectoryPath, include);
                }
            }
        }

        private void ProcessSingleInclude(Statements statements, List<string> loadedFullPaths, string currentDirectoryPath, Control include)
        {
            // get full path of .include
            string includePath = include.Parameters[0].Image.Trim('"');

            includePath = ConvertPath(includePath);

            bool isAbsolutePath = Path.IsPathRooted(includePath);
            string includeFullPath = isAbsolutePath ? includePath : Path.Combine(currentDirectoryPath, includePath);

            // check if path is not already loaded
            if (!loadedFullPaths.Contains(includeFullPath))
            {
                // check if file exists
                if (!File.Exists(includeFullPath))
                {
                    throw new InvalidOperationException($"Netlist include at { includeFullPath}  is not found");
                }

                // mark includeFullPath as loaded (before loading all includes to avoid loops)
                loadedFullPaths.Add(includeFullPath);

                // get include content
                string includeContent = FileReader.GetFileContent(includeFullPath);
                if (includeContent != null)
                {
                    // get include netlist model
                    Netlist includeModel = NetlistModelReader.GetNetlistModel(
                        includeContent,
                        new ParserSettings() { HasTitle = false, IsEndRequired = false });

                    // process includes of include netlist
                    Process(includeModel, loadedFullPaths, Path.GetDirectoryName(includeFullPath));

                    // merge include with netlist 
                    statements.Merge(includeModel.Statements);
                }
                else
                {
                    throw new InvalidOperationException($"Netlist include at { includeFullPath} could not be loaded");
                }
            }
        }

        private string ConvertPath(string includePath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return includePath.Replace("/", "\\");
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return includePath.Replace("\\", "/");
            }

            return includePath;
        }
    }
}
