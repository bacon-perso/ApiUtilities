using System.Security.Cryptography;
using System.Text;

namespace Bacon.Utilities.Services;

/// <summary>
/// Crypto helper class.
/// </summary>
public static class Crypto
{
    /// <summary>
    /// Creates a SHA256 hash of the specified input.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>A hash</returns>
    public static string Sha256(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
    }

    /// <summary>
    /// Creates a SHA512 hash of the specified input.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>A hash</returns>
    public static string Sha512(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        return Convert.ToBase64String(SHA512.HashData(Encoding.UTF8.GetBytes(input)));
    }

    /// <summary>
    /// Generate Random Password
    /// </summary>
    /// <param name="useLowercase">Use lower case characters (a-z)</param>
    /// <param name="useUppercase">Use upper case characters (A-Z)</param>
    /// <param name="useNumbers">Use numbers (0-9)</param>
    /// <param name="useSpecial">Use special characters (!@$%^&amp;*()#.-_~)</param>
    /// <param name="passwordSize">Size of the password</param>
    /// <exception cref="ArgumentNullException">At least a single setting must be turned on to generate a password</exception>
    /// <returns>A randomly generated password</returns>
    public static string GeneratePassword(bool useLowercase, bool useUppercase, bool useNumbers, bool useSpecial, ushort passwordSize)
    {
        if (!(useLowercase || useUppercase || useNumbers || useSpecial))
        {
            throw new ArgumentException("At least a single setting must be turned on to generate a password");
        }

        string characterSet = string.Concat(
            useLowercase ? "abcdefghijklmnopqrstuvwxyz" : "",
            useUppercase ? "ABCDEFGHIJKLMNOPQRSTUVWXYZ" : "",
            useNumbers ? "0123456789" : "",
            useSpecial ? "!@$%^&*()#.-_~" : "");

        return RandomNumberGenerator.GetString(characterSet, passwordSize);
    }

    /// <summary>
    /// Encrypts a plain text string using AES-256-GCM.
    /// The nonce is randomly generated and embedded in the output.
    /// </summary>
    /// <param name="plainText">The string to encrypt</param>
    /// <param name="key">The 32 bytes key (AES-256)</param>
    /// <exception cref="ArgumentException">The plain text cannot be null or empty</exception>
    /// <exception cref="ArgumentNullException">The key must not be null</exception>
    /// <exception cref="ArgumentOutOfRangeException">The key size must be 32 bytes</exception>
    /// <returns>A base64 string containing the nonce, ciphertext, and authentication tag</returns>
    public static string Encrypt(string plainText, byte[] key)
    {
        ArgumentException.ThrowIfNullOrEmpty(plainText, nameof(plainText));
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentOutOfRangeException.ThrowIfNotEqual(key.Length, 32, nameof(key));

        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

        int nonceSize = AesGcm.NonceByteSizes.MaxSize;
        int tagSize = AesGcm.TagByteSizes.MaxSize;

        byte[] result = new byte[nonceSize + plainBytes.Length + tagSize];

        Span<byte> nonce = result.AsSpan(0, nonceSize);
        Span<byte> cipher = result.AsSpan(nonceSize, plainBytes.Length);
        Span<byte> tag = result.AsSpan(nonceSize + plainBytes.Length, tagSize);

        RandomNumberGenerator.Fill(nonce);

        using AesGcm aesGcm = new(key, tagSize);
        aesGcm.Encrypt(nonce, plainBytes, cipher, tag);

        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// Decrypts a string encrypted with <see cref="Encrypt(string, byte[])"/> using AES-256-GCM.
    /// </summary>
    /// <param name="encryptedText">The base64 encoded encrypted text</param>
    /// <param name="key">The 32 bytes key (AES-256)</param>
    /// <exception cref="ArgumentException">The encrypted text cannot be null or empty</exception>
    /// <exception cref="ArgumentException">The encrypted text is invalid</exception>
    /// <exception cref="FormatException">The encrypted text is not a valid base64 string</exception>
    /// <exception cref="ArgumentNullException">The key must not be null</exception>
    /// <exception cref="ArgumentOutOfRangeException">The key size must be 32 bytes</exception>
    /// <exception cref="CryptographicException">The data is invalid or the authentication tag does not match</exception>
    /// <returns>The decrypted string</returns>
    public static string Decrypt(string encryptedText, byte[] key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(encryptedText, nameof(encryptedText));
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentOutOfRangeException.ThrowIfNotEqual(key.Length, 32, nameof(key));

        ReadOnlySpan<byte> data = Convert.FromBase64String(encryptedText);

        int nonceSize = AesGcm.NonceByteSizes.MaxSize;
        int tagSize = AesGcm.TagByteSizes.MaxSize;
        int cipherSize = data.Length - nonceSize - tagSize;

        if (cipherSize <= 0)
        {
            throw new ArgumentException("The encrypted text is invalid", nameof(encryptedText));
        }

        ReadOnlySpan<byte> nonce = data[..nonceSize];
        ReadOnlySpan<byte> cipherBytes = data[nonceSize..(nonceSize + cipherSize)];
        ReadOnlySpan<byte> tag = data[(nonceSize + cipherSize)..];

        byte[] plainBytes = new byte[cipherSize];

        using AesGcm aes = new(key, tagSize);
        aes.Decrypt(nonce, cipherBytes, tag, plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }
}