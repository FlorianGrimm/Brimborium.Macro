// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using McMaster.Extensions.Hosting.CommandLine;
using McMaster.Extensions.Hosting.CommandLine.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.Hosting;

/// <summary>
///     Extension methods for <see cref="IHostBuilder" /> support.
/// </summary>
public static class HostBuilderExtensions
{

    /// <summary>
    ///     Configures an instance of <typeparamref name="TApp" /> using <see cref="CommandLineApplication" /> to provide
    ///     command line parsing on the given <paramref name="args" />.
    /// </summary>
    /// <typeparam name="TApp">The type of the command line application implementation</typeparam>
    /// <param name="hostBuilder">This instance</param>
    /// <param name="args">The command line arguments</param>
    /// <returns>fluent this</returns>
    public static HostApplicationBuilder UseCommandLineApplication<TApp>(
        this HostApplicationBuilder hostBuilder,
        string[] args, 
        Action<CommandLineApplication<TApp>> configure)
        where TApp : class
    {
        var state = new CommandLineState(args);
        hostBuilder.Services
            .AddCommonServices(state)
            .AddSingleton<ICommandLineService, CommandLineService<TApp>>()
            .AddSingleton<Action<CommandLineApplication<TApp>>>(configure)
            ;
        return hostBuilder;
    }

    private static IServiceCollection AddCommonServices(this IServiceCollection services, CommandLineState state)
    {
        services.TryAddSingleton<StoreExceptionHandler>();
        services.TryAddSingleton<IUnhandledExceptionHandler>(provider => provider.GetRequiredService<StoreExceptionHandler>());
        services.TryAddSingleton(PhysicalConsole.Singleton);
        services
            .AddSingleton<IHostLifetime, CommandLineLifetime>()
            .AddSingleton(provider =>
            {
                state.SetConsole(provider.GetRequiredService<IConsole>());
                return state;
            })
            .AddSingleton<CommandLineContext>(state);
        return services;
    }
}
