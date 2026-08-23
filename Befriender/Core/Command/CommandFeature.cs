using Befriender.Core.Command.Services;
using Befriender.Core.Framework;
using Microsoft.Extensions.DependencyInjection;

namespace Befriender.Core.Command;

public class CommandFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<CommandDispatcher>();
    }
}