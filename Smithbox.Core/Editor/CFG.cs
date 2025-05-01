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

    /// <summary>
    /// The path to the user's Mod Engine 2 exe
    /// </summary>
    public string ModEngineInstall = "";

    /// <summary>
    /// The dll arguments to use with the Mod Engine 2 launch
    /// </summary>
    public string ModEngineDlls = "";

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
    /// If true, the windows can be moved.
    /// </summary>
    public bool AllowInterfaceMovement = false;

    /// <summary>
    /// If true, the Projects window is displayed
    /// </summary>
    public bool DisplayProjectListWindow = true;

    /// <summary>
    /// If true, the Project window is displayed
    /// </summary>
    public bool DisplayProjectWindow = true;

    /// <summary>
    /// The display scaling to apply to the Interface
    /// </summary>
    public float InterfaceDisplayScale = 1.0f;

    /// <summary>
    /// If true, the Project window is displayed
    /// </summary>
    public bool DisplaySecondaryParamEditor = false;

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

    /// <summary>
    /// If true, then community field names will be used. If false, the internal field names will be used.
    /// </summary>
    public bool DisplayCommunityFieldNames = true;

    /// <summary>
    /// If true, then the vanilla field columns is displayed
    /// </summary>
    public bool DisplayVanillaColumns = true;

    /// <summary>
    /// If true, then the aux field columns is displayed
    /// </summary>
    public bool DisplayAuxColumns = true;

    /// <summary>
    /// If true, then the field offset columns is displayed
    /// </summary>
    public bool DisplayOffsetColumn = true;

    /// <summary>
    /// If true, then the field type columns is displayed
    /// </summary>
    public bool DisplayTypeColumn = true;

    /// <summary>
    /// If true, then the field type columns is displayed
    /// </summary>
    public bool DisplayInformationColumn = true;

    /// <summary>
    /// The length of string after which the column text is truncated
    /// </summary>
    public int ParamFieldColumnTruncationLength = 30;

    /// <summary>
    /// The width of the ImGui input widgets in the field column
    /// </summary>
    public float ParamFieldInputWidth = 250f;

    /// <summary>
    /// If true, the Image Preview window is visible
    /// </summary>
    public bool DisplayParamImageView = true;

    /// <summary>
    /// If true, the padding fields are displayed
    /// </summary>
    public bool DisplayPaddingFields = true;

    /// <summary>
    /// The default offset to apply to duplicated rows
    /// </summary>
    public int ParamRowDuplicateOffset = 1;

    //----------------------------------
    // Model Editor
    //----------------------------------

    //----------------------------------
    // Behavior Editor
    //----------------------------------
    /// <summary>
    /// The width of the ImGui input widgets in the field column
    /// </summary>
    public float BehaviorFieldInputWidth = 250f;

    //----------------------------------
    // Collision Editor
    //----------------------------------
    /// <summary>
    /// The width of the ImGui input widgets in the field column
    /// </summary>
    public float CollisionFieldInputWidth = 250f;

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
