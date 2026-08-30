namespace Befriender.Core.FreeCompany;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Framework;
using Befriender.Core.FreeCompany.Contracts;
using Befriender.Core.FreeCompany.Scanners;
using Befriender.Core.FreeCompany.Services;
using Microsoft.Extensions.DependencyInjection;

public class FreeCompanyFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IFreeCompanyScanner, MemoryFreeCompanyScanner>();

        services.AddSingleton<FreeCompanyRepository>();
        services.AddSingleton<IFreeCompanyRepository>(provider => provider.GetRequiredService<FreeCompanyRepository>());
        services.AddSingleton<ICharacterSource>(provider => provider.GetRequiredService<FreeCompanyRepository>());

        services.AddSingleton<FreeCompanySyncService>();
        services.AddSingleton<IFreeCompanySyncService>(provider => provider.GetRequiredService<FreeCompanySyncService>());
    }
}