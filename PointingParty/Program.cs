using PointingParty;
using PointingParty.Client;
using PointingParty.Components;
using Syncfusion.Blazor;
using _Imports = PointingParty.Client.Components._Imports;
using System.Reflection;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddSignalR().AddAzureSignalR(options => options.InitialHubServerConnectionCount = 1);

// GameContext is purely injected for prerendering purposes
builder.Services.AddTransient<IGameContext, MockGameContext>();
builder.Services.AddSyncfusionBlazor();

// Try to read license from embedded assembly metadata
var assembly = Assembly.GetExecutingAssembly();
var licenseFromMetadata = assembly
    .GetCustomAttributes<AssemblyMetadataAttribute>()
    .FirstOrDefault(a => a.Key == "SyncfusionLicenseKey")?.Value;

// Register Syncfusion license from metadata, configuration, or env
var syncfusionLicense = licenseFromMetadata
                        ?? builder.Configuration["Syncfusion:LicenseKey"]
                        ?? builder.Configuration["SYNCFUSION_LICENSE_KEY"]
                        ?? Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY");
if (!string.IsNullOrWhiteSpace(syncfusionLicense))
{
    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncfusionLicense);
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.MapStaticAssets();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(_Imports).Assembly);

app.MapHub<GameEventHub>("/events");

app.Run();
