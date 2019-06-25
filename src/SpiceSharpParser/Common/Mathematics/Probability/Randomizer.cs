using System;
using System.Collections.Generic;

namespace SpiceSharpParser.Common.Mathematics.Probability
{
    public class Randomizer : IRandomizer
    {
        private readonly Dictionary<string, Pdf> _pdfDictionary = new Dictionary<string, Pdf>();
        private readonly Dictionary<string, CustomRandomNumberProviderFactory> _customRandomNumberProviderFactories = new Dictionary<string, CustomRandomNumberProviderFactory>();
        private readonly DefaultRandomNumberProviderFactory _defaultRandomNumberProviderFactory = new DefaultRandomNumberProviderFactory();

        public void RegisterPdf(string name, Pdf pdf)
        {
            _pdfDictionary[name] = pdf;
        }

        public IRandomDoubleProvider GetRandomDoubleProvider(int? seed, string pdfName = null)
        {
            if (pdfName == null)
            {
                return _defaultRandomNumberProviderFactory.GetRandomDouble(seed);
            }

            if (_pdfDictionary.ContainsKey(pdfName))
            {
                if (!_customRandomNumberProviderFactories.ContainsKey(pdfName))
                {
                    _customRandomNumberProviderFactories[pdfName] = new CustomRandomNumberProviderFactory(_pdfDictionary[pdfName]);
                }

                return _customRandomNumberProviderFactories[pdfName].GetRandomDouble(seed);
            }
            else
            {
                throw new ArgumentException("Unknown pdf", nameof(pdfName));
            }
        }

        public IRandomIntegerProvider GetRandomIntegerProvider(int? seed, string pdfName = null)
        {
            if (pdfName == null)
            {
                return _defaultRandomNumberProviderFactory.GetRandomInteger(seed);
            }

            if (_pdfDictionary.ContainsKey(pdfName))
            {
                if (!_customRandomNumberProviderFactories.ContainsKey(pdfName))
                {
                    _customRandomNumberProviderFactories[pdfName] = new CustomRandomNumberProviderFactory(_pdfDictionary[pdfName]);
                }

                return _customRandomNumberProviderFactories[pdfName].GetRandomInteger(seed);
            }
            else
            {
                throw new ArgumentException("Unknown pdf", nameof(pdfName));
            }
        }

        public IRandomNumberProvider GetRandomProvider(int? seed, string pdfName = null)
        {
            if (pdfName == null)
            {
                return _defaultRandomNumberProviderFactory.GetRandom(seed);
            }

            if (_pdfDictionary.ContainsKey(pdfName))
            {
                if (!_customRandomNumberProviderFactories.ContainsKey(pdfName))
                {
                    _customRandomNumberProviderFactories[pdfName] = new CustomRandomNumberProviderFactory(_pdfDictionary[pdfName]);
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
