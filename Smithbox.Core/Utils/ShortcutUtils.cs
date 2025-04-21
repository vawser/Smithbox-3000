using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.Utils;

public class ShortcutUtils
{
    public static bool UpdateShortcutDetection()
    {
        if (ImGui.IsWindowFocused(ImGuiFocusedFlags.RootWindow | ImGuiFocusedFlags.ChildWindows))
        {
            return true;
        }

        return false;
    }
}
