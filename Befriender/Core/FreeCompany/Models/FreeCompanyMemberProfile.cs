namespace Befriender.Core.FreeCompany.Models;

public class FreeCompanyMemberProfile {
    public ulong ContentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public uint HomeWorldId { get; set; }
    public uint CurrentWorldId { get; set; }
    public byte JobId { get; set; }
    public uint LocationId { get; set; }
    public bool IsOnline { get; set; }
    public string FcTag { get; set; } = string.Empty;

    // Ajout du masque pour les icônes de statut
    public ulong OnlineStateMask { get; set; }
}