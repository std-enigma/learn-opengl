using Silk.NET.OpenGL;

namespace Shared.OpenGL;

public class BufferObject<TDataType> : IDisposable where TDataType : unmanaged
{
    private readonly GL _gl;
    private readonly uint _handle;
    private readonly BufferTargetARB _type;

    public unsafe BufferObject(GL gl, BufferTargetARB type, ReadOnlySpan<TDataType> data)
    {
        _gl = gl;
        _type = type;
        _handle = gl.GenBuffer();

        Bind();
        fixed (TDataType* bufData = data)
        {
            _gl.BufferData(type, (nuint)(data.Length * sizeof(TDataType)), bufData, BufferUsageARB.StaticDraw);
        }
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    public void Bind()
    {
        _gl.BindBuffer(_type, _handle);
    }

    private void ReleaseUnmanagedResources()
    {
        _gl.DeleteBuffer(_handle);
    }

    ~BufferObject()
    {
        ReleaseUnmanagedResources();
    }
}