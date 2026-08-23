using Befriender.Core.Configuration.Models;

namespace Befriender.Core.Configuration.Contracts;

public interface IConfigurationService {
    PluginConfiguration GetConfig();
    void Save();
}