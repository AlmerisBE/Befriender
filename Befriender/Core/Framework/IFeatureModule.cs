using Microsoft.Extensions.DependencyInjection;

namespace Befriender.Core.Framework;

public interface IFeatureModule {
    void RegisterServices(IServiceCollection services);
}