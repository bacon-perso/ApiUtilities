using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Bacon.Utilities;

/// <summary>
/// Provides geniric validations
/// </summary>
public static partial class Validators
{
    #region Password Validations

    /// <summary>
    /// Validate if a password has illegal characters
    /// </summary>
    /// <param name="password">A System.String that represents the password to be validated</param>
    /// <param name="minPasswordLength">An unsigned System.Byte that represents the minimum length of the required password.</param>
    /// <param name="minRequiredNonAlphanumericCharacters">A System.Byte that represents the minimum required non alphanumeric characters.</param>
    /// <returns>A System.Boolean indicating whether the password is valid or not.</returns>
    /// <exception cref="ArgumentNullException">The password cannot be null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The minimum password length cannot be 0.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The maximum password length cannot exceed 256 characters.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The minimum password length cannot be bigger than the maximum password length.</exception>
    /// <exception cref="ArgumentException">The password cannot be empty.</exception>
    /// <exception cref="ArgumentException">The password cannot be made of only white spaces.</exception>
    public static bool ValidatePasswordComplexity(string password, byte minPasswordLength, byte minRequiredNonAlphanumericCharacters)
    {
        return ValidatePasswordComplexity(password, minPasswordLength, 256, 0, 0, 0, minRequiredNonAlphanumericCharacters);
    }

    /// <summary>
    /// Validate if a password has illegal characters
    /// </summary>
    /// <param name="password">A System.String that represents the password to be validated</param>
    /// <param name="minPasswordLength">An unsigned System.Byte that represents the minimum length of the required password</param>
    /// <param name="maxPasswordLength">A signed 16-bit integer that represents the maximum length of the required password, up to 256 characters.</param>
    /// <param name="minimumUpperCaseCharacters">An unsigned System.Byte that represents the minimum required upper case characters.</param>
    /// <param name="minimumLowerCaseCharacters">An unsigned System.Byte that represents the minimum required lower case characters.</param>
    /// <param name="minimumNumericalCharacters">An unsigned System.Byte that represents the minimum required numeric characters.</param>
    /// <param name="minimumSpecialCharacters">An unsigned System.Byte that represents the minimum required non alphanumeric characters.</param>
    /// <returns>A System.Boolean indicating whether the password is valid or not.</returns>
    /// <exception cref="ArgumentNullException">The password cannot be null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The minimum password length cannot be 0.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The maximum password length cannot exceed 256 characters.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The minimum password length cannot be bigger than the maximum password length.</exception>
    /// <exception cref="ArgumentException">The password cannot be empty.</exception>
    /// <exception cref="ArgumentException">The password cannot be made of only white spaces.</exception>
    public static bool ValidatePasswordComplexity(string password, byte minPasswordLength, ushort maxPasswordLength, byte minimumUpperCaseCharacters, byte minimumLowerCaseCharacters, byte minimumNumericalCharacters, byte minimumSpecialCharacters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password, nameof(password));

