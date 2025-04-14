using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Widgets;
using Hexa.NET.ImGui.Widgets.Dialogs;
using Hexa.NET.ImGui.Widgets.Extras.TextEditor;
using Smithbox.Core.Editor;
using Smithbox.Core.Interface;
using Smithbox.Core.Interface.Input;
using Smithbox.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
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

    public ParamEditor(int id, Project projectOwner)
    {
        Project = projectOwner;
        ID = id;

        ActionManager = new();
        ParamActions = new(this);
    }

    public void Draw(string[] cmd)
    {
        ImGui.Begin($"Param Editor##ParamEditor{ID}", MainWindowFlags);

        uint dockspaceID = ImGui.GetID($"ParamEditorDockspace{ID}");
        ImGui.DockSpace(dockspaceID, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);

        Menubar();

        if (Project.ParamData.Initialized)
        {
            ParamActions.Draw();

            if (Project.IsSelected)
            {
                DisplayEditor();
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

                ImGui.Separator();

                if (ImGui.MenuItem("Duplicate Row"))
                {
                    ParamActions.DuplicateRow();
                }
                UIHelper.Tooltip($"Duplicates current selection.");

                if (ImGui.BeginMenu("Duplicate Row to Commutative Param", ParamActions.IsCommutativeParam()))
                {
                    ParamActions.DisplayCommutativeDuplicateMenu();

                    ImGui.EndMenu();
                }
                UIHelper.Tooltip($"Duplicates current selection to a commutative param.");

                if (ImGui.MenuItem("Remove Row"))
                {
                    ParamActions.DeleteRow();
                }
                UIHelper.Tooltip($"Deletes current selection.");

                if (ImGui.MenuItem("Copy Row"))
                {
                    ParamActions.CopyRow();
                }
                UIHelper.Tooltip($"Copy current selection to clipboard.");

                if (ImGui.MenuItem("Paste Row"))
                {
                    ParamActions.PasteRow();
                }
                UIHelper.Tooltip($"Paste current selection into current param.");

                if (ImGui.MenuItem("Go to Selected Row"))
                {
                    ParamActions.GoToRow();
                }
                UIHelper.Tooltip($"Go to currently selected row.");

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

    private bool DetectShortcuts = false;

    /// <summary>
    /// Called after the windows
    /// </summary>
    private void Shortcuts()
    {
        if (DetectShortcuts)
        {
            if (Keyboard.KeyPress(Key.S))
            {
                Save();
            }
        }
    }

    // TEMP
    private string _selectedParam = "";

    private void DisplayEditor()
    {
        DetectShortcuts = false;

        ImGui.Begin($"Params##ParamList{ID}", SubWindowFlags);

        if (ImGui.IsWindowFocused())
        {
            DetectShortcuts = true;
        }

        for(int i = 0; i < Project.ParamData.PrimaryBank.Params.Count; i++)
        {
            var entry = Project.ParamData.PrimaryBank.Params.ElementAt(i);

            if (ImGui.Selectable($"{entry.Key}##paramEntry{i}"))
            {
                _selectedParam = entry.Key;
            }
        }

        ImGui.End();

        ImGui.Begin($"Rows##ParamRowList{ID}", SubWindowFlags);

        if (ImGui.IsWindowFocused())
        {
            DetectShortcuts = true;
        }

        if(_selectedParam != "")
        {
            var curParam = Project.ParamData.PrimaryBank.Params[_selectedParam];

            for (int i = 0; i < curParam.Rows.Count; i++)
            {
                var curRow = curParam.Rows[i];

                var rowName = $"{i}:{curRow.ID}";
                if(curRow.Name != null)
                {
                    rowName = $"{rowName} {curRow.Name}";
                }

                if (ImGui.Selectable($"{rowName}##rowEntry{i}"))
                {

                }
            }
        }

        ImGui.End();

        ImGui.Begin($"Fields##ParamRowFieldEditor{ID}", SubWindowFlags);

        if (ImGui.IsWindowFocused())
        {
            DetectShortcuts = true;
        }

        ImGui.End();
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
}
