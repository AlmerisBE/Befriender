namespace Befriender.Core.Characters;

using Befriender.Core.Characters.Actions;
using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Services;
using Befriender.Core.Characters.Storage;
using Befriender.Core.Framework;
using Microsoft.Extensions.DependencyInjection;

public class CharactersFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        // Identity
        services.AddSingleton<ICharacterIdentityService, CharacterIdentityService>();

        // Core Registry & Storage
        services.AddSingleton<CharacterRegistry>();
        services.AddSingleton<ICharacterRegistry>(provider => provider.GetRequiredService<CharacterRegistry>());
        services.AddSingleton<ICharacterStorage, JsonCharacterStorage>();

        // Groups & Tags
        services.AddSingleton<ICharacterGroupStorage, JsonCharacterGroupStorage>();
        services.AddSingleton<ICharacterGroupRepository, CharacterGroupRepository>();

        services.AddSingleton<ICharacterTagStorage, JsonCharacterTagStorage>();
        services.AddSingleton<ICharacterTagRepository, CharacterTagRepository>();

        // Services transversaux des personnages
        services.AddSingleton<IRemoveCharacterRequestService, RemoveCharacterRequestService>();

        // Actions Consolidation
        services.AddSingleton<ICharacterAction, CopyNameAction>();
        services.AddSingleton<ICharacterAction, DeleteCharacterDataAction>();
        services.AddSingleton<ICharacterAction, EstateTeleportationAction>();
        services.AddSingleton<ICharacterAction, JoinCharacterAction>();
        services.AddSingleton<ICharacterAction, NativeInviteToPartyAction>();
        services.AddSingleton<ICharacterAction, RequestRemoveCharacterAction>();
        services.AddSingleton<ICharacterAction, SendTellAction>();
        services.AddSingleton<ICharacterAction, TrackCharacterAction>();
        services.AddSingleton<ICharacterAction, UnmarkForRemovalAction>();
        services.AddSingleton<ICharacterAction, UntrackCharacterAction>();
        services.AddSingleton<ICharacterAction, ViewAdventurerPlateAction>();
        services.AddSingleton<ICharacterAction, ViewPartyFinderListingAction>();
        services.AddSingleton<ICharacterAction, ViewSearchInfoAction>();

        services.AddSingleton<ICharacterActionService, CharacterActionService>();
    }
}