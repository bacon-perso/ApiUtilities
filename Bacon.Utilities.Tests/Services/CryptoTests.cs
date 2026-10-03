using Bacon.Utilities.Services;
using System.Security.Cryptography;

namespace Bacon.Utilities.Tests.Services;

[TestFixture]
internal sealed class CryptoTests
{
    private static readonly byte[] _validKey = RandomNumberGenerator.GetBytes(32);

    #region Sha256

    [TestCase("", ExpectedResult = "")]
    [TestCase("   ", ExpectedResult = "")]
    [TestCase("\t\r\n", ExpectedResult = "")]
    [TestCase("toto", ExpectedResult = "MfemXjFVhqwZi9eYtmKc5JA9CJlHbVdBqfMuLlIbamY=")]
    public string Sha256_Valid(string value)
    {
        return Crypto.Sha256(value);
    }

    [TestCase(null, ExpectedResult = "")]
    public string Sha256_WithNull_ShouldReturnEmptyString(string? value)
    {
        return Crypto.Sha256(value!);
    }

    [TestCase("é")]
    [TestCase("日本語 😀")]
    public void Sha256_WithUnicode_ShouldMatchUtf8Hash(string value)
    {
        string expected = Convert.ToBase64String(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value)));

        Assert.That(Crypto.Sha256(value), Is.EqualTo(expected));
    }

    [TestCase()]
    public void Sha256_CalledTwiceWithSameInput_ShouldReturnSameValue()
    {
        string first = Crypto.Sha256("toto");
        string second = Crypto.Sha256("toto");

        Assert.That(first, Is.EqualTo(second));
    }

    #endregion Sha256

    #region Sha512

    [TestCase("", ExpectedResult = "")]
    [TestCase("   ", ExpectedResult = "")]
    [TestCase("\t\r\n", ExpectedResult = "")]
    [TestCase("toto", ExpectedResult = "EOBrmQ1E3gCRohE/2VyS/JBRZq8UeqdjJjnEGqfyaxYgxHRDgTxgW5JMBVkcFh7MNZRPxpxEM6SdEPxrBKM2EQ==")]
    public string Sha512_Valid(string value)
    {
        return Crypto.Sha512(value);
    }

    [TestCase(null, ExpectedResult = "")]
    public string Sha512_WithNull_ShouldReturnEmptyString(string? value)
    {
        return Crypto.Sha512(value!);
    }

    [TestCase("é")]
    [TestCase("日本語 😀")]
    public void Sha512_WithUnicode_ShouldMatchUtf8Hash(string value)
    {
        string expected = Convert.ToBase64String(SHA512.HashData(System.Text.Encoding.UTF8.GetBytes(value)));

        Assert.That(Crypto.Sha512(value), Is.EqualTo(expected));
    }

    #endregion Sha512

    #region GeneratePassword

    [TestCase(true, false, false, false, (ushort)32, "^[a-z]+$")]
    [TestCase(false, true, false, false, (ushort)32, "^[A-Z]+$")]
    [TestCase(false, false, true, false, (ushort)32, "^[0-9]+$")]
    [TestCase(false, false, false, true, (ushort)32, @"^[!@$%^&*()#.\-_~]+$")]
    [TestCase(true, true, true, true, (ushort)32, @"^[a-zA-Z0-9!@$%^&*()#.\-_~]+$")]
    public void GeneratePassword_Valid(bool useLowercase, bool useUppercase, bool useNumbers, bool useSpecial, ushort passwordSize, string pattern)
    {
        string password = Crypto.GeneratePassword(useLowercase, useUppercase, useNumbers, useSpecial, passwordSize);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(password, Has.Length.EqualTo(passwordSize));
            Assert.That(password, Does.Match(pattern));
        }
    }

    [TestCase()]
    public void GeneratePassword_CalledTwice_ShouldReturnDifferentValues()
    {
        string first = Crypto.GeneratePassword(true, true, true, true, 32);
        string second = Crypto.GeneratePassword(true, true, true, true, 32);

        Assert.That(first, Is.Not.EqualTo(second));
    }

    [TestCase(true, true, true, true, (ushort)0)]
    [TestCase(true, false, false, false, (ushort)0)]
    public void GeneratePassword_WithSizeZero_ShouldReturnEmptyString(bool useLowercase, bool useUppercase, bool useNumbers, bool useSpecial, ushort passwordSize)
    {
        Assert.That(Crypto.GeneratePassword(useLowercase, useUppercase, useNumbers, useSpecial, passwordSize), Is.Empty);
    }

    [TestCase(true, true, true, true, (ushort)1)]
    [TestCase(true, true, true, true, (ushort)1000)]
    [TestCase(true, true, true, true, ushort.MaxValue)]
    public void GeneratePassword_WithVariousSizes_ShouldReturnRequestedLength(bool useLowercase, bool useUppercase, bool useNumbers, bool useSpecial, ushort passwordSize)
    {
        string password = Crypto.GeneratePassword(useLowercase, useUppercase, useNumbers, useSpecial, passwordSize);

        Assert.That(password, Has.Length.EqualTo(passwordSize));
    }

    [TestCase(true, false, true, false, @"^[a-z0-9]+$")]
    [TestCase(false, true, false, true, @"^[A-Z!@$%^&*()#.\-_~]+$")]
    [TestCase(true, true, false, false, @"^[a-zA-Z]+$")]
    [TestCase(false, false, true, true, @"^[0-9!@$%^&*()#.\-_~]+$")]
    public void GeneratePassword_WithMixedSets_ShouldOnlyUseSelectedCharacters(bool useLowercase, bool useUppercase, bool useNumbers, bool useSpecial, string pattern)
    {
        string password = Crypto.GeneratePassword(useLowercase, useUppercase, useNumbers, useSpecial, 200);

        Assert.That(password, Does.Match(pattern));
    }

    [TestCase(false, false, false, false, (ushort)10)]
    public void GeneratePassword_ShouldThrowArgumentException(bool useLowercase, bool useUppercase, bool useNumbers, bool useSpecial, ushort passwordSize)
    {
        Assert.That(() => Crypto.GeneratePassword(useLowercase, useUppercase, useNumbers, useSpecial, passwordSize), Throws.TypeOf<ArgumentException>());
    }

    #endregion GeneratePassword

    #region Encrypt

    [TestCase("toto")]
    [TestCase("This is a longer plain text with special chars !@#$%^&*()")]
    public void Encrypt_ThenDecrypt_Valid(string plainText)
    {
        string encrypted = Crypto.Encrypt(plainText, _validKey);
        string decrypted = Crypto.Decrypt(encrypted, _validKey);

        Assert.That(decrypted, Is.EqualTo(plainText));
    }

    [TestCase("héllo wörld")]
    [TestCase("日本語のテキスト")]
    [TestCase("emoji 😀👩🏽 test")]
    [TestCase("   ")]
    [TestCase("\t\r\n")]
    [TestCase("a")]
    public void Encrypt_ThenDecrypt_WithUnicodeOrWhitespace_Valid(string plainText)
    {
        string encrypted = Crypto.Encrypt(plainText, _validKey);

        Assert.That(Crypto.Decrypt(encrypted, _validKey), Is.EqualTo(plainText));
    }

    [TestCase()]
    public void Encrypt_ThenDecrypt_WithLargePayload_Valid()
    {
        string plainText = new('x', 100_000);

        string encrypted = Crypto.Encrypt(plainText, _validKey);

        Assert.That(Crypto.Decrypt(encrypted, _validKey), Is.EqualTo(plainText));
    }

    [TestCase("a")]
    [TestCase("toto")]
    [TestCase("héllo")]
    [TestCase("emoji 😀")]
    public void Encrypt_ShouldProduceNoncePlusCipherPlusTagLayout(string plainText)
    {
        int expectedLength = 12 + System.Text.Encoding.UTF8.GetByteCount(plainText) + 16;

        byte[] data = Convert.FromBase64String(Crypto.Encrypt(plainText, _validKey));

        Assert.That(data, Has.Length.EqualTo(expectedLength));
    }

    [TestCase("toto")]
    [TestCase("héllo wörld 😀")]
    public void Encrypt_Output_ShouldBeDecryptableWithIndependentAesGcm(string plainText)
    {
        byte[] data = Convert.FromBase64String(Crypto.Encrypt(plainText, _validKey));

        ReadOnlySpan<byte> nonce = data.AsSpan(0, 12);
        ReadOnlySpan<byte> cipher = data.AsSpan(12, data.Length - 12 - 16);
        ReadOnlySpan<byte> tag = data.AsSpan(data.Length - 16, 16);

        byte[] plainBytes = new byte[cipher.Length];

        using AesGcm aesGcm = new(_validKey, 16);
        aesGcm.Decrypt(nonce, cipher, tag, plainBytes);

        Assert.That(System.Text.Encoding.UTF8.GetString(plainBytes), Is.EqualTo(plainText));
    }

    [TestCase()]
    public void Encrypt_CalledTwice_ShouldUseDifferentNonces()
    {
        byte[] first = Convert.FromBase64String(Crypto.Encrypt("toto", _validKey));
        byte[] second = Convert.FromBase64String(Crypto.Encrypt("toto", _validKey));

        Assert.That(first.AsSpan(0, 12).SequenceEqual(second.AsSpan(0, 12)), Is.False);
    }

    [TestCase()]
    public void Encrypt_CalledTwice_ShouldReturnDifferentValues()
    {
        string first = Crypto.Encrypt("toto", _validKey);
        string second = Crypto.Encrypt("toto", _validKey);

        Assert.That(first, Is.Not.EqualTo(second));
    }

    [TestCase(null)]
    public void Encrypt_WithNull_ShouldThrowArgumentNullException(string? plainText)
    {
        Assert.That(() => Crypto.Encrypt(plainText!, _validKey), Throws.TypeOf<ArgumentNullException>());
    }

    [TestCase("")]
    public void Encrypt_WithEmptyString_ShouldThrowArgumentException(string plainText)
    {
        Assert.That(() => Crypto.Encrypt(plainText, _validKey), Throws.TypeOf<ArgumentException>());
    }

    [TestCase()]
    public void Encrypt_WithNullKey_ShouldThrowArgumentNullException()
    {
        Assert.That(() => Crypto.Encrypt("toto", null!), Throws.TypeOf<ArgumentNullException>());
    }

    [TestCase(16)]
    [TestCase(24)]
    [TestCase(33)]
    public void Encrypt_WithInvalidKeySize_ShouldThrowArgumentOutOfRangeException(int keySize)
    {
        byte[] key = RandomNumberGenerator.GetBytes(keySize);

        Assert.That(() => Crypto.Encrypt("toto", key), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    #endregion Encrypt

    #region Decrypt

    [TestCase(null)]
    public void Decrypt_WithNull_ShouldThrowArgumentNullException(string? encryptedText)
    {
        Assert.That(() => Crypto.Decrypt(encryptedText!, _validKey), Throws.TypeOf<ArgumentNullException>());
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Decrypt_WithEmptyOrWhiteSpace_ShouldThrowArgumentException(string encryptedText)
    {
        Assert.That(() => Crypto.Decrypt(encryptedText, _validKey), Throws.TypeOf<ArgumentException>());
    }

    [TestCase()]
    public void Decrypt_WithNullKey_ShouldThrowArgumentNullException()
    {
        string encrypted = Crypto.Encrypt("toto", _validKey);

        Assert.That(() => Crypto.Decrypt(encrypted, null!), Throws.TypeOf<ArgumentNullException>());
    }

    [TestCase(16)]
    [TestCase(24)]
    [TestCase(33)]
    public void Decrypt_WithInvalidKeySize_ShouldThrowArgumentOutOfRangeException(int keySize)
    {
        byte[] key = RandomNumberGenerator.GetBytes(keySize);
        string encrypted = Crypto.Encrypt("toto", _validKey);

        Assert.That(() => Crypto.Decrypt(encrypted, key), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [TestCase()]
    public void Decrypt_TooShortEncryptedText_ShouldThrowArgumentException()
    {
        string tooShort = Convert.ToBase64String(new byte[27]);

        // exercises the cipherSize <= 0 guard for payloads smaller than nonce+tag.
        Assert.That(() => Crypto.Decrypt(tooShort, _validKey), Throws.TypeOf<ArgumentException>());
    }

    [TestCase()]
    public void Decrypt_WithKnownVector_ShouldReturnPlainText()
    {
        //Produced independently with AesGcm: key = bytes 0..31, nonce = bytes 0xA0..0xAB, layout = nonce | ciphertext | tag
        byte[] key = new byte[32];
        for (int i = 0; i < key.Length; i++)
        {
            key[i] = (byte)i;
        }

        const string vector = "oKGio6Slpqeoqaqrrn0QQSrnIv0DBui9JleR5dgK8twYo0FJUFP74+o=";

        Assert.That(Crypto.Decrypt(vector, key), Is.EqualTo("Hello, Bacon!"));
    }

    [TestCase()]
    public void Decrypt_WithIndependentlyBuiltPayload_ShouldReturnPlainText()
    {
        const string plainText = "héllo 😀";
        byte[] nonce = RandomNumberGenerator.GetBytes(12);
        byte[] plainBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        byte[] cipher = new byte[plainBytes.Length];
        byte[] tag = new byte[16];

        using (AesGcm aesGcm = new(_validKey, 16))
        {
            aesGcm.Encrypt(nonce, plainBytes, cipher, tag);
        }

        byte[] payload = [.. nonce, .. cipher, .. tag];

        Assert.That(Crypto.Decrypt(Convert.ToBase64String(payload), _validKey), Is.EqualTo(plainText));
    }

    [TestCase(28)]
    public void Decrypt_WithNoncePlusTagOnly_ShouldThrowArgumentException(int length)
    {
        string boundary = Convert.ToBase64String(new byte[length]);

        Assert.That(() => Crypto.Decrypt(boundary, _validKey), Throws.TypeOf<ArgumentException>());
    }

    [TestCase(29)]
    [TestCase(40)]
    public void Decrypt_WithMinimalValidLengthButInvalidContent_ShouldThrowAuthenticationTagMismatchException(int length)
    {
        string payload = Convert.ToBase64String(new byte[length]);

        Assert.That(() => Crypto.Decrypt(payload, _validKey), Throws.TypeOf<AuthenticationTagMismatchException>());
    }

    [TestCase(0)]
    [TestCase(11)]
    public void Decrypt_TamperedNonce_ShouldThrowAuthenticationTagMismatchException(int index)
    {
        byte[] data = Convert.FromBase64String(Crypto.Encrypt("toto", _validKey));
        data[index] ^= 0xFF;

        Assert.That(() => Crypto.Decrypt(Convert.ToBase64String(data), _validKey), Throws.TypeOf<AuthenticationTagMismatchException>());
    }

    [TestCase(12)]
    [TestCase(13)]
    public void Decrypt_TamperedCipherBytes_ShouldThrowAuthenticationTagMismatchException(int index)
    {
        byte[] data = Convert.FromBase64String(Crypto.Encrypt("toto", _validKey));
        data[index] ^= 0xFF;

        Assert.That(() => Crypto.Decrypt(Convert.ToBase64String(data), _validKey), Throws.TypeOf<AuthenticationTagMismatchException>());
    }

    [TestCase()]
    public void Decrypt_TamperedFirstTagByte_ShouldThrowAuthenticationTagMismatchException()
    {
        byte[] data = Convert.FromBase64String(Crypto.Encrypt("toto", _validKey));
        data[^16] ^= 0xFF;

        Assert.That(() => Crypto.Decrypt(Convert.ToBase64String(data), _validKey), Throws.TypeOf<AuthenticationTagMismatchException>());
    }

    [TestCase("not base64 !!")]
    [TestCase("abc")]
    public void Decrypt_WithInvalidBase64_ShouldThrowFormatException(string encryptedText)
    {
        Assert.That(() => Crypto.Decrypt(encryptedText, _validKey), Throws.TypeOf<FormatException>());
    }

    [TestCase()]
    public void Decrypt_WrongKey_ShouldThrowAuthenticationTagMismatchException()
    {
        byte[] wrongKey = RandomNumberGenerator.GetBytes(32);
        string encrypted = Crypto.Encrypt("toto", _validKey);

        Assert.That(() => Crypto.Decrypt(encrypted, wrongKey), Throws.TypeOf<AuthenticationTagMismatchException>());
    }

    [TestCase()]
    public void Decrypt_TamperedCipherText_ShouldThrowAuthenticationTagMismatchException()
    {
        string encrypted = Crypto.Encrypt("toto", _validKey);
        byte[] data = Convert.FromBase64String(encrypted);
        data[^1] ^= 0xFF;
        string tampered = Convert.ToBase64String(data);

        Assert.That(() => Crypto.Decrypt(tampered, _validKey), Throws.TypeOf<AuthenticationTagMismatchException>());
    }

    #endregion Decrypt
}