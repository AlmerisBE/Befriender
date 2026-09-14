namespace Befriender.Tests.UI.Theme.Converters;

using global::Befriender.UI.Theme.Converters;
using System.Numerics;
using System.Text.Json;
using Xunit;

public class Vector4HexJsonConverterTests {
    private JsonSerializerOptions options;

    public Vector4HexJsonConverterTests() {
        this.options = new JsonSerializerOptions();
        this.options.Converters.Add(new Vector4HexJsonConverter());
    }

    [Fact]
    public void Read_ValidHex_ReturnsCorrectVector4() {
        string json = "\"#FF8040FF\"";

        var result = JsonSerializer.Deserialize<Vector4>(json, this.options);

        Assert.Equal(1.0f, result.X);
        Assert.Equal(128f / 255f, result.Y, 0.01f);
        Assert.Equal(64f / 255f, result.Z, 0.01f);
        Assert.Equal(1.0f, result.W);
    }

    [Fact]
    public void Read_MissingAlpha_AppendsFullOpacity() {
        string json = "\"#FF0000\"";

        var result = JsonSerializer.Deserialize<Vector4>(json, this.options);

        Assert.Equal(1.0f, result.X);
        Assert.Equal(0.0f, result.Y);
        Assert.Equal(0.0f, result.Z);
        Assert.Equal(1.0f, result.W);
    }

    [Fact]
    public void Read_InvalidHex_ReturnsVectorOneAsFallback() {
        string json = "\"#INVALID\"";

        var result = JsonSerializer.Deserialize<Vector4>(json, this.options);

        Assert.Equal(Vector4.One, result);
    }

    [Fact]
    public void Write_Vector4_OutputsCorrectHex() {
        var vector = new Vector4(1.0f, 0.5f, 0.25f, 1.0f);

        string json = JsonSerializer.Serialize(vector, this.options);

        Assert.Equal("\"#FF7F3FFF\"", json);
    }
}