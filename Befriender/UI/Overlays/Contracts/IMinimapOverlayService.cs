namespace Befriender.UI.Overlays.Contracts;

using System.Numerics;

public interface IMinimapOverlayService {
    Vector2 CalculateOverlayPosition(Vector3 localPos, Vector3 friendPos, Vector2 mapCenter, float scale, float maxRadius);
}