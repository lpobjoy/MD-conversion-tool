using Microsoft.JSInterop;
using System.Text.Json;

namespace MDConverter.Services;

public class PandocService
{
    private readonly IJSRuntime _jsRuntime;
    private bool _initialized = false;

    public PandocService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<bool> InitializeAsync()
    {
        if (_initialized)
            return true;

        try
        {
            Console.WriteLine("Initializing Pandoc WASM service...");
            _initialized = await _jsRuntime.InvokeAsync<bool>("pandocInterop.initializePandoc");
            
            if (_initialized)
            {
                Console.WriteLine("✅ Pandoc WASM service initialized successfully");
            }
            else
            {
                Console.WriteLine("❌ Pandoc WASM service initialization failed");
            }
            
            return _initialized;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error initializing Pandoc WASM: {ex.Message}");
            return false;
        }
    }

    public async Task<byte[]?> ConvertMarkdownToDocxAsync(string markdownContent)
    {
        if (!_initialized)
        {
            var success = await InitializeAsync();
            if (!success)
            {
                throw new InvalidOperationException("Pandoc WASM is not initialized");
            }
        }

        try
        {
            Console.WriteLine($"Converting markdown to DOCX with Pandoc WASM ({markdownContent.Length} chars)...");
            
            // Call JavaScript to convert using Pandoc WASM
            var result = await _jsRuntime.InvokeAsync<byte[]>("pandocInterop.convertMarkdownToDocx", markdownContent);
            
            Console.WriteLine($"✅ Conversion successful, output size: {result?.Length ?? 0} bytes");
            
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Pandoc conversion failed: {ex.Message}");
            throw;
        }
    }

    public async Task<byte[]?> ConvertMarkdownToPdfAsync(string markdownContent)
    {
        if (!_initialized)
        {
            var success = await InitializeAsync();
            if (!success)
            {
                throw new InvalidOperationException("Pandoc WASM is not initialized");
            }
        }

        try
        {
            Console.WriteLine($"Converting markdown to PDF with Pandoc WASM ({markdownContent.Length} chars)...");
            
            // Call JavaScript to convert using Pandoc WASM
            var result = await _jsRuntime.InvokeAsync<byte[]>("pandocInterop.convertMarkdownToPdf", markdownContent);
            
            Console.WriteLine($"✅ Conversion successful, output size: {result?.Length ?? 0} bytes");
            
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Pandoc PDF conversion failed: {ex.Message}");
            throw;
        }
    }

    public async Task<byte[]?> ConvertMarkdownToPptxAsync(string markdownContent)
    {
        if (!_initialized)
        {
            var success = await InitializeAsync();
            if (!success)
            {
                throw new InvalidOperationException("Pandoc WASM is not initialized");
            }
        }

        try
        {
            Console.WriteLine($"Converting markdown to PPTX with Pandoc WASM ({markdownContent.Length} chars)...");
            
            // Call JavaScript to convert using Pandoc WASM
            var result = await _jsRuntime.InvokeAsync<byte[]>("pandocInterop.convertMarkdownToPptx", markdownContent);
            
            Console.WriteLine($"✅ Conversion successful, output size: {result?.Length ?? 0} bytes");
            
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Pandoc PPTX conversion failed: {ex.Message}");
            throw;
        }
    }

    public async Task<byte[]?> ConvertMarkdownToBeamerAsync(string markdownContent)
    {
        if (!_initialized)
        {
            var success = await InitializeAsync();
            if (!success)
            {
                throw new InvalidOperationException("Pandoc WASM is not initialized");
            }
        }

        try
        {
            Console.WriteLine($"Converting markdown to Beamer with Pandoc WASM ({markdownContent.Length} chars)...");
            
            // Call JavaScript to convert using Pandoc WASM
            var result = await _jsRuntime.InvokeAsync<byte[]>("pandocInterop.convertMarkdownToBeamer", markdownContent);
            
            Console.WriteLine($"✅ Conversion successful, output size: {result?.Length ?? 0} bytes");
            
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Pandoc Beamer conversion failed: {ex.Message}");
            throw;
        }
    }

    public async Task<byte[]?> ConvertMarkdownToRevealJsAsync(string markdownContent)
    {
        if (!_initialized)
        {
            var success = await InitializeAsync();
            if (!success)
            {
                throw new InvalidOperationException("Pandoc WASM is not initialized");
            }
        }

        try
        {
            Console.WriteLine($"Converting markdown to Reveal.js with Pandoc WASM ({markdownContent.Length} chars)...");
            
            // Call JavaScript to convert using Pandoc WASM
            var result = await _jsRuntime.InvokeAsync<byte[]>("pandocInterop.convertMarkdownToRevealJs", markdownContent);
            
            Console.WriteLine($"✅ Conversion successful, output size: {result?.Length ?? 0} bytes");
            
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Pandoc Reveal.js conversion failed: {ex.Message}");
            throw;
        }
    }
}
