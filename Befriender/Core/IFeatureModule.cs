using Microsoft.Extensions.DependencyInjection;

namespace Befriender.Core;

public interface IFeatureModule {
    void RegisterServices(IServiceCollection services);
}