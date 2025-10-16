using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using MDConverter.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace MDConverter.Services;

public class MarkdownParser
{
    private readonly MarkdownPipeline _pipeline;

    public MarkdownParser()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UsePipeTables()
            .UseGridTables()
            .UseListExtras()
            .UseEmphasisExtras()
            .UseTaskLists()
            .Build();
    }

    public (string htmlContent, List<MermaidDiagram> diagrams) ParseMarkdown(string markdown)
    {
        var diagrams = ExtractMermaidDiagrams(markdown);
        var modifiedMarkdown = ReplaceMermaidWithPlaceholders(markdown, diagrams);
        var html = Markdown.ToHtml(modifiedMarkdown, _pipeline);
        
        return (html, diagrams);
    }

    public List<MermaidDiagram> ExtractMermaidDiagrams(string markdown)
    {
        var diagrams = new List<MermaidDiagram>();
        var mermaidPattern = @"```mermaid\s*([\s\S]*?)```";
        var matches = Regex.Matches(markdown, mermaidPattern);

        for (int i = 0; i < matches.Count; i++)
        {
            var match = matches[i];
            var code = match.Groups[1].Value.Trim();
            Console.WriteLine($"Extracted Mermaid code {i}:");
            Console.WriteLine(code);
            Console.WriteLine("--- End of extracted code ---");
            
            diagrams.Add(new MermaidDiagram
            {
                Code = code,
                Index = i
            });
        }

        return diagrams;
    }

    private string ReplaceMermaidWithPlaceholders(string markdown, List<MermaidDiagram> diagrams)
    {
        var result = markdown;
        foreach (var diagram in diagrams)
        {
            var pattern = @"```mermaid\s*" + Regex.Escape(diagram.Code) + @"\s*```";
            result = Regex.Replace(result, pattern, $"{{{{MERMAID_PLACEHOLDER_{diagram.Id}}}}}", RegexOptions.Singleline);
        }
        return result;
    }

    public string ReplacePlaceholdersWithImages(string html, List<MermaidDiagram> diagrams)
    {
        Console.WriteLine($"ReplacePlaceholdersWithImages called with {diagrams.Count} diagrams");
        var result = html;
        
        foreach (var diagram in diagrams)
        {
            var placeholder = $"{{{{MERMAID_PLACEHOLDER_{diagram.Id}}}}}";
            Console.WriteLine($"Looking for placeholder: {placeholder}");
            Console.WriteLine($"HTML contains placeholder: {html.Contains(placeholder)}");
            
            if (string.IsNullOrEmpty(diagram.RenderedSvg))
            {
                Console.WriteLine($"WARNING: Diagram {diagram.Id} has no rendered SVG!");
                continue;
            }
            
            var imgTag = $"<img src='data:image/svg+xml;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(diagram.RenderedSvg))}' alt='Mermaid Diagram {diagram.Index}' />";
            Console.WriteLine($"Replacing with img tag, length: {imgTag.Length}");
            
            result = result.Replace(placeholder, imgTag);
        }
        
        Console.WriteLine($"Replacement complete. Result length: {result.Length}");
        return result;
    }

    public string ReplacePlaceholdersWithSvgFiles(string html, List<MermaidDiagram> diagrams, string outputDir)
    {
        Console.WriteLine($"ReplacePlaceholdersWithSvgFiles called with {diagrams.Count} diagrams");
        var result = html;
        
        // Ensure output directory exists
        Directory.CreateDirectory(outputDir);
        
        foreach (var diagram in diagrams)
        {
            var placeholder = $"{{{{MERMAID_PLACEHOLDER_{diagram.Id}}}}}";
            Console.WriteLine($"Looking for placeholder: {placeholder}");
            
            if (string.IsNullOrEmpty(diagram.RenderedSvg))
            {
                Console.WriteLine($"WARNING: Diagram {diagram.Id} has no rendered SVG!");
                continue;
            }
            
            // Save SVG to file
            var svgFileName = $"mermaid-{diagram.Id}.svg";
            var svgFilePath = Path.Combine(outputDir, svgFileName);
            
            // Use the original SVG (before cleanup) for file output to preserve styling
            var originalSvg = diagram.RenderedSvg;
            File.WriteAllText(svgFilePath, originalSvg);
            Console.WriteLine($"Saved SVG file: {svgFilePath}, length: {originalSvg.Length}");
            
            // Create img tag that references the file
            var imgTag = $"<img src='{svgFileName}' alt='Mermaid Diagram {diagram.Index}' data-mermaid-id='{diagram.Id}' />";
            Console.WriteLine($"Replacing with file reference: {imgTag}");
            
            result = result.Replace(placeholder, imgTag);
        }
        
        Console.WriteLine($"SVG file replacement complete. Result length: {result.Length}");
        return result;
    }
}
