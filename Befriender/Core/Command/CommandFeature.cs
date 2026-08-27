namespace Befriender.Core.Command;

using Befriender.Core.Command.Contracts;
using Befriender.Core.Command.Implementations;
using Befriender.Core.Command.Services;
using Befriender.Core.Framework;
using Microsoft.Extensions.DependencyInjection;

public class CommandFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<CommandDispatcher>();

        // Register diagnostic command
        services.AddSingleton<ICommand, DumpGameDataCommand>();
    }
}