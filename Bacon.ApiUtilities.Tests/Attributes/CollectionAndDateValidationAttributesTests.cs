using Bacon.ApiUtilities.Attributes.Validations;
using Bacon.ApiUtilities.Models;
using Bacon.ApiUtilities.Tests.Support;
using System.ComponentModel.DataAnnotations;

namespace Bacon.ApiUtilities.Tests.Attributes;

[TestFixture]
internal sealed class UtcDateTimeFormatAttributeTests
{
    private readonly UtcDateTimeFormatAttribute _attribute = new();
    private ValidationAttributeTestHelper _helper = null!;

    [SetUp]
    public void SetUp()
    {
        _helper = new();
    }

    [Test]
    public void Null_ShouldBeValid()
    {
        Assert.That(_helper.Validate(_attribute, null), Is.EqualTo(ValidationResult.Success));
    }

    [Test]
    public void UtcDateTime_ShouldBeValid()
    {
        Assert.That(_helper.Validate(_attribute, new DateTime(2025, 1, 2, 3, 4, 5, DateTimeKind.Utc)), Is.EqualTo(ValidationResult.Success));
    }

    [Test]
    public void NullableUtcDateTime_ShouldBeValid()
    {
        DateTime? value = new DateTime(2025, 1, 2, 3, 4, 5, DateTimeKind.Utc);

        Assert.That(_helper.Validate(_attribute, value), Is.EqualTo(ValidationResult.Success));
    }

    [TestCase(DateTimeKind.Local)]
    [TestCase(DateTimeKind.Unspecified)]
    public void NonUtcDateTime_ShouldBeInvalid(DateTimeKind kind)
    {
        ValidationResult? result = _helper.Validate(_attribute, new DateTime(2025, 1, 2, 3, 4, 5, kind));

        Assert.Multiple(() =>
        {
            Assert.That(result?.ErrorMessage, Is.EqualTo(ValidationAttributeTestHelper.InvalidMessage(ValidationAttributeTypes.UtcDateTimeFormatAttribute)));
            Assert.That(_helper.InvalidResults, Is.EqualTo([ValidationAttributeTypes.UtcDateTimeFormatAttribute]));
        });
    }

    [Test]
    public void ValueThatIsNotADateTime_ShouldThrowWithAMessage()
    {
        InvalidCastException? exception = Assert.Throws<InvalidCastException>(() => _helper.Validate(_attribute, "2025-01-02T03:04:05Z"));

        Assert.That(exception!.Message, Does.Contain(nameof(UtcDateTimeFormatAttribute)));
    }

    [Test]
    public void DateTimeOffset_ShouldThrow()
    {
        Assert.Throws<InvalidCastException>(() => _helper.Validate(_attribute, DateTimeOffset.UtcNow));
    }
}

[TestFixture]
internal sealed class RequiredEnumerableContentAttributeTests
{
    private readonly RequiredEnumerableContentAttribute _attribute = new();
    private ValidationAttributeTestHelper _helper = null!;

    [SetUp]
    public void SetUp()
    {
        _helper = new();
    }

    [Test]
    public void Null_ShouldBeValid()
    {
        Assert.That(_helper.Validate(_attribute, null), Is.EqualTo(ValidationResult.Success));
    }

    [Test]
    public void EmptyCollection_ShouldBeValid()
    {
        Assert.That(_helper.Validate(_attribute, new List<string>()), Is.EqualTo(ValidationResult.Success));
    }

    [Test]
    public void CollectionWithoutNullItems_ShouldBeValid()
    {
        Assert.That(_helper.Validate(_attribute, new List<string> { "a", "b" }), Is.EqualTo(ValidationResult.Success));
    }

    [Test]
    public void CollectionWithANullItem_ShouldBeInvalid()
    {
        ValidationResult? result = _helper.Validate(_attribute, new List<string?> { "a", null, "b" });

        Assert.Multiple(() =>
        {
            Assert.That(result?.ErrorMessage, Is.EqualTo(ValidationAttributeTestHelper.InvalidMessage(ValidationAttributeTypes.RequiredEnumerableContentAttribute)));
            Assert.That(_helper.InvalidResults, Is.EqualTo([ValidationAttributeTypes.RequiredEnumerableContentAttribute]));
        });
    }

