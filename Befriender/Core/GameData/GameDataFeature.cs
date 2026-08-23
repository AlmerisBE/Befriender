namespace Befriender.Core.GameData;

using Befriender.Core.Framework;
using Befriender.Core.GameData.Contracts;
using Befriender.Core.GameData.Services;
using Microsoft.Extensions.DependencyInjection;

public class GameDataFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IGameDataService, GameDataService>();
    }
}