        if (minPasswordLength == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minPasswordLength), "The minimum password length cannot be 0.");
        }

        if (maxPasswordLength > 256)
        {
            throw new ArgumentOutOfRangeException(nameof(maxPasswordLength), "The maximum password length cannot exceed 256 characters.");
        }

        //if  the min password length is bigger than the max passowrd, throw an exception
        if (minPasswordLength > maxPasswordLength)
        {
            throw new ArgumentOutOfRangeException(nameof(minPasswordLength), "The minimum password length cannot be bigger than the maximum password length.");
        }

        int upper = 0;
        int lower = 0;
        int number = 0;
        foreach (char character in password)
        {
            if (char.IsUpper(character))
            {
                upper++;
                continue;
            }

            if (char.IsLower(character))
            {
                lower++;
                continue;
            }

            if (char.IsDigit(character))
            {
                number++;
            }
        }

        // Special is "none of the above".
        int special = (password.Length - upper - lower - number);

        if (password.Length < minPasswordLength
            || password.Length > maxPasswordLength
            || upper < minimumUpperCaseCharacters
            || lower < minimumLowerCaseCharacters
            || number < minimumNumericalCharacters
            || special < minimumSpecialCharacters)
        {
            return false;
        }
        // Passed all checks.
        return true;
    }

    #endregion Password Validations

    #region Username Validations

    /// <summary>
    /// Validates a username. Will check if it has a mimimum of 1 characters and a maximum of 256 characters
    /// </summary>
    /// <param name="username">A System.String that represents the username to be validated</param>
    /// <returns>A System.Boolean indicating whether the username is valid or not.</returns>
    /// <exception cref="ArgumentNullException">The username cannot be null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The minimum password length cannot be bigger than the maximum password length.</exception>
    /// <exception cref="ArgumentException">The username cannot be empty.</exception>
    /// <exception cref="ArgumentException">The username cannot be made of only white spaces.</exception>
    public static bool ValidateUsername(string username)
    {
        return ValidateUsername(username, 1, 256);
    }

    /// <summary>
    /// Validates a username
    /// </summary>
    /// <param name="username">A System.String that represents the username to be validated</param>
    /// <param name="minUsernameLength">A System.Byte that represents the minimum length accepted for a username</param>
    /// <param name="maxUsernameLength">A System.UInt16 that represents the maximum length accepted for a username</param>
    /// <returns>A System.Boolean indicating whether the username is valid or not.</returns>
    /// <exception cref="ArgumentNullException">The username cannot be null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The minimum username length cannot be less than 1 characters.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The maximum username length cannot exceed 256 characters.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The minimum username length cannot be bigger than the maximum username length.</exception>
    /// <exception cref="ArgumentException">The username cannot be empty.</exception>
    /// <exception cref="ArgumentException">The username cannot be made of only white spaces.</exception>
    public static bool ValidateUsername(string username, byte minUsernameLength, ushort maxUsernameLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username, nameof(username));

        if (minUsernameLength < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(minUsernameLength), "The minimum username length cannot be less than 1 characters.");
        }

        if (maxUsernameLength > 256)
        {
            throw new ArgumentOutOfRangeException(nameof(maxUsernameLength), "The maximum username length cannot exceed 256 characters.");
        }

        if (minUsernameLength > maxUsernameLength)
        {
            throw new ArgumentOutOfRangeException(nameof(minUsernameLength), "The minimum username length cannot be bigger than the maximum username length.");
        }


        if (username.Length < minUsernameLength || username.Length > maxUsernameLength || username.Any(char.IsWhiteSpace))
        {
            return false;
        }

        return true;
    }

    #endregion Username Validations

    #region Email Validations

    /// <summary>
    /// Validate if a email has illegal character 
    /// </summary>
    /// <param name="email">A System.String that represents the email to be validated</param>
    /// <returns>True if valid, otherwise false</returns>
    /// <exception cref="ArgumentNullException">The email cannot be null.</exception>
    /// <exception cref="ArgumentException">The email cannot be empty.</exception>
    /// <exception cref="ArgumentException">The email cannot be made of only white spaces.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The email must not have more than 256 characters.</exception>
    public static bool ValidateEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email, nameof(email));

        //technically 320 but DB fields are limited to 256 until a customer requests more
        if (email.Length > 256)
        {
            throw new ArgumentOutOfRangeException(nameof(email), "The email must not have more than 256 characters.");
        }

        //The local part and the domain are separated by the last @, since a quoted local part can contain an @.
        //Nothing is trimmed or normalized: white space, comments, display names, angle brackets and trailing dots are all rejected by the recipient and domain validation
        int separatorIndex = email.LastIndexOf('@');

        if (separatorIndex <= 0 || separatorIndex == email.Length - 1)
        {
            return false;
        }

        if (!ValidateEmailRecipient(email[..separatorIndex]))
        {
            return false;
        }

        return ValidateDomain(email[(separatorIndex + 1)..]);
    }

    /// <summary>
    /// Validate an email recipient (local part)
    /// </summary>
    /// <param name="recipient">The recipient or local part of the email</param>
    /// <returns>True if valid, otherwise false</returns>
    public static bool ValidateEmailRecipient(string recipient)
    {
        //check if the recipient is longer than 64 characters
        if (recipient.Length > 64)
        {
            return false;
        }

        return EmailRecipientRegex().IsMatch(recipient);
    }

    #endregion Email Validations

    #region DNS Validations

    /// <summary>
    /// Validate a domain part (all sub-domains)
    /// </summary>
    /// <param name="domain">The domain part of the email</param>
    /// <returns>True if valid, otherwise false</returns>
    public static bool ValidateDomain(string domain)
    {
        ArgumentNullException.ThrowIfNull(domain, nameof(domain));

        //Technically 255, if we make the change to have a max email length of 320 to perfectly match the RFC
        if (domain.Length > 254)
        {
            return false;
        }

        ReadOnlySpan<char> domainSpan = domain.AsSpan();

        //check that each sub domains are not empty and do not exceed 63 characters (RFC 1035)
        foreach (Range range in domainSpan.Split('.'))
        {
            int length = range.GetOffsetAndLength(domainSpan.Length).Length;

            if (length == 0 || length > 63)
            {
                return false;
            }
        }

        return DomainRegex().IsMatch(domain);
    }

    /// <summary>
    /// Validate IP Address
    /// </summary>
    /// <param name="ipAddress">IP Address</param>
    /// <param name="uriHostNameType">IPv4 or IPv6</param>
    /// <returns>True if valid, otherwise false</returns>
    /// <exception cref="ArgumentNullException">The ipAddress cannot be null.</exception>
    public static bool ValidateIpAddress(string ipAddress, UriHostNameType uriHostNameType)
    {
        ArgumentNullException.ThrowIfNull(ipAddress, nameof(ipAddress));

        return uriHostNameType switch
        {
            UriHostNameType.IPv4 => IPAddress.TryParse(ipAddress, out IPAddress? v4)
                                    && v4.AddressFamily == AddressFamily.InterNetwork
                                    && v4.ToString() == ipAddress,
            UriHostNameType.IPv6 => !ipAddress.Contains('[')
                                    && !ipAddress.Contains(']')
                                    && IPAddress.TryParse(ipAddress, out IPAddress? v6)
                                    && v6.AddressFamily == AddressFamily.InterNetworkV6,
            _ => false
        };
    }

    /// <summary>
    /// Validate host name
    /// </summary>
    /// <param name="hostName">hostname</param>
    /// <returns>Host entry if valid, otherwise null. A null, empty or white space host name is never valid</returns>
    public async static Task<IPHostEntry?> ParseHostNameAsync(string hostName)
    {
        //an empty host name would otherwise resolve to the local machine
        if (string.IsNullOrWhiteSpace(hostName))
        {
            return null;
        }

        try
        {
            return await Dns.GetHostEntryAsync(hostName);
        }
        catch
        {
            return null;
        }
    }

    #endregion DNS Validations

    #region Duplicate Validations

    /// <summary>
    /// Validate if items contains duplicated items
    /// Use this function when the type of list is a primitive type
    /// </summary>
    /// <typeparam name="TItem">The generic item</typeparam>
    /// <param name="items">The list of items to validate</param>
    /// <param name="duplicatedValues">Out the duplicated items if found. Null if none</param>
    /// <returns>Whether if collection has duplicated values. If true, then no duplication found, if false, duplicated values found</returns>
    /// <exception cref="ArgumentNullException">The items cannot be null.</exception>
    public static bool TryValidateDuplicatedItems<TItem>(IEnumerable<TItem> items, [NotNullWhen(false)] out IEnumerable<TItem>? duplicatedValues)
    {
        ArgumentNullException.ThrowIfNull(items, nameof(items));

        List<TItem> duplicatedItems = FindDuplicatedKeys(items, static (item) => item);

        if (duplicatedItems.Count != 0)
        {
            duplicatedValues = duplicatedItems;
            return false;
        }

        duplicatedValues = null;
        return true;
    }

    /// <summary>
    /// Validate if items contains duplicated items
    /// Use this function when the type of list is an object
    /// </summary>
    /// <typeparam name="TItem">The generic item</typeparam>
    /// <typeparam name="TSelectedItem">The selected item to validate from the initial items</typeparam>
    /// <param name="items">The list of items to validate</param>
    /// <param name="groupByFunc">The group by function</param>
    /// <param name="selectFunc">The select function</param>
    /// <param name="duplicatedValues">Out the duplicated items if found. Null if none</param>
    /// <returns>Whether if collection has duplicated values. If true, then no duplication found, if false, duplicated values found</returns>
    /// <exception cref="ArgumentNullException">The items cannot be null.</exception>
    /// <exception cref="ArgumentNullException">The group by func cannot be null.</exception>
    /// <exception cref="ArgumentNullException">The select  cannot be null.</exception>
    public static bool TryValidateDuplicatedItems<TItem, TSelectedItem>(IEnumerable<TItem> items, Func<TItem, TSelectedItem> groupByFunc, Func<IGrouping<TSelectedItem, TItem>, TSelectedItem> selectFunc, [NotNullWhen(false)] out IEnumerable<TSelectedItem>? duplicatedValues)
    {
        ArgumentNullException.ThrowIfNull(items, nameof(items));
        ArgumentNullException.ThrowIfNull(groupByFunc, nameof(groupByFunc));
        ArgumentNullException.ThrowIfNull(selectFunc, nameof(selectFunc));

        List<TSelectedItem> duplicateItems = [.. items.GroupBy(groupByFunc).Where(w => w.Count() > 1).Select(selectFunc)];

        if (duplicateItems.Count != 0)
        {
            duplicatedValues = duplicateItems;
            return false;
        }

        duplicatedValues = null;
        return true;
    }

    private static List<TKey> FindDuplicatedKeys<TItem, TKey>(IEnumerable<TItem> items, Func<TItem, TKey> keySelector)
    {
        HashSet<TKey> seen = [];
        HashSet<TKey> duplicated = [];
        List<TKey> firstSeenOrder = [];

        foreach (TItem item in items)
        {
            TKey key = keySelector(item);

            if (seen.Add(key))
            {
                firstSeenOrder.Add(key);
            }
            else
            {
                duplicated.Add(key);
            }
        }

        if (duplicated.Count == 0)
        {
            return [];
        }

        List<TKey> result = new(duplicated.Count);

        foreach (TKey key in firstSeenOrder)
        {
            if (duplicated.Contains(key))
            {
                result.Add(key);
            }
        }

        return result;
    }

    #endregion Duplicate Validations

    #region Stored Proc Content Validations

    /// <summary>
    /// Validate the results from stored procedures (multiple validation functions)
    /// </summary>
    /// <typeparam name="TValidatorKey">The dictionary key type</typeparam>
    /// <typeparam name="TData">The data to validate</typeparam>
    /// <param name="dataTable">Data to validate</param>
    /// <param name="validators">list of functions containing logic to validate</param>
    /// <returns>A dictionary containing all the errors</returns>
    public static IDictionary<TValidatorKey, ICollection<TData>> ValidateCheckStoredProcs<TValidatorKey, TData>(DataTable dataTable, IEnumerable<(TValidatorKey validatorKey, Func<DataRow, (TData data, bool isInvalid)> validator)> validators) where TValidatorKey : notnull
    {
        Dictionary<TValidatorKey, ICollection<TData>> invalidItemDic = [];

        foreach (DataRow dataRow in dataTable.Rows)
        {
            foreach ((TValidatorKey validatorKey, Func<DataRow, (TData data, bool isInvalid)> validator) in validators)
            {
                (TData data, bool isInvalid) = validator(dataRow);

                if (isInvalid)
                {
                    if (!invalidItemDic.TryGetValue(validatorKey, out ICollection<TData>? value))
                    {
                        value = [];
                        invalidItemDic.Add(validatorKey, value);
                    }

                    value.Add(data);
                }
            }
        }

        IEnumerable<KeyValuePair<TValidatorKey, ICollection<TData>>> invalidValidations = invalidItemDic.Where(w => w.Value.Count > 0);

        Dictionary<TValidatorKey, ICollection<TData>> invalidItems = [];
        foreach (KeyValuePair<TValidatorKey, ICollection<TData>> invalidItem in invalidValidations)
        {
            invalidItems.Add(invalidItem.Key, invalidItem.Value);
        }

        return invalidItems;
    }

    #endregion Stored Proc Content Validations

    #region Regex

    //Domain validation regex
    [GeneratedRegex("^(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x5e-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])\\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex DomainRegex();

    //Email recipient validation regex
    [GeneratedRegex("^(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*\")\\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex EmailRecipientRegex();

    #endregion Regex
}