using System.Drawing;
using Shared.OpenGL;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Application;

public class App
{
    private readonly IWindow _window;

    private GL? _gl;

    private ShaderObject? _shader;
    private VertexArrayObject<float, uint>? _vao;

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

    private void OnLoad()
    {
        var input = _window.CreateInput();
        foreach (var keyboard in input.Keyboards)
            keyboard.KeyDown += OnKeyDown;

        _gl = _window.CreateOpenGL();
        _gl.ClearColor(Color.CornflowerBlue);

        var vertices = new[]
        {
            0.0f, 0.5f, 0.0f, 1.0f, 0.7f, 0.8f,
            -0.5f, -0.5f, 0.0f, 0.7f, 1.0f, 0.9f,
            0.5f, -0.5f, 0.0f, 0.9f, 0.8f, 1.0f
        };
        var vbo = new BufferObject<float>(_gl, BufferTargetARB.ArrayBuffer, vertices);

        var indices = new[] { 0u, 1u, 2u };
        var ebo = new BufferObject<uint>(_gl, BufferTargetARB.ElementArrayBuffer, indices);

        _vao = new VertexArrayObject<float, uint>(_gl, vbo, ebo);

        _shader = ShaderObject.FromFile(_gl, "Resources/default.vert", "Resources/default.frag");

        const int posLoc = 0;
        _vao.VertexAttribPointer(posLoc, 3, VertexAttribPointerType.Float, 6, 0);

        const int colorLoc = 1;
        _vao.VertexAttribPointer(colorLoc, 3, VertexAttribPointerType.Float, 6, 3);

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
        _vao?.Bind();
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