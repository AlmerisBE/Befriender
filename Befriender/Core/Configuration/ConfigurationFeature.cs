namespace Befriender.Core.Configuration;

using Befriender.Core.Command.Contracts;
using Befriender.Core.Configuration.Commands;
using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Configuration.Services;
using Befriender.Core.Framework;
using Befriender.UI.Configuration.Tabs; // Ajouté
using Befriender.UI.Windows.Contracts;
using Microsoft.Extensions.DependencyInjection;

public class ConfigurationFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IConfigurationService, ConfigurationService>();

        services.AddSingleton<ITab, ConfigTab>();

        services.AddSingleton<ICommand, ConfigCommand>();
    }
}