using Bacon.Utilities.Services;
using System.Collections;
using System.Data;
using System.Globalization;
using System.Reflection;

namespace Bacon.Utilities.Tests.Services;

[TestFixture]
internal sealed class DataConvertersTests
{
    private class MyObject
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    #region DictionaryToDataTable

    #region Source Data

    public static IEnumerable DictionaryToDataTable_WithEmptyData_Valid_Source
    {
        get
        {
            yield return new TestCaseData<string, string>("toto", "tata");
            yield return new TestCaseData<int, int>(1, 1);
            yield return new TestCaseData<int, List<int>>(1, [1, 1]);
            yield return new TestCaseData<int, List<int?>>(1, [1, 1]);
            yield return new TestCaseData<int, List<int>?>(1, [1, 1]);
        }
    }

    public static IEnumerable DictionaryToDataTable_WithSimpleData_Valid_Source
    {
        get
        {
            yield return new TestCaseData(typeof(string), typeof(string), new object[] { "1", "3" }, new object[] { "2", "4" });
            yield return new TestCaseData(typeof(int), typeof(string), new object[] { 1, 3 }, new object[] { "2", "4" });
            yield return new TestCaseData(typeof(string), typeof(int), new object[] { "1", "3" }, new object[] { 2, 4 });
            yield return new TestCaseData(typeof(long), typeof(long), new object[] { 1L, 3L }, new object[] { 2L, 4L });
            yield return new TestCaseData(typeof(int), typeof(Guid), new object[] { 1, 3 }, new object[] { new Guid("967773ff-1ccd-4aff-a143-ded8c9834831"), Guid.Empty });
            yield return new TestCaseData(typeof(Guid), typeof(int), new object[] { new Guid("967773ff-1ccd-4aff-a143-ded8c9834831"), Guid.Empty }, new object[] { 2, 4 });
        }
    }

    public static IEnumerable DictionaryToDataTable_WithNullData_Valid_Source
    {
        get
        {
            yield return new TestCaseData<string?>("2");
            yield return new TestCaseData<int?>(2);
            yield return new TestCaseData<double?>(2);
            yield return new TestCaseData<Guid?>(new Guid("967773ff-1ccd-4aff-a143-ded8c9834831"));
        }
    }

    public static IEnumerable DictionaryToDataTable_WithComplexData_Valid_Source
    {
        get
        {
            yield return new TestCaseData(typeof(int), typeof(IEnumerable<int?>), new object[] { 1, 3 }, new object[] { new List<int?> { 2, null }, new List<int?> { 4, null } });
            yield return new TestCaseData(typeof(int), typeof(List<int?>), new object[] { 1, 3 }, new object[] { new List<int?> { 2, null }, new List<int?> { 4, null } });
            yield return new TestCaseData(typeof(int), typeof(IEnumerable<string?>), new object[] { 1, 3 }, new object[] { new List<string?> { "2", null }, new List<string?> { "4", null } });
            yield return new TestCaseData(typeof(int), typeof(List<Guid?>), new object[] { 1, 3 }, new object[] { new List<Guid?> { new Guid("967773ff-1ccd-4aff-a143-ded8c9834831"), null }, new List<Guid?> { Guid.Empty, null } });
        }
    }

    #endregion Source Data

