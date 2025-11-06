var board = new Board(30, 20);

class Board
{
    public int Height { get; set; }
    public int Width { get; set; }
    public Board(int width, int height)
    {
        Height = height;
        Width = width;
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
