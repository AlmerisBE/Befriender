namespace Befriender.Tests.UI.Overlays.Services;

using global::Befriender.UI.Overlays.Services;
using System.Numerics;
using Xunit;

public class MinimapOverlayServiceTests {
    [Fact]
    public void CalculateOverlayPosition_CorrectlyTranslatesAndClamps3DWorldTo2DScreen() {
        var service = new MinimapOverlayService(null!, null!, null!, null!, null!, null!, null!, null!);

        var localPos = new Vector3(100f, 50f, 200f);
        var friendPos = new Vector3(110f, 50f, 190f);
        var mapCenter = new Vector2(500f, 500f);
        float scale = 2.0f;
        float maxRadius = 100f;

        var result = service.CalculateOverlayPosition(localPos, friendPos, mapCenter, scale, maxRadius);

        // Standard test without clamping (distance is within 100f)
        Assert.Equal(520f, result.X);
        Assert.Equal(480f, result.Y);

        // Edge case: target is extremely far away, should clamp strictly to maxRadius
        var farFriendPos = new Vector3(300f, 50f, 200f); // deltaX = 200, deltaZ = 0
        var clampedResult = service.CalculateOverlayPosition(localPos, farFriendPos, mapCenter, scale, maxRadius);

        Assert.Equal(600f, clampedResult.X); // mapCenter X (500) + maxRadius (100)
        Assert.Equal(500f, clampedResult.Y); // mapCenter Y remains unchanged
    }
}