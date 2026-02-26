using OpenAPIDyalog.Utils;

namespace OpenAPIDyalog.Tests.Utils;

public class StringHelpersTests
{
    [Fact]
    public void ToValidAplName_EmptyString_ReturnsEmptyName()
    {
        Assert.Equal("⍙empty", StringHelpers.ToValidAplName(""));
    }

    [Fact]
    public void ToValidAplName_ValidName_ReturnsUnchanged()
    {
        Assert.Equal("validName", StringHelpers.ToValidAplName("validName"));
    }

    [Fact]
    public void ToValidAplName_StartsWithDigit_StartsWithDeltaUnderbar()
    {
        var result = StringHelpers.ToValidAplName("123abc");
        Assert.StartsWith("⍙", result);
    }

    [Fact]
    public void ToValidAplName_WithHyphen_EscapesHyphen()
    {
        // Hyphen is UCS code 45 → encoded as ⍙45⍙
        var result = StringHelpers.ToValidAplName("hello-world");
        Assert.Contains("45", result);
    }

    [Fact]
    public void CommentLines_Null_ReturnsEmpty()
    {
        Assert.Equal("", StringHelpers.CommentLines(null));
    }

    [Fact]
    public void CommentLines_MultipleLines_PrefixesEach()
    {
        var result = StringHelpers.CommentLines("line1\nline2");
        Assert.Equal("⍝ line1\n⍝ line2", result);
    }
}
