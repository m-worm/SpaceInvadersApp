using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using SpaceInvaders.Core.Engine;

namespace SpaceInvaders.Core;

public class GameCanvas : Control
{
    private static readonly IBrush BlackBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
    private static readonly IBrush GreenBrush = new SolidColorBrush(Color.FromRgb(0, 255, 0));
    private static readonly IBrush WhiteBrush = Brushes.White;
    private static readonly IBrush CyanBrush = new SolidColorBrush(Color.FromRgb(0, 255, 255));
    private static readonly IBrush MagentaBrush = new SolidColorBrush(Color.FromRgb(255, 0, 255));
    private static readonly IBrush RedBrush = new SolidColorBrush(Color.FromRgb(255, 50, 50));
    private static readonly IBrush YellowBrush = new SolidColorBrush(Color.FromRgb(255, 255, 0));

    private readonly GameEngine _engine = new();
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private double _lastTime;
    private readonly Typeface _typeface = new(FontFamily.Default, FontStyle.Normal, FontWeight.Bold);

    public GameCanvas()
    {
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        timer.Tick += OnTick;
        timer.Start();
        _lastTime = _stopwatch.Elapsed.TotalSeconds;
    }

    public void UpdateInput(HashSet<Key> keys)
    {
        _engine.UpdateInput(keys);
    }

    private void OnTick(object? sender, EventArgs e)
    {
        double current = _stopwatch.Elapsed.TotalSeconds;
        double dt = Math.Min(current - _lastTime, 0.1);
        _lastTime = current;
        _engine.Update(dt);
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        context.DrawRectangle(BlackBrush, null, new Rect(0, 0, GameEngine.ScreenWidth, GameEngine.ScreenHeight));

        switch (_engine.Phase)
        {
            case GamePhase.StartScreen:
                DrawStartScreen(context);
                break;
            case GamePhase.Playing:
                DrawGame(context);
                break;
            case GamePhase.GameOver:
                DrawGame(context);
                DrawGameOver(context);
                break;
        }
    }

    private void DrawGame(DrawingContext context)
    {
        DrawBarriers(context);
        DrawAliens(context);
        DrawBullets(context);
        if (_engine.Player.IsAlive)
            DrawPlayer(context);
        DrawHud(context);
    }

    private void DrawPlayer(DrawingContext context)
    {
        DrawSprite(context, SpriteData.Player, _engine.Player.X, _engine.Player.Y, GreenBrush, GameEngine.PixelScale);
    }

    private void DrawAliens(DrawingContext context)
    {
        foreach (var alien in _engine.Aliens)
        {
            if (!alien.IsAlive) continue;

            var sprite = SpriteData.GetAlienSprite(alien.Row, alien.AnimationFrame);
            var brush = alien.Row switch
            {
                0 => WhiteBrush,
                1 or 2 => CyanBrush,
                _ => MagentaBrush,
            };
            DrawSprite(context, sprite, alien.X, alien.Y, brush, GameEngine.PixelScale);
        }
    }

    private void DrawBullets(DrawingContext context)
    {
        foreach (var bullet in _engine.Bullets)
        {
            if (!bullet.IsActive) continue;
            var brush = bullet.IsPlayerBullet ? WhiteBrush : YellowBrush;
            context.DrawRectangle(brush, null, new Rect(bullet.X, bullet.Y, bullet.Width, bullet.Height));
        }
    }

    private void DrawBarriers(DrawingContext context)
    {
        foreach (var barrier in _engine.Barriers)
        {
            int rows = barrier.Cells.GetLength(0);
            int cols = barrier.Cells.GetLength(1);
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (!barrier.Cells[r, c]) continue;
                    context.DrawRectangle(GreenBrush, null, new Rect(
                        barrier.X + c * barrier.CellSize,
                        barrier.Y + r * barrier.CellSize,
                        barrier.CellSize, barrier.CellSize));
                }
            }
        }
    }

    private void DrawHud(DrawingContext context)
    {
        DrawText(context, $"SCORE: {_engine.Info.Score}", 20, 10, 22, WhiteBrush);
        DrawText(context, $"LEVEL: {_engine.Info.Level}", GameEngine.ScreenWidth / 2 - 40, 10, 22, WhiteBrush);
        DrawText(context, $"LIVES: {_engine.Info.Lives}", GameEngine.ScreenWidth - 140, 10, 22, WhiteBrush);
    }

    private void DrawStartScreen(DrawingContext context)
    {
        DrawText(context, "SPACE INVADERS", GameEngine.ScreenWidth / 2 - 140, 180, 40, GreenBrush);
        DrawText(context, "Arrow Keys / A,D - Move", GameEngine.ScreenWidth / 2 - 130, 300, 20, WhiteBrush);
        DrawText(context, "Space - Fire", GameEngine.ScreenWidth / 2 - 65, 330, 20, WhiteBrush);
        DrawText(context, "Press ENTER to Start", GameEngine.ScreenWidth / 2 - 115, 420, 24, YellowBrush);
    }

    private void DrawGameOver(DrawingContext context)
    {
        context.DrawRectangle(new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)), null,
            new Rect(0, 0, GameEngine.ScreenWidth, GameEngine.ScreenHeight));
        DrawText(context, "GAME OVER", GameEngine.ScreenWidth / 2 - 110, 200, 40, RedBrush);
        DrawText(context, $"Final Score: {_engine.Info.Score}", GameEngine.ScreenWidth / 2 - 100, 280, 24, WhiteBrush);
        DrawText(context, "Press ENTER to Restart", GameEngine.ScreenWidth / 2 - 125, 380, 24, YellowBrush);
    }

    private void DrawText(DrawingContext context, string text, double x, double y, double fontSize, IBrush brush)
    {
        var formatted = new FormattedText(text, System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight, _typeface, fontSize, brush);
        context.DrawText(formatted, new Point(x, y));
    }

    private static void DrawSprite(DrawingContext context, bool[,] sprite, double x, double y, IBrush brush, int scale)
    {
        int rows = sprite.GetLength(0);
        int cols = sprite.GetLength(1);
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (!sprite[r, c]) continue;
                context.DrawRectangle(brush, null, new Rect(
                    x + c * scale, y + r * scale, scale, scale));
            }
        }
    }
}
