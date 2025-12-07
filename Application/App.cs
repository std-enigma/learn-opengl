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
    private uint _program1;
    private uint _program2;
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
            -0.5f, 0.0f, 0.0f, -0.25f, 0.5f, 0.0f, 0.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 0.25f, 0.5f, 0.0f, 0.5f, 0.0f, 0.0f
        };
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

        const string fragCode1 = """
                                 #version 330 core

                                 out vec4 FragColor;

                                 void main()
                                 {
                                     FragColor = vec4(0.4, 0.3, 0.6, 1.0);
                                 }
                                 """;

        const string fragCode2 = """
                                 #version 330 core

                                 out vec4 FragColor;

                                 void main()
                                 {
                                     FragColor = vec4(1.0, 0.5, 0.2, 1.0);
                                 }
                                 """;

        var frag1 = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(frag1, fragCode1);
        _gl.CompileShader(frag1);
        _gl.GetShader(frag1, ShaderParameterName.CompileStatus, out var fStatus1);
        if (fStatus1 != (int)GLEnum.True)
            throw new Exception("Fragment shader (1) has failed to compile: " + _gl.GetShaderInfoLog(frag1));

        var frag2 = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(frag2, fragCode2);
        _gl.CompileShader(frag2);
        _gl.GetShader(frag2, ShaderParameterName.CompileStatus, out var fStatus2);
        if (fStatus2 != (int)GLEnum.True)
            throw new Exception("Fragment shader (2) has failed to compile: " + _gl.GetShaderInfoLog(frag2));

        _program1 = _gl.CreateProgram();
        _gl.AttachShader(_program1, vert);
        _gl.AttachShader(_program1, frag1);
        _gl.LinkProgram(_program1);
        _gl.GetProgram(_program1, ProgramPropertyARB.LinkStatus, out var lStatus1);
        if (lStatus1 != (int)GLEnum.True)
            throw new Exception("Shader program (1) failed to link: " + _gl.GetProgramInfoLog(_program1));

        _program2 = _gl.CreateProgram();
        _gl.AttachShader(_program2, vert);
        _gl.AttachShader(_program2, frag2);
        _gl.LinkProgram(_program2);
        _gl.GetProgram(_program2, ProgramPropertyARB.LinkStatus, out var lStatus2);
        if (lStatus2 != (int)GLEnum.True)
            throw new Exception("Shader program (2) failed to link: " + _gl.GetProgramInfoLog(_program2));

        const int posLoc = 0;
        _gl.EnableVertexAttribArray(posLoc);
        _gl.VertexAttribPointer(posLoc, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

        _gl.DetachShader(_program1, vert);
        _gl.DetachShader(_program1, frag1);
        _gl.DetachShader(_program2, frag2);
        _gl.DeleteShader(vert);
        _gl.DeleteShader(frag1);
        _gl.DeleteShader(frag2);

        _gl.BindVertexArray(0);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
    }

    private void OnRender(double deltaTime)
    {
        _gl?.Clear(ClearBufferMask.ColorBufferBit);

        _gl?.BindVertexArray(_vao);
        _gl?.UseProgram(_program1);
        _gl?.DrawArrays(PrimitiveType.Triangles, 0, 3);
        _gl?.UseProgram(_program2);
        _gl?.DrawArrays(PrimitiveType.Triangles, 3, 3);
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