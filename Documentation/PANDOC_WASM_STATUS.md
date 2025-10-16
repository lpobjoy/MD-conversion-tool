# Pandoc WASM Integration Status

## Current State: In Progress ⚠️

We've laid the groundwork for Pandoc WASM integration, but it requires additional WASI filesystem support.

### What's Been Done ✅

1. **Downloaded pandoc.wasm** (50.4MB) - Real Pandoc compiled to WebAssembly
2. **Created pandoc-interop.js** - JavaScript wrapper placeholder
3. **Created PandocService.cs** - C# service ready to call Pandoc
4. **Added UI toggle** - Conversion mode selector (currently disabled)

### What's Needed 🔧

The official pandoc-wasm demo (https://tweag.github.io/pandoc-wasm) works because it uses a custom WASI implementation. To fully integrate it into our Blazor app, we need:

1. **WASI Filesystem Shim**: 
   - Options: `@bjorn3/browser_wasi_shim` or custom implementation
   - Required for virtual filesystem (reading/writing files)

2. **Proper WASI Runtime**:
   - The current `@wasmer/wasi` doesn't provide the full filesystem API
   - Need to study the pandoc-wasm frontend source: https://github.com/tweag/pandoc-wasm/tree/master/frontend

3. **Command-line Arguments**:
   - Pandoc expects CLI args (`-f markdown -t docx -o output.docx`)
   - Need to properly pass these through WASI

### Alternative Approach 🎯

**Current Working Solution**: Custom DOCX Converter
- ✅ Fixed text duplication bug (empty paragraph + ProcessInlineElements)
- ✅ HTML entity decoding working
- ✅ Table formatting improved
- ✅ PNG embedding with aspect ratios working
- ⏳ **Test this first** - it may be good enough!

### Next Steps

**Option A: Complete Pandoc WASM Integration**
1. Study https://github.com/tweag/pandoc-wasm/blob/master/frontend/index.html
2. Implement browser_wasi_shim
3. Create proper WASI filesystem bridge
4. Test with actual pandoc.wasm module

**Option B: Improve Custom Converter**
1. Test current fixes (text duplication, tables, etc.)
2. Add any missing features
3. Keep it simple and maintainable

**Recommendation**: Test the custom converter first! The fixes we made should significantly improve DOCX quality. If it's good enough, we can keep Pandoc WASM as a "future enhancement."

## Resources

- Pandoc WASM Demo: https://tweag.github.io/pandoc-wasm
- Pandoc WASM Source: https://github.com/tweag/pandoc-wasm
- Browser WASI Shim: https://github.com/bjorn3/browser_wasi_shim
- Pandoc Diagram Filter: https://github.com/pandoc-ext/diagram (requires Lua - not supported in WASM build)
