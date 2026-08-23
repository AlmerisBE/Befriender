using Befriender.Core;
using Befriender.Features.Command.Contracts;
using Befriender.Features.Greeting.Commands;
using Befriender.Features.Greeting.Contracts;
using Befriender.Features.Greeting.Providers;
using Befriender.Features.Greeting.Services;
using Befriender.Features.Localization.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Befriender.Features.Greeting;

public class GreetingFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IGreetingService, GreetingService>();
        services.AddSingleton<ICommand, GreetingCommandAction>();
        services.AddSingleton<ILocalizationProvider, GreetingLocalizationProvider>();
    }
}