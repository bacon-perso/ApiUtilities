using System.Data;
using System.Net;

namespace Bacon.Utilities.Tests.Services;

[TestFixture]
internal sealed class ValidatorsTests
{
    #region Password Validations

    [TestCase("abcd", (byte)4, (ushort)256, (byte)0, (byte)0, (byte)0, (byte)0, ExpectedResult = true)]
    [TestCase("abcd", (byte)4, (ushort)256, (byte)1, (byte)0, (byte)0, (byte)0, ExpectedResult = false)]
    [TestCase("abcd", (byte)4, (ushort)256, (byte)0, (byte)1, (byte)0, (byte)0, ExpectedResult = true)]
    [TestCase("abcd", (byte)4, (ushort)256, (byte)0, (byte)0, (byte)1, (byte)0, ExpectedResult = false)]
    [TestCase("abcd", (byte)4, (ushort)256, (byte)0, (byte)0, (byte)0, (byte)1, ExpectedResult = false)]
    [TestCase("abcd", (byte)4, (ushort)256, (byte)1, (byte)1, (byte)0, (byte)0, ExpectedResult = false)]
    [TestCase("abcd", (byte)4, (ushort)256, (byte)1, (byte)0, (byte)1, (byte)0, ExpectedResult = false)]
    [TestCase("abcd", (byte)4, (ushort)256, (byte)1, (byte)0, (byte)0, (byte)1, ExpectedResult = false)]
    [TestCase("abcd", (byte)4, (ushort)256, (byte)0, (byte)1, (byte)1, (byte)0, ExpectedResult = false)]
    [TestCase("abcd", (byte)4, (ushort)256, (byte)0, (byte)1, (byte)0, (byte)1, ExpectedResult = false)]
    [TestCase("abcd", (byte)4, (ushort)256, (byte)0, (byte)0, (byte)1, (byte)1, ExpectedResult = false)]
    [TestCase("abcd", (byte)4, (ushort)256, (byte)1, (byte)1, (byte)1, (byte)1, ExpectedResult = false)]
    [TestCase("abcd", (byte)4, (byte)4, (byte)0, (byte)0, (byte)0, (byte)0, ExpectedResult = true)]
    [TestCase("abcd", (byte)8, (ushort)256, (byte)1, (byte)1, (byte)1, (byte)1, ExpectedResult = false)]
    [TestCase("Pass word1!", (byte)4, (ushort)256, (byte)0, (byte)0, (byte)0, (byte)0, ExpectedResult = true)]
    [TestCase("Pass word1!", (byte)255, (ushort)256, (byte)0, (byte)0, (byte)0, (byte)0, ExpectedResult = false)]
    [TestCase("Pass word1!", (byte)4, (ushort)256, (byte)1, (byte)0, (byte)0, (byte)0, ExpectedResult = true)]
    [TestCase("Pass word1!", (byte)4, (ushort)256, (byte)0, (byte)1, (byte)0, (byte)0, ExpectedResult = true)]
    [TestCase("Pass word1!", (byte)4, (ushort)256, (byte)0, (byte)0, (byte)1, (byte)0, ExpectedResult = true)]
    [TestCase("Pass word1!", (byte)4, (ushort)256, (byte)0, (byte)0, (byte)0, (byte)1, ExpectedResult = true)]
    [TestCase("Pass word1!", (byte)4, (ushort)256, (byte)1, (byte)1, (byte)0, (byte)0, ExpectedResult = true)]
    [TestCase("Pass word1!", (byte)4, (ushort)256, (byte)1, (byte)0, (byte)1, (byte)0, ExpectedResult = true)]
    [TestCase("Pass word1!", (byte)4, (ushort)256, (byte)1, (byte)0, (byte)0, (byte)1, ExpectedResult = true)]
    [TestCase("Pass word1!", (byte)4, (ushort)256, (byte)0, (byte)1, (byte)1, (byte)0, ExpectedResult = true)]
    [TestCase("Pass word1!", (byte)4, (ushort)256, (byte)0, (byte)1, (byte)0, (byte)1, ExpectedResult = true)]
    [TestCase("Pass word1!", (byte)4, (ushort)256, (byte)0, (byte)0, (byte)1, (byte)1, ExpectedResult = true)]
    [TestCase("Pass word1!", (byte)4, (ushort)256, (byte)1, (byte)1, (byte)1, (byte)1, ExpectedResult = true)]
    [TestCase("Pass word1!", (byte)8, (ushort)256, (byte)1, (byte)1, (byte)1, (byte)1, ExpectedResult = true)]
    [TestCase("Pass word1", (byte)4, (ushort)256, (byte)0, (byte)0, (byte)0, (byte)0, ExpectedResult = true)]
    [TestCase("Pass word1", (byte)255, (ushort)256, (byte)0, (byte)0, (byte)0, (byte)0, ExpectedResult = false)]
    [TestCase("Pass word1", (byte)4, (ushort)256, (byte)1, (byte)0, (byte)0, (byte)0, ExpectedResult = true)]
    [TestCase("Pass word1", (byte)4, (ushort)256, (byte)0, (byte)1, (byte)0, (byte)0, ExpectedResult = true)]
    [TestCase("Pass word1", (byte)4, (ushort)256, (byte)0, (byte)0, (byte)1, (byte)0, ExpectedResult = true)]
    [TestCase("Pass word1", (byte)4, (ushort)256, (byte)0, (byte)0, (byte)0, (byte)1, ExpectedResult = true)]
    [TestCase("Pass word1", (byte)4, (ushort)256, (byte)1, (byte)1, (byte)0, (byte)0, ExpectedResult = true)]
    [TestCase("Pass word1", (byte)4, (ushort)256, (byte)1, (byte)0, (byte)1, (byte)0, ExpectedResult = true)]
    [TestCase("Pass word1", (byte)4, (ushort)256, (byte)1, (byte)0, (byte)0, (byte)1, ExpectedResult = true)]
    [TestCase("Pass word1", (byte)4, (ushort)256, (byte)0, (byte)1, (byte)1, (byte)0, ExpectedResult = true)]
    [TestCase("Pass word1", (byte)4, (ushort)256, (byte)0, (byte)1, (byte)0, (byte)1, ExpectedResult = true)]
    [TestCase("Pass word1", (byte)4, (ushort)256, (byte)0, (byte)0, (byte)1, (byte)1, ExpectedResult = true)]
    [TestCase("Pass word1", (byte)4, (ushort)256, (byte)1, (byte)1, (byte)1, (byte)1, ExpectedResult = true)]
    [TestCase("Pass word1", (byte)8, (ushort)256, (byte)1, (byte)1, (byte)1, (byte)1, ExpectedResult = true)]
    [TestCase("PasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPassword1!", (byte)4, (ushort)256, (byte)0, (byte)0, (byte)0, (byte)0, ExpectedResult = false)]
    [TestCase("PasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPassword1!", (byte)255, (ushort)256, (byte)0, (byte)0, (byte)0, (byte)0, ExpectedResult = false)]
    [TestCase("PasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPassword1!", (byte)4, (ushort)256, (byte)1, (byte)0, (byte)0, (byte)0, ExpectedResult = false)]
    [TestCase("PasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPassword1!", (byte)4, (ushort)256, (byte)0, (byte)1, (byte)0, (byte)0, ExpectedResult = false)]
    [TestCase("PasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPassword1!", (byte)4, (ushort)256, (byte)0, (byte)0, (byte)1, (byte)0, ExpectedResult = false)]
    [TestCase("PasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPassword1!", (byte)4, (ushort)256, (byte)0, (byte)0, (byte)0, (byte)1, ExpectedResult = false)]
    [TestCase("PasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPassword1!", (byte)4, (ushort)256, (byte)1, (byte)1, (byte)0, (byte)0, ExpectedResult = false)]
    [TestCase("PasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPassword1!", (byte)4, (ushort)256, (byte)1, (byte)0, (byte)1, (byte)0, ExpectedResult = false)]
    [TestCase("PasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPassword1!", (byte)4, (ushort)256, (byte)1, (byte)0, (byte)0, (byte)1, ExpectedResult = false)]
    [TestCase("PasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPassword1!", (byte)4, (ushort)256, (byte)0, (byte)1, (byte)1, (byte)0, ExpectedResult = false)]
    [TestCase("PasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPassword1!", (byte)4, (ushort)256, (byte)0, (byte)1, (byte)0, (byte)1, ExpectedResult = false)]
    [TestCase("PasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPassword1!", (byte)4, (ushort)256, (byte)0, (byte)0, (byte)1, (byte)1, ExpectedResult = false)]
    [TestCase("PasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPassword1!", (byte)4, (ushort)256, (byte)1, (byte)1, (byte)1, (byte)1, ExpectedResult = false)]
    [TestCase("PasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPasswordPassword1!", (byte)8, (ushort)256, (byte)1, (byte)1, (byte)1, (byte)1, ExpectedResult = false)]
    [TestCase(@"R\u001b[0;31m", (byte)4, (ushort)256, (byte)0, (byte)0, (byte)0, (byte)0, ExpectedResult = true)]
    [TestCase(@"R \u001b[0;31m", (byte)8, (ushort)256, (byte)1, (byte)1, (byte)1, (byte)1, ExpectedResult = true)]
    [TestCase(@"Roses are \u001b[0;31mred\u001b[0m, violets are \u001b[0;34mblue. Hope you enjoy terminal hue", (byte)4, (ushort)256, (byte)0, (byte)0, (byte)0, (byte)0, ExpectedResult = true)]
    [TestCase(@"Roses are \u001b[0;31mred\u001b[0m, violets are \u001b[0;34mblue. Hope you enjoy terminal hue", (byte)8, (ushort)256, (byte)1, (byte)1, (byte)1, (byte)1, ExpectedResult = true)]
    [TestCase("() { _; } >_[$($())] { touch /tmp/blns.shellshock2.fail; }", (byte)4, (ushort)256, (byte)0, (byte)0, (byte)0, (byte)0, ExpectedResult = true)]
    [TestCase("() { _; } >_[$($())] { touch /tmp/blns.shellshock2.fail; }", (byte)8, (ushort)256, (byte)1, (byte)1, (byte)1, (byte)1, ExpectedResult = false)]
    [TestCase("<<< %s(un='%s') = %u", (byte)4, (ushort)256, (byte)0, (byte)0, (byte)0, (byte)0, ExpectedResult = true)]
    [TestCase("<<< %s(un='%s') = %u", (byte)8, (ushort)256, (byte)1, (byte)1, (byte)1, (byte)1, ExpectedResult = false)]
    [TestCase("CLOCK$", (byte)4, (ushort)256, (byte)0, (byte)0, (byte)0, (byte)0, ExpectedResult = true)]
    [TestCase("CLOCK$", (byte)8, (ushort)256, (byte)1, (byte)1, (byte)1, (byte)1, ExpectedResult = false)]
    [TestCase("../../../../../../../../../../../etc/passwd%00", (byte)4, (ushort)256, (byte)0, (byte)0, (byte)0, (byte)0, ExpectedResult = true)]
    [TestCase("../../../../../../../../../../../etc/passwd%00", (byte)8, (ushort)256, (byte)1, (byte)1, (byte)1, (byte)1, ExpectedResult = false)]
    [TestCase("Ṱ̺̺̕o͞ ̷i̲̬͇̪͙n̝̗͕v̟̜̘̦͟o̶̙̰̠kè͚̮̺̪̹̱̤ ̖t̝͕̳̣̻̪͞h̼͓̲̦̳̘̲e͇̣̰̦̬͎ ̢̼̻̱̘h͚͎͙̜̣̲ͅi̦̲̣̰̤v̻͍e̺̭̳̪̰-m̢iͅn̖̺̞̲̯̰d̵̼̟͙̩̼̘̳ ̞̥̱̳̭r̛̗̘e͙p͠r̼̞̻̭̗e̺̠̣͟s̘͇̳͍̝͉e͉̥̯̞̲͚̬͜ǹ̬͎͎̟̖͇̤t͍̬̤͓̼̭͘ͅi̪̱n͠g̴͉ ͏͉ͅc̬̟h͡a̫̻̯͘o̫̟̖͍̙̝͉s̗̦̲.̨̹͈̣", (byte)4, (ushort)256, (byte)0, (byte)0, (byte)0, (byte)0, ExpectedResult = true)]
    [TestCase("Ṱ̺̺̕o͞ ̷i̲̬͇̪͙n̝̗͕v̟̜̘̦͟o̶̙̰̠kè͚̮̺̪̹̱̤ ̖t̝͕̳̣̻̪͞h̼͓̲̦̳̘̲e͇̣̰̦̬͎ ̢̼̻̱̘h͚͎͙̜̣̲ͅi̦̲̣̰̤v̻͍e̺̭̳̪̰-m̢iͅn̖̺̞̲̯̰d̵̼̟͙̩̼̘̳ ̞̥̱̳̭r̛̗̘e͙p͠r̼̞̻̭̗e̺̠̣͟s̘͇̳͍̝͉e͉̥̯̞̲͚̬͜ǹ̬͎͎̟̖͇̤t͍̬̤͓̼̭͘ͅi̪̱n͠g̴͉ ͏͉ͅc̬̟h͡a̫̻̯͘o̫̟̖͍̙̝͉s̗̦̲.̨̹͈̣", (byte)8, (ushort)256, (byte)1, (byte)1, (byte)1, (byte)1, ExpectedResult = false)]
    [TestCase(@"
test
", (byte)4, (ushort)256, (byte)0, (byte)0, (byte)0, (byte)0, ExpectedResult = true)]
    [TestCase(@"
test
", (byte)8, (ushort)256, (byte)1, (byte)1, (byte)1, (byte)1, ExpectedResult = false)]

    [TestCase("مُنَاقَشَةُ سُبُلِ اِسْتِخْدَامِ اللُّغَةِ فِي النُّظُمِ الْقَائِمَةِ وَفِيم يَخُصَّ التَّطْبِيقَاتُ الْحاسُوبِيَّةُ، ", (byte)4, (ushort)256, (byte)0, (byte)0, (byte)0, (byte)0, ExpectedResult = true)]
    [TestCase("مُنَاقَشَةُ سُبُلِ اِسْتِخْدَامِ اللُّغَةِ فِي النُّظُمِ الْقَائِمَةِ وَفِيم يَخُصَّ التَّطْبِيقَاتُ الْحاسُوبِيَّةُ، ", (byte)8, (ushort)256, (byte)1, (byte)1, (byte)1, (byte)1, ExpectedResult = false)]

    [TestCase("(╯°□°）╯︵ ┻━┻)", (byte)4, (ushort)256, (byte)0, (byte)0, (byte)0, (byte)0, ExpectedResult = true)]
    [TestCase("(╯°□°）╯︵ ┻━┻)", (byte)8, (ushort)256, (byte)1, (byte)1, (byte)1, (byte)1, ExpectedResult = false)]

    [TestCase("(｡◕ ∀ ◕｡)", (byte)4, (ushort)256, (byte)0, (byte)0, (byte)0, (byte)0, ExpectedResult = true)]
    [TestCase("(｡◕ ∀ ◕｡)", (byte)8, (ushort)256, (byte)1, (byte)1, (byte)1, (byte)1, ExpectedResult = false)]

    [TestCase("(ﾉಥ益ಥ）ﾉ﻿ ┻━┻", (byte)4, (ushort)256, (byte)0, (byte)0, (byte)0, (byte)0, ExpectedResult = true)]
    [TestCase("(ﾉಥ益ಥ）ﾉ﻿ ┻━┻", (byte)8, (ushort)256, (byte)1, (byte)1, (byte)1, (byte)1, ExpectedResult = false)]

    [TestCase("😍👩🏽👨‍🦰 👨🏿‍🦰 👨‍🦱 👨🏿‍🦱 🦹🏿‍♂️👾 🙇 💁 🙅 🙆 🙋 🙎 🙍🐵 🙈 🙉 🙊", (byte)4, (ushort)256, (byte)0, (byte)0, (byte)0, (byte)0, ExpectedResult = true)]
    [TestCase("😍👩🏽👨‍🦰 👨🏿‍🦰 👨‍🦱 👨🏿‍🦱 🦹🏿‍♂️👾 🙇 💁 🙅 🙆 🙋 🙎 🙍🐵 🙈 🙉 🙊", (byte)8, (ushort)256, (byte)1, (byte)1, (byte)1, (byte)1, ExpectedResult = false)]
    public bool ValidatePasswordComplexity_Valid(string password, byte minPasswordLength, ushort maxPasswordLength, byte minimumUpperCaseCharacters, byte minimumLowerCaseCharacters, byte minimumNumericalCharacters, byte minimumSpecialCharacters)
    {
        return Validators.ValidatePasswordComplexity(password, minPasswordLength, maxPasswordLength, minimumUpperCaseCharacters, minimumLowerCaseCharacters, minimumNumericalCharacters, minimumSpecialCharacters);
    }

    [TestCase(null, (byte)10, (ushort)256, (byte)0, (byte)0, (byte)0, (byte)0)]
    public void ValidatePasswordComplexity_ShouldThrowArgumentNullException(string? password, byte minPasswordLength, ushort maxPasswordLength, byte minimumUpperCaseCharacters, byte minimumLowerCaseCharacters, byte minimumNumericalCharacters, byte minimumSpecialCharacters)
    {
        Assert.That(() => Validators.ValidatePasswordComplexity(password!, minPasswordLength, maxPasswordLength, minimumUpperCaseCharacters, minimumLowerCaseCharacters, minimumNumericalCharacters, minimumSpecialCharacters), Throws.TypeOf<ArgumentNullException>());
    }

    [TestCase("", (byte)8, (ushort)256, (byte)0, (byte)0, (byte)0, (byte)0)]
    [TestCase("         ", (byte)8, (ushort)256, (byte)0, (byte)0, (byte)0, (byte)0)]
    public void ValidatePasswordComplexity_ShouldThrowArgumentException(string password, byte minPasswordLength, ushort maxPasswordLength, byte minimumUpperCaseCharacters, byte minimumLowerCaseCharacters, byte minimumNumericalCharacters, byte minimumSpecialCharacters)
    {
        Assert.That(() => Validators.ValidatePasswordComplexity(password, minPasswordLength, maxPasswordLength, minimumUpperCaseCharacters, minimumLowerCaseCharacters, minimumNumericalCharacters, minimumSpecialCharacters), Throws.TypeOf<ArgumentException>());
    }

    [TestCase("abcd", (byte)0, (ushort)256, (byte)0, (byte)0, (byte)0, (byte)0)]
    [TestCase("abcd", (byte)8, (ushort)257, (byte)0, (byte)0, (byte)0, (byte)0)]
    [TestCase("abcd", (byte)3, (ushort)2, (byte)0, (byte)0, (byte)0, (byte)0)]
    public void ValidatePasswordComplexity_ShouldThrowArgumentOutOfRangeException(string password, byte minPasswordLength, ushort maxPasswordLength, byte minimumUpperCaseCharacters, byte minimumLowerCaseCharacters, byte minimumNumericalCharacters, byte minimumSpecialCharacters)
    {
        Assert.That(() => Validators.ValidatePasswordComplexity(password, minPasswordLength, maxPasswordLength, minimumUpperCaseCharacters, minimumLowerCaseCharacters, minimumNumericalCharacters, minimumSpecialCharacters), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [TestCase("abcd!", (byte)4, (byte)1, ExpectedResult = true)]
    [TestCase("abcd", (byte)4, (byte)1, ExpectedResult = false)]
    [TestCase("ab!", (byte)4, (byte)0, ExpectedResult = false)]
    [TestCase("abcd", (byte)4, (byte)0, ExpectedResult = true)]
    [TestCase("!!!!", (byte)4, (byte)4, ExpectedResult = true)]
    [TestCase("!!!a", (byte)4, (byte)4, ExpectedResult = false)]
    [TestCase("Ab1!", (byte)4, (byte)1, ExpectedResult = true)]
    [TestCase("Ab1!", (byte)5, (byte)1, ExpectedResult = false)]
    public bool ValidatePasswordComplexity_WithThreeArguments_Valid(string password, byte minPasswordLength, byte minRequiredNonAlphanumericCharacters)
    {
        return Validators.ValidatePasswordComplexity(password, minPasswordLength, minRequiredNonAlphanumericCharacters);
    }

    [TestCase(null)]
    public void ValidatePasswordComplexity_WithThreeArguments_ShouldThrowArgumentNullException(string? password)
    {
        Assert.That(() => Validators.ValidatePasswordComplexity(password!, (byte)4, (byte)0), Throws.TypeOf<ArgumentNullException>());
    }

    [TestCase("")]
    [TestCase("   ")]
    public void ValidatePasswordComplexity_WithThreeArguments_ShouldThrowArgumentException(string password)
    {
        Assert.That(() => Validators.ValidatePasswordComplexity(password, (byte)4, (byte)0), Throws.TypeOf<ArgumentException>());
    }

    [TestCase()]
    public void ValidatePasswordComplexity_WithThreeArgumentsAndZeroMinLength_ShouldThrowArgumentOutOfRangeException()
    {
        Assert.That(() => Validators.ValidatePasswordComplexity("abcd", (byte)0, (byte)0), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [TestCase(4, ExpectedResult = true)]
    [TestCase(255, ExpectedResult = true)]
    [TestCase(256, ExpectedResult = true)]
    [TestCase(257, ExpectedResult = false)]
    [TestCase(1000, ExpectedResult = false)]
    public bool ValidatePasswordComplexity_LengthBoundaries(int length)
    {
        return Validators.ValidatePasswordComplexity(new string('a', length), (byte)4, (ushort)256, (byte)0, (byte)0, (byte)0, (byte)0);
    }

    [TestCase(3, ExpectedResult = false)]
    [TestCase(4, ExpectedResult = true)]
    [TestCase(5, ExpectedResult = true)]
    public bool ValidatePasswordComplexity_MinimumLengthBoundaries(int length)
    {
        return Validators.ValidatePasswordComplexity(new string('a', length), (byte)4, (ushort)256, (byte)0, (byte)0, (byte)0, (byte)0);
    }

    [TestCase("Éa٣!", (byte)1, (byte)1, (byte)1, (byte)1, ExpectedResult = true)]  //É (upper), a (lower), ٣ (Arabic-Indic digit), ! (special)
    [TestCase("Éa٣!", (byte)2, (byte)1, (byte)1, (byte)1, ExpectedResult = false)]
    [TestCase("éééé", (byte)0, (byte)4, (byte)0, (byte)0, ExpectedResult = true)]  //é counts as lower case
    [TestCase("ÉÉÉÉ", (byte)4, (byte)0, (byte)0, (byte)0, ExpectedResult = true)]  //É counts as upper case
    public bool ValidatePasswordComplexity_WithUnicodeCharacters(string password, byte minUpper, byte minLower, byte minDigits, byte minSpecial)
    {
        return Validators.ValidatePasswordComplexity(password, (byte)4, (ushort)256, minUpper, minLower, minDigits, minSpecial);
    }

    [TestCase("aaaA1!", (byte)1, (byte)5, (byte)1, (byte)1, ExpectedResult = false)]
    [TestCase("aaaA1!", (byte)1, (byte)3, (byte)1, (byte)1, ExpectedResult = true)]
    [TestCase("AA11!!", (byte)2, (byte)0, (byte)2, (byte)2, ExpectedResult = true)]
    [TestCase("AA11!!", (byte)3, (byte)0, (byte)2, (byte)2, ExpectedResult = false)]
    [TestCase("AA11!!", (byte)2, (byte)0, (byte)3, (byte)2, ExpectedResult = false)]
    [TestCase("AA11!!", (byte)2, (byte)0, (byte)2, (byte)3, ExpectedResult = false)]
    public bool ValidatePasswordComplexity_CharacterClassCounts_AreExact(string password, byte minUpper, byte minLower, byte minDigits, byte minSpecial)
    {
        return Validators.ValidatePasswordComplexity(password, (byte)1, (ushort)256, minUpper, minLower, minDigits, minSpecial);
    }

    #endregion Password Validations

    #region Username Validations

    [TestCase("username", (byte)4, (ushort)256, ExpectedResult = true)]
    [TestCase("u", (byte)4, (ushort)256, ExpectedResult = false)]
    [TestCase("usernameusername", (byte)4, (ushort)10, ExpectedResult = false)]
    [TestCase("username^", (byte)4, (ushort)256, ExpectedResult = true)]
    [TestCase("username@", (byte)4, (ushort)256, ExpectedResult = true)]
    [TestCase("email@domain.ca", (byte)4, (ushort)256, ExpectedResult = true)]
    [TestCase("123345", (byte)4, (ushort)256, ExpectedResult = true)]
    [TestCase("username12435", (byte)4, (ushort)256, ExpectedResult = true)]
    [TestCase(@"Us3r$@.\_-", (byte)4, (ushort)256, ExpectedResult = true)]
    [TestCase(@"Us3r$@.\_-$", (byte)4, (ushort)256, ExpectedResult = true)]
    [TestCase(@"Us3r$@.\_-+", (byte)4, (ushort)256, ExpectedResult = true)]
    [TestCase("user|name", (byte)4, (ushort)256, ExpectedResult = true)]
    [TestCase("username!", (byte)4, (ushort)256, ExpectedResult = true)]
    [TestCase("username\"", (byte)4, (ushort)256, ExpectedResult = true)]
    [TestCase("username#", (byte)4, (ushort)256, ExpectedResult = true)]
    [TestCase(@"Roses are \u001b[0;31mred\u001b[0m, violets are \u001b[0;34mblue. Hope you enjoy terminal hue", (byte)4, (ushort)256, ExpectedResult = false)]
    [TestCase("() { _; } >_[$($())] { touch /tmp/blns.shellshock2.fail; }", (byte)4, (ushort)256, ExpectedResult = false)]
    [TestCase("<<< %s(un='%s') = %u", (byte)4, (ushort)256, ExpectedResult = false)]
    [TestCase("CLOCK$", (byte)4, (ushort)256, ExpectedResult = true)]
    [TestCase("../../../../../../../../../../../etc/passwd%00", (byte)4, (ushort)256, ExpectedResult = true)]
    [TestCase("../../../../../../../../../../../etc/hosts", (byte)4, (ushort)256, ExpectedResult = true)]
    [TestCase("File:///", (byte)4, (ushort)256, ExpectedResult = true)]
    [TestCase("%s%s%s%s%s", (byte)4, (ushort)256, ExpectedResult = true)]
    [TestCase("{0}", (byte)4, (ushort)256, ExpectedResult = false)]
    [TestCase("$ENV{'HOME'}", (byte)4, (ushort)256, ExpectedResult = true)]
    [TestCase("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><!DOCTYPE foo [ <!ELEMENT foo ANY ><!ENTITY xxe SYSTEM \"file:///etc/passwd\" >]><foo>&xxe;</foo>", (byte)4, (ushort)256, ExpectedResult = false)]
    [TestCase("Kernel.exit(1)", (byte)4, (ushort)256, ExpectedResult = true)]
    [TestCase("1;DROP TABLE users", (byte)4, (ushort)256, ExpectedResult = false)]
    [TestCase("1'; DROP TABLE users-- 1", (byte)4, (ushort)256, ExpectedResult = false)]
    [TestCase("<SCRIPT SRC=//ha.ckers.org/.j>", (byte)4, (ushort)256, ExpectedResult = false)]
    [TestCase("<IMG SRC= onmouseover=\"alert('xxs')\">", (byte)4, (ushort)256, ExpectedResult = false)]
    [TestCase("Ṱ̺̺̕o͞ ̷i̲̬͇̪͙n̝̗͕v̟̜̘̦͟o̶̙̰̠kè͚̮̺̪̹̱̤ ̖t̝͕̳̣̻̪͞h̼͓̲̦̳̘̲e͇̣̰̦̬͎ ̢̼̻̱̘h͚͎͙̜̣̲ͅi̦̲̣̰̤v̻͍e̺̭̳̪̰-m̢iͅn̖̺̞̲̯̰d̵̼̟͙̩̼̘̳ ̞̥̱̳̭r̛̗̘e͙p͠r̼̞̻̭̗e̺̠̣͟s̘͇̳͍̝͉e͉̥̯̞̲͚̬͜ǹ̬͎͎̟̖͇̤t͍̬̤͓̼̭͘ͅi̪̱n͠g̴͉ ͏͉ͅc̬̟h͡a̫̻̯͘o̫̟̖͍̙̝͉s̗̦̲.̨̹͈̣", (byte)4, (ushort)256, ExpectedResult = false)]
    [TestCase(@"
test
", (byte)4, (ushort)256, ExpectedResult = false)]
    [TestCase("مُنَاقَشَةُ سُبُلِ اِسْتِخْدَامِ اللُّغَةِ فِي النُّظُمِ الْقَائِمَةِ وَفِيم يَخُصَّ التَّطْبِيقَاتُ الْحاسُوبِيَّةُ، ", (byte)4, (ushort)256, ExpectedResult = false)]
    [TestCase("(╯°□°）╯︵ ┻━┻)", (byte)4, (ushort)256, ExpectedResult = false)]
    [TestCase("(｡◕ ∀ ◕｡)", (byte)4, (ushort)256, ExpectedResult = false)]
    [TestCase("(ﾉಥ益ಥ）ﾉ﻿ ┻━┻", (byte)4, (ushort)256, ExpectedResult = false)]
    public bool ValidateUsername_Valid(string username, byte minUsernameLength, ushort maxUsernameLength)
    {
        return Validators.ValidateUsername(username, minUsernameLength, maxUsernameLength);
    }

    [TestCase(null, (byte)4, (ushort)256)]
    public void ValidateUsername_ShouldThrowArgumentNullException(string? username, byte minUsernameLength, ushort maxUsernameLength)
    {
        Assert.That(() => Validators.ValidateUsername(username!, minUsernameLength, maxUsernameLength), Throws.TypeOf<ArgumentNullException>());
    }

    [TestCase("username", (byte)0, (ushort)256)]
    [TestCase("username", (byte)4, (ushort)257)]
    [TestCase("username", (byte)255, (ushort)4)]
    public void ValidateUsername_ShouldThrowArgumentOutOfRangeException(string username, byte minUsernameLength, ushort maxUsernameLength)
    {
        Assert.That(() => Validators.ValidateUsername(username, minUsernameLength, maxUsernameLength), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [TestCase("", (byte)4, (ushort)256)]
    [TestCase("       ", (byte)4, (ushort)256)]
    public void ValidateUsername_ShouldThrowArgumentException(string username, byte minUsernameLength, ushort maxUsernameLength)
    {
        Assert.That(() => Validators.ValidateUsername(username, minUsernameLength, maxUsernameLength), Throws.TypeOf<ArgumentException>());
    }

    [TestCase("a", ExpectedResult = true)]
    [TestCase("user", ExpectedResult = true)]
    [TestCase("us er", ExpectedResult = false)]
    [TestCase("us\ter", ExpectedResult = false)]
    [TestCase("us\ner", ExpectedResult = false)]
    [TestCase(" user", ExpectedResult = false)]
    [TestCase("user ", ExpectedResult = false)]
    public bool ValidateUsername_WithDefaultLengths_Valid(string username)
    {
        return Validators.ValidateUsername(username);
    }

    [TestCase(1, ExpectedResult = true)]
    [TestCase(255, ExpectedResult = true)]
    [TestCase(256, ExpectedResult = true)]
    [TestCase(257, ExpectedResult = false)]
    public bool ValidateUsername_WithDefaultLengths_LengthBoundaries(int length)
    {
        return Validators.ValidateUsername(new string('a', length));
    }

    [TestCase(3, ExpectedResult = false)]
    [TestCase(4, ExpectedResult = true)]
    [TestCase(10, ExpectedResult = true)]
    [TestCase(11, ExpectedResult = false)]
    public bool ValidateUsername_WithCustomLengths_LengthBoundaries(int length)
    {
        return Validators.ValidateUsername(new string('a', length), (byte)4, (ushort)10);
    }

    [TestCase("abcd", (byte)4, (ushort)4, ExpectedResult = true)]
    [TestCase("abc", (byte)4, (ushort)4, ExpectedResult = false)]
    [TestCase("abcde", (byte)4, (ushort)4, ExpectedResult = false)]
    public bool ValidateUsername_WithMinEqualToMax_Valid(string username, byte minUsernameLength, ushort maxUsernameLength)
    {
        return Validators.ValidateUsername(username, minUsernameLength, maxUsernameLength);
    }

    [TestCase(null)]
    public void ValidateUsername_WithDefaultLengths_ShouldThrowArgumentNullException(string? username)
    {
        Assert.That(() => Validators.ValidateUsername(username!), Throws.TypeOf<ArgumentNullException>());
    }

    [TestCase("")]
    [TestCase("   ")]
    public void ValidateUsername_WithDefaultLengths_ShouldThrowArgumentException(string username)
    {
        Assert.That(() => Validators.ValidateUsername(username), Throws.TypeOf<ArgumentException>());
    }

    #endregion Username Validations

    #region Email Validations

    [TestCase("d@d.com", ExpectedResult = true)]
    [TestCase("a@exp.com", ExpectedResult = true)]
    [TestCase("RR@er.c", ExpectedResult = true)]
    [TestCase("pds!#$%&'*+@protonmail.com", ExpectedResult = true)]
    [TestCase("pds!#$%&'*a+@protonmail.com", ExpectedResult = true)]
    [TestCase("email@example.com", ExpectedResult = true)]
    [TestCase("firstname.lastname@example.com", ExpectedResult = true)]
    [TestCase("email@subdomain.example.com", ExpectedResult = true)]
    [TestCase("firstname+lastname@example.com", ExpectedResult = true)]
    [TestCase("email@123.123.123.123", ExpectedResult = true)]
    [TestCase("email@[123.123.123.123]", ExpectedResult = true)]
    [TestCase("\"email\"@example.com", ExpectedResult = true)]
    [TestCase("1234567890@example.com", ExpectedResult = true)]
    [TestCase("email@example-one.com", ExpectedResult = true)]
    [TestCase("_______@example.com", ExpectedResult = true)]
    [TestCase("email@example.name", ExpectedResult = true)]
    [TestCase("email@example.museum", ExpectedResult = true)]
    [TestCase("email@example.co.jp", ExpectedResult = true)]
    [TestCase("firstname-lastname@example.com", ExpectedResult = true)]
    [TestCase(@"much.”more\ unusual”@example.com", ExpectedResult = false)]
    [TestCase("much.\"more\\ unusual\"@example.com", ExpectedResult = false)]
    [TestCase("very.unusual.”@”.unusual.com@example.com", ExpectedResult = false)]
    [TestCase("very.unusual.\"@\".unusual.com@example.com", ExpectedResult = false)]
    [TestCase("very.”(),:;<>[]”.VERY.”very@\\\\ \"very”.unusual@strange.example.com", ExpectedResult = false)]
    [TestCase("very.\"(),:;<>[]\".VERY.\"very@\\\\ \"very\".unusual@strange.example.com", ExpectedResult = false)]
    [TestCase("plainaddress", ExpectedResult = false)]
    [TestCase("#@%^%#$@#$@#.com", ExpectedResult = false)]
    [TestCase("@example.com", ExpectedResult = false)]
    [TestCase("Joe Smith <email@example.com>", ExpectedResult = false)]
    [TestCase("email.example.com", ExpectedResult = false)]
    [TestCase("email@example@example.com", ExpectedResult = false)]
    [TestCase(".email@example.com", ExpectedResult = false)]
    [TestCase("email.@example.com", ExpectedResult = false)]
    [TestCase("email..email@example.com", ExpectedResult = false)]
    [TestCase("あいうえお@example.com", ExpectedResult = false)]
    [TestCase("email@example.com (Joe Smith)", ExpectedResult = false)]
    [TestCase("email@example", ExpectedResult = false)]
    [TestCase("email@-example.com", ExpectedResult = false)]
    [TestCase("email@example.web", ExpectedResult = true)]
    [TestCase("email@111.222.333.44444", ExpectedResult = true)] //weird but ok. Kubba said to consider it as "not an IP"
    [TestCase("email@example..com", ExpectedResult = false)]
    [TestCase("Abc..123@example.com", ExpectedResult = false)]
    [TestCase("”(),:;<>[\\]@example.com", ExpectedResult = false)]
    [TestCase("just”not”right@example.com", ExpectedResult = false)]
    [TestCase("just\"not\"right@example.com", ExpectedResult = false)]
    [TestCase("this\\ is\"really\"not\\allowed@example.com", ExpectedResult = false)]
    [TestCase("emailemailemailemailemailemailemailemailemailemailemailemailemail@example.com", ExpectedResult = false)]
    [TestCase("email@domaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomain.com", ExpectedResult = false)]
    [TestCase("email@domaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomain.domain.com", ExpectedResult = false)]
    public bool ValidateEmail_Valid(string email)
    {
        return Validators.ValidateEmail(email);
    }

    [TestCase(null)]
    public void ValidateEmail_ShouldThrowArgumentNullException(string? email)
    {
        Assert.That(() => Validators.ValidateEmail(email!), Throws.TypeOf<ArgumentNullException>());
    }

    [TestCase("thisisalongtextthatshouldbemorethan256characterslongthisisalongtextthatshouldbemorethan256characterslongthisisalongtextthatshouldbemorethan256characterslongthisisalongtextthatshouldbemorethan256characterslongthisisalongtextthatshouldbemorethan256characterslong")]
    public void ValidateEmail_ShouldThrowArgumentOutOfRangeException(string email)
    {
        Assert.That(() => Validators.ValidateEmail(email), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [TestCase("")]
    [TestCase("       ")]
    public void ValidateEmail_ShouldThrowArgumentException(string email)
    {
        Assert.That(() => Validators.ValidateEmail(email), Throws.TypeOf<ArgumentException>());
    }

    [TestCase("EMAIL@EXAMPLE.COM", ExpectedResult = true)]
    [TestCase("Email@Example.Com", ExpectedResult = true)]
    [TestCase("\"a@b\"@example.com", ExpectedResult = true)]  //the last @ separates the local part, so an @ inside a quoted local part is fine
    [TestCase("email@[1.2.3.4]", ExpectedResult = true)]
    [TestCase("email@[1.2.3.tag:abc]", ExpectedResult = true)]
    public bool ValidateEmail_Casing_Valid(string email)
    {
        return Validators.ValidateEmail(email);
    }

    //The address is validated exactly as received: nothing is trimmed or normalized
    [TestCase(" email@example.com", ExpectedResult = false)]
    [TestCase("email@example.com ", ExpectedResult = false)]
    [TestCase("\temail@example.com", ExpectedResult = false)]
    [TestCase("email@example.com\n", ExpectedResult = false)]
    [TestCase("email@example.com\r\n", ExpectedResult = false)]
    [TestCase("email@example.com\r", ExpectedResult = false)]
    [TestCase("email\n@example.com", ExpectedResult = false)]
    [TestCase("email@example.c\nom", ExpectedResult = false)]
    [TestCase("email @example.com", ExpectedResult = false)]
    [TestCase("email@ example.com", ExpectedResult = false)]
    [TestCase("<email@example.com>", ExpectedResult = false)]
    [TestCase("Joe <email@example.com>", ExpectedResult = false)]
    [TestCase("email(comment)@example.com", ExpectedResult = false)]
    [TestCase("email@(comment)example.com", ExpectedResult = false)]
    [TestCase("email@example.com(comment)", ExpectedResult = false)]
    [TestCase("email@example.com.", ExpectedResult = false)]
    [TestCase("email@example.com\0", ExpectedResult = false)]
    [TestCase("em\0ail@example.com", ExpectedResult = false)]
    [TestCase("em ail@example.com", ExpectedResult = false)]
    [TestCase("email@exam ple.com", ExpectedResult = false)]
    [TestCase("email@example.com,other@example.com", ExpectedResult = false)]
    [TestCase("email@example.com;other@example.com", ExpectedResult = false)]
    [TestCase("a@b@example.com", ExpectedResult = false)]
    [TestCase("email@@example.com", ExpectedResult = false)]
    [TestCase("email@example.com:25", ExpectedResult = false)]
    [TestCase("email@example.com>", ExpectedResult = false)]
    [TestCase("\"em ail\"@example.com", ExpectedResult = false)]
    [TestCase("email@", ExpectedResult = false)]
    [TestCase("@", ExpectedResult = false)]
    [TestCase("@@", ExpectedResult = false)]
    [TestCase("a@", ExpectedResult = false)]
    [TestCase("email@.com", ExpectedResult = false)]
    [TestCase("email@[IPv6:2001:db8::1]", ExpectedResult = false)]
    public bool ValidateEmail_NotNormalized_Invalid(string email)
    {
        return Validators.ValidateEmail(email);
    }

    [TestCase()]
    public void ValidateEmail_WithExactly256Characters_ShouldBeValid()
    {
        //64 (local part) + 1 (@) + 191 (domain: 3 labels of 60, a label of 4, ".com") = 256
        string domain = string.Join('.', Enumerable.Repeat(new string('a', 60), 3)) + "." + new string('b', 4) + ".com";
        string email = new string('a', 64) + "@" + domain;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(email, Has.Length.EqualTo(256));
            Assert.That(Validators.ValidateEmail(email), Is.True);
        }
    }

    [TestCase()]
    public void ValidateEmail_With257Characters_ShouldThrowArgumentOutOfRangeException()
    {
        string email = new string('a', 64) + "@" + new string('b', 192);

        Assert.That(email, Has.Length.EqualTo(257));
        Assert.That(() => Validators.ValidateEmail(email), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [TestCase("a", ExpectedResult = true)]
    [TestCase("first.last", ExpectedResult = true)]
    [TestCase("user+tag", ExpectedResult = true)]
    [TestCase("USER", ExpectedResult = true)]
    [TestCase("\"quoted\"", ExpectedResult = true)]
    [TestCase("!#$%&'*+/=?^_`{|}~-", ExpectedResult = true)]
    [TestCase("", ExpectedResult = false)]
    [TestCase(".a", ExpectedResult = false)]
    [TestCase("a.", ExpectedResult = false)]
    [TestCase("a..b", ExpectedResult = false)]
    [TestCase("a b", ExpectedResult = false)]
    [TestCase("a@b", ExpectedResult = false)]
    [TestCase("a,b", ExpectedResult = false)]
    [TestCase("あ", ExpectedResult = false)]
    [TestCase("abc\n", ExpectedResult = false)]   //$ would match before a trailing new line, \z does not
    [TestCase("abc\r\n", ExpectedResult = false)]
    [TestCase("abc\r", ExpectedResult = false)]
    [TestCase("\"quoted\"\n", ExpectedResult = false)]
    [TestCase(" abc", ExpectedResult = false)]
    [TestCase("abc ", ExpectedResult = false)]
    public bool ValidateEmailRecipient_Valid(string recipient)
    {
        return Validators.ValidateEmailRecipient(recipient);
    }

    [TestCase(63, ExpectedResult = true)]
    [TestCase(64, ExpectedResult = true)]
    [TestCase(65, ExpectedResult = false)]
    [TestCase(200, ExpectedResult = false)]
    public bool ValidateEmailRecipient_LengthBoundaries(int length)
    {
        return Validators.ValidateEmailRecipient(new string('a', length));
    }

    #endregion Email Validations

    #region DNS Validations

    #region Domain

    [TestCase("d.com", ExpectedResult = true)]
    [TestCase("exp.com", ExpectedResult = true)]
    [TestCase("er.c", ExpectedResult = true)]
    [TestCase("123.123.123.123", ExpectedResult = true)]
    [TestCase("[123.123.123.123]", ExpectedResult = true)]
    [TestCase("example.name", ExpectedResult = true)]
    [TestCase("example.museum", ExpectedResult = true)]
    [TestCase("example.co.jp", ExpectedResult = true)]
    [TestCase("strange.example.com", ExpectedResult = true)]
    [TestCase("plainaddress", ExpectedResult = false)]
    [TestCase("#@%^%#$@#$@#.com", ExpectedResult = false)]
    [TestCase("あいうえお.com", ExpectedResult = false)]
    [TestCase("-example.com", ExpectedResult = false)]
    [TestCase("example.web", ExpectedResult = true)]
    [TestCase("111.222.333.44444", ExpectedResult = true)] //weird but ok. Kubba said to consider it as "not an IP"
    [TestCase("example..com", ExpectedResult = false)]
    [TestCase("domaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomain.com", ExpectedResult = false)]
    [TestCase("domaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomain.domain.com", ExpectedResult = false)]
    public bool ValidateDomain_Valid(string domain)
    {
        return Validators.ValidateDomain(domain);
    }

    [TestCase(null)]
    public void ValidateDomain_ShouldThrowArgumentNullException(string? domain)
    {
        Assert.That(() => Validators.ValidateDomain(domain!), Throws.TypeOf<ArgumentNullException>());
    }

    [TestCase("EXAMPLE.COM", ExpectedResult = true)]
    [TestCase("Example.Com", ExpectedResult = true)]
    [TestCase("a.b", ExpectedResult = true)]
    [TestCase("example.com.", ExpectedResult = false)]
    [TestCase("", ExpectedResult = false)]
    [TestCase(".", ExpectedResult = false)]
    [TestCase(".com", ExpectedResult = false)]
    [TestCase("example.com\n", ExpectedResult = false)]   //$ would match before a trailing new line, \z does not
    [TestCase("example.com\r\n", ExpectedResult = false)]
    [TestCase("example.com\r", ExpectedResult = false)]
    [TestCase("[1.2.3.tag:abc]\n", ExpectedResult = false)]
    [TestCase("[1.2.3.4]\n", ExpectedResult = false)]
    [TestCase(" example.com", ExpectedResult = false)]
    [TestCase("example.com ", ExpectedResult = false)]
    public bool ValidateDomain_CasingAndEmptyLabels_Valid(string domain)
    {
        return Validators.ValidateDomain(domain);
    }

    //Address literal: the content after the "tag:" may not contain a bare [ or ], but a backslash escaped character is allowed
    [TestCase("[1.2.3.tag:abc]", ExpectedResult = true)]
    [TestCase("[1.2.3.tag:a-b_c]", ExpectedResult = true)]
    [TestCase("[1.2.3.tag:a^b~c]", ExpectedResult = true)]
    [TestCase("[1.2.3.tag:a\\]b]", ExpectedResult = true)]
    [TestCase("[1.2.3.tag:abc]def]", ExpectedResult = false)]
    [TestCase("[1.2.3.tag:a[b]", ExpectedResult = false)]
    [TestCase("[1.2.3.tag:a b]", ExpectedResult = false)]
    [TestCase("[1.2.3.256]", ExpectedResult = false)]
    [TestCase("[1.2.3]", ExpectedResult = false)]
    [TestCase("[1.2.3.4", ExpectedResult = false)]
    public bool ValidateDomain_AddressLiterals_Valid(string domain)
    {
        return Validators.ValidateDomain(domain);
    }

    [TestCase(1, ExpectedResult = true)]
    [TestCase(63, ExpectedResult = true)]
    [TestCase(64, ExpectedResult = false)]
    [TestCase(65, ExpectedResult = false)]
    [TestCase(100, ExpectedResult = false)]
    public bool ValidateDomain_LabelLengthBoundaries(int labelLength)
    {
        return Validators.ValidateDomain(new string('a', labelLength) + ".com");
    }

    [TestCase(254, ExpectedResult = true)]
    [TestCase(255, ExpectedResult = false)]
    [TestCase(300, ExpectedResult = false)]
    public bool ValidateDomain_TotalLengthBoundaries(int totalLength)
    {
        //4 labels of 60 characters (243 characters with the dots), then a variable label, then ".com"
        string prefix = string.Join('.', Enumerable.Repeat(new string('a', 60), 4));
        string domain = prefix + "." + new string('b', totalLength - prefix.Length - 1 - ".com".Length) + ".com";

        Assert.That(domain, Has.Length.EqualTo(totalLength));

        return Validators.ValidateDomain(domain);
    }

    #endregion Domain

    #region IPAddress

    [TestCase("123.123.123.123", UriHostNameType.Unknown, ExpectedResult = false)]
    [TestCase("123.123.123.123", UriHostNameType.Basic, ExpectedResult = false)]
    [TestCase("123.123.123.123", UriHostNameType.Dns, ExpectedResult = false)]
    [TestCase("123.123.123.123", UriHostNameType.IPv4, ExpectedResult = true)]
    [TestCase("123.123.123.123", UriHostNameType.IPv6, ExpectedResult = false)]

    [TestCase("3002:0bd6:0000:0000:0000:ee00:0033:6778", UriHostNameType.Unknown, ExpectedResult = false)]
    [TestCase("3002:0bd6:0000:0000:0000:ee00:0033:6778", UriHostNameType.Basic, ExpectedResult = false)]
    [TestCase("3002:0bd6:0000:0000:0000:ee00:0033:6778", UriHostNameType.Dns, ExpectedResult = false)]
    [TestCase("3002:0bd6:0000:0000:0000:ee00:0033:6778", UriHostNameType.IPv4, ExpectedResult = false)]
    [TestCase("3002:0bd6:0000:0000:0000:ee00:0033:6778", UriHostNameType.IPv6, ExpectedResult = true)]

    [TestCase("3002:0bd6:0000:0000:0000:ee00:0033:677z", UriHostNameType.Unknown, ExpectedResult = false)]
    [TestCase("3002:0bd6:0000:0000:0000:ee00:0033:677z", UriHostNameType.Basic, ExpectedResult = false)]
    [TestCase("3002:0bd6:0000:0000:0000:ee00:0033:677z", UriHostNameType.Dns, ExpectedResult = false)]
    [TestCase("3002:0bd6:0000:0000:0000:ee00:0033:677z", UriHostNameType.IPv4, ExpectedResult = false)]
    [TestCase("3002:0bd6:0000:0000:0000:ee00:0033:677z", UriHostNameType.IPv6, ExpectedResult = false)]

    [TestCase("[123.123.123.123]", UriHostNameType.Unknown, ExpectedResult = false)]
    [TestCase("[123.123.123.123]", UriHostNameType.Basic, ExpectedResult = false)]
    [TestCase("[123.123.123.123]", UriHostNameType.Dns, ExpectedResult = false)]
    [TestCase("[123.123.123.123]", UriHostNameType.IPv4, ExpectedResult = false)]
    [TestCase("[123.123.123.123]", UriHostNameType.IPv6, ExpectedResult = false)]

    [TestCase("abc", UriHostNameType.Unknown, ExpectedResult = false)]
    [TestCase("abc", UriHostNameType.Basic, ExpectedResult = false)]
    [TestCase("abc", UriHostNameType.Dns, ExpectedResult = false)]
    [TestCase("abc", UriHostNameType.IPv4, ExpectedResult = false)]
    [TestCase("abc", UriHostNameType.IPv6, ExpectedResult = false)]

    [TestCase("a.b.c.d", UriHostNameType.Unknown, ExpectedResult = false)]
    [TestCase("a.b.c.d", UriHostNameType.Basic, ExpectedResult = false)]
    [TestCase("a.b.c.d", UriHostNameType.Dns, ExpectedResult = false)]
    [TestCase("a.b.c.d", UriHostNameType.IPv4, ExpectedResult = false)]
    [TestCase("a.b.c.d", UriHostNameType.IPv6, ExpectedResult = false)]

    [TestCase("1.b.2.3", UriHostNameType.Unknown, ExpectedResult = false)]
    [TestCase("1.b.2.3", UriHostNameType.Basic, ExpectedResult = false)]
    [TestCase("1.b.2.3", UriHostNameType.Dns, ExpectedResult = false)]
    [TestCase("1.b.2.3", UriHostNameType.IPv4, ExpectedResult = false)]
    [TestCase("1.b.2.3", UriHostNameType.IPv6, ExpectedResult = false)]

    [TestCase("111.222.333.44444", UriHostNameType.Unknown, ExpectedResult = false)]
    [TestCase("111.222.333.44444", UriHostNameType.Basic, ExpectedResult = false)]
    [TestCase("111.222.333.44444", UriHostNameType.Dns, ExpectedResult = false)]
    [TestCase("111.222.333.44444", UriHostNameType.IPv4, ExpectedResult = false)]
    [TestCase("111.222.333.44444", UriHostNameType.IPv6, ExpectedResult = false)]
    public bool ValidateIpAddress_Valid(string ipAddress, UriHostNameType uriHostNameType)
    {
        return Validators.ValidateIpAddress(ipAddress, uriHostNameType);
    }

    [TestCase(null, UriHostNameType.Unknown)]
    [TestCase(null, UriHostNameType.Basic)]
    [TestCase(null, UriHostNameType.Dns)]
    [TestCase(null, UriHostNameType.IPv4)]
    [TestCase(null, UriHostNameType.IPv6)]
    public void ValidateIpAddress_ShouldThrowArgumentNullException(string? ipAddress, UriHostNameType uriHostNameType)
    {
        Assert.That(() => Validators.ValidateIpAddress(ipAddress!, uriHostNameType), Throws.TypeOf<ArgumentNullException>());
    }

    //IPv4 must be a strict dotted quad: no shorthand, no hexadecimal, no leading zeros, no surrounding white space
    [TestCase("1.2.3.4", ExpectedResult = true)]
    [TestCase("0.0.0.0", ExpectedResult = true)]
    [TestCase("255.255.255.255", ExpectedResult = true)]
    [TestCase("256.1.1.1", ExpectedResult = false)]
    [TestCase("1.2.3.4.5", ExpectedResult = false)]
    [TestCase("1.2.3.", ExpectedResult = false)]
    [TestCase("1", ExpectedResult = false)]
    [TestCase("1.2.3", ExpectedResult = false)]
    [TestCase("127.1", ExpectedResult = false)]
    [TestCase("2130706433", ExpectedResult = false)]
    [TestCase("01.02.03.04", ExpectedResult = false)]
    [TestCase("0x7f.0.0.1", ExpectedResult = false)]
    [TestCase("", ExpectedResult = false)]
    [TestCase(" 1.2.3.4", ExpectedResult = false)]
    [TestCase("1.2.3.4 ", ExpectedResult = false)]
    [TestCase("::1", ExpectedResult = false)]
    [TestCase("::ffff:1.2.3.4", ExpectedResult = false)]
    public bool ValidateIpAddress_IPv4_Strict(string ipAddress)
    {
        return Validators.ValidateIpAddress(ipAddress, UriHostNameType.IPv4);
    }

    //IPv6 accepts the standard compressed forms but not brackets or ports
    [TestCase("::1", ExpectedResult = true)]
    [TestCase("::", ExpectedResult = true)]
    [TestCase("2001:db8::1", ExpectedResult = true)]
    [TestCase("2001:DB8::1", ExpectedResult = true)]
    [TestCase("2001:db8:0:0:0:0:0:1", ExpectedResult = true)]
    [TestCase("1:2:3:4:5:6:7:8", ExpectedResult = true)]
    [TestCase("::ffff:1.2.3.4", ExpectedResult = true)]
    [TestCase("fe80::1%1", ExpectedResult = true)]
    [TestCase("1:2:3:4:5:6:7:8:9", ExpectedResult = false)]
    [TestCase(":::", ExpectedResult = false)]
    [TestCase("12345::", ExpectedResult = false)]
    [TestCase("g::1", ExpectedResult = false)]
    [TestCase("[::1]", ExpectedResult = false)]
    [TestCase("[::1]:80", ExpectedResult = false)]
    [TestCase("[2001:db8::1", ExpectedResult = false)]
    [TestCase("2001:db8::1]", ExpectedResult = false)]
    [TestCase("1.2.3.4", ExpectedResult = false)]
    [TestCase("", ExpectedResult = false)]
    [TestCase(" ::1", ExpectedResult = false)]
    [TestCase("::1 ", ExpectedResult = false)]
    public bool ValidateIpAddress_IPv6_Valid(string ipAddress)
    {
        return Validators.ValidateIpAddress(ipAddress, UriHostNameType.IPv6);
    }

    #endregion IPAddress

    #region Hostname

    [TestCase("d.com", ExpectedResult = false)]
    [TestCase("zfgdzgdgfd.com", ExpectedResult = false)]
    [TestCase("https://google.com", ExpectedResult = false)]
    [TestCase("protonmail.com", ExpectedResult = true)]
    [TestCase("google.com", ExpectedResult = true)]
    [TestCase("domaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomain.com", ExpectedResult = false)]
    [TestCase("domaindomaindomaindomaindomaindomaindomaindomaindomaindomaindomain.domain.com", ExpectedResult = false)]
    public async Task<bool> ParseHostNameAsync_Valid(string hostname)
    {
        IPHostEntry? iPHostEntry = await Validators.ParseHostNameAsync(hostname);

        return !Equals(iPHostEntry, null);
    }

    [TestCase()]
    public async Task ParseHostNameAsync_WithNull_ShouldReturnNull()
    {
        IPHostEntry? result = await Validators.ParseHostNameAsync(null!);

        Assert.That(result, Is.Null);
    }

    //These do not need network access
    [TestCase("localhost", ExpectedResult = true)]
    [TestCase("127.0.0.1", ExpectedResult = true)]
    [TestCase("", ExpectedResult = false)]
    [TestCase(" ", ExpectedResult = false)]
    [TestCase("   ", ExpectedResult = false)]
    [TestCase("\t", ExpectedResult = false)]
    [TestCase("bad host", ExpectedResult = false)]
    public async Task<bool> ParseHostNameAsync_WithoutNetwork_Valid(string hostname)
    {
        IPHostEntry? iPHostEntry = await Validators.ParseHostNameAsync(hostname);

        return iPHostEntry != null;
    }

    #endregion Hostname

    #endregion DNS Validations

    #region Duplicate Validations

    #region Source Data

    internal sealed class TestDup
    {
        public int IntValue;

        public string StringValue = string.Empty;
    }

    private static readonly int[] _validListNoFunc = [1, 2, 3];
    private static readonly int[] _invalidListNoFunc = [1, 1, 2];
    private static readonly TestDup[] _validListWithFunc =
    [
        new TestDup() { IntValue = 1, StringValue = "A" },
        new TestDup() { IntValue = 2, StringValue = "B" },
        new TestDup() { IntValue = 3, StringValue = "C" }
    ];
    private static readonly TestDup[] _invalidListWithFunc =
    [
        new TestDup() { IntValue = 1, StringValue = "A" },
        new TestDup() { IntValue = 1, StringValue = "B" },
        new TestDup() { IntValue = 3, StringValue = "C" }
    ];

    private static readonly object[] _noFunc =
    [
        new object?[] { _validListNoFunc, true, null },
        new object[] { _invalidListNoFunc, false, Enumerable.Repeat(1, 1)},
    ];

    private static readonly object[] _withFunc =
    [
        new object[] { _validListWithFunc, true, null!},
        new object[] { _invalidListWithFunc, false, Enumerable.Repeat(1, 1) }
    ];

    #endregion Source Data

    [TestCaseSource(nameof(_noFunc))]
    public void TryValidateDuplicatedItems_NoFunc_Valid<T>(IEnumerable<T> items, bool expectedResult, IEnumerable<T>? expectedDuplicatedValues)
    {
        bool result = Validators.TryValidateDuplicatedItems(items, out IEnumerable<T>? duplicatedValues);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(expectedResult, Is.EqualTo(result));

            Assert.That(() =>
            {
                if (result)
                {
                    return duplicatedValues == null;
                }

                if (duplicatedValues == null)
                {
                    return false;
                }

                return duplicatedValues.SequenceEqual(expectedDuplicatedValues!);
            });
        }
    }

    [TestCaseSource(nameof(_withFunc))]
    public void TryValidateDuplicatedItems_WithFunc_Valid(IEnumerable<TestDup> items, bool expectedResult, IEnumerable<int>? expectedDuplicatedValues)
    {
        bool result = Validators.TryValidateDuplicatedItems(items, (g) => { return g.IntValue; }, (s) => { return s.Key; }, out IEnumerable<int>? duplicatedValues);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(expectedResult, Is.EqualTo(result));

            Assert.That(() =>
            {
                if (result)
                {
                    return duplicatedValues == null;
                }

                if (duplicatedValues == null)
                {
                    return false;
                }

                return duplicatedValues.SequenceEqual(expectedDuplicatedValues!);
            });
        }
    }

    [TestCase(null)]
    public void TryValidateDuplicatedItems_ShouldThrowArgumentNullException(int[]? items)
    {
        Assert.That(() => Validators.TryValidateDuplicatedItems(items!, out _), Throws.TypeOf<ArgumentNullException>());
    }

    #region Duplicate Validations (additional scenarios)

    private sealed class NullableKeyItem
    {
        public string? Key { get; init; }

        public int Number { get; init; }
    }

    #region No Func

    [TestCase()]
    public void TryValidateDuplicatedItems_NoFunc_WithMultipleNulls_ShouldReportNullAsDuplicate()
    {
        string?[] items = ["a", null, null, "b"];

        bool result = Validators.TryValidateDuplicatedItems(items, out IEnumerable<string?>? duplicatedValues);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.False);
            Assert.That(duplicatedValues, Is.EqualTo(new string?[] { null }));
        }
    }

    [TestCase()]
    public void TryValidateDuplicatedItems_NoFunc_WithSingleNull_ShouldBeValid()
    {
        string?[] items = ["a", null, "b"];

        bool result = Validators.TryValidateDuplicatedItems(items, out IEnumerable<string?>? duplicatedValues);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.True);
            Assert.That(duplicatedValues, Is.Null);
        }
    }

    [TestCase()]
    public void TryValidateDuplicatedItems_NoFunc_WithNullsAndOtherDuplicates_ShouldReportInFirstOccurrenceOrder()
    {
        string?[] items = ["b", null, "a", "b", null, "a"];

        bool result = Validators.TryValidateDuplicatedItems(items, out IEnumerable<string?>? duplicatedValues);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.False);
            Assert.That(duplicatedValues, Is.EqualTo(new string?[] { "b", null, "a" }));
        }
    }

    [TestCase()]
    public void TryValidateDuplicatedItems_NoFunc_WithSeveralDuplicates_ShouldReportInFirstOccurrenceOrder()
    {
        int[] items = [3, 1, 3, 2, 1, 2];

        bool result = Validators.TryValidateDuplicatedItems(items, out IEnumerable<int>? duplicatedValues);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.False);
            Assert.That(duplicatedValues, Is.EqualTo(new[] { 3, 1, 2 }));
        }
    }

    [TestCase()]
    public void TryValidateDuplicatedItems_NoFunc_WithValueAppearingSeveralTimes_ShouldReportItOnce()
    {
        int[] items = [7, 7, 7, 8];

        bool result = Validators.TryValidateDuplicatedItems(items, out IEnumerable<int>? duplicatedValues);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.False);
            Assert.That(duplicatedValues, Is.EqualTo(new[] { 7 }));
        }
    }

    [TestCase()]
    public void TryValidateDuplicatedItems_NoFunc_WithEmptyCollection_ShouldBeValid()
    {
        bool result = Validators.TryValidateDuplicatedItems(Array.Empty<int>(), out IEnumerable<int>? duplicatedValues);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.True);
            Assert.That(duplicatedValues, Is.Null);
        }
    }

    [TestCase()]
    public void TryValidateDuplicatedItems_NoFunc_WithSingleItem_ShouldBeValid()
    {
        bool result = Validators.TryValidateDuplicatedItems([1], out IEnumerable<int>? duplicatedValues);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.True);
            Assert.That(duplicatedValues, Is.Null);
        }
    }

    [TestCase()]
    public void TryValidateDuplicatedItems_NoFunc_WithStrings_ShouldBeCaseSensitive()
    {
        string[] items = ["a", "A"];

        bool result = Validators.TryValidateDuplicatedItems(items, out IEnumerable<string>? duplicatedValues);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.True);
            Assert.That(duplicatedValues, Is.Null);
        }
    }

    [TestCase()]
    public void TryValidateDuplicatedItems_NoFunc_ShouldEnumerateSourceOnlyOnce()
    {
        int enumerations = 0;

        IEnumerable<int> Source()
        {
            enumerations++;

            yield return 1;
            yield return 1;
            yield return 2;
        }

        bool result = Validators.TryValidateDuplicatedItems(Source(), out IEnumerable<int>? duplicatedValues);
        int enumerationsAfterCall = enumerations;

        //the caller can enumerate the result as often as needed without running the source again
        List<int> firstPass = [.. duplicatedValues!];
        List<int> secondPass = [.. duplicatedValues!];

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.False);
            Assert.That(enumerationsAfterCall, Is.EqualTo(1));
            Assert.That(enumerations, Is.EqualTo(1));
            Assert.That(firstPass, Is.EqualTo(new[] { 1 }));
            Assert.That(secondPass, Is.EqualTo(new[] { 1 }));
        }
    }

    [TestCase()]
    public void TryValidateDuplicatedItems_NoFunc_ShouldNotBeAffectedBySourceChangesAfterTheCall()
    {
        List<int> items = [1, 1, 2];

        Validators.TryValidateDuplicatedItems(items, out IEnumerable<int>? duplicatedValues);

        items.AddRange([2, 2]);

        Assert.That(duplicatedValues, Is.EqualTo(new[] { 1 }));
    }

    #endregion No Func

    #region With Func

    [TestCase()]
    public void TryValidateDuplicatedItems_WithFunc_WithMultipleNullKeys_ShouldReportNullAsDuplicate()
    {
        NullableKeyItem[] items =
        [
            new() { Key = null, Number = 1 },
            new() { Key = null, Number = 2 },
            new() { Key = "a", Number = 3 }
        ];

        bool result = Validators.TryValidateDuplicatedItems(items, (i) => i.Key, (g) => g.Key, out IEnumerable<string?>? duplicatedValues);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.False);
            Assert.That(duplicatedValues, Is.EqualTo(new string?[] { null }));
        }
    }

    [TestCase()]
    public void TryValidateDuplicatedItems_WithFunc_WithSingleNullKey_ShouldBeValid()
    {
        NullableKeyItem[] items =
        [
            new() { Key = null, Number = 1 },
            new() { Key = "a", Number = 2 }
        ];

        bool result = Validators.TryValidateDuplicatedItems(items, (i) => i.Key, (g) => g.Key, out IEnumerable<string?>? duplicatedValues);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.True);
            Assert.That(duplicatedValues, Is.Null);
        }
    }

    [TestCase()]
    public void TryValidateDuplicatedItems_WithFunc_WithSeveralDuplicates_ShouldReportInFirstOccurrenceOrder()
    {
        int[] items = [3, 1, 3, 2, 1, 2];

        bool result = Validators.TryValidateDuplicatedItems(items, (i) => i, (g) => g.Key, out IEnumerable<int>? duplicatedValues);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.False);
            Assert.That(duplicatedValues, Is.EqualTo(new[] { 3, 1, 2 }));
        }
    }

    [TestCase()]
    public void TryValidateDuplicatedItems_WithFunc_WithCustomSelect_ShouldReturnSelectedValues()
    {
        //groups by remainder: 1 => [1, 4], 2 => [2, 5], 0 => [3]. Only keys 1 and 2 are duplicated
        int[] items = [1, 4, 2, 5, 3];

        bool result = Validators.TryValidateDuplicatedItems(items, (i) => i % 3, (g) => g.Key * 10, out IEnumerable<int>? duplicatedValues);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.False);
            Assert.That(duplicatedValues, Is.EqualTo(new[] { 10, 20 }));
        }
    }

    [TestCase()]
    public void TryValidateDuplicatedItems_WithFunc_WithSelectUsingGroupContent_ShouldReturnSelectedValues()
    {
        TestDup[] items =
        [
            new() { IntValue = 1, StringValue = "A" },
            new() { IntValue = 1, StringValue = "B" },
            new() { IntValue = 1, StringValue = "C" },
            new() { IntValue = 3, StringValue = "D" }
        ];

        //the selector returns the number of items in each duplicated group instead of the key
        bool result = Validators.TryValidateDuplicatedItems(items, (i) => i.IntValue, (g) => g.Count(), out IEnumerable<int>? duplicatedValues);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.False);
            Assert.That(duplicatedValues, Is.EqualTo(new[] { 3 }));
        }
    }

    [TestCase()]
    public void TryValidateDuplicatedItems_WithFunc_WithEmptyCollection_ShouldBeValid()
    {
        bool result = Validators.TryValidateDuplicatedItems(Array.Empty<int>(), (i) => i, (g) => g.Key, out IEnumerable<int>? duplicatedValues);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.True);
            Assert.That(duplicatedValues, Is.Null);
        }
    }

    [TestCase()]
    public void TryValidateDuplicatedItems_WithFunc_ShouldEnumerateSourceOnlyOnce()
    {
        int enumerations = 0;

        IEnumerable<int> Source()
        {
            enumerations++;

            yield return 1;
            yield return 1;
            yield return 2;
        }

        bool result = Validators.TryValidateDuplicatedItems(Source(), (i) => i, (g) => g.Key, out IEnumerable<int>? duplicatedValues);
        int enumerationsAfterCall = enumerations;

        List<int> firstPass = [.. duplicatedValues!];
        List<int> secondPass = [.. duplicatedValues!];

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.False);
            Assert.That(enumerationsAfterCall, Is.EqualTo(1));
            Assert.That(enumerations, Is.EqualTo(1));
            Assert.That(firstPass, Is.EqualTo(new[] { 1 }));
            Assert.That(secondPass, Is.EqualTo(new[] { 1 }));
        }
    }

    [TestCase()]
    public void TryValidateDuplicatedItems_WithFunc_WithNullItems_ShouldThrowArgumentNullException()
    {
        int[]? items = null;

        Assert.That(() => Validators.TryValidateDuplicatedItems(items!, (i) => i, (g) => g.Key, out _), Throws.TypeOf<ArgumentNullException>());
    }

    [TestCase()]
    public void TryValidateDuplicatedItems_WithFunc_WithNullGroupByFunc_ShouldThrowArgumentNullException()
    {
        int[] items = [1, 2];
        Func<int, int>? groupByFunc = null;

        Assert.That(() => Validators.TryValidateDuplicatedItems(items, groupByFunc!, (g) => g.Key, out _), Throws.TypeOf<ArgumentNullException>());
    }

    [TestCase()]
    public void TryValidateDuplicatedItems_WithFunc_WithNullSelectFunc_ShouldThrowArgumentNullException()
    {
        int[] items = [1, 2];
        Func<IGrouping<int, int>, int>? selectFunc = null;

        Assert.That(() => Validators.TryValidateDuplicatedItems(items, (i) => i, selectFunc!, out _), Throws.TypeOf<ArgumentNullException>());
    }

    #endregion With Func

    #endregion Duplicate Validations (additional scenarios)

    #endregion Duplicate Validations

    #region Stored Proc Content Validations

    [TestCase(1, new int[0])]
    public void ValidateCheckStoredProcs_Valid(int uniqueKey, ICollection<int> expectedCollection)
    {
        DataTable dataTable = LoadDataTable([(uniqueKey, 1)]);

        IEnumerable<(int uniqueKey, Func<DataRow, (int data, bool isInvalid)> validator)> validators =
        [
            (1, (dataRow) =>
            {
                int value = Convert.ToInt32(dataRow["Value"]);

                bool exists = value == 1;

                return (value, !exists);
            })
        ];

        IDictionary<int, ICollection<int>> data = Validators.ValidateCheckStoredProcs(dataTable, validators);

        Assert.That(data, Has.Count.EqualTo(expectedCollection.Count));
    }

    [TestCase(2, new int[1] { 1 })]
    public void ValidateCheckStoredProcs_Invalid(int expectedInvalidKey, int[] expectedInvalidData)
    {
        DataTable dataTable = LoadDataTable([(1, 1)]);

        IEnumerable<(int uniqueKey, Func<DataRow, (int data, bool isInvalid)> validator)> validators =
        [
            (1, (dataRow) =>
            {
                int value = Convert.ToInt32(dataRow["Value"]);

                bool exists = value == 1;

                return (value, !exists);
            }),
            ( 2, (dataRow) =>
            {
                int value = Convert.ToInt32(dataRow["Value"]);

                bool exists = value == 1;

                return (value, exists);
            })
        ];

        IDictionary<int, ICollection<int>> data = Validators.ValidateCheckStoredProcs(dataTable, validators);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(data, Has.Count.EqualTo(1));
            Assert.That(data.ContainsKey(expectedInvalidKey), Is.True);
            Assert.That(data[expectedInvalidKey], Is.EqualTo(expectedInvalidData));
        }
    }

    [TestCase()]
    public void ValidateCheckStoredProcs_WithSeveralInvalidRows_ShouldAccumulateUnderTheSameKey()
    {
        DataTable dataTable = LoadDataTable([(1, 1), (2, 2), (3, 3)]);

        IEnumerable<(int uniqueKey, Func<DataRow, (int data, bool isInvalid)> validator)> validators =
        [
            (1, (dataRow) =>
            {
                int value = Convert.ToInt32(dataRow["Value"]);

                return (value, value != 1);
            })
        ];

        IDictionary<int, ICollection<int>> data = Validators.ValidateCheckStoredProcs(dataTable, validators);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(data, Has.Count.EqualTo(1));
            Assert.That(data.ContainsKey(1), Is.True);
            Assert.That(data[1], Is.EqualTo(new[] { 2, 3 }));
        }
    }

    [TestCase()]
    public void ValidateCheckStoredProcs_WithSeveralFailingValidators_ShouldReturnOneEntryPerFailingValidator()
    {
        DataTable dataTable = LoadDataTable([(1, 5)]);

        IEnumerable<(int uniqueKey, Func<DataRow, (int data, bool isInvalid)> validator)> validators =
        [
            (10, (dataRow) => (Convert.ToInt32(dataRow["Value"]), true)),
            (20, (dataRow) => (Convert.ToInt32(dataRow["Value"]) * 2, true)),
            (30, (dataRow) => (Convert.ToInt32(dataRow["Value"]), false))
        ];

        IDictionary<int, ICollection<int>> data = Validators.ValidateCheckStoredProcs(dataTable, validators);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(data, Has.Count.EqualTo(2));
            Assert.That(data[10], Is.EqualTo(new[] { 5 }));
            Assert.That(data[20], Is.EqualTo(new[] { 10 }));
            Assert.That(data.ContainsKey(30), Is.False);
        }
    }

    [TestCase()]
    public void ValidateCheckStoredProcs_WithOnlyValidRows_ShouldReturnEmptyDictionary()
    {
        DataTable dataTable = LoadDataTable([(1, 1), (2, 1)]);

        IEnumerable<(int uniqueKey, Func<DataRow, (int data, bool isInvalid)> validator)> validators =
        [
            (1, (dataRow) => (Convert.ToInt32(dataRow["Value"]), false))
        ];

        Assert.That(Validators.ValidateCheckStoredProcs(dataTable, validators), Is.Empty);
    }

    [TestCase()]
    public void ValidateCheckStoredProcs_WithEmptyTable_ShouldReturnEmptyDictionary()
    {
        DataTable dataTable = LoadDataTable([]);

        IEnumerable<(int uniqueKey, Func<DataRow, (int data, bool isInvalid)> validator)> validators =
        [
            (1, (dataRow) => (Convert.ToInt32(dataRow["Value"]), true))
        ];

        Assert.That(Validators.ValidateCheckStoredProcs(dataTable, validators), Is.Empty);
    }

    [TestCase()]
    public void ValidateCheckStoredProcs_WithNoValidators_ShouldReturnEmptyDictionary()
    {
        DataTable dataTable = LoadDataTable([(1, 1)]);

        IEnumerable<(int uniqueKey, Func<DataRow, (int data, bool isInvalid)> validator)> validators = [];

        Assert.That(Validators.ValidateCheckStoredProcs(dataTable, validators), Is.Empty);
    }

    [TestCase()]
    public void ValidateCheckStoredProcs_ShouldRunEveryValidatorOnEveryRow()
    {
        DataTable dataTable = LoadDataTable([(1, 1), (2, 2), (3, 3)]);
        int calls = 0;

        IEnumerable<(int uniqueKey, Func<DataRow, (int data, bool isInvalid)> validator)> validators =
        [
            (1, (dataRow) =>
            {
                calls++;

                return (Convert.ToInt32(dataRow["Value"]), false);
            }),
            (2, (dataRow) =>
            {
                calls++;

                return (Convert.ToInt32(dataRow["Value"]), false);
            })
        ];

        Validators.ValidateCheckStoredProcs(dataTable, validators);

        Assert.That(calls, Is.EqualTo(6));
    }

    private static DataTable LoadDataTable(IEnumerable<(int key, int value)> values)
    {
        DataTable dataTable = new();
        dataTable.Columns.Add("ID", typeof(int));
        dataTable.Columns.Add("Value", typeof(int));

        foreach ((int key, int value) in values)
        {
            DataRow dataRow = dataTable.NewRow();

            dataRow["ID"] = key;
            dataRow["Value"] = value;

            dataTable.Rows.Add(dataRow);
        }

        return dataTable;
    }

    #endregion Stored Proc Content Validations
}