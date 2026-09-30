# Migration Task: Replace Quill Editor with Markdown-Based Content Editing (+ Copy-Code Feature)

## Goal
Remove the Quill WYSIWYG editor from the Angular frontend and replace it with a
Markdown-based authoring flow. Content is now stored as raw Markdown text.
Rendering to HTML happens server-side (.NET) using Markdig, with a custom
extension that wraps fenced code blocks in a "copy code" UI. This must work
for multiple users creating content, so sanitize all rendered HTML before it
is stored or served.

Work through the sections in order. Each section lists concrete file/package
changes. Ask for clarification only if an existing file/path referenced below
doesn't exist in the repo — otherwise infer the closest equivalent and proceed.

---

## 1. Backend (.NET) — Markdown rendering pipeline

### 1.1 Add packages
```
dotnet add package Markdig
dotnet add package Ganss.Xss   # HtmlSanitizer — required since untrusted users can now submit content
```

### 1.2 Create a custom Markdig extension for copy-code blocks
Create `Rendering/CopyCodeBlockExtension.cs`:

```csharp
using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

public class CopyCodeBlockExtension : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline) { }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is HtmlRenderer htmlRenderer)
        {
            var original = htmlRenderer.ObjectRenderers.FindExact<CodeBlockRenderer>();
            if (original != null)
            {
                htmlRenderer.ObjectRenderers.Remove(original);
            }
            htmlRenderer.ObjectRenderers.AddIfNotAlready(new CopyCodeBlockRenderer(original));
        }
    }
}

public class CopyCodeBlockRenderer : Markdig.Renderers.Html.CodeBlockRenderer
{
    private readonly CodeBlockRenderer _original;

    public CopyCodeBlockRenderer(CodeBlockRenderer original)
    {
        _original = original;
    }

    protected override void Write(HtmlRenderer renderer, CodeBlock obj)
    {
        renderer.Write("<div class=\"code-block-wrapper\">");
        renderer.Write("<button type=\"button\" class=\"copy-code-btn\" aria-label=\"Copy code\">Copy</button>");
        _original.Write(renderer, obj); // delegates to Markdig's normal <pre><code> rendering (keeps language class for syntax highlighting)
        renderer.Write("</div>");
    }
}
```

> Note: Markdig's `FencedCodeBlockRenderer` inherits from `CodeBlockRenderer`, so
> this override also catches fenced code blocks (` ```csharp `). Verify against
> the installed Markdig version — if `CodeBlockRenderer.Write` isn't accessible
> this way, fall back to a post-render regex/HTML-parse pass in
> `MarkdownRenderService` (section 1.3) that wraps `<pre><code>` blocks instead.

### 1.3 Create `MarkdownRenderService`
Create `Services/MarkdownRenderService.cs`:

```csharp
using Markdig;
using Ganss.Xss;

public class MarkdownRenderService
{
    private readonly MarkdownPipeline _pipeline;
    private readonly HtmlSanitizer _sanitizer;

    public MarkdownRenderService()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()   // tables, footnotes, task lists, etc.
            .Use<CopyCodeBlockExtension>()
            .DisableHtml()            // IMPORTANT: strips raw inline HTML from user markdown input
            .Build();

        _sanitizer = new HtmlSanitizer();
        _sanitizer.AllowedTags.Add("button");
        _sanitizer.AllowedAttributes.Add("class");
        _sanitizer.AllowedAttributes.Add("aria-label");
        _sanitizer.AllowedAttributes.Add("type");
    }

    public string ToSafeHtml(string markdown)
    {
        var html = Markdown.ToHtml(markdown ?? string.Empty, _pipeline);
        return _sanitizer.Sanitize(html);
    }
}
```

Register in `Program.cs`:
```csharp
builder.Services.AddSingleton<MarkdownRenderService>();
```

**Why sanitize even with `DisableHtml()`:** defense in depth. Multiple users
writing content means this is no longer trusted-author-only content — always
sanitize the final HTML server-side before storing/serving it, regardless of
what the markdown pipeline already strips.

### 1.4 Data model / DB changes
- Rename or add a column: `ContentMarkdown` (nvarchar(max)) — the raw source of truth.
- Add `ContentHtml` (nvarchar(max)) — the rendered, sanitized cache, regenerated on every save.
- Keep old `Content` column temporarily if you need a rollback path; otherwise drop it in a follow-up migration once verified.

EF Core migration:
```
dotnet ef migrations add ReplaceQuillContentWithMarkdown
dotnet ef database update
```

If existing rows have Quill-generated HTML in the old `Content` column, write
a one-time data migration/backfill step that either:
  - leaves old posts rendering from their legacy HTML column (add an `IsLegacyHtml` flag), or
  - converts old HTML to Markdown using a tool like `ReverseMarkdown` NuGet package, storing the result in `ContentMarkdown`.
Confirm with me which approach before running it against production data.

### 1.5 API endpoint changes
Update the existing write endpoint (find it — likely `PostsController` or
similar) so the request DTO takes raw markdown:

```csharp
public record CreatePostRequest(string Title, string ContentMarkdown, /* existing fields */);

