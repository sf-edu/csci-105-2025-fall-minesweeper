using System.Text.Json;

Board board = null!;
var bombCount = 99;

var width = 30;
var height = 20;

var bombsPlaced = false;

if (Board.HasSave())
{
    Console.Write("Continue save? [Y/n] ");
    switch (Console.ReadKey(true).Key)
    {
        case ConsoleKey.N:
            board = new Board(width, height);
            break;

        default:
            board = Board.Load();
            bombsPlaced = true;
            break;
    }
}
else
{
    board = new Board(width, height);
}

int x = board.Width / 2;
int y = board.Height / 2;

Console.Clear();
Console.CursorVisible = false;

while (true)
{
    board.Render();

    if (board.State != BoardState.InProgress)
    {
        break;
    }

    Console.SetCursorPosition(x * 2, y);
    Console.BackgroundColor = ConsoleColor.DarkYellow;
    Console.Write(" ");
    Console.ResetColor();

    switch (Console.ReadKey(true).Key)
    {
        case ConsoleKey.Q:
        case ConsoleKey.Escape:
            Console.Clear();
            Console.CursorVisible = true;
            board.Save();
            return;

        case ConsoleKey.K:
        case ConsoleKey.W:
        case ConsoleKey.UpArrow:
            y = Math.Max(y - 1, 0);
            break;

        case ConsoleKey.H:
        case ConsoleKey.A:
        case ConsoleKey.LeftArrow:
            x = Math.Max(x - 1, 0);
            break;

        case ConsoleKey.J:
        case ConsoleKey.S:
        case ConsoleKey.DownArrow:
            y = Math.Min(y + 1, board.Height - 1);
            break;

        case ConsoleKey.L:
        case ConsoleKey.D:
        case ConsoleKey.RightArrow:
            x = Math.Min(x + 1, board.Width - 1);
            break;

        case ConsoleKey.Spacebar:
            if (!bombsPlaced)
            {
                board.PlaceBombs(bombCount, x, y);
                board.CountNeighbors();
                bombsPlaced = true;
            }
            board.Reveal(x, y);
            board.CheckForVictory();
            break;

        case ConsoleKey.F:
            board.ToggleFlag(x, y);
            board.CheckForVictory();
            break;
    }
}

class Board
{
    public Space[] Spaces { get; set; }
    public int Height { get; set; }
    public int Width { get; set; }

    public BoardState State { get; set; } = BoardState.InProgress;

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

    static string SAVE_DIRECTORY =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".minesweeper"
        );
    static string SAVE_PATH => Path.Combine(SAVE_DIRECTORY, "save.json");

    public static bool HasSave()
    {
        return File.Exists(SAVE_PATH);
    }

    public static Board Load()
    {
        return JsonSerializer.Deserialize<Board>(File.ReadAllText(SAVE_PATH))!;
    }

    public void Save()
    {
        if (!Directory.Exists(SAVE_DIRECTORY))
        {
            Directory.CreateDirectory(SAVE_DIRECTORY);
        }

        File.WriteAllText(SAVE_PATH, JsonSerializer.Serialize(this));
    }

    public static void Delete()
    {
        if (File.Exists(SAVE_PATH))
        {
            File.Delete(SAVE_PATH);
        }
    }

    public Space GetSpace(int x, int y)
    {
        return Spaces[x + y * Width];
    }

    public void PlaceBombs(int count, int cursorX, int cursorY)
    {
        var random = new Random();

        var top = Math.Max(cursorY - 1, 0);
        var left = Math.Max(cursorX - 1, 0);
        var right = Math.Min(cursorX + 1, Width - 1);
        var bottom = Math.Min(cursorY + 1, Height - 1);

        for (var i = 0; i < count; i++)
        {
            PlaceBomb(random, top, left, right, bottom);
        }
    }

    public void PlaceBomb(Random random, int top, int left, int right, int bottom)
    {
        while (true)
        {
            var x = random.Next(Width);
            var y = random.Next(Height);

            if (left <= x && x <= right && top <= y && y <= bottom)
            {
                continue;
            }

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

    public void Reveal(int x, int y, bool popFlags = false)
    {
        var space = GetSpace(x, y);
        if (space.State == SpaceState.Revealed || space.State == SpaceState.Flagged && !popFlags)
        {
            return;
        }

        space.State = SpaceState.Revealed;

        if (space.IsBomb)
        {
            foreach (var s in Spaces.Where(s => s.IsBomb))
            {
                s.State = SpaceState.Revealed;
            }

            State = BoardState.Defeat;
            Board.Delete();
            return;
        }

        if (0 < space.Neighbors)
        {
            return;
        }

        var top = Math.Max(y - 1, 0);
        var left = Math.Max(x - 1, 0);
        var right = Math.Min(x + 1, Width - 1);
        var bottom = Math.Min(y + 1, Height - 1);

        for (var y1 = top; y1 <= bottom; y1++)
        {
            for (var x1 = left; x1 <= right; x1++)
            {
                Reveal(x1, y1, true);
            }
        }
    }

    public void ToggleFlag(int x, int y)
    {
        var space = GetSpace(x, y);

        switch (space.State)
        {
            case SpaceState.Hidden:
                space.State = SpaceState.Flagged;
                break;

            case SpaceState.Flagged:
                space.State = SpaceState.Hidden;
                break;
        }
    }

    public void CheckForVictory()
    {
        var unflaggedBombs = Spaces
            .Where(s => s.IsBomb)
            .Where(s => s.State != SpaceState.Flagged)
            .Count();

        var unrevealedSpaces = Spaces
            .Where(s => !s.IsBomb)
            .Where(s => s.State != SpaceState.Revealed)
            .Count();

        if (unflaggedBombs == 0 && unrevealedSpaces == 0)
        {
            State = BoardState.Victory;
            Board.Delete();
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

        var bombCount = Spaces.Count(s => s.IsBomb);
        var flagCount = Spaces.Count(s => s.State == SpaceState.Flagged);

        Console.WriteLine($"\nFlags remaining: {bombCount - flagCount}       ");

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("\nwasd / hjkl / arrows, f to flag, space to reveal");
        Console.ResetColor();

        switch (State)
        {
            case BoardState.Victory:
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nCongrats! You live to blow up another minefield.");
                Console.ResetColor();
                break;

            case BoardState.Defeat:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nHave fun not having limbs anymore, sucker.");
                Console.ResetColor();
                break;
        }
    }
}

enum BoardState
{
    InProgress,
    Victory,
    Defeat,
}

class Space
{
    public SpaceState State { get; set; } = SpaceState.Hidden;
    public bool IsBomb { get; set; } = false;
    public int Neighbors { get; set; } = 0;

    public const char EMPTY_TOKEN = ' ';

    public const char HIDDEN_TOKEN = '.';
    public const ConsoleColor HIDDEN_FG = ConsoleColor.Green;

    public const char BOMB_TOKEN = '@';
    public const ConsoleColor BOMB_FG = ConsoleColor.Red;

    public const char FLAG_TOKEN = 'F';
    public const ConsoleColor FLAG_FG = ConsoleColor.White;

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
                Console.ForegroundColor = HIDDEN_FG;
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
