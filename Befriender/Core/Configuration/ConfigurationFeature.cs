using Befriender.Core.Command.Contracts;
using Befriender.Core.Configuration.Commands;
using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Configuration.Services;
using Befriender.Core.Framework;
using Befriender.UI.Windows;
using Dalamud.Interface.Windowing;
using Microsoft.Extensions.DependencyInjection;

namespace Befriender.Core.Configuration;

public class ConfigurationFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IConfigurationService, ConfigurationService>();

        // Window registration
        services.AddSingleton<ConfigWindow>();
        services.AddSingleton<Window>(provider => provider.GetRequiredService<ConfigWindow>());

        services.AddSingleton<ICommand, ConfigCommand>();
    }
}