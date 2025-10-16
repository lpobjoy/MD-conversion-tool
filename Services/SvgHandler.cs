using System.Text.RegularExpressions;
using Microsoft.JSInterop;

namespace MDConverter.Services;

public class SvgHandler
{
    private readonly IJSRuntime _jsRuntime;

    public SvgHandler(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Extract all SVG image references from markdown (both file paths and embedded SVG code)
    /// Returns tuples of (original reference, svg content, index)
    /// </summary>
    public async Task<List<SvgReference>> ExtractSvgReferencesAsync(string markdown)
    {
        var svgRefs = new List<SvgReference>();
        int index = 0;

        // Pattern 1: External SVG file references: ![alt](path/to/file.svg)
        var svgFilePattern = @"!\[([^\]]*)\]\(([^\)]+\.svg)\)";
        var fileMatches = Regex.Matches(markdown, svgFilePattern);
        
        foreach (Match match in fileMatches)
        {
            var altText = match.Groups[1].Value;
            var filePath = match.Groups[2].Value;
            
            svgRefs.Add(new SvgReference
            {
                Index = index++,
                OriginalReference = match.Value,
                AltText = altText,
                FilePath = filePath,
                IsEmbedded = false,
                SvgContent = null // Will need to be loaded if file exists
            });
        }

        // Pattern 2: Embedded SVG in markdown (less common but possible)
        // e.g., <svg>...</svg> directly in markdown
        var embeddedSvgPattern = @"<svg[^>]*>[\s\S]*?</svg>";
        var embeddedMatches = Regex.Matches(markdown, embeddedSvgPattern);
        
        foreach (Match match in embeddedMatches)
        {
            svgRefs.Add(new SvgReference
            {
                Index = index++,
                OriginalReference = match.Value,
                AltText = $"Embedded SVG {index}",
                FilePath = null,
                IsEmbedded = true,
                SvgContent = match.Value
            });
        }

        return svgRefs;
    }

    /// <summary>
    /// Convert SVG content to PNG base64 using browser canvas
    /// </summary>
    public async Task<string?> ConvertSvgToPngAsync(string svgContent, int width = 1600, int height = 1200)
    {
        try
        {
            var pngBase64 = await _jsRuntime.InvokeAsync<string>("convertSvgToPng", svgContent, width, height);
            return pngBase64;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Failed to convert SVG to PNG: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Replace SVG references in markdown with PNG data URIs or SVG data URIs
    /// </summary>
    public async Task<string> ProcessSvgReferencesAsync(
        string markdown, 
        List<SvgReference> svgRefs, 
        bool convertToPng = true)
    {
        string processedMarkdown = markdown;

        foreach (var svgRef in svgRefs)
        {
            string replacement;

            if (convertToPng && !string.IsNullOrEmpty(svgRef.SvgContent))
            {
                // Convert SVG to PNG
                var pngBase64 = await ConvertSvgToPngAsync(svgRef.SvgContent);
                
                if (!string.IsNullOrEmpty(pngBase64))
                {
                    replacement = $"![{svgRef.AltText}](data:image/png;base64,{pngBase64})";
                    Console.WriteLine($"✅ Converted SVG reference {svgRef.Index} to PNG data URI");
                }
                else
                {
                    // Fallback: keep original or use SVG data URI
                    if (svgRef.IsEmbedded && !string.IsNullOrEmpty(svgRef.SvgContent))
                    {
                        var svgBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(svgRef.SvgContent));
                        replacement = $"![{svgRef.AltText}](data:image/svg+xml;base64,{svgBase64})";
                        Console.WriteLine($"⚠️ Using SVG data URI fallback for reference {svgRef.Index}");
                    }
                    else
                    {
                        replacement = svgRef.OriginalReference;
                        Console.WriteLine($"⚠️ Keeping original SVG reference {svgRef.Index}");
                    }
                }
            }
            else if (!string.IsNullOrEmpty(svgRef.SvgContent))
            {
                // Keep as SVG data URI (for HTML output)
                var svgBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(svgRef.SvgContent));
                replacement = $"![{svgRef.AltText}](data:image/svg+xml;base64,{svgBase64})";
                Console.WriteLine($"✅ Embedded SVG {svgRef.Index} as data URI");
            }
            else
            {
                // External file that couldn't be loaded - keep original (will warn in Pandoc)
                replacement = svgRef.OriginalReference;
                Console.WriteLine($"⚠️ Keeping external SVG reference {svgRef.Index}: {svgRef.FilePath}");
            }

            // Replace in markdown
            processedMarkdown = processedMarkdown.Replace(svgRef.OriginalReference, replacement);
        }

        return processedMarkdown;
    }
}

public class SvgReference
{
    public int Index { get; set; }
    public string OriginalReference { get; set; } = "";
    public string AltText { get; set; } = "";
    public string? FilePath { get; set; }
    public bool IsEmbedded { get; set; }
    public string? SvgContent { get; set; }
}
