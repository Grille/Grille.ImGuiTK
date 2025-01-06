using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Grille.ImGuiTK.GLSL;

public static class ShaderCode
{
    public static string FragmentSource => GetText("main.frag");
    public static string VertexSource => GetText("main.vert");

    private static string GetText(string name)
    {
        var type = typeof(ShaderCode);
        var asm = type.Assembly;
        var path = $"{type.Namespace}.{name}";
        using var stream = asm.GetManifestResourceStream(path);
        using var reader = new StreamReader(stream, leaveOpen: true);
        return reader.ReadToEnd();
    }
}
