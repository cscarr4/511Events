using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace _511Events.Logic
{
    public static class HtmlHelperExtensions
    {
        public static IHtmlContent ProgressBar(this IHtmlHelper htmlHelper, bool? hidden = false)
        {
            var card = new TagBuilder("div");
            var header = new TagBuilder("div");
            var body = new TagBuilder("div");
            var container = new TagBuilder("div");
            var bar = new TagBuilder("div");

            card.Attributes["id"] = TagBuilder.CreateSanitizedId("progressBarCard", htmlHelper.IdAttributeDotReplacement);
            card.AddCssClass("card");
            
            header.Attributes["id"] = TagBuilder.CreateSanitizedId("progressBarHeader", htmlHelper.IdAttributeDotReplacement);
            header.AddCssClass("card-header");

            body.Attributes["id"] = TagBuilder.CreateSanitizedId("progressBarBody", htmlHelper.IdAttributeDotReplacement);
            body.AddCssClass("card-body");

            container.Attributes["id"] = TagBuilder.CreateSanitizedId("progressBarContainer", htmlHelper.IdAttributeDotReplacement);
            container.AddCssClass("progress");

            bar.Attributes["id"] = TagBuilder.CreateSanitizedId("progressBar", htmlHelper.IdAttributeDotReplacement);
            bar.AddCssClass("progress-bar");
            bar.Attributes["role"] = "progressbar";

            container.InnerHtml.SetHtmlContent(bar);
            body.InnerHtml.SetHtmlContent(container);
            card.InnerHtml.AppendHtml(header);
            card.InnerHtml.AppendHtml(body);

            return card;
        }
    }
}
