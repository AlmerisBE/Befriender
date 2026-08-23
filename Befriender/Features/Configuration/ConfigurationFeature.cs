using Befriender.Core;
using Befriender.Features.Command.Contracts;
using Befriender.Features.Configuration.Commands;
using Befriender.Features.Configuration.Contracts;
using Befriender.Features.Configuration.Services;
using Befriender.Features.Configuration.UI;
using Dalamud.Interface.Windowing;
using Microsoft.Extensions.DependencyInjection;

namespace Befriender.Features.Configuration;

public class ConfigurationFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IConfigurationService, ConfigurationService>();

        // Window registration
        services.AddSingleton<ConfigWindow>();
        services.AddSingleton<Window>(provider => provider.GetRequiredService<ConfigWindow>());

        services.AddSingleton<ICommand, ConfigCommand>();
    }
}