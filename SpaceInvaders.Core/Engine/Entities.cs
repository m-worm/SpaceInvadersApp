namespace SpaceInvaders.Core.Engine;

public class PlayerShip
{
    public double X { get; set; }
    public double Y { get; set; }
    public int Width { get; } = 13;
    public int Height { get; } = 8;
    public bool IsAlive { get; set; } = true;
}

public class Alien
{
    public double X { get; set; }
    public double Y { get; set; }
    public int Height { get; } = 8;
    public bool IsAlive { get; set; } = true;
    public int Row { get; set; }
    public int Column { get; set; }
    public int AnimationFrame { get; set; }
}

public class Bullet
{
    public double X { get; set; }
    public double Y { get; set; }
    public int Width { get; } = 2;
    public int Height { get; } = 6;
    public double VelocityY { get; set; }
    public bool IsPlayerBullet { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Barrier
{
    public double X { get; set; }
    public double Y { get; set; }
    public bool[,] Cells { get; set; } = null!;
    public int CellSize { get; } = 4;
}

public class GameInfo
{
    public int Score { get; set; }
    public int Lives { get; set; } = 3;
    public int Level { get; set; } = 1;
}

public enum GamePhase
{
    StartScreen,
    Playing,
    GameOver
}