[HttpPost("api/posts")]
public async Task<IActionResult> Create(CreatePostRequest request)
{
    var html = _markdownRenderService.ToSafeHtml(request.ContentMarkdown);

    var post = new Post
    {
        Title = request.Title,
        ContentMarkdown = request.ContentMarkdown,
        ContentHtml = html,
        // ...existing fields (author, createdDate, etc.)
    };

    // existing save-to-DB logic stays the same
}
```

Update the read/response DTO to include both `contentMarkdown` (for editing)
and `contentHtml` (for display):
```csharp
public record PostResponse(int Id, string Title, string ContentMarkdown, string ContentHtml, /* ... */);
```

Add basic server-side validation: max length on `ContentMarkdown` (pick a
sane limit, e.g. 200,000 chars) to prevent abuse, since this is now
user-generated content from multiple accounts.

---

## 2. Frontend (Angular) — remove Quill, add Markdown editor

### 2.1 Remove Quill
```
npm uninstall ngx-quill quill
```
- Remove `QuillModule` from any Angular module imports.
- Remove `<quill-editor>` usage from templates.
- Remove any Quill-specific SCSS imports (`quill.snow.css`, etc.).
- Search the codebase for `quill` (case-insensitive) and remove remaining references, including any `[modules]="quillConfig"` bindings and toolbar config objects.

### 2.2 Add a Markdown editor component
Install a lightweight preview renderer for the client (client-side preview
only — **the backend render is always the canonical/stored version**):
```
npm install ngx-markdown marked
```

Create `PostEditorComponent` (`post-editor.component.ts`) as a split-pane editor:

```typescript
import { Component } from '@angular/core';
import { FormControl } from '@angular/forms';

@Component({
  selector: 'app-post-editor',
  templateUrl: './post-editor.component.html',
  styleUrls: ['./post-editor.component.scss']
})
export class PostEditorComponent {
  markdownControl = new FormControl('');

  insertSyntax(before: string, after: string = ''): void {
    // toolbar helper: wraps or inserts markdown syntax at cursor position
    // e.g. insertSyntax('**', '**') for bold, insertSyntax('```\n', '\n```') for code block
  }

  save(): void {
    this.postService.save({
      title: this.titleControl.value,
      contentMarkdown: this.markdownControl.value
    }).subscribe(/* existing save handling */);
  }
}
```

Template (`post-editor.component.html`):
```html
<div class="editor-toolbar">
  <button type="button" (click)="insertSyntax('**','**')">Bold</button>
  <button type="button" (click)="insertSyntax('_','_')">Italic</button>
  <button type="button" (click)="insertSyntax('[', '](url)')">Link</button>
  <button type="button" (click)="insertSyntax('\n```\n', '\n```\n')">Code block</button>
</div>

<div class="editor-split">
  <textarea [formControl]="markdownControl" placeholder="Write in Markdown..."></textarea>
  <div class="preview" markdown [data]="markdownControl.value"></div>
