var board = new Board(30, 20);

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
