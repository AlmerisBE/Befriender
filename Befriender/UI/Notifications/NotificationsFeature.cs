namespace Befriender.UI.Notifications;

using Befriender.Core.Framework;
using Befriender.UI.Notifications.Services;
using Microsoft.Extensions.DependencyInjection;

public class NotificationsFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<OnlineNotificationService>();
    }
}