</div>
```

`markdown` directive comes from `ngx-markdown` (`MarkdownModule.forRoot()` in
your app module) — it's for **live preview only** while writing. Don't rely
on the client-rendered preview HTML for anything persisted; always send raw
markdown to the API and use the server's `contentHtml` for the published view.

### 2.3 Display component — render stored HTML + wire up copy-code buttons
Wherever posts are currently displayed (e.g. `PostDetailComponent`):

```typescript
import { Component, AfterViewChecked, ElementRef } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Component({ selector: 'app-post-detail', templateUrl: './post-detail.component.html' })
export class PostDetailComponent implements AfterViewChecked {
  private copyButtonsBound = new WeakSet<Element>();

  constructor(private sanitizer: DomSanitizer, private el: ElementRef) {}

  trustHtml(html: string): SafeHtml {
    return this.sanitizer.bypassSecurityTrustHtml(html); // safe: html is sanitized server-side, not raw user input at this point
  }

  ngAfterViewChecked(): void {
    const buttons = this.el.nativeElement.querySelectorAll('.copy-code-btn');
    buttons.forEach((btn: Element) => {
      if (this.copyButtonsBound.has(btn)) return;
      this.copyButtonsBound.add(btn);
      btn.addEventListener('click', () => this.copyCode(btn));
    });
  }

  private copyCode(button: Element): void {
    const wrapper = button.closest('.code-block-wrapper');
    const codeEl = wrapper?.querySelector('code');
    if (!codeEl) return;

    navigator.clipboard.writeText(codeEl.textContent ?? '').then(() => {
      const original = button.textContent;
      button.textContent = 'Copied!';
      button.classList.add('copied');
      setTimeout(() => {
        button.textContent = original;
        button.classList.remove('copied');
      }, 1500);
    });
  }
}
```

Template:
```html
<article [innerHTML]="trustHtml(post.contentHtml)"></article>
```

### 2.4 Copy-code button styling
Add to global styles (or a shared SCSS partial):

```scss
.code-block-wrapper {
  position: relative;

  pre {
    padding: 1rem;
    overflow-x: auto;
    border-radius: 6px;
  }

  .copy-code-btn {
    position: absolute;
    top: 0.5rem;
    right: 0.5rem;
    padding: 0.25rem 0.6rem;
    font-size: 0.75rem;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    opacity: 0.7;
    transition: opacity 0.15s ease;

    &:hover { opacity: 1; }
    &.copied { background-color: #2ecc71; color: white; }
  }
}
```

If you want syntax highlighting on the rendered code (not just the copy
button), add `highlight.js` or `Prism.js` and run it in `ngAfterViewChecked`
alongside the copy-button binding, targeting the same `pre code` elements.

---

## 3. Security checklist (multi-user content — do not skip)
- [ ] `DisableHtml()` set on the Markdig pipeline (backend).
- [ ] `HtmlSanitizer` sanitizes every rendered HTML string before it's stored or returned.
- [ ] Max length validation on `ContentMarkdown` server-side.
- [ ] Re-sanitize on every save (never trust a previously-sanitized value as still safe if the pipeline changes later).
- [ ] Confirm `contentHtml` is only ever set by the server — the Angular client must never send pre-rendered HTML to the save endpoint.
- [ ] Rate-limit the save endpoint if not already covered by existing API middleware.

## 4. Testing checklist
- [ ] Create a post with a fenced code block in 2+ languages; verify each renders inside `.code-block-wrapper` with a working copy button.
- [ ] Attempt to submit raw `<script>` tags via the markdown textarea; verify they're stripped from the stored/rendered HTML.
- [ ] Verify existing posts (pre-migration) still render correctly after the data migration/backfill step.
- [ ] Verify the editor's live preview matches (closely) what the server ultimately renders.
- [ ] Cross-browser check on `navigator.clipboard.writeText` (requires HTTPS or localhost — confirm your deployed environment serves over HTTPS).