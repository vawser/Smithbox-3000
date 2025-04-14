using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using Smithbox.Core.Interface;
using Smithbox.Core.Interface.ImGuiDemo;
using Smithbox.Core.ParamEditorNS;
using Smithbox.Core.Utils;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Smithbox.Core.Editor;

/// <summary>
/// This is the top-level handler for everything
/// </summary>
public class Smithbox
{
    private Project SelectedProject;
    private List<Project> Projects = new();

    private bool HasSetup = false;

    public Smithbox()
    {

    }

    /// <summary>
    /// Setup the program, creating folders, initializing generic banks, etc.
    /// </summary>
    public void Setup()
    {
        if (HasSetup)
            return;

        HasSetup = true;

        FolderUtils.SetupFolders();

        // Load configuration variables
        CFG.Load();
        UI.Load();

        LoadExistingProjects();

        ProjectCreation.Setup();

        // Set culture to invariant so we don't get funny issues due to localization
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        // Setup ignore asserts feature
        BinaryReaderEx.IgnoreAsserts = CFG.Current.IgnoreReadAsserts;
    }

    /// <summary>
    /// Draw loop
    /// </summary>
    public void Draw()
    {
        var command = EditorCommand.GetNextCommand();

        RenderDockspace();

        UIHelper.ApplyBaseStyle();

        // Special modals
        ParamUpgrader.Draw();
        MessageBox.Draw();
        ColorPicker.Draw();
        ProjectCreation.Draw();
        EditorSettings.Draw();
        ControlSettings.Draw();
        InterfaceSettings.Draw();

        Menubar();

        if (CFG.Current.DisplayProjectListWindow)
        {
            ImGui.Begin($"Project List##ProjectListWindow");

            DisplayProjectActions();
            DisplayProjectList();

            ImGui.End();
        }

        if (CFG.Current.DisplayProjectWindow)
        {
            foreach (var projectEntry in Projects)
            {
                if (projectEntry == SelectedProject)
                {
                    projectEntry.Draw(command);
                }
            }
        }

        UIHelper.UnapplyBaseStyle();

        // Create new project if triggered to do so
        if (ProjectCreation.Create)
        {
            ProjectCreation.Create = false;
            CreateProject();
        }
    }

    private void RenderDockspace()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

        ImGuiViewportPtr viewport = ImGui.GetMainViewport();

        ImGui.SetNextWindowPos(viewport.Pos);
        ImGui.SetNextWindowSize(viewport.Size);
        ImGui.SetNextWindowViewport(viewport.ID);

        ImGuiWindowFlags windowFlags =
            ImGuiWindowFlags.NoTitleBar |
            ImGuiWindowFlags.NoCollapse |
            ImGuiWindowFlags.NoResize |
            ImGuiWindowFlags.NoMove |
            ImGuiWindowFlags.NoBringToFrontOnFocus |
            ImGuiWindowFlags.NoNavFocus |
            ImGuiWindowFlags.MenuBar;

        ImGui.Begin("MainDockspace_W", windowFlags);

        uint dockspaceID = ImGui.GetID("MainDockspace");

        ImGui.DockSpace(dockspaceID, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);

        ImGui.End();

