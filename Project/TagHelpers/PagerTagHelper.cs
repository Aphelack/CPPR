using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;

namespace Project.TagHelpers;

[HtmlTargetElement("pager")]
public class PagerTagHelper : TagHelper
{
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PagerTagHelper(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
    {
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
    }

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string? Category { get; set; }
    public bool Admin { get; set; } = false;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "nav";
        output.Attributes.SetAttribute("aria-label", "Page navigation");

        var ul = new TagBuilder("ul");
        ul.AddCssClass("pagination");
        ul.AddCssClass("justify-content-center");

        // Previous button
        ul.InnerHtml.AppendHtml(CreatePageItem("Previous", CurrentPage - 1, CurrentPage == 1));

        // Page numbers
        for (int i = 1; i <= TotalPages; i++)
        {
            ul.InnerHtml.AppendHtml(CreatePageItem(i.ToString(), i, false, i == CurrentPage));
        }

        // Next button
        ul.InnerHtml.AppendHtml(CreatePageItem("Next", CurrentPage + 1, CurrentPage == TotalPages));

        output.Content.SetHtmlContent(ul);
    }

    private TagBuilder CreatePageItem(string text, int pageNo, bool disabled = false, bool active = false)
    {
        var li = new TagBuilder("li");
        li.AddCssClass("page-item");
        
        if (disabled)
            li.AddCssClass("disabled");
        if (active)
            li.AddCssClass("active");

        var a = new TagBuilder("a");
        a.AddCssClass("page-link");
        
        if (!disabled)
        {
            string? url;
            if (Admin)
            {
                // For admin Razor pages - use the custom route
                url = $"/admin/product?pageNo={pageNo}";
            }
            else
            {
                // For regular controller actions
                url = _linkGenerator.GetPathByAction(
                    _httpContextAccessor.HttpContext!,
                    action: "Index",
                    controller: "Product",
                    values: new { category = Category, pageNo = pageNo }
                ) ?? "#";
            }
            a.Attributes.Add("href", url);
        }
        else
        {
            a.Attributes.Add("href", "#");
        }

        a.InnerHtml.Append(text);
        li.InnerHtml.AppendHtml(a);

        return li;
    }
}
