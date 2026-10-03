using Bacon.Utilities.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace Bacon.Utilities.Tests.Extensions;

[TestFixture]
internal sealed class StringExtensionsTests
{
    #region MaskString

    [TestCase("abcdef", (ushort)0, (ushort)5, '*', ExpectedResult = "*****")]
    [TestCase("abcdef", (ushort)0, (ushort)6, '*', ExpectedResult = "******")]
    [TestCase("abcdef", (ushort)1, (ushort)5, '*', ExpectedResult = "a****")]
    [TestCase("abcdef", (ushort)0, (ushort)0, '*', ExpectedResult = "abcdef")]
    [TestCase("abcdef", (ushort)6, (ushort)6, '*', ExpectedResult = "abcdef")]
    [TestCase("abcdef", (ushort)0, (ushort)10, '*', ExpectedResult = "**********")]
    [TestCase("abcdef", (ushort)5, (ushort)10, '*', ExpectedResult = "abcde*****")]
    [TestCase("abcdef", (ushort)6, (ushort)10, '*', ExpectedResult = "abcdef****")]
    [TestCase("abcdef", (ushort)7, (ushort)10, '*', ExpectedResult = "**********")]
    [TestCase("", (ushort)0, (ushort)0, '*', ExpectedResult = "")]
    [TestCase("", (ushort)0, (ushort)5, '*', ExpectedResult = "*****")]
    [TestCase("", (ushort)3, (ushort)5, '*', ExpectedResult = "*****")]
    [TestCase("abcdef", (ushort)7, (ushort)7, '*', ExpectedResult = "abcdef")]
    [TestCase("abcdef", (ushort)20, (ushort)20, '*', ExpectedResult = "abcdef")]
    [TestCase("abcdef", (ushort)1, (ushort)1, '#', ExpectedResult = "abcdef")]
    [TestCase("abcdef", (ushort)3, (ushort)6, '#', ExpectedResult = "abc###")]
    public string MaskString_Valid(string value, ushort maskStart, ushort maskEnd, char maskChar)
    {
        return value.MaskString(maskStart, maskEnd, maskChar);
    }

    [TestCase(null, (ushort)5, (ushort)10, '*')]
    public void MaskString_ShouldThrowArgumentNullException([AllowNull] string? value, [AllowNull] ushort maskStart, [AllowNull] ushort maskEnd, [AllowNull] char maskChar)
    {
        Assert.That(() => value!.MaskString(maskStart, maskEnd, maskChar), Throws.TypeOf<ArgumentNullException>());
    }

    [TestCase("nffdbgfdbgjdfbgfbg", (ushort)10, (ushort)5, '*')]
    [TestCase("nffdbgfdbgjdfbgfbg", (ushort)5, (ushort)10, ' ')]
    public void MaskString_ShouldThrowArgumentException(string value, ushort maskStart, ushort maskEnd, char maskChar)
    {
        Assert.That(() => value.MaskString(maskStart, maskEnd, maskChar), Throws.TypeOf<ArgumentException>());
    }

    #endregion MaskString
}