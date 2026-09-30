using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace ContentHub.Infrastructure.Rendering;

public sealed class CopyCodeBlockExtension : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is HtmlRenderer htmlRenderer)
        {
            var original = htmlRenderer.ObjectRenderers.FindExact<CodeBlockRenderer>();
            if (original is not null)
            {
                htmlRenderer.ObjectRenderers.Remove(original);
            }

            htmlRenderer.ObjectRenderers.AddIfNotAlready(new CopyCodeBlockRenderer(original));
        }
    }
}

public sealed class CopyCodeBlockRenderer : CodeBlockRenderer
{
    private readonly CodeBlockRenderer? _original;

    public CopyCodeBlockRenderer(CodeBlockRenderer? original)
    {
        _original = original;
    }

    protected override void Write(HtmlRenderer renderer, CodeBlock obj)
    {
        var language = obj is FencedCodeBlock fencedCode && !string.IsNullOrWhiteSpace(fencedCode.Info)
            ? fencedCode.Info.Trim()
            : string.Empty;

        renderer.Write("<div class=\"code-block-wrapper\" data-language=\"");
        renderer.Write(language.Replace("\"", "&quot;"));
        renderer.Write("\">");

        renderer.Write("<div class=\"code-block-header\">");
        if (!string.IsNullOrWhiteSpace(language))
        {
            renderer.Write("<span class=\"code-language-label\">");
            renderer.Write(language);
            renderer.Write("</span>");
        }

        renderer.Write("<button type=\"button\" class=\"copy-code-btn\" aria-label=\"Copy code\">");
        renderer.Write("<svg viewBox=\"0 0 24 24\" aria-hidden=\"true\" focusable=\"false\"><path d=\"M14 3a2 2 0 0 1 2 2v1h1a2 2 0 0 1 2 2v10a2 2 0 0 1-2 2H9a2 2 0 0 1-2-2v-1H6a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h8Zm0 2H6v10h2V8a2 2 0 0 1 2-2h4V5Zm-4 4h7v10H9V9Z\" fill=\"currentColor\"/></svg>");
        renderer.Write("</button>");
        renderer.Write("</div>");

        _original?.Write(renderer, obj);

        renderer.Write("</div>");
    }
}
