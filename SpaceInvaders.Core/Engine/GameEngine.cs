using Avalonia.Input;

namespace SpaceInvaders.Core.Engine;

public class GameEngine
{
    public const int ScreenWidth = 800;
    public const int ScreenHeight = 600;
    public const int PixelScale = 3;
    private const double PlayerSpeed = 300.0;
    private const double PlayerBulletSpeed = -450.0;
    private const double EnemyBulletSpeed = 200.0;
    private const int AlienColumns = 11;
    private const int AlienRows = 5;
    private const double AlienHSpacing = 48;
    private const double AlienVSpacing = 40;
    private const double AlienStartX = 60;
    private const double AlienStartY = 80;
    private const int BarrierCount = 4;
    private const double BarrierY = 460;
    private const double PlayerY = 540;
    private const double PlayerFireCooldown = 0.4;
    private const double BaseEnemyFireInterval = 1.5;
    private const double AlienStepX = 6;
    private const double AlienDropY = 20;
    private const double BaseMoveInterval = 0.6;
    private const double MinMoveInterval = 0.03;

    public GamePhase Phase { get; private set; } = GamePhase.StartScreen;
    public PlayerShip Player { get; private set; } = new();
    public List<Alien> Aliens { get; private set; } = [];
    public List<Bullet> Bullets { get; private set; } = [];
    public List<Barrier> Barriers { get; private set; } = [];
    public GameInfo Info { get; private set; } = new();

    private HashSet<Key> _pressedKeys = [];
    private int _alienDirection = 1;
    private double _alienMoveTimer;
    private double _enemyFireTimer;
    private double _playerFireCooldown;
    private double _playerRespawnTimer;
    private readonly Random _random = new();
    private int _totalAliens;

    public void UpdateInput(HashSet<Key> keys)
    {
        _pressedKeys = keys;
    }

    public void Update(double dt)
    {
        switch (Phase)
        {
            case GamePhase.StartScreen:
                if (_pressedKeys.Contains(Key.Enter))
                    StartGame();
                break;

            case GamePhase.Playing:
                UpdatePlaying(dt);
                break;

            case GamePhase.GameOver:
                if (_pressedKeys.Contains(Key.Enter))
                    StartGame();
                break;
        }
    }

    private void StartGame()
    {
        Info = new GameInfo();
        InitializeLevel();
    }

    private void InitializeLevel()
    {
        Player = new PlayerShip
        {
            X = ScreenWidth / 2.0 - (13 * PixelScale) / 2.0,
            Y = PlayerY,
            IsAlive = true
        };

        Aliens = [];
        for (int row = 0; row < AlienRows; row++)
        {
            for (int col = 0; col < AlienColumns; col++)
            {
                Aliens.Add(new Alien
                {
                    X = AlienStartX + col * AlienHSpacing,
                    Y = AlienStartY + row * AlienVSpacing,
                    Row = row,
                    Column = col,
                    AnimationFrame = 0,
                    IsAlive = true
                });
            }
        }
        _totalAliens = Aliens.Count;

        Bullets = [];
        InitializeBarriers();

        _alienDirection = 1;
        _alienMoveTimer = GetMoveInterval();
        _enemyFireTimer = BaseEnemyFireInterval / (1 + (Info.Level - 1) * 0.2);
        _playerFireCooldown = 0;
        _playerRespawnTimer = 0;

        Phase = GamePhase.Playing;
    }

    private void InitializeBarriers()
    {
        Barriers = [];
        double totalWidth = ScreenWidth - 120;
        double spacing = totalWidth / BarrierCount;

        for (int i = 0; i < BarrierCount; i++)
        {
            int cellRows = 8;
            int cellCols = 11;
            var cells = new bool[cellRows, cellCols];

            for (int r = 0; r < cellRows; r++)
                for (int c = 0; c < cellCols; c++)
                    cells[r, c] = true;

            // Rounded top corners
            cells[0, 0] = false; cells[0, 1] = false;
            cells[0, cellCols - 1] = false; cells[0, cellCols - 2] = false;
            cells[1, 0] = false;
            cells[1, cellCols - 1] = false;

            // Arch cutout at bottom center
            for (int r = cellRows - 3; r < cellRows; r++)
                for (int c = 4; c <= 6; c++)
                    cells[r, c] = false;
            cells[cellRows - 4, 5] = false;

            Barriers.Add(new Barrier
            {
                X = 60 + i * spacing + spacing / 2 - (cellCols * 4) / 2.0,
                Y = BarrierY,
                Cells = cells
            });
        }
    }

    private double GetMoveInterval()
    {
        int aliveCount = Aliens.Count(a => a.IsAlive);
        if (aliveCount == 0) return BaseMoveInterval;
        double ratio = (double)aliveCount / _totalAliens;
        double levelFactor = 1.0 / (1 + (Info.Level - 1) * 0.15);
        return Math.Max(MinMoveInterval, BaseMoveInterval * ratio * levelFactor);
    }

