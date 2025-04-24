using Hexa.NET.ImGui;
using Smithbox.Core.Editor;
using Smithbox.Core.Interface.Input;
using Smithbox.Core.ParamEditorNS;
using Smithbox.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.BehaviorEditorNS;

public class BehaviorEditor
{
    private Project Project;

    public ActionManager ActionManager;
    public BehaviorSelection Selection;

    public BehaviorTreeView TreeView;
    public BehaviorNodeView NodeView;
    public BehaviorFieldView FieldView;
    public BehaviorFieldInput FieldInput;

    // Defined here so we can remove NoMove when setting up the imgui.ini
    private ImGuiWindowFlags MainWindowFlags = ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoMove;
    private ImGuiWindowFlags SubWindowFlags = ImGuiWindowFlags.NoMove;

    public int ID = 0;

    public bool DetectShortcuts = false;

    public BehaviorEditor(int id, Project projectOwner)
    {
        Project = projectOwner;
        ID = id;

        ActionManager = new();

        FieldInput = new(Project, this);

        Selection = new(Project, this);
        TreeView = new(Project, this);
        NodeView = new(Project, this);
        FieldView = new(Project, this);
    }

    public void Draw()
    {
        ImGui.Begin($"Behavior Editor##Behavior Editor", MainWindowFlags);

        DetectShortcuts = ShortcutUtils.UpdateShortcutDetection();

        uint dockspaceID = ImGui.GetID("BehaviorEditorDockspace");
        ImGui.DockSpace(dockspaceID, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);

        Menubar();
        Shortcuts();

        ImGui.End();

        ImGui.Begin($"Behavior List##BehaviorList", SubWindowFlags);

        DisplayBehaviorList();

        ImGui.End();

        ImGui.Begin($"Havok Objects##BehaviorTreeView", SubWindowFlags);

        TreeView.Draw();

        ImGui.End();

        ImGui.Begin($"Nodes##BehaviorNodeView", SubWindowFlags);

        NodeView.Draw();

        ImGui.End();

        ImGui.Begin($"Fields##BehaviorFieldView", SubWindowFlags);

        FieldView.Draw();

        ImGui.End();
    }

    private void DisplayBehaviorList()
    {
        DisplayHeader();

        ImGui.BeginChild("behaviorSelectionList");

        for (int i = 0; i < Project.BehaviorData.BehaviorFiles.Entries.Count; i++)
        {
            var curEntry = Project.BehaviorData.BehaviorFiles.Entries[i];

            var isSelected = Selection.IsFileSelected(i, curEntry.Filename);

            if (ImGui.Selectable($"{curEntry.Filename}##fileEntry{i}", isSelected))
            {
                Selection.SelectFile(i, curEntry.Filename);
                Project.BehaviorData.LoadBinder(curEntry.Filename, curEntry.Path, Project.BehaviorData.PrimaryBank);
            }
        }

        ImGui.EndChild();
    }
    public void DisplayHeader()
    {

    }

    private void Menubar()
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

            ImGui.EndMenuBar();
        }
    }

    private void Shortcuts()
    {
        if (DetectShortcuts)
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
        Task<bool> saveTask = Project.BehaviorData.PrimaryBank.Save();
        bool saveTaskFinished = await saveTask;

        if (saveTaskFinished)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Behavior Editor] Saved behavior file: {Project.BehaviorData.PrimaryBank.CurrentBinderName}");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Behavior Editor] Failed to save behavior file: {Project.BehaviorData.PrimaryBank.CurrentBinderName}");
        }
    }
}
