var board = new Board(30, 20);
board.PlaceBombs(99);

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
    }
}


class Space
{
    public SpaceState State = SpaceState.Hidden;
    public bool IsBomb = false;
    public int Neighbors = 0;
}

enum SpaceState
{
    Hidden,
    Revealed,
    Flagged,
}
