using Befriender.Core.Framework;
using Befriender.Core.Logging.Contracts;
using Befriender.Core.Logging.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Befriender.Core.Logging;

public class LoggingFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<ILoggerService, LoggerService>();
    }
}