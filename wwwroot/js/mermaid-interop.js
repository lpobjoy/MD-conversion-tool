// Mermaid JavaScript Interop for Blazor
// Note: Mermaid is initialized in index.html using the ESM import pattern

// Helper function to wait for Mermaid to be available
window.waitForMermaid = async function() {
    let attempts = 0;
    const maxAttempts = 50; // 5 seconds max
    
    while (typeof window.mermaid === 'undefined' && attempts < maxAttempts) {
        await new Promise(resolve => setTimeout(resolve, 100));
        attempts++;
    }
    
    if (typeof window.mermaid === 'undefined') {
        throw new Error('Mermaid library failed to load after 5 seconds');
    }
    
    return window.mermaid;
};

// Render a Mermaid diagram using the official API
// Follows: https://mermaid.js.org/config/usage.html#api-usage
window.renderMermaid = async function(code, elementId) {
    try {
        // Wait for Mermaid to be available
        const mermaid = await window.waitForMermaid();
        
        console.log('Rendering Mermaid diagram:', elementId);
        console.log('Full Mermaid code:');
        console.log(code);
        console.log('--- End of Mermaid code ---');

        // Generate unique ID for the diagram
        const id = 'mermaid-' + elementId;
        
        // Add a small delay to ensure fonts are loaded
        await new Promise(resolve => setTimeout(resolve, 100));
        
        // Create a temporary div to ensure DOM context
        const tempDiv = document.createElement('div');
        tempDiv.style.visibility = 'hidden';
        tempDiv.style.position = 'absolute';
        tempDiv.style.top = '-9999px';
        document.body.appendChild(tempDiv);
        
        try {
            // Render the diagram
            const { svg } = await mermaid.render(id, code);
            
            // Clean up temp element
            document.body.removeChild(tempDiv);
            
            console.log('Successfully rendered diagram:', elementId);
            console.log('SVG length:', svg.length);
            
        // Check if SVG contains text elements
        const hasText = svg.includes('<text') || svg.includes('textPath');
        const hasForeignObject = svg.includes('<foreignObject');
        const hasSpanElements = svg.includes('<span');
        const hasDivElements = svg.includes('<div');
        const hasLabels = svg.includes('Start') || svg.includes('Decision') || svg.includes('Action');
        
        console.log('SVG contains <text> elements:', hasText);
        console.log('SVG contains <foreignObject> elements:', hasForeignObject);
        console.log('SVG contains <span> elements:', hasSpanElements);
        console.log('SVG contains <div> elements:', hasDivElements);
        console.log('SVG contains expected labels:', hasLabels);
        
        // Log a larger portion to see the structure
        console.log('SVG content (first 1000 chars):', svg.substring(0, 1000));
        
        // Look for specific patterns
        const foreignObjectMatch = svg.match(/<foreignObject[^>]*>(.*?)<\/foreignObject>/s);
        if (foreignObjectMatch) {
            console.log('Found foreignObject content:', foreignObjectMatch[1]);
        }
        
        // Clean up SVG by converting foreignObject to text elements
        const cleanedSvg = cleanupSvgForWordCompatibility(svg);
        console.log('SVG cleaned, length change:', svg.length, '->', cleanedSvg.length);
        
        return cleanedSvg;
        } catch (renderError) {
            // Clean up temp element on error
            document.body.removeChild(tempDiv);
            throw renderError;
        }
    } catch (error) {
        console.error('Error rendering Mermaid diagram:', error);
        console.error('Element ID:', elementId);
        console.error('Code:', code);
        return `<svg><text x="10" y="20" fill="red">Error: ${error.message}</text></svg>`;
    }
};

