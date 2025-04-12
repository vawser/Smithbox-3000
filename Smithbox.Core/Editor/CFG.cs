using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Smithbox.Core.Interface;
using Smithbox.Core.Utils;

namespace Smithbox.Core.Editor;

public class CFG
{
    //----------------------------------
    // Project
    //----------------------------------

    //----------------------------------
    // Interface
    //----------------------------------
    /// <summary>
    /// If true, the Projects window is displayed
    /// </summary>
    public bool DisplayProjectListWindow = true;

    /// <summary>
    /// If true, the Project window is displayed
    /// </summary>
    public bool DisplayProjectWindow = true;

    /// <summary>
    /// If true, the File Browser window is displayed
    /// </summary>
    public bool DisplayFileBrowser = true;

    /// <summary>
    /// If true, the Primary Param Editor window is displayed
    /// </summary>
    public bool DisplayPrimaryParamEditor = true;

    /// <summary>
    /// If true, the Secondary Param Editor window is displayed
    /// </summary>
    public bool DisplaySecondaryParamEditor = true;

    /// <summary>
    /// The display scaling to apply to the Interface
    /// </summary>
    public float InterfaceDisplayScale = 1.0f;

    /// <summary>
    /// If true, the interface display scaling with account for DPI
    /// </summary>
    public bool ScalebyDPI = true;

    /// <summary>
    /// If true, alias text will wrap rather than be truncated at window edge.
    /// </summary>
    public bool WrapAliasDisplay = false;

    /// <summary>
    /// If true, the General Logger is displayed.
    /// </summary>
    public bool DisplayGeneralLogger = true;

    /// <summary>
    /// If true, the Warning Logger is displayed.
    /// </summary>
    public bool DisplayWarningLogger = true;

    /// <summary>
    /// The duration after which the General logger preview will fade out
    /// </summary>
    public float GeneralLoggerPreviewFadeTime = 30.0f;

    /// <summary>
    /// The duration after which the Warning logger preview will fade out
    /// </summary>
    public float WarningLoggerPreviewFadeTime = 30.0f;

    //----------------------------------
    // Param Editor
    //----------------------------------
    /// <summary>
    /// If true, the project Meta files are used instead of the primary Meta files.
    /// </summary>
    public bool UseProjectMeta = false;


    //----------------------------------
    /// General
    //----------------------------------
    public static CFG Current { get; private set; }
    public static CFG Default { get; } = new();

    public static void Setup()
    {
        Current = new CFG();
    }

    public static void Load()
    {
        var folder = FolderUtils.GetConfigurationFolder();
        var file = Path.Combine(folder, "Configuration.json");

        if (!File.Exists(file))
        {
            MessageBox.Print("Default configuration set.");

            Current = new CFG();
            Save();
        }
        else
        {
            try
            {
                var filestring = File.ReadAllText(file);
                var options = new JsonSerializerOptions();
                Current = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.CFG);

                if (Current == null)
                {
                    throw new Exception("JsonConvert returned null");
                }
            }
            catch (Exception e)
            {
                MessageBox.Print("Configuration failed to load, default configuration has been restored.");

                Current = new CFG();
                Save();
            }
        }
    }

    public static void Save()
    {
        var folder = FolderUtils.GetConfigurationFolder();
        var file = Path.Combine(folder, "Configuration.json");

        var json = JsonSerializer.Serialize(Current, SmithboxSerializerContext.Default.CFG);

        File.WriteAllText(file, json);
    }
}
