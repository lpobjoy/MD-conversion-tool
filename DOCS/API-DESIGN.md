# MD Converter Public API Design

## Overview
Design a RESTful API that AI agents can use to convert Markdown files to various formats (DOCX, PDF, HTML, PPTX, Beamer, Reveal.js).

## Current Architecture Challenge
**Problem**: The current application is 100% client-side Blazor WebAssembly
- All processing happens in the browser
- Pandoc WASM (50MB) runs client-side
- No backend server exists
- Privacy-focused: no data sent to servers

## API Architecture Options

### Option 1: Add ASP.NET Core Backend API (Recommended)
Create a separate API project that can be deployed alongside or independently from the Blazor frontend.

**Architecture**:
```
MD-conversion-tool/
├── MDConverter/              # Existing Blazor WASM project
├── MDConverter.Api/          # NEW - ASP.NET Core Web API
│   ├── Controllers/
│   │   └── ConversionController.cs
│   ├── Services/
│   │   ├── PandocNativeService.cs  # Uses native Pandoc CLI
│   │   └── MermaidRenderer.cs      # Server-side Mermaid rendering
│   └── Program.cs
└── docker-compose.yml        # Optional: containerized deployment
```

**Benefits**:
- ✅ Use native Pandoc (faster, more features)
- ✅ Server-side Mermaid rendering with Puppeteer
- ✅ API can be hosted separately (Azure App Service, AWS Lambda, etc.)
- ✅ Rate limiting, authentication, caching possible
- ✅ Swagger/OpenAPI documentation auto-generated

**Drawbacks**:
- ❌ Requires server infrastructure
- ❌ Data sent to server (privacy trade-off)
- ❌ Additional hosting costs

### Option 2: Serverless Functions (Azure Functions / AWS Lambda)
Expose conversion as serverless endpoints.

**Architecture**:
```
FunctionApp/
├── ConvertMarkdown/
│   ├── function.json
│   └── run.csx
└── host.json
```

**Benefits**:
- ✅ Pay-per-use pricing
- ✅ Auto-scaling
- ✅ Easy deployment

**Drawbacks**:
- ❌ Cold start times
- ❌ Limited execution time (5-10 min)
- ❌ Pandoc needs to be included in deployment

### Option 3: API Gateway to Existing UI (Hybrid)
Keep Blazor WASM as-is, add minimal API layer that returns a URL to the conversion UI with pre-filled markdown.

**Not recommended** - doesn't solve the agent automation need.

## Recommended Approach: ASP.NET Core Web API

### API Endpoints Design

#### 1. Health Check
```http
GET /api/health
```
Response:
```json
{
  "status": "healthy",
  "version": "1.0.0",
  "pandocVersion": "3.1.8"
}
```

#### 2. Convert Markdown
```http
POST /api/convert
Content-Type: application/json
```

Request Body:
```json
{
  "markdown": "# Your Markdown\n\n```mermaid\ngraph TD\nA-->B\n```",
  "outputFormat": "docx",  // "docx" | "pdf" | "html" | "pptx" | "beamer" | "revealjs"
  "options": {
    "renderMermaid": true,  // default: true
    "theme": "default",     // mermaid theme
    "includeMetadata": true,
    "filename": "document"
  }
}
```

Response (Binary):
```http
HTTP/1.1 200 OK
Content-Type: application/vnd.openxmlformats-officedocument.wordprocessingml.document
Content-Disposition: attachment; filename="document.docx"
X-Conversion-Time-Ms: 1234

[Binary DOCX data]
```

Response (Error):
```json
{
  "error": "ConversionFailed",
  "message": "Failed to render Mermaid diagram",
  "details": "Invalid mermaid syntax at line 5"
}
```

#### 3. Batch Conversion
```http
POST /api/convert/batch
Content-Type: application/json
```

Request:
```json
{
  "conversions": [
    {
      "id": "doc1",
      "markdown": "# Document 1",
      "outputFormat": "docx"
    },
    {
      "id": "doc2",
      "markdown": "# Document 2",
      "outputFormat": "pptx"
    }
  ]
}
```

Response:
```json
{
  "results": [
    {
      "id": "doc1",
      "status": "success",
      "downloadUrl": "https://api.mdconverter.com/downloads/abc123.docx",
      "expiresAt": "2025-10-16T12:00:00Z"
    },
    {
      "id": "doc2",
      "status": "failed",
      "error": "Invalid mermaid syntax"
    }
  ]
}
```

