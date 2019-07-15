namespace SpiceSharpParser.Common.Mathematics.Probability.Pdfs
{
    /// <summary>
    /// Uniform distribution Pdf.
    /// </summary>
    public class UniformPdf : Pdf
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UniformPdf"/> class.
        /// </summary>
        public UniformPdf()
        {
            Points.Add(new Point(-1.0, 0.5));
            Points.Add(new Point(1.0, 0.5));
        }
    }
}
