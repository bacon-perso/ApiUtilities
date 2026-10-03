using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Bacon.ApiUtilities.Tests.Support;

[ApiController]
[Route("nested-validation")]
public sealed class NestedValidationTestController : ControllerBase
{
    [HttpPost("body")]
    public IActionResult Body([FromBody] NestedRoot model)
    {
        return Ok();
    }

    [HttpPost("mixed/{id}")]
    public IActionResult Mixed([FromRoute][PathMarker] string id, [FromQuery][QueryMarker] string filter, [FromBody] NestedRoot model)
    {
        return Ok();
    }

    [HttpPost("form")]
    public IActionResult Form([FromForm] FormRoot model)
    {
        return Ok();
    }

    [HttpPost("shared")]
    public ActionResult<NestedRoot> Shared([FromBody] NestedMiddle model)
    {
        return Ok(new NestedRoot());
    }

    [HttpPost("body-list")]
    public IActionResult BodyList([FromBody] List<NestedRoot> model)
    {
        return Ok();
    }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class RootPropertyMarkerAttribute : ValidationAttribute;

[AttributeUsage(AttributeTargets.Property)]
public sealed class MiddlePropertyMarkerAttribute : ValidationAttribute;

[AttributeUsage(AttributeTargets.Property)]
public sealed class LeafPropertyMarkerAttribute : ValidationAttribute;

[AttributeUsage(AttributeTargets.Class)]
public sealed class RootClassMarkerAttribute : ValidationAttribute;

[AttributeUsage(AttributeTargets.Class)]
public sealed class MiddleClassMarkerAttribute : ValidationAttribute;

[AttributeUsage(AttributeTargets.Class)]
public sealed class LeafClassMarkerAttribute : ValidationAttribute;

[RootClassMarker]
public sealed class NestedRoot
{
    [RootPropertyMarker]
    public string? Name { get; set; }

    public List<NestedMiddle> Middles { get; set; } = [];
}

[MiddleClassMarker]
public sealed class NestedMiddle
{
    [MiddlePropertyMarker]
    public string? Title { get; set; }

    public List<NestedLeaf> Leaves { get; set; } = [];
}

[LeafClassMarker]
public sealed class NestedLeaf
{
    [LeafPropertyMarker]
    public string? Value { get; set; }
}

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class PathMarkerAttribute : ValidationAttribute;

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class QueryMarkerAttribute : ValidationAttribute;

[AttributeUsage(AttributeTargets.Property)]
public sealed class FormPropertyMarkerAttribute : ValidationAttribute;

[AttributeUsage(AttributeTargets.Property)]
public sealed class FormItemPropertyMarkerAttribute : ValidationAttribute;

public sealed class FormRoot
{
    [FormPropertyMarker]
    public string? Name { get; set; }

    public List<FormItem> Items { get; set; } = [];
}

public sealed class FormItem
{
    [FormItemPropertyMarker]
    public string? Label { get; set; }
}

[ApiController]
[Route("tagged-validation")]
[Bacon.ApiUtilities.Attributes.Apis.EndpointTag("CustomTag")]
public sealed class TaggedTestController : ControllerBase
{
    [HttpPost]
    public IActionResult Tagged()
    {
        return Ok();
    }
}
