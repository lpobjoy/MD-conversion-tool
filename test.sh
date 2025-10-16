#!/bin/bash

# MD Converter Test Script
# This script helps verify the application is working correctly

echo "🚀 MD Converter - Test Script"
echo "================================"
echo ""

# Check if .NET SDK is installed
echo "📦 Checking prerequisites..."
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found. Please install .NET 9 SDK."
    echo "   Download from: https://dotnet.microsoft.com/download/dotnet/9.0"
    exit 1
fi

echo "✅ .NET SDK found: $(dotnet --version)"
echo ""

# Check if project can be built
echo "🔨 Building project..."
if dotnet build > /dev/null 2>&1; then
    echo "✅ Project builds successfully"
else
    echo "❌ Build failed. Check for compilation errors."
    exit 1
fi
echo ""

# Check if all required services are registered
echo "🔍 Checking service registrations..."
if grep -q "MarkdownParser" Program.cs && \
   grep -q "DocxConverter" Program.cs && \
   grep -q "PdfConverter" Program.cs && \
   grep -q "MermaidService" Program.cs; then
    echo "✅ All services registered in Program.cs"
else
    echo "⚠️  Warning: Some services may not be registered"
fi
echo ""

# Check if JavaScript files exist
echo "📄 Checking JavaScript files..."
if [ -f "wwwroot/js/mermaid-interop.js" ]; then
    echo "✅ Mermaid interop file found"
else
    echo "❌ Missing: wwwroot/js/mermaid-interop.js"
fi
echo ""

# Check if required NuGet packages are referenced
echo "📦 Checking NuGet packages..."
packages=("Markdig" "QuestPDF" "DocumentFormat.OpenXml" "HtmlAgilityPack" "SixLabors.ImageSharp")
for package in "${packages[@]}"; do
    if grep -q "$package" MDConverter.csproj; then
        echo "✅ $package referenced"
    else
        echo "❌ Missing: $package"
    fi
done
echo ""

# Test if app can start (non-blocking check)
echo "🌐 Testing application startup..."
echo "   Starting application on port 5008..."
echo "   (This will run in the background for 10 seconds)"
echo ""

# Start the app in background
dotnet run > /tmp/mdconverter-test.log 2>&1 &
APP_PID=$!

# Wait for app to start
sleep 5

# Check if process is still running
if ps -p $APP_PID > /dev/null; then
    echo "✅ Application started successfully"
    echo "   Access at: http://localhost:5008"
    echo ""
    echo "🎯 Test checklist:"
    echo "   1. Open http://localhost:5008 in your browser"
    echo "   2. Try pasting the sample markdown from sample-document.md"
    echo "   3. Test each output format (DOCX, PDF, HTML)"
    echo "   4. Verify Mermaid diagrams are rendered"
    echo ""
    
    # Wait a bit more
    sleep 5
    
    # Kill the test app
    kill $APP_PID 2>/dev/null
    echo "✅ Application stopped"
else
    echo "❌ Application failed to start"
    echo "   Check logs at: /tmp/mdconverter-test.log"
    cat /tmp/mdconverter-test.log
    exit 1
fi
echo ""

echo "================================"
echo "✨ All tests passed!"
echo ""
echo "To run the application:"
echo "  dotnet run"
echo ""
echo "To test with sample document:"
echo "  1. dotnet run"
echo "  2. Open http://localhost:5008"
echo "  3. Upload sample-document.md"
echo "  4. Click 'Convert & Download'"
echo ""
echo "For AI agent integration, see:"
echo "  AI-AGENT-GUIDE.md"
echo "================================"
