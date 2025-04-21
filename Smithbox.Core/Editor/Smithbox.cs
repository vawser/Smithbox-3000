using DotNext;
using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Smithbox.Core.Interface;
using Smithbox.Core.Interface.ImGuiDemo;
using Smithbox.Core.ParamEditorNS;
using Smithbox.Core.Resources;
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
using static Hexa.NET.Utilities.IO.FileUtils.Win;

namespace Smithbox.Core.Editor;

/// <summary>
/// This is the top-level handler for everything
/// </summary>
public class Smithbox
{
    public Project SelectedProject;
    public List<Project> Projects = new();

    public bool HasSetup = false;

    public ProjectDisplay ProjectDisplayConfig;

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

        LoadProjectDisplayConfig();
        LoadExistingProjects();

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
        ColorPicker.Draw();
        ProjectCreation.Draw();
        ProjectSettings.Draw();
        ProjectAliasEditor.Draw();
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
    public unsafe void DisplayProjectList()
    {
        UIHelper.SimpleHeader("projectListHeader", "Available Projects", "The projects currently available to select from.", UI.Current.ImGui_Highlight_Text);

        var orderedProjects = Projects
        .OrderBy(p =>
        {
            foreach (var kvp in ProjectDisplayConfig.ProjectOrder)
            {
                if (kvp.Value == p.ProjectGUID)
                {
                    return kvp.Key;
                }
            }
            return int.MaxValue; // Put untracked projects at the end
        })
        .ToList();

        int dragSourceIndex = -1;
        int dragTargetIndex = -1;

        for (int i = 0; i < orderedProjects.Count; i++)
        {
            var project = orderedProjects[i];
            var imGuiID = project.ProjectGUID;

            // Highlight selectable
            if (ImGui.Selectable($"{project.ProjectName}##{imGuiID}", SelectedProject == project))
            {
                SelectedProject = project;

                foreach (var tEntry in Projects)
                    tEntry.IsSelected = false;

                SelectedProject.IsSelected = true;
            }

            // Begin drag
            if (ImGui.BeginDragDropSource())
            {
                int payloadIndex = i;
                ImGui.SetDragDropPayload("PROJECT_DRAG", &payloadIndex, sizeof(int));
                ImGui.Text(project.ProjectName);
                ImGui.EndDragDropSource();
            }

            // Accept drop
            if (ImGui.BeginDragDropTarget())
            {
                var payload = ImGui.AcceptDragDropPayload("PROJECT_DRAG");
                if (payload.Handle != null)
                {
                    int* droppedIndex = (int*)payload.Data;
                    dragSourceIndex = *droppedIndex;
                    dragTargetIndex = i;
                }
                ImGui.EndDragDropTarget();
            }

            if (ImGui.BeginPopupContextItem($"ProjectListContextMenu{imGuiID}"))
            {
                if(ImGui.MenuItem($"Open Project Settings##projectSettings_{imGuiID}"))
                {
                    ProjectSettings.Show(this, SelectedProject);
                }

                if (ImGui.MenuItem($"Open Project Aliases##projectAliases_{imGuiID}"))
                {
                    ProjectAliasEditor.Show(this, SelectedProject);
                }

                if (CFG.Current.ModEngineInstall != "")
                {
                    if (ImGui.MenuItem($"Launch Mod##launchMod{imGuiID}"))
                    {
                        ModEngineUtils.LaunchMod(SelectedProject);
                    }
                }

                ImGui.EndPopup();
            }
        }

        if (dragSourceIndex >= 0 && dragTargetIndex >= 0 && dragSourceIndex != dragTargetIndex)
        {
            var movedProject = orderedProjects[dragSourceIndex];
            orderedProjects.RemoveAt(dragSourceIndex);
            orderedProjects.Insert(dragTargetIndex, movedProject);

            // Rebuild order dictionary
            ProjectDisplayConfig.ProjectOrder.Clear();
            for (int i = 0; i < orderedProjects.Count; i++)
            {
                ProjectDisplayConfig.ProjectOrder[i] = orderedProjects[i].ProjectGUID;
            }

            SaveProjectDisplayConfig();
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

    private void LoadProjectDisplayConfig()
    {
        var folder = FolderUtils.GetConfigurationFolder();
        var file = Path.Combine(folder, "Project Display.json");

        if (!File.Exists(file))
        {
            ProjectDisplayConfig = new ProjectDisplay();
        }
        else
        {
            try
            {
                var filestring = File.ReadAllText(file);
                var options = new JsonSerializerOptions();
                ProjectDisplayConfig = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.ProjectDisplay);

                if (ProjectDisplayConfig == null)
                {
                    throw new Exception("[Smithbox] Failed to read Project Display.json");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Failed to load Project Display.json");

                ProjectDisplayConfig = new ProjectDisplay();
            }
        }
    }

    public void SaveProjectDisplayConfig()
    {
        var folder = FolderUtils.GetConfigurationFolder();
        var file = Path.Combine(folder, "Project Display.json");

        var json = JsonSerializer.Serialize(ProjectDisplayConfig, SmithboxSerializerContext.Default.ProjectDisplay);

        File.WriteAllText(file, json);
    }

    private void LoadExistingProjects()
    {
        // Read all the stored project jsons and create an existing Project dict
        var projectJsonList = FolderUtils.GetStoredProjectJsonList();

        var buildProjectOrder = false;

        // If it is not set, create initial order
        if (ProjectDisplayConfig.ProjectOrder == null)
        {
            ProjectDisplayConfig.ProjectOrder = new();
            buildProjectOrder = true;
        }

        for(int i = 0; i < projectJsonList.Count; i++)
        {
            var entry = projectJsonList[i];

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
                        curProject.Source = this;

                        // Ignore unsupported projects
                        if (ProjectUtils.IsSupportedProjectType(curProject.ProjectType))
                        {
                            Projects.Add(curProject);

                            if (buildProjectOrder)
                            {
                                ProjectDisplayConfig.ProjectOrder.Add(i, curProject.ProjectGUID);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to load project: {entry}", LogLevel.Warning);
                }
            }
        }

        if (buildProjectOrder)
        {
            SaveProjectDisplayConfig();
        }

        if (Projects.Count > 0)
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

        var newProject = new Project(this, guid, projectName, projectPath, dataPath, projectType);

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
