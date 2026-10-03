using System.Collections;

namespace Bacon.Utilities.Extensions;

/// <summary>
/// Generic extensions
/// </summary>
public static class GenericExtensions
{
    extension<T>(T value)
    {
        /// <summary>
        /// Validates if an object is part of a collection
        /// </summary>
        /// <param name="collection">A collection T. Will accept arrays and lists</param>
        /// <returns>Returns true if the object is found in the collection, false if not</returns>
        /// <exception cref="ArgumentNullException">"The object being validated cannot be null."</exception>
        /// <exception cref="ArgumentNullException">"The collection that the object needs to be found with cannot be null."</exception>
        /// <exception cref="ArgumentException">"Collections implementing the IDictionary interface are not accepted."</exception>
        /// <exception cref="ArgumentException">"The type of object is mismatching the type of the collection's arguments."</exception>
        public bool IsIn(ICollection<T> collection)
        {
            ValidateIsIn(value, collection);

            return collection.Contains(value);
        }

        /// <summary>
        /// Validates in an object is part of a collection
        /// </summary>
        /// <param name="collection">A collection of T. Will accept arrays and lists</param>
        /// <param name="index">Returns the index of the first occurrence of the object in the collection. 0 based int, -1 if the object is not found</param>
        /// <returns>Returns true if the object is found in the collection, false if not</returns>
        /// <exception cref="ArgumentNullException">"The object being validated cannot be null."</exception>
        /// <exception cref="ArgumentNullException">"The collection that the object needs to be found with cannot be null."</exception>
        /// <exception cref="ArgumentException">"Collections implementing the IDictionary interface are not accepted."</exception>
        /// <exception cref="ArgumentException">"The type of object is mismatching the type of the collection's arguments."</exception>
        public bool IsIn(ICollection<T> collection, out int index)
        {
            ValidateIsIn(value, collection);

            index = IndexOf(value, collection);

            return index >= 0;
        }

        /// <summary>
        /// Validates in an object is not part of a collection
        /// </summary>
        /// <param name="collection">A collection of T. Will accept arrays and lists</param>
        /// <returns>Returns true if the object is not found in the collection, false if it is</returns>
        /// <exception cref="ArgumentNullException">"The object being validated cannot be null."</exception>
        /// <exception cref="ArgumentNullException">"The collection that the object needs to be found with cannot be null."</exception>
        public bool IsNotIn(ICollection<T> collection)
        {
            return !value.IsIn(collection);
        }

        #region Privates

        private void ValidateIsIn(ICollection<T> collection)
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            ArgumentNullException.ThrowIfNull(collection, nameof(collection));

            if (collection is IDictionary)
            {
                throw new ArgumentException("Collections implementing the IDictionary interface are not accepted.");
            }

            if (collection.GetType().GetElementType() is Type elementType && !elementType.IsAssignableFrom(value.GetType()))
            {
                throw new ArgumentException("The type of object is mismatching the type of the collection's arguments.");
            }
        }

        #endregion
    }

    #region Private

    private static int IndexOf<T>(T value, ICollection<T> collection)
    {
        if (collection is IList<T> list)
        {
            return list.IndexOf(value);
        }

        int index = 0;

        foreach (T item in collection)
        {
            if (EqualityComparer<T>.Default.Equals(item, value))
            {
                return index;
            }

            index++;
        }

        return -1;
    }

    #endregion Private
}