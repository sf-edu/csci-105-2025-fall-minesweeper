var board = new Board(30, 20);
board.PlaceBombs(99);
board.CountNeighbors();

Console.Clear();
Console.CursorVisible = false;

int x = board.Width / 2;
int y = board.Height / 2;

while (true)
{
    board.Render();
    Console.SetCursorPosition(x * 2, y);
    Console.ForegroundColor = ConsoleColor.Black;
    Console.BackgroundColor = ConsoleColor.Green;
    Console.Write("X");
    Console.ResetColor();

    switch (Console.ReadKey(true).Key)
    {
        case ConsoleKey.Escape:
            Console.Clear();
            Console.CursorVisible = true;
            return;

        case ConsoleKey.W:
        case ConsoleKey.UpArrow:
            y = Math.Max(y - 1, 0);
            break;

        case ConsoleKey.A:
        case ConsoleKey.LeftArrow:
            x = Math.Max(x - 1, 0);
            break;

        case ConsoleKey.S:
        case ConsoleKey.DownArrow:
            y = Math.Min(y + 1, board.Height - 1);
            break;

        case ConsoleKey.D:
        case ConsoleKey.RightArrow:
            x = Math.Min(x + 1, board.Width - 1);
            break;

    }
}

class Board
{
    public Space[] Spaces { get; set; }
    public int Height { get; set; }
    public int Width { get; set; }

    public Board(int width, int height)
    {
        Spaces = new Space[width * height];
        Height = height;
        Width = width;

        for (var i = 0; i < Spaces.Length; i++)
        {
            Spaces[i] = new Space();
        }
    }

    public Space GetSpace(int x, int y)
    {
        return Spaces[x + y * Width];
    }

    public void PlaceBombs(int count)
    {
        var random = new Random();

        for (var i = 0; i < count; i++)
        {
            PlaceBomb(random);
        }
    }

    public void PlaceBomb(Random random)
    {
        while (true)
        {
            var x = random.Next(Width);
            var y = random.Next(Height);

            var space = GetSpace(x, y);

            if (!space.IsBomb)
            {
                space.IsBomb = true;
                return;
            }
        }
    }

    /// <summary>
    /// Label all non-bomb spaces with the number of bombs it is adjacent to.
    /// This includes diagonals.
    /// </summary>
    public void CountNeighbors()
    {
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var top = Math.Max(y - 1, 0);
                var left = Math.Max(x - 1, 0);
                var right = Math.Min(x + 1, Width - 1);
                var bottom = Math.Min(y + 1, Height - 1);

                for (var y1 = top; y1 <= bottom; y1++)
                {
                    for (var x1 = left; x1 <= right; x1++)
                    {
                        if (GetSpace(x1, y1).IsBomb)
                        {
                            GetSpace(x, y).Neighbors++;
                        }
                    }
                }
            }
        }
    }
    public void Render()
    {
        Console.SetCursorPosition(0, 0);

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                GetSpace(x, y).Render();
            }

            Console.WriteLine();
        }
    }
}

class Space
{
    public SpaceState State = SpaceState.Hidden;
    public bool IsBomb = false;
    public int Neighbors = 0;

    public const char EMPTY_TOKEN = ' ';

    public const char HIDDEN_TOKEN = ' ';
    public const ConsoleColor HIDDEN_BG = ConsoleColor.Green;

    public const char BOMB_TOKEN = '@';
    public const ConsoleColor BOMB_FG = ConsoleColor.Red;

    public const char FLAG_TOKEN = 'X';
    public const ConsoleColor FLAG_FG = ConsoleColor.Blue;
    public const ConsoleColor FLAG_BG = ConsoleColor.Green;

    public readonly Dictionary<int, ConsoleColor> NEIGHBOR_COLORS = new()
    {
        { 1, ConsoleColor.Blue },
        { 2, ConsoleColor.Green },
        { 3, ConsoleColor.Red },
        { 4, ConsoleColor.Magenta },
        { 5, ConsoleColor.Yellow },
        { 6, ConsoleColor.Cyan },
        { 7, ConsoleColor.DarkYellow },
        { 8, ConsoleColor.DarkGreen },
    };

    /// SAFETY: Assumes the cursor is at the correct location on-screen.
    public void Render()
    {
        switch (State)
        {
            case SpaceState.Hidden:
                Console.BackgroundColor = HIDDEN_BG;
                Console.Write(HIDDEN_TOKEN);
                break;

            case SpaceState.Revealed:
                if (IsBomb)
                {
                    Console.ForegroundColor = BOMB_FG;
                    Console.Write(BOMB_TOKEN);
                    break;
                }
                else if (Neighbors <= 0)
                {
                    Console.Write(EMPTY_TOKEN);
                }
                else
                {
                    Console.ForegroundColor = NEIGHBOR_COLORS[Neighbors];
                    Console.Write(Neighbors);
                }
                break;

            case SpaceState.Flagged:
                Console.ForegroundColor = FLAG_FG;
                Console.BackgroundColor = FLAG_BG;
                Console.Write(FLAG_TOKEN);
                break;
        }

        Console.Write(" ");
        Console.ResetColor();
    }
}

enum SpaceState
{
    Hidden,
    Revealed,
    Flagged,
}
