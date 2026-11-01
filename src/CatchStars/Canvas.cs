using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace CatchStars;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct Vertex(Vector2 position, Color4 color)
{
    public readonly Vector2 Position = position;
    public readonly Color4 Color = color;
}

internal sealed class Canvas
{
    private readonly List<Vertex> _vertices = [];
    public Vertex[] Vertices => _vertices.ToArray();

    private void Triangle(Vector2 a, Vector2 b, Vector2 c, Color4 color)
    {
        _vertices.Add(new Vertex(a, color));
        _vertices.Add(new Vertex(b, color));
        _vertices.Add(new Vertex(c, color));
    }

    public void Rectangle(float x, float y, float width, float height, Color4 color)
    {
        Triangle(new(x, y), new(x + width, y), new(x + width, y + height), color);
        Triangle(new(x, y), new(x + width, y + height), new(x, y + height), color);
    }

    public void Star(float x, float y, float radius, Color4 color)
    {
        for (var i = 0; i < 10; i++)
        {
            Vector2 Point(int index)
            {
                var angle = MathF.PI / 2 + index * MathF.PI / 5;
                var length = index % 2 == 0 ? radius : radius * 0.45f;
                return new Vector2(x + MathF.Cos(angle) * length, y + MathF.Sin(angle) * length);
            }
            Triangle(new(x, y), Point(i), Point(i + 1), color);
        }
    }

    public void Text(string text, float x, float y, float size, Color4 color, bool centered = false)
    {
        if (centered) x -= (text.Length * 6 - 1) * size / 2;
        foreach (var character in text)
        {
            var rows = Glyph(character);
            for (var row = 0; row < rows.Length; row++)
                for (var column = 0; column < 5; column++)
                    if ((rows[row] & (1 << (4 - column))) != 0)
                        Rectangle(x + column * size, y + (6 - row) * size, size, size, color);
            x += 6 * size;
        }
    }

    private static ReadOnlySpan<byte> Glyph(char c) => c switch
    {
        'A' => [14, 17, 17, 31, 17, 17, 17], 'B' => [30, 17, 17, 30, 17, 17, 30],
        'C' => [14, 17, 16, 16, 16, 17, 14], 'D' => [30, 17, 17, 17, 17, 17, 30],
        'E' => [31, 16, 16, 30, 16, 16, 31], 'F' => [31, 16, 16, 30, 16, 16, 16],
        'G' => [14, 17, 16, 23, 17, 17, 15], 'H' => [17, 17, 17, 31, 17, 17, 17],
        'I' => [31, 4, 4, 4, 4, 4, 31], 'J' => [7, 2, 2, 2, 18, 18, 12],
        'K' => [17, 18, 20, 24, 20, 18, 17], 'L' => [16, 16, 16, 16, 16, 16, 31],
        'M' => [17, 27, 21, 21, 17, 17, 17], 'N' => [17, 25, 25, 21, 19, 19, 17],
        'O' => [14, 17, 17, 17, 17, 17, 14], 'P' => [30, 17, 17, 30, 16, 16, 16],
        'Q' => [14, 17, 17, 17, 21, 18, 13], 'R' => [30, 17, 17, 30, 20, 18, 17],
        'S' => [15, 16, 16, 14, 1, 1, 30], 'T' => [31, 4, 4, 4, 4, 4, 4],
        'U' => [17, 17, 17, 17, 17, 17, 14], 'V' => [17, 17, 17, 17, 17, 10, 4],
        'W' => [17, 17, 17, 21, 21, 21, 10], 'X' => [17, 17, 10, 4, 10, 17, 17],
        'Y' => [17, 17, 10, 4, 4, 4, 4], 'Z' => [31, 1, 2, 4, 8, 16, 31],
        '0' => [14, 17, 19, 21, 25, 17, 14], '1' => [4, 12, 4, 4, 4, 4, 14],
        '2' => [14, 17, 1, 2, 4, 8, 31], '3' => [30, 1, 1, 14, 1, 1, 30],
        '4' => [2, 6, 10, 18, 31, 2, 2], '5' => [31, 16, 16, 30, 1, 1, 30],
        '6' => [14, 16, 16, 30, 17, 17, 14], '7' => [31, 1, 2, 4, 8, 8, 8],
        '8' => [14, 17, 17, 14, 17, 17, 14], '9' => [14, 17, 17, 15, 1, 1, 14],
        '/' => [1, 1, 2, 4, 8, 16, 16], '.' => [0, 0, 0, 0, 0, 12, 12],
        ':' => [0, 12, 12, 0, 12, 12, 0], '-' => [0, 0, 0, 31, 0, 0, 0],
        '!' => [4, 4, 4, 4, 4, 0, 4], '?' => [14, 17, 1, 2, 4, 0, 4],
        _ => []
    };
}
