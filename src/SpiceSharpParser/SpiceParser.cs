﻿using System;
using System.Collections.Generic;
using SpiceSharpParser.Common.FileSystem;
using SpiceSharpParser.Common.Mathematics.Probability;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.Lexers;
using SpiceSharpParser.Lexers.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Processors;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.Parsers.Netlist;
using SpiceSharpParser.Parsers.Netlist.Spice;

namespace SpiceSharpParser
{
    /// <summary>
    /// The SPICE netlist parser.
    /// </summary>
    public class SpiceParser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceParser"/> class.
        /// </summary>
        public SpiceParser()
            : this(new SpiceParserSettings())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceParser"/> class.
        /// </summary>
        /// <param name="settings">SPICE parser settings.</param>
        public SpiceParser(SpiceParserSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            SingleNetlistParser = new SingleSpiceNetlistParser(Settings.Parsing);

            TokenProviderPool = new SpiceTokenProviderPool();
            var includesPreprocessor = new IncludesPreprocessor(
                new FileReader(() => Settings.ExternalFilesEncoding),
                TokenProviderPool,
                SingleNetlistParser,
                () => Settings.WorkingDirectory,
                Settings.Lexing);

            var libPreprocessor = new LibPreprocessor(
                new FileReader(() => Settings.ExternalFilesEncoding),
                TokenProviderPool,
                SingleNetlistParser,
                includesPreprocessor,
                () => Settings.WorkingDirectory,
                Settings.Lexing);

            var appendModelPreprocessor = new AppendModelPreprocessor();
            var akoModelPreprocessor = new AkoModelPreprocessor();
            var sweepsPreprocessor = new SweepsPreprocessor();
            var ifPostprocessor = new IfPreprocessor();
            var macroPreprocessor = new MacroPreprocessor();

            Preprocessors.AddRange(new IProcessor[] { includesPreprocessor, libPreprocessor, macroPreprocessor, appendModelPreprocessor, akoModelPreprocessor, sweepsPreprocessor, ifPostprocessor });
        }

        public ISpiceTokenProviderPool TokenProviderPool { get; set; }

        /// <summary>
        /// Gets the parser parserSettings.
        /// </summary>
        public SpiceParserSettings Settings { get; }

        /// <summary>
        /// Gets the preprocessors.
        /// </summary>
        public List<IProcessor> Preprocessors { get; } = new List<IProcessor>();

        /// <summary>
        /// Gets the SPICE netlist parser.
        /// </summary>
        protected ISingleSpiceNetlistParser SingleNetlistParser { get; }

        /// <summary>
        /// Parses the netlist.
        /// </summary>
        /// <param name="spiceNetlist">Netlist to parse.</param>
        /// <returns>
        /// A parsing result.
        /// </returns>
        public SpiceParserResult ParseNetlist(string spiceNetlist)
        {
            if (spiceNetlist == null)
            {
                throw new ArgumentNullException(nameof(spiceNetlist));
            }

            var result = new SpiceParserResult { ValidationResult = new ValidationEntryCollection() };

            // Get tokens
            try
            {
                var tokens = TokenProviderPool.GetSpiceTokenProvider(Settings.Lexing).GetTokens(spiceNetlist);

                SpiceNetlist originalNetlistModel = SingleNetlistParser.Parse(tokens);

                result.InputModel = originalNetlistModel;
                result.FinalModel = GetFinalModel(originalNetlistModel, result.ValidationResult);
            }
            catch (LexerException e)
            {
                result.ValidationResult.Add(
                    new ValidationEntry(
                        ValidationEntrySource.Lexer,
                        ValidationEntryLevel.Error,
                        e.ToString(),
                        null));
            }
            catch (ParseException e)
            {
                result.ValidationResult.Add(
                    new ValidationEntry(
                        ValidationEntrySource.Parser,
                        ValidationEntryLevel.Error,
                        e.ToString(),
                        null));
            }

            return result;
        }

        private SpiceNetlist GetFinalModel(SpiceNetlist originalNetlistModel, ValidationEntryCollection validationResult)
        {
            SpiceNetlist finalModel = (SpiceNetlist)originalNetlistModel.Clone();
            var preprocessorContext = GetEvaluationContext();

            foreach (var preprocessor in Preprocessors)
            {
                preprocessor.Validation = validationResult;

                if (preprocessor is IEvaluatorConsumer consumer)
                {
                    consumer.EvaluationContext = preprocessorContext;
                    consumer.CaseSettings = Settings?.CaseSensitivity;
                }

                finalModel.Statements = preprocessor.Process(finalModel.Statements);
            }

            return finalModel;
        }

        private EvaluationContext GetEvaluationContext()
        {
            var nodeNameGenerator = new MainCircuitNodeNameGenerator(
                new[] { "0" },
                Settings.CaseSensitivity.IsEntityNamesCaseSensitive,
                ".");

            var objectNameGenerator = new ObjectNameGenerator(string.Empty, ".");
            INameGenerator nameGenerator = new NameGenerator(nodeNameGenerator, objectNameGenerator);
            var expressionParserFactory = new ExpressionParserFactory(Settings.CaseSensitivity);

            EvaluationContext preprocessorContext = new SpiceEvaluationContext(
                string.Empty,
                Settings.CaseSensitivity,
                new Randomizer(
                    Settings.CaseSensitivity.IsDistributionNameCaseSensitive,
                    seed: 0),
                expressionParserFactory,
                new ExpressionFeaturesReader(expressionParserFactory),
                new ExpressionValueProvider(expressionParserFactory),
                nameGenerator);
            return preprocessorContext;
        }
    }
}