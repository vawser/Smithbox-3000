using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using Smithbox.Core.Interface;
using Smithbox.Core.Interface.ImGuiDemo;
using Smithbox.Core.Utils;
using System;
using System.Collections.Generic;
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
    private OpenFileDialog FileDialog;

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
    }

    /// <summary>
    /// Draw loop
    /// </summary>
    public void Draw()
    {
        UIHelper.ApplyBaseStyle();

        MessageBox.Draw();
        ProjectCreation.Draw();

        Menubar();

        if (CFG.Current.DisplayProjectsWindow)
        {
            ImGui.Begin($"Projects##BaseDock_Window");

            DisplayProjectActions();
            DisplayProjectList();

            ImGui.End();
        }

        foreach (var projectEntry in Projects)
        {
            projectEntry.Draw();
        }

        UIHelper.UnapplyBaseStyle();

        // Create new project if triggered to do so
        if (ProjectCreation.Create)
        {
            ProjectCreation.Create = false;
            CreateProject();
        }
    }
    private void Menubar()
    {
        if (ImGui.BeginMainMenuBar())
        {
            ImGui.Separator();

            if (ImGui.BeginMenu("View"))
            {
                if (ImGui.MenuItem("Projects", CFG.Current.DisplayProjectsWindow))
                {
                    CFG.Current.DisplayProjectsWindow = !CFG.Current.DisplayProjectsWindow;
                }
                UIHelper.Tooltip("Toggle the visibility of the Projects window.");

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

                ImGui.EndMenu();
            }

            ImGui.Separator();

            ImGui.EndMainMenuBar();
        }
    }

    /// <summary>
    /// Actions to scan for projects or add a new project
    /// </summary>
    public void DisplayProjectActions()
    {
        var windowWidth = ImGui.GetWindowWidth() * 0.95f;
        var buttonSize = new Vector2(windowWidth, 32);

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

        foreach (var projectEntry in Projects)
        {
            if (ImGui.Selectable($"{projectEntry.ProjectName}##{projectEntry.ProjectGUID}", SelectedProject == projectEntry))
            {
                SelectedProject = projectEntry;

                foreach (var tEntry in Projects)
                {
                    tEntry.IsSelected = false;
                }
                SelectedProject.IsSelected = true;
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
                        TaskLogs.AddLog($"Failed to load project: {entry}", LogLevel.Warning);
                    }
                    else
                    {
                        Projects.Add(curProject);
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"Failed to load project: {entry}", LogLevel.Warning);
                }
            }
        }

        if(Projects.Count > 0)
        {
            var firstProject = Projects.First();

            SelectedProject = firstProject;
            SelectedProject.IsSelected = true;
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
