using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace Grille.ImGuiTK.OpenGL;

public unsafe class GLState
{
    int prevVAO;
    int prevArrayBuffer;
    int prevProgram;
    bool prevBlendEnabled;
    bool prevScissorTestEnabled;
    BlendEquationMode prevBlendEquationRgb;
    BlendEquationMode prevBlendEquationAlpha;
    BlendingFactorSrc prevBlendFuncSrcRgb;
    BlendingFactorSrc prevBlendFuncSrcAlpha;
    BlendingFactorDest prevBlendFuncDstRgb;
    BlendingFactorDest prevBlendFuncDstAlpha;
    bool prevCullFaceEnabled;
    bool prevDepthTestEnabled;
    TextureUnit prevActiveTextureUnit;
    int prevTexture02D;
    readonly int[] prevScissorBox;
    readonly int[] prevPolygonMode;

    readonly int GLVersion;
    readonly bool CompatibilityProfile;

    public bool StateBackupEnabled { get; set; }



    public GLState()
    {
        prevScissorBox = new int[4];
        prevPolygonMode = new int[2];

        StateBackupEnabled = true;

        int major = GL.GetInteger(GetPName.MajorVersion);
        int minor = GL.GetInteger(GetPName.MinorVersion);

        GLVersion = major * 100 + minor * 10;
        CompatibilityProfile = (GL.GetInteger((GetPName)All.ContextProfileMask) & (int)All.ContextCompatibilityProfileBit) != 0;

    }

    public void Backup()
    {
        if (!StateBackupEnabled)
        {
            return;
        }

        prevVAO = GL.GetInteger(GetPName.VertexArrayBinding);
        prevArrayBuffer = GL.GetInteger(GetPName.ArrayBufferBinding);
        prevProgram = GL.GetInteger(GetPName.CurrentProgram);
        prevBlendEnabled = GL.GetBoolean(GetPName.Blend);
        prevScissorTestEnabled = GL.GetBoolean(GetPName.ScissorTest);
        prevBlendEquationRgb = (BlendEquationMode)GL.GetInteger(GetPName.BlendEquationRgb);
        prevBlendEquationAlpha = (BlendEquationMode)GL.GetInteger(GetPName.BlendEquationAlpha);
        prevBlendFuncSrcRgb = (BlendingFactorSrc)GL.GetInteger(GetPName.BlendSrcRgb);
        prevBlendFuncSrcAlpha = (BlendingFactorSrc)GL.GetInteger(GetPName.BlendSrcAlpha);
        prevBlendFuncDstRgb = (BlendingFactorDest)GL.GetInteger(GetPName.BlendDstRgb);
        prevBlendFuncDstAlpha = (BlendingFactorDest)GL.GetInteger(GetPName.BlendDstAlpha);
        prevCullFaceEnabled = GL.GetBoolean(GetPName.CullFace);
        prevDepthTestEnabled = GL.GetBoolean(GetPName.DepthTest);
        prevActiveTextureUnit = (TextureUnit)GL.GetInteger(GetPName.ActiveTexture);
        GL.ActiveTexture(TextureUnit.Texture0);
        prevTexture02D = GL.GetInteger(GetPName.TextureBinding2D);
        fixed (int* iptr = prevScissorBox)
        {
            GL.GetInteger(GetPName.ScissorBox, iptr);
        }
        fixed (int* iptr = prevPolygonMode)
        {
            GL.GetInteger(GetPName.PolygonMode, iptr);
        }
    }

    public void Setup()
    {
        Backup();

        if (GLVersion <= 310 || CompatibilityProfile)
        {
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
            GL.PolygonMode(MaterialFace.Back, PolygonMode.Fill);
        }
        else
        {
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }

        GL.Enable(EnableCap.Blend);
        GL.Enable(EnableCap.ScissorTest);
        GL.BlendEquation(BlendEquationMode.FuncAdd);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Disable(EnableCap.CullFace);
        GL.Disable(EnableCap.DepthTest);
    }

    public void Restore()
    {
        if (!StateBackupEnabled)
        {
            return;
        }

        // Reset state
        GL.BindTexture(TextureTarget.Texture2D, prevTexture02D);
        GL.ActiveTexture(prevActiveTextureUnit);
        GL.UseProgram(prevProgram);
        GL.BindVertexArray(prevVAO);
        GL.Scissor(prevScissorBox[0], prevScissorBox[1], prevScissorBox[2], prevScissorBox[3]);
        GL.BindBuffer(BufferTarget.ArrayBuffer, prevArrayBuffer);
        GL.BlendEquationSeparate(prevBlendEquationRgb, prevBlendEquationAlpha);
        GL.BlendFuncSeparate(prevBlendFuncSrcRgb, prevBlendFuncDstRgb, prevBlendFuncSrcAlpha, prevBlendFuncDstAlpha);

        void Set(EnableCap cap, bool value)
        {
            if (value) GL.Enable(cap); else GL.Disable(cap);
        }

        Set(EnableCap.Blend, prevBlendEnabled);
        Set(EnableCap.DepthTest, prevDepthTestEnabled);
        Set(EnableCap.CullFace, prevCullFaceEnabled);
        Set(EnableCap.ScissorTest, prevScissorTestEnabled);

        if (GLVersion <= 310 || CompatibilityProfile)
        {
            GL.PolygonMode(MaterialFace.Front, (PolygonMode)prevPolygonMode[0]);
            GL.PolygonMode(MaterialFace.Back, (PolygonMode)prevPolygonMode[1]);
        }
        else
        {
            GL.PolygonMode(MaterialFace.FrontAndBack, (PolygonMode)prevPolygonMode[0]);
        }
    }
}
