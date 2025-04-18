﻿using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Widgets;
using Hexa.NET.ImGui.Widgets.Dialogs;
using Hexa.NET.ImGui.Widgets.Extras.TextEditor;
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

        Selection = new(this);

        FieldDecorator = new(Project, this);
        FieldInput = new(Project, this);
        SearchEngine = new(Project, this);

        ParamView = new(Project, this);
        RowView = new(Project, this);
        FieldView = new(Project, this);
        FieldImageView = new(Project, this);
    }

    public void Draw(Command cmd)
    {
        ImGui.Begin($"Param Editor##ParamEditor{ID}", MainWindowFlags);

        uint dockspaceID = ImGui.GetID($"ParamEditorDockspace{ID}");
        ImGui.DockSpace(dockspaceID, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);

        ProcessCommand(cmd);
        Menubar();

        if (Project.ParamData.Initialized)
        {
            FieldView.SetupFieldOrder();
            ParamActions.Draw();

            if (Project.IsSelected)
            {
                ParamView.Draw(cmd);
                RowView.Draw(cmd);
                FieldView.Draw(cmd);
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

    /// <summary>
    /// Editor Command handling
    /// </summary>
    /// <param name="cmd"></param>
    private void ProcessCommand(Command cmd)
    {
        if (cmd == null)
            return;

        if (cmd.Editor is EditorTarget.ParamEditor)
        {
            var instructions = cmd.Instructions;
            var action = instructions[0];

            if (action == "select")
            {
                var location = instructions[1];

                if (location == "param")
                {

                }
                if (location == "row")
                {

                }
                if (location == "field")
                {

                }
            }
        }
    }
}
