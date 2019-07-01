using System;
using System.Collections.Generic;

namespace SpiceSharpParser.Common.Mathematics.Probability
{
    /// <summary>
    /// Random numbers facade.
    /// </summary>
    public class Randomizer : IRandomizer
    {
        private readonly Dictionary<string, Pdf> _pdfDictionary = new Dictionary<string, Pdf>();
        private readonly Dictionary<string, CustomRandomNumberProviderFactory> _customRandomNumberProviderFactories = new Dictionary<string, CustomRandomNumberProviderFactory>();
        private readonly DefaultRandomNumberProviderFactory _defaultRandomNumberProviderFactory = new DefaultRandomNumberProviderFactory();

        /// <summary>
        /// Default number of CDF points.
        /// </summary>
        public const int DefaultCdfPoints = 1000;

        /// <summary>
        /// Initializes a new instance of the <see cref="Randomizer"/> class.
        /// </summary>
        /// <param name="cdfPoints">Number of cdf points.</param>
        public Randomizer(int? cdfPoints = null)
        {
            CurrentPdfName = null;
            CdfPoints = cdfPoints ?? DefaultCdfPoints;
        }

        /// <summary>
        /// Gets or sets current pdf name.
        /// </summary>
        public string CurrentPdfName { get; set; }

        /// <summary>
        /// Gets or sets number of CDF points.
        /// </summary>
        public int CdfPoints { get; set; }

        /// <summary>
        /// Registers a Pdf in the randomizer.
        /// </summary>
        /// <param name="name">Name of Pdf.</param>
        /// <param name="pdf">Pdf.</param>
        public void RegisterPdf(string name, Pdf pdf)
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
                if (!_customRandomNumberProviderFactories.ContainsKey(pdfName))
                {
                    _customRandomNumberProviderFactories[pdfName] =
                        new CustomRandomNumberProviderFactory(
                            _pdfDictionary[pdfName],
                            CdfPoints);
                }

                return _customRandomNumberProviderFactories[pdfName].GetRandom(seed);
            }
            else
            {
                throw new ArgumentException("Unknown pdf", nameof(pdfName));
            }
        }
    }
}
