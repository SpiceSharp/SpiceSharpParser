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
    /// Processes .lib statements with 2 parameters  from netlist file.
    /// </summary>
    public class LibPreProcessor : ILibPreProcessor
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="LibPreProcessor"/> class.
        /// </summary>
        /// <param name="fileReader">File reader</param>
        public LibPreProcessor(IFileReader fileReader, INetlistModelReader netlistModelReader, IIncludesPreProcessor includesPreProcessor)
        {
            IncludesPreProcessor = includesPreProcessor;
            NetlistModelReader = netlistModelReader;
            FileReader = fileReader;
        }

        /// <summary>
        /// Gets the file reader.
        /// </summary>
        public IFileReader FileReader { get; }

        /// <summary>
        /// Gets the netlist model reader.
        /// </summary>
        public INetlistModelReader NetlistModelReader { get; }

        /// <summary>
        /// Getst the inclue preprocessor.
        /// </summary>
        public IIncludesPreProcessor IncludesPreProcessor { get; }

        /// <summary>
        /// Processes .include statements.
        /// </summary>
        /// <param name="netlistModel">Netlist model to seach for .include statements</param>
        /// <param name="currentDirectoryPath">Current working directory path</param>
        public void Process(Netlist netlistModel, string currentDirectoryPath = null)
        {
            if (currentDirectoryPath == null)
            {
                currentDirectoryPath = Directory.GetCurrentDirectory();
            }

            bool libFound = ProcessLib(netlistModel, currentDirectoryPath);

            while (libFound)
            {
                IncludesPreProcessor.Process(netlistModel, currentDirectoryPath);
                libFound = ProcessLib(netlistModel, currentDirectoryPath);
            }
        }

        private bool ProcessLib(Netlist netlistModel, string currentDirectoryPath)
        {
            bool result = false;
            var subCircuits = netlistModel.Statements.Where(statement => statement is SubCircuit s);
            if (subCircuits.Any())
            {
                foreach (SubCircuit subCircuit in subCircuits.ToArray())
                {
                    var subCircuitLibs = subCircuit.Statements.Where(statement => statement is Control c && (c.Name.ToLower() == "lib"));

                    foreach (Control include in subCircuitLibs.ToArray())
                    {
                        result = true;
                        ProcessSingleLib(subCircuit.Statements, currentDirectoryPath, include);
                    }
                }
            }

            var libs = netlistModel.Statements.Where(statement => statement is Control c && (c.Name.ToLower() == "lib"));

            if (libs.Any())
            {
                result = true;
                foreach (Control include in libs.ToArray())
                {
                    ProcessSingleLib(netlistModel.Statements, currentDirectoryPath, include);
                }
            }

            return result;
        }

        private void ProcessSingleLib(Statements statements, string currentDirectoryPath, Control lib)
        {
            // get full path of .lib
            string libPath = ConvertPath(lib.Parameters.GetString(0));
            bool isAbsolutePath = Path.IsPathRooted(libPath);
            string libFullPath = isAbsolutePath ? libPath : Path.Combine(currentDirectoryPath, libPath);

            // check if file exists
            if (!File.Exists(libFullPath))
            {
                throw new InvalidOperationException($"Netlist include at { libFullPath}  is not found");
            }

            // get lib content
            string libContent = FileReader.GetFileContent(libFullPath);
            if (libContent != null)
            {
                //1. get lib netlist model
                Netlist includeModel = NetlistModelReader.GetNetlistModel(
                    libContent,
                    new ParserSettings() { HasTitle = false, IsEndRequired = false, IsNewlineRequired = false });

                var allStatements = includeModel.Statements.ToList();

                //2. Find lib by entry
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
            else
            {
                throw new InvalidOperationException($"Netlist include at { libFullPath} could not be loaded");
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
