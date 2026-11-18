namespace Befriender.Core.GameData.Services;

using Befriender.Core.GameData.Contracts;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;

public class ChatBoxService : IChatBoxService {
    public unsafe void SendMessage(string message) {
        if (string.IsNullOrWhiteSpace(message)) return;

        using var cmd = new Utf8String(message);

        cmd.SanitizeString(
            AllowedEntities.Unknown9 |
            AllowedEntities.Payloads |
            AllowedEntities.OtherCharacters |
            AllowedEntities.SpecialCharacters |
            AllowedEntities.Numbers |
            AllowedEntities.LowercaseLetters |
            AllowedEntities.UppercaseLetters);

        var uiModule = UIModule.Instance();
        if (uiModule != null) {
            uiModule->ProcessChatBoxEntry(&cmd);
        }
    }
}