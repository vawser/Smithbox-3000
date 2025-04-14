using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Smithbox.Core.Interface;
using Smithbox.Core.Editor;
using System.Numerics;
using Smithbox.Core.Utils;

namespace Smithbox.Core.Interface;

public class UI
{
    public static UI Current { get; private set; }
    public static UI Default { get; } = new();

    public static void Setup()
    {
        Current = new UI();
    }

    public static void Load()
    {
        var folder = FolderUtils.GetConfigurationFolder();
        var file = Path.Combine(folder, "Interface.json");

        if (!File.Exists(file))
        {
            Current = new UI();
            Save();
        }
        else
        {
            try
            {
                var filestring = File.ReadAllText(file);
                var options = new JsonSerializerOptions();
                Current = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.UI);

                if (Current == null)
                {
                    throw new Exception("JsonConvert returned null");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Interface Configuration failed to load, default configuration has been restored.");

                Current = new UI();
                Save();
            }
        }
    }

    public static void Save()
    {
        var folder = FolderUtils.GetConfigurationFolder();
        var file = Path.Combine(folder, "Interface.json");

        var json = JsonSerializer.Serialize(Current, SmithboxSerializerContext.Default.UI);

        File.WriteAllText(file, json);
    }
    
    // Window
    public Vector4 ImGui_MainBg = new Vector4(0.176f, 0.176f, 0.188f, 1.0f);
    public Vector4 ImGui_ChildBg = new Vector4(0.145f, 0.145f, 0.149f, 1.0f);
    public Vector4 ImGui_PopupBg = new Vector4(0.106f, 0.106f, 0.110f, 1.0f);
    public Vector4 ImGui_Border = new Vector4(0.247f, 0.247f, 0.275f, 1.0f);
    public Vector4 ImGui_TitleBarBg = new Vector4(0.176f, 0.176f, 0.188f, 1.0f);
    public Vector4 ImGui_TitleBarBg_Active = new Vector4(0.176f, 0.176f, 0.188f, 1.0f);
    public Vector4 ImGui_MenuBarBg = new Vector4(0.176f, 0.176f, 0.188f, 1.0f);

    // Scroll
    public Vector4 ImGui_ScrollbarBg = new Vector4(0.243f, 0.243f, 0.249f, 1.0f);
    public Vector4 ImGui_ScrollbarGrab = new Vector4(0.408f, 0.408f, 0.408f, 1.0f);
    public Vector4 ImGui_ScrollbarGrab_Hover = new Vector4(0.635f, 0.635f, 0.635f, 1.0f);
    public Vector4 ImGui_ScrollbarGrab_Active = new Vector4(1.000f, 1.000f, 1.000f, 1.0f);
    public Vector4 ImGui_SliderGrab = new Vector4(0.635f, 0.635f, 0.635f, 1.0f);
    public Vector4 ImGui_SliderGrab_Active = new Vector4(1.000f, 1.000f, 1.000f, 1.0f);

    // Tab
    public Vector4 ImGui_Tab = new Vector4(0.176f, 0.176f, 0.188f, 1.0f);
    public Vector4 ImGui_Tab_Hover = new Vector4(0.110f, 0.592f, 0.918f, 1.0f);
    public Vector4 ImGui_Tab_Active = new Vector4(0.200f, 0.600f, 1.000f, 1.0f);
    public Vector4 ImGui_UnfocusedTab = new Vector4(0.176f, 0.176f, 0.188f, 1.0f);
    public Vector4 ImGui_UnfocusedTab_Active = new Vector4(0.247f, 0.247f, 0.275f, 1.0f);

    // Button
    public Vector4 ImGui_Button = new Vector4(0.176f, 0.176f, 0.188f, 1.0f);
    public Vector4 ImGui_Button_Hovered = new Vector4(0.247f, 0.247f, 0.275f, 1.0f);
    public Vector4 ImGui_ButtonActive = new Vector4(0.200f, 0.600f, 1.000f, 1.0f);

    // Selection
    public Vector4 ImGui_Selection = new Vector4(0.087f, 0.296f, 0.437f, 1.000f);
    public Vector4 ImGui_Selection_Hover = new Vector4(0.247f, 0.247f, 0.275f, 1.0f);
    public Vector4 ImGui_Selection_Active = new Vector4(0.161f, 0.550f, 0.939f, 1.0f);

    // Input 
    public Vector4 ImGui_Input_Background = new Vector4(0.200f, 0.200f, 0.216f, 1.0f);
    public Vector4 ImGui_Input_Background_Hover = new Vector4(0.247f, 0.247f, 0.275f, 1.0f);
    public Vector4 ImGui_Input_Background_Active = new Vector4(0.200f, 0.200f, 0.216f, 1.0f);
    public Vector4 ImGui_Input_CheckMark = new Vector4(1.000f, 1.000f, 1.000f, 1.0f);
    public Vector4 ImGui_Input_Conflict_Background = new Vector4(0.25f, 0.2f, 0.2f, 1.0f);
    public Vector4 ImGui_Input_Vanilla_Background = new Vector4(0.2f, 0.22f, 0.2f, 1.0f);
    public Vector4 ImGui_Input_Default_Background = new Vector4(0.180f, 0.180f, 0.196f, 1.0f);
    public Vector4 ImGui_Input_AuxVanilla_Background = new Vector4(0.2f, 0.2f, 0.35f, 1.0f);
    public Vector4 ImGui_Input_DiffCompare_Background = new Vector4(0.2f, 0.2f, 0.35f, 1.0f);
    public Vector4 ImGui_MultipleInput_Background = new Vector4(0.0f, 0.5f, 0.0f, 0.1f);
    public Vector4 ImGui_ErrorInput_Background = new Vector4(0.8f, 0.2f, 0.2f, 1.0f);

    // Text
    public Vector4 ImGui_Default_Text_Color = new Vector4(0.9f, 0.9f, 0.9f, 1.0f);
    public Vector4 ImGui_Warning_Text_Color = new Vector4(1.0f, 0f, 0f, 1.0f);
    public Vector4 ImGui_Benefit_Text_Color = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
    public Vector4 ImGui_Invalid_Text_Color = new Vector4(1.0f, 0.3f, 0.3f, 1.0f);
    public Vector4 ImGui_Highlight_Text = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);

    // Logger
    public Vector4 ImGui_Logger_Information_Color = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
    public Vector4 ImGui_Logger_Warning_Color = new Vector4(1.0f, 0f, 0f, 1.0f);
    public Vector4 ImGui_Logger_Error_Color = new Vector4(1.0f, 0.5f, 0.5f, 1.0f);
}
