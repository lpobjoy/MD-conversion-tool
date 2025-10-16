using MDConverter.Models;
using Microsoft.JSInterop;

namespace MDConverter.Services;

public class PdfConverter
{
    private readonly IJSRuntime _jsRuntime;

    public PdfConverter(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<ConversionResult> ConvertToPdf(string html, List<MermaidDiagram> diagrams, string fileName = "document.pdf")
    {
        try
        {
            // Use JavaScript interop to generate PDF in the browser
            var pdfBytes = await _jsRuntime.InvokeAsync<byte[]>("generatePdfFromHtml", html, fileName);

            var result = new ConversionResult
            {
                Success = true,
                FileData = pdfBytes,
                FileName = fileName,
                MimeType = "application/pdf"
            };

            return result;
        }
        catch (Exception ex)
        {
            return new ConversionResult
            {
                Success = false,
                ErrorMessage = $"Error converting to PDF: {ex.Message}"
            };
        }
    }
}
