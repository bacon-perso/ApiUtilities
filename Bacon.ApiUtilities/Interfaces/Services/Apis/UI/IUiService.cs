using Microsoft.AspNetCore.Builder;

namespace Bacon.ApiUtilities.Interfaces.Services.Apis.UI;

internal interface IUiService
{
    void UseUI(WebApplication webApplication);
}