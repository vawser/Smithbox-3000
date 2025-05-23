﻿using Hexa.NET.ImGui;
using Smithbox.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.Interface;

public static class InterfaceSettings
{
    private static bool Display = false;
    private static bool Open = false;
    public static bool Commit = false;

    public static void Setup()
    {

    }

    public static void Show()
    {
        Display = true;
    }

    public static void Draw()
    {
        if (Display)
        {
            Open = true;
            ImGui.OpenPopup("Interface Settings");
            Display = false;
        }

        var inputWidth = 400.0f;

        var viewport = ImGui.GetMainViewport();
        Vector2 center = viewport.Pos + viewport.Size / 2;

        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

        ImGui.SetNextWindowSize(new Vector2(1200, 800), ImGuiCond.Always);

        if (ImGui.BeginPopupModal("Interface Settings", ref Open, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove))
        {
            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                var curDisplayScale = CFG.Current.InterfaceDisplayScale;
                ImGui.DragFloat("Display Scale##interfaceDisplayScale", ref curDisplayScale);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    CFG.Current.InterfaceDisplayScale = curDisplayScale;
                }
                UIHelper.Tooltip("The display scale to use for the interface.");

                // Scale by DPI
                ImGui.Checkbox("Scale by DPI##scaleByDPI", ref CFG.Current.ScalebyDPI);
                UIHelper.Tooltip("If true, the interface display scaling with account for DPI.");

                // Word Wrap Aliases
                ImGui.Checkbox("Word Wrap Aliases##wordWrapAliases", ref CFG.Current.WrapAliasDisplay);
                UIHelper.Tooltip("If true, alias text (e.g. the name of the map next to its ID) will wrap if it exceeds the border of the window, rather than being truncated.");

                // Allow Window Movement
                ImGui.Checkbox("Allow Window Movement##allowWindowMovement", ref CFG.Current.AllowInterfaceMovement);
                UIHelper.Tooltip("If true, the user can drag the windows around.");
            }

            if (ImGui.CollapsingHeader("Coloring", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGui.Button("Reset Coloring to Default"))
                {
                    UI.ResetToDefault();
                }
            }

            if (ImGui.CollapsingHeader("Font", ImGuiTreeNodeFlags.DefaultOpen))
            {
                var curFontSize = ImGuiCFG.Current.FontSize;
                ImGui.DragFloat("Font Size##fontSize", ref curFontSize);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    ImGuiCFG.Current.FontSize = curFontSize;
                    
                    // TODO: need to work out how to rebuild the fonts properly
                    //ImGuiManager.RebuildFonts = true;
                }
                UIHelper.Tooltip("The font size to use.");
            }

            if (ImGui.CollapsingHeader("Logger", ImGuiTreeNodeFlags.DefaultOpen))
            {
                // Display General Logger
                ImGui.Checkbox("Display General Logger##displayGeneralLogger", ref CFG.Current.DisplayGeneralLogger);
                UIHelper.Tooltip("If true, the General Logger preview and window will be visible.");

                // Display Warning Logger
                ImGui.Checkbox("Display Warning Logger##displayWarningLogger", ref CFG.Current.DisplayWarningLogger);
                UIHelper.Tooltip("If true, the Warning Logger preview and window will be visible.");
            }

            ImGui.EndPopup();
        }
    }
}
