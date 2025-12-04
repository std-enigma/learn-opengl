namespace Application;

using System.Drawing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

internal class Application
{
    private uint _vao;
    private uint _program;

    private Vector3D<float> currentColor = new(1f, 0f, 0f);
    private Vector3D<float> targetColor = new(0.4f, 0.3f, 0.6f);
    private const float transitionSpeed = 0.5f;
    private readonly Random rng = new();
    private readonly Vector3D<float>[] palette = new[]
    {
        new Vector3D<float>(1f, 0.5f, 0.2f), // orange
        new Vector3D<float>(1f, 1f, 0.2f), // yellow
        new Vector3D<float>(0.2f, 0.6f, 1f), // blue
        new Vector3D<float>(0.7f, 0.2f, 1f), // purple
        new Vector3D<float>(0.2f, 1f, 0.4f), // aqua green
    };

    private GL? _gl;
    private readonly IWindow _window;

    public Application(string title, int width, int height)
    {
        var options = WindowOptions.Default with
        {
            Title = title,
            Size = new Vector2D<int>(width, height),
        };
        _window = Window.Create(options);
        _window.Load += OnLoad;
        _window.Render += OnRender;
        _window.Resize += OnResize;
    }

    // Load andn initialize application resources
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

        var vertices = new float[] { 0.0f, 0.5f, 0.0f, -0.5f, -0.5f, 0.0f, 0.5f, -0.5f, 0.0f };
        fixed (float* buf = vertices)
            _gl.BufferData(
                BufferTargetARB.ArrayBuffer,
                (nuint)(vertices.Length * sizeof(float)),
                buf,
                BufferUsageARB.StaticDraw
            );

        var vertCode =
            @"
            #version 330 core

            layout (location = 0) in vec3 aPos;

            void main()
            {
                gl_Position = vec4(aPos, 1.0);
            }
            ";
        var vert = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vert, vertCode);
        _gl.CompileShader(vert);
        _gl.GetShader(vert, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int)GLEnum.True)
            throw new Exception("Vertex shader failed to compile: " + _gl.GetShaderInfoLog(vert));

        var fragCode =
            @"
            #version 330 core

            uniform vec3 uColor;
            out vec4 FragColor;

            void main() {
                FragColor = vec4(uColor, 1.0f);
            }
            ";
        var frag = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(frag, fragCode);
        _gl.CompileShader(frag);
        _gl.GetShader(frag, ShaderParameterName.CompileStatus, out int fStatus);
        if (fStatus != (int)GLEnum.True)
            throw new Exception("Fragment shader failed to compile: " + _gl.GetShaderInfoLog(frag));

        _program = _gl.CreateProgram();
        _gl.AttachShader(_program, vert);
        _gl.AttachShader(_program, frag);
        _gl.LinkProgram(_program);
        _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int)GLEnum.True)
            throw new Exception(
                "Failed to link the shader program: " + _gl.GetProgramInfoLog(_program)
            );

        _gl.DetachShader(_program, vert);
        _gl.DetachShader(_program, frag);
        _gl.DeleteShader(vert);
        _gl.DeleteShader(frag);

        const uint posLoc = 0;
        _gl.EnableVertexAttribArray(posLoc);
        _gl.VertexAttribPointer(
            posLoc,
            3,
            VertexAttribPointerType.Float,
            false,
            3 * sizeof(float),
            (void*)0
        );

        _gl.BindVertexArray(0);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
    }

    // Render loop
    private void OnRender(double deltaTime)
    {
        if (_gl is null)
            return;

        _gl.Clear(ClearBufferMask.ColorBufferBit);

        _gl.UseProgram(_program);
        currentColor = Vector3D.Lerp(currentColor, targetColor, (float)deltaTime * transitionSpeed);
        if (Vector3D.Distance(currentColor, targetColor) < 0.02f)
            targetColor = palette[rng.Next(palette.Length)];
        var vertColorUniform = _gl.GetUniformLocation(_program, "uColor");
        _gl.Uniform3(vertColorUniform, currentColor.ToSystem());

        _gl.BindVertexArray(_vao);
        _gl.DrawArrays(PrimitiveType.Triangles, 0, 3);
    }

    // Handle window resizing
    private void OnResize(Vector2D<int> newSize)
    {
        _gl?.Viewport(newSize);
    }

    // Handle keyboard presses
    private void OnKeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        if (key is Key.Escape)
            _window.Close();
    }

    // Run the program
    public void Run()
    {
        _window.Run();
    }
}
