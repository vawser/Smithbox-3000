using Hexa.NET.ImGui;
using Smithbox.Core.Editor;
using Smithbox.Core.Interface;
using Smithbox.Core.Interface.Input;
using Smithbox.Core.Resources;
using Smithbox.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Smithbox.Core.ParamEditorNS;

public class ParamEditor
{
    private Project Project;

    // Defined here so we can remove NoMove when setting up the imgui.ini
    private ImGuiWindowFlags MainWindowFlags = ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoMove;
    private ImGuiWindowFlags SubWindowFlags = ImGuiWindowFlags.NoMove;

    public int ID = 0;

    public ActionManager ActionManager;
    public ParamActions ParamActions;

    public ParamSelection Selection;

    public ParamListView ParamView;
    public ParamRowView RowView;
    public ParamFieldView FieldView;
    public ParamImageView FieldImageView;

    public ParamFieldDecorator FieldDecorator;
    public ParamFieldInput FieldInput;
    public ParamSearchEngine SearchEngine;

    public bool DetectShortcuts = false;

    public ParamEditor(int id, Project projectOwner)
    {
        Project = projectOwner;
        ID = id;

        ActionManager = new();
        ParamActions = new(this);

        Selection = new(Project, this);

        FieldDecorator = new(Project, this);
        FieldInput = new(Project, this);
        SearchEngine = new(Project, this);

        ParamView = new(Project, this);
        RowView = new(Project, this);
        FieldView = new(Project, this);
        FieldImageView = new(Project, this);
    }

    public void Draw()
    {
        ImGui.Begin($"Param Editor##ParamEditor{ID}", MainWindowFlags);

        uint dockspaceID = ImGui.GetID($"ParamEditorDockspace{ID}");
        ImGui.DockSpace(dockspaceID, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);

        Menubar();

        if (Project.ParamData.Initialized)
        {
            FieldView.SetupFieldOrder();
            ParamActions.Draw();

            if (Project.IsSelected)
            {
                ParamView.Draw();
                RowView.Draw();
                FieldView.Draw();
                SearchEngine.Draw();
                FieldImageView.Draw();
            }
            else
            {
                ImGui.Text("You have not selected a project yet.");
            }
        }

        Shortcuts();

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

                if (ParamUpgrader.SupportsParamUpgrading(Project) && ParamUpgrader.IsParamUpgradeValid(Project))
                {
                    if (ImGui.MenuItem("Upgrade"))
                    {
                        ParamUpgrader.Start(Project);
                    }
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Edit"))
            {
                if (ImGui.MenuItem("Undo"))
                {
                    if (ActionManager.CanUndo())
                    {
                        ActionManager.UndoAction();
                    }
                }

                if (ImGui.MenuItem($"Undo Fully"))
                {
                    if (ActionManager.CanUndo())
                    {
                        ActionManager.UndoAllAction();
                    }
                }

                if (ImGui.MenuItem($"Redo"))
                {
                    if (ActionManager.CanRedo())
                    {
                        ActionManager.RedoAction();
                    }
                }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Data"))
            {
                if (Project.ParamData.Initialized)
                {
                    RowNameOptions();
                    ParamDataOptions();
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Information"))
            {
                if (Project.ParamData.Initialized)
                {
                    var primaryVersion = ParamUtils.ParseRegulationVersion(Project.ParamData.PrimaryBank.ParamVersion);

                    ImGui.Text($"Primary Bank Version: {primaryVersion}");

                    var vanillaVersion = ParamUtils.ParseRegulationVersion(Project.ParamData.VanillaBank.ParamVersion);

                    ImGui.Text($"Vanilla Bank Version: {vanillaVersion}");
                }
                ImGui.EndMenu();
            }

            ParamUpgrader.ParamUpgradeWarning(Project);

            ImGui.EndMenuBar();
        }
    }

    /// <summary>
    /// Called after the windows
    /// </summary>
    private void Shortcuts()
    {
        // Editor-level shortcuts
        if (ParamView.DetectShortcuts || RowView.DetectShortcuts || FieldView.DetectShortcuts || FieldImageView.DetectShortcuts)
        {
            if (Keyboard.KeyPress(Key.S) && Keyboard.IsDown(Key.LCtrl))
            {
                Save();
            }

            if (Keyboard.KeyPress(Key.Z) && Keyboard.IsDown(Key.LCtrl))
            {
                ActionManager.UndoAction();
            }

            if (Keyboard.KeyPress(Key.R) && Keyboard.IsDown(Key.LCtrl))
            {
                ActionManager.RedoAction();
            }
        }
    }

    private async void Save()
    {
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
    }

    private void RowNameOptions()
    {
        if (ImGui.BeginMenu("Row Names"))
        {
            if (ImGui.BeginMenu("Import"))
            {
                if (ImGui.BeginMenu("Community Names"))
                {
                    if (ImGui.MenuItem($"By Index"))
                    {
                        Project.ParamData.PrimaryBank.ImportRowNames(ImportRowNameType.Index, ImportRowNameSourceType.Community);
                    }
                    UIHelper.Tooltip("This will import the community names, matching via row index. Warning: this will not function as desired if you have edited the row order.");

                    if (ImGui.MenuItem($"By ID"))
                    {
                        Project.ParamData.PrimaryBank.ImportRowNames(ImportRowNameType.ID, ImportRowNameSourceType.Community);
                    }
                    UIHelper.Tooltip("This will import the developer names, matching via row ID.");

                    ImGui.EndMenu();
                }

                // Only these projects have Developer Names
                if (Project.ProjectType is ProjectType.AC6 or ProjectType.BB)
                {
                    if (ImGui.BeginMenu("Developer Names"))
                    {
                        if (ImGui.MenuItem($"By Index"))
                        {
                            Project.ParamData.PrimaryBank.ImportRowNames(ImportRowNameType.Index, ImportRowNameSourceType.Developer);
                        }
                        UIHelper.Tooltip("This will import the community names, matching via row index. Warning: this will not function as desired if you have edited the row order.");

                        if (ImGui.MenuItem($"By ID"))
                        {
                            Project.ParamData.PrimaryBank.ImportRowNames(ImportRowNameType.ID, ImportRowNameSourceType.Developer);
                        }
                        UIHelper.Tooltip("This will import the developer names, matching via row ID.");

                        ImGui.EndMenu();
                    }
                }

                if (ImGui.BeginMenu("From File"))
                {
                    if (ImGui.MenuItem($"By Index"))
                    {
                        var filePath = PathUtils.GetFileSelection();

                        if (filePath != "")
                        {
                            Project.ParamData.PrimaryBank.ImportRowNames(ImportRowNameType.Index, ImportRowNameSourceType.External, filePath);
                        }
                    }
                    UIHelper.Tooltip("This will import the external names, matching via row index. Warning: this will not function as desired if you have edited the row order.");

                    if (ImGui.MenuItem($"By ID"))
                    {
                        var filePath = PathUtils.GetFileSelection();

                        if (filePath != "")
                        {
                            Project.ParamData.PrimaryBank.ImportRowNames(ImportRowNameType.ID, ImportRowNameSourceType.External, filePath);
                        }
                    }
                    UIHelper.Tooltip("This will import the external names, matching via row ID.");

                    ImGui.EndMenu();
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Export"))
            {
                if (ImGui.BeginMenu("JSON"))
                {
                    if (ImGui.MenuItem("All"))
                    {
                        var filePath = PathUtils.GetFolderSelection();

                        if (filePath != "")
                        {
                            Project.ParamData.PrimaryBank.ExportRowNames(ExportRowNameType.JSON, filePath);
                        }
                    }
                    UIHelper.Tooltip("Export the row names for your project to the selected folder.");


                    if (ImGui.MenuItem("Selected Param"))
                    {
                        var filePath = PathUtils.GetFolderSelection();

                        if (filePath != "")
                        {
                            Project.ParamData.PrimaryBank.ExportRowNames(ExportRowNameType.JSON, filePath, Selection._selectedParamName);
                        }
                    }
                    UIHelper.Tooltip("Export the row names for the currently selected param to the selected folder.");

                    ImGui.EndMenu();
                }
                UIHelper.Tooltip("Export file will use the JSON storage format.");

                if (ImGui.BeginMenu("Text"))
                {
                    if (ImGui.MenuItem("All"))
                    {
                        var filePath = PathUtils.GetFolderSelection();

                        if (filePath != "")
                        {
                            Project.ParamData.PrimaryBank.ExportRowNames(ExportRowNameType.Text, filePath);
                        }
                    }
                    UIHelper.Tooltip("Export the row names for your project to the selected folder.");


                    if (ImGui.MenuItem("Selected Param"))
                    {
                        var filePath = PathUtils.GetFolderSelection();

                        if (filePath != "")
                        {
                            Project.ParamData.PrimaryBank.ExportRowNames(ExportRowNameType.Text, filePath, Selection._selectedParamName);
                        }
                    }
                    UIHelper.Tooltip("Export the row names for the currently selected param to the selected folder.");

                    ImGui.EndMenu();
                }
                UIHelper.Tooltip("Export file will use the Text storage format. This format cannot be imported back in.");

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }

    private void ParamDataOptions()
    {

        if (ImGui.BeginMenu("Param Data"))
        {

            ImGui.EndMenu();
        }
    }
}
