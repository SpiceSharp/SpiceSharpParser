﻿using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice
{
    public class SpiceNetlistReaderSettings
    {
        /// <summary>
        /// Working directory provider.
        /// </summary>
        private readonly Func<string> _workingDirectoryProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceNetlistReaderSettings"/> class.
        /// </summary>
        /// <param name="caseSensitivitySettings">
        /// Case sensitivity settings.
        /// </param>
        /// <param name="workingDirectoryProvider">
        /// Working directory provider.
        /// </param>
        /// <param name="separator">
        /// Separator for node and object names.
        /// </param>
        /// <param name="expandSubcircuits">
        /// Expand subcircuits.
        /// </param>
        public SpiceNetlistReaderSettings(
            ISpiceNetlistCaseSensitivitySettings caseSensitivitySettings,
            Func<string> workingDirectoryProvider,
            string separator = ".",
            bool expandSubcircuits = true)
        {
            EvaluatorMode = SpiceExpressionMode.Spice3f5;
            Mappings = new SpiceObjectMappings();
            Orderer = new SpiceStatementsOrderer();

            CaseSensitivity = caseSensitivitySettings ?? throw new ArgumentNullException(nameof(caseSensitivitySettings));
            _workingDirectoryProvider = workingDirectoryProvider ?? throw new ArgumentNullException(nameof(workingDirectoryProvider));
            Separator = separator;
            ExpandSubcircuits = expandSubcircuits;
        }

        /// <summary>
        /// Gets or sets the evaluator mode.
        /// </summary>
        public SpiceExpressionMode EvaluatorMode { get; set; }

        /// <summary>
        /// Gets or sets the evaluator random seed.
        /// </summary>
        public int? Seed { get; set; }

        /// <summary>
        /// Gets or sets the object mappings.
        /// </summary>
        public ISpiceObjectMappings Mappings { get; set; }

        /// <summary>
        /// Gets or sets the statements orderer.
        /// </summary>
        public ISpiceStatementsOrderer Orderer { get; set; }

        /// <summary>
        /// Gets the case-sensitivity settings.
        /// </summary>
        public ISpiceNetlistCaseSensitivitySettings CaseSensitivity { get; }
        
        /// <summary>
        /// Gets or sets the separator for object and node names.
        /// </summary>
        public string Separator { get; set; }

        /// <summary>
        /// Gets a value indicating whether subcircuts should be expaned.
        /// </summary>
        public bool ExpandSubcircuits { get; set; }

        /// <summary>
        /// Gets working directory.
        /// </summary>
        public string WorkingDirectory => _workingDirectoryProvider();
    }
}