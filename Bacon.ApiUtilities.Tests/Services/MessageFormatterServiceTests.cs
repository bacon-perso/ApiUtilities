using Bacon.ApiUtilities.Services;
using System.Globalization;

namespace Bacon.ApiUtilities.Tests.Services;

[TestFixture]
internal sealed class MessageFormatterServiceTests
{
    private sealed class ThrowingValue
    {
        public override string ToString()
        {
            throw new InvalidOperationException("ToString failed");
        }
    }

    #region Formatting

    [Test]
    public void FormatErrorResultMessage_WithNullMetadata_ShouldReturnTheTemplateUnchanged()
    {
        Assert.That(MessageFormatterService.FormatErrorResultMessage("{0} is invalid", null), Is.EqualTo("{0} is invalid"));
    }

    [Test]
    public void FormatErrorResultMessage_ShouldSubstituteTheValuesInOrder()
    {
        string result = MessageFormatterService.FormatErrorResultMessage("{0} must have at most {1} characters", ["Name", 10]);

        Assert.That(result, Is.EqualTo("Name must have at most 10 characters"));
    }

    [Test]
    public void FormatErrorResultMessage_WithMoreValuesThanPlaceholders_ShouldIgnoreTheExtraValues()
    {
        string result = MessageFormatterService.FormatErrorResultMessage("{0} is invalid", ["Name", 10, "extra"]);

        Assert.That(result, Is.EqualTo("Name is invalid"));
    }

    [Test]
    public void FormatErrorResultMessage_WithEmptyMetadataAndNoPlaceholder_ShouldReturnTheTemplate()
    {
        Assert.That(MessageFormatterService.FormatErrorResultMessage("Something failed", []), Is.EqualTo("Something failed"));
    }

    [Test]
    public void FormatErrorResultMessage_WithEscapedBraces_ShouldWriteLiteralBraces()
    {
        Assert.That(MessageFormatterService.FormatErrorResultMessage("Use {{0}} for {0}", ["Name"]), Is.EqualTo("Use {0} for Name"));
    }

    [Test]
    public void FormatErrorResultMessage_WithANullValue_ShouldWriteAnEmptyString()
    {
        string result = MessageFormatterService.FormatErrorResultMessage("[{0}]", new object[] { null! });

        Assert.That(result, Is.EqualTo("[]"));
    }

    [Test]
    public void FormatErrorResultMessage_ShouldFormatWithTheInvariantCulture()
    {
        CultureInfo previousCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

            Assert.That(MessageFormatterService.FormatErrorResultMessage("Value {0}", [1.5m]), Is.EqualTo("Value 1.5"));
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    #endregion Formatting

    #region Format errors fall back to the template

    [Test]
    public void FormatErrorResultMessage_WithMorePlaceholdersThanValues_ShouldReturnTheTemplate()
    {
        Assert.That(MessageFormatterService.FormatErrorResultMessage("{0} and {1}", ["Name"]), Is.EqualTo("{0} and {1}"));
    }

    [Test]
    public void FormatErrorResultMessage_WithPlaceholdersAndEmptyMetadata_ShouldReturnTheTemplate()
    {
        Assert.That(MessageFormatterService.FormatErrorResultMessage("{0} is invalid", []), Is.EqualTo("{0} is invalid"));
    }

    [TestCase("{0")]
    [TestCase("{")]
    [TestCase("}")]
    [TestCase("{abc}")]
    [TestCase("{0:")]
    public void FormatErrorResultMessage_WithMalformedTemplate_ShouldReturnTheTemplate(string template)
    {
        Assert.That(MessageFormatterService.FormatErrorResultMessage(template, ["Name"]), Is.EqualTo(template));
    }

    #endregion Format errors fall back to the template

    #region Other failures are not swallowed

    /// <summary>
    /// Only format errors fall back to the template. A value that fails while being converted to text is a real fault
    /// </summary>
    [Test]
    public void FormatErrorResultMessage_WhenAValueThrowsWhileFormatting_ShouldNotSwallowTheException()
    {
        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() => MessageFormatterService.FormatErrorResultMessage("{0}", [new ThrowingValue()]));

        Assert.That(exception!.Message, Is.EqualTo("ToString failed"));
    }

    #endregion Other failures are not swallowed
}
