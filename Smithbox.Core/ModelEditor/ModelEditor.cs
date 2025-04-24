using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;
using Hexa.NET.Mathematics;
using Smithbox.Core.Editor;
using Smithbox.Core.Interface.Input;
using Smithbox.Core.Utils;
using Smithbox_Core;
using System.Numerics;

namespace Smithbox.Core.ModelEditorNS;

public class ModelEditor
{
    private Project Project;

    public int ID = 0;

    public ActionManager ActionManager;

    public ModelSelection Selection;

    public ModelFileList FileList;
    public ModelInternalFileList InternalFileList;
    public ModelDataView DataView;
    public ModelViewportView ViewportView;
    public ModelFieldView FieldView;
    public ModelFieldInput FieldInput;

    public ModelEditor(int id, Project projectOwner)
    {
        Project = projectOwner;
        ID = id;

        ActionManager = new();

        Selection = new(Project, this);
        FieldInput = new(Project, this);

        FileList = new(Project, this);
        InternalFileList = new(Project, this);
        DataView = new(Project, this);
        ViewportView = new(Project, this);
        FieldView = new(Project, this);
    }

    public void Draw()
    {
        ImGui.Begin($"Model Editor##ModelEditor{ID}", Project.Source.MainWindowFlags);

        DetectShortcuts = ShortcutUtils.UpdateShortcutDetection();

        uint dockspaceID = ImGui.GetID($"ModelEditorDockspace{ID}");
        ImGui.DockSpace(dockspaceID, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);

        Menubar();
        Shortcuts();

        ImGui.End();

        ImGui.Begin($"File List##ModelFileList", Project.Source.SubWindowFlags);

        FileList.Draw();

        ImGui.End();

        ImGui.Begin($"Internal File List##ModelInternalFileList", Project.Source.SubWindowFlags);

        InternalFileList.Draw();

        ImGui.End();

        ImGui.Begin($"Model Data##ModelDataView", Project.Source.SubWindowFlags);

        DataView.Draw();

        ImGui.End();

        ImGui.Begin($"Viewport##ModelViewport", Project.Source.SubWindowFlags);

        ViewportView.Draw();

        ImGui.End();

        ImGui.Begin($"Fields##ModelFieldView", Project.Source.SubWindowFlags);

        FieldView.Draw();

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

    private bool DetectShortcuts = false;

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
        //Task<bool> saveTask = Project.BehaviorData.PrimaryBank.Save();
        //bool saveTaskFinished = await saveTask;

        //if (saveTaskFinished)
        //{
        //    TaskLogs.AddLog($"[{Project.ProjectName}:Behavior Editor] Saved behavior file: {Selection._selectedFileName}");
        //}
        //else
        //{
        //    TaskLogs.AddLog($"[{Project.ProjectName}:Behavior Editor] Failed to save behavior file: {Selection._selectedFileName}");
        //}
    }
}
