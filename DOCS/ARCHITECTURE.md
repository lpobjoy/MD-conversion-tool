# MD Converter - Architecture Overview

## System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                         Web Browser                                  │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │                    Blazor WebAssembly App                      │  │
│  │  ┌─────────────────────────────────────────────────────────┐  │  │
│  │  │                  User Interface Layer                    │  │  │
│  │  │  ┌──────────────────────────────────────────────────┐   │  │  │
│  │  │  │            Home.razor (Main Page)                 │   │  │  │
│  │  │  │  - File upload                                    │   │  │  │
│  │  │  │  - Text input area                                │   │  │  │
│  │  │  │  - Format selection (DOCX/PDF/HTML)               │   │  │  │
│  │  │  │  - Conversion button                              │   │  │  │
│  │  │  │  - Status display                                 │   │  │  │
│  │  │  │  - Preview pane                                   │   │  │  │
│  │  │  └──────────────────────────────────────────────────┘   │  │  │
│  │  └─────────────────────────────────────────────────────────┘  │  │
│  │                                                                 │  │
│  │  ┌─────────────────────────────────────────────────────────┐  │  │
│  │  │                  Service Layer                          │  │  │
│  │  │  ┌──────────────┐  ┌──────────────┐  ┌─────────────┐  │  │  │
│  │  │  │  Markdown    │  │   Mermaid    │  │    DOCX     │  │  │  │
│  │  │  │   Parser     │  │   Service    │  │  Converter  │  │  │  │
│  │  │  │              │  │              │  │             │  │  │  │
│  │  │  │  - Parse MD  │  │  - JS Interop│  │  - OpenXML  │  │  │  │
│  │  │  │  - Extract   │  │  - Render    │  │  - Tables   │  │  │  │
│  │  │  │    diagrams  │  │    SVG       │  │  - Styles   │  │  │  │
│  │  │  └──────────────┘  └──────────────┘  └─────────────┘  │  │  │
│  │  │  ┌──────────────┐  ┌──────────────┐                   │  │  │
│  │  │  │     PDF      │  │     HTML     │                   │  │  │
│  │  │  │  Converter   │  │  Generator   │                   │  │  │
│  │  │  │              │  │              │                   │  │  │
│  │  │  │  - QuestPDF  │  │  - Direct    │                   │  │  │
│  │  │  │  - Layout    │  │    output    │                   │  │  │
│  │  │  │  - Styling   │  │  - CSS       │                   │  │  │
│  │  │  └──────────────┘  └──────────────┘                   │  │  │
│  │  └─────────────────────────────────────────────────────────┘  │  │
│  │                                                                 │  │
│  │  ┌─────────────────────────────────────────────────────────┐  │  │
│  │  │                 External Libraries                      │  │  │
│  │  │  ┌──────────┐  ┌──────────┐  ┌────────────────────┐   │  │  │
│  │  │  │ Markdig  │  │ QuestPDF │  │ DocumentFormat.    │   │  │  │
│  │  │  │          │  │          │  │    OpenXml         │   │  │  │
│  │  │  └──────────┘  └──────────┘  └────────────────────┘   │  │  │
│  │  │  ┌──────────┐  ┌──────────┐                           │  │  │
│  │  │  │  HTML    │  │  Image   │                           │  │  │
│  │  │  │ Agility  │  │  Sharp   │                           │  │  │
│  │  │  │  Pack    │  │          │                           │  │  │
│  │  │  └──────────┘  └──────────┘                           │  │  │
│  │  └─────────────────────────────────────────────────────────┘  │  │
│  └───────────────────────────────────────────────────────────────┘  │
│                                                                      │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │                  JavaScript Layer                             │  │
│  │  ┌──────────────────────────────────────────────────────────┐ │  │
│  │  │  mermaid-interop.js                                      │ │  │
│  │  │  - initMermaid()                                         │ │  │
│  │  │  - renderMermaid(code, elementId)                        │ │  │
│  │  │  - downloadFile(filename, mimeType, base64Data)          │ │  │
│  │  └──────────────────────────────────────────────────────────┘ │  │
│  │  ┌──────────────────────────────────────────────────────────┐ │  │
│  │  │  Mermaid.js (CDN)                                        │ │  │
│  │  │  - Diagram rendering engine                              │ │  │
│  │  └──────────────────────────────────────────────────────────┘ │  │
│  └───────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
```

## Data Flow Diagram

```
┌──────────────┐
│  User Input  │
│  - File      │
│  - Text      │
└──────┬───────┘
       │
       ▼
