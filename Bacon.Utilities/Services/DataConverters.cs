using System.Collections;
using System.Data;
using System.Globalization;
using System.Text;

namespace Bacon.Utilities.Services;

/// <summary>
/// Contains functions to help convert data
/// </summary>
public static class DataConverters
{
    #region Dictionary to datatable

    /// <summary>
    /// Converts a dictionary into a DataTable
    /// </summary>
    /// <typeparam name="TKey">A non null unique key</typeparam>
    /// <typeparam name="TValue">A value that can contain an IEnumerable</typeparam>
    /// <param name="dictionary">A dictionary</param>
    /// <returns>A DataTable containing the columns "Key" and "Value" matching the types received</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static DataTable DictionaryToDataTable<TKey, TValue>(Dictionary<TKey, TValue> dictionary) where TKey : notnull
    {
        Type type = typeof(TValue);
        Type? underlyingNullableType = Nullable.GetUnderlyingType(type);

        if (underlyingNullableType != null)
        {
            type = underlyingNullableType;
        }

        if (type.GetInterfaces().Any(a => a == typeof(IDictionary)))
        {
            throw new InvalidOperationException("The value type cannot be assignable from an IDictionary");
        }

        Type? underlyingType = null;
        if (type.IsGenericType)
        {
            underlyingType = type.GetGenericArguments()[0];

            underlyingNullableType = Nullable.GetUnderlyingType(underlyingType);

            if (underlyingNullableType != null)
            {
                underlyingType = underlyingNullableType;
            }
        }

        if (underlyingType != null && !underlyingType.Equals(typeof(string)) && underlyingType.GetInterfaces().Any(a => a == typeof(IEnumerable)))
        {
            throw new InvalidOperationException("The value type cannot be assignable from an IEnumerable<IEnumerable>");
        }

        DataTable dataTable = new();
        dataTable.Columns.Add("Key", typeof(TKey));
        dataTable.Columns.Add("Value", underlyingType ?? type);

        foreach (KeyValuePair<TKey, TValue> keyValuePair in dictionary)
        {
            if (underlyingType == null || keyValuePair.Value == null)
            {
                DataRow emptyValueRow = dataTable.NewRow();

                emptyValueRow["Key"] = keyValuePair.Key;
                emptyValueRow["Value"] = keyValuePair.Value == null ? DBNull.Value : keyValuePair.Value;

                dataTable.Rows.Add(emptyValueRow);

                continue;
            }

            IEnumerable objects = (keyValuePair.Value as IEnumerable)!;

            foreach (object value in objects)
            {
                DataRow row = dataTable.NewRow();

                row["Key"] = keyValuePair.Key;
                row["Value"] = value ?? DBNull.Value;

                dataTable.Rows.Add(row);
            }
        }

        return dataTable;
    }

    #endregion Dictionary to datatable

    #region List to datatable

    /// <summary>
    /// Converts an object to a datatable
    /// </summary>
    /// <param name="value">A single byte</param>
    /// <returns>A datatable containing the content of the value. Columns name is "Item" and will match the data type of TValue</returns>
    /// <exception cref="InvalidOperationException">The value type cannot be assignable from an IEnumerable</exception>
    /// <exception cref="ArgumentNullException">The values cannot contain a null value</exception>
    public static DataTable ItemToDataTable<TValue>(TValue value) where TValue : notnull => ListToDataTable([value]);

    /// <summary>
    /// Converts a list of binary numbers to a datatable
    /// </summary>
    /// <param name="values">A list of long</param>
    /// <returns>A datatable containing the content of the list. Columns name is "Item" and will match the data type of TValue</returns>
    /// <exception cref="InvalidOperationException">The value type cannot be assignable from an IEnumerable</exception>
    /// <exception cref="ArgumentNullException">The values cannot contain a null value</exception>
    public static DataTable ListToDataTable<TValue>(IEnumerable<TValue> values) where TValue : notnull
    {
        ArgumentNullException.ThrowIfNull(values, nameof(values));

        Type type = typeof(TValue);
        type = Nullable.GetUnderlyingType(type) ?? type;

        if (!type.Equals(typeof(string)) && type.GetInterfaces().Any(a => a == typeof(IEnumerable)))
        {
            throw new InvalidOperationException("The value type cannot be assignable from an IEnumerable<IEnumerable>");
        }

        DataTable dataTable = new();
        dataTable.Columns.Add("Item", type);

        foreach (TValue item in values)
        {
            if (item is null)
            {
                throw new ArgumentNullException(null, "The values cannot contain a null value");
            }

            DataRow row = dataTable.NewRow();

            row["Item"] = item;

            dataTable.Rows.Add(row);
        }

        return dataTable;
    }

