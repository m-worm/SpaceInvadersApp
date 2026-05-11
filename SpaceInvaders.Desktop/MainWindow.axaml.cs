using Avalonia.Controls;
using Avalonia.Input;
using SpaceInvaders.Core;

namespace SpaceInvaders.Desktop;

public partial class MainWindow : Window
{
    private readonly HashSet<Key> _pressedKeys = [];
    private GameCanvas? _gameCanvas;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _gameCanvas = this.FindControl<GameCanvas>("GameCanvas");
        Focus();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        _pressedKeys.Add(e.Key);
        _gameCanvas?.UpdateInput(_pressedKeys);
        e.Handled = true;
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        _pressedKeys.Remove(e.Key);
        _gameCanvas?.UpdateInput(_pressedKeys);
        e.Handled = true;
    }
}
