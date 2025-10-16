using Microsoft.JSInterop;
using MDConverter.Models;

namespace MDConverter.Services;

public class MermaidService
{
    private readonly IJSRuntime _jsRuntime;

    public MermaidService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    // Note: Mermaid is now initialized in index.html using ESM import pattern
    // as per official documentation: https://mermaid.js.org/config/usage.html#api-usage

    public async Task<string> RenderMermaid(string mermaidCode, string elementId)
    {
        try
        {
            var result = await _jsRuntime.InvokeAsync<string>("renderMermaid", mermaidCode, elementId);
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error rendering mermaid: {ex.Message}");
            return $"<div>Error rendering diagram: {ex.Message}</div>";
        }
    }

    public async Task<List<MermaidDiagram>> RenderAllDiagrams(List<MermaidDiagram> diagrams)
    {
        Console.WriteLine($"RenderAllDiagrams called with {diagrams.Count} diagrams");
        
        foreach (var diagram in diagrams)
        {
            Console.WriteLine($"Processing diagram: {diagram.Id}");
            Console.WriteLine($"Code length: {diagram.Code?.Length ?? 0}");
            
            try
            {
                diagram.RenderedSvg = await RenderMermaid(diagram.Code, diagram.Id);
                Console.WriteLine($"SVG rendered for {diagram.Id}, length: {diagram.RenderedSvg?.Length ?? 0}");
                
                // Note: PNG conversion is kept for backward compatibility but DOCX now uses SVG natively
                // PDF generation uses html2canvas which handles SVG automatically
                try
                {
                    var pngBase64 = await _jsRuntime.InvokeAsync<string>("convertSvgToPng", diagram.RenderedSvg, 800, 600);
                    diagram.RenderedPng = pngBase64;
                    Console.WriteLine($"PNG converted for {diagram.Id}, base64 length: {pngBase64?.Length ?? 0}");
                }
                catch (Exception pngEx)
                {
                    Console.WriteLine($"Failed to convert diagram {diagram.Id} to PNG: {pngEx.Message}");
                    // Not critical - DOCX uses SVG, PDF uses html2canvas
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to render diagram {diagram.Id}: {ex.Message}");
                diagram.RenderedSvg = $"<svg><text>Error rendering diagram</text></svg>";
            }
        }
        
        Console.WriteLine($"RenderAllDiagrams completed");
        return diagrams;
    }
}
