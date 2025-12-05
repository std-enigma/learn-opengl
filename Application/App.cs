namespace Application;

using System.Drawing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

public class App
{
    private GL? _gl;
    private readonly IWindow _window;

    public App(string title, int width, int height)
    {
        var options = WindowOptions.Default with
        {
            Title = title,
            Size = new Vector2D<int>(width, height)
        };
        _window = Window.Create(options);
        _window.Load += OnLoad;
        _window.Render += OnRender;
    }

    public void Run()
    {
        _window.Run();
    }

    private void OnLoad()
    {
        var input = _window.CreateInput();
        foreach (var keyboard in input.Keyboards)
            keyboard.KeyDown += OnKeyDown;

        _gl = _window.CreateOpenGL();
        _gl.ClearColor(Color.CornflowerBlue);
    }

    private void OnRender(double deltaTime)
    {
        _gl?.Clear(ClearBufferMask.ColorBufferBit);
    }

    private void OnKeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        if (key is Key.Escape)
            _window.Close();
    }
}