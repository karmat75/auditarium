using Auditarium.Web.Components;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;

namespace Auditarium.Web.TagHelpers;

[HtmlTargetElement("aud-status")]
public sealed class StatusTagHelper : TagHelper
{
    [HtmlAttributeName("value")]
    public object? Value { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var presentation = StatusPresentations.Resolve(Value);

        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", $"aud-status aud-status--{presentation.Tone}");
        output.Attributes.SetAttribute("aria-label", $"Status: {presentation.Label}");
        output.Content.SetHtmlContent($"<span class=\"aud-status__label\">{HtmlEncoder.Default.Encode(presentation.Label)}</span>");
    }
}
