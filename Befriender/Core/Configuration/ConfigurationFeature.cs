namespace Befriender.Core.Configuration;

using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Configuration.Services;
using Befriender.Core.Framework;
using Befriender.UI.Command.Contracts;
using Befriender.UI.Command.Implementations;
using Befriender.UI.Configuration.Tabs;
using Befriender.UI.MainWindow.Contracts;
using Microsoft.Extensions.DependencyInjection;

public class ConfigurationFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<AutomationService>();

        services.AddSingleton<ITab, ConfigTab>();
        services.AddSingleton<ICommand, ConfigCommand>();
    }
}