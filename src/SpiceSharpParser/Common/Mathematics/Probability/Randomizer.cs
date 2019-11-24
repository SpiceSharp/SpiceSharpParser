using SpiceSharpParser.Common.Mathematics.Probability.Pdfs;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.Common.Mathematics.Probability
{
    /// <summary>
    /// Random numbers facade.
    /// </summary>
    public class Randomizer : IRandomizer
    {
        /// <summary>
        /// Default number of CDF points.
        /// </summary>
        private static readonly int DefaultCdfPoints = 1000;

        /// <summary>
        /// Default limit of normal distribution.
        /// </summary>
        private static readonly int DefaultNormalLimit = 3;

        private readonly bool _isDistributionNameCaseSensitive;
        private readonly Dictionary<string, Func<Pdf>> _pdfDictionary = null;
        private readonly Dictionary<string, Pdf> _pdfInstancesDictionary = null;
        private readonly Dictionary<string, Cdf> _cdfDictionary = null;
        private readonly Dictionary<string, CustomRandomNumberProviderFactory> _customRandomNumberProviderFactories = null;
        private readonly DefaultRandomNumberProviderFactory _defaultRandomNumberProviderFactory = new DefaultRandomNumberProviderFactory();

        /// <summary>
        /// Initializes a new instance of the <see cref="Randomizer"/> class.
        /// </summary>
        /// <param name="isDistributionNameCaseSensitive">Is distribution name case-sensitive.</param>
        /// <param name="cdfPoints">Number of cdf points.</param>
        /// <param name="normalLimit">Normal limit.</param>
        /// <param name="seed">Seed.</param>
        public Randomizer(
            bool isDistributionNameCaseSensitive = false,
            int? cdfPoints = null,
            double? normalLimit = null,
            int? seed = null,
            Dictionary<string, Func<Pdf>> pdfDictionary = null,
            Dictionary<string, Pdf> pdfInstances = null,
            Dictionary<string, Cdf> cdfDictionary = null,
            Dictionary<string, CustomRandomNumberProviderFactory> customRandomNumberProviderFactories = null)
        {
            _isDistributionNameCaseSensitive = isDistributionNameCaseSensitive;
            _pdfDictionary = pdfDictionary ?? new Dictionary<string, Func<Pdf>>(StringComparerProvider.Get(isDistributionNameCaseSensitive));
            _pdfInstancesDictionary = pdfInstances ?? new Dictionary<string, Pdf>(StringComparerProvider.Get(isDistributionNameCaseSensitive));
            _cdfDictionary = cdfDictionary ?? new Dictionary<string, Cdf>(StringComparerProvider.Get(isDistributionNameCaseSensitive));
            _customRandomNumberProviderFactories = customRandomNumberProviderFactories ?? new Dictionary<string, CustomRandomNumberProviderFactory>(StringComparerProvider.Get(isDistributionNameCaseSensitive));

            CurrentPdfName = null;
            CdfPoints = cdfPoints ?? DefaultCdfPoints;
            NormalLimit = normalLimit ?? DefaultNormalLimit;
            Seed = seed;
            RegisterDefaultPdfs();
        }

        /// <summary>
        /// Gets or sets normal limit.
        /// </summary>
        public double NormalLimit { get; set; }

        /// <summary>
        /// Gets or sets current pdf name.
        /// </summary>
        public string CurrentPdfName { get; set; }

        /// <summary>
        /// Gets or sets number of CDF points.
        /// </summary>
        public int CdfPoints { get; set; }

        public int? Seed { get; set; }

        /// <summary>
        /// Registers a Pdf in the randomizer.
        /// </summary>
        /// <param name="name">Name of Pdf.</param>
        /// <param name="pdf">Pdf.</param>
        public void RegisterPdf(string name, Func<Pdf> pdf)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            _pdfDictionary[name] = pdf ?? throw new ArgumentNullException(nameof(pdf));
        }

        /// <summary>
        /// Gets a random double provider for a given seed and pdf.
        /// </summary>
        /// <param name="pdfName">Name of PDF.</param>
        /// <returns>
        /// A random double provider.
        /// </returns>
        public IRandomDoubleProvider GetRandomDoubleProvider(string pdfName = null)
        {
            return GetRandomProvider(pdfName);
        }

        /// <summary>
        /// Gets a random integer provider for a given seed and pdf.
        /// </summary>
        /// <param name="pdfName">Name of PDF.</param>
        /// <returns>
        /// A random integer provider.
        /// </returns>
        public IRandomIntegerProvider GetRandomIntegerProvider(string pdfName = null)
        {
            return GetRandomProvider(pdfName);
        }

        public IRandomizer Clone()
        {
            var randomizer = new Randomizer(
                _isDistributionNameCaseSensitive,
                CdfPoints,
                NormalLimit,
                Seed,
                _pdfDictionary,
                _pdfInstancesDictionary,
                _cdfDictionary);

            return randomizer;
        }

        /// <summary>
        /// Gets a random number provider for a given seed and pdf.
        /// </summary>
        /// <param name="pdfName">Name of PDF.</param>
        /// <returns>
        /// A random number provider.
        /// </returns>
        public IRandomNumberProvider GetRandomProvider(string pdfName = null)
        {
            string workingPdfName = null;

            if (pdfName == null)
            {
                if (CurrentPdfName == null)
                {
                    return _defaultRandomNumberProviderFactory.GetRandom(Seed);
                }
                else
                {
                    workingPdfName = CurrentPdfName;
                }
            }
            else
            {
                workingPdfName = pdfName;
            }

            if (_pdfDictionary.ContainsKey(workingPdfName))
            {
                if (!_pdfInstancesDictionary.ContainsKey(workingPdfName))
                {
                    _pdfInstancesDictionary[workingPdfName] = _pdfDictionary[workingPdfName]();
                }

                if (!_cdfDictionary.ContainsKey(workingPdfName))
                {
                    _cdfDictionary[workingPdfName] = new Cdf(_pdfInstancesDictionary[workingPdfName], CdfPoints);
                }

                if (!_customRandomNumberProviderFactories.ContainsKey(workingPdfName))
                {
                    _customRandomNumberProviderFactories[workingPdfName] = new CustomRandomNumberProviderFactory(_cdfDictionary[workingPdfName]);
                }

                return _customRandomNumberProviderFactories[workingPdfName].GetRandom(Seed);
            }
            else
            {
                throw new ArgumentException("Unknown pdf", nameof(workingPdfName));
            }
        }

        private void RegisterDefaultPdfs()
        {
            RegisterPdf("uniform", () => new UniformPdf());
            RegisterPdf("gauss", () => new NormalPdf(CdfPoints, NormalLimit));
        }
    }
}