    [TestCaseSource(nameof(DictionaryToDataTable_WithEmptyData_Valid_Source))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
    public void DictionaryToDataTable_WithEmptyData_Valid<TKey, TValue>(TKey key, TValue value) where TKey: notnull
    {
        DataTable result = DataConverters.DictionaryToDataTable(new Dictionary<TKey, TValue>());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Rows, Is.Empty);
            Assert.That(result.Columns, Has.Count.EqualTo(2));
            Assert.That(result.Columns[0].DataType.FullName, Is.EqualTo(typeof(TKey).FullName));
            Assert.That(result.Columns[1].DataType.FullName, Is.EqualTo(GetUnderlyingType(typeof(TValue)).FullName));
        }
    }

    [TestCaseSource(nameof(DictionaryToDataTable_WithSimpleData_Valid_Source))]
    public void DictionaryToDataTable_WithSimpleData_Valid(Type keyType, Type valueType, object[] keys, object[] values)
    {
        Guid guid = new("967773ff-1ccd-4aff-a143-ded8c9834831");

        Type dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
        IDictionary dic = (IDictionary)Activator.CreateInstance(dictType)!;
        for (int i = 0; i < keys.Length; i++)
        {
            dic.Add(keys[i], values[i]);
        }

        MethodInfo method = typeof(DataConverters).GetMethod(nameof(DataConverters.DictionaryToDataTable))!.MakeGenericMethod(keyType, valueType);

        DataTable result = (DataTable)method.Invoke(null, [dic])!;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Rows, Has.Count.EqualTo(2));
            Assert.That(result.Columns, Has.Count.EqualTo(2));
            Assert.That(result.Columns[0].DataType.FullName, Is.EqualTo(keyType.FullName));
            Assert.That(result.Columns[1].DataType.FullName, Is.EqualTo(valueType.FullName));

            if (keyType.Equals(typeof(Guid)))
            {
                Assert.That(result.Rows[0][0], Is.EqualTo(guid));
                Assert.That(result.Rows[1][0], Is.EqualTo(Guid.Empty));
            }
            else
            {
                Assert.That(result.Rows[0][0].ToString(), Is.EqualTo("1"));
                Assert.That(result.Rows[1][0].ToString(), Is.EqualTo("3"));
            }

            if (valueType.Equals(typeof(Guid)))
            {
                Assert.That(result.Rows[0][1], Is.EqualTo(guid));
                Assert.That(result.Rows[1][1], Is.EqualTo(Guid.Empty));
            }
            else
            {
                Assert.That(result.Rows[0][1].ToString(), Is.EqualTo("2"));
                Assert.That(result.Rows[1][1].ToString(), Is.EqualTo("4"));
            }
        }
    }

    [TestCaseSource(nameof(DictionaryToDataTable_WithNullData_Valid_Source))]
    public void DictionaryToDataTable_WithNullData_Valid<TValue>(TValue? value)
    {
        Type valueType = typeof(TValue);
        valueType = Nullable.GetUnderlyingType(valueType) ?? valueType;

        Guid guid = new("967773ff-1ccd-4aff-a143-ded8c9834831");

        Dictionary<int, TValue?> dic = new()
        {
            { 1, value },
            {3, default }
        };

        DataTable result = DataConverters.DictionaryToDataTable(dic);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Rows, Has.Count.EqualTo(2));
            Assert.That(result.Columns, Has.Count.EqualTo(2));
            Assert.That(result.Columns[0].DataType.FullName, Is.EqualTo(typeof(int).FullName));
            Assert.That(result.Columns[1].DataType.FullName, Is.EqualTo(valueType.FullName));

            Assert.That(result.Rows[0][0], Is.EqualTo(1));
            Assert.That(result.Rows[1][0], Is.EqualTo(3));

            if (valueType.Equals(typeof(Guid)))
            {
                Assert.That(result.Rows[0][1], Is.EqualTo(guid));
            }
            else
            {
                Assert.That(result.Rows[0][1].ToString(), Is.EqualTo("2"));
            }

            Assert.That(result.Rows[1][1], Is.EqualTo(DBNull.Value));
        }
    }

    [TestCaseSource(nameof(DictionaryToDataTable_WithComplexData_Valid_Source))]
    public void DictionaryToDataTable_WithComplexData_Valid(Type keyType, Type valueType, object[] keys, object[] values)
    {
        Guid guid = new("967773ff-1ccd-4aff-a143-ded8c9834831");

        Type dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
        IDictionary dic = (IDictionary)Activator.CreateInstance(dictType)!;
        for (int i = 0; i < keys.Length; i++)
        {
            dic.Add(keys[i], values[i]);
        }

        MethodInfo method = typeof(DataConverters).GetMethod(nameof(DataConverters.DictionaryToDataTable))!.MakeGenericMethod(keyType, valueType);

        DataTable result = (DataTable)method.Invoke(null, [dic])!;

        Type underlyingValueType = GetUnderlyingType(valueType);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Rows, Has.Count.EqualTo(4));
            Assert.That(result.Columns, Has.Count.EqualTo(2));
            Assert.That(result.Columns[0].DataType.FullName, Is.EqualTo(keyType.FullName));
            Assert.That(result.Columns[1].DataType.FullName, Is.EqualTo(underlyingValueType.FullName));

            Assert.That(result.Rows[0][0], Is.EqualTo(1));
            Assert.That(result.Rows[1][0], Is.EqualTo(1));
            Assert.That(result.Rows[2][0], Is.EqualTo(3));
            Assert.That(result.Rows[3][0], Is.EqualTo(3));

            if (underlyingValueType.Equals(typeof(Guid)))
            {
                Assert.That(result.Rows[0][1], Is.EqualTo(guid));
                Assert.That(result.Rows[2][1], Is.EqualTo(Guid.Empty));
            }
            else
            {
                Assert.That(result.Rows[0][1].ToString(), Is.EqualTo("2"));
                Assert.That(result.Rows[2][1].ToString(), Is.EqualTo("4"));
            }

            Assert.That(result.Rows[1][1], Is.EqualTo(DBNull.Value));
            Assert.That(result.Rows[3][1], Is.EqualTo(DBNull.Value));
        }
    }

    [TestCase()]
    public void DictionaryToDataTable_WithDictionary_ShouldThrowInvalidOperationException()
    {
        Dictionary<string, Dictionary<string, string>> dic = [];

        Assert.That(() => DataConverters.DictionaryToDataTable(dic), Throws.TypeOf<InvalidOperationException>());
    }

    [TestCase()]
    public void DictionaryToDataTable_WithNotNullList_ShouldThrowInvalidOperationException()
    {
        Dictionary<string, IEnumerable<IEnumerable<long>>> dic = [];

        Assert.That(() => DataConverters.DictionaryToDataTable(dic), Throws.TypeOf<InvalidOperationException>());
    }

    [TestCase()]
    public void DictionaryToDataTable_WithNull_ShouldThrowInvalidOperationException()
    {
        Dictionary<string, IEnumerable<IEnumerable<long>>?> dic = [];

        Assert.That(() => DataConverters.DictionaryToDataTable(dic), Throws.TypeOf<InvalidOperationException>());
    }

    [TestCase()]
    public void DictionaryToDataTable_WithNullValue_ShouldThrowInvalidOperationException()
    {
        Dictionary<string, IEnumerable<IEnumerable<long>?>> dic = [];

        Assert.That(() => DataConverters.DictionaryToDataTable(dic), Throws.TypeOf<InvalidOperationException>());
    }

    private static Type GetUnderlyingType(Type type)
    {
        Type? underlyingNullableType = Nullable.GetUnderlyingType(type);

        if (underlyingNullableType != null)
        {
            type = underlyingNullableType;
        }

        Type? underlyingType = type;
        if (type.IsGenericType)
        {
            underlyingType = type.GetGenericArguments()[0];

            underlyingNullableType = Nullable.GetUnderlyingType(underlyingType);

            if (underlyingNullableType != null)
            {
                underlyingType = underlyingNullableType;
            }
        }

        return underlyingType ?? type;
    }

    #endregion DictionaryToDataTable

    #region ItemToDataTable

    #region Source Data

    public static IEnumerable ItemToDataTable_WithData_Valid_Source
    {
        get
        {
            yield return new TestCaseData<byte>(1);
            yield return new TestCaseData<int>(1);
            yield return new TestCaseData<long>(1);
            yield return new TestCaseData<double>(1.01);
            yield return new TestCaseData<string>("toto");
            yield return new TestCaseData<Guid>(new Guid("967773ff-1ccd-4aff-a143-ded8c9834831"));
        }
    }

    #endregion Source Data

    [TestCaseSource(nameof(ItemToDataTable_WithData_Valid_Source))]
    public void ItemToDataTable_WithData_Valid<TValue>(TValue value) where TValue : notnull
    {
        DataTable result = DataConverters.ItemToDataTable(value);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Columns, Has.Count.EqualTo(1));
            Assert.That(result.Columns.Contains("Item"));
            Assert.That(result.Rows[0][0], Is.EqualTo(value));
            Assert.That(result.Columns[0].DataType.FullName, Is.EqualTo(typeof(TValue).FullName));
        }
    }

    [TestCase(null)]
    public void ItemToDataTable_ShouldThrowArgumentNullException(string? value)
    {
        Assert.That(() => DataConverters.ItemToDataTable(value!), Throws.TypeOf<ArgumentNullException>());
    }

    //In this case, c# string? behaves as a string as long as a none nullable value is passed, so will not throw an InvalidOperationException
    [TestCase("toto")]
    public void ItemToDataTable_WithNullableStringWithValue_ShouldThrowNothing(string? value)
    {
        Assert.That(() => DataConverters.ItemToDataTable(value!), Throws.Nothing);
    }

    #endregion ItemToDataTable

    #region ListToDataTable

    #region Source Data

    private static readonly object[] ListToDataTable_WithEmptyList_Valid_Source =
    [
        //should return ArgumentNullException
        new object[] { Enumerable.Empty<MyObject>() },
        new object[] { Array.Empty<string>() },
        new object[] { Enumerable.Empty<int>() },
        new object[] { Enumerable.Empty<double>() },
        new object[] { Enumerable.Empty<Guid>() },
        new object[] { Enumerable.Empty<object>() },
        new object[] { Enumerable.Empty<int?>() },
        new object[] { Enumerable.Empty<Guid?>() },
    ];

    private static readonly object[] ListToDataTable_ShouldThrowInvalidOperationException_Source =
    [
        //should return ArgumentNullException
        new object?[] { Enumerable.Empty<IEnumerable<MyObject>>() },
        new object?[] { Enumerable.Empty<IDictionary<string, string>>() },
        new object?[] { Enumerable.Repeat<List<int>>([1], 1) },
    ];

    public static IEnumerable ListToDataTable_ShouldThrowArgumentNullException_Source
    {
        get
        {
            yield return new TestCaseData<string?>("1");
            yield return new TestCaseData<int?>(1);
            yield return new TestCaseData<double?>(1);
            yield return new TestCaseData<Guid?>(new Guid("967773ff-1ccd-4aff-a143-ded8c9834831"));
            yield return new TestCaseData<object?>(new Guid("967773ff-1ccd-4aff-a143-ded8c9834831"));
        }
    }

    public static IEnumerable ListToDataTable_WithData_Valid_Source
    {
        get
        {
            yield return new TestCaseData<byte>(1, 2);
            yield return new TestCaseData<int>(1, 2);
            yield return new TestCaseData<long>(1, 2);
            yield return new TestCaseData<double>(1.01, 2.01);
            yield return new TestCaseData<Guid>(new("967773ff-1ccd-4aff-a143-ded8c9834831"), Guid.Empty);
            yield return new TestCaseData<string>("toto", "tata");
            yield return new TestCaseData<object>("toto", 2);
            yield return new TestCaseData<int?>(1, 2);
            yield return new TestCaseData<string?>("toto", "tata");
            yield return new TestCaseData<object?>("toto", 2);
        }
    }

    #endregion Source Data

    [TestCaseSource(nameof(ListToDataTable_ShouldThrowInvalidOperationException_Source))]
    public void ListToDataTable_ShouldThrowInvalidOperationException<TValue>(IEnumerable<TValue> list) where TValue : notnull
    {
        Assert.That(() => DataConverters.ListToDataTable(list), Throws.TypeOf<InvalidOperationException>());
    }

    [TestCaseSource(nameof(ListToDataTable_ShouldThrowArgumentNullException_Source))]
    public void ListToDataTable_ShouldThrowArgumentNullException<TValue>(TValue value)
    {
        Type listType = typeof(List<>).MakeGenericType([typeof(TValue)]);
        object obj = (IList)Activator.CreateInstance(listType)! ?? throw new TypeLoadException("Cannot create instance");
        List<TValue> list = obj as List<TValue> ?? throw new InvalidCastException("Cannot cast list");

        list.AddRange([value, default!]);

#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
        Assert.That(() => DataConverters.ListToDataTable(list), Throws.TypeOf<ArgumentNullException>());
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
    }

    [TestCaseSource(nameof(ListToDataTable_WithEmptyList_Valid_Source))]
    public void ListToDataTable_WithEmptyList_Valid<TValue>(IEnumerable<TValue> list) where TValue: notnull
    {
        Type type = typeof(TValue);
        type = Nullable.GetUnderlyingType(type) ?? type;

        DataTable result = DataConverters.ListToDataTable(list);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Columns, Has.Count.EqualTo(1));
            Assert.That(result.Columns.Contains("Item"));
            Assert.That(result.Rows, Is.Empty);
            Assert.That(result.Columns[0].DataType.FullName, Is.EqualTo(type.FullName));
        }
    }

    [TestCaseSource(nameof(ListToDataTable_WithData_Valid_Source))]
    public void ListToDataTable_WithData_Valid<TValue>(TValue value1, TValue value2) where TValue: notnull
    {
        Type type = typeof(TValue);
        type = Nullable.GetUnderlyingType(type) ?? type;

        DataTable result = DataConverters.ListToDataTable([value1, value2]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Columns, Has.Count.EqualTo(1));
            Assert.That(result.Columns.Contains("Item"));
            Assert.That(result.Rows, Has.Count.EqualTo(2));
            Assert.That(result.Rows[0]["Item"], Is.EqualTo(value1));
            Assert.That(result.Rows[1]["Item"], Is.EqualTo(value2));
            Assert.That(result.Columns[0].DataType.FullName, Is.EqualTo(type.FullName));
        }
    }

    [TestCase()]
    public void ListToDataTable_WithNullList_ShouldThrowArgumentNullException()
    {
        IEnumerable<int>? values = null;

        Assert.That(() => DataConverters.ListToDataTable(values!), Throws.TypeOf<ArgumentNullException>());
    }

    [TestCase()]
    public void ListToDataTable_ShouldEnumerateSourceOnlyOnce()
    {
        int enumerations = 0;

        IEnumerable<int> Source()
        {
            enumerations++;

            yield return 1;
            yield return 2;
            yield return 3;
        }

        DataTable result = DataConverters.ListToDataTable(Source());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(enumerations, Is.EqualTo(1));
            Assert.That(result.Rows, Has.Count.EqualTo(3));
        }
    }

    [TestCase()]
    public void ListToDataTable_WithNullInLazySequence_ShouldThrowArgumentNullException()
    {
        IEnumerable<string?> Source()
        {
            yield return "a";
            yield return null;
            yield return "c";
        }

#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
        Assert.That(() => DataConverters.ListToDataTable(Source()), Throws.TypeOf<ArgumentNullException>());
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
    }

    [TestCase()]
    public void ListToDataTable_ShouldPreserveOrder()
    {
        DataTable result = DataConverters.ListToDataTable([3, 1, 2]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Rows[0]["Item"], Is.EqualTo(3));
            Assert.That(result.Rows[1]["Item"], Is.EqualTo(1));
            Assert.That(result.Rows[2]["Item"], Is.EqualTo(2));
        }
    }

    #endregion ListToDataTable

    #region ToDateTime

    private static readonly DateTime _date = new(2020, 01, 31, 0, 0, 0, DateTimeKind.Utc);

    [TestCase("01-31-2020", "en-US")]
    [TestCase("01/31/2020", "en-US")]
    [TestCase("2020-01-31", null)]
    [TestCase("2020-01-31 00:00:00.000", "fr-CA")]
    [TestCase("2020-01-31 00:00:00.000", "en-US")]
    [TestCase("2020-01-31 00:00:00.000", null)]
    [TestCase("2020/01/31", null)]
    [TestCase("2020/01/31", "fr-CA")]
    [TestCase(null, "fr-CA")]
    [TestCase(null, "en-US")]
    [TestCase(null, null)]
    public void ToDateTime_Valid(object? value, string? languageCode)
    {
        IFormatProvider? formatProvider = null;
        if (!string.IsNullOrEmpty(languageCode))
        {
            formatProvider = new CultureInfo(languageCode);
        }

        DateTime? dateTime =  DataConverters.ToDateTime(value, DateTimeKind.Utc, formatProvider);

        Assert.That(dateTime, Is.EqualTo(value == null ? null : _date));
    }

    [TestCase("1")]
    [TestCase("@")]
    [TestCase("a")]
    [TestCase("31012020")]
    [TestCase("01312020")]
    [TestCase("20200131")]
    [TestCase("20203101")]
    [TestCase("2020/31/01")]
    [TestCase("2020-31-01")]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase(5)]
    [TestCase(3.14)]   //would be read as March 14 if parsed through its string representation
    [TestCase(1.5)]
    [TestCase(12.25)]
    [TestCase(20200131)]
    [TestCase(true)]
    [TestCase(3.14f)]
    public void ToDateTime_ShouldThrowFormatException(object value)
    {
        Assert.That(() => DataConverters.ToDateTime(value), Throws.TypeOf<FormatException>());
    }

    [TestCase()]
    public void ToDateTime_WithOtherTypes_ShouldThrowFormatException()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(() => DataConverters.ToDateTime(3.14m), Throws.TypeOf<FormatException>());
            Assert.That(() => DataConverters.ToDateTime(new Guid("967773ff-1ccd-4aff-a143-ded8c9834831")), Throws.TypeOf<FormatException>());
            Assert.That(() => DataConverters.ToDateTime(new object()), Throws.TypeOf<FormatException>());
            Assert.That(() => DataConverters.ToDateTime(TimeSpan.FromHours(3)), Throws.TypeOf<FormatException>());
        }
    }

    [TestCase()]
    public void ToDateTime_WithDBNull_ShouldReturnNull()
    {
        Assert.That(DataConverters.ToDateTime(DBNull.Value), Is.Null);
    }

    [TestCase(DateTimeKind.Utc)]
    [TestCase(DateTimeKind.Local)]
    [TestCase(DateTimeKind.Unspecified)]
    public void ToDateTime_WithString_ShouldApplyRequestedKind(DateTimeKind kind)
    {
        DateTime? result = DataConverters.ToDateTime("2020-01-31", kind);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Value.Kind, Is.EqualTo(kind));
            Assert.That(result.Value.Ticks, Is.EqualTo(new DateTime(2020, 1, 31).Ticks));
        }
    }

    [TestCase()]
    public void ToDateTime_WithoutKind_ShouldDefaultToUtc()
    {
        DateTime? result = DataConverters.ToDateTime("2020-01-31");

        Assert.That(result!.Value.Kind, Is.EqualTo(DateTimeKind.Utc));
    }

    [TestCase(DateTimeKind.Utc)]
    [TestCase(DateTimeKind.Local)]
    [TestCase(DateTimeKind.Unspecified)]
    public void ToDateTime_WithDateTime_ShouldReturnSameTicksWithRequestedKind(DateTimeKind kind)
    {
        DateTime source = new(2020, 1, 31, 13, 45, 10, 123, DateTimeKind.Local);

        DateTime? result = DataConverters.ToDateTime(source, kind);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result!.Value.Ticks, Is.EqualTo(source.Ticks));
            Assert.That(result.Value.Kind, Is.EqualTo(kind));
        }
    }

    [TestCase()]
    public void ToDateTime_WithDateTimeAndDifferentFormatProvider_ShouldNotDependOnCulture()
    {
        DateTime source = new(2020, 1, 31, 13, 45, 10, DateTimeKind.Unspecified);

        DateTime? result = DataConverters.ToDateTime(source, DateTimeKind.Utc, new CultureInfo("fr-CA"));

        Assert.That(result!.Value.Ticks, Is.EqualTo(source.Ticks));
    }

    [TestCase()]
    public void ToDateTime_WithDateTimeOffset_ShouldUseDateTimePartAndRequestedKind()
    {
        DateTimeOffset source = new(2020, 1, 31, 13, 0, 0, TimeSpan.FromHours(-5));

        DateTime? result = DataConverters.ToDateTime(source);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result!.Value.Ticks, Is.EqualTo(new DateTime(2020, 1, 31, 13, 0, 0).Ticks));
            Assert.That(result.Value.Kind, Is.EqualTo(DateTimeKind.Utc));
        }
    }

    [TestCase("en-US", 1, 2)]
    [TestCase("en-GB", 2, 1)]
    public void ToDateTime_WithAmbiguousString_ShouldFollowFormatProvider(string languageCode, int expectedMonth, int expectedDay)
    {
        DateTime? result = DataConverters.ToDateTime("01/02/2020", DateTimeKind.Utc, new CultureInfo(languageCode));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result!.Value.Month, Is.EqualTo(expectedMonth));
            Assert.That(result.Value.Day, Is.EqualTo(expectedDay));
            Assert.That(result.Value.Year, Is.EqualTo(2020));
        }
    }

    #endregion ToDateTime

    #region Base64Encode

    [TestCase("test", ExpectedResult = "dGVzdA==")]
    [TestCase("TEsT", ExpectedResult = "VEVzVA==")]
    [TestCase("TEsT1!", ExpectedResult = "VEVzVDEh")]
    [TestCase("", ExpectedResult = "")]
    [TestCase("é", ExpectedResult = "w6k=")]
    [TestCase(" ", ExpectedResult = "IA==")]
    public string Base64Encode_Valid(string value)
    {
        return DataConverters.Base64Encode(value);
    }

    [TestCase(null)]
    public void Base64Encode_ShouldThrowArgumentNullException(string? value)
    {
        Assert.That(() => DataConverters.Base64Encode(value!), Throws.TypeOf<ArgumentNullException>());
    }

    #endregion Base64Encode

    #region Base64Decode

    [TestCase("dGVzdA==", ExpectedResult = "test")]
    [TestCase("VEVzVA==", ExpectedResult = "TEsT")]
    [TestCase("VEVzVDEh", ExpectedResult = "TEsT1!")]
    [TestCase("SGVsbG8=\n", ExpectedResult = "Hello")]
    [TestCase("SGVsbG8=\r\n", ExpectedResult = "Hello")]
    [TestCase("SGVsbG8= ", ExpectedResult = "Hello")]
    [TestCase(" SGVsbG8=", ExpectedResult = "Hello")]
    [TestCase("SGVs\r\nbG8=", ExpectedResult = "Hello")]
    [TestCase("SGVs\tbG8=", ExpectedResult = "Hello")]
    [TestCase("SGVs bG8=", ExpectedResult = "Hello")]
    [TestCase("w6k=", ExpectedResult = "é")]
    [TestCase("", ExpectedResult = "")]
    public string Base64Decode_Valid(string value)
    {
        return DataConverters.Base64Decode(value);
    }

    [TestCase(null)]
    public void Base64Decode_ShouldThrowArgumentNullException(string? value)
    {
        Assert.That(() => DataConverters.Base64Decode(value!), Throws.TypeOf<ArgumentNullException>());
    }

    [TestCase("Not a decoded string")]
    [TestCase("abc")]
    [TestCase("====")]
    [TestCase("SGVsbG8")]
    [TestCase("SGVs*G8=")]
    [TestCase(" ")]
    [TestCase("   ")]
    [TestCase("\t")]
    [TestCase("\r\n")]
    [TestCase(" \r\n\t ")]
    public void Base64Decode_ShouldThrowFormatException(string value)
    {
        Assert.That(() => DataConverters.Base64Decode(value), Throws.TypeOf<FormatException>());
    }

    [TestCase("test")]
    [TestCase("é")]
    [TestCase("日本語のテキスト")]
    [TestCase("emoji 😀👩🏽 test")]
    [TestCase("multi\r\nline\ttext")]
    [TestCase("   ")]
    public void Base64EncodeThenDecode_ShouldReturnOriginalValue(string value)
    {
        Assert.That(DataConverters.Base64Decode(DataConverters.Base64Encode(value)), Is.EqualTo(value));
    }

    [TestCase()]
    public void Base64Decode_WithLongMimeStyleLineBreaks_ShouldNotContainNullCharacters()
    {
        string original = new('a', 300);
        string encoded = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(original), Base64FormattingOptions.InsertLineBreaks);

        string result = DataConverters.Base64Decode(encoded);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.EqualTo(original));
            Assert.That(result, Does.Not.Contain('\0'));
        }
    }

    #endregion Base64Decode

    #region TryParseBoolean

    [TestCase("0", ExpectedResult = false)]
    [TestCase("1", ExpectedResult = true)]
    [TestCase("FaLsE", ExpectedResult = false)]
    [TestCase("TrUE", ExpectedResult = true)]
    [TestCase("true", ExpectedResult = true)]
    [TestCase("false", ExpectedResult = false)]
    [TestCase(" true ", ExpectedResult = true)]
    [TestCase(" FALSE", ExpectedResult = false)]
    public bool TryParseBoolean_Valid(string value)
    {
        bool success = DataConverters.TryParseBoolean(value, out bool result);

        Assert.That(success, Is.True);

        return result;
    }

    [TestCase("", ExpectedResult = false)]
    [TestCase("IsFalse", ExpectedResult = false)]
    [TestCase("No", ExpectedResult = false)]
    [TestCase("YeS", ExpectedResult = false)]
    [TestCase("On", ExpectedResult = false)]
    [TestCase("oFf", ExpectedResult = false)]
    [TestCase(" 1", ExpectedResult = false)]
    [TestCase("1 ", ExpectedResult = false)]
    [TestCase(" 0", ExpectedResult = false)]
    [TestCase("2", ExpectedResult = false)]
    [TestCase("-1", ExpectedResult = false)]
    [TestCase("11", ExpectedResult = false)]
    [TestCase("true1", ExpectedResult = false)]
    [TestCase("   ", ExpectedResult = false)]
    public bool TryParseBoolean_Invalid(string value)
    {
        return DataConverters.TryParseBoolean(value, out _);
    }

    [TestCase(null)]
    public void TryParseBoolean_ShouldThrowArgumentNullException(string? value)
    {
        Assert.That(() => DataConverters.TryParseBoolean(value!, out _), Throws.TypeOf<ArgumentNullException>());
    }

    #endregion TryParseBoolean
}