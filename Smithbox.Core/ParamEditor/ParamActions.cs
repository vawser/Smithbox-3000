using Hexa.NET.ImGui;
using Smithbox.Core.Actions;
using Smithbox.Core.Editor;
using Smithbox.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.ParamEditorNS;

public class ParamActions
{
    private ParamEditor Editor;

    public Vector2 WindowPosition;

    public bool DisplayDuplicateMenu;

    public ParamActions(ParamEditor curEditor)
    {
        Editor = curEditor;
    }

    public void Draw()
    {
        if (DisplayDuplicateMenu)
        {
            DuplicateMenu();
        }
    }

    public void DuplicateRowMenu()
    {
        WindowPosition = ImGui.GetCursorScreenPos();
        DisplayDuplicateMenu = true;
    }

    private void DuplicateMenu()
    {
        var flags = ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize;
        var buttonSize = new Vector2(250, 24);

        ImGui.SetNextWindowPos(WindowPosition);
        if (ImGui.Begin("Duplicate##DuplicateOptions", ref DisplayDuplicateMenu, flags))
        {
            ImGui.InputInt("Offset##duplicateOffset", ref CFG.Current.ParamRowDuplicateOffset);

            // Duplicate
            if (ImGui.Button("Duplicate##duplicateAction", buttonSize))
            {
                DuplicateRow(CFG.Current.ParamRowDuplicateOffset);
                DisplayDuplicateMenu = false;
            }
            UIHelper.Tooltip("Close the term builder.");

            // Cancel
            if (ImGui.Button("Cancel##cancelDuplicate", buttonSize))
            {
                DisplayDuplicateMenu = false;
            }
            UIHelper.Tooltip("Close the term builder.");

            ImGui.End();
        }
    }

    public void DuplicateRow(int idOffset)
    {
        var param = Editor.Selection.GetSelectedParam();
        var rows = Editor.Selection._selectedRows;

        var action = new AddParamRow(Editor, param, rows, idOffset);
        Editor.ActionManager.ExecuteAction(action);
    }

    public void DeleteRow()
    {
        var param = Editor.Selection.GetSelectedParam();
        var rows = Editor.Selection._selectedRows;
        var paramRows = rows.Select(e => e.Row).ToList();

        var action = new DeleteParamRow(Editor, param, paramRows);
        Editor.ActionManager.ExecuteAction(action);
    }

    public void CopyRow()
    {
        /*
        if (_activeView._selection.RowSelectionExists())
        {
            CopySelectionToClipboard();
        }
        */
    }

    public void PasteRow()
    {
        /*
        if (ParamBank.ClipboardRows.Any())
        {
            EditorCommandQueue.AddCommand(@"param/menu/ctrlVPopup");
        }
        */
    }

    public void GoToRow()
    {
        /*
        if (_activeView._selection.RowSelectionExists())
        {
            GotoSelectedRow = true;
        }
        */
    }

    public bool IsCommutativeParam()
    {
        return true;
    }

    public void DisplayCommutativeDuplicateMenu()
    {

    }
}