    #endregion List to datatable

    #region DateTime

    /// <summary>
    /// Converts an object into a Nullable Datetime
    /// Accepts null, DBNull, DateTime, DateTimeOffset and string values. Any other type is rejected
    /// </summary>
    /// <param name="value">The value to convert into a datetime</param>
    /// <param name="dateTimeKind">The datetime Kind. Defaults to UTC</param>
    /// <param name="formatProvider">The format provide used to parse a string value</param>
    /// <exception cref="FormatException">The value is not a DateTime, or is of a type that cannot represent one.</exception>
    /// <returns></returns>
    public static DateTime? ToDateTime(object? value, DateTimeKind dateTimeKind = DateTimeKind.Utc, IFormatProvider? formatProvider = null)
    {
        if (value == null || value == DBNull.Value)
        {
            return null;
        }

        DateTime dateTime;
        switch(value)
        {
            case DateTime dt:
                dateTime = dt;
                break;

            case DateTimeOffset dto:
                dateTime = dto.DateTime;
                break;

            case string text:
                if (!DateTime.TryParse(text, formatProvider, DateTimeStyles.None, out dateTime))
                {
                    throw new FormatException("The value is not a DateTime.");
                }

                break;

            default:
                //numbers and other types must not be parsed through their string representation (3.14 would become March 14)
                throw new FormatException("The value is not a DateTime.");
        }

        return DateTime.SpecifyKind(dateTime, dateTimeKind);
    }

    #endregion DateTime

    #region Base64

    /// <summary>
    /// Encodes a plain text string to base64 a base64 string
    /// Passing a null value will throw an exception.
    /// </summary>
    /// <param name="plainText">a plain text string</param>
    /// <returns>A base64 string. If empty string, then will return empty string</returns>
    /// <exception cref="ArgumentNullException">A null value cannot be encoded</exception>
    public static string Base64Encode(string plainText)
    {
        ArgumentNullException.ThrowIfNull(plainText, nameof(plainText));

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
    }

    /// <summary>
    /// Returns a plain text string from a base 64 string
    /// </summary>
    /// <param name="base64EncodedData">the base64 string value.</param>
    /// <returns>Returns a pain text string. If the value was empty, then empty string value.</returns>
    /// <exception cref="ArgumentNullException">The value to decode cannot be null.</exception>
    /// <exception cref="FormatException">The value was not encoded in base 64, or is made of only white spaces. White spaces around or inside a valid base 64 value are ignored.</exception>
    public static string Base64Decode(string base64EncodedData)
    {
        ArgumentNullException.ThrowIfNull(base64EncodedData, nameof(base64EncodedData));

        if (base64EncodedData == string.Empty)
        {
            return string.Empty;
        }

        //white space is ignored by the decoder, so a white space only value would silently decode to nothing. Only an empty string represents an empty value
        if (string.IsNullOrWhiteSpace(base64EncodedData))
        {
            throw new FormatException("The value was not encoded in base 64.");
        }

        try
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedData));
        }
        catch (FormatException)
        {
            throw new FormatException("The value was not encoded in base 64.");
        }
    }

    #endregion Base64

    #region Boolean

    /// <summary>
    /// Try to parse a string to a boolean.
    /// The value 1 or yes is converted to true
    /// The value 0 or no is converted to false
    /// </summary>
    /// <param name="value">String value to parse</param>
    /// <param name="result">returns the result of the parsing</param>
    /// <returns>If the parsing succeeded or failed</returns>
    public static bool TryParseBoolean(string value, out bool result)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));

        switch(value)
        {
            case "1":
                result = true;
                return true;

            case "0":
                result = false;
                return true;

            default:
                return bool.TryParse(value, out result);
        }
    }

    #endregion Boolean
}