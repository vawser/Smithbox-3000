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
    /// <summary>
    /// If true, file reads will ignore failed asserts
    /// </summary>
    public bool IgnoreReadAsserts = false;

    /// <summary>
    /// If true, verbose logging will be enabled.
    /// </summary>
    public bool EnableVerboseLogging = true;

    //----------------------------------
    // Viewport
    //----------------------------------
    /// <summary>
    /// If true, the mouse free look will use inverted Y.
    /// </summary>
    public bool UseInvertedControls = false;

    /// <summary>
    /// The look sensitivity for the free look.
    /// </summary>
    public float LookSensitivity = 0.005f;

    /// <summary>
    /// The base speed for free look movement
    /// </summary>
    public float FreeLookBaseSpeed = 10.0f;

    /// <summary>
    /// The speed multiplier for free look movement in 'Fast' mode.
    /// </summary>
    public float FreeLookFastSpeed = 4.0f;

    /// <summary>
    /// The speed multiplier for free look movement in 'Slow' mode.
    /// </summary>
    public float FreeLookSlowSpeed = 0.25f;

    //----------------------------------
    // Interface
    //----------------------------------
    /// <summary>
    /// If true, the Script Console window is displayed
    /// </summary>
    public bool DisplayScriptConsoleWindow = true;

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
    /// If true, the Primary Model Editor window is displayed
    /// </summary>
    public bool DisplayPrimaryModelEditor = true;

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

    //----------------------------------
    // Param Editor
    //----------------------------------
    /// <summary>
    /// If true, the project Meta files are used instead of the primary Meta files.
    /// </summary>
    public bool UseProjectMeta = false;

    /// <summary>
    /// If true, then loose params are prioitized over packed params (for games where it is relevant)
    /// </summary>
    public bool UseLooseParams = false;

    /// <summary>
    /// If true, then loose params are repacked into the enc_regulation for DS2 projects.
    /// </summary>
    public bool RepackLooseDS2Params = false;

    /// <summary>
    /// If true, then row name restore will use index matching. If false, it will use ID matching.
    /// </summary>
    public bool UseIndexMatchForRowNameRestore = true;


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
                TaskLogs.AddLog("[Smithbox] Configuration failed to load, default configuration has been restored.");

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