#### 4. Get Supported Formats
```http
GET /api/formats
```

Response:
```json
{
  "documents": [
    {
      "format": "docx",
      "name": "Microsoft Word",
      "mimeType": "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
      "extension": ".docx",
      "supportsMermaid": true
    },
    {
      "format": "pdf",
      "name": "PDF Document",
      "mimeType": "application/pdf",
      "extension": ".pdf",
      "supportsMermaid": true
    }
  ],
  "presentations": [
    {
      "format": "pptx",
      "name": "PowerPoint Presentation",
      "mimeType": "application/vnd.openxmlformats-officedocument.presentationml.presentation",
      "extension": ".pptx",
      "supportsMermaid": true
    }
  ]
}
```

### Authentication Options

#### Option A: API Key (Simple)
```http
POST /api/convert
Authorization: Bearer YOUR_API_KEY
Content-Type: application/json
```

#### Option B: OAuth 2.0 (Enterprise)
```http
POST /api/convert
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
Content-Type: application/json
```

#### Option C: No Auth (Public, Rate-Limited)
```http
POST /api/convert
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1634385600
```

### Implementation Stack

**Backend**:
- ASP.NET Core 9 Web API
- Pandoc CLI (installed on server)
- Puppeteer for Mermaid rendering
- Redis for caching (optional)

**Deployment Options**:
1. **Azure App Service** - Easy, managed
2. **Docker Container** - Portable, any cloud
3. **Azure Container Apps** - Serverless containers
4. **AWS Elastic Beanstalk** - AWS equivalent

### Cost Estimation (Azure App Service)

- **Basic B1**: ~$13/month (1 core, 1.75GB RAM)
- **Standard S1**: ~$70/month (1 core, 1.75GB RAM, better SLA)
- **Premium P1v3**: ~$150/month (2 cores, 8GB RAM, auto-scale)

### Security Considerations

1. **Input Validation**:
   - Max markdown size (e.g., 5MB)
   - Sanitize file paths
   - Rate limiting per API key

2. **Resource Limits**:
   - Timeout conversions after 30s
   - Max concurrent conversions per user
   - Memory limits

3. **Output Safety**:
   - Scan for malicious content
   - Temporary file cleanup
   - Secure file storage

### Agent Integration Example

**OpenAI Function Definition**:
```json
{
  "name": "convert_markdown_document",
  "description": "Convert markdown content to DOCX, PDF, PowerPoint or other formats. Supports Mermaid diagrams.",
  "parameters": {
    "type": "object",
    "properties": {
      "markdown": {
        "type": "string",
        "description": "The markdown content to convert"
      },
      "outputFormat": {
        "type": "string",
        "enum": ["docx", "pdf", "html", "pptx", "beamer", "revealjs"],
        "description": "The desired output format"
      },
      "filename": {
        "type": "string",
        "description": "Optional filename for the output document"
      }
    },
    "required": ["markdown", "outputFormat"]
  }
}
```

**Usage in Agent**:
```python
import requests

response = requests.post(
    "https://api.mdconverter.com/api/convert",
    headers={
        "Authorization": "Bearer YOUR_API_KEY",
        "Content-Type": "application/json"
    },
    json={
        "markdown": "# My Document\n\n```mermaid\ngraph TD\nA-->B\n```",
        "outputFormat": "docx",
        "options": {
            "renderMermaid": True,
            "filename": "report"
        }
    }
)

if response.status_code == 200:
    with open("report.docx", "wb") as f:
        f.write(response.content)
```

## Next Steps

1. **Phase 1**: Create MDConverter.Api project
2. **Phase 2**: Implement core conversion endpoints
3. **Phase 3**: Add Mermaid rendering with Puppeteer
4. **Phase 4**: Deploy to Azure App Service
5. **Phase 5**: Add authentication & rate limiting
6. **Phase 6**: Create Swagger documentation
7. **Phase 7**: Publish agent integration guide

## Questions to Consider

1. **Pricing Model**: Free tier + paid plans?
2. **Data Retention**: How long to keep converted files?
3. **Logging**: What metrics to track?
4. **SLA**: Uptime guarantees?
5. **Support**: Community vs. enterprise support?
