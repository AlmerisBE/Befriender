using Dalamud.Game;
using System.Collections.Generic;

namespace Befriender.Features.Localization.Contracts;

public interface ILocalizationProvider {
    IReadOnlyDictionary<ClientLanguage, Dictionary<string, string>> GetTranslations();
}