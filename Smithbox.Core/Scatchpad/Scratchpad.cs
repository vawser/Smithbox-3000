using Hexa.NET.ImGui;
using Smithbox.Core.Editor;
using Smithbox.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.Scatchpad;

/// <summary>
/// This is a blank editor used for personal stuff, should not be included in release builds
/// </summary>
public class Scratchpad
{
    private Project Project;

    // Defined here so we can remove NoMove when setting up the imgui.ini
    private ImGuiWindowFlags MainWindowFlags = ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoMove;
    private ImGuiWindowFlags SubWindowFlags = ImGuiWindowFlags.NoMove;

    public int ID = 0;

    public ActionManager ActionManager;

    public Scratchpad(int id, Project projectOwner)
    {
        Project = projectOwner;
        ID = id;

        ActionManager = new();
    }

    public void Draw(string[] cmd)
    {
        ImGui.Begin($"Scratchpad##Scratchpad{ID}", MainWindowFlags);

        uint dockspaceID = ImGui.GetID($"ScratchpadDockspace{ID}");
        ImGui.DockSpace(dockspaceID, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);

        Menubar();

        // Add all relevant data bank checks here since scatchpad may access any of them
        if (Project.ParamData.Initialized)
        {
            if (Project.IsSelected)
            {
                DisplayEditor();
            }
            else
            {
                ImGui.Text("You have not selected a project yet.");
            }
        }

        ImGui.End();
    }

    private async Task Menubar()
    {
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Save"))
                {
                    Save();
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Edit"))
            {

                ImGui.EndMenu();
            }

            ImGui.EndMenuBar();
        }
    }

    private void DisplayEditor()
    {

    }

    private async void Save()
    {
        /*
        Task<bool> saveTask = Project.ParamData.PrimaryBank.Save();
        bool saveTaskFinished = await saveTask;

        if (saveTaskFinished)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Saved primary param bank.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to save primary param bank.");
        }
        */
    }
}
