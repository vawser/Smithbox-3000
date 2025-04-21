﻿using Hexa.NET.ImGui;
using HKLib.hk2018.hk.RPC;
using Smithbox.Core.Interface;
using Smithbox.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.Editor;

public static class ProjectCreation
{
    private static bool Display = false;
    public static bool Create = false;

    public static string ProjectName = "";
    public static string ProjectPath = "";
    public static string DataPath = "";
    public static ProjectType ProjectType = ProjectType.None;

    public static void Reset()
    {
        ProjectName = "";
        ProjectPath = "";
        DataPath = "";
        ProjectType = ProjectType.None;
    }

    public static void Show()
    {
        Display = true;
    }

    public static void Draw()
    {
        var inputWidth = 400.0f;

        var viewport = ImGui.GetMainViewport();
        Vector2 center = viewport.Pos + viewport.Size / 2;

        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

        ImGui.SetNextWindowSize(new Vector2(640, 240), ImGuiCond.Always);

        if (Display)
        {
            if (ImGui.Begin("Project Creation##projectCreationWindow", ref Display, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove))
            {
                if (ImGui.BeginTable($"projectCreationTable", 3, ImGuiTableFlags.SizingFixedFit))
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

                    // Project Type
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Project Type");
                    UIHelper.Tooltip("The game this project is targeting.");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.SetNextItemWidth(inputWidth);
                    if (ImGui.BeginCombo("##projectTypePicker", ProjectType.GetDisplayName()))
                    {
                        foreach (var entry in Enum.GetValues<ProjectType>())
                        {
                            var type = (ProjectType)entry;

                            if (ProjectUtils.IsSupportedProjectType(type))
                            {
                                if (ImGui.Selectable(type.GetDisplayName()))
                                {
                                    ProjectType = type;
                                }
                            }
                        }
                        ImGui.EndCombo();
                    }

                    ImGui.TableSetColumnIndex(2);

                    ImGui.EndTable();

                    if (!AllowCreation())
                    {
                        ImGui.Separator();

                        ImGui.Text(GetCreationBlockedTooltip());

                        ImGui.Separator();
                    }

                    var buttonSize = new Vector2(310, 24);

                    // Create
                    if (AllowCreation())
                    {
                        if (ImGui.Button("Create##createProjectCreation", buttonSize))
                        {
                            Display = false;
                            Create = true;
                        }
                    }
                    else
                    {
                        ImGui.BeginDisabled();
                        if (ImGui.Button("Create##createProjectCreation", buttonSize))
                        {
                        }
                        ImGui.EndDisabled();
                    }

                    ImGui.SameLine();

                    // Cancel
                    if (ImGui.Button("Cancel##cancelProjectCreation", buttonSize))
                    {
                        Display = false;
                    }
                }


                ImGui.End();
            }
        }
    }

    private static bool AllowCreation()
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

    private static string GetCreationBlockedTooltip()
    {
        var tooltip = "You cannot create a project due to the following issues:";

        if (ProjectName == "")
            tooltip = tooltip + "\n" + "Project Name cannot be empty.";

        if (!Directory.Exists(ProjectPath))
            tooltip = tooltip + "\n" + "Project Path is set to an invalid path.";

        if (!Directory.Exists(DataPath))
            tooltip = tooltip + "\n" + "Data Path is set to an invalid path.";

        return tooltip;
    }
}
