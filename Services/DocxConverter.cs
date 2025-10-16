using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using MDConverter.Models;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

namespace MDConverter.Services;

public class DocxConverter
{
    public async Task<ConversionResult> ConvertToDocx(string html, List<MermaidDiagram> diagrams, string fileName = "document.docx", string? svgFilesDir = null)
    {
        try
        {
            Console.WriteLine($"ConvertToDocx called");
            Console.WriteLine($"HTML length: {html.Length}");
            Console.WriteLine($"HTML contains <img: {html.Contains("<img")}");
            Console.WriteLine($"Number of diagrams: {diagrams.Count}");
            
            // Log first 500 chars of HTML to see structure
            Console.WriteLine($"HTML start: {html.Substring(0, Math.Min(500, html.Length))}");
            
            using var memoryStream = new MemoryStream();
            using (var wordDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document))
            {
                var mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                // Parse HTML and convert to Word elements
                var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(html);
                
                Console.WriteLine($"HtmlAgilityPack parsed, DocumentNode children: {htmlDoc.DocumentNode.ChildNodes.Count}");

                ProcessHtmlNode(htmlDoc.DocumentNode, body, mainPart, diagrams, svgFilesDir);

                mainPart.Document.Save();
            }

            var result = new ConversionResult
            {
                Success = true,
                FileData = memoryStream.ToArray(),
                FileName = fileName,
                MimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
            };

            return await Task.FromResult(result);
        }
        catch (Exception ex)
        {
            return new ConversionResult
            {
                Success = false,
                ErrorMessage = $"Error converting to DOCX: {ex.Message}"
            };
        }
    }

    private void ProcessHtmlNode(HtmlNode node, Body body, MainDocumentPart mainPart, List<MermaidDiagram> diagrams, string? svgFilesDir = null)
    {
        Console.WriteLine($"ProcessHtmlNode called with {node.ChildNodes.Count} child nodes");
        
        foreach (var childNode in node.ChildNodes)
        {
            Console.WriteLine($"Processing node: {childNode.Name}");
            
            switch (childNode.Name.ToLower())
            {
                case "h1":
                    body.AppendChild(CreateHeading(childNode.InnerText, 1));
                    break;
                case "h2":
                    body.AppendChild(CreateHeading(childNode.InnerText, 2));
                    break;
                case "h3":
                    body.AppendChild(CreateHeading(childNode.InnerText, 3));
                    break;
                case "h4":
                    body.AppendChild(CreateHeading(childNode.InnerText, 4));
                    break;
                case "p":
                    // Check if this paragraph contains only an img tag (Mermaid diagram)
                    var imgNode = childNode.SelectSingleNode(".//img");
                    if (imgNode != null && childNode.ChildNodes.Count == 1)
                    {
                        Console.WriteLine("Paragraph contains single img tag - processing as image");
                        ProcessImage(imgNode, body, mainPart, diagrams, svgFilesDir);
                    }
                    else
                    {
                        // Create empty paragraph and let ProcessInlineElements fill it
                        var para = new Paragraph();
                        ProcessInlineElements(childNode, para);
                        
                        // Only add if paragraph has content
                        if (para.ChildElements.Any())
                        {
                            body.AppendChild(para);
                        }
                    }
                    break;
                case "ul":
                case "ol":
                    ProcessList(childNode, body);
                    break;
                case "img":
                    ProcessImage(childNode, body, mainPart, diagrams, svgFilesDir);
                    break;
                case "code":
                case "pre":
                    body.AppendChild(CreateCodeBlock(childNode.InnerText));
                    break;
                case "table":
                    ProcessTable(childNode, body);
                    break;
                case "#text":
                    if (!string.IsNullOrWhiteSpace(childNode.InnerText))
                    {
                        body.AppendChild(CreateParagraph(childNode.InnerText));
                    }
                    break;
                default:
                    if (childNode.HasChildNodes)
                    {
                        ProcessHtmlNode(childNode, body, mainPart, diagrams);
                    }
                    break;
            }
        }
    }

    private Paragraph CreateHeading(string text, int level)
    {
        // Decode HTML entities
        var decodedText = System.Net.WebUtility.HtmlDecode(text);
        var paragraph = new Paragraph(new Run(new Text(decodedText)));
        var properties = new ParagraphProperties(
            new ParagraphStyleId() { Val = $"Heading{level}" }
        );
        paragraph.PrependChild(properties);
        return paragraph;
    }

    private Paragraph CreateParagraph(string text)
    {
        // Decode HTML entities
        var decodedText = System.Net.WebUtility.HtmlDecode(text);
        return new Paragraph(new Run(new Text(decodedText)));
    }

    private Paragraph CreateCodeBlock(string code)
    {
        var paragraph = new Paragraph();
        var run = new Run(new Text(code));
        
        var runProperties = new RunProperties(
            new RunFonts() { Ascii = "Courier New" },
            new FontSize() { Val = "20" }
        );
        
        run.PrependChild(runProperties);
        paragraph.AppendChild(run);
        
        return paragraph;
    }

    private void ProcessInlineElements(HtmlNode node, Paragraph paragraph)
    {
        Console.WriteLine($"ProcessInlineElements: {node.ChildNodes.Count} children");
        
        foreach (var child in node.ChildNodes)
        {
            Console.WriteLine($"  Inline element: {child.Name}");
            
            if (child.Name == "strong" || child.Name == "b")
            {
                var text = System.Net.WebUtility.HtmlDecode(child.InnerText);
                var run = new Run(new Text(text));
                run.RunProperties = new RunProperties(new Bold());
                paragraph.AppendChild(run);
            }
            else if (child.Name == "em" || child.Name == "i")
            {
                var text = System.Net.WebUtility.HtmlDecode(child.InnerText);
                var run = new Run(new Text(text));
                run.RunProperties = new RunProperties(new Italic());
                paragraph.AppendChild(run);
            }
            else if (child.Name == "code")
            {
                var text = System.Net.WebUtility.HtmlDecode(child.InnerText);
                var run = new Run(new Text(text));
                run.RunProperties = new RunProperties(
                    new RunFonts() { Ascii = "Courier New" },
                    new FontSize() { Val = "20" }
                );
                paragraph.AppendChild(run);
            }
            else if (child.Name == "#text")
            {
                var text = System.Net.WebUtility.HtmlDecode(child.InnerText);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    paragraph.AppendChild(new Run(new Text(text)));
                }
            }
            else if (child.Name == "img")
            {
                Console.WriteLine("  Found img tag inside paragraph!");
                // Note: We can't embed images directly in a paragraph that already has text
                // This is a limitation - we'd need to create a separate paragraph for the image
                // For now, log it so we know this is happening
            }
        }
    }

    private void ProcessList(HtmlNode listNode, Body body)
    {
        foreach (var item in listNode.SelectNodes(".//li") ?? Enumerable.Empty<HtmlNode>())
        {
            // Create empty paragraph and let ProcessInlineElements fill it
            var paragraph = new Paragraph();
            ProcessInlineElements(item, paragraph);
            
            // If no formatted content was added, use plain text
            if (!paragraph.ChildElements.Any())
            {
                paragraph.AppendChild(new Run(new Text(System.Net.WebUtility.HtmlDecode(item.InnerText))));
            }
            
            var properties = new ParagraphProperties(
                new NumberingProperties(
                    new NumberingLevelReference() { Val = 0 },
                    new NumberingId() { Val = 1 }
                )
            );
            paragraph.PrependChild(properties);
            body.AppendChild(paragraph);
        }
    }

    private (int width, int height) GetPngDimensions(byte[] pngBytes)
    {
        // PNG signature is 8 bytes, then comes IHDR chunk
        // IHDR chunk: 4 bytes length + 4 bytes "IHDR" + 4 bytes width + 4 bytes height
        if (pngBytes.Length < 24)
            return (800, 600); // Default fallback
        
        // Width is at bytes 16-19 (big-endian)
        int width = (pngBytes[16] << 24) | (pngBytes[17] << 16) | (pngBytes[18] << 8) | pngBytes[19];
        
        // Height is at bytes 20-23 (big-endian)
        int height = (pngBytes[20] << 24) | (pngBytes[21] << 16) | (pngBytes[22] << 8) | pngBytes[23];
        
        Console.WriteLine($"Extracted PNG dimensions: {width}x{height}");
        return (width, height);
    }

    private void ProcessImage(HtmlNode imgNode, Body body, MainDocumentPart mainPart, List<MermaidDiagram> diagrams, string? svgFilesDir = null)
    {
        var src = imgNode.GetAttributeValue("src", "");
        Console.WriteLine($"ProcessImage called with src: {src.Substring(0, Math.Min(100, src.Length))}...");
        
        // Check if it's a linked SVG file (new approach)
        if (src.EndsWith(".svg") && !src.StartsWith("data:"))
        {
            Console.WriteLine($"Found linked SVG file: {src}");
            ProcessLinkedSvgImage(imgNode, body, mainPart, diagrams, svgFilesDir, src);
            return;
        }
        
        // Check if it's a base64 PNG (mermaid diagram converted to PNG)
        if (src.StartsWith("data:image/png;base64,"))
        {
            Console.WriteLine($"Found PNG data URI image");
            try
            {
                // Extract the base64 part from the src
                var pngBase64 = src.Substring("data:image/png;base64,".Length);
                var pngBytes = Convert.FromBase64String(pngBase64);
                
                Console.WriteLine($"Decoded PNG, size: {pngBytes.Length} bytes");
                
                // Get actual PNG dimensions
                var (width, height) = GetPngDimensions(pngBytes);
                
                // Convert pixels to EMUs (English Metric Units)
                // 1 inch = 914400 EMUs, assuming 96 DPI: pixels * 914400 / 96 = pixels * 9525
                long widthEmu = width * 9525L;
                long heightEmu = height * 9525L;
                
                // Optional: Scale down if too large (max 6.5 inches wide for standard margins)
                const long maxWidthEmu = 5943600L; // 6.5 inches
                if (widthEmu > maxWidthEmu)
                {
                    double scale = (double)maxWidthEmu / widthEmu;
                    widthEmu = maxWidthEmu;
                    heightEmu = (long)(heightEmu * scale);
                    Console.WriteLine($"Scaled down image to fit page, new dimensions: {widthEmu} x {heightEmu} EMUs");
                }
                
                Console.WriteLine($"Image dimensions in EMUs: {widthEmu} x {heightEmu}");
                
                // Add PNG as image part
                var imagePart = mainPart.AddImagePart(ImagePartType.Png);
                using (var stream = new MemoryStream(pngBytes))
                {
                    imagePart.FeedData(stream);
                }
                
                var imageId = mainPart.GetIdOfPart(imagePart);
                Console.WriteLine($"Added PNG image part with ID: {imageId}");
                
                // Create the image element with drawing using actual dimensions
                var element = new Drawing(
                    new DW.Inline(
                        new DW.Extent() { Cx = widthEmu, Cy = heightEmu },
                        new DW.EffectExtent() { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                        new DW.DocProperties() { Id = (UInt32Value)1U, Name = "Mermaid Diagram" },
                        new DW.NonVisualGraphicFrameDrawingProperties(
                            new A.GraphicFrameLocks() { NoChangeAspect = true }),
                        new A.Graphic(
                            new A.GraphicData(
                                new PIC.Picture(
                                    new PIC.NonVisualPictureProperties(
                                        new PIC.NonVisualDrawingProperties() { Id = (UInt32Value)0U, Name = "Mermaid Diagram" },
                                        new PIC.NonVisualPictureDrawingProperties()),
                                    new PIC.BlipFill(
                                        new A.Blip() { Embed = imageId },
                                        new A.Stretch(new A.FillRectangle())),
                                    new PIC.ShapeProperties(
                                        new A.Transform2D(
                                            new A.Offset() { X = 0L, Y = 0L },
                                            new A.Extents() { Cx = widthEmu, Cy = heightEmu }),
                                        new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle }))
                            ) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                    ) { DistanceFromTop = (UInt32Value)0U, DistanceFromBottom = (UInt32Value)0U, DistanceFromLeft = (UInt32Value)0U, DistanceFromRight = (UInt32Value)0U }
                );
                
                var paragraph = new Paragraph(new Run(element));
                body.AppendChild(paragraph);
                Console.WriteLine($"Added PNG image to document with correct aspect ratio");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing PNG data URI: {ex.Message}");
            }
        }
        
        // Check if it's a base64 SVG (mermaid diagram) - legacy approach
        if (src.StartsWith("data:image/svg+xml;base64,"))
        {
            Console.WriteLine($"Found SVG image, looking for matching diagram from {diagrams.Count} diagrams");
            try
            {
                // Extract the base64 part from the src
                var srcBase64 = src.Substring("data:image/svg+xml;base64,".Length);
                
                // Find the corresponding diagram with PNG data
                MermaidDiagram? matchingDiagram = null;
                foreach (var diagram in diagrams)
                {
                    if (string.IsNullOrEmpty(diagram.RenderedSvg))
                    {
                        Console.WriteLine($"Diagram {diagram.Id} has no RenderedSvg");
                        continue;
                    }
                    
                    var diagramSvgBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(diagram.RenderedSvg));
                    Console.WriteLine($"Comparing with diagram {diagram.Id}, base64 match: {srcBase64 == diagramSvgBase64}");
                    
                    if (srcBase64 == diagramSvgBase64)
                    {
                        matchingDiagram = diagram;
                        Console.WriteLine($"Found matching diagram: {diagram.Id}");
                        break;
                    }
                }

                if (matchingDiagram != null && !string.IsNullOrEmpty(matchingDiagram.RenderedSvg))
                {
                    Console.WriteLine($"Using SVG for diagram {matchingDiagram.Id}, SVG length: {matchingDiagram.RenderedSvg.Length}");
                    
                    // Use SVG data directly (Word 2016+ supports SVG natively)
                    var svgBytes = System.Text.Encoding.UTF8.GetBytes(matchingDiagram.RenderedSvg);
                    
                    // Add SVG as image part
                    var imagePart = mainPart.AddImagePart(ImagePartType.Svg);
                    using (var stream = new MemoryStream(svgBytes))
                    {
                        imagePart.FeedData(stream);
                    }
                    
                    var imageId = mainPart.GetIdOfPart(imagePart);
                    
                    // Create the image element with drawing (SVG will be embedded as vector graphics)
                    var element = new Drawing(
                        new DW.Inline(
                            new DW.Extent() { Cx = 5486400L, Cy = 3200400L }, // Width and height in EMUs
                            new DW.EffectExtent() { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                            new DW.DocProperties() { Id = (UInt32Value)1U, Name = "Mermaid Diagram (SVG)" },
                            new DW.NonVisualGraphicFrameDrawingProperties(
                                new A.GraphicFrameLocks() { NoChangeAspect = true }),
                            new A.Graphic(
                                new A.GraphicData(
                                    new PIC.Picture(
                                        new PIC.NonVisualPictureProperties(
                                            new PIC.NonVisualDrawingProperties() { Id = (UInt32Value)0U, Name = "diagram.svg" },
                                            new PIC.NonVisualPictureDrawingProperties()),
                                        new PIC.BlipFill(
                                            new A.Blip(
                                                new A.BlipExtensionList(
                                                    new A.BlipExtension() { Uri = "{28A0092B-C50C-407E-A947-70E740481C1C}" })
                                            )
                                            {
                                                Embed = imageId,
                                                CompressionState = A.BlipCompressionValues.Print
                                            },
                                            new A.Stretch(new A.FillRectangle())),
                                        new PIC.ShapeProperties(
                                            new A.Transform2D(
                                                new A.Offset() { X = 0L, Y = 0L },
                                                new A.Extents() { Cx = 5486400L, Cy = 3200400L }),
                                            new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle }))
                                ) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                        )
                        {
                            DistanceFromTop = (UInt32Value)0U,
                            DistanceFromBottom = (UInt32Value)0U,
                            DistanceFromLeft = (UInt32Value)0U,
                            DistanceFromRight = (UInt32Value)0U,
                            EditId = "50D07946"
                        });

                    var paragraph = new Paragraph(new Run(element));
                    body.AppendChild(paragraph);
                }
                else
                {
                    Console.WriteLine("No matching diagram found or PNG is empty");
                    // Fallback to text placeholder
                    var paragraph = new Paragraph(new Run(new Text("[Mermaid Diagram - No PNG available]")));
                    body.AppendChild(paragraph);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing image: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                // Fallback to placeholder
                var paragraph = new Paragraph(new Run(new Text($"[Mermaid Diagram - Error: {ex.Message}]")));
                body.AppendChild(paragraph);
            }
        }
    }

    private void ProcessLinkedSvgImage(HtmlNode imgNode, Body body, MainDocumentPart mainPart, List<MermaidDiagram> diagrams, string? svgFilesDir, string src)
    {
        try
        {
            // Check if this is a mermaid diagram by looking for data-mermaid-id attribute
            var mermaidId = imgNode.GetAttributeValue("data-mermaid-id", "");
            if (!string.IsNullOrEmpty(mermaidId))
            {
                Console.WriteLine($"Processing Mermaid diagram file: {src}, ID: {mermaidId}");
                
                // Find the matching diagram
                var matchingDiagram = diagrams.FirstOrDefault(d => d.Id == mermaidId);
                if (matchingDiagram != null && !string.IsNullOrEmpty(matchingDiagram.RenderedSvg))
                {
                    Console.WriteLine($"Found matching diagram: {matchingDiagram.Id}, SVG length: {matchingDiagram.RenderedSvg.Length}");
                    
                    // Use the original SVG data (preserves all styling)
                    var svgBytes = System.Text.Encoding.UTF8.GetBytes(matchingDiagram.RenderedSvg);
                    
                    // Add SVG as image part
                    var imagePart = mainPart.AddImagePart(ImagePartType.Svg);
                    using (var stream = new MemoryStream(svgBytes))
                    {
                        imagePart.FeedData(stream);
                    }
                    
                    var imageId = mainPart.GetIdOfPart(imagePart);
                    
                    // Create the image element with drawing (SVG will be embedded as vector graphics)
                    var element = new Drawing(
                        new DW.Inline(
                            new DW.Extent() { Cx = 5486400L, Cy = 3200400L }, // Width and height in EMUs
                            new DW.EffectExtent() { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                            new DW.DocProperties() { Id = (UInt32Value)1U, Name = "Mermaid Diagram (SVG)" },
                            new DW.NonVisualGraphicFrameDrawingProperties(
                                new A.GraphicFrameLocks() { NoChangeAspect = true }),
                            new A.Graphic(
                                new A.GraphicData(
                                    new PIC.Picture(
                                        new PIC.NonVisualPictureProperties(
                                            new PIC.NonVisualDrawingProperties() { Id = (UInt32Value)0U, Name = "diagram.svg" },
                                            new PIC.NonVisualPictureDrawingProperties()),
                                        new PIC.BlipFill(
                                            new A.Blip() { Embed = imageId },
                                            new A.Stretch(new A.FillRectangle())),
                                        new PIC.ShapeProperties(
                                            new A.Transform2D(
                                                new A.Offset() { X = 0L, Y = 0L },
                                                new A.Extents() { Cx = 5486400L, Cy = 3200400L }),
                                            new A.PresetGeometry(
                                                new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle })))
                            { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                        )
                        {
                            DistanceFromTop = (UInt32Value)0U,
                            DistanceFromBottom = (UInt32Value)0U,
                            DistanceFromLeft = (UInt32Value)0U,
                            DistanceFromRight = (UInt32Value)0U,
                            EditId = "50D07946"
                        });

                    var paragraph = new Paragraph();
                    var run = new Run(element);
                    paragraph.AppendChild(run);
                    body.AppendChild(paragraph);
                    
                    Console.WriteLine($"Successfully embedded linked SVG diagram: {mermaidId}");
                    return;
                }
            }
            
            // If not a mermaid diagram or no matching diagram found, try to load from file system
            if (!string.IsNullOrEmpty(svgFilesDir))
            {
                var svgFilePath = Path.Combine(svgFilesDir, src);
                if (File.Exists(svgFilePath))
                {
                    Console.WriteLine($"Loading SVG from file: {svgFilePath}");
                    var svgContent = File.ReadAllText(svgFilePath);
                    var svgBytes = System.Text.Encoding.UTF8.GetBytes(svgContent);
                    
                    // Add SVG as image part
                    var imagePart = mainPart.AddImagePart(ImagePartType.Svg);
                    using (var stream = new MemoryStream(svgBytes))
                    {
                        imagePart.FeedData(stream);
                    }
                    
                    var imageId = mainPart.GetIdOfPart(imagePart);
                    
                    // Create simple image element
                    var element = new Drawing(
                        new DW.Inline(
                            new DW.Extent() { Cx = 5486400L, Cy = 3200400L },
                            new DW.EffectExtent() { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                            new DW.DocProperties() { Id = (UInt32Value)1U, Name = "SVG Image" },
                            new DW.NonVisualGraphicFrameDrawingProperties(
                                new A.GraphicFrameLocks() { NoChangeAspect = true }),
                            new A.Graphic(
                                new A.GraphicData(
                                    new PIC.Picture(
                                        new PIC.NonVisualPictureProperties(
                                            new PIC.NonVisualDrawingProperties() { Id = (UInt32Value)0U, Name = src },
                                            new PIC.NonVisualPictureDrawingProperties()),
                                        new PIC.BlipFill(
                                            new A.Blip() { Embed = imageId },
                                            new A.Stretch(new A.FillRectangle())),
                                        new PIC.ShapeProperties(
                                            new A.Transform2D(
                                                new A.Offset() { X = 0L, Y = 0L },
                                                new A.Extents() { Cx = 5486400L, Cy = 3200400L }),
                                            new A.PresetGeometry(
                                                new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle })))
                            { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })));

                    var paragraph = new Paragraph();
                    var run = new Run(element);
                    paragraph.AppendChild(run);
                    body.AppendChild(paragraph);
                    
                    Console.WriteLine($"Successfully embedded SVG from file: {src}");
                    return;
                }
            }
            
            Console.WriteLine($"Could not find SVG file or matching diagram for: {src}");
            // Fallback to placeholder
            var fallbackParagraph = new Paragraph(new Run(new Text($"[SVG Image: {src}]")));
            body.AppendChild(fallbackParagraph);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing linked SVG image: {ex.Message}");
            // Fallback to placeholder
            var paragraph = new Paragraph(new Run(new Text($"[SVG Image - Error: {ex.Message}]")));
            body.AppendChild(paragraph);
        }
    }

    private void ProcessTable(HtmlNode tableNode, Body body)
    {
        var table = new Table();
        
        // Add table properties with borders
        var tableProperties = new TableProperties(
            new TableBorders(
                new TopBorder() { Val = BorderValues.Single, Size = 4 },
                new BottomBorder() { Val = BorderValues.Single, Size = 4 },
                new LeftBorder() { Val = BorderValues.Single, Size = 4 },
                new RightBorder() { Val = BorderValues.Single, Size = 4 },
                new InsideHorizontalBorder() { Val = BorderValues.Single, Size = 4 },
                new InsideVerticalBorder() { Val = BorderValues.Single, Size = 4 }
            ),
            new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct }
        );
        table.AppendChild(tableProperties);
        
        var rows = tableNode.SelectNodes(".//tr");
        if (rows != null)
        {
            bool isFirstRow = true;
            foreach (var row in rows)
            {
                var tableRow = new TableRow();
                var cells = row.SelectNodes(".//td|.//th");
                
                if (cells != null)
                {
                    foreach (var cell in cells)
                    {
                        // Decode HTML entities
                        var cellText = System.Net.WebUtility.HtmlDecode(cell.InnerText);
                        var run = new Run(new Text(cellText));
                        
                        // Bold header cells
                        if (cell.Name == "th" || isFirstRow)
                        {
                            run.RunProperties = new RunProperties(new Bold());
                        }
                        
                        var para = new Paragraph(run);
                        var tableCell = new TableCell(para);
                        
                        // Add cell borders and padding
                        var cellProperties = new TableCellProperties(
                            new TableCellMargin(
                                new TopMargin() { Width = "100", Type = TableWidthUnitValues.Dxa },
                                new BottomMargin() { Width = "100", Type = TableWidthUnitValues.Dxa },
                                new LeftMargin() { Width = "100", Type = TableWidthUnitValues.Dxa },
                                new RightMargin() { Width = "100", Type = TableWidthUnitValues.Dxa }
                            )
                        );
                        tableCell.AppendChild(cellProperties);
                        
                        tableRow.AppendChild(tableCell);
                    }
                }
                
                table.AppendChild(tableRow);
                isFirstRow = false;
            }
        }
        
        body.AppendChild(table);
    }
}
