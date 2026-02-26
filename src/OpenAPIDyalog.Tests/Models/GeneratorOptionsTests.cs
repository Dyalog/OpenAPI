using OpenAPIDyalog.Models;

namespace OpenAPIDyalog.Tests.Models;

public class GeneratorOptionsTests
{
    [Fact]
    public void IsValid_EmptyPath_ReturnsFalse()
    {
        Assert.False(new GeneratorOptions { SpecificationPath = "" }.IsValid());
    }

    [Fact]
    public void IsValid_WhitespacePath_ReturnsFalse()
    {
        Assert.False(new GeneratorOptions { SpecificationPath = "   " }.IsValid());
    }

    [Fact]
    public void IsValid_WithPath_ReturnsTrue()
    {
        Assert.True(new GeneratorOptions { SpecificationPath = "openapi.json" }.IsValid());
    }

    [Fact]
    public void GetValidationErrors_MissingPath_ReturnsExactlyOneError()
    {
        var errors = new GeneratorOptions().GetValidationErrors();
        Assert.Single(errors);
    }

    [Fact]
    public void GetValidationErrors_WithPath_ReturnsNoErrors()
    {
        var errors = new GeneratorOptions { SpecificationPath = "openapi.json" }.GetValidationErrors();
        Assert.Empty(errors);
    }
}
