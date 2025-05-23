﻿using Hexa.NET.ImGui;
using Smithbox.Core.Interface;
using Smithbox.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.Editor;

public static class ProjectSettings
{
    private static Smithbox BaseEditor;
    private static Project TargetProject;

    private static bool Display = false;
    public static bool Create = false;

    public static string ProjectName = "";
    public static string ProjectPath = "";
    public static string DataPath = "";
    public static ProjectType ProjectType = ProjectType.None;
    public static bool AutoSelect = false;

    public static bool EnableFileBrowser = true;
    public static bool EnableParamEditor = true;
    public static bool EnableMapEditor = true;
    public static bool EnableModelEditor = true;
    public static bool EnableBehaviorEditor = true;
    public static bool EnableCollisionEditor = true;

    public static void Show(Smithbox baseEditor, Project curProject)
    {
        BaseEditor = baseEditor;
        TargetProject = curProject;

        ProjectName = curProject.ProjectName;
        ProjectPath = curProject.ProjectPath;
        DataPath = curProject.DataPath;
        ProjectType = curProject.ProjectType;
        AutoSelect = curProject.AutoSelect;

        EnableFileBrowser = curProject.EnableFileBrowser;
        EnableParamEditor = curProject.EnableParamEditor;
        EnableMapEditor = curProject.EnableMapEditor;
        EnableModelEditor = curProject.EnableModelEditor;
        EnableBehaviorEditor = curProject.EnableBehaviorEditor;
        EnableCollisionEditor = curProject.EnableCollisionEditor;

        Display = true;
    }

    public static void Draw()
    {
        var inputWidth = 400.0f;

        var viewport = ImGui.GetMainViewport();
        Vector2 center = viewport.Pos + viewport.Size / 2;

        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

        ImGui.SetNextWindowSize(new Vector2(670, 356), ImGuiCond.Always);

        if (Display)
        {
            if (ImGui.Begin("Project Settings##projectSettingsWindow", ref Display, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
            {
                if (ImGui.BeginTable($"projectSettingsTable", 3, ImGuiTableFlags.SizingFixedFit))
                {
                    ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Input", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Action", ImGuiTableColumnFlags.WidthStretch);

                    // Project Name
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Name");
                    UIHelper.Tooltip("The name of the project.");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.SetNextItemWidth(inputWidth);
                    ImGui.InputText("##projectNameInput", ref ProjectName, 255);

                    ImGui.TableSetColumnIndex(2);

                    // Project Path
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Project Directory");
                    UIHelper.Tooltip("The location of the project.");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.SetNextItemWidth(inputWidth);
                    ImGui.InputText("##projectPathInput", ref ProjectPath, 255);

                    ImGui.TableSetColumnIndex(2);

                    if (ImGui.Button("Select##projectPathSelect"))
                    {
                        ProjectPath = PathUtils.GetFolderSelection();
                    }

                    // Data Path
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Data Directory");
                    UIHelper.Tooltip("The location of the game data.\nSelect the game executable directory.");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.SetNextItemWidth(inputWidth);
                    ImGui.InputText("##dataPathInput", ref DataPath, 255);

                    ImGui.TableSetColumnIndex(2);

                    if (ImGui.Button("Select##dataPathSelect"))
                    {
                        DataPath = PathUtils.GetFolderSelection();
                    }

                    // Automatic Load
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Automatic Load");
                    UIHelper.Tooltip("If true, then this project will be automatically loaded when Smithbox launches.");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.SetNextItemWidth(inputWidth);

                    ImGui.Checkbox("##projectAutoLoad", ref AutoSelect);

                    ImGui.TableSetColumnIndex(2);

                    // File Browser
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Enable File Browser");
                    UIHelper.Tooltip("If true, the File Browser will be initialized for this project.");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.SetNextItemWidth(inputWidth);

                    ImGui.Checkbox("##projectEnableFileBrowser", ref EnableFileBrowser);

                    ImGui.TableSetColumnIndex(2);

                    // Param Editor
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Enable Param Editor");
                    UIHelper.Tooltip("If true, the Param Editor will be initialized for this project.");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.SetNextItemWidth(inputWidth);

                    ImGui.Checkbox("##projectEnableParamEditor", ref EnableParamEditor);

                    ImGui.TableSetColumnIndex(2);

                    // Map Editor
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Enable Map Editor");
                    UIHelper.Tooltip("If true, the Map Editor will be initialized for this project.");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.SetNextItemWidth(inputWidth);

                    ImGui.Checkbox("##projectEnableMapEditor", ref EnableMapEditor);

                    ImGui.TableSetColumnIndex(2);

                    // Model Editor
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Enable Model Editor");
                    UIHelper.Tooltip("If true, the Model Editor will be initialized for this project.");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.SetNextItemWidth(inputWidth);

                    ImGui.Checkbox("##projectEnableModelEditor", ref EnableModelEditor);

                    ImGui.TableSetColumnIndex(2);

                    // Behavior Editor
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Enable Behavior Editor");
                    UIHelper.Tooltip("If true, the Behavior Editor will be initialized for this project.");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.SetNextItemWidth(inputWidth);

                    ImGui.Checkbox("##projectEnableBehaviorEditor", ref EnableBehaviorEditor);

                    ImGui.TableSetColumnIndex(2);

                    // Collision Editor
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Enable Collision Editor");
                    UIHelper.Tooltip("If true, the Collision Editor will be initialized for this project.");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.SetNextItemWidth(inputWidth);

                    ImGui.Checkbox("##projectEnableCollisionEditor", ref EnableCollisionEditor);

                    ImGui.TableSetColumnIndex(2);

                    ImGui.EndTable();

                    if (!AllowUpdate())
                    {
                        ImGui.Separator();

                        ImGui.Text(GetUpdateBlockedTooltip());

                        ImGui.Separator();
                    }

                    var buttonSize = new Vector2(200, 24);

                    // Update
                    if (AllowUpdate())
                    {
                        if (ImGui.Button("Update##updateProjectSettings", buttonSize))
                        {
                            Display = false;

                            TargetProject.ProjectName = ProjectName;
                            TargetProject.ProjectPath = ProjectPath;
                            TargetProject.DataPath = DataPath;
                            TargetProject.ProjectType = ProjectType;
                            TargetProject.AutoSelect = AutoSelect;

                            TargetProject.EnableFileBrowser = EnableFileBrowser;
                            TargetProject.EnableParamEditor = EnableParamEditor;
                            TargetProject.EnableMapEditor = EnableMapEditor;
                            TargetProject.EnableModelEditor = EnableModelEditor;
                            TargetProject.EnableBehaviorEditor = EnableBehaviorEditor;
                            TargetProject.EnableCollisionEditor = EnableCollisionEditor;

                            TargetProject.Save();

                            TargetProject.InitializeEditors();
                        }
                        UIHelper.Tooltip("Updates the project settings.");
                    }
                    else
                    {
                        ImGui.BeginDisabled();
                        if (ImGui.Button("Update##updateProjectSettings", buttonSize))
                        {
                        }
                        ImGui.EndDisabled();
                    }

                    ImGui.SameLine();

                    // Delete
                    if (ImGui.Button("Delete##deleteProject", buttonSize))
                    {
                        Display = false;

                        // Delete the project file
                        var filename = @$"{AppContext.BaseDirectory}\.smithbox\Projects\{TargetProject.ProjectGUID}.json";
                        if (File.Exists(filename))
                        {
                            File.Delete(filename);
                        }

                        // Remove the project from the ordering file and retain sequential ids
                        var curOrderEntry = BaseEditor.ProjectDisplayConfig.ProjectOrder.Where(e => e.Value == TargetProject.ProjectGUID).FirstOrDefault();

                        var existingOrder = BaseEditor.ProjectDisplayConfig.ProjectOrder;
                        var newOrder = new Dictionary<int, Guid>();

                        var count = 0;
                        for (int i = 0; i < existingOrder.Count; i++)
                        {
                            var curGuid = existingOrder[i];
                            if (curGuid != TargetProject.ProjectGUID)
                            {
                                newOrder.Add(count, curGuid);
                                count++;
                            }
                        }

                        BaseEditor.ProjectDisplayConfig.ProjectOrder = newOrder;

                        BaseEditor.SaveProjectDisplayConfig();

                        // Unload the project editor stuff
                        BaseEditor.SelectedProject = null;
                        BaseEditor.Projects.Remove(TargetProject);
                    }
                    UIHelper.Tooltip("Deletes this project entry (not the mod data) and unloads the current project editors.");

                    ImGui.SameLine();

                    // Cancel
                    if (ImGui.Button("Close##closeProjectSettings", buttonSize))
                    {
                        Display = false;
                    }
                    UIHelper.Tooltip("Closes the project settings menu.");
                }


                ImGui.End();
            }
        }
    }

    private static bool AllowUpdate()
    {
        bool isAllowed = true;

        if (ProjectName == "")
            isAllowed = false;

        if (!Directory.Exists(ProjectPath))
            isAllowed = false;

        if (!Directory.Exists(DataPath))
            isAllowed = false;

        if (ProjectName == "")
            isAllowed = false;

        return isAllowed;
    }

    private static string GetUpdateBlockedTooltip()
    {
        var tooltip = "You cannot update this project due to the following issues:";

        if (ProjectName == "")
            tooltip = tooltip + "\n" + "Project Name cannot be empty.";

        if (!Directory.Exists(ProjectPath))
            tooltip = tooltip + "\n" + "Project Path is set to an invalid path.";

        if (!Directory.Exists(DataPath))
            tooltip = tooltip + "\n" + "Data Path is set to an invalid path.";

        return tooltip;
    }
}
