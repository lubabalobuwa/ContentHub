using Ganss.Xss;
using Markdig;

namespace ContentHub.Infrastructure.Rendering;

public sealed class MarkdownRenderService
{
    private readonly MarkdownPipeline _pipeline;
    private readonly HtmlSanitizer _sanitizer;

    public MarkdownRenderService()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Use<CopyCodeBlockExtension>()
            .DisableHtml()
            .Build();

        _sanitizer = new HtmlSanitizer();
        _sanitizer.AllowedTags.Add("button");
        _sanitizer.AllowedTags.Add("svg");
        _sanitizer.AllowedTags.Add("path");
        _sanitizer.AllowedTags.Add("span");

        _sanitizer.AllowedAttributes.Add("class");
        _sanitizer.AllowedAttributes.Add("aria-label");
        _sanitizer.AllowedAttributes.Add("type");
        _sanitizer.AllowedAttributes.Add("viewBox");
        _sanitizer.AllowedAttributes.Add("fill");
        _sanitizer.AllowedAttributes.Add("d");
        _sanitizer.AllowedAttributes.Add("aria-hidden");
        _sanitizer.AllowedAttributes.Add("focusable");
        _sanitizer.AllowedAttributes.Add("data-language");
    }

    public string ToSafeHtml(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return string.Empty;

        var html = Markdown.ToHtml(markdown, _pipeline);
        return _sanitizer.Sanitize(html);
    }
}
