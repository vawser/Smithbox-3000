using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Smithbox.Core.Editor;


[JsonSourceGenerationOptions(WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata, IncludeFields = true)]
[JsonSerializable(typeof(CFG))]
internal partial class CfgSerializerContext : JsonSerializerContext
{
}

public class CFG
{
    public static CFG Current { get; private set; }
    public static CFG Default { get; } = new();

    /// <summary>
    /// Load config
    /// </summary>
    public static void Load()
    {
        var folder = GetConfigFolder();
        var file = Path.Combine(folder, "Configuration.json");

        if (!File.Exists(file))
        {
            MessageBox.Print("Configuration could not be loaded.");

            Current = new CFG();
            Save();
        }
        else
        {
            try
            {
                var filestring = File.ReadAllText(file);
                var options = new JsonSerializerOptions();
                Current = JsonSerializer.Deserialize(filestring, CfgSerializerContext.Default.CFG);

                if (Current == null)
                {
                    throw new Exception("JsonConvert returned null");
                }
            }
            catch (Exception e)
            {
                MessageBox.Print("Configuration could not be loaded.");

                Current = new CFG();
                Save();
            }
        }
    }

    private static void Save()
    {
        var folder = GetConfigFolder();
        var file = Path.Combine(folder, "Configuration.json");

        var json = JsonSerializer.Serialize(Current, CfgSerializerContext.Default.CFG);

        File.WriteAllText(file, json);
    }

    public static string GetConfigFolder()
    {
        var folder = $"{AppContext.BaseDirectory}/{Consts.DataFolder}/";

        return folder;
    }
}
