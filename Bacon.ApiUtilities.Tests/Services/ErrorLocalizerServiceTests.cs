using Bacon.ApiUtilities.Services.Apis;
using Bacon.ApiUtilities.Tests.Support;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Bacon.ApiUtilities.Tests.Services;

[TestFixture]
internal sealed class ErrorLocalizerServiceTests
{
    private sealed class StubStringLocalizer(Dictionary<string, string> resources) : IStringLocalizer<TestResources>
    {
        public LocalizedString this[string name] => resources.TryGetValue(name, out string? value) ? new(name, value, resourceNotFound: false) : new(name, name, resourceNotFound: true);

        public LocalizedString this[string name, params object[] arguments] => this[name];

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            return resources.Select(s => new LocalizedString(s.Key, s.Value, resourceNotFound: false));
        }
    }

    [Test]
    public void GetResourceValue_WithAKnownKey_ShouldReturnTheLocalizedValue()
    {
        ErrorLocalizerService<TestResources> service = new(new StubStringLocalizer(new() { ["412001"] = "{0} is invalid" }));

        Assert.That(service.GetResourceValue("412001"), Is.EqualTo("{0} is invalid"));
    }

    [Test]
    public void GetResourceValue_WithAnUnknownKey_ShouldReturnTheKey()
    {
        ErrorLocalizerService<TestResources> service = new(new StubStringLocalizer([]));

        Assert.That(service.GetResourceValue("999999"), Is.EqualTo("999999"));
    }

    [Test]
    public void GetResourceValue_WithTheFrameworkLocalizerAndNoResourceFile_ShouldReturnTheKeyAndNeverNull()
    {
        using ServiceProvider serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddLocalization()
            .BuildServiceProvider();

        ErrorLocalizerService<TestResources> service = new(serviceProvider.GetRequiredService<IStringLocalizer<TestResources>>());

        Assert.That(service.GetResourceValue("123456"), Is.EqualTo("123456"));
    }
}
