namespace SpaceInvaders.Core.Engine;

public static class SpriteData
{
    private static bool[,] Parse(string[] rows)
    {
        var result = new bool[rows.Length, rows[0].Length];
        for (int r = 0; r < rows.Length; r++)
            for (int c = 0; c < rows[r].Length; c++)
                result[r, c] = rows[r][c] == '#';
        return result;
    }

    // Squid (top row, 8x8)
    public static readonly bool[,] Squid0 = Parse([
        "...##...",
        "..####..",
        ".######.",
        "##.##.##",
        "########",
        "..#..#..",
        ".#.##.#.",
        "#.#..#.#",
    ]);

    public static readonly bool[,] Squid1 = Parse([
        "...##...",
        "..####..",
        ".######.",
        "##.##.##",
        "########",
        ".#.##.#.",
        "#......#",
        ".#....#.",
    ]);

    // Crab (middle rows, 11x8)
    public static readonly bool[,] Crab0 = Parse([
        "..#.....#..",
        "...#...#...",
        "..#######..",
        ".##.###.##.",
        "###########",
        "#.#######.#",
        "#.#.....#.#",
        "...##.##...",
    ]);

    public static readonly bool[,] Crab1 = Parse([
        "..#.....#..",
        "#..#...#..#",
        "#.#######.#",
        "###.###.###",
        "###########",
        ".#########.",
        "..#.....#..",
        ".#.......#.",
    ]);

    // Octopus (bottom rows, 12x8)
    public static readonly bool[,] Octopus0 = Parse([
        "....####....",
        ".##########.",
        "############",
        "###..##..###",
        "############",
        "..###..###..",
        ".##.####.##.",
        "#..#....#..#",
    ]);

    public static readonly bool[,] Octopus1 = Parse([
        "....####....",
        ".##########.",
        "############",
        "###..##..###",
        "############",
        "...##..##...",
        "..##.##.##..",
        "..#......#..",
    ]);

    // Player ship (13x8)
    public static readonly bool[,] Player = Parse([
        "......#......",
        ".....###.....",
        ".....###.....",
        "..#########..",
        ".###########.",
        "#############",
        "#############",
        "#############",
    ]);

    public static bool[,] GetAlienSprite(int row, int frame)
    {
        return row switch
        {
            0 => frame == 0 ? Squid0 : Squid1,
            1 or 2 => frame == 0 ? Crab0 : Crab1,
            _ => frame == 0 ? Octopus0 : Octopus1,
        };
    }

    public static int GetAlienWidth(int row) => row switch
    {
        0 => 8,
        1 or 2 => 11,
        _ => 12,
    };
}
