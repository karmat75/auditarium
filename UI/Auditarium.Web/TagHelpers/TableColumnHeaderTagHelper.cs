using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Auditarium.Web.TagHelpers;

[HtmlTargetElement("th")]
public sealed class TableColumnHeaderTagHelper : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (!output.Attributes.ContainsName("scope"))
        {
            output.Attributes.SetAttribute("scope", "col");
        }
    }
}
