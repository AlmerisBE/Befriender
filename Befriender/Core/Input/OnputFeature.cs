namespace Befriender.Core.Input;

using Befriender.Core.Framework;
using Befriender.Core.Input.Contracts;
using Befriender.Core.Input.Services;
using Microsoft.Extensions.DependencyInjection;

public class InputFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<HotkeyService>();
        services.AddSingleton<IHotkeyService>(provider => provider.GetRequiredService<HotkeyService>());
    }
}