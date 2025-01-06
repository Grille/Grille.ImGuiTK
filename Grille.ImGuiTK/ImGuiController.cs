using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
using ErrorCode = OpenTK.Graphics.OpenGL4.ErrorCode;
using Grille.ImGuiTK.GLSL;
using Grille.ImGuiTK.OpenGL;

namespace Grille.ImGuiTK;

public class ImGuiController : IDisposable
{
    private bool _frameBegun;

    private int _windowWidth;
    private int _windowHeight;

    private System.Numerics.Vector2 _scaleFactor = System.Numerics.Vector2.One;

    public GLRenderer Renderer { get; }
    /// <summary>
    /// Constructs a new ImGuiController.
    /// </summary>
    public ImGuiController()
    {
        Renderer = new();

        IntPtr context = ImGui.CreateContext();
        ImGui.SetCurrentContext(context);
        var io = ImGui.GetIO();
        io.Fonts.AddFontDefault();

        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
        // Enable Docking
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

        Renderer.Objects.CreateDeviceResources();

        SetPerFrameImGuiData(1f / 60f);

        ImGui.NewFrame();
        _frameBegun = true;
    }

    public void WindowResized(int width, int height)
    {
        _windowWidth = width;
        _windowHeight = height;
    }

    /// <summary>
    /// Renders the ImGui draw list data.
    /// </summary>
    public void Render()
    {
        if (_frameBegun)
        {
            _frameBegun = false;
            ImGui.Render();
            Renderer.RenderImDrawData();
        }
    }

    /// <summary>
    /// Updates ImGui input and IO configuration state.
    /// </summary>
    public void Update(GameWindow wnd, float deltaSeconds)
    {
        if (_frameBegun)
        {
            ImGui.Render();
        }

        SetPerFrameImGuiData(deltaSeconds);
        UpdateImGuiInput(wnd);

        _frameBegun = true;
        ImGui.NewFrame();
    }

    /// <summary>
    /// Sets per-frame data based on the associated window.
    /// This is called by Update(float).
    /// </summary>
    private void SetPerFrameImGuiData(float deltaSeconds)
    {
        ImGuiIOPtr io = ImGui.GetIO();
        io.DisplaySize = new System.Numerics.Vector2(
            _windowWidth / _scaleFactor.X,
            _windowHeight / _scaleFactor.Y);
        io.DisplayFramebufferScale = _scaleFactor;
        io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
    }

    readonly List<char> PressedChars = new List<char>();

    private void UpdateImGuiInput(GameWindow wnd)
    {
        ImGuiIOPtr io = ImGui.GetIO();

        MouseState MouseState = wnd.MouseState;
        KeyboardState KeyboardState = wnd.KeyboardState;

        io.MouseDown[0] = MouseState[MouseButton.Left];
        io.MouseDown[1] = MouseState[MouseButton.Right];
        io.MouseDown[2] = MouseState[MouseButton.Middle];
        io.MouseDown[3] = MouseState[MouseButton.Button4];
        io.MouseDown[4] = MouseState[MouseButton.Button5];

        var screenPoint = new Vector2i((int)MouseState.X, (int)MouseState.Y);
        var point = screenPoint;//wnd.PointToClient(screenPoint);
        io.MousePos = new System.Numerics.Vector2(point.X, point.Y);

        foreach (Keys key in Enum.GetValues(typeof(Keys)))
        {
            if (key == Keys.Unknown)
            {
                continue;
            }
            io.AddKeyEvent(TranslateKey(key), KeyboardState.IsKeyDown(key));
        }

        foreach (var c in PressedChars)
        {
            io.AddInputCharacter(c);
        }
        PressedChars.Clear();

        io.KeyCtrl = KeyboardState.IsKeyDown(Keys.LeftControl) || KeyboardState.IsKeyDown(Keys.RightControl);
        io.KeyAlt = KeyboardState.IsKeyDown(Keys.LeftAlt) || KeyboardState.IsKeyDown(Keys.RightAlt);
        io.KeyShift = KeyboardState.IsKeyDown(Keys.LeftShift) || KeyboardState.IsKeyDown(Keys.RightShift);
        io.KeySuper = KeyboardState.IsKeyDown(Keys.LeftSuper) || KeyboardState.IsKeyDown(Keys.RightSuper);
    }

    internal void PressChar(char keyChar)
    {
        PressedChars.Add(keyChar);
    }

    internal void MouseScroll(Vector2 offset)
    {
        ImGuiIOPtr io = ImGui.GetIO();

        io.MouseWheel = offset.Y;
        io.MouseWheelH = offset.X;
    }


    /// <summary>
    /// Frees all graphics resources used by the renderer.
    /// </summary>
    public void Dispose()
    {
        Renderer.Dispose();
    }

    public static ImGuiKey TranslateKey(Keys key)
    {
        if (key >= Keys.D0 && key <= Keys.D9)
            return key - Keys.D0 + ImGuiKey._0;

        if (key >= Keys.A && key <= Keys.Z)
            return key - Keys.A + ImGuiKey.A;

        if (key >= Keys.KeyPad0 && key <= Keys.KeyPad9)
            return key - Keys.KeyPad0 + ImGuiKey.Keypad0;

        if (key >= Keys.F1 && key <= Keys.F24)
            return key - Keys.F1 + ImGuiKey.F24;

        switch (key)
        {
            case Keys.Tab: return ImGuiKey.Tab;
            case Keys.Left: return ImGuiKey.LeftArrow;
            case Keys.Right: return ImGuiKey.RightArrow;
            case Keys.Up: return ImGuiKey.UpArrow;
            case Keys.Down: return ImGuiKey.DownArrow;
            case Keys.PageUp: return ImGuiKey.PageUp;
            case Keys.PageDown: return ImGuiKey.PageDown;
            case Keys.Home: return ImGuiKey.Home;
            case Keys.End: return ImGuiKey.End;
            case Keys.Insert: return ImGuiKey.Insert;
            case Keys.Delete: return ImGuiKey.Delete;
            case Keys.Backspace: return ImGuiKey.Backspace;
            case Keys.Space: return ImGuiKey.Space;
            case Keys.Enter: return ImGuiKey.Enter;
            case Keys.Escape: return ImGuiKey.Escape;
            case Keys.Apostrophe: return ImGuiKey.Apostrophe;
            case Keys.Comma: return ImGuiKey.Comma;
            case Keys.Minus: return ImGuiKey.Minus;
            case Keys.Period: return ImGuiKey.Period;
            case Keys.Slash: return ImGuiKey.Slash;
            case Keys.Semicolon: return ImGuiKey.Semicolon;
            case Keys.Equal: return ImGuiKey.Equal;
            case Keys.LeftBracket: return ImGuiKey.LeftBracket;
            case Keys.Backslash: return ImGuiKey.Backslash;
            case Keys.RightBracket: return ImGuiKey.RightBracket;
            case Keys.GraveAccent: return ImGuiKey.GraveAccent;
            case Keys.CapsLock: return ImGuiKey.CapsLock;
            case Keys.ScrollLock: return ImGuiKey.ScrollLock;
            case Keys.NumLock: return ImGuiKey.NumLock;
            case Keys.PrintScreen: return ImGuiKey.PrintScreen;
            case Keys.Pause: return ImGuiKey.Pause;
            case Keys.KeyPadDecimal: return ImGuiKey.KeypadDecimal;
            case Keys.KeyPadDivide: return ImGuiKey.KeypadDivide;
            case Keys.KeyPadMultiply: return ImGuiKey.KeypadMultiply;
            case Keys.KeyPadSubtract: return ImGuiKey.KeypadSubtract;
            case Keys.KeyPadAdd: return ImGuiKey.KeypadAdd;
            case Keys.KeyPadEnter: return ImGuiKey.KeypadEnter;
            case Keys.KeyPadEqual: return ImGuiKey.KeypadEqual;
            case Keys.LeftShift: return ImGuiKey.LeftShift;
            case Keys.LeftControl: return ImGuiKey.LeftCtrl;
            case Keys.LeftAlt: return ImGuiKey.LeftAlt;
            case Keys.LeftSuper: return ImGuiKey.LeftSuper;
            case Keys.RightShift: return ImGuiKey.RightShift;
            case Keys.RightControl: return ImGuiKey.RightCtrl;
            case Keys.RightAlt: return ImGuiKey.RightAlt;
            case Keys.RightSuper: return ImGuiKey.RightSuper;
            case Keys.Menu: return ImGuiKey.Menu;
            default: return ImGuiKey.None;
        }
    }
}