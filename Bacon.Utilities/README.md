# Bacon.Utilities

A lightweight collection of general-purpose .NET helpers: string/date/generic extension methods, cryptography helpers, data conversion utilities, and validators (passwords, usernames, emails, domains, IP addresses, duplicate detection). Targets `net10.0`.

## Installation

Reference the project directly, or package it as a NuGet package:

```xml
<ItemGroup>
  <ProjectReference Include="..\Bacon.Utilities\Bacon.Utilities.csproj" />
</ItemGroup>
```

### Dependencies

- [`libphonenumber-csharp`](https://www.nuget.org/packages/libphonenumber-csharp) — not called by `Bacon.Utilities` itself. It is referenced here so that it flows transitively to `Bacon.ApiUtilities`, whose `PhoneNumberFormatAttribute` uses it for phone number validation.

`Validators.ValidateEmail` has no third-party dependency.

## Extensions

### `DateExtensions`

```csharp
using Bacon.Utilities.Extensions;

bool inRange = someDate.IsBetweenDates(dateFrom, dateTo); // inclusive on both bounds
bool afterStart = someDate.IsBetweenDates(dateFrom, null); // a null bound is open-ended
bool anyDate = someDate.IsBetweenDates(null, null);        // both null => always true
```

### `GenericExtensions`

```csharp
using Bacon.Utilities.Extensions;

bool found = value.IsIn(collection);
bool foundWithIndex = value.IsIn(collection, out int index); // index is -1 if not found
bool notFound = value.IsNotIn(collection);
```

Works with any `ICollection<T>` (arrays, lists, sets, linked lists...) and uses the default equality of `T`. A derived value can be searched in a base-typed collection, and nullable values in nullable collections.

Throws `ArgumentNullException` for a null value/collection, and `ArgumentException` if the collection is an `IDictionary`, or if it is an array whose element type is incompatible with `value` (possible through array covariance, for example an `object` searched in a `string[]`).

### `StringExtensions`

```csharp
using Bacon.Utilities.Extensions;

string masked = "4111111111111111".MaskString(maskStart: 4, maskEnd: 16, maskChar: '*');
// "4111************"
```

## Services

### `Crypto`

```csharp
using Bacon.Utilities.Services;

string sha256 = Crypto.Sha256("input");
string sha512 = Crypto.Sha512("input");

string password = Crypto.GeneratePassword(
    useLowercase: true, useUppercase: true, useNumbers: true, useSpecial: true, passwordSize: 16);

byte[] key = /* 32-byte AES-256 key */;
string encrypted = Crypto.Encrypt("plain text", key); // AES-256-GCM, random nonce embedded in output
string decrypted = Crypto.Decrypt(encrypted, key);
```

The encrypted value is the base64 of `nonce (12 bytes) | ciphertext | authentication tag (16 bytes)`. `Decrypt` throws `FormatException` for a value that is not valid base64, `ArgumentException` for one that is too short, and `AuthenticationTagMismatchException` (a `CryptographicException`) when the key is wrong or the data was tampered with. `Sha256`/`Sha512` return an empty string for null, empty or white space input.

### `DataConverters`

```csharp
using Bacon.Utilities.Services;
using System.Data;

// Dictionary/List -> DataTable
DataTable dt1 = DataConverters.DictionaryToDataTable(myDictionary); // columns: "Key", "Value"
DataTable dt2 = DataConverters.ListToDataTable(myList);             // column: "Item"
DataTable dt3 = DataConverters.ItemToDataTable(myValue);             // single-row DataTable

// DateTime helper
DateTime? parsed = DataConverters.ToDateTime(dbValue); // null/DBNull => null, defaults to DateTimeKind.Utc
DateTime? parsedLocal = DataConverters.ToDateTime("01/02/2020", DateTimeKind.Local, new CultureInfo("en-GB"));

// Base64
string encoded = DataConverters.Base64Encode("plain text");
string decoded = DataConverters.Base64Decode(encoded);

// Boolean parsing ("1" and "0" in addition to what bool.TryParse accepts)
bool parsedOk = DataConverters.TryParseBoolean("1", out bool result); // true, result = true
```

`ToDateTime` accepts `null`, `DBNull`, `DateTime`, `DateTimeOffset` (its offset is dropped) and `string` values. The kind is applied to the result without converting the time. Any other type, including numbers, throws `FormatException`, and so does an unparseable string.

`Base64Decode` returns an empty string for `""`, ignores white space inside or around a valid value (line-wrapped base64 works), and throws `FormatException` for invalid input, including a value made only of white space.

### `Validators`

```csharp
using Bacon.Utilities;

bool validPassword = Validators.ValidatePasswordComplexity(password, minPasswordLength: 8, minRequiredNonAlphanumericCharacters: 1);
// or the fully parameterized overload:
bool validPassword2 = Validators.ValidatePasswordComplexity(
    password, minPasswordLength: 8, maxPasswordLength: 64,
    minimumUpperCaseCharacters: 1, minimumLowerCaseCharacters: 1,
    minimumNumericalCharacters: 1, minimumSpecialCharacters: 1);

bool validUsername = Validators.ValidateUsername(username); // 1-256 chars, no whitespace
bool validUsername2 = Validators.ValidateUsername(username, minUsernameLength: 4, maxUsernameLength: 32);

bool validEmail = Validators.ValidateEmail(email); // validated as received, max 256 chars
bool validRecipient = Validators.ValidateEmailRecipient("first.last"); // local part only
bool validDomain = Validators.ValidateDomain(domain);
bool validIp = Validators.ValidateIpAddress(ip, UriHostNameType.IPv4); // or .IPv6

IPHostEntry? hostEntry = await Validators.ParseHostNameAsync(hostName); // null if it cannot be resolved

// Duplicates: null values are supported and reported like any other value
bool noDuplicates = Validators.TryValidateDuplicatedItems(items, out IEnumerable<int>? duplicates);
bool noDuplicateNames = Validators.TryValidateDuplicatedItems(
    people, p => p.Name, group => group.Key, out IEnumerable<string>? duplicatedNames);

// Validate rows coming back from stored procedures against multiple rules at once
IDictionary<string, ICollection<string>> errors = Validators.ValidateCheckStoredProcs(
    dataTable,
    [
        ("RuleA", row => (data: row["Column"].ToString()!, isInvalid: /* condition */ false)),
    ]);
```

Most validation methods throw `ArgumentNullException`/`ArgumentException`/`ArgumentOutOfRangeException` on invalid input parameters (not on invalid data) — check the XML doc comments on each method for the exact contract.

Validation rules worth knowing:

- **Email:** the address is checked exactly as received. It is split on the last `@`, then the local part goes through `ValidateEmailRecipient` (at most 64 characters, dot-atom or quoted string) and the domain through `ValidateDomain`. Nothing is trimmed or normalized, so surrounding white space, display names (`Joe <a@b.com>`), angle brackets, comments, trailing dots and line breaks all make the address invalid. Addresses over 256 characters throw `ArgumentOutOfRangeException`.
- **Domain:** each label is 1-63 characters and the whole domain at most 254. A bracketed address literal (`[1.2.3.4]`) is accepted.
- **IP address:** IPv4 must be a strict dotted quad (no shorthand such as `1.2.3` or `127.1`, no hexadecimal, no leading zeros, no surrounding white space). IPv6 accepts the standard compressed forms (`::1`, `2001:db8::1`) and IPv4-mapped addresses, but not brackets or ports (`[::1]:80`).
- **Host name:** `ParseHostNameAsync` performs a real DNS lookup. Null, empty or white space names return `null` instead of resolving to the local machine.
- **Duplicates:** results come back in order of first occurrence, and the input is enumerated once. The overload taking `groupByFunc` and `selectFunc` gives full control over what is compared and what is reported.

## Testing

Unit tests live in `Bacon.Utilities.Tests` and cover the extensions, services and validators above. Run them with:

```
dotnet test Bacon.Utilities.Tests
```

The host name tests in `ValidatorsTests` include a few that resolve real domains (`google.com`, `protonmail.com`), so they need network access.
