using Bacon.ApiUtilities.Interfaces.Services.Apis;
using Microsoft.Extensions.Localization;

namespace Bacon.ApiUtilities.Services.Apis;

internal sealed class ErrorLocalizerService<TResource>(IStringLocalizer<TResource> stringLocalizer) : IErrorLocalizerService
{
    public string GetResourceValue(string resourceKey)
    {
        return stringLocalizer[resourceKey];
    }
}