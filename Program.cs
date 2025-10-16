using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MDConverter;
using MDConverter.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register conversion services
builder.Services.AddScoped<MarkdownParser>();
builder.Services.AddScoped<DocxConverter>();
builder.Services.AddScoped<PdfConverter>();
builder.Services.AddScoped<MermaidService>();
builder.Services.AddScoped<PandocService>();
builder.Services.AddScoped<SvgHandler>();
builder.Services.AddSingleton<AppState>();

await builder.Build().RunAsync();
