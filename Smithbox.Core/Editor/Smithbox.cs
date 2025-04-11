using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using Smithbox.Core.Interface;
using Smithbox.Core.Interface.ImGuiDemo;
using Smithbox.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    }

    /// <summary>
    /// Draw loop
    /// </summary>
    public void Draw()
    {
        MessageBox.Draw();

        uint dockspaceID = ImGui.GetID("BaseDock");

        ImGui.Begin($"Projects##BaseDock_Window");

        foreach(var projectEntry in Projects)
        {
            projectEntry.Draw();
        }

        DisplayProjectActions();
        DisplayProjectList();

        ImGui.End();
    }

    /// <summary>
    /// Actions to scan for projects or add a new project
    /// </summary>
    public void DisplayProjectActions()
    {
        if (ImGui.Button("Add Project"))
        {
            var dialog = new OpenFileDialog();
            if(dialog.Result == OpenFileResult.Ok)
            {
                var file = dialog.SelectedFile;
            }
        }
    }

    /// <summary>
    /// The list of stored projects.
    /// </summary>
    public void DisplayProjectList()
    {
        if(SelectedProject != null)
        {
            ImGui.Separator();


            ImGui.Separator();
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

                    var curProject = JsonSerializer.Deserialize(filestring, ProjectSerializerContext.Default.Project);

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
    }

    private void CreateProject()
    {
        var newProject = new Project(GUID.Generate());



        Projects.Add(newProject);

    }
}