┌──────────────────────┐
│  MarkdownParser      │
│  ParseMarkdown()     │
│  1. Parse syntax     │
│  2. Extract Mermaid  │
│  3. Create HTML      │
└──────┬───────────────┘
       │
       ▼
┌──────────────────────┐
│  MermaidService      │
│  RenderAllDiagrams() │
│  1. Loop diagrams    │
│  2. JS Interop       │
│  3. Get SVG          │
└──────┬───────────────┘
       │
       ▼
┌──────────────────────┐
│  MarkdownParser      │
│  ReplacePlaceholders │
│  Embed SVG in HTML   │
└──────┬───────────────┘
       │
       ▼
┌──────────────────────┐     ┌──────────────────────┐     ┌──────────────────────┐
│  DocxConverter       │     │  PdfConverter        │     │  HTML Generator      │
│  ConvertToDocx()     │     │  ConvertToPdf()      │     │  ConvertToHtml()     │
│                      │     │                      │     │                      │
│  1. Parse HTML       │     │  1. Parse HTML       │     │  1. Wrap HTML        │
│  2. Create Word doc  │     │  2. Create PDF doc   │     │  2. Add styling      │
│  3. Add content      │     │  3. Add content      │     │  3. Embed SVGs       │
│  4. Return bytes     │     │  4. Return bytes     │     │  4. Return bytes     │
└──────┬───────────────┘     └──────┬───────────────┘     └──────┬───────────────┘
       │                            │                            │
       └────────────────────────────┴────────────────────────────┘
                                    │
                                    ▼
                          ┌──────────────────────┐
                          │  ConversionResult    │
                          │  - Success           │
                          │  - FileData (bytes)  │
                          │  - FileName          │
                          │  - MimeType          │
                          └──────┬───────────────┘
                                 │
                                 ▼
                          ┌──────────────────────┐
                          │  JavaScript          │
                          │  downloadFile()      │
                          │  Trigger download    │
                          └──────┬───────────────┘
                                 │
                                 ▼
                          ┌──────────────────────┐
                          │  User Downloads      │
                          │  Document            │
                          └──────────────────────┘
```

## Component Interaction Diagram

```
Home.razor
    │
    ├──> @inject MarkdownParser
    │       │
    │       └──> ParseMarkdown(markdown)
    │               │
    │               ├──> Markdig.ToHtml()
    │               └──> ExtractMermaidDiagrams()
    │
    ├──> @inject MermaidService
    │       │
    │       └──> RenderAllDiagrams(diagrams)
    │               │
    │               └──> IJSRuntime.InvokeAsync("renderMermaid")
    │                       │
    │                       └──> JavaScript: mermaid.render()
    │
    ├──> @inject DocxConverter
    │       │
    │       └──> ConvertToDocx(html, diagrams)
    │               │
    │               ├──> WordprocessingDocument.Create()
    │               ├──> HtmlDocument.LoadHtml()
    │               └──> ProcessHtmlNode()
    │
    ├──> @inject PdfConverter
    │       │
    │       └──> ConvertToPdf(html, diagrams)
    │               │
    │               ├──> QuestPDF.Document.Create()
    │               ├──> HtmlDocument.LoadHtml()
    │               └──> ProcessHtmlForPdf()
    │
    └──> ConvertToHtml(html)
            │
            └──> Wrap in HTML template with CSS
```

## File Organization

```
MDConverter.csproj
├── Program.cs                      [Entry point, DI registration]
├── App.razor                       [Root component]
├── _Imports.razor                  [Global using statements]
│
├── Models/
│   └── ConversionResult.cs         [DTOs, enums]
│
├── Services/
│   ├── MarkdownParser.cs           [Markdown → HTML + diagrams]
│   ├── MermaidService.cs           [JS Interop for Mermaid]
│   ├── DocxConverter.cs            [HTML → DOCX]
│   └── PdfConverter.cs             [HTML → PDF]
│
├── Pages/
│   ├── Home.razor                  [Main conversion UI]
│   ├── Counter.razor               [Default sample]
│   └── Weather.razor               [Default sample]
│
├── Layout/
│   ├── MainLayout.razor            [App layout]
│   └── NavMenu.razor               [Navigation]
│
└── wwwroot/
    ├── index.html                  [HTML shell]
    ├── js/
    │   └── mermaid-interop.js     [Mermaid bridge]
    └── css/
        └── app.css                 [Custom styles]
