using Hexa.NET.ImGui;
using Smithbox.Core.Editor;
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

    // Defined here so we can remove NoMove when setting up the imgui.ini
    private ImGuiWindowFlags MainWindowFlags = ImGuiWindowFlags.MenuBar; //| ImGuiWindowFlags.NoMove;
    private ImGuiWindowFlags SubWindowFlags = ImGuiWindowFlags.None;

    public int ID = 0;

    public bool Initialized = false;

    public BehaviorEditor(int id, Project projectOwner)
    {
        Project = projectOwner;
        ID = id;

        ActionManager = new();

        Selection = new(this);
        TreeView = new(Project, this);
        NodeView = new(Project, this);
        FieldView = new(Project, this);

        Initialize();
    }

    public async void Initialize()
    {
        if (Initialized)
            return;

        // TODO: use Project.FileDictionary to build list of behbnds

        Initialized = true;
    }

    public void Draw()
    {
        ImGui.Begin($"Behavior Editor##Behavior Editor", MainWindowFlags);

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
        if (ImGui.IsWindowHovered())
        {

        }
    }
    private async void Save()
    {
        Task<bool> saveTask = Project.BehaviorData.PrimaryBank.Save();
        bool saveTaskFinished = await saveTask;

        if (saveTaskFinished)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Behavior Editor] Saved behavior file.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Behavior Editor] Failed to save behavior file.");
        }
    }
}
