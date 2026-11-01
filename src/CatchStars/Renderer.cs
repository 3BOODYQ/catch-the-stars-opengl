using System.Runtime.InteropServices;
using CatchStars.Core;
using OpenTK.Graphics.OpenGL4;

namespace CatchStars;

internal sealed class Renderer : IDisposable
{
    private readonly int _program;
    private readonly int _vertexArray;
    private readonly int _vertexBuffer;

    public Renderer()
    {
        _program = CreateProgram();
        _vertexArray = GL.GenVertexArray();
        _vertexBuffer = GL.GenBuffer();
        GL.BindVertexArray(_vertexArray);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
        var stride = Marshal.SizeOf<Vertex>();
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, stride, 2 * sizeof(float));
        GL.EnableVertexAttribArray(1);
        GL.UseProgram(_program);
        GL.Uniform2(GL.GetUniformLocation(_program, "uWorldSize"),
            (float)GameRules.Width, (float)GameRules.Height);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    public void Draw(Canvas canvas)
    {
        var vertices = canvas.Vertices;
        GL.UseProgram(_program);
        GL.BindVertexArray(_vertexArray);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * Marshal.SizeOf<Vertex>(),
            vertices, BufferUsageHint.StreamDraw);
        GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Length);
    }

    private static int CreateProgram()
    {
        const string vertexSource = """
            #version 330 core
            layout (location = 0) in vec2 aPosition;
            layout (location = 1) in vec4 aColor;
            uniform vec2 uWorldSize;
            out vec4 vertexColor;
            void main() {
                gl_Position = vec4(aPosition / uWorldSize * 2.0 - 1.0, 0.0, 1.0);
                vertexColor = aColor;
            }
            """;
        const string fragmentSource = """
            #version 330 core
            in vec4 vertexColor;
            out vec4 outputColor;
            void main() { outputColor = vertexColor; }
            """;
        var vertex = Compile(ShaderType.VertexShader, vertexSource);
        var fragment = 0;
        var program = GL.CreateProgram();
        try
        {
            fragment = Compile(ShaderType.FragmentShader, fragmentSource);
            GL.AttachShader(program, vertex);
            GL.AttachShader(program, fragment);
            GL.LinkProgram(program);
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var linked);
            if (linked == 0)
                throw new InvalidOperationException($"Shader link failed: {GL.GetProgramInfoLog(program)}");
            return program;
        }
        catch
        {
            GL.DeleteProgram(program);
            throw;
        }
        finally
        {
            GL.DeleteShader(vertex);
            if (fragment != 0) GL.DeleteShader(fragment);
        }
    }

    private static int Compile(ShaderType type, string source)
    {
        var shader = GL.CreateShader(type);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);
        GL.GetShader(shader, ShaderParameter.CompileStatus, out var compiled);
        if (compiled != 0) return shader;
        var message = GL.GetShaderInfoLog(shader);
        GL.DeleteShader(shader);
        throw new InvalidOperationException($"{type} failed: {message}");
    }

    public void Dispose()
    {
        GL.DeleteBuffer(_vertexBuffer);
        GL.DeleteVertexArray(_vertexArray);
        GL.DeleteProgram(_program);
    }
}
