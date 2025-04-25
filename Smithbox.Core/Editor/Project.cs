using Andre.IO.VFS;
using Hexa.NET.ImGui;
using Smithbox.Core.BehaviorEditorNS;
using Smithbox.Core.CollisionEditorNS;
using Smithbox.Core.FileBrowserNS;
using Smithbox.Core.Interface;
using Smithbox.Core.MapEditorNS;
using Smithbox.Core.ModelEditorNS;
using Smithbox.Core.ParamEditorNS;
using Smithbox.Core.Resources;
using Smithbox.Core.Utils;
using System.Diagnostics;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Smithbox.Core.Editor;

/// <summary>
/// This is the project-level handler, includes all data and editors for the project
/// </summary>
public class Project
{
    public Guid ProjectGUID;
    public string ProjectName;
    public string ProjectPath;
    public string DataPath;
    public ProjectType ProjectType;

    /// <summary>
    /// Tracked so we only ever import param row names automatically once.
    /// </summary>
    public bool ImportedParamRowNames;

    /// <summary>
    /// Tracked so we know when to apply Param Row stripping, and if to restore them.
    /// </summary>
    public bool EnableParamRowStrip;

    /// <summary>
    /// If true, then this project is auto-selected on program start.
    /// </summary>
    public bool AutoSelect;

    // Editors enabled for this project
    public bool EnableFileBrowser;
    public bool EnableParamEditor;
    public bool EnableMapEditor;
    public bool EnableModelEditor;
    public bool EnableBehaviorEditor;
    public bool EnableCollisionEditor;

    public Project() { }

    public Project(Smithbox source, Guid newGuid, string projectName, string projectPath, string dataPath, ProjectType projectType)
    {
        Source = source;
        ProjectGUID = newGuid;
        ProjectName = projectName;
        ProjectPath = projectPath;
        DataPath = dataPath;
        ProjectType = projectType;
        ImportedParamRowNames = false;
        EnableParamRowStrip = false;

        EnableFileBrowser = true;
        EnableParamEditor = true;
        EnableMapEditor = true;
        EnableModelEditor = true;
        EnableBehaviorEditor = true;
        EnableCollisionEditor = true;

        ActionManager = new ActionManager();
    }

    /// <summary>
    /// Base class
    /// </summary>
    [JsonIgnore]
    public Smithbox Source;

    /// <summary>
    /// Action manager for project-level changes (e.g. aliases)
    /// </summary>
    [JsonIgnore]
    public ActionManager ActionManager;

    /// <summary>
    /// Compound filesystem, contains all the other systems, in the order of precedence
    /// Project -> Vanilla (VanillaReal -> VanillaBinder)
    /// </summary>
    [JsonIgnore]
    public VirtualFileSystem FS = EmptyVirtualFileSystem.Instance;

    /// <summary>
    /// Filesystem for files in the Project directory
    /// </summary>
    [JsonIgnore]
    public VirtualFileSystem ProjectFS = EmptyVirtualFileSystem.Instance;

    /// <summary>
    /// Filesystem for the inner (bindered) files in the Data directory
    /// </summary>
    [JsonIgnore]
    public VirtualFileSystem VanillaBinderFS = EmptyVirtualFileSystem.Instance;

    /// <summary>
    /// Filesystem for the files in the Data directory
    /// </summary>
    [JsonIgnore]
    public VirtualFileSystem VanillaRealFS = EmptyVirtualFileSystem.Instance;

    /// <summary>
    /// Filesystem for the files in the Data directory (groups Binder and Real) 
    /// </summary>
    [JsonIgnore]
    public VirtualFileSystem VanillaFS = EmptyVirtualFileSystem.Instance;

    /// <summary>
    /// The file dictionary used by this project.
    /// Used to build file lists without explicit lookups, since we are using VFS
    /// </summary>
    [JsonIgnore]
    public FileDictionary FileDictionary;

    /// <summary>
    /// Aliases
    /// </summary>
    [JsonIgnore]
    public AliasStore Aliases;

    /// <summary>
    /// Shoebox Layouts
    /// </summary>
    [JsonIgnore]
    public ShoeboxLayoutContainer ShoeboxLayouts;

    /// <summary>
    /// If true, the data elements (i.e. Aliases and Editors) for this project have been initialized.
    /// </summary>
    [JsonIgnore]
    private bool Initialized = false;

    /// <summary>
    /// Track if we are in the process of initialization
    /// </summary>
    [JsonIgnore]
    private bool IsInitializing = false;

