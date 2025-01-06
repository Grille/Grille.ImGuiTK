using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ErrorCode = OpenTK.Graphics.OpenGL4.ErrorCode;

using ImGuiNET;

namespace Grille.ImGuiTK.OpenGL;

public class GLRenderer : IDisposable
{
    public GLObjects Objects { get; }
    public GLState State { get; }

    public bool DebugPrintEnabled { get; set; }

    public GLRenderer()
    {
        Objects = new();
        State = new();
    }

    public void RenderImDrawData()
    {
        RenderImDrawData(ImGui.GetDrawData());
    }

    public void RenderImDrawData(ImDrawDataPtr draw_data)
    {
        if (draw_data.CmdListsCount == 0)
        {
            return;
        }
      
        State.Setup();
        Objects.Bind();
        Objects.GuaranteeBufferSize(draw_data);

        // Setup orthographic projection matrix into our constant buffer
        ImGuiIOPtr io = ImGui.GetIO();
        Objects.UpdateShader(io);
        CheckGLError("Projection");

        draw_data.ScaleClipRects(io.DisplayFramebufferScale);

        // Render command lists
        for (int n = 0; n < draw_data.CmdListsCount; n++)
        {
            ImDrawListPtr cmd_list = draw_data.CmdLists[n];

            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), cmd_list.VtxBuffer.Data);
            CheckGLError($"Data Vert {n}");

            GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, cmd_list.IdxBuffer.Size * sizeof(ushort), cmd_list.IdxBuffer.Data);
            CheckGLError($"Data Idx {n}");

            for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
            {
                ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
                if (pcmd.UserCallback != IntPtr.Zero)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, (int)pcmd.TextureId);
                    CheckGLError("Texture");

                    // We do _windowHeight - (int)clip.W instead of (int)clip.Y because gl has flipped Y when it comes to these coordinates
                    var clip = pcmd.ClipRect;
                    GL.Scissor((int)clip.X, (int)draw_data.DisplaySize.Y - (int)clip.W, (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));
                    CheckGLError("Scissor");

                    if ((io.BackendFlags & ImGuiBackendFlags.RendererHasVtxOffset) != 0)
                    {
                        GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(pcmd.IdxOffset * sizeof(ushort)), unchecked((int)pcmd.VtxOffset));
                    }
                    else
                    {
                        GL.DrawElements(BeginMode.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (int)pcmd.IdxOffset * sizeof(ushort));
                    }
                    CheckGLError("Draw");
                }
            }
        }

        State.Restore();

        CheckGLError("End of frame");
    }

    void CheckGLError(string title)
    {
        if (!DebugPrintEnabled)
        {
            return;
        }

        ErrorCode error;
        int i = 1;
        while ((error = GL.GetError()) != ErrorCode.NoError)
        {
            Debug.Print($"{title} ({i++}): {error}");
        }
    }

    public void Dispose()
    {
        Objects.Dispose();
    }
}
