using Bacon.ApiUtilities.Services;
using System.Net;

namespace Bacon.ApiUtilities.Tests.Services;

/// <summary>
/// The http status code of an internal error code is its first 3 digits. The number of extra digits is not restricted.
/// </summary>
[TestFixture]
internal sealed class InternalErrorCodesHelperTests
{
    #region Valid codes

    [TestCase(429, HttpStatusCode.TooManyRequests)]
    [TestCase(4290, HttpStatusCode.TooManyRequests)]
    [TestCase(42900, HttpStatusCode.TooManyRequests)]
    [TestCase(429000, HttpStatusCode.TooManyRequests)]
    [TestCase(429999999999, HttpStatusCode.TooManyRequests)]
    [TestCase(500000, HttpStatusCode.InternalServerError)]
    [TestCase(412000, HttpStatusCode.PreconditionFailed)]
    [TestCase(100, HttpStatusCode.Continue)]
    public void TryGetHttpStatusCode_WithWellFormedCode_ShouldReturnTheFirstThreeDigits(long internalErrorCode, HttpStatusCode expectedHttpStatusCode)
    {
        bool result = InternalErrorCodesHelper.TryGetHttpStatusCode(internalErrorCode, out HttpStatusCode? httpStatusCode);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(expectedHttpStatusCode, Is.EqualTo(httpStatusCode));
        });
    }

    #endregion Valid codes

    #region Invalid codes

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(42)]
    [TestCase(99)]
    [TestCase(600000)]
    [TestCase(999999)]
    [TestCase(-429)]
    [TestCase(-429000)]
    [TestCase(long.MinValue)]
    [TestCase(long.MaxValue)]
    public void TryGetHttpStatusCode_WithMalformedCode_ShouldFail(long internalErrorCode)
    {
        bool result = InternalErrorCodesHelper.TryGetHttpStatusCode(internalErrorCode, out HttpStatusCode? httpStatusCode);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(httpStatusCode, Is.Null);
        });
    }

    #endregion Invalid codes
}
