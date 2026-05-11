namespace SpaceInvaders.Core.Engine;

public static class CollisionDetector
{
    private static bool Intersects(double x1, double y1, double w1, double h1,
                                   double x2, double y2, double w2, double h2)
    {
        return x1 < x2 + w2 && x1 + w1 > x2 && y1 < y2 + h2 && y1 + h1 > y2;
    }

    public static void CheckBulletAlienCollisions(List<Bullet> bullets, List<Alien> aliens, GameInfo info, int scale)
    {
        foreach (var bullet in bullets)
        {
            if (!bullet.IsActive || !bullet.IsPlayerBullet) continue;

            foreach (var alien in aliens)
            {
                if (!alien.IsAlive) continue;

                double aw = SpriteData.GetAlienWidth(alien.Row) * scale;
                double ah = alien.Height * scale;

                if (Intersects(bullet.X, bullet.Y, bullet.Width, bullet.Height,
                               alien.X, alien.Y, aw, ah))
                {
                    bullet.IsActive = false;
                    alien.IsAlive = false;
                    info.Score += alien.Row switch
                    {
                        0 => 30,
                        1 or 2 => 20,
                        _ => 10,
                    };
                    break;
                }
            }
        }
    }

    public static bool CheckBulletPlayerCollision(List<Bullet> bullets, PlayerShip player, int scale)
    {
        double pw = player.Width * scale;
        double ph = player.Height * scale;

        foreach (var bullet in bullets)
        {
            if (!bullet.IsActive || bullet.IsPlayerBullet) continue;

            if (Intersects(bullet.X, bullet.Y, bullet.Width, bullet.Height,
                           player.X, player.Y, pw, ph))
            {
                bullet.IsActive = false;
                return true;
            }
        }
        return false;
    }

    public static void CheckBulletBarrierCollisions(List<Bullet> bullets, List<Barrier> barriers)
    {
        foreach (var bullet in bullets)
        {
            if (!bullet.IsActive) continue;

            foreach (var barrier in barriers)
            {
                int cols = barrier.Cells.GetLength(1);
                int rows = barrier.Cells.GetLength(0);
                double bw = cols * barrier.CellSize;
                double bh = rows * barrier.CellSize;

                if (!Intersects(bullet.X, bullet.Y, bullet.Width, bullet.Height,
                                barrier.X, barrier.Y, bw, bh))
                    continue;

                int startCol = Math.Max(0, (int)((bullet.X - barrier.X) / barrier.CellSize));
                int endCol = Math.Min(cols - 1, (int)((bullet.X + bullet.Width - barrier.X) / barrier.CellSize));
                int startRow = Math.Max(0, (int)((bullet.Y - barrier.Y) / barrier.CellSize));
                int endRow = Math.Min(rows - 1, (int)((bullet.Y + bullet.Height - barrier.Y) / barrier.CellSize));

                bool hit = false;
                for (int r = startRow; r <= endRow; r++)
                    for (int c = startCol; c <= endCol; c++)
                        if (barrier.Cells[r, c])
                        {
                            barrier.Cells[r, c] = false;
                            hit = true;
                        }

                if (hit)
                {
                    int damageRadius = 1;
                    int centerRow = (startRow + endRow) / 2;
                    int centerCol = (startCol + endCol) / 2;
                    for (int r = centerRow - damageRadius; r <= centerRow + damageRadius; r++)
                        for (int c = centerCol - damageRadius; c <= centerCol + damageRadius; c++)
                            if (r >= 0 && r < rows && c >= 0 && c < cols)
                                barrier.Cells[r, c] = false;

                    bullet.IsActive = false;
                    break;
                }
            }
        }
    }

    public static void CheckAlienBarrierCollisions(List<Alien> aliens, List<Barrier> barriers, int scale)
    {
        foreach (var alien in aliens)
        {
            if (!alien.IsAlive) continue;

            double aw = SpriteData.GetAlienWidth(alien.Row) * scale;
            double ah = alien.Height * scale;

            foreach (var barrier in barriers)
            {
                int cols = barrier.Cells.GetLength(1);
                int rows = barrier.Cells.GetLength(0);
                double bw = cols * barrier.CellSize;
                double bh = rows * barrier.CellSize;

                if (!Intersects(alien.X, alien.Y, aw, ah, barrier.X, barrier.Y, bw, bh))
                    continue;

                int startCol = Math.Max(0, (int)((alien.X - barrier.X) / barrier.CellSize));
                int endCol = Math.Min(cols - 1, (int)((alien.X + aw - barrier.X) / barrier.CellSize));
                int startRow = Math.Max(0, (int)((alien.Y - barrier.Y) / barrier.CellSize));
                int endRow = Math.Min(rows - 1, (int)((alien.Y + ah - barrier.Y) / barrier.CellSize));

                for (int r = startRow; r <= endRow; r++)
                    for (int c = startCol; c <= endCol; c++)
                        barrier.Cells[r, c] = false;
            }
        }
    }
}