    private void UpdatePlaying(double dt)
    {
        if (_playerRespawnTimer > 0)
        {
            _playerRespawnTimer -= dt;
            if (_playerRespawnTimer <= 0)
                Player.IsAlive = true;
            return;
        }

        // Player movement
        if (Player.IsAlive)
        {
            if (_pressedKeys.Contains(Key.Left) || _pressedKeys.Contains(Key.A))
                Player.X -= PlayerSpeed * dt;
            if (_pressedKeys.Contains(Key.Right) || _pressedKeys.Contains(Key.D))
                Player.X += PlayerSpeed * dt;

            double pw = Player.Width * PixelScale;
            Player.X = Math.Clamp(Player.X, 10, ScreenWidth - pw - 10);

            // Player firing
            _playerFireCooldown -= dt;
            if (_pressedKeys.Contains(Key.Space) && _playerFireCooldown <= 0)
            {
                Bullets.Add(new Bullet
                {
                    X = Player.X + pw / 2 - 1,
                    Y = Player.Y - 6,
                    VelocityY = PlayerBulletSpeed,
                    IsPlayerBullet = true
                });
                _playerFireCooldown = PlayerFireCooldown;
            }
        }

        // Update bullets
        foreach (var bullet in Bullets)
        {
            bullet.Y += bullet.VelocityY * dt;
            if (bullet.Y < -10 || bullet.Y > ScreenHeight + 10)
                bullet.IsActive = false;
        }

        // Alien movement
        _alienMoveTimer -= dt;
        if (_alienMoveTimer <= 0)
        {
            bool needsDrop = false;

            foreach (var alien in Aliens)
            {
                if (!alien.IsAlive) continue;
                double aw = SpriteData.GetAlienWidth(alien.Row) * PixelScale;
                double newX = alien.X + AlienStepX * _alienDirection;
                if (newX < 5 || newX + aw > ScreenWidth - 5)
                {
                    needsDrop = true;
                    break;
                }
            }

            if (needsDrop)
            {
                foreach (var alien in Aliens)
                {
                    if (!alien.IsAlive) continue;
                    alien.Y += AlienDropY;
                    alien.AnimationFrame = 1 - alien.AnimationFrame;
                }
                _alienDirection *= -1;
            }
            else
            {
                foreach (var alien in Aliens)
                {
                    if (!alien.IsAlive) continue;
                    alien.X += AlienStepX * _alienDirection;
                    alien.AnimationFrame = 1 - alien.AnimationFrame;
                }
            }

            _alienMoveTimer = GetMoveInterval();
        }

        // Enemy firing
        _enemyFireTimer -= dt;
        if (_enemyFireTimer <= 0)
        {
            var bottomAliens = GetBottomRowAliens();
            if (bottomAliens.Count > 0)
            {
                var shooter = bottomAliens[_random.Next(bottomAliens.Count)];
                double aw = SpriteData.GetAlienWidth(shooter.Row) * PixelScale;
                Bullets.Add(new Bullet
                {
                    X = shooter.X + aw / 2 - 1,
                    Y = shooter.Y + shooter.Height * PixelScale,
                    VelocityY = EnemyBulletSpeed + Info.Level * 15,
                    IsPlayerBullet = false,
                });
            }
            _enemyFireTimer = (BaseEnemyFireInterval / (1 + (Info.Level - 1) * 0.2)) *
                              (0.5 + _random.NextDouble() * 0.5);
        }

        // Collision detection
        CollisionDetector.CheckBulletAlienCollisions(Bullets, Aliens, Info, PixelScale);

        if (Player.IsAlive && CollisionDetector.CheckBulletPlayerCollision(Bullets, Player, PixelScale))
        {
            Info.Lives--;
            if (Info.Lives <= 0)
            {
                Phase = GamePhase.GameOver;
                return;
            }
            Player.IsAlive = false;
            _playerRespawnTimer = 1.5;
            Player.X = ScreenWidth / 2.0 - (13 * PixelScale) / 2.0;
        }

        CollisionDetector.CheckBulletBarrierCollisions(Bullets, Barriers);
        CollisionDetector.CheckAlienBarrierCollisions(Aliens, Barriers, PixelScale);

        // Remove dead bullets
        Bullets.RemoveAll(b => !b.IsActive);

        // Check if aliens reached player level
        foreach (var alien in Aliens)
        {
            if (alien.IsAlive && alien.Y + alien.Height * PixelScale >= PlayerY)
            {
                Phase = GamePhase.GameOver;
                return;
            }
        }

        // Check win condition
        if (Aliens.All(a => !a.IsAlive))
        {
            Info.Level++;
            InitializeLevel();
        }
    }

    private List<Alien> GetBottomRowAliens()
    {
        var bottom = new Dictionary<int, Alien>();
        foreach (var alien in Aliens)
        {
            if (!alien.IsAlive) continue;
            if (!bottom.ContainsKey(alien.Column) || alien.Row > bottom[alien.Column].Row)
                bottom[alien.Column] = alien;
        }
        return [.. bottom.Values];
    }
}
