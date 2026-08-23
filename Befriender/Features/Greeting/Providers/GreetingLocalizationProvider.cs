using Befriender.Features.Localization.Providers;

namespace Befriender.Features.Greeting.Providers;

public class GreetingLocalizationProvider : JsonLocalizationProvider {
    // The base logical path. The abstract class will append ".en.json", ".fr.json", etc.
    protected override string ResourceBasePath => "Befriender.Features.Greeting.Resources";
}