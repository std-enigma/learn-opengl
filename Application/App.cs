using System.Drawing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Shader = Shared.OpenGL.Shader;

namespace Application;

public class App
{
    private readonly IWindow _window;

    private GL? _gl;

    private Shader? _shader;
    private uint _vao;

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
        _window.Resize += OnResize;
    }

    public void Run()
    {
        _window.Run();
    }

    private unsafe void OnLoad()
    {
        var input = _window.CreateInput();
        foreach (var keyboard in input.Keyboards)
            keyboard.KeyDown += OnKeyDown;

        _gl = _window.CreateOpenGL();
        _gl.ClearColor(Color.CornflowerBlue);

        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        var vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);

        var vertices = new[]
        {
            0.0f, 0.5f, 0.0f, 1.0f, 0.7f, 0.8f,
            -0.5f, -0.5f, 0.0f, 0.7f, 1.0f, 0.9f,
            0.5f, -0.5f, 0.0f, 0.9f, 0.8f, 1.0f
        };
        fixed (float* bufData = vertices)
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), bufData,
                BufferUsageARB.StaticDraw);
        }

        var ebo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

        var indices = new[] { 0u, 1u, 2u };
        fixed (uint* bufData = indices)
        {
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), bufData,
                BufferUsageARB.StaticDraw);
        }

        _shader = Shader.FromFile(_gl, "Resources/default.vert", "Resources/default.frag");
        _shader.Use();
        _shader.SetUniform("u_offset", 0.5f);

        const int posLoc = 0;
        _gl.EnableVertexAttribArray(posLoc);
        _gl.VertexAttribPointer(posLoc, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

        const int colorLoc = 1;
        _gl.EnableVertexAttribArray(colorLoc);
        _gl.VertexAttribPointer(colorLoc, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float),
            3 * sizeof(float));

        _gl.UseProgram(0);
        _gl.BindVertexArray(0);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
    }

    private unsafe void OnRender(double deltaTime)
    {
        if (_gl is null)
            return;

        _gl.Clear(ClearBufferMask.ColorBufferBit);

        _shader?.Use();
        _gl.BindVertexArray(_vao);
        _gl.DrawElements(PrimitiveType.Triangles, 3, DrawElementsType.UnsignedInt, (void*)0);
    }

    private void OnResize(Vector2D<int> newSize)
    {
        _gl?.Viewport(newSize);
    }

    private void OnKeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        if (key is Key.Escape)
            _window.Close();
    }
}