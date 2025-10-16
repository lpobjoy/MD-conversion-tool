// Pandoc WASM interop - Based on the official pandoc-wasm demo
// https://github.com/tweag/pandoc-wasm

import {
    WASI,
    OpenFile,
    File,
    ConsoleStdout,
    PreopenDirectory,
} from "https://cdn.jsdelivr.net/npm/@bjorn3/browser_wasi_shim@0.3.0/dist/index.js";

let pandocInstance = null;
let inFile = null;
let outFile = null;

// Initialize Pandoc WASM module
export async function initializePandoc() {
    try {
        console.log('🚀 Initializing Pandoc WASM...');
        
        // Setup WASI filesystem with input/output files
        const args = ["pandoc.wasm", "+RTS", "-H64m", "-RTS"];
        const env = [];
        
        inFile = new File(new Uint8Array());
        outFile = new File(new Uint8Array());
        
        const fds = [
            new OpenFile(new File(new Uint8Array())), // stdin
            ConsoleStdout.lineBuffered((msg) => console.log(`[Pandoc stdout] ${msg}`)),
            ConsoleStdout.lineBuffered((msg) => console.warn(`[Pandoc stderr] ${msg}`)),
            new PreopenDirectory("/", [
                ["in", inFile],
                ["out", outFile],
            ]),
        ];
        
        const options = { debug: false };
        const wasi = new WASI(args, env, fds, options);
        
        console.log('📦 Loading pandoc.wasm module...');
        // Use document.baseURI to get the correct base path for GitHub Pages
        const baseHref = document.querySelector('base')?.getAttribute('href') || '/';
        const pandocUrl = new URL('pandoc.wasm', new URL(baseHref, window.location.href)).href;
        console.log('📍 Pandoc URL:', pandocUrl);
        
        console.log('🌐 Fetching pandoc.wasm...');
        const response = await fetch(pandocUrl);
        if (!response.ok) {
            throw new Error(`Failed to fetch pandoc.wasm: ${response.status} ${response.statusText}`);
        }
        console.log('✅ Fetch successful, size:', response.headers.get('content-length'), 'bytes');
        
        console.log('⚙️ Instantiating WebAssembly module...');
        const { instance } = await WebAssembly.instantiateStreaming(
            response,
            {
                wasi_snapshot_preview1: wasi.wasiImport,
            }
        );
        console.log('✅ WebAssembly instantiation complete');
        
        console.log('🔧 Initializing WASI and Haskell runtime...');
        wasi.initialize(instance);
        instance.exports.__wasm_call_ctors();
        
        // Initialize Haskell runtime
        function memory_data_view() {
            return new DataView(instance.exports.memory.buffer);
        }
        
        const argc_ptr = instance.exports.malloc(4);
        memory_data_view().setUint32(argc_ptr, args.length, true);
        const argv = instance.exports.malloc(4 * (args.length + 1));
        
        for (let i = 0; i < args.length; ++i) {
            const arg = instance.exports.malloc(args[i].length + 1);
            new TextEncoder().encodeInto(
                args[i],
                new Uint8Array(instance.exports.memory.buffer, arg, args[i].length)
            );
            memory_data_view().setUint8(arg + args[i].length, 0);
            memory_data_view().setUint32(argv + 4 * i, arg, true);
        }
        memory_data_view().setUint32(argv + 4 * args.length, 0, true);
        
        const argv_ptr = instance.exports.malloc(4);
        memory_data_view().setUint32(argv_ptr, argv, true);
        
        instance.exports.hs_init_with_rtsopts(argc_ptr, argv_ptr);
        
        pandocInstance = instance;
        
        console.log('✅ Pandoc WASM ready!');
        return true;
    } catch (error) {
        console.error('❌ Failed to initialize Pandoc WASM:', error);
        return false;
    }
}

