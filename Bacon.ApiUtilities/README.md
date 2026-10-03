# Bacon.ApiUtilities

ASP.NET Core (`net10.0`) building blocks for Web APIs: a unified exception/validation-error pipeline with localized, internally-coded error responses, OpenAPI documentation generation (Swagger UI / Scalar) driven by multiple documents, endpoint metadata attributes, request rate limiting, and a set of ready-made model validation attributes.

A fully working example of every feature below lives in the **`Bacon.ApiUtilities.Samples`** project (see [Sample walkthrough](#sample-walkthrough)) — run it and browse `/` (Swagger UI) to see it live.

## Installation

```xml
<ItemGroup>
  <ProjectReference Include="..\Bacon.ApiUtilities\Bacon.ApiUtilities.csproj" />
</ItemGroup>
```

Depends on `Microsoft.AspNetCore.OpenApi`, `Microsoft.OpenApi`, `Scalar.AspNetCore`, `Swashbuckle.AspNetCore.SwaggerUI`, and `Bacon.Utilities`.

## Quick start

`Program.cs` (see `Bacon.ApiUtilities.Samples/Program.cs` for the full example):

```csharp
using Bacon.ApiUtilities.Extensions;
using Bacon.ApiUtilities.Models.Apis.Documentations;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddApiExceptionHandler<ErrorCodeResources>(o =>
    {
        // First 3 digits of each code must match the corresponding HTTP status code
        o.InternalServerErrorInternalErrorCode = 500000;
        o.UnhandledValidationInternalErrorCode = 412000;

        o.BuiltInValidationAttributeSettings = ApiExceptionHandlerSettings.GetBuiltInValidationAttributeSettings();
        o.CustomValidationAttributeSettings = ApiExceptionHandlerSettings.GetCustomValidationattributeSettings();
    })
    .AddApiRateLimiting(o =>
    {
        o.RateLimitInternalErrorCode = 429000;
    })
    .AddOpenApiDocumentation(o =>
    {
        o.OpenApiDocumentInfos =
        [
            new() { Name = "sample1", Title = "Sample API 1", Version = "1" }
        ];
        o.DisplayHiddenEndpoints = builder.Environment.IsDevelopment();
        o.UiConfigs.UiType = Bacon.ApiUtilities.Models.UiTypes.Swagger; // or .Scalar
        o.UiConfigs.VirtualPath = string.Empty;
    });

WebApplication app = builder.Build();

app.UseRouting();              // required before UseOutputCache and UseRateLimiter: both read the endpoint metadata
app.UseCors();

app.UseRequestLocalization();  // if the application is localized: before UseOutputCache, so cached documents are kept per language

app.UseOutputCache();          // needed when OutputCacheDuration is above zero; after UseRouting (and after UseRequestLocalization): OpenAPI documents use an output cache policy

app.UseApiExceptionHandler();  // turns CustomException and unhandled exceptions into an ErrorResult. Keep it before UseStatusCodePages
app.UseStatusCodePages();      // fills in the body (through ProblemDetails) of framework errors that are not exceptions, such as an unknown route

app.UseOpenApiDocumentation();

app.UseAuthentication();       // before UseRateLimiter so the caller's identity is known
app.UseAuthorization();

app.UseRateLimiter();          // required for [EndpointRateLimit] to have any effect

app.MapControllers();
await app.RunAsync();
```

`ErrorCodeResources` is a plain marker class paired with `.resx` files (e.g. `ErrorCodeResources.en-US.resx`) whose keys are the internal error codes (as strings) and whose values are message templates (`{0}`, `{1}`, ...). See [Error codes & localization](#error-codes--localization).

### Required middleware and ordering

Some features depend on ASP.NET Core middleware that the library **cannot add for you**. If one is missing, nothing fails at startup: the feature is silently inactive.

| Middleware | Needed for | If missing |
|---|---|---|
| `app.UseOutputCache()` | `AddOpenApiDocumentation` (caches the generated OpenAPI documents, see `OutputCacheDuration`). It only has an effect when `OutputCacheDuration` is greater than zero, which is not the default (no caching) | The documents are never cached: they are regenerated on every request. Placed **before** `UseRouting()`, it also caches nothing, without any error |
| `app.UseRouting()` | `UseOutputCache()` and `[EndpointRateLimit]` (the endpoint must be resolved first: the cache policy of the OpenAPI endpoint and the rate limit are read from its metadata) | Endpoint metadata is not found, so nothing is cached and no rate limiting is applied |
| `app.UseRequestLocalization()` | Localized applications: the error messages written in the OpenAPI documents use the culture of the request, and the cached documents are kept per resolved culture (the configured default culture when the request does not ask for another one) | No effect if the application is not localized. If it runs **after** `UseOutputCache()`, the cache cannot see the request culture: every language receives the document generated for the first request, until the cache expires, without any error |
| `app.UseApiExceptionHandler()` | Error responses for a thrown `CustomException` or an unhandled exception. Rate limiting rejections do **not** need it: the rate limiter writes its own `ErrorResult` | A thrown `CustomException` or an unhandled exception is not turned into an `ErrorResult` |
| `app.UseAuthentication()` / `app.UseAuthorization()` | `[EndpointRateLimit]` caller identity (user or client id) | Callers are identified by IP address only |
| `app.UseRateLimiter()` | `[EndpointRateLimit]` | `[EndpointRateLimit]` is documented in OpenAPI but **never enforced** |

The order that matters:

1. `UseRouting()` before `UseOutputCache()`. The output cache reads the cache policy from the endpoint that routing selects. Called earlier, it does not see the policy set by `AddOpenApiDocumentation`, and the OpenAPI documents are silently not cached. The Samples call it just before `UseCors()`.
2. `UseRequestLocalization()` before `UseOutputCache()`. The cached OpenAPI documents vary by the culture that the localization middleware resolved for the request (the default culture when the `Accept-Language` header is missing or not supported). The output cache reads that culture when the request reaches it, so the localization middleware must have run already.
3. `UseRouting()` before `UseRateLimiter()`.
4. `UseAuthentication()` and `UseAuthorization()` before `UseRateLimiter()`.
5. `UseApiExceptionHandler()` before `UseStatusCodePages()`, and early in the pipeline. It only handles **thrown exceptions**, so it must wrap the middleware and endpoints that can throw. This is the order used by the Samples.

The position of `UseApiExceptionHandler()` relative to `UseRateLimiter()` does not matter for rate limiting: the rate limiter writes the 429 `ErrorResult` itself. Place `UseApiExceptionHandler()` early anyway, so that it also wraps the rest of the pipeline for thrown exceptions.

#### Framework errors that are not exceptions

`UseApiExceptionHandler()` does not see errors that the framework returns as a status code, such as a call to an endpoint that does not exist (404), a method not allowed (405), an unsupported media type (415) or an unsupported API version (400). No exception is thrown, so they never reach it. Their body is written through ASP.NET Core's ProblemDetails support (`AddProblemDetails()`), and `UseStatusCodePages()` fills in the body of status codes that are returned empty, such as an unknown route. To return these in the `ErrorResult` format as well, register an `IProblemDetailsWriter` for the status codes you want (see `Bacon.ApiUtilities.Samples/ProjectConfigs/ApiVersioningErrorResponseProvider.cs`).

## Exception handling & model validation

### How errors are surfaced

Every response — thrown `CustomException`, an unhandled exception, or a failed model validation attribute — is turned into a consistent JSON `ErrorResult`:

```json
{
  "httpStatusCode": 404,
  "internalCode": 404001,
  "message": "The following cities cannot be found: paris",
  "metadata": ["paris"]
}
```

- `httpStatusCode` — the HTTP status code.
- `internalCode` — your app-specific code; **its first 3 digits must equal the HTTP status code**. This is enforced at startup for the codes you configure in the options. Codes passed to `CustomException` or `[ApiInternalErrorCodes]` are not checked at startup.
- `message` — resolved from the matching `.resx` entry, with `metadata` values substituted via `string.Format`.

Only **one** `ErrorResult` is returned per failed request. When several model validation errors occur, the response carries the first one that uses `UnhandledValidationInternalErrorCode`, otherwise the first error found. Validation failures that do not come from this library's attributes (standard DataAnnotations attributes such as `[StringLength]`, binding errors, ...) are reported with `UnhandledValidationInternalErrorCode`.

### Throwing errors from application code

```csharp
using Bacon.ApiUtilities.Extensions;

cities.Contains(city, StringComparer.OrdinalIgnoreCase)
    .ThrowCustomExceptionIfFalse(HttpStatusCode.NotFound, 404001, city);

cities.Contains(city, StringComparer.OrdinalIgnoreCase)
    .ThrowCustomExceptionIfTrue(HttpStatusCode.Conflict, 409001, city);

// Equivalent to:
if (!cities.Contains(city, StringComparer.OrdinalIgnoreCase))
{
    throw new CustomException(HttpStatusCode.NotFound, 404001, city);
}
```

### Adding your own exception handlers

`AddApiExceptionHandler` registers an `IExceptionHandler` that handles **every** exception: a `CustomException` becomes the `ErrorResult` it describes, and any other exception becomes the 500 `ErrorResult`. ASP.NET Core calls the registered handlers in registration order and stops at the first one that returns `true`. So a handler of your own must be registered **before** `AddApiExceptionHandler` to run. One registered after it is never called, because the library's handler has already handled the exception.

```csharp
builder.Services.AddExceptionHandler<KeyNotFoundExceptionHandler>(); // before AddApiExceptionHandler

builder.Services.AddControllers()
    .AddApiExceptionHandler<ErrorCodeResources>(o => { /* ... */ });

public sealed class KeyNotFoundExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not KeyNotFoundException)
        {
            return false; // not handled here: falls through to the library's handler
        }

        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        await httpContext.Response.WriteAsync("...", cancellationToken);

        return true;
    }
}
```

Return `false` for the exceptions your handler does not deal with, and they are handled by the library as usual.

### Built-in model validation attributes

Under `Bacon.ApiUtilities.Attributes.Validations`, each mapped in `AddApiExceptionHandler`'s `BuiltInValidationAttributeSettings` to an internal error code:

| Attribute | Purpose |
|---|---|
| `RequiredFieldAttribute` | Required (also validates non-empty collections item-by-item) |
| `AllowedMinLengthAttribute` / `AllowedMaxLengthAttribute` | String/collection length bounds |
| `AllowedRangeAttribute` | Numeric range |
| `DuplicatedItemsAttribute` | Collection has no duplicate items |
| `RequiredEnumerableContentAttribute` | Collection is null or contains no null items |
| `EmptyGuidAttribute` | `Guid`/`Guid?`/collections thereof are not `Guid.Empty` |
| `EmailFormatAttribute` | Email format, checked as received with no trimming (delegates to `Bacon.Utilities.Validators.ValidateEmail`) |
| `PhoneNumberFormatAttribute` | Valid phone number (via `libphonenumber-csharp`) |
| `UrlFormatAttribute(UriKind, maxUriLength = 2000)` | Valid absolute/relative URI (`maxUriLength` cannot exceed 4000) |
| `UtcDateTimeFormatAttribute` | `DateTime`/`DateTime?` must have `DateTimeKind.Utc` |

```csharp
public class CreateCaptionRequest
{
    [RequiredField]
    [AllowedMaxLength(256)]
    public string? Text { get; set; }

    [EmailFormat]
    public string? ContactEmail { get; set; }
}
```

### Custom validation attributes

Write your own by inheriting `ValidationAttribute` and resolving `IModelValidationService` from the `ValidationContext` (see `Bacon.ApiUtilities.Samples/Attributes/Validations/AllowedCaptionAttribute.cs`):

```csharp
public class AllowedCaptionAttribute(int maxCaptionTextLength) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        IModelValidationService modelStateValidation = validationContext.GetRequiredService<IModelValidationService>();

        if (/* invalid */ false)
        {
            return modelStateValidation.CreateInvalidModelValidationResult(validationContext, "412011", HttpStatusCode.PreconditionFailed, 412011, maxCaptionTextLength.ToString());
        }

        return ValidationResult.Success;
    }
}
```

Register the type's possible internal error codes via `CustomValidationAttributeSettings` in `AddApiExceptionHandler`:

```csharp
public static Dictionary<Type, IEnumerable<long>> GetCustomValidationattributeSettings() => new()
{
    { typeof(AllowedCaptionAttribute), [412010, 412011, 412012, 412014] },
};
```

### Error codes & localization

Internal error codes are the keys of a resource file (e.g. `Resources/ErrorCodeResources.en-US.resx`):

```xml
<data name="404001" xml:space="preserve"><value>The following cities cannot be found: {0}</value></data>
<data name="412001" xml:space="preserve"><value>The maximum allowed length of '{0}' is {1}</value></data>
```

The `TResource` type passed to `AddApiExceptionHandler<TResource>` is just the marker class .NET's resource manager uses to locate these `.resx` files (standard ASP.NET Core localization conventions — one file per supported culture).

## OpenAPI documentation

`AddOpenApiDocumentation` / `UseOpenApiDocumentation` wrap `Microsoft.AspNetCore.OpenApi` generation with support for **multiple documents**, endpoint grouping, hidden endpoints, and a choice of UI.

```csharp
.AddOpenApiDocumentation(o =>
{
    o.OpenApiDocumentInfos =
    [
        new() { Name = "sample1", Title = "Sample API 1", Version = "1", License = new() { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") } },
        new() { Name = "sample2", Title = "Sample API 2", Version = "1" },
    ];
    o.DisplayHiddenEndpoints = false; // hide [HiddenApi]-tagged endpoints/controllers
    o.OutputCacheDuration = TimeSpan.FromMinutes(5);

    // UI configuration
    o.UiConfigs.UiType = Bacon.ApiUtilities.Models.UiTypes.Swagger; // Swagger | Scalar
    o.UiConfigs.VirtualPath = string.Empty;
    o.UiConfigs.CorsPolicyCss = new() { { "Internal", "#FFAC1C" }, { "CloudFlare", "#F38020" } }; // color-codes CORS policies in the UI
});
```

```csharp
app.UseOpenApiDocumentation(); // serves the documents and the configured UI
```

The UI options (`UiConfigs`) are part of `AddOpenApiDocumentation` and are validated at startup together with the rest of the options.

| Option | Default | Notes |
|---|---|---|
| `OpenApiDocumentInfos` | required | One entry per document: `Name` (unique, URL-friendly, no whitespace), `Title`, `Version`, and optionally `Description` and `License` |
| `DisplayHiddenEndpoints` | `true` | Hidden endpoints are **shown** unless you set it to `false` |
| `OutputCacheDuration` | `TimeSpan.Zero` | No caching by default. See the `UseOutputCache()` requirement above |
| `UiConfigs.UiType` | `Swagger` | `Swagger` or `Scalar` |
| `UiConfigs.VirtualPath` | empty | The UI is served at the root. Use URL-safe segments such as `docs` or `api/docs` |
| `UiConfigs.CollapseEndpoints` | `true` | Applies to both UIs |
| `UiConfigs.SupportedSubmitMethods` | `null` | Swagger UI only. The HTTP methods that have "Try it out" enabled. `null` or an empty array disables it for every operation |
| `UiConfigs.CorsPolicyCss` | empty | Swagger UI only. Key: the CORS policy name, value: a hex color (`#RGB` or `#RRGGBB`) |

### Endpoint attributes (`Bacon.ApiUtilities.Attributes.Apis`)

| Attribute | Target | Purpose |
|---|---|---|
| `EndpointDefinition(name)` | Class | Assigns the controller to one of the `OpenApiDocumentInfos` documents |
| `EndpointTag(tag)` | Class | Groups the controller's endpoints under a tag in the UI (instead of the default controller-name tag) |
| `HiddenApi` | Class / Method | Hides the endpoint unless `DisplayHiddenEndpoints` is `true` |
| `ApiResponse(type, statusCode, ...)` | Method | Thin wrapper over `ProducesResponseTypeAttribute` |
| `ApiInternalErrorCodes(params long[])` | Method | Documents the internal error codes an endpoint may return |
| `EndpointRateLimit { MilliSeconds = ... }` | Class / Method | Per-user/IP rate limiting; returns HTTP 429 when exceeded. Requires `app.UseRateLimiter()` (see [Request rate limiting](#request-ratelimiting)) |

```csharp
[ApiController]
[Route("v{version:apiVersion}/weather")]
[EndpointTag("Weather")]
[EndpointDefinition("sample1")]
[ApiVersion(1)]
public class WeatherForecastController : ControllerBase
{
    [HttpGet("{city}")]
    [ApiResponse(typeof(WeatherForecast))]
    [EndpointRateLimit(MilliSeconds = 20)]
    [ApiInternalErrorCodes(404001)]
    public async Task<IActionResult> GetWeatherByCityAsync([FromRoute, RequiredField] string city)
    {
        // ...
    }
}
```

### OpenAPI extensions

Each documented operation carries two vendor extensions. The Swagger UI script reads them to show a privacy stamp next to the endpoint.

| Extension | Content |
|---|---|
| `x-access-rights` | `privileges`: `Anonymous` for an `[AllowAnonymous]` endpoint, otherwise the privileges exposed through `IEndpointAccessPrivileges`, comma separated. `privacy`: the name of the policy of `[EnableCors]`, otherwise `Public`, or `Private` for a `[HiddenApi]` endpoint |
| `x-ratelimit` | `time-in-milliseconds`: the `MilliSeconds` of `[EndpointRateLimit]`. Only present on endpoints that have a rate limit |

## Request ratelimiting

`[EndpointRateLimit(MilliSeconds = n)]` limits an endpoint to one call every `n` milliseconds per caller. It is built on ASP.NET Core's `Microsoft.AspNetCore.RateLimiting`, so it needs the middleware described in [Required middleware and ordering](#required-middleware-and-ordering): `UseRouting()`, authentication, then `UseRateLimiter()`.

- **Caller identity**: the `sub` claim, then the `NameIdentifier` claim, then `client_id`, then the remote IP address (`anonymous` if there is none). Each caller gets a separate limit per action.
- **Rejection**: the rate limiter writes the HTTP 429 response itself, with `RateLimitInternalErrorCode`, as the usual localized `ErrorResult`. It does not depend on `UseApiExceptionHandler()`, and it works wherever that middleware is placed, or if it is not used. The rejection is logged as a warning under the category `Bacon.ApiUtilities.RateLimiting`. The message template receives `MilliSeconds` as `{0}`:

  ```xml
  <data name="429000" xml:space="preserve"><value>You may only perform this action every {0} milliseconds.</value></data>
  ```

- **Configuration**: set `MilliSeconds` to the delay between two calls. `0`, or leaving it unset (the default), means the endpoint is **not rate limited**: no limiter is created and no error is raised. The property is an unsigned integer (`uint`), so a negative value does not compile.
- **Timing**: the limit is a token bucket of one token that refills every `MilliSeconds`. The refill is timer based, so for very small values the delay between two accepted calls is approximate.
- **Behind a reverse proxy**: configure `UseForwardedHeaders()` before the rate limiter. Otherwise all anonymous callers share the proxy's IP address, and therefore one limit.
- **Custom rate limiting**: the library sets `RateLimiterOptions.GlobalLimiter` and `RateLimiterOptions.OnRejected`. If your application configures its own, check that they do not overwrite each other.

## Endpoint authorization pattern

`IEndpointAccessPrivileges<TPrivilege>` is a small contract for building your own privilege-based authorization attribute. `Bacon.ApiUtilities.Samples/Attributes/Authorization/BaseCustomAuthorizationAttribute.cs` shows a complete `IAsyncAuthorizationFilter` implementation that reads claims, validates the token's grant type, and checks the caller's privileges against `[BaseCustomAuthorization(Privileges.get_weather)]`-style attributes on an endpoint.

## Pagination

`ListReturnData<TItem>` is a small wrapper for paginated results:

```csharp
public IActionResult GetItems() => Ok(new ListReturnData<Item>
{
    NbFilteredItems = totalCount,
    Items = pagedItems,
}); // => { "nbFilteredItems": ..., "items": [...] }
```

## Sample walkthrough

`Bacon.ApiUtilities.Samples` is a runnable Web API demonstrating the library end-to-end:

- **`Program.cs`** — full startup wiring: CORS policies, HSTS/HTTPS redirection, security headers, request localization, API versioning (`Asp.Versioning.Mvc`), `AddApiExceptionHandler` + `AddOpenApiDocumentation`, and `UseOpenApiDocumentation` with Swagger UI.
- **`Controllers/WeatherForecastController.cs`** — versioned, tagged, rate limited endpoints using `RequiredField`, `ApiResponse`, `ApiInternalErrorCodes`, `HiddenApi`, and `ThrowCustomExceptionIfFalse`/`ThrowCustomExceptionIfTrue`.
- **`Controllers/AnotherController.cs`** — a version-neutral controller in the same OpenAPI document, to show multiple controllers/tags sharing a document.
- **`ProjectConfigs/ApiExceptionHandlerSettings.cs`** — maps every built-in and custom validation attribute to its internal error code.
- **`ProjectConfigs/ApiVersioningErrorResponseProvider.cs`** — an `IProblemDetailsWriter` that returns the 400, 404, 405 and 415 errors that the framework produces without an exception (unknown route, wrong method, unsupported API version or media type) in the `ErrorResult` format.
- **`Attributes/Validations/AllowedCaptionAttribute.cs`** and **`Attributes/Validations/AnotherValidationAttribute.cs`** — custom, multi-error-code validation attributes.
- **`Attributes/Authorization/BaseCustomAuthorizationAttribute.cs`** — custom privilege-based authorization attribute built on `IEndpointAccessPrivileges<Privileges>`.
- **`Resources/ErrorCodeResources.en-US.resx`** — the localized message catalog keyed by internal error code.

Run it with `dotnet run --project Bacon.ApiUtilities.Samples` and open the root URL to browse the generated OpenAPI documents in Swagger UI.
