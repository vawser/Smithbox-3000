using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.Interface;

public static class ColorPicker
{
    private static bool Display = false;
    private static bool Open = false;

    private static string Message = "";

    private static Vector4 Color = new Vector4(1f, 1f, 1f, 1f); // Store RGBA as Vector4

    public unsafe static void Draw()
    {
        if (Display)
        {
            Open = true;
            ImGui.OpenPopup("Color Picker");
            Display = false;
        }

        if (ImGui.BeginPopupModal("Color Picker", ref Open, ImGuiWindowFlags.AlwaysAutoResize))
        {
            Vector4 color = Color;

            var flags = ImGuiColorEditFlags.DisplayHex | ImGuiColorEditFlags.DisplayHsv | ImGuiColorEditFlags.DisplayRgb;

            ImGui.ColorPicker4("colorPicker", (float*)&color, flags);

            Color = color;
            UI.Current.ImGui_Button = color;

            ImGui.EndPopup();
        }
    }

    public static void Show()
    {
        Display = true;
    }
}