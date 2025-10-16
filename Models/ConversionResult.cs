namespace MDConverter.Models;

public class ConversionResult
{
    public bool Success { get; set; }
    public byte[]? FileData { get; set; }
    public string? FileName { get; set; }
    public string? MimeType { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> Warnings { get; set; } = new();
}

public class MermaidDiagram
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Code { get; set; } = string.Empty;
    public string RenderedSvg { get; set; } = string.Empty;
    public string? RenderedPng { get; set; } // Base64 PNG for DOCX/PDF
    public int Index { get; set; }
}

public enum ExportFormat
{
    Files,  // PNG files + updated markdown
    Pandoc, // PNG files + markdown + pandoc command
    Docx,
    Pdf,
    Html
}
