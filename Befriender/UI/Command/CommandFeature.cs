namespace Befriender.UI.Command;

using Befriender.Core.Framework;
using Befriender.UI.Command.Contracts;
using Befriender.UI.Command.Implementations;
using Befriender.UI.Command.Services;
using Microsoft.Extensions.DependencyInjection;

public class CommandFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<CommandDispatcher>();

        // Register diagnostic command
        services.AddSingleton<ICommand, DumpGameDataCommand>();
    }
}