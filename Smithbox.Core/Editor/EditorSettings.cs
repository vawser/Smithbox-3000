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
            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                // Ignore Read Asserts
                ImGui.Checkbox("Ignore Read Asserts##ignoreReadAsserts", ref CFG.Current.IgnoreReadAsserts);
                UIHelper.Tooltip("If true, file reads will ignore failed asserts. Useful for loading files that have been 'corrupted' intentionally.");

                // Enable Verbose Logging
                ImGui.Checkbox("Enable Verbose Logging##enableVerboseLogging", ref CFG.Current.EnableVerboseLogging);
                UIHelper.Tooltip("If true, berbose logging will be displayed in the general logger.");
            }

            if (ImGui.CollapsingHeader("Mod Engine", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.InputText("Executable Path##modEnginePath", ref CFG.Current.ModEngineInstall, 255);
                UIHelper.Tooltip("Select the modengine2_launcher.exe within your ModEngine2 install folder.");

                ImGui.SameLine();

                if (ImGui.Button("Select##modEnginePathSelect"))
                {
                    using (var dialog = new OpenFileDialog())
                    {
                        DialogResult result = dialog.ShowDialog();

                        if (result == DialogResult.OK)
                        {
                            if(dialog.FileName.Contains("modengine2_launcher.exe"))
                            {
                                CFG.Current.ModEngineInstall = dialog.FileName;
                            }
                            else
                            {
                                MessageBox.Show("The file you selected was not modengine2_launcher.exe");
                            }
                        }
                    }
                }

                ImGui.InputText("DLL Entries##modEngineDllEntries", ref CFG.Current.ModEngineDlls, 255);
                UIHelper.Tooltip("The relative paths of the DLLs to include in the 'Launch Mod' action. Separate them by a space if using multiple.");
            }

            if (FeatureFlags.IncludeParamEditor)
            {
                if(ImGui.CollapsingHeader("Param Editor", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    // Prioritize Loose Params
                    ImGui.Checkbox("Prioritize Loose Params##useLooseParams", ref CFG.Current.UseLooseParams);
                    UIHelper.Tooltip("If true, then loose params will be prioritized over packed params.");

                    ImGui.InputInt("Field Name Column Width##paramFieldWidth", ref CFG.Current.ParamFieldColumnTruncationLength);
                    UIHelper.Tooltip("The width of the param field name column.");

                    ImGui.DragFloat("Field Input Column Width##paramFieldInputWidth", ref CFG.Current.ParamFieldInputWidth);
                    UIHelper.Tooltip("The width of the param field input column.");
                }
            }

            if (FeatureFlags.IncludeModelEditor)
            {
                if (ImGui.CollapsingHeader("Viewport", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    // Use Inverted Controls
                    ImGui.Checkbox("Use Inverted Controls", ref CFG.Current.UseInvertedControls);
                    UIHelper.Tooltip("If true, then the mouse look will use inverted Y.");

                    // Look Sensitivity
                    var curLookSensitivity = CFG.Current.LookSensitivity;
                    ImGui.DragFloat("Look Sensitivity##viewportLookSensitivity", ref curLookSensitivity);
                    if (ImGui.IsItemDeactivatedAfterEdit())
                    {
                        CFG.Current.LookSensitivity = curLookSensitivity;
                    }
                    UIHelper.Tooltip("The look sensitivity of the free look mode in the viewport.");

                    // Free Look: Base Speed
                    var curBaseSpeed = CFG.Current.FreeLookBaseSpeed;
                    ImGui.DragFloat("Free Look: Base Speed##viewportBaseSpeed", ref curBaseSpeed);
                    if (ImGui.IsItemDeactivatedAfterEdit())
                    {
                        CFG.Current.FreeLookBaseSpeed = curBaseSpeed;
                    }
                    UIHelper.Tooltip("The base movement speed of the camera in free look mode.");

                    // Free Look: Fast Multiplier
                    var curFastMultiplier = CFG.Current.FreeLookFastSpeed;
                    ImGui.DragFloat("Free Look: Fast Speed Multiplier##viewportFastMultiplier", ref curFastMultiplier);
                    if (ImGui.IsItemDeactivatedAfterEdit())
                    {
                        CFG.Current.FreeLookFastSpeed = curFastMultiplier;
                    }
                    UIHelper.Tooltip("The multiplier to apply to the movement speed of the camera when in 'Fast' mode.");

                    // Free Look: Slow Multiplier
                    var curSlowMultiplier = CFG.Current.FreeLookSlowSpeed;
                    ImGui.DragFloat("Free Look: Slow Speed Multiplier##viewportSlowMultiplier", ref curSlowMultiplier);
                    if (ImGui.IsItemDeactivatedAfterEdit())
                    {
                        CFG.Current.FreeLookSlowSpeed = curSlowMultiplier;
                    }
                    UIHelper.Tooltip("The multiplier to apply to the movement speed of the camera when in 'Slow' mode.");

                }
            }

            ImGui.EndPopup();
        }
    }
}
