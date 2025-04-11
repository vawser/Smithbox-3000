using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.Editor;

public static class MessageBox
{
    private static bool Display = false;
    private static bool Open = false;

    private static string Message = "";

    public static void Draw()
    {
        if(Display)
        {
            Open = true;
            ImGui.OpenPopup("Message");
            Display = false;
        }

        var viewport = ImGui.GetMainViewport();
        Vector2 center = viewport.Pos + viewport.Size / 2;

        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

        ImGui.SetNextWindowSize(new Vector2(640, 400), ImGuiCond.Always);

        if (ImGui.BeginPopupModal("Message", ref Open, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove))
        {
            ImGui.Text(Message);

            if(ImGui.Button("Close"))
            {
                Open = false;
            }

            ImGui.EndPopup();
        }
    }

    public static void Print(string text, string title = "")
    {
        Display = true;
        Message = text;
    }
}
