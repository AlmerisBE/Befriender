namespace Befriender.Core.Characters;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Services;
using Befriender.Core.Framework;
using Microsoft.Extensions.DependencyInjection;

public class CharactersFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<CharacterRegistry>();
        services.AddSingleton<ICharacterRegistry>(provider => provider.GetRequiredService<CharacterRegistry>());
    }
}