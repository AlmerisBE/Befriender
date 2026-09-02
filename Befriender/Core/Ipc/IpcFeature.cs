namespace Befriender.Core.Ipc;

using Befriender.Core.Framework;
using Befriender.Core.Ipc.Contracts;
using Befriender.Core.Ipc.Services;
using Microsoft.Extensions.DependencyInjection;

public class IpcFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IIpcProviderService, IpcProviderService>();
    }
}