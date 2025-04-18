using Andre.IO.VFS;
using Hexa.NET.ImGui;
using Smithbox.Core.FileBrowserNS;
using Smithbox.Core.Interface;
using Smithbox.Core.ModelEditorNS;
using Smithbox.Core.ParamEditorNS;
using Smithbox.Core.Resources;
using Smithbox.Core.TextureEditorNS;
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
    }

    /// <summary>
    /// Base class
    /// </summary>
    [JsonIgnore]
    public Smithbox Source;

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
    /// File Browser
    /// </summary>
    [JsonIgnore]
    public FileBrowser FileBrowser;

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
    /// Param Editor
    /// </summary>
    [JsonIgnore]
    public ParamEditor PrimaryParamEditor;

    [JsonIgnore]
    public ParamEditor SecondaryParamEditor;

    [JsonIgnore]
    public ParamData ParamData;

    /// <summary>
    /// Model Editor Editor
    /// </summary>
    [JsonIgnore]
    public ModelEditor PrimaryModelEditor;

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
    /// Texture Editor
    /// </summary>
    [JsonIgnore]
    public TextureEditor TextureEditor;

    public async void Initialize()
    {
        TaskLogs.AddLog($"[{ProjectName}] Initializing...");

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

        // File Browser
        if (FeatureFlags.IncludeFileBrowser)
        {
            FileBrowser = new(0, this);
        }

        // Param Editor
        if (FeatureFlags.IncludeParamEditor)
        {
            ParamData = new(this);

            PrimaryParamEditor = new ParamEditor(0, this);
            SecondaryParamEditor = new ParamEditor(1, this);
        }

        // Model Editor
        if (FeatureFlags.IncludeModelEditor)
        {
            PrimaryModelEditor = new ModelEditor(0, this);
        }

        // Texture Editor
        if (FeatureFlags.IncludeTextureEditor)
        {
            TextureEditor = new TextureEditor(0, this);
        }

        IsInitializing = false;
        Initialized = true;
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
            if (FeatureFlags.IncludeFileBrowser)
            {
                if (CFG.Current.DisplayFileBrowser)
                {
                    FileBrowser.Draw(cmd);
                }
            }

            // Param Editor
            if (FeatureFlags.IncludeParamEditor)
            {
                ParamData.Update();

                if (CFG.Current.DisplayPrimaryParamEditor)
                {
                    PrimaryParamEditor.Draw(cmd);
                }

                if (CFG.Current.DisplaySecondaryParamEditor)
                {
                    SecondaryParamEditor.Draw(cmd);
                }
            }

            // Model Editor
            if (FeatureFlags.IncludeModelEditor)
            {
                if (CFG.Current.DisplayPrimaryModelEditor)
                {
                    PrimaryModelEditor.Draw(cmd);
                }
            }

            // Texture Editor
            if (FeatureFlags.IncludeTextureEditor)
            {
                if (CFG.Current.DisplayTextureEditor)
                {
                    TextureEditor.Draw(cmd);
                }
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

            // Technically not per project, but functionally belongs here
            if (ImGui.BeginMenu("View"))
            {
                if (FeatureFlags.IncludeFileBrowser)
                {
                    if (ImGui.MenuItem("File Browser", CFG.Current.DisplayFileBrowser))
                    {
                        CFG.Current.DisplayFileBrowser = !CFG.Current.DisplayFileBrowser;
                    }
                    UIHelper.Tooltip("Toggle the visibility of the File Browser window.");
                }

                if (FeatureFlags.IncludeParamEditor)
                {
                    if (ImGui.MenuItem("Param Editor (Primary)", CFG.Current.DisplayPrimaryParamEditor))
                    {
                        CFG.Current.DisplayPrimaryParamEditor = !CFG.Current.DisplayPrimaryParamEditor;
                    }
                    UIHelper.Tooltip("Toggle the visibility of the Param Editor (Primary) window.");

                    if (ImGui.MenuItem("Param Editor (Secondary)", CFG.Current.DisplaySecondaryParamEditor))
                    {
                        CFG.Current.DisplaySecondaryParamEditor = !CFG.Current.DisplaySecondaryParamEditor;
                    }
                    UIHelper.Tooltip("Toggle the visibility of the Param Editor (Secondary) window.");
                }

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
            fileSystems.Add(ProjectFS);

            var andreGame = ProjectType.AsAndreGame();

            if (andreGame != null)
            {
                VanillaBinderFS = ArchiveBinderVirtualFileSystem.FromGameFolder(DataPath, andreGame.Value);
                fileSystems.Add(VanillaBinderFS);

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

        return true;
    }

    public async Task<bool> SetupAliases()
    {
        await Task.Delay(1000);

        Aliases = new();

        var folder = $@"{AppContext.BaseDirectory}\Assets\Aliases\{LocatorUtils.GetGameDirectory(ProjectType)}";
        var file = Path.Combine(folder, "Aliases.json");

        if (File.Exists(file))
        {
            try
            {
                var filestring = File.ReadAllText(file);
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
            string sourcePath = $@"menu\hi\01_common.sblytbnd.dcx";

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
