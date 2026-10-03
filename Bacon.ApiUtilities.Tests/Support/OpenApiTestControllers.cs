using Bacon.ApiUtilities.Attributes.Apis;
using Bacon.ApiUtilities.Models.Apis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bacon.ApiUtilities.Tests.Support;

public sealed class WidgetDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}

public sealed class OtherDocumentDto
{
    public int Value { get; set; }
}

public sealed class HiddenOnlyDto
{
    public int Secret { get; set; }
}

public sealed class GenericItemADto
{
    public string Name { get; set; } = string.Empty;
}

public sealed class GenericItemBDto
{
    public int Count { get; set; }
}

public static class OpenApiTestControllers
{
    public const string DocumentName = "testdoc";
    public const string OtherDocumentName = "otherdoc";
    public const string GenericDocumentName = "genericdoc";
    public const string RateLimitDocumentName = "ratelimitdoc";
    public const string TagName = "Widgets";
    public const string HiddenOnlyTagName = "HiddenOnly";
    public const int RateLimitMilliSeconds = 5000;
    public const long CustomErrorCode = 409001;
}

/// <summary>
/// Belongs to the <see cref="OpenApiTestControllers.DocumentName"/> document
/// </summary>
[ApiController]
[Route("openapitest")]
[EndpointDefinition(OpenApiTestControllers.DocumentName)]
[EndpointTag(OpenApiTestControllers.TagName)]
public sealed class OpenApiTestController : ControllerBase
{
    [HttpGet("plain")]
    public ActionResult<WidgetDto> Plain()
    {
        return Ok(new WidgetDto());
    }

    [HttpGet("anonymous")]
    [AllowAnonymous]
    public IActionResult Anonymous()
    {
        return Ok();
    }

    [HttpGet("limited")]
    [EndpointRateLimit(MilliSeconds = OpenApiTestControllers.RateLimitMilliSeconds)]
    [ApiInternalErrorCodes(OpenApiTestControllers.CustomErrorCode)]
    public IActionResult Limited()
    {
        return Ok();
    }

    [HttpGet("hidden")]
    [HiddenApi]
    public IActionResult Hidden()
    {
        return Ok();
    }

    [HttpGet("deprecated")]
    [Obsolete("Test only")]
    public IActionResult Deprecated()
    {
        return Ok();
    }
}

/// <summary>
/// A visible and a hidden endpoint sharing the same route. When hidden endpoints are not displayed,
/// the path must stay with the visible operation only.
/// </summary>
[ApiController]
[Route("openapitest-shared")]
[EndpointDefinition(OpenApiTestControllers.DocumentName)]
[EndpointTag(OpenApiTestControllers.TagName)]
public sealed class OpenApiSharedRouteTestController : ControllerBase
{
    [HttpGet]
    public IActionResult VisibleGet()
    {
        return Ok();
    }

    [HttpPost]
    [HiddenApi]
    public IActionResult HiddenPost()
    {
        return Ok();
    }
}

/// <summary>
/// Only has hidden endpoints, with a tag and a schema that no other endpoint of the document uses.
/// When hidden endpoints are not displayed, neither the tag nor the schema may remain in the document.
/// </summary>
[ApiController]
[Route("openapitest-hiddenonly")]
[EndpointDefinition(OpenApiTestControllers.DocumentName)]
[EndpointTag(OpenApiTestControllers.HiddenOnlyTagName)]
public sealed class OpenApiHiddenOnlyTestController : ControllerBase
{
    [HttpGet("secret")]
    [HiddenApi]
    public ActionResult<HiddenOnlyDto> Secret()
    {
        return Ok(new HiddenOnlyDto());
    }
}

/// <summary>
/// Returns the same generic type closed over two different types, in a document of its own so the schema assertions of the other documents are not affected.
/// Both closed types must get their own schema, under the name the operations reference
/// </summary>
[ApiController]
[Route("openapitest-generic")]
[EndpointDefinition(OpenApiTestControllers.GenericDocumentName)]
public sealed class OpenApiGenericTestController : ControllerBase
{
    [HttpGet("a")]
    public ActionResult<ListReturnData<GenericItemADto>> ItemsA()
    {
        return Ok(new ListReturnData<GenericItemADto>());
    }

    [HttpGet("b")]
    public ActionResult<ListReturnData<GenericItemBDto>> ItemsB()
    {
        return Ok(new ListReturnData<GenericItemBDto>());
    }
}

/// <summary>
/// In a document of its own so the assertions of the other documents are not affected.
/// Only the endpoint with a MilliSeconds above 0 is really rate limited, so only that one may document the rate limit response and extension
/// </summary>
[ApiController]
[Route("openapitest-ratelimit")]
[EndpointDefinition(OpenApiTestControllers.RateLimitDocumentName)]
public sealed class OpenApiRateLimitDocumentationTestController : ControllerBase
{
    [HttpGet("zero")]
    [EndpointRateLimit(MilliSeconds = 0)]
    public IActionResult Zero()
    {
        return Ok();
    }

    [HttpGet("unset")]
    [EndpointRateLimit]
    public IActionResult Unset()
    {
        return Ok();
    }

    [HttpGet("limited")]
    [EndpointRateLimit(MilliSeconds = OpenApiTestControllers.RateLimitMilliSeconds)]
    public IActionResult Limited()
    {
        return Ok();
    }
}

/// <summary>
/// Belongs to another document, so it must never appear in the <see cref="OpenApiTestControllers.DocumentName"/> document
/// </summary>
[ApiController]
[Route("openapitest-other")]
[EndpointDefinition(OpenApiTestControllers.OtherDocumentName)]
public sealed class OpenApiOtherDocumentTestController : ControllerBase
{
    [HttpGet("item")]
    public ActionResult<OtherDocumentDto> Item()
    {
        return Ok(new OtherDocumentDto());
    }
}
