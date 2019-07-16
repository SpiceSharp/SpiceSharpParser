using System;
using System.Collections.Generic;
using SpiceSharpParser.Common.Mathematics.Probability.Pdfs;

namespace SpiceSharpParser.Common.Mathematics.Probability
{
    /// <summary>
    /// Random numbers facade.
    /// </summary>
    public class Randomizer : IRandomizer
    {
        private readonly Dictionary<string, Func<Pdf>> _pdfDictionary = null;
        private readonly Dictionary<string, Pdf> _pdfInstancesDictionary = null;
        private readonly Dictionary<string, Cdf> _cdfDictionary = null;
        private readonly Dictionary<string, CustomRandomNumberProviderFactory> _customRandomNumberProviderFactories = null;
        private readonly DefaultRandomNumberProviderFactory _defaultRandomNumberProviderFactory = new DefaultRandomNumberProviderFactory();
        private double _normalLimit;
        private int _cdfPoints;

        /// <summary>
        /// Default number of CDF points.
        /// </summary>
        public const int DefaultCdfPoints = 1000;

        /// <summary>
        /// Default limit of normal distribution.
        /// </summary>
        public const int DefaultNormalLimit = 3;

        /// <summary>
        /// Initializes a new instance of the <see cref="Randomizer"/> class.
        /// </summary>
        /// <param name="isDistributionNameCaseSensitive">Is distribution name case-sensitive</param>
        /// <param name="cdfPoints">Number of cdf points.</param>
        /// <param name="normalLimit">Normal limit.</param>
        public Randomizer(bool isDistributionNameCaseSensitive = false, int? cdfPoints = null, double? normalLimit = null)
        {
            _pdfDictionary = new Dictionary<string, Func<Pdf>>(StringComparerProvider.Get(isDistributionNameCaseSensitive));
            _pdfInstancesDictionary = new Dictionary<string, Pdf>(StringComparerProvider.Get(isDistributionNameCaseSensitive));
            _cdfDictionary = new Dictionary<string, Cdf>(StringComparerProvider.Get(isDistributionNameCaseSensitive));
            _customRandomNumberProviderFactories = new Dictionary<string, CustomRandomNumberProviderFactory>(StringComparerProvider.Get(isDistributionNameCaseSensitive));

            CurrentPdfName = null;
            CdfPoints = cdfPoints ?? DefaultCdfPoints;
            NormalLimit = normalLimit ?? DefaultNormalLimit;

            RegisterDefaultPdfs();
        }

        /// <summary>
        /// Gets or sets normal limit.
        /// </summary>
        public double NormalLimit
        {
            get => _normalLimit;
            set
            {
                _normalLimit = value;
            }
        }

        /// <summary>
        /// Gets or sets current pdf name.
        /// </summary>
        public string CurrentPdfName { get; set; }
        
        /// <summary>
        /// Gets or sets number of CDF points.
        /// </summary>
        public int CdfPoints
        { 
            get => _cdfPoints;
            set
            {
                _cdfPoints = value;
            }
        }

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
        /// <param name="seed">Seed.</param>
        /// <param name="pdfName">Name of PDF.</param>
        /// <returns>
        /// A random double provider.
        /// </returns>
        public IRandomDoubleProvider GetRandomDoubleProvider(int? seed, string pdfName = null)
        {
            return GetRandomProvider(seed, pdfName);
        }

        /// <summary>
        /// Gets a random integer provider for a given seed and pdf.
        /// </summary>
        /// <param name="seed">Seed.</param>
        /// <param name="pdfName">Name of PDF.</param>
        /// <returns>
        /// A random integer provider.
        /// </returns>
        public IRandomIntegerProvider GetRandomIntegerProvider(int? seed, string pdfName = null)
        {
            return GetRandomProvider(seed, pdfName);
        }

        /// <summary>
        /// Gets a random number provider for a given seed and pdf.
        /// </summary>
        /// <param name="seed">Seed.</param>
        /// <param name="pdfName">Name of PDF.</param>
        /// <returns>
        /// A random number provider.
        /// </returns>
        public IRandomNumberProvider GetRandomProvider(int? seed, string pdfName = null)
        {
            if (pdfName == null)
            {
                if (CurrentPdfName == null)
                {
                    return _defaultRandomNumberProviderFactory.GetRandom(seed);
                }
                else
                {
                    pdfName = CurrentPdfName;
                }
            }

            if (_pdfDictionary.ContainsKey(pdfName))
            {

                if (!_pdfInstancesDictionary.ContainsKey(pdfName))
                {
                    _pdfInstancesDictionary[pdfName] = _pdfDictionary[pdfName]();
                }

                if (!_cdfDictionary.ContainsKey(pdfName))
                {
                    _cdfDictionary[pdfName] = new Cdf(_pdfInstancesDictionary[pdfName], CdfPoints);
                }

                if (!_customRandomNumberProviderFactories.ContainsKey(pdfName))
                {
                    _customRandomNumberProviderFactories[pdfName] = new CustomRandomNumberProviderFactory(_cdfDictionary[pdfName]);
                }

                return _customRandomNumberProviderFactories[pdfName].GetRandom(seed);
            }
            else
            {
                throw new ArgumentException("Unknown pdf", nameof(pdfName));
            }
        }

        private void RegisterDefaultPdfs()
        {
            RegisterPdf("uniform", () => new UniformPdf());
            RegisterPdf("gauss", () => new NormalPdf(CdfPoints, NormalLimit));
        }
    }
}
