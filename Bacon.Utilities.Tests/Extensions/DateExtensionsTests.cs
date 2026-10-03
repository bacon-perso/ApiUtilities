using Bacon.Utilities.Extensions;
using System.Collections;

namespace Bacon.Utilities.Tests.Extensions;

[TestFixture]
internal sealed class DateExtensionsTests
{
    #region IsBetweenDates

    public static IEnumerable IsBetweenDates_Valid_Source
    {
        get
        {
            yield return new TestCaseData<DateTime, DateTime?, DateTime?>(new DateTime(2020, 3, 5), new DateTime(2020, 3, 4), new DateTime(2020, 3, 6)).Returns(true);
            yield return new TestCaseData<DateTime, DateTime?, DateTime?>(new DateTime(2020, 3, 5), new DateTime(2020, 3, 5), new DateTime(2020, 3, 5)).Returns(true);
            yield return new TestCaseData<DateTime, DateTime?, DateTime?>(new DateTime(2020, 3, 5, 1, 1, 1), new DateTime(2020, 3, 5, 1, 1, 1), new DateTime(2020, 3, 5, 1, 1, 1) ).Returns(true);
            yield return new TestCaseData<DateTime, DateTime?, DateTime?>(new DateTime(2020, 3, 5, 1, 1, 1), new DateTime(2020, 3, 5, 1, 1, 0), new DateTime(2020, 3, 5, 1, 1, 3) ).Returns(true);
            yield return new TestCaseData<DateTime, DateTime?, DateTime?>(new DateTime(2020, 3, 5, 1, 1, 1), new DateTime(2020, 3, 5, 1, 1, 0), new DateTime(2020, 3, 5, 1, 1, 1) ).Returns(true);
            yield return new TestCaseData<DateTime, DateTime?, DateTime?>(new DateTime(2020, 3, 5, 1, 1, 1), new DateTime(2020, 3, 5, 1, 1, 1), new DateTime(2020, 3, 5, 1, 1, 2) ).Returns(true);
            yield return new TestCaseData<DateTime, DateTime?, DateTime?>(new DateTime(2020, 3, 5, 1, 1, 1), null!, new DateTime(2020, 3, 5, 1, 1, 2)).Returns(true);
            yield return new TestCaseData<DateTime, DateTime?, DateTime?>(new DateTime(2020, 3, 5, 1, 1, 1), new DateTime(2020, 3, 5, 1, 1, 1), null!).Returns(true);
            yield return new TestCaseData<DateTime, DateTime?, DateTime?>(new DateTime(2020, 3, 5), new DateTime(2020, 3, 6), new DateTime(2020, 3, 4)).Returns(false);
            yield return new TestCaseData<DateTime, DateTime?, DateTime?>(new DateTime(2020, 3, 5), new DateTime(2020, 3, 6), new DateTime(2020, 3, 7)).Returns(false);
            yield return new TestCaseData<DateTime, DateTime?, DateTime?>(new DateTime(2020, 3, 5), new DateTime(2020, 3, 3), new DateTime(2020, 3, 4)).Returns(false);

            //only dateTo provided: dateFrom is treated as open ended
            yield return new TestCaseData<DateTime, DateTime?, DateTime?>(new DateTime(2020, 3, 5), null!, new DateTime(2020, 3, 5)).Returns(true);
            yield return new TestCaseData<DateTime, DateTime?, DateTime?>(new DateTime(2020, 3, 5), null!, new DateTime(2020, 3, 4)).Returns(false);
            yield return new TestCaseData<DateTime, DateTime?, DateTime?>(DateTime.MinValue, null!, new DateTime(2020, 3, 4)).Returns(true);

            //only dateFrom provided: dateTo is treated as open ended
            yield return new TestCaseData<DateTime, DateTime?, DateTime?>(new DateTime(2020, 3, 5), new DateTime(2020, 3, 5), null!).Returns(true);
            yield return new TestCaseData<DateTime, DateTime?, DateTime?>(new DateTime(2020, 3, 5), new DateTime(2020, 3, 6), null!).Returns(false);
            yield return new TestCaseData<DateTime, DateTime?, DateTime?>(DateTime.MaxValue, new DateTime(2020, 3, 6), null!).Returns(true);

            //no bounds provided
            yield return new TestCaseData<DateTime, DateTime?, DateTime?>(new DateTime(2020, 3, 5), null!, null!).Returns(true);
            yield return new TestCaseData<DateTime, DateTime?, DateTime?>(DateTime.MinValue, null!, null!).Returns(true);
            yield return new TestCaseData<DateTime, DateTime?, DateTime?>(DateTime.MaxValue, null!, null!).Returns(true);
        }
    }

    [TestCaseSource(nameof(IsBetweenDates_Valid_Source))]
    public bool IsBetweenDates_Valid(DateTime source, DateTime? value1, DateTime? value2)
    {
        return source.IsBetweenDates(value1, value2);
    }

    #endregion IsBetweenDates
}