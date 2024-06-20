using Microsoft.AspNetCore.SignalR.Client;

namespace PointingParty.Client;

// Provide a harness for the SignalR source generator
internal static partial class HubConnectionExtensions
{
    [HubClientProxy]
    public static partial IDisposable ClientRegistration<T>(this HubConnection connection, T provider);

    [HubServerProxy]
    public static partial T ServerProxy<T>(this HubConnection connection);
}

[AttributeUsage(AttributeTargets.Method)]
internal class HubServerProxyAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Method)]
internal class HubClientProxyAttribute : Attribute
{
}