```

## Technology Dependencies

```
.NET 9 SDK
    │
    ├──> Blazor WebAssembly
    │       │
    │       ├──> Dependency Injection
    │       ├──> JS Interop
    │       └──> Component Model
    │
    └──> NuGet Packages
            │
            ├──> Markdig 0.42.0
            │       └──> Advanced markdown parsing
            │
            ├──> QuestPDF 2025.7.3
            │       └──> PDF generation
            │
            ├──> DocumentFormat.OpenXml 3.3.0
            │       └──> DOCX generation
            │
            ├──> HtmlAgilityPack 1.12.4
            │       └──> HTML parsing
            │
            └──> SixLabors.ImageSharp 3.1.11
                    └──> Image processing

Browser
    │
    └──> CDN Dependencies
            │
            ├──> Mermaid.js 11.x
            │       └──> Diagram rendering
            │
            └──> Bootstrap Icons 1.11.x
                    └──> UI icons
```

## Conversion Pipeline Details

### Markdown → DOCX

```
Markdown String
    │
    ▼
MarkdownParser.ParseMarkdown()
    │
    ├──> Extract Mermaid: ```mermaid ... ```
    ├──> Replace with placeholders: {{MERMAID_PLACEHOLDER_ID}}
    └──> Convert to HTML with Markdig
    │
    ▼
MermaidService.RenderAllDiagrams()
    │
    └──> For each diagram:
         ├──> Call JavaScript: mermaid.render(code)
         └──> Store SVG result
    │
    ▼
MarkdownParser.ReplacePlaceholders()
    │
    └──> Replace {{MERMAID_PLACEHOLDER_ID}} with <img> tags
    │
    ▼
DocxConverter.ConvertToDocx()
    │
    ├──> Load HTML with HtmlAgilityPack
    ├──> Create WordprocessingDocument
    ├──> Process HTML nodes:
    │    ├──> <h1-h6> → Heading styles
    │    ├──> <p> → Paragraph
    │    ├──> <table> → Table
    │    ├──> <ul/ol> → Lists
    │    ├──> <code> → Code block
    │    └──> <img> → Placeholder text
    └──> Return DOCX bytes
    │
    ▼
Download DOCX file
```

### Markdown → PDF

```
Markdown String
    │
    [Same parsing steps as DOCX]
    │
    ▼
PdfConverter.ConvertToPdf()
    │
    ├──> Load HTML with HtmlAgilityPack
    ├──> Create QuestPDF Document
    ├──> Process HTML nodes:
    │    ├──> <h1-h6> → Text with size/bold
    │    ├──> <p> → Text
    │    ├──> <table> → QuestPDF Table
    │    ├──> <ul/ol> → Formatted lists
    │    ├──> <code> → Code block style
    │    └──> <img> → Placeholder text
    └──> Return PDF bytes
    │
    ▼
Download PDF file
```

### Markdown → HTML

```
Markdown String
    │
    [Same parsing steps as DOCX]
    │
    ▼
ConvertToHtml()
    │
    ├──> Wrap HTML in <!DOCTYPE html>
    ├──> Add CSS styling
    ├──> Embed SVG diagrams directly
    └──> Return HTML bytes
    │
    ▼
Download HTML file (or preview)
```

## State Management

```
Home.razor Component State:
├── markdownContent: string         [User input]
├── selectedFormat: ExportFormat    [DOCX/PDF/HTML]
├── fileName: string                [Output filename]
├── isConverting: bool              [Conversion in progress]
├── statusMessage: string           [User feedback]
├── statusSuccess: bool             [Success/error indicator]
├── showPreview: bool               [Show HTML preview]
└── previewHtml: string             [Preview content]
```

## Error Handling Flow

```
User Action
    │
    ▼
Try {
    Parse Markdown
    │
    ├──> Success → Extract diagrams
    │              │
    │              ▼
    │         Render diagrams
    │              │
    │              ├──> Success → Convert format
    │              │              │
    │              │              ├──> Success → Download
    │              │              │              │
    │              │              │              └──> Show success message
    │              │              │
    │              │              └──> Error → Show error message
    │              │
    │              └──> Error → Show warning, continue with placeholder
    │
    └──> Error → Show error message
}
Catch (Exception e) {
    Display error to user
    Log to console
    Reset state
}
```

---

*This architecture supports the core goal: Convert MD to documents with Mermaid diagrams, all in the browser.*
