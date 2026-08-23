using Befriender.Core;
using Befriender.Features.Command.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Befriender.Features.Command;

public class CommandFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<CommandDispatcher>();
    }
}