using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PointingParty.Client;
using Syncfusion.Blazor;
using System.Reflection;

// Register Syncfusion license from assembly metadata or configuration
var builder = WebAssemblyHostBuilder.CreateDefault(args);
var entry = Assembly.GetExecutingAssembly();
var licenseFromMetadata = entry.GetCustomAttributes<AssemblyMetadataAttribute>()
    .FirstOrDefault(a => a.Key == "SyncfusionLicenseKey")?.Value;
var license = licenseFromMetadata
              ?? builder.Configuration["Syncfusion:LicenseKey"]
              ?? builder.Configuration["SYNCFUSION_LICENSE_KEY"];
if (!string.IsNullOrWhiteSpace(license))
{
    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(license);
}

builder.Services.AddTransient<IGameContext, GameContext>();
builder.Services.AddSyncfusionBlazor();

await builder.Build().RunAsync();
