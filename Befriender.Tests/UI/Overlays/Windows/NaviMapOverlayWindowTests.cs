namespace Befriender.Tests.UI.Overlays.Windows;

using global::Befriender.UI.Overlays.Windows;
using System;
using System.Numerics;
using Xunit;

public class NaviMapOverlayWindowTests {
    [Fact]
    public void CalculateOverlayPosition_CorrectlyTranslatesAndClamps3DWorldTo2DScreen() {
        var localPos = new Vector3(100f, 50f, 200f);
        var friendPos = new Vector3(110f, 50f, 190f);
        var mapCenter = new Vector2(500f, 500f);

        float rotation = 0f;
        float zoom = 1.0f;
        float naviScale = 1.0f;
        float sizeFactor = 100f;
        float maxRadius = 200f;
        bool isLocked = false;

        // Call the static method directly on the Window class
        var noRotationResult = NaviMapOverlayWindow.CalculateOverlayPosition(localPos, friendPos, mapCenter, rotation, zoom, naviScale, sizeFactor, maxRadius, isLocked);

        Assert.Equal(505f, noRotationResult.X);
        Assert.Equal(495f, noRotationResult.Y);

        float rot90 = MathF.PI / 2f;
        var rotatedResult = NaviMapOverlayWindow.CalculateOverlayPosition(localPos, friendPos, mapCenter, rot90, zoom, naviScale, sizeFactor, maxRadius, isLocked);

        Assert.Equal(505f, MathF.Round(rotatedResult.X));
        Assert.Equal(505f, MathF.Round(rotatedResult.Y));
    }
}