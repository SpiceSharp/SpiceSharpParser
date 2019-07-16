using System;

namespace SpiceSharpParser.Common.Mathematics.Probability
{
    /// <summary>
    /// Interface for all random numbers generator facades.
    /// </summary>
    public interface IRandomizer
    {
        /// <summary>
        /// Gets or sets current pdf name.
        /// </summary>
        string CurrentPdfName { get; set; }

        /// <summary>
        /// Gets or sets number of cdf points.
        /// </summary>
        int CdfPoints { get; set; }

        /// <summary>
        /// Gets or sets normal limit.
        /// </summary>
        double NormalLimit { get; set; }

        /// <summary>
        /// Registers a Pdf in the randomizer.
        /// </summary>
        /// <param name="name">Name of Pdf.</param>
        /// <param name="pdf">Pdf factory.</param>
        void RegisterPdf(string name, Func<Pdf> pdf);

        /// <summary>
        /// Gets a random number provider for a given seed and pdf.
        /// </summary>
        /// <param name="seed">Seed.</param>
        /// <param name="pdfName">Name of PDF.</param>
        /// <returns>
        /// A random number provider.
        /// </returns>
        IRandomNumberProvider GetRandomProvider(int? seed, string pdfName = null);

        /// <summary>
        /// Gets a random double provider for a given seed and pdf.
        /// </summary>
        /// <param name="seed">Seed.</param>
        /// <param name="pdfName">Name of PDF.</param>
        /// <returns>
        /// A random double provider.
        /// </returns>
        IRandomDoubleProvider GetRandomDoubleProvider(int? seed, string pdfName = null);

        /// <summary>
        /// Gets a random integer provider for a given seed and pdf.
        /// </summary>
        /// <param name="seed">Seed.</param>
        /// <param name="pdfName">Name of PDF.</param>
        /// <returns>
        /// A random integer provider.
        /// </returns>
        IRandomIntegerProvider GetRandomIntegerProvider(int? seed, string pdfName = null);
    }
}