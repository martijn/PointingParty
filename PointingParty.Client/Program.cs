using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PointingParty.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddTransient<GameContext>();

await builder.Build().RunAsync();
