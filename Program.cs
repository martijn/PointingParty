using System.Net;
using MassTransit;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Rewrite;
using PointingParty.Components;
using PointingParty.Infrastructure;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.SetInMemorySagaRepositoryProvider();

    x.AddConsumer<DebugConsumer>();
    x.AddConsumer<EventHub>();

    x.UsingInMemory((context, cfg) => { cfg.ConfigureEndpoints(context); });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("172.16.0.0"), 16));
});

builder.Services.AddSingleton<EventHub>();
builder.Services.AddScoped<GameContext>();

var app = builder.Build();

app.UseForwardedHeaders();
app.UseRewriter(new RewriteOptions().AddRedirectToNonWwwPermanent());

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", true);
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
