using Silk.NET.OpenGL;

namespace Shared.OpenGL;

public class VertexArrayObject<TVertexType, TIndexType> : IDisposable
    where TVertexType : unmanaged where TIndexType : unmanaged
{
    private readonly GL _gl;
    private readonly uint _handle;

    public VertexArrayObject(GL gl, BufferObject<TVertexType> vbo, BufferObject<TIndexType> ebo)
    {
        _gl = gl;
        _handle = _gl.CreateVertexArray();

        Bind();
        vbo.Bind();
        ebo.Bind();
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    public void Bind()
    {
        _gl.BindVertexArray(_handle);
    }

    public unsafe void VertexAttribPointer(uint index, int count, VertexAttribPointerType type, uint vertexSize,
        int offset)
    {
        _gl.EnableVertexAttribArray(index);
        _gl.VertexAttribPointer(index, count, type, false, vertexSize * (uint)sizeof(TVertexType),
            (void*)(offset * sizeof(TVertexType)));
    }

    private void ReleaseUnmanagedResources()
    {
        _gl.DeleteVertexArray(_handle);
    }

    ~VertexArrayObject()
    {
        ReleaseUnmanagedResources();
    }
}