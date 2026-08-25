namespace Befriender.UI.Theme.Converters;

using System;
using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Vector4HexJsonConverter : JsonConverter<Vector4> {
    public override Vector4 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var hex = reader.GetString();
        if (string.IsNullOrEmpty(hex) || !hex.StartsWith("#")) {
            return Vector4.One;
        }

        hex = hex.Substring(1);
        if (hex.Length == 6) {
            hex += "FF"; // Auto-append full opacity if missing
        }

        if (hex.Length != 8) {
            return Vector4.One;
        }

        if (uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgba)) {
            return new Vector4(
                ((rgba >> 24) & 0xFF) / 255f,
                ((rgba >> 16) & 0xFF) / 255f,
                ((rgba >> 8) & 0xFF) / 255f,
                (rgba & 0xFF) / 255f
            );
        }
        return Vector4.One;
    }

    public override void Write(Utf8JsonWriter writer, Vector4 value, JsonSerializerOptions options) {
        var r = (byte)(Math.Clamp(value.X, 0f, 1f) * 255f);
        var g = (byte)(Math.Clamp(value.Y, 0f, 1f) * 255f);
        var b = (byte)(Math.Clamp(value.Z, 0f, 1f) * 255f);
        var a = (byte)(Math.Clamp(value.W, 0f, 1f) * 255f);
        writer.WriteStringValue($"#{r:X2}{g:X2}{b:X2}{a:X2}");
    }
}