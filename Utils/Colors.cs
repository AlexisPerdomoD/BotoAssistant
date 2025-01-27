using System.Collections.Immutable;

namespace Boto.Utils;

public class Colors
{
    public enum ColorType
    {
        Primary,
        Secondary,
        Debug,
        Information,
        Warning,
        Error,
    }

    private static ImmutableDictionary<ColorType, string> _colorMap { get; set; }

    public static void LoadColors(Dictionary<ColorType, string> customColors)
    {
        Dictionary<ColorType, string> colorMap = _colorMap.ToDictionary(
            static entry => entry.Key,
            static entry => entry.Value
        );
        foreach (var color in customColors)
        {
            colorMap[color.Key] = color.Value;
        }
        _colorMap = colorMap.ToImmutableDictionary();
    }

    public static string GetByType(ColorType colorType) =>
        _colorMap.TryGetValue(colorType, out var color)
            ? color
            : throw new ArgumentOutOfRangeException(
                nameof(colorType),
                colorType,
                "Color type not found."
            );

    // Inicializar colores predeterminados
    static Colors()
    {
        var colorMap = new Dictionary<ColorType, string>
        {
            [ColorType.Primary] = "#4CAF50", // Verde vibrante
            [ColorType.Secondary] = "#8BC34A", // Verde claro
            [ColorType.Debug] = "#2196F3", // Azul brillante
            [ColorType.Information] = "#00BCD4", // Cyan
            [ColorType.Warning] = "#FFEB3B", // Amarillo
            [ColorType.Error] = "#F44336", // Rojo
        };

        _colorMap = colorMap.ToImmutableDictionary();
    }
}

