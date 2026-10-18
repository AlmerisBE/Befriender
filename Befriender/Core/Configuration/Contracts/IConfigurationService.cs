namespace Befriender.Core.Configuration.Contracts;

using Befriender.Core.Configuration.Models;
using System;

public interface IConfigurationService {
    event Action? ConfigSaved;
    PluginConfiguration GetConfig();
    void Save();
}