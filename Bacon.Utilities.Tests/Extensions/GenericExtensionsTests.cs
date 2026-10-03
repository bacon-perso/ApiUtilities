using Bacon.Utilities.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace Bacon.Utilities.Tests.Extensions;

[TestFixture]
internal sealed class GenericExtensionsTests
{
    #region Source Data

    private static readonly short[] shortArr = [1, 2, 3];
    private static readonly int[] intArr = [1, 2, 3];
    private static readonly long[] longArr = [1, 2, 3];
    private static readonly double[] doubleArr = [1.07789, 1.0];
    private static readonly string[] strArr = ["allo", "test"];
    private static readonly short?[] nullShortArr = [1, 2, 3];
    private static readonly List<short> shortList = [1, 2, 3];
    private static readonly List<int> intList = [1, 2, 3];
    private static readonly List<long> longList = [1, 2, 3];
    private static readonly List<double> doubleList = [1.07789, 1.0];
    private static readonly List<string> strList = ["allo", "test"];
    private static readonly List<short?> nullShortList = [1, 2, 3];

    #region IsIn

    private static readonly object[] _sourceIsIn_ShouldWork =
    [
        //should return true
        new object[] { (short)1, shortArr, true },
        new object[] { (int)1, intArr, true },
        new object[] { (long)1, longArr, true },
        new object[] { (double)1.0, doubleArr, true },
        new object[] { "test", strArr, true },
        //should return false
        new object[] { (short)4, shortArr, false },
        new object[] { (int)4, intArr, false },
        new object[] { (long)4, longArr, false },
        new object[] { (double)1.0001, doubleArr, false },
        new object[] { "testt", strArr, false },
        //should return true
        new object[] { (short)1, shortList, true },
        new object[] { (int)1, intList, true },
        new object[] { (long)1, longList, true },
        new object[] { (double)1.0, doubleList, true },
        new object[] { "test", strList, true },
        //should return false
        new object[] { (short)4, shortList, false },
        new object[] { (int)4, intList, false },
        new object[] { (long)4, longList, false },
        new object[] { (double)1.0001, doubleList, false },
        new object[] { "testt", strList, false },
    ];

    private static readonly object[] _sourceIsIn_ShouldWorkWithIndex =
    [
        //should return true
        new object[] { (short)1, shortArr, true, 0 },
        new object[] { (int)1, intArr, true, 0 },
        new object[] { (long)1, longArr, true, 0 },
        new object[] { (double)1.0, doubleArr, true, 1 },
        new object[] { "test", strArr, true, 1 },
        //should return false
        new object[] { (short)4, shortArr, false, -1 },
        new object[] { (int)4, intArr, false, -1 },
        new object[] { (long)4, longArr, false, -1 },
        new object[] { (double)1.0001, doubleArr, false, -1 },
        new object[] { "testt", strArr, false, -1 },
        //should return true
        new object[] { (short)1, shortList, true, 0 },
        new object[] { (int)1, intList, true, 0 },
        new object[] { (long)1, longList, true, 0 },
        new object[] { (double)1.0, doubleList, true, 1 },
        new object[] { "test", strList, true, 1 },
        //should return false
        new object[] { (short)4, shortList, false, -1 },
        new object[] { (int)4, intList, false, -1 },
        new object[] { (long)4, longList, false, -1 },
        new object[] { (double)1.0001, doubleList, false, -1 },
        new object[] { "testt", strList, false, -1 },
    ];

    private static readonly object[] _sourceIsIn_ShouldGiveArgumentNullException =
    [
        //should return ArgumentNullException
        new object?[] { null, nullShortArr },
        new object?[] { null, nullShortList },
        new object?[] { 1, null },
        new object?[] { 1, null },
    ];

    #endregion IsIn

    #region IsNotIn

    private static readonly object[] _sourceIsNotIn_ShouldWork =
    [
        //should return true
        new object[] { (short)1, shortArr, false },
        new object[] { (int)1, intArr, false },
        new object[] { (long)1, longArr, false },
        new object[] { (double)1.0, doubleArr, false },
        new object[] { "test", strArr, false },
        //should return false
        new object[] { (short)4, shortArr, true },
        new object[] { (int)4, intArr, true },
        new object[] { (long)4, longArr, true },
        new object[] { (double)1.0001, doubleArr, true },
        new object[] { "testt", strArr, true },
        //should return true
        new object[] { (short)1, shortList, false },
        new object[] { (int)1, intList, false },
        new object[] { (long)1, longList, false },
        new object[] { (double)1.0, doubleList, false },
        new object[] { "test", strList, false },
        //should return false
        new object[] { (short)4, shortList, true },
        new object[] { (int)4, intList, true },
        new object[] { (long)4, longList, true },
        new object[] { (double)1.0001, doubleList, true },
        new object[] { "testt", strList, true },
    ];

    private static readonly object[] _sourceIsNotIn_ShouldGiveArgumentNullException =
    [
        //should return ArgumentNullException
        new object?[] { null, nullShortArr },
        new object?[] { null, nullShortList },
        new object?[] { 1, null },
        new object?[] { 1, null },
    ];

    #endregion IsNotIn

    #endregion Source Data

    #region IsIn

    [TestCaseSource(nameof(_sourceIsIn_ShouldWork))]
    public void IsIn_Valid<T>(T obj, ICollection<T> collection, bool expectedResult)
    {
        bool result = obj.IsIn(collection);
        Assert.That(expectedResult, Is.EqualTo(result));
    }

    [TestCaseSource(nameof(_sourceIsIn_ShouldGiveArgumentNullException))]
    public void IsIn_ShouldThrowArgumentNullException<T>([AllowNull] T obj, [AllowNull] ICollection<T> collection)
    {
        Assert.That(() => obj.IsIn(collection!), Throws.TypeOf<ArgumentNullException>());
    }

    #endregion IsIn

    #region IsIn (out)

    [TestCaseSource(nameof(_sourceIsIn_ShouldWorkWithIndex))]
    public void IsIn<T>(T obj, ICollection<T> collection, bool expectedResult, int expectedIndex)
    {
        bool result = obj.IsIn(collection, out int index);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(expectedResult, Is.EqualTo(result));
            Assert.That(expectedIndex, Is.EqualTo(index));
        }
    }

    #endregion IsIn (out)

    #region IsNotIn

    [TestCaseSource(nameof(_sourceIsNotIn_ShouldWork))]
    public void IsNotIn<T>(T obj, ICollection<T> collection, bool expectedResult)
    {
        bool result = obj.IsNotIn(collection);
        Assert.That(expectedResult, Is.EqualTo(result));
    }

    [TestCaseSource(nameof(_sourceIsNotIn_ShouldGiveArgumentNullException))]
    public void IsNotIn_ArgumentNullException<T>([AllowNull] T obj, [AllowNull] ICollection<T> collection)
    {
        Assert.That(() => obj.IsNotIn(collection!), Throws.TypeOf<ArgumentNullException>());
    }

    #endregion IsNotIn

    #region Additional Scenarios

    private class Animal;

    private sealed class Dog : Animal;

    private sealed record Item(int Id);

    #region Derived Types

    [TestCase()]
    public void IsIn_DerivedValueInBaseTypedList_ShouldReturnTrue()
    {
        Dog dog = new();
        Animal value = dog;
        List<Animal> animals = [new Animal(), dog];

        Assert.That(value.IsIn(animals), Is.True);
    }

    [TestCase()]
    public void IsIn_DerivedValueInBaseTypedArray_ShouldReturnTrue()
    {
        Dog dog = new();
        Animal value = dog;
        Animal[] animals = [new Animal(), dog];

        using (Assert.EnterMultipleScope())
        {
            Assert.That(value.IsIn(animals), Is.True);
            Assert.That(value.IsIn(animals, out int index), Is.True);
            Assert.That(index, Is.EqualTo(1));
        }
    }

    [TestCase()]
    public void IsIn_DerivedValueNotInBaseTypedCollection_ShouldReturnFalse()
    {
        Animal value = new Dog();
        List<Animal> list = [new Animal(), new Dog()];
        Animal[] array = [new Animal(), new Dog()];

        using (Assert.EnterMultipleScope())
        {
            Assert.That(value.IsIn(list), Is.False);
            Assert.That(value.IsIn(array), Is.False);
            Assert.That(value.IsNotIn(list), Is.True);
            Assert.That(value.IsNotIn(array), Is.True);
        }
    }

    [TestCase()]
    public void IsIn_ReferenceValueInBaseTypedListWithClassInInterfaceList_ShouldReturnTrue()
    {
        IEquatable<string> value = "test";
        List<IEquatable<string>> list = ["allo", "test"];

        using (Assert.EnterMultipleScope())
        {
            Assert.That(value.IsIn(list), Is.True);
            Assert.That(value.IsIn(list, out int index), Is.True);
            Assert.That(index, Is.EqualTo(1));
        }
    }

    #endregion Derived Types

    #region Nullable Values

    [TestCase()]
    public void IsIn_NullableValueInNullableList_ShouldWork()
    {
        short? present = 2;
        short? missing = 9;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(present.IsIn(nullShortList), Is.True);
            Assert.That(missing.IsIn(nullShortList), Is.False);
            Assert.That(present.IsIn(nullShortList, out int index), Is.True);
            Assert.That(index, Is.EqualTo(1));
            Assert.That(missing.IsIn(nullShortList, out int missingIndex), Is.False);
            Assert.That(missingIndex, Is.EqualTo(-1));
            Assert.That(present.IsNotIn(nullShortList), Is.False);
            Assert.That(missing.IsNotIn(nullShortList), Is.True);
        }
    }

    [TestCase()]
    public void IsIn_NullableValueInNullableArray_ShouldWork()
    {
        short? present = 3;
        short? missing = 9;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(present.IsIn(nullShortArr), Is.True);
            Assert.That(missing.IsIn(nullShortArr), Is.False);
            Assert.That(present.IsIn(nullShortArr, out int index), Is.True);
            Assert.That(index, Is.EqualTo(2));
            Assert.That(present.IsNotIn(nullShortArr), Is.False);
            Assert.That(missing.IsNotIn(nullShortArr), Is.True);
        }
    }

    #endregion Nullable Values

    #region Type Mismatch (array covariance)

    [TestCase()]
    public void IsIn_ValueOfDifferentTypeThanArrayElements_ShouldThrowArgumentException()
    {
        object value = 5;
        string[] strings = ["a", "b"];

        Assert.That(() => value.IsIn(strings), Throws.TypeOf<ArgumentException>());
    }

    [TestCase()]
    public void IsIn_WithIndex_ValueOfDifferentTypeThanArrayElements_ShouldThrowArgumentException()
    {
        object value = 5;
        string[] strings = ["a", "b"];

        Assert.That(() => value.IsIn(strings, out _), Throws.TypeOf<ArgumentException>());
    }

    [TestCase()]
    public void IsNotIn_ValueOfDifferentTypeThanArrayElements_ShouldThrowArgumentException()
    {
        object value = 5;
        string[] strings = ["a", "b"];

        Assert.That(() => value.IsNotIn(strings), Throws.TypeOf<ArgumentException>());
    }

    #endregion Type Mismatch (array covariance)

    #region Dictionary

    [TestCase()]
    public void IsIn_WithDictionary_ShouldThrowArgumentException()
    {
        Dictionary<string, int> dictionary = new() { ["a"] = 1 };
        KeyValuePair<string, int> value = new("a", 1);

        Assert.That(() => value.IsIn(dictionary), Throws.TypeOf<ArgumentException>());
    }

    [TestCase()]
    public void IsIn_WithIndex_WithDictionary_ShouldThrowArgumentException()
    {
        Dictionary<string, int> dictionary = new() { ["a"] = 1 };
        KeyValuePair<string, int> value = new("a", 1);

        Assert.That(() => value.IsIn(dictionary, out _), Throws.TypeOf<ArgumentException>());
    }

    [TestCase()]
    public void IsNotIn_WithDictionary_ShouldThrowArgumentException()
    {
        Dictionary<string, int> dictionary = new() { ["a"] = 1 };
        KeyValuePair<string, int> value = new("a", 1);

        Assert.That(() => value.IsNotIn(dictionary), Throws.TypeOf<ArgumentException>());
    }

    #endregion Dictionary

    #region Empty Collections

    [TestCase()]
    public void IsIn_WithEmptyCollections_ShouldReturnFalse()
    {
        int[] array = [];
        List<int> list = [];
        HashSet<int> set = [];

        using (Assert.EnterMultipleScope())
        {
            Assert.That(1.IsIn(array), Is.False);
            Assert.That(1.IsIn(list), Is.False);
            Assert.That(1.IsIn(set), Is.False);
        }
    }

    [TestCase()]
    public void IsIn_WithIndex_WithEmptyCollections_ShouldReturnFalseAndMinusOne()
    {
        int[] array = [];
        List<int> list = [];
        HashSet<int> set = [];

        using (Assert.EnterMultipleScope())
        {
            Assert.That(1.IsIn(array, out int arrayIndex), Is.False);
            Assert.That(arrayIndex, Is.EqualTo(-1));
            Assert.That(1.IsIn(list, out int listIndex), Is.False);
            Assert.That(listIndex, Is.EqualTo(-1));
            Assert.That(1.IsIn(set, out int setIndex), Is.False);
            Assert.That(setIndex, Is.EqualTo(-1));
        }
    }

    [TestCase()]
    public void IsNotIn_WithEmptyCollections_ShouldReturnTrue()
    {
        int[] array = [];
        List<int> list = [];
        HashSet<int> set = [];

        using (Assert.EnterMultipleScope())
        {
            Assert.That(1.IsNotIn(array), Is.True);
            Assert.That(1.IsNotIn(list), Is.True);
            Assert.That(1.IsNotIn(set), Is.True);
        }
    }

    #endregion Empty Collections

    #region Index Lookup

    [TestCase()]
    public void IsIn_WithIndex_WithLinkedList_ShouldReturnEnumerationIndex()
    {
        LinkedList<int> linkedList = new([10, 20, 30]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(10.IsIn(linkedList, out int first), Is.True);
            Assert.That(first, Is.EqualTo(0));
            Assert.That(30.IsIn(linkedList, out int last), Is.True);
            Assert.That(last, Is.EqualTo(2));
            Assert.That(40.IsIn(linkedList, out int missing), Is.False);
            Assert.That(missing, Is.EqualTo(-1));
        }
    }

    [TestCase()]
    public void IsIn_WithIndex_WithSortedSet_ShouldReturnSortedIndex()
    {
        SortedSet<int> sortedSet = [30, 10, 20];

        Assert.That(30.IsIn(sortedSet, out int index), Is.True);
        Assert.That(index, Is.EqualTo(2));
    }

    [TestCase()]
    public void IsIn_WithIndex_WithHashSet_ShouldFindValue()
    {
        HashSet<int> set = [10, 20, 30];

        using (Assert.EnterMultipleScope())
        {
            Assert.That(20.IsIn(set, out int index), Is.True);
            Assert.That(index, Is.InRange(0, 2));
            Assert.That(99.IsIn(set, out int missingIndex), Is.False);
            Assert.That(missingIndex, Is.EqualTo(-1));
        }
    }

    [TestCase()]
    public void IsIn_WithIndex_WithDuplicates_ShouldReturnFirstIndex()
    {
        List<int> list = [5, 7, 7, 9];
        int[] array = [5, 7, 7, 9];
        LinkedList<int> linkedList = new([5, 7, 7, 9]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(7.IsIn(list, out int listIndex), Is.True);
            Assert.That(listIndex, Is.EqualTo(1));
            Assert.That(7.IsIn(array, out int arrayIndex), Is.True);
            Assert.That(arrayIndex, Is.EqualTo(1));
            Assert.That(7.IsIn(linkedList, out int linkedListIndex), Is.True);
            Assert.That(linkedListIndex, Is.EqualTo(1));
        }
    }

    #endregion Index Lookup

    #region Equality Semantics

    [TestCase()]
    public void IsIn_WithStrings_ShouldBeCaseSensitive()
    {
        List<string> list = ["test"];

        using (Assert.EnterMultipleScope())
        {
            Assert.That("test".IsIn(list), Is.True);
            Assert.That("TEST".IsIn(list), Is.False);
            Assert.That("TEST".IsNotIn(list), Is.True);
        }
    }

    [TestCase()]
    public void IsIn_WithRecords_ShouldUseValueEquality()
    {
        List<Item> list = [new Item(1), new Item(2)];
        Item[] array = [new Item(1), new Item(2)];

        using (Assert.EnterMultipleScope())
        {
            Assert.That(new Item(2).IsIn(list), Is.True);
            Assert.That(new Item(2).IsIn(list, out int listIndex), Is.True);
            Assert.That(listIndex, Is.EqualTo(1));
            Assert.That(new Item(2).IsIn(array, out int arrayIndex), Is.True);
            Assert.That(arrayIndex, Is.EqualTo(1));
            Assert.That(new Item(3).IsIn(list), Is.False);
            Assert.That(new Item(3).IsNotIn(array), Is.True);
        }
    }

    #endregion Equality Semantics

    #region Null Arguments (out overload)

    [TestCase()]
    public void IsIn_WithIndex_WithNullValue_ShouldThrowArgumentNullException()
    {
        string? value = null;
        List<string?> list = ["a"];

        Assert.That(() => value.IsIn(list, out _), Throws.TypeOf<ArgumentNullException>());
    }

    [TestCase()]
    public void IsIn_WithIndex_WithNullCollection_ShouldThrowArgumentNullException()
    {
        ICollection<int>? collection = null;

        Assert.That(() => 1.IsIn(collection!, out _), Throws.TypeOf<ArgumentNullException>());
    }

    #endregion Null Arguments (out overload)

    #endregion Additional Scenarios
}