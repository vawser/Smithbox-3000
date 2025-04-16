using Smithbox.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Smithbox.Core.Interface;


public class ImGuiCFG
{
    //----------------------------------
    // Font
    //----------------------------------
    /// <summary>
    /// The size of the font glyphs
    /// </summary>
    public float FontSize = 14.0f;

    public static ImGuiCFG Current { get; private set; }
    public static ImGuiCFG Default { get; } = new();

    public static void Setup()
    {
        Current = new ImGuiCFG();
    }

    public static void Load()
    {
        var folder = FolderUtils.GetConfigurationFolder();
        var file = Path.Combine(folder, "ImGui.json");

        if (!File.Exists(file))
        {
            Current = new ImGuiCFG();
            Save();
        }
        else
        {
            try
            {
                var filestring = File.ReadAllText(file);
                var options = new JsonSerializerOptions();
                Current = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.ImGuiCFG);

                if (Current == null)
                {
                    throw new Exception("JsonConvert returned null");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] ImGui Configuration failed to load, default configuration has been restored.");

                Current = new ImGuiCFG();
                Save();
            }
        }
    }

    public static void Save()
    {
        var folder = FolderUtils.GetConfigurationFolder();
        var file = Path.Combine(folder, "ImGui.json");

        if(!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var json = JsonSerializer.Serialize(Current, SmithboxSerializerContext.Default.ImGuiCFG);

        File.WriteAllText(file, json);
    }
}