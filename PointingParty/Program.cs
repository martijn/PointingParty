using PointingParty;
using PointingParty.Client;
using PointingParty.Components;
using _Imports = PointingParty.Client.Components._Imports;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddSignalR().AddAzureSignalR(options => options.InitialHubServerConnectionCount = 1);

// GameContext is purely injected for prerendering purposes
builder.Services.AddTransient<IGameContext, MockGameContext>();

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