    [Test]
    public void ArrayWithNullItems_ShouldBeInvalidOnceNoMatterHowManyNulls()
    {
        _helper.Validate(_attribute, new object?[] { null, null });

        Assert.That(_helper.InvalidResults, Has.Count.EqualTo(1));
    }

    [Test]
    public void LazySequence_ShouldBeEnumeratedOnlyOnce()
    {
        int enumerations = 0;

        IEnumerable<string> Sequence()
        {
            enumerations++;
            yield return "a";
            yield return "b";
        }

        ValidationResult? result = _helper.Validate(_attribute, Sequence());

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(ValidationResult.Success));
            Assert.That(enumerations, Is.EqualTo(1));
        });
    }

    [Test]
    public void ValueThatIsNotEnumerable_ShouldThrowWithAMessage()
    {
        InvalidCastException? exception = Assert.Throws<InvalidCastException>(() => _helper.Validate(_attribute, 5));

        Assert.That(exception!.Message, Does.Contain(nameof(RequiredEnumerableContentAttribute)));
    }
}

[TestFixture]
internal sealed class DuplicatedItemsAttributeTests
{
    private readonly DuplicatedItemsAttribute _attribute = new();
    private ValidationAttributeTestHelper _helper = null!;

    [SetUp]
    public void SetUp()
    {
        _helper = new();
    }

    [Test]
    public void Null_ShouldBeValid()
    {
        Assert.That(_helper.Validate(_attribute, null), Is.EqualTo(ValidationResult.Success));
    }

    [Test]
    public void EmptyCollection_ShouldBeValid()
    {
        Assert.That(_helper.Validate(_attribute, new List<int>()), Is.EqualTo(ValidationResult.Success));
    }

    [Test]
    public void UniqueItems_ShouldBeValid()
    {
        Assert.That(_helper.Validate(_attribute, new List<int> { 1, 2, 3 }), Is.EqualTo(ValidationResult.Success));
    }

    [Test]
    public void SingleNullItem_ShouldBeValid()
    {
        Assert.That(_helper.Validate(_attribute, new List<string?> { "a", null }), Is.EqualTo(ValidationResult.Success));
    }

    [Test]
    public void DuplicatedNumbers_ShouldBeInvalid()
    {
        ValidationResult? result = _helper.Validate(_attribute, new List<int> { 1, 2, 1 });

        Assert.Multiple(() =>
        {
            Assert.That(result?.ErrorMessage, Is.EqualTo(ValidationAttributeTestHelper.InvalidMessage(ValidationAttributeTypes.DuplicatedItemsAttribute)));
            Assert.That(_helper.InvalidResults, Is.EqualTo([ValidationAttributeTypes.DuplicatedItemsAttribute]));
        });
    }

    [Test]
    public void DuplicatedStrings_ShouldBeInvalid()
    {
        Assert.That(_helper.Validate(_attribute, new[] { "a", "b", "a" }), Is.Not.Null);
    }

    [Test]
    public void StringsThatDifferOnlyByCase_ShouldBeValid()
    {
        Assert.That(_helper.Validate(_attribute, new[] { "a", "A" }), Is.EqualTo(ValidationResult.Success));
    }

    [Test]
    public void DuplicatedNullItems_ShouldBeInvalid()
    {
        Assert.That(_helper.Validate(_attribute, new List<string?> { null, null }), Is.Not.Null);
    }

    [Test]
    public void DuplicatedItems_ShouldBeReportedOnceNoMatterHowManyDuplicates()
    {
        _helper.Validate(_attribute, new[] { 1, 1, 2, 2, 3, 3 });

        Assert.That(_helper.InvalidResults, Has.Count.EqualTo(1));
    }

    [Test]
    public void Dictionary_ShouldThrow()
    {
        Assert.Throws<InvalidCastException>(() => _helper.Validate(_attribute, new Dictionary<string, int> { ["a"] = 1 }));
    }

    [Test]
    public void ValueThatIsNotEnumerable_ShouldThrow()
    {
        Assert.Throws<InvalidCastException>(() => _helper.Validate(_attribute, 5));
    }
}
