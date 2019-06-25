namespace SpiceSharpParser.Common.Mathematics.Probability
{
    public interface IRandomizer
    {
        void RegisterPdf(string name, Pdf pdf);

        IRandomNumberProvider GetRandomProvider(int? seed, string pdfName = null);

        IRandomDoubleProvider GetRandomDoubleProvider(int? seed, string pdfName = null);

        IRandomIntegerProvider GetRandomIntegerProvider(int? seed, string pdfName = null);
    }
}