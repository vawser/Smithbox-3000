using Hexa.NET.ImGui;
using Smithbox.Core.Editor;
using Smithbox.Core.Interface;
using Smithbox.Core.Interface.Input;
using Smithbox.Core.Utils;
using System.Numerics;

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
        ImGui.Begin($"Behavior Editor##Behavior Editor", Project.Source.MainWindowFlags);

        DetectShortcuts = ShortcutUtils.UpdateShortcutDetection();

        uint dockspaceID = ImGui.GetID("BehaviorEditorDockspace");
        ImGui.DockSpace(dockspaceID, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);

        Menubar();
        Shortcuts();

        ImGui.End();

        ImGui.Begin($"Binders##BehaviorFileList", Project.Source.SubWindowFlags);

        DisplayBinderList();

        ImGui.End();

        ImGui.Begin($"Binder Files##BehaviorInternalFileList", Project.Source.SubWindowFlags);

        DisplayBinderFileList();

        ImGui.End();

        ImGui.Begin($"Havok Objects##BehaviorTreeView", Project.Source.SubWindowFlags);

        TreeView.Draw();

        ImGui.End();

        ImGui.Begin($"Nodes##BehaviorNodeView", Project.Source.SubWindowFlags);

        NodeView.Draw();

        ImGui.End();

        ImGui.Begin($"Fields##BehaviorFieldView", Project.Source.SubWindowFlags);

        FieldView.Draw();

        ImGui.End();
    }

    private void DisplayBinderList()
    {
        ImGui.BeginChild("behaviorBinderList");

        for (int i = 0; i < Project.BehaviorData.BehaviorFiles.Entries.Count; i++)
        {
            var curEntry = Project.BehaviorData.BehaviorFiles.Entries[i];

            var isSelected = Selection.IsFileSelected(i, curEntry.Filename);

            if (ImGui.Selectable($"{curEntry.Filename}##fileEntry{i}", isSelected))
            {
                Selection.SelectFile(i, curEntry.Filename);

                Project.BehaviorData.PrimaryBank.LoadBinder(curEntry.Filename, curEntry.Path);
            }

            if (Project.Aliases.Characters.Any(e => e.ID.ToLower() == curEntry.Filename.ToLower()))
            {
                var nameEntry = Project.Aliases.Characters.Where(e => e.ID.ToLower() == curEntry.Filename.ToLower()).FirstOrDefault();

                if (nameEntry != null)
                {
                    UIHelper.DisplayAlias(nameEntry.Name);
                }
            }
        }

        ImGui.EndChild();
    }
    private void DisplayBinderFileList()
    {
        ImGui.BeginChild("behaviorBinderFileList");

        if(Project.BehaviorData.PrimaryBank.Binders.ContainsKey(Selection._selectedFileName))
        {
            var targetBinder = Project.BehaviorData.PrimaryBank.Binders[Selection._selectedFileName];

            for(int i = 0; i < targetBinder.Files.Count; i++)
            {
                var curEntry = targetBinder.Files[i];
                var displayName = BehaviorUtils.GetInternalFileTitle(curEntry.Name);
                var isSelected = Selection.IsInternalFileSelected(i, curEntry.Name);

                if (ImGui.Selectable($"{displayName}##internalFileEntry{i}", isSelected))
                {
                    Selection.SelectInternalFile(i, curEntry.Name);

                    Project.BehaviorData.PrimaryBank.LoadInternalFile();
                }
            }
        }

        ImGui.EndChild();
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
            TaskLogs.AddLog($"[{Project.ProjectName}:Behavior Editor] Saved behavior file: {Selection._selectedFileName}");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Behavior Editor] Failed to save behavior file: {Selection._selectedFileName}");
        }
    }
}
