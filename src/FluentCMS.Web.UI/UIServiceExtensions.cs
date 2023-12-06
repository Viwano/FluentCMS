﻿namespace Microsoft.Extensions.DependencyInjection;

public static class UIServiceExtensions
{

    public static IServiceCollection AddUIServices(this IServiceCollection services)
    {
        // Add services to the container.
        services.AddRazorComponents()
            .AddInteractiveServerComponents();

        return services;
    }
}