    /// <summary>
    /// If true, this project is the currently selected project.
    /// </summary>
    [JsonIgnore]
    public bool IsSelected = false;

    /// <summary>
    /// File Browser
    /// </summary>
    [JsonIgnore]
    public FileBrowser FileBrowser;

    /// <summary>
    /// Param Editor
    /// </summary>
    [JsonIgnore]
    public ParamEditor PrimaryParamEditor;

    [JsonIgnore]
    public ParamEditor SecondaryParamEditor;

    [JsonIgnore]
    public ParamData ParamData;

    /// <summary>
    /// Map Editor
    /// </summary>
    [JsonIgnore]
    public MapData MapData;

    [JsonIgnore]
    public MapEditor MapEditor;

    /// <summary>
    /// Model Editor
    /// </summary>
    [JsonIgnore]
    public ModelData ModelData;

    [JsonIgnore]
    public ModelEditor ModelEditor;

    /// <summary>
    /// Behavior Editor
    /// </summary>
    [JsonIgnore]
    public BehaviorData BehaviorData;

    [JsonIgnore]
    public BehaviorEditor BehaviorEditor;

    /// <summary>
    /// Collision Editor
    /// </summary>
    [JsonIgnore]
    public CollisionData CollisionData;

    [JsonIgnore]
    public CollisionEditor CollisionEditor;

    public async void Initialize()
    {
        TaskLogs.AddLog($"[{ProjectName}] Initializing...");

        ActionManager = new();

        // DLLs
        Task<bool> dllGrabTask = SetupDLLs();
        bool dllGrabResult = await dllGrabTask;

        if (!dllGrabResult)
        {
            TaskLogs.AddLog($"[{ProjectName}] Failed to grab oo2core.dll");
        }

        // VFS
        Task<bool> vfsTask = SetupVFS();
        bool vfsSetup = await vfsTask;

        if (vfsSetup)
        {
            TaskLogs.AddLog($"[{ProjectName}] Setup virtual filesystem.");
        }
        else
        {
            TaskLogs.AddLog($"[{ProjectName}] Failed to setup virtual filesystem.");
        }

        // Aliases
        Task<bool> aliasesTask = SetupAliases();
        bool aliasesSetup = await aliasesTask;

        if (aliasesSetup)
        {
            TaskLogs.AddLog($"[{ProjectName}] Setup aliases.");
        }
        else
        {
            TaskLogs.AddLog($"[{ProjectName}] Failed to setup aliases.");
        }

        // Image Shoebox
        Task<bool> shoeboxTask = SetupShoeboxLayouts();
        bool shoeboxSetup = await shoeboxTask;

        if (aliasesSetup)
        {
            TaskLogs.AddLog($"[{ProjectName}] Setup shoebox layouts.");
        }
        else
        {
            TaskLogs.AddLog($"[{ProjectName}] Failed to setup shoebox layouts.");
        }

        InitializeEditors();

        IsInitializing = false;
        Initialized = true;
    }

    /// <summary>
    /// Called if the project editor settings are changed
    /// </summary>
    public void InitializeEditors()
    {
        // TODO: this should probably dispose of existing stuff, not just null them
        FileBrowser = null;

        ParamData = null;
        PrimaryParamEditor = null;
        SecondaryParamEditor = null;

        MapData = null;
        MapEditor = null;

        ModelData = null;
        ModelEditor = null;

        BehaviorData = null;
        BehaviorEditor = null;

        CollisionData = null;
        CollisionEditor = null;

        // File Browser
        if (EnableFileBrowser && FeatureFlags.EnableFileBrowser)
        {
            FileBrowser = new(0, this);
        }

        // Param Editor
        if (EnableParamEditor && FeatureFlags.EnableParamEditor)
        {
            ParamData = new(this);

            PrimaryParamEditor = new ParamEditor(0, this);
            SecondaryParamEditor = new ParamEditor(1, this);
        }

        // Map Editor
        if (EnableMapEditor && FeatureFlags.EnableMapEditor)
        {
            MapData = new(this);

            MapEditor = new MapEditor(0, this);
        }

        // Model Editor
        if (EnableModelEditor && FeatureFlags.EnableModelEditor)
        {
            ModelData = new(this);

            ModelEditor = new ModelEditor(0, this);
        }

        // Behavior Editor
        if (EnableBehaviorEditor && FeatureFlags.EnableBehaviorEditor)
        {
            BehaviorData = new(this);

            BehaviorEditor = new BehaviorEditor(0, this);
        }

        // Collision Editor
        if (EnableCollisionEditor && FeatureFlags.EnableCollisionEditor)
        {
            CollisionData = new(this);

            CollisionEditor = new CollisionEditor(0, this);
        }
    }

    public void Draw(Command cmd)
    {
        ImGui.Begin($"Project##Project", ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoMove);

        uint dockspaceID = ImGui.GetID("ProjectDockspace");
        ImGui.DockSpace(dockspaceID, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);

        // Only initialize once the project is selected
        // This is so we don't try and initialize all
        // projects in the stored list immediately
        if (IsSelected && !Initialized && !IsInitializing)
        {
            IsInitializing = true;
            Initialize();
        }

        if (Initialized)
        {
            Menubar();

            // File Browser
            if (EnableFileBrowser && FileBrowser != null)
            {
                FileBrowser.Draw();
            }

            // Param Editor
            if (EnableParamEditor && ParamData != null && PrimaryParamEditor != null)
            {
                PrimaryParamEditor.Draw();

                if (CFG.Current.DisplaySecondaryParamEditor)
                {
                    SecondaryParamEditor.Draw();
                }
            }

            // Map Editor
            if (EnableMapEditor && MapEditor != null)
            {
                MapEditor.Draw();
            }

            // Model Editor
            if (EnableModelEditor && ModelEditor != null)
            {
                ModelEditor.Draw();
            }

            // Behavior Editor
            if (EnableBehaviorEditor && BehaviorEditor != null)
            {
                BehaviorEditor.Draw();
            }

            // Collision Editor
            if (EnableCollisionEditor && CollisionEditor != null)
            {
                CollisionEditor.Draw();
            }
        }

        ImGui.End();
    }

    private void Menubar()
    {
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.BeginMenu($"File"))
            {
                if (ImGui.MenuItem("Open Project Folder"))
                {
                    Process.Start("explorer.exe", ProjectPath);
                }
                UIHelper.Tooltip("Open the project folder for this project.");

                ImGui.EndMenu();
            }

            ImGui.EndMenuBar();
        }
    }

    public void Save()
    {
        var folder = FolderUtils.GetProjectFolder();
        var file = Path.Combine(folder, $"{ProjectGUID}.json");

        var json = JsonSerializer.Serialize(this, SmithboxSerializerContext.Default.Project);

        File.WriteAllText(file, json);
    }

    public async Task<bool> SetupDLLs()
    {
        await Task.Delay(1000);

        if (ProjectType is ProjectType.SDT or ProjectType.ER)
        {
            var rootDllPath = Path.Join(DataPath, "oo2core_6_win64.dll");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "oo2core_6_win64.dll");

            if (!File.Exists(rootDllPath))
            {
                return false;
            }
            else
            {
                if (!File.Exists(projectDllPath))
                {
                    File.Copy(rootDllPath, projectDllPath);
                }
            }
        }

        if (ProjectType is ProjectType.AC6)
        {
            var rootDllPath = Path.Join(DataPath, "oo2core_8_win64.dll");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "oo2core_8_win64.dll");

            if (!File.Exists(rootDllPath))
            {
                return false;
            }
            else
            {
                if (!File.Exists(projectDllPath))
                {
                    File.Copy(rootDllPath, projectDllPath);
                }
            }
        }

        return true;
    }

    public async Task<bool> SetupVFS()
    {
        await Task.Delay(1000);

        List<VirtualFileSystem> fileSystems = [];

        ProjectFS.Dispose();
        VanillaRealFS.Dispose();
        VanillaBinderFS.Dispose();
        VanillaFS.Dispose();
        FS.Dispose();

        // Order of addition to FS determines precendence when getting a file
        // e.g. ProjectFS is prioritised over VanillaFS

        // Project File System
        if (Directory.Exists(ProjectPath))
        {
            ProjectFS = new RealVirtualFileSystem(ProjectPath, false);
            fileSystems.Add(ProjectFS);
        }
        else
        {
            ProjectFS = EmptyVirtualFileSystem.Instance;
        }

        // Vanilla File System
        if (Directory.Exists(DataPath))
        {
            VanillaRealFS = new RealVirtualFileSystem(DataPath, false);
            fileSystems.Add(VanillaRealFS);

            var andreGame = ProjectType.AsAndreGame();

            if (andreGame != null)
            {
                if (!ProjectType.IsLooseGame())
                {
                    VanillaBinderFS = ArchiveBinderVirtualFileSystem.FromGameFolder(DataPath, andreGame.Value);
                    fileSystems.Add(VanillaBinderFS);
                }

                VanillaFS = new CompundVirtualFileSystem([VanillaRealFS, VanillaBinderFS]);
            }
            else
            {
                VanillaRealFS = EmptyVirtualFileSystem.Instance;
                VanillaFS = EmptyVirtualFileSystem.Instance;
            }
        }
        else
        {
            VanillaRealFS = EmptyVirtualFileSystem.Instance;
            VanillaFS = EmptyVirtualFileSystem.Instance;
        }


        if (fileSystems.Count == 0)
            FS = EmptyVirtualFileSystem.Instance;
        else
            FS = new CompundVirtualFileSystem(fileSystems);

        var folder = @$"{AppContext.BaseDirectory}\Assets\File Dictionaries\";
        var file = "";

        // Build the file dictionary JSON objects here
        switch (ProjectType)
        {
            case ProjectType.DES: 
                file = "DES-File-Dictionary.json"; break;
            case ProjectType.DS1:
                file = "DS1-File-Dictionary.json"; break;
            case ProjectType.DS1R:
                file = "DS1R-File-Dictionary.json"; break;
            case ProjectType.DS2:
                file = "DS2-File-Dictionary.json"; break;
            case ProjectType.DS2S:
                file = "DS2S-File-Dictionary.json"; break;
            case ProjectType.DS3:
                file = "DS3-File-Dictionary.json"; break;
            case ProjectType.BB:
                file = "BB-File-Dictionary.json"; break;
            case ProjectType.SDT:
                file = "SDT-File-Dictionary.json"; break;
            case ProjectType.ER:
                file = "ER-File-Dictionary.json"; break;
            case ProjectType.AC6:
                file = "AC6-File-Dictionary.json"; break;
            case ProjectType.ERN:
                file = "ERN-File-Dictionary.json"; break;
            default: break;
        }

        var filepath = $"{folder}{file}";

        FileDictionary = new();
        FileDictionary.Entries = new();

        if (File.Exists(filepath))
        {
            try
            {
                var filestring = File.ReadAllText(filepath);
                var options = new JsonSerializerOptions();
                FileDictionary = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.FileDictionary);

                if (FileDictionary == null)
                {
                    throw new Exception("JsonConvert returned null");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Failed to load file dictionary.");
            }
        }

        // TODO: add check for project-unique files, and add them to the FileDictionary so we can point to project-unique files

        return true;
    }

    public async Task<bool> SetupAliases()
    {
        await Task.Delay(1000);

        Aliases = new();

        var sourceFolder = $@"{AppContext.BaseDirectory}\Assets\Aliases\{LocatorUtils.GetGameDirectory(ProjectType)}";
        var sourceFile = Path.Combine(sourceFolder, "Aliases.json");

        var projectFolder = $@"{ProjectPath}\.smithbox\Assets\Aliases\{LocatorUtils.GetGameDirectory(ProjectType)}";
        var projectFile = Path.Combine(projectFolder, "Aliases.json");

        var targetFile = sourceFile;

        if (File.Exists(projectFile))
        {
            targetFile = projectFile;
        }

        if (File.Exists(targetFile))
        {
            try
            {
                var filestring = File.ReadAllText(targetFile);
                var options = new JsonSerializerOptions();
                Aliases = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.AliasStore);

                if (Aliases == null)
                {
                    throw new Exception("[Smithbox] Failed to read Aliases.json");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Failed to load Aliases.json");
            }
        }

        return true;
    }

    public async Task<bool> SetupShoeboxLayouts()
    {
        await Task.Delay(1000);

        ShoeboxLayouts = new(this);

        if (ProjectType is ProjectType.ER or ProjectType.AC6)
        {
            string sourcePath = $@"menu/hi/01_common.sblytbnd.dcx";

            if (FS.FileExists(sourcePath))
            {
                ShoeboxLayouts.LoadLayouts(sourcePath);
                ShoeboxLayouts.BuildTextureDictionary();
            }
            else
            {
                var filename = Path.GetFileNameWithoutExtension(sourcePath);
                TaskLogs.AddLog($"Failed to load Shoebox Layout: {filename} at {sourcePath}");
            }
        }

        return true;
    }
}
