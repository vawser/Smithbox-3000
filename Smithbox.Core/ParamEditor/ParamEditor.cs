using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Widgets;
using Hexa.NET.ImGui.Widgets.Extras.TextEditor;
using Smithbox.Core.Editor;
using Smithbox.Core.Interface;
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

    public int ID = 0;

    public ActionManager ActionManager;
    public ParamActions ParamActions;

    private TextEditor textEditor;

    public ParamEditor(int id, Project projectOwner)
    {
        Project = projectOwner;
        ID = id;

        ActionManager = new();
        ParamActions = new(this);

        textEditor = new("test", new TextSource("test text"));
    }

    public void Draw()
    {
        ImGui.Begin($"Param Editor##ParamEditor{ID}", ImGuiWindowFlags.MenuBar);

        Menubar();

        if (Project.ParamData.Initialized)
        {
            Shortcuts();
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
        else
        {
            ImGuiSpinner.Spinner(50f, 5.0f, ColorUtils.ColorFromVec4(UI.Current.ImGui_Highlight_Text));
        }

        ImGui.End();
    }

    private void Menubar()
    {
        if (ImGui.BeginMenuBar())
        {
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

            ImGui.EndMenuBar();
        }
    }

    private void Shortcuts()
    {
        if (ImGui.IsWindowHovered())
        {

        }
    }

    private void DisplayEditor()
    {

    }
}