        ImGui.PopStyleVar();
    }

    private void Menubar()
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu($"Settings"))
            {
                if (ImGui.MenuItem("Editor"))
                {
                    EditorSettings.Show();
                }
                UIHelper.Tooltip("Open the Editor settings window.");

                if (ImGui.MenuItem("Controls"))
                {
                    ControlSettings.Show();
                }
                UIHelper.Tooltip("Open the Controls settings window.");

                if (ImGui.MenuItem("Interface"))
                {
                    InterfaceSettings.Show();
                }
                UIHelper.Tooltip("Open the Interface settings window.");

                ImGui.EndMenu();
            }

            ImGui.Separator();

            if (ImGui.BeginMenu("View"))
            {
                if (ImGui.MenuItem("Project List", CFG.Current.DisplayProjectListWindow))
                {
                    CFG.Current.DisplayProjectListWindow = !CFG.Current.DisplayProjectListWindow;
                }
                UIHelper.Tooltip("Toggle the visibility of the Project Lists window.");

                if (ImGui.MenuItem("Project", CFG.Current.DisplayProjectWindow))
                {
                    CFG.Current.DisplayProjectWindow = !CFG.Current.DisplayProjectWindow;
                }
                UIHelper.Tooltip("Toggle the visibility of the Project window.");

                ImGui.EndMenu();
            }

            ImGui.Separator();

            TaskLogs.DisplayActionLoggerBar();
            TaskLogs.DisplayActionLoggerWindow();

            ImGui.Separator();

            TaskLogs.DisplayWarningLoggerBar();
            TaskLogs.DisplayWarningLoggerWindow();  

            ImGui.EndMainMenuBar();
        }
    }

    /// <summary>
    /// Actions to scan for projects or add a new project
    /// </summary>
    public void DisplayProjectActions()
    {
        var windowWidth = ImGui.GetWindowWidth() * 0.95f;
        var buttonSize = new Vector2(windowWidth, 24);

        if (ImGui.Button("Add Project", buttonSize))
        {
            ProjectCreation.Show();
        }
        UIHelper.Tooltip($"Add a new project to the project list.");
    }

    /// <summary>
    /// The list of stored projects.
    /// </summary>
    public void DisplayProjectList()
    {
        UIHelper.SimpleHeader("projectListHeader", "Available Projects", "The projects currently available to select from.", UI.Current.ImGui_Highlight_Text);

        for(int i = 0; i < Projects.Count; i++)
        {
            var projectEntry = Projects[i];

            var imGuiID = projectEntry.ProjectGUID;

            if (ImGui.Selectable($"{projectEntry.ProjectName}##{imGuiID}", SelectedProject == projectEntry))
            {
                SelectedProject = projectEntry;

                foreach (var tEntry in Projects)
                {
                    tEntry.IsSelected = false;
                }
                SelectedProject.IsSelected = true;
            }

            if (ImGui.BeginPopupContextItem($"ProjectListContextMenu{imGuiID}"))
            {
                if(projectEntry.AutoSelect)
                {
                    if (ImGui.Selectable($"Disable Automatic Load##autoLoadDisable{imGuiID}"))
                    {
                        projectEntry.AutoSelect = false;
                        projectEntry.Save();
                    }
                    UIHelper.Tooltip("Disable automatic load for this project.");
                }
                else
                {
                    if (ImGui.Selectable($"Enable Automatic Load##autoLoadEnable{imGuiID}"))
                    {
                        projectEntry.AutoSelect = true;
                        projectEntry.Save();
                    }
                    UIHelper.Tooltip("Set this project to automatically load when Smithbox starts.");
                }

                ImGui.EndPopup();
            }
        }
    }

    /// <summary>
    /// Handle stuff before session end
    /// </summary>
    public void Exit()
    {
        foreach (var projectEntry in Projects)
        {
            projectEntry.Save();
        }

        CFG.Save();
        UI.Save();
        ImGuiCFG.Save();
    }

    private void LoadExistingProjects()
    {
        // Read all the stored project jsons and create an existing Project dict
        var projectJsonList = FolderUtils.GetStoredProjectJsonList();

        foreach (var entry in projectJsonList)
        {
            if (File.Exists(entry))
            {
                try
                {
                    var filestring = File.ReadAllText(entry);
                    var options = new JsonSerializerOptions();

                    var curProject = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.Project);

                    if (curProject == null)
                    {
                        TaskLogs.AddLog($"[Smithbox] Failed to load project: {entry}", LogLevel.Warning);
                    }
                    else
                    {
                        // Ignore unsupported projects
                        if (ProjectUtils.IsSupportedProjectType(curProject.ProjectType))
                        {
                            Projects.Add(curProject);
                        }
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to load project: {entry}", LogLevel.Warning);
                }
            }
        }

        if(Projects.Count > 0)
        {
            foreach(var projectEntry in Projects)
            {
                if(projectEntry.AutoSelect)
                {
                    SelectedProject = projectEntry;
                    SelectedProject.IsSelected = true;
                }
            }
        }
    }

    private void CreateProject()
    {
        var guid = GUID.Generate();
        var projectName = ProjectCreation.ProjectName;
        var projectPath = ProjectCreation.ProjectPath;
        var dataPath = ProjectCreation.DataPath;
        var projectType = ProjectCreation.ProjectType;

        var newProject = new Project(guid, projectName, projectPath, dataPath, projectType);

        ProjectCreation.Reset();

        Projects.Add(newProject);

        newProject.Save();

        // Auto-select new project
        foreach (var tEntry in Projects)
        {
            tEntry.IsSelected = false;
        }
        SelectedProject.IsSelected = true;
    }
}
