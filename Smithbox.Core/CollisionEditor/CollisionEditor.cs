using Hexa.NET.ImGui;
using Smithbox.Core.Editor;
using Smithbox.Core.Interface.Input;
using Smithbox.Core.Utils;
using System.Numerics;

namespace Smithbox.Core.CollisionEditorNS;

public class CollisionEditor
{
    private Project Project;

    public ActionManager ActionManager;
    public CollisionSelection Selection;

    public CollisionTreeView TreeView;
    public CollisionNodeView NodeView;
    public CollisionViewport ViewportView;
    public CollisionFieldView FieldView;
    public CollisionFieldInput FieldInput;

    public int ID = 0;

    public bool DetectShortcuts = false;

    public CollisionEditor(int id, Project projectOwner)
    {
        Project = projectOwner;
        ID = id;

        ActionManager = new();

        FieldInput = new(Project, this);

        Selection = new(Project, this);
        TreeView = new(Project, this);
        NodeView = new(Project, this);
        ViewportView = new(Project, this);
        FieldView = new(Project, this);
    }

    public void Draw()
    {
        ImGui.Begin($"Collision Editor##CollisionEditor", Project.Source.MainWindowFlags);

        DetectShortcuts = ShortcutUtils.UpdateShortcutDetection();

        uint dockspaceID = ImGui.GetID("CollisionEditorDockspace");
        ImGui.DockSpace(dockspaceID, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);

        Menubar();
        Shortcuts();

        ImGui.End();

        ImGui.Begin($"Binders##CollisionFileList", Project.Source.SubWindowFlags);

        DisplayBinderList();

        ImGui.End();

        ImGui.Begin($"Binder Files##CollisionInternalFileList", Project.Source.SubWindowFlags);

        DisplayBinderFileList();

        ImGui.End();

        ImGui.Begin($"Havok Objects##CollisionTreeView", Project.Source.SubWindowFlags);

        TreeView.Draw();

        ImGui.End();

        ImGui.Begin($"Nodes##CollisionNodeView", Project.Source.SubWindowFlags);

        NodeView.Draw();

        ImGui.End();

        ImGui.Begin($"Viewport##CollisionViewport", Project.Source.SubWindowFlags);

        ViewportView.Draw();

        ImGui.End();

        ImGui.Begin($"Fields##CollisionFieldView", Project.Source.SubWindowFlags);

        FieldView.Draw();

        ImGui.End();
    }

    private void DisplayBinderList()
    {
        ImGui.BeginChild("collisionBinderList");

        for (int i = 0; i < Project.CollisionData.CollisionFiles.Entries.Count; i++)
        {
            var curEntry = Project.CollisionData.CollisionFiles.Entries[i];

            var isSelected = Selection.IsFileSelected(i, curEntry.Filename);

            if (ImGui.Selectable($"{curEntry.Filename}##fileEntry{i}", isSelected))
            {
                Selection.SelectFile(i, curEntry.Filename);

                Project.CollisionData.PrimaryBank.LoadBinder(curEntry.Filename, curEntry.Path);
            }
        }

        ImGui.EndChild();
    }

    private void DisplayBinderFileList()
    {
        ImGui.BeginChild("collisionBinderFileList");

        if (Project.CollisionData.PrimaryBank.Binders.ContainsKey(Selection._selectedFileName))
        {
            var targetBinder = Project.CollisionData.PrimaryBank.Binders[Selection._selectedFileName];

            for (int i = 0; i < targetBinder.Files.Count; i++)
            {
                var curEntry = targetBinder.Files[i];
                var displayName = curEntry.Name;
                var isSelected = Selection.IsInternalFileSelected(i, curEntry.Name);

                // Ignore the compendium file
                if (curEntry.Name.Contains(".compendium"))
                    continue;

                if (ImGui.Selectable($"{displayName}##internalFileEntry{i}", isSelected))
                {
                    Selection.SelectInternalFile(i, curEntry.Name);

                    Project.CollisionData.PrimaryBank.LoadInternalFile();
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
        Task<bool> saveTask = Project.CollisionData.PrimaryBank.Save();
        bool saveTaskFinished = await saveTask;

        if (saveTaskFinished)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Collision Editor] Saved collision file: {Selection._selectedFileName}");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Collision Editor] Failed to save collision file: {Selection._selectedFileName}");
        }
    }
}