window.downloadFile = function(fileName, mimeType, base64Data) {
    const link = document.createElement('a');
    link.download = fileName;
    link.href = `data:${mimeType};base64,${base64Data}`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

// Read file content from input element
window.readFileContent = function(fileInputElement) {
    return new Promise((resolve, reject) => {
        if (!fileInputElement || !fileInputElement.files || fileInputElement.files.length === 0) {
            reject(new Error('No file selected'));
            return;
        }

        const file = fileInputElement.files[0];
        const reader = new FileReader();
        
        reader.onload = function(e) {
            resolve(e.target.result);
        };
        
        reader.onerror = function(e) {
            reject(new Error('Error reading file: ' + e.target.error));
        };
        
        reader.readAsText(file);
    });
};

// Convert SVG to PNG for DOCX embedding
window.convertSvgToPng = async function(svgString, maxWidth, maxHeight) {
    console.log('Converting SVG to PNG...');
    console.log('SVG length:', svgString.length);
    console.log('Max dimensions:', maxWidth, 'x', maxHeight);
    
    return new Promise((resolve, reject) => {
        try {
            // Parse SVG to get natural dimensions
            const parser = new DOMParser();
            const svgDoc = parser.parseFromString(svgString, 'image/svg+xml');
            const svgElement = svgDoc.documentElement;
            
            // Get SVG's natural dimensions
            let naturalWidth = svgElement.viewBox?.baseVal?.width || svgElement.width?.baseVal?.value || 800;
            let naturalHeight = svgElement.viewBox?.baseVal?.height || svgElement.height?.baseVal?.value || 600;
            
            console.log('Natural SVG dimensions:', naturalWidth, 'x', naturalHeight);
            
            // Calculate scale factor to fit within max dimensions while preserving aspect ratio
            let scale = 1;
            if (maxWidth && maxHeight) {
                const scaleX = maxWidth / naturalWidth;
                const scaleY = maxHeight / naturalHeight;
                scale = Math.min(scaleX, scaleY, 2); // Cap at 2x for quality, but allow scaling up
            }
            
            // Calculate final dimensions (preserve aspect ratio)
            const canvasWidth = Math.round(naturalWidth * scale);
            const canvasHeight = Math.round(naturalHeight * scale);
            
            console.log('Final canvas dimensions:', canvasWidth, 'x', canvasHeight, '(scale:', scale + ')');
            
            // Create canvas with correct dimensions
            const canvas = document.createElement('canvas');
            canvas.width = canvasWidth;
            canvas.height = canvasHeight;
            
            const ctx = canvas.getContext('2d');
            ctx.fillStyle = 'white';
            ctx.fillRect(0, 0, canvasWidth, canvasHeight);
            
            // Create an image with data URI to avoid CORS issues
            const img = new Image();
            
            img.onload = function() {
                try {
                    console.log('Image loaded, drawing to canvas...');
                    ctx.drawImage(img, 0, 0, canvasWidth, canvasHeight);
                    
                    // Convert to PNG base64
                    const pngData = canvas.toDataURL('image/png');
                    const base64 = pngData.split(',')[1];
                    
                    console.log('PNG conversion successful, base64 length:', base64.length);
                    resolve(base64);
                } catch (err) {
                    console.error('Failed to draw image:', err);
                    reject(new Error('Failed to draw image: ' + err.message));
                }
            };
            
            img.onerror = function(err) {
                console.error('Failed to load SVG image:', err);
                reject(new Error('Failed to load SVG image'));
            };
            
            // Use data URI instead of blob URL to avoid CORS issues
            const svgBase64 = btoa(unescape(encodeURIComponent(svgString)));
            img.src = 'data:image/svg+xml;base64,' + svgBase64;
            console.log('Image src set, waiting for load...');
            
        } catch (error) {
            reject(error);
        }
    });
};

// PDF Generation using jsPDF
window.generatePdfFromHtml = async function(html, fileName) {
    try {
        // Wait for jsPDF to be loaded
        if (typeof jspdf === 'undefined' || typeof jspdf.jsPDF === 'undefined') {
            throw new Error('jsPDF library not loaded');
        }

        const { jsPDF } = jspdf;
        const doc = new jsPDF({
            orientation: 'portrait',
            unit: 'mm',
            format: 'a4'
        });

        // Parse HTML and add to PDF
        const tempDiv = document.createElement('div');
        tempDiv.innerHTML = html;
        tempDiv.style.width = '190mm'; // A4 width minus margins
        tempDiv.style.padding = '10mm';
        tempDiv.style.fontFamily = 'Arial, sans-serif';
        tempDiv.style.fontSize = '11pt';
        tempDiv.style.lineHeight = '1.5';
        tempDiv.style.backgroundColor = 'white';
        
        // Temporarily add to document for rendering
        tempDiv.style.position = 'absolute';
        tempDiv.style.left = '-9999px';
        tempDiv.style.top = '0';
        document.body.appendChild(tempDiv);

        try {
            // Use html2canvas if available for better rendering
            if (typeof html2canvas !== 'undefined') {
                // Wait a bit for images to render
                await new Promise(resolve => setTimeout(resolve, 500));
                
                const canvas = await html2canvas(tempDiv, {
                    scale: 2,
                    useCORS: true,
                    allowTaint: true,
                    logging: false,
                    backgroundColor: '#ffffff',
                    onclone: function(clonedDoc) {
                        // Ensure SVGs are visible in cloned document
                        const svgs = clonedDoc.querySelectorAll('svg');
                        svgs.forEach(svg => {
                            svg.style.display = 'block';
                        });
                    }
                });
                
                const imgData = canvas.toDataURL('image/png');
                const imgWidth = 190; // A4 width minus margins
                const imgHeight = (canvas.height * imgWidth) / canvas.width;
                
                let heightLeft = imgHeight;
                let position = 10;

                doc.addImage(imgData, 'PNG', 10, position, imgWidth, imgHeight);
                heightLeft -= 277; // A4 height minus margins

                while (heightLeft > 0) {
                    position = heightLeft - imgHeight + 10;
                    doc.addPage();
                    doc.addImage(imgData, 'PNG', 10, position, imgWidth, imgHeight);
                    heightLeft -= 277;
                }
            } else {
                // Fallback: Simple text extraction
                await addContentToPdf(doc, tempDiv);
            }
        } finally {
            document.body.removeChild(tempDiv);
        }

        // Get PDF as byte array
        const pdfBlob = doc.output('blob');
        const arrayBuffer = await pdfBlob.arrayBuffer();
        const uint8Array = new Uint8Array(arrayBuffer);
        
        // Convert to regular array for .NET
        return Array.from(uint8Array);
    } catch (error) {
        console.error('Error generating PDF:', error);
        throw error;
    }
};

// Helper function to add content to PDF (fallback method)
async function addContentToPdf(doc, element) {
    let yPosition = 20;
    const pageHeight = 277; // A4 height minus margins
    const lineHeight = 7;
    const margin = 10;
    const maxWidth = 190;

    function addText(text, fontSize, isBold, indent = 0) {
        if (yPosition > pageHeight - 20) {
            doc.addPage();
            yPosition = 20;
        }

        doc.setFontSize(fontSize);
        doc.setFont('helvetica', isBold ? 'bold' : 'normal');
        
        const lines = doc.splitTextToSize(text, maxWidth - indent);
        lines.forEach(line => {
            if (yPosition > pageHeight - 20) {
                doc.addPage();
                yPosition = 20;
            }
            doc.text(line, margin + indent, yPosition);
            yPosition += lineHeight;
        });
    }

    function processNode(node, indent = 0) {
        const nodeName = node.nodeName.toLowerCase();

        switch(nodeName) {
            case 'h1':
                yPosition += 5;
                addText(node.textContent, 20, true);
                yPosition += 3;
                break;
            case 'h2':
                yPosition += 4;
                addText(node.textContent, 16, true);
                yPosition += 2;
                break;
            case 'h3':
                yPosition += 3;
                addText(node.textContent, 14, true);
                yPosition += 2;
                break;
            case 'h4':
            case 'h5':
            case 'h6':
                yPosition += 2;
                addText(node.textContent, 12, true);
                yPosition += 1;
                break;
            case 'p':
                if (node.textContent.trim()) {
                    addText(node.textContent, 11, false, indent);
                    yPosition += 2;
                }
                break;
            case 'li':
                addText('• ' + node.textContent, 11, false, indent + 5);
                break;
            case 'code':
            case 'pre':
                doc.setFillColor(240, 240, 240);
                doc.rect(margin, yPosition - 3, maxWidth, lineHeight, 'F');
                addText(node.textContent, 9, false);
                break;
            case 'img':
                addText('[Diagram]', 11, false, indent);
                yPosition += 2;
                break;
            case 'table':
                addText('[Table content]', 11, false, indent);
                yPosition += 2;
                break;
            default:
                if (node.childNodes && node.childNodes.length > 0) {
                    node.childNodes.forEach(child => {
                        if (child.nodeType === 1) { // Element node
                            processNode(child, indent);
                        }
                    });
                }
                break;
        }
    }

    processNode(element);
}

// Clean up SVG to be compatible with Word documents and PNG conversion
window.cleanupSvgForWordCompatibility = function(svg) {
    console.log('Cleaning SVG for Word compatibility...');
    
    let replacementCount = 0;
    let cleaned = svg;
    
    // Helper function to extract text from HTML content
    function extractTextFromContent(content) {
        let text = '';
        const pMatch = content.match(/<p[^>]*>([^<]+)/);
        const spanMatch = content.match(/<span[^>]*>([^<]+)/);
        const textMatch = content.match(/>([^<]+)</);
        
        if (pMatch) {
            text = pMatch[1].trim();
        } else if (spanMatch) {
            text = spanMatch[1].trim();
        } else if (textMatch) {
            text = textMatch[1].trim();
        }
        
        return text;
    }
    
    // Helper function for direct foreignObjects (fallback)
    function handleDirectForeignObject(match, content) {
        console.log('Handling direct foreignObject...');
        const text = extractTextFromContent(content);
        if (!text) return '';
        
        // If no position info, place at origin (better than nothing)
        return `<text x="50" y="50" text-anchor="middle" dominant-baseline="central" font-family="Arial, sans-serif" font-size="12" fill="#000000">${text}</text>`;
    }
    
    // Find all g elements that contain foreignObject elements and extract transforms
    cleaned = cleaned.replace(/<g[^>]*transform="([^"]*)"[^>]*>([\s\S]*?)<\/g>/gs, 
        (match, transform, groupContent) => {
            // Check if this group contains a foreignObject
            if (!groupContent.includes('<foreignObject')) {
                return match; // Return unchanged if no foreignObject
            }
            
            console.log('Found g with foreignObject:', match.substring(0, 150) + '...');
            console.log('Transform:', transform);
            
            // Extract translate values from transform
            const translateMatch = transform.match(/translate\(([^,)]+),?\s*([^)]*)\)/);
            if (!translateMatch) {
                console.log('No translate found in transform, trying direct foreignObject...');
                return handleDirectForeignObject(match, groupContent);
            }
            
            const translateX = parseFloat(translateMatch[1]) || 0;
            const translateY = parseFloat(translateMatch[2]) || 0;
            
            // Extract foreignObject content
            const foreignObjectMatch = groupContent.match(/<foreignObject[^>]*>(.*?)<\/foreignObject>/s);
            if (!foreignObjectMatch) {
                return match; // Return unchanged if no foreignObject found
            }
            
            const content = foreignObjectMatch[1];
            const text = extractTextFromContent(content);
            console.log('Extracted text:', text);
            
            if (!text || text === '') {
                console.log('No text found, removing group');
                return '';
            }
            
            // For transformed groups, use the translate position
            console.log(`Converting to text at (${translateX}, ${translateY}): "${text}"`);
            replacementCount++;
            
            // Preserve other elements in the group and add the text element
            let otherElements = groupContent.replace(/<foreignObject[^>]*>.*?<\/foreignObject>/s, '');
            
            // Create a text element at the transform position
            const textElement = `<text x="${translateX}" y="${translateY}" text-anchor="middle" dominant-baseline="central" font-family="Arial, sans-serif" font-size="12" fill="#000000">${text}</text>`;
            
            // If there are other elements, preserve the group structure
            if (otherElements.trim()) {
                return `<g transform="${transform}">${otherElements}${textElement}</g>`;
            } else {
                return textElement;
            }
        }
    );
    
    // Clean up any remaining standalone foreignObject elements
    cleaned = cleaned.replace(/<foreignObject[^>]*>.*?<\/foreignObject>/gs, '');
    
    // Ensure proper XML namespace and structure
    if (!cleaned.includes('xmlns="http://www.w3.org/2000/svg"')) {
        cleaned = cleaned.replace('<svg', '<svg xmlns="http://www.w3.org/2000/svg"');
    }
    
    console.log(`Foreign objects converted to text elements: ${replacementCount} replacements made`);
    return cleaned;
};

// Render Mermaid diagram and return original SVG (no cleanup) for export
window.renderMermaidOriginal = async function(code, elementId) {
    try {
        // Wait for Mermaid to be available
        const mermaid = await window.waitForMermaid();
        
        console.log('Rendering original Mermaid diagram for export:', elementId);
        console.log('Code:', code);
        
        // Generate unique ID for this rendering
        const uniqueId = 'mermaid-original-' + elementId;
        
        // Use mermaid.render() to generate SVG
        const { svg } = await mermaid.render(uniqueId, code);
        
        console.log('Original SVG rendered successfully, length:', svg.length);
        
        // Return the original SVG without any modifications
        // This preserves all the original Mermaid styling, colors, fonts, etc.
        return svg;
        
    } catch (error) {
        console.error('Error rendering original Mermaid diagram:', error);
        throw new Error(`Failed to render Mermaid diagram: ${error.message}`);
    }
};
