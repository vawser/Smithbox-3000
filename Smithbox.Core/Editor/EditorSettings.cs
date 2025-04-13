using Hexa.NET.ImGui;
using Smithbox.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.Editor;

public static class EditorSettings
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
            ImGui.OpenPopup("Editor Settings");
            Display = false;
        }

        var inputWidth = 400.0f;

        var viewport = ImGui.GetMainViewport();
        Vector2 center = viewport.Pos + viewport.Size / 2;

        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

        ImGui.SetNextWindowSize(new Vector2(1200, 800), ImGuiCond.Always);

        if (ImGui.BeginPopupModal("Editor Settings", ref Open, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove))
        {
            if (ImGui.CollapsingHeader("General"))
            {
                // Ignore Read Asserts
                ImGui.Checkbox("Ignore Read Asserts##ignoreReadAsserts", ref CFG.Current.IgnoreReadAsserts);
                UIHelper.Tooltip("If true, file reads will ignore failed asserts. Useful for loading files that have been 'corrupted' intentionally.");

                // Enable Verbose Logging
                ImGui.Checkbox("Enable Verbose Logging##enableVerboseLogging", ref CFG.Current.EnableVerboseLogging);
                UIHelper.Tooltip("If true, berbose logging will be displayed in the general logger.");
            }

            if (FeatureFlags.IncludeParamEditor)
            {
                if(ImGui.CollapsingHeader("Param Editor"))
                {
                    // Prioritize Loose Params
                    ImGui.Checkbox("Prioritize Loose Params##useLooseParams", ref CFG.Current.UseLooseParams);
                    UIHelper.Tooltip("If true, then loose params will be prioritized over packed params.");
                    
                }
            }

            ImGui.EndPopup();
        }
    }
}
