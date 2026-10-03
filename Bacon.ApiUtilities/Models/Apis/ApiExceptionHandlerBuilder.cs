using Microsoft.Extensions.DependencyInjection;

namespace Bacon.ApiUtilities.Models.Apis;

/// <summary>
/// Api document builder
/// </summary>
public sealed class ApiExceptionHandlerBuilder(IServiceCollection serviceCollection)
{
    internal IServiceCollection ServiceCollection { get; } = serviceCollection;
}