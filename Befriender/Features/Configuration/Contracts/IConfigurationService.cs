using Befriender.Features.Configuration.Models;

namespace Befriender.Features.Configuration.Contracts;

public interface IConfigurationService {
    PluginConfiguration GetConfig();
    void Save();
}