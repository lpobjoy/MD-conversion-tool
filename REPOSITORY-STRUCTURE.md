# MD Converter Repository Structure

```
MD-conversion-tool/
├── 📄 README.md                        # Project documentation
├── 📄 API-DESIGN.md                    # API specification for agents
├── 📄 SESSION-SUMMARY.md               # Latest session summary
├── 📄 PRESENTATION-FORMATS.md          # Presentation format guide
├── 📄 SVG-HANDLING.md                  # SVG processing documentation
├── 📄 PANDOC_WASM_STATUS.md            # Pandoc WASM integration notes
├── 📄 MDConverter.sln                  # Solution file
├── 📄 MDConverter.csproj               # Blazor WASM project file
├── 📄 Program.cs                       # App entry point
├── 📄 .gitignore                       # Git ignore rules
│
├── 📁 Layout/                          # Layout components
│   ├── MainLayout.razor                # Root layout with navbar & dropdown
│   ├── MainLayout.razor.css            # Layout styles
│   └── NavMenu.razor                   # Navigation menu (deprecated)
│
├── 📁 Pages/                           # Blazor pages
│   ├── Home.razor                      # Main converter UI
│   ├── SvgExtractor.razor              # Diagram extractor tool
│   └── Error.razor                     # Error page
│
├── 📁 Services/                        # Business logic services
│   ├── AppState.cs                     # Cross-component communication
│   ├── PandocService.cs                # Pandoc WASM C# wrapper
│   ├── SvgHandler.cs                   # SVG extraction & conversion
│   ├── MarkdownParser.cs               # Markdown parsing & Mermaid extraction
│   ├── DocxConverter.cs                # Custom DOCX generator (fallback)
│   ├── PdfConverter.cs                 # PDF generation
│   └── MermaidService.cs               # Mermaid diagram rendering
│
├── 📁 Models/                          # Data models
│   ├── MermaidDiagram.cs               # Diagram data structure
│   ├── ConversionResult.cs             # Conversion output model
│   └── ExportFormat.cs                 # Format enumeration
│
├── 📁 wwwroot/                         # Static files
│   ├── index.html                      # Main HTML entry
│   ├── favicon.png                     # Site icon
│   ├── pandoc.wasm                     # Pandoc WebAssembly binary (50.4MB)
│   │
│   ├── 📁 css/
│   │   └── app.css                     # Custom styles (dropdown, navbar)
│   │
│   ├── 📁 js/
│   │   ├── mermaid-interop.js          # Mermaid rendering & SVG→PNG
│   │   └── pandoc-interop.js           # Pandoc WASM JavaScript bridge
│   │
│   └── 📁 lib/                         # Third-party libraries
│       └── bootstrap/                  # Bootstrap 5 CSS
│
├── 📁 Properties/                      # Build properties
│   └── launchSettings.json             # Dev server settings
│
└── 📁 MDConverter.Api/                 # 🆕 API Project (NEW!)
    ├── MDConverter.Api.csproj          # API project file
    ├── Program.cs                      # API entry point
    ├── appsettings.json                # Configuration
    ├── appsettings.Development.json    # Dev configuration
    │
    ├── 📁 Controllers/                 # API endpoints (TO BE CREATED)
    │   └── ConversionController.cs     # Main conversion endpoint
    │
    ├── 📁 Services/                    # API services (TO BE CREATED)
    │   ├── PandocNativeService.cs      # Native Pandoc CLI wrapper
    │   └── MermaidRenderer.cs          # Server-side Mermaid rendering
    │
    └── 📁 Models/                      # API models (TO BE CREATED)
        ├── ConversionRequest.cs        # API request model
        └── ConversionResponse.cs       # API response model
```

## Key Files Explained

### Frontend (Blazor WASM)

**Pages/Home.razor** (1,157 lines)
- Main converter UI with file upload, format selection, conversion
- Handles both diagram and no-diagram markdown paths
- Integrates About modal, Pandoc WASM, Mermaid rendering
- Supports 6 output formats: DOCX, PDF, HTML, PPTX, Beamer, Reveal.js

**Services/PandocService.cs** (188 lines)
- C# wrapper for Pandoc WASM JavaScript functions
- Methods: `ConvertMarkdownToDocxAsync`, `ConvertMarkdownToPdfAsync`, etc.
- Handles initialization and error reporting

**Services/SvgHandler.cs** (150 lines)
- Extracts SVG references from markdown
- Converts SVG to PNG for DOCX/PDF/PPTX/Beamer
- Keeps SVG for HTML/Reveal.js (better quality)

**wwwroot/js/pandoc-interop.js** (326 lines)
- JavaScript bridge to Pandoc WASM
- Uses `@bjorn3/browser_wasi_shim` for WASI filesystem
- Exports: `convertMarkdownToDocx`, `convertMarkdownToPptx`, etc.

**wwwroot/pandoc.wasm** (50.44 MB)
- Complete Pandoc compiled to WebAssembly
- Supports all Pandoc output formats
- Includes Haskell runtime

### Backend API (To Be Implemented)

**MDConverter.Api/Controllers/ConversionController.cs** (FUTURE)
```csharp
[ApiController]
[Route("api/[controller]")]
public class ConversionController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Convert([FromBody] ConversionRequest request)
    {
        // 1. Validate markdown input
        // 2. Render Mermaid diagrams
        // 3. Call Pandoc (native or WASM)
        // 4. Return binary file
    }
}
```

## Technology Stack

### Frontend
- **Framework**: Blazor WebAssembly (.NET 9)
- **Document Conversion**: Pandoc WASM (3.1.8)
- **Diagram Rendering**: Mermaid.js 11 (ESM)
- **SVG Conversion**: html2canvas 1.4.1
- **Markdown Parsing**: Markdig
- **DOCX Generation**: DocumentFormat.OpenXml 3.3.0
- **UI**: Bootstrap 5, Bootstrap Icons

### Backend API (Planned)
- **Framework**: ASP.NET Core 9 Web API
- **Document Conversion**: Native Pandoc CLI (faster)
- **Diagram Rendering**: Puppeteer (headless Chrome)
- **Authentication**: API Keys or OAuth 2.0
- **Rate Limiting**: AspNetCoreRateLimit
- **Documentation**: Swashbuckle (Swagger/OpenAPI)

### Deployment Options
1. **Azure App Service** - Managed PaaS
2. **Docker Container** - Portable, any cloud
3. **Azure Container Apps** - Serverless containers
4. **AWS Elastic Beanstalk** - AWS managed PaaS

## File Sizes

Large files to be aware of:
- `wwwroot/pandoc.wasm` - 50.44 MB (WebAssembly binary)
- `wwwroot/lib/bootstrap/` - ~1 MB (CSS framework)
- Total project size: ~53 MB

## Port Configuration

- **Blazor WASM Dev**: http://localhost:5008
- **API Dev** (planned): http://localhost:5000

## Git Status

- **Repository**: https://github.com/lpobjoy/MD-conversion-tool
- **Branch**: main
- **Latest Commit**: e672410 - "Add presentation formats..."
- **Status**: ✅ All changes pushed to GitHub

## Recent Changes (Commit e672410)

### Added (8 files)
1. PANDOC_WASM_STATUS.md
2. PRESENTATION-FORMATS.md
3. SVG-HANDLING.md
4. Services/AppState.cs
5. Services/PandocService.cs
6. Services/SvgHandler.cs
7. wwwroot/js/pandoc-interop.js
8. wwwroot/pandoc.wasm

### Modified (8 files)
1. Pages/Home.razor - Presentation formats, bug fixes
2. Layout/MainLayout.razor - Navbar, dropdown menu
3. Layout/NavMenu.razor - Cleaned up
4. Program.cs - Registered new services
5. README.md - Updated documentation
6. wwwroot/css/app.css - Dropdown styling
7. wwwroot/index.html - Updated imports
8. wwwroot/js/mermaid-interop.js - Print dialog support

## What's Next?

To implement the API for AI agents, you need to:

1. **Decision Point**: Choose hosting platform and authentication strategy
2. **Implement**: ConversionController with endpoints
3. **Install**: Pandoc on server or use WASM
4. **Add**: Puppeteer for Mermaid rendering
5. **Deploy**: To chosen cloud platform
6. **Document**: Create Swagger/OpenAPI docs
7. **Test**: With AI agent integration

See `API-DESIGN.md` for complete specification!
