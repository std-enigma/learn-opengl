using System.Drawing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Application;

public class App
{
    private readonly IWindow _window;

    private GL? _gl;
    private uint _program;
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

        var vertices = new[] { 0.0f, 0.5f, 0.0f, 0.5f, -0.5f, 0.0f, -0.5f, -0.5f, 0.0f };
        fixed (float* bufData = vertices)
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), bufData,
                BufferUsageARB.StaticDraw);
        }

        const string vertCode = """
                                #version 330 core

                                layout(location = 0) in vec3 Position;

                                void main()
                                {
                                    gl_Position = vec4(Position, 1.0);
                                }
                                """;

        var vert = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vert, vertCode);
        _gl.CompileShader(vert);
        _gl.GetShader(vert, ShaderParameterName.CompileStatus, out var vStatus);
        if (vStatus != (int)GLEnum.True)
            throw new Exception("Vertex shader has failed to compile: " + _gl.GetShaderInfoLog(vert));

        const string fragCode = """
                                #version 330 core

                                out vec4 FragColor;

                                void main()
                                {
                                    FragColor = vec4(0.4, 0.3, 0.6, 1.0);
                                }
                                """;

        var frag = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(frag, fragCode);
        _gl.CompileShader(frag);
        _gl.GetShader(frag, ShaderParameterName.CompileStatus, out var fStatus);
        if (fStatus != (int)GLEnum.True)
            throw new Exception("Fragment shader has failed to compile: " + _gl.GetShaderInfoLog(frag));


        _program = _gl.CreateProgram();
        _gl.AttachShader(_program, vert);
        _gl.AttachShader(_program, frag);
        _gl.LinkProgram(_program);
        _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out var lStatus);
        if (lStatus != (int)GLEnum.True)
            throw new Exception("Shader program failed to link: " + _gl.GetProgramInfoLog(_program));

        const int posLoc = 0;
        _gl.EnableVertexAttribArray(posLoc);
        _gl.VertexAttribPointer(posLoc, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

        _gl.DetachShader(_program, vert);
        _gl.DetachShader(_program, frag);
        _gl.DeleteShader(vert);
        _gl.DeleteShader(frag);

        _gl.BindVertexArray(0);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
    }

    private void OnRender(double deltaTime)
    {
        _gl?.Clear(ClearBufferMask.ColorBufferBit);

        _gl?.UseProgram(_program);
        _gl?.BindVertexArray(_vao);
        _gl?.DrawArrays(PrimitiveType.Triangles, 0, 3);
    }

    private void OnKeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        if (key is Key.Escape)
            _window.Close();
    }
}