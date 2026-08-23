using Befriender.Core;
using Befriender.Features.Logging.Contracts;
using Befriender.Features.Logging.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Befriender.Features.Logging;

public class LoggingFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<ILoggerService, LoggerService>();
    }
}