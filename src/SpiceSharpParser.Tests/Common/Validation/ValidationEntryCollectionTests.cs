using System.Linq;
using SpiceSharpParser.Common.Validation;
using Xunit;

namespace SpiceSharpParser.Tests.Common.Validation
{
    public class ValidationEntryCollectionTests
    {
        [Fact]
        public void When_CollectionHasErrorsAndWarnings_Expect_FilteredAccessors()
        {
            var entries = new ValidationEntryCollection();

            entries.AddError(ValidationEntrySource.Reader, "reader error");
            entries.AddWarning(ValidationEntrySource.Parser, "parser warning");

            var error = Assert.Single(entries.Errors);
            var warning = Assert.Single(entries.Warnings);

            Assert.True(entries.HasError);
            Assert.True(entries.HasWarning);
            Assert.Equal(ValidationEntryLevel.Error, error.Level);
            Assert.Equal("reader error", error.Message);
            Assert.Equal(ValidationEntryLevel.Warning, warning.Level);
            Assert.Equal("parser warning", warning.Message);
            Assert.Equal(2, entries.Count());
        }
    }
}
