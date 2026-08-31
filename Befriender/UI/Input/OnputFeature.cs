namespace Befriender.UI.Input;

using Befriender.Core.Framework;
using Befriender.UI.Input.Contracts;
using Befriender.UI.Input.Services;
using Microsoft.Extensions.DependencyInjection;

public class InputFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<HotkeyService>();
        services.AddSingleton<IHotkeyService>(provider => provider.GetRequiredService<HotkeyService>());
    }
}