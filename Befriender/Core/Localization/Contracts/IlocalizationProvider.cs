using Dalamud.Game;
using System.Collections.Generic;

namespace Befriender.Core.Localization.Contracts;

public interface ILocalizationProvider {
    IReadOnlyDictionary<ClientLanguage, Dictionary<string, string>> GetTranslations();
}