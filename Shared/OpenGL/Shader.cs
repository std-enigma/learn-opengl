using Silk.NET.OpenGL;

namespace Shared.OpenGL;

public class Shader : IDisposable
{
    private readonly GL _gl;
    private readonly uint _handle;

    private Shader(GL gl, string vertCode, string fragCode)
    {
        _gl = gl;
        _handle = gl.CreateProgram();

        var vert = LoadShader(ShaderType.VertexShader, vertCode);
        var frag = LoadShader(ShaderType.FragmentShader, fragCode);
        _gl.AttachShader(_handle, vert);
        _gl.AttachShader(_handle, frag);
        _gl.LinkProgram(_handle);
        _gl.GetProgram(_handle, ProgramPropertyARB.LinkStatus, out var status);
        _gl.GetProgramInfoLog(_handle, out var infoLog);
        if (status != (int)GLEnum.True)
            throw new Exception($"Error linking program {_handle}, failed with error {infoLog}");
        _gl.DetachShader(_handle, vert);
        _gl.DetachShader(_handle, frag);
        _gl.DeleteShader(vert);
        _gl.DeleteShader(frag);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    public void Use()
    {
        _gl.UseProgram(_handle);
    }

    public void SetUniform(string name, float value)
    {
        var location = _gl.GetUniformLocation(_handle, name);
        _gl.Uniform1(location, value);
    }

    public static Shader FromFile(GL gl, string vertPath, string fragPath)
    {
        var vertSource = File.ReadAllText(vertPath);
        var fragSource = File.ReadAllText(fragPath);
        return new Shader(gl, vertSource, fragSource);
    }

    private uint LoadShader(ShaderType type, string source)
    {
        var handle = _gl.CreateShader(type);
        _gl.ShaderSource(handle, source);
        _gl.CompileShader(handle);
        _gl.GetShader(handle, ShaderParameterName.CompileStatus, out var status);
        _gl.GetShaderInfoLog(handle, out var infoLog);
        return status != (int)GLEnum.True
            ? throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}")
            : handle;
    }

    private void ReleaseUnmanagedResources()
    {
        _gl.DeleteProgram(_handle);
    }

    ~Shader()
    {
        ReleaseUnmanagedResources();
    }
}