// Run pandoc with arguments
function pandoc(args_array) {
    if (!pandocInstance) {
        throw new Error('Pandoc not initialized');
    }
    
    // Concatenate arguments into a single string separated by spaces
    const args_str = args_array.join(' ');
    
    const args_ptr = pandocInstance.exports.malloc(args_str.length);
    new TextEncoder().encodeInto(
        args_str,
        new Uint8Array(pandocInstance.exports.memory.buffer, args_ptr, args_str.length)
    );
    
    pandocInstance.exports.wasm_main(args_ptr, args_str.length);
}

// Convert markdown to DOCX using Pandoc WASM
export async function convertMarkdownToDocx(markdownContent) {
    if (!pandocInstance) {
        throw new Error('Pandoc WASM not initialized. Call initializePandoc() first.');
    }
    
    try {
        console.log('📝 Converting markdown to DOCX with Pandoc WASM...');
        console.log('Input length:', markdownContent.length, 'characters');
        
        // Write markdown content to the input file
        inFile.data = new TextEncoder().encode(markdownContent);
        console.log('✅ Input file written:', inFile.data.length, 'bytes');
        
        // Clear the output file before running
        outFile.data = new Uint8Array();
        
        // Run pandoc: read from /in, write DOCX to /out
        const args = ["/in", "-f", "markdown", "-t", "docx", "-o", "/out"];
        console.log('Running: pandoc', args.join(' '));
        
        pandoc(args);
        
        // Read the DOCX binary data from the output file
        const docxBytes = outFile.data;
        
        if (!docxBytes || docxBytes.length === 0) {
            throw new Error('Pandoc produced empty output');
        }
        
        console.log('✅ Conversion complete! Output size:', docxBytes.length, 'bytes');
        
        return docxBytes;
    } catch (error) {
        console.error('❌ Pandoc conversion failed:', error);
        throw error;
    }
}

// Convert markdown to PDF using Pandoc WASM
export async function convertMarkdownToPdf(markdownContent) {
    if (!pandocInstance) {
        throw new Error('Pandoc WASM not initialized. Call initializePandoc() first.');
    }
    
    try {
        console.log('📝 Converting markdown to PDF with Pandoc WASM...');
        console.log('Input length:', markdownContent.length, 'characters');
        
        // Write markdown content to the input file
        inFile.data = new TextEncoder().encode(markdownContent);
        console.log('✅ Input file written:', inFile.data.length, 'bytes');
        
        // Clear the output file before running
        outFile.data = new Uint8Array();
        
        // Run pandoc: read from /in, write PDF to /out
        // Using HTML as intermediate format since PDF engines may not be available in WASM
        const args = ["/in", "-f", "markdown", "-t", "html", "-o", "/out", "--self-contained"];
        console.log('Running: pandoc', args.join(' '));
        
        pandoc(args);
        
        // Read the HTML output
        const htmlBytes = outFile.data;
        
        if (!htmlBytes || htmlBytes.length === 0) {
            throw new Error('Pandoc produced empty output');
        }
        
        console.log('⚠️ Note: Direct PDF generation not available in Pandoc WASM');
        console.log('Returning HTML output instead. Consider using browser print-to-PDF.');
        console.log('✅ Conversion complete! Output size:', htmlBytes.length, 'bytes');
        
        return htmlBytes;
    } catch (error) {
        console.error('❌ Pandoc conversion failed:', error);
        throw error;
    }
}

// Convert markdown to PPTX using Pandoc WASM
export async function convertMarkdownToPptx(markdownContent) {
    if (!pandocInstance) {
        throw new Error('Pandoc WASM not initialized. Call initializePandoc() first.');
    }
    
    try {
        console.log('📝 Converting markdown to PPTX with Pandoc WASM...');
        console.log('Input length:', markdownContent.length, 'characters');
        
        // Write markdown content to the input file
        inFile.data = new TextEncoder().encode(markdownContent);
        console.log('✅ Input file written:', inFile.data.length, 'bytes');
        
        // Clear the output file before running
        outFile.data = new Uint8Array();
        
        // Run pandoc: read from /in, write PPTX to /out
        // Each heading (# or ##) becomes a slide
        const args = ["/in", "-f", "markdown", "-t", "pptx", "-o", "/out"];
        console.log('Running: pandoc', args.join(' '));
        
        pandoc(args);
        
        // Read the PPTX binary data from the output file
        const pptxBytes = outFile.data;
        
        if (!pptxBytes || pptxBytes.length === 0) {
            throw new Error('Pandoc produced empty output');
        }
        
        console.log('✅ Conversion complete! Output size:', pptxBytes.length, 'bytes');
        
        return pptxBytes;
    } catch (error) {
        console.error('❌ Pandoc PPTX conversion failed:', error);
        throw error;
    }
}

// Convert markdown to Beamer (LaTeX presentation) using Pandoc WASM
export async function convertMarkdownToBeamer(markdownContent) {
    if (!pandocInstance) {
        throw new Error('Pandoc WASM not initialized. Call initializePandoc() first.');
    }
    
    try {
        console.log('📝 Converting markdown to Beamer with Pandoc WASM...');
        console.log('Input length:', markdownContent.length, 'characters');
        
        // Write markdown content to the input file
        inFile.data = new TextEncoder().encode(markdownContent);
        console.log('✅ Input file written:', inFile.data.length, 'bytes');
        
        // Clear the output file before running
        outFile.data = new Uint8Array();
        
        // Run pandoc: read from /in, write Beamer LaTeX to /out
        const args = ["/in", "-f", "markdown", "-t", "beamer", "-o", "/out"];
        console.log('Running: pandoc', args.join(' '));
        
        pandoc(args);
        
        // Read the Beamer LaTeX output
        const beamerBytes = outFile.data;
        
        if (!beamerBytes || beamerBytes.length === 0) {
            throw new Error('Pandoc produced empty output');
        }
        
        console.log('✅ Conversion complete! Output size:', beamerBytes.length, 'bytes');
        
        return beamerBytes;
    } catch (error) {
        console.error('❌ Pandoc Beamer conversion failed:', error);
        throw error;
    }
}

// Convert markdown to Reveal.js (HTML presentation) using Pandoc WASM
export async function convertMarkdownToRevealJs(markdownContent) {
    if (!pandocInstance) {
        throw new Error('Pandoc WASM not initialized. Call initializePandoc() first.');
    }
    
    try {
        console.log('📝 Converting markdown to Reveal.js with Pandoc WASM...');
        console.log('Input length:', markdownContent.length, 'characters');
        
        // Write markdown content to the input file
        inFile.data = new TextEncoder().encode(markdownContent);
        console.log('✅ Input file written:', inFile.data.length, 'bytes');
        
        // Clear the output file before running
        outFile.data = new Uint8Array();
        
        // Run pandoc: read from /in, write Reveal.js HTML to /out
        const args = ["/in", "-f", "markdown", "-t", "revealjs", "-o", "/out", "--standalone"];
        console.log('Running: pandoc', args.join(' '));
        
        pandoc(args);
        
        // Read the Reveal.js HTML output
        const revealBytes = outFile.data;
        
        if (!revealBytes || revealBytes.length === 0) {
            throw new Error('Pandoc produced empty output');
        }
        
        console.log('✅ Conversion complete! Output size:', revealBytes.length, 'bytes');
        
        return revealBytes;
    } catch (error) {
        console.error('❌ Pandoc Reveal.js conversion failed:', error);
        throw error;
    }
}

// Make functions available globally for .NET interop
window.pandocInterop = {
    initializePandoc,
    convertMarkdownToDocx,
    convertMarkdownToPdf,
    convertMarkdownToPptx,
    convertMarkdownToBeamer,
    convertMarkdownToRevealJs
};

console.log('Pandoc interop loaded');
