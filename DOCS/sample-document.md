# Sample Document for Testing

## Introduction

This is a **comprehensive test document** that demonstrates all the features of the MD Converter tool. It includes various markdown elements and *Mermaid diagrams*.

## Features Overview

The converter supports:
- Headers (H1-H6)
- **Bold** and *italic* text
- `Inline code`
- Links: [GitHub](https://github.com)
- Images (when provided)

### Code Blocks

Here's a C# code example:

```csharp
public class DocumentConverter
{
    public async Task<byte[]> ConvertAsync(string markdown)
    {
        var parser = new MarkdownParser();
        var result = await parser.ParseAsync(markdown);
        return result.ToBytes();
    }
}
```

And here's some JavaScript:

```javascript
const convertDocument = async (markdown) => {
    const parser = new MarkdownParser();
    const result = await parser.parse(markdown);
    return result;
};
```

## Mermaid Diagrams

### System Architecture

This diagram shows the overall architecture of our converter:

```mermaid
graph TB
    A[User Input] --> B{File or Text?}
    B -->|File| C[File Upload]
    B -->|Text| D[Text Area]
    C --> E[Markdown Parser]
    D --> E
    E --> F[Extract Mermaid]
    F --> G[Render Diagrams]
    G --> H{Output Format}
    H -->|DOCX| I[Word Converter]
    H -->|PDF| J[PDF Converter]
    H -->|HTML| K[HTML Generator]
    I --> L[Download]
    J --> L
    K --> L
```

### Sequence Diagram

Here's how the conversion process works:

```mermaid
sequenceDiagram
    participant User
    participant Browser
    participant Parser
    participant Mermaid
    participant Converter
    
    User->>Browser: Upload/Paste Markdown
    Browser->>Parser: Parse Content
    Parser->>Parser: Extract Code Blocks
    Parser->>Mermaid: Render Diagrams
    Mermaid-->>Parser: SVG Output
    Parser->>Converter: Convert to Format
    Converter-->>Browser: Generate File
    Browser-->>User: Download File
```

### State Diagram

The document conversion lifecycle:

```mermaid
stateDiagram-v2
    [*] --> Idle
    Idle --> Parsing: User Clicks Convert
    Parsing --> Rendering: Markdown Parsed
    Rendering --> Converting: Diagrams Rendered
    Converting --> Complete: Conversion Done
    Complete --> Downloading: Generate Download
    Downloading --> Idle: Download Complete
    
    Parsing --> Error: Parse Failed
    Rendering --> Error: Render Failed
    Converting --> Error: Conversion Failed
    Error --> Idle: Reset
```

## Tables

Here's a comparison of supported formats:

| Format | File Extension | Supports Diagrams | File Size | Use Case |
|--------|----------------|-------------------|-----------|----------|
| DOCX   | .docx          | ✅ Yes            | Medium    | Professional documents |
| PDF    | .pdf           | ✅ Yes            | Small     | Sharing and printing |
| HTML   | .html          | ✅ Yes            | Small     | Web publishing |

### Feature Matrix

| Feature              | Supported | Notes                        |
|---------------------|-----------|------------------------------|
| Headers             | ✅        | H1-H6                        |
| Bold/Italic         | ✅        | Full support                 |
| Code Blocks         | ✅        | Syntax highlighting in HTML  |
| Tables              | ✅        | All formats                  |
| Lists               | ✅        | Ordered and unordered        |
| Mermaid Diagrams    | ✅        | All diagram types            |
| Images              | ✅        | Base64 embedded              |
| Links               | ✅        | All formats                  |

## Lists

### Ordered List

1. First item
2. Second item
3. Third item
   1. Sub-item 3.1
   2. Sub-item 3.2
4. Fourth item

### Unordered List

- Main point
- Another point
  - Sub-point
  - Another sub-point
- Final point

### Task List

- [x] Implement markdown parsing
- [x] Add Mermaid support
- [x] Create DOCX converter
- [x] Create PDF converter
- [x] Create HTML converter
- [ ] Add custom styling options
- [ ] Support for more formats

## Blockquotes

> "The best way to predict the future is to invent it."
> - Alan Kay

> This is a multi-line quote.
> It can span multiple lines
> and maintain formatting.

## Complex Flowchart

Let's create a more complex diagram:

```mermaid
flowchart LR
    A[Start] --> B{Check Input}
    B -->|Valid| C[Process Markdown]
    B -->|Invalid| D[Show Error]
    D --> Z[End]
    
    C --> E{Has Mermaid?}
    E -->|Yes| F[Extract Diagrams]
    E -->|No| G[Continue]
    
    F --> H[Render Each Diagram]
    H --> I{Render Success?}
    I -->|Yes| J[Embed in Content]
    I -->|No| K[Use Placeholder]
    J --> G
    K --> G
    
    G --> L{Select Format}
    L -->|DOCX| M[Generate Word Doc]
    L -->|PDF| N[Generate PDF]
    L -->|HTML| O[Generate HTML]
    
    M --> P[Package File]
    N --> P
    O --> P
    
    P --> Q[Trigger Download]
    Q --> Z
    
    style A fill:#90EE90
    style Z fill:#FFB6C1
    style D fill:#FF6B6B
    style K fill:#FFD93D
```

## Pie Chart

Distribution of supported features:

```mermaid
pie title Feature Distribution
    "Text Formatting" : 30
    "Diagrams" : 25
    "Tables" : 20
    "Code Blocks" : 15
    "Other" : 10
```

## Conclusion

This document demonstrates all the features supported by the MD Converter. The tool successfully:

1. ✅ Parses markdown content
2. ✅ Renders Mermaid diagrams
3. ✅ Converts to multiple formats
4. ✅ Maintains formatting and structure
5. ✅ Works entirely in the browser

### Thank You!

Thank you for using the **MD Converter** tool. This powerful Blazor WebAssembly application makes it easy to convert your markdown documents while preserving all formatting and diagrams!

---

*Generated with MD Converter - A Blazor WebAssembly Application*
