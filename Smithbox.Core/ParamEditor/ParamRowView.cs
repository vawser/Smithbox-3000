using Hexa.NET.ImGui;
using Smithbox.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.ParamEditorNS;

public class ParamRowView
{
    public Project Project;
    public ParamEditor Editor;

    private int ID;

    // Defined here so we can remove NoMove when setting up the imgui.ini
    private ImGuiWindowFlags MainWindowFlags = ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoMove;
    private ImGuiWindowFlags SubWindowFlags = ImGuiWindowFlags.NoMove;

    public bool DetectShortcuts = false;

    public ParamRowView(Project curProject, ParamEditor editor)
    {
        ID = editor.ID;
        Project = curProject;
        Editor = editor;
    }

    public void Draw(string[] cmd)
    {
        ImGui.Begin($"Rows##ParamRowList{ID}", SubWindowFlags);

        if (ImGui.IsWindowFocused())
        {
            DetectShortcuts = true;
        }

        if (Editor.Selection._selectedParam != null)
        {
            var curParam = Project.ParamData.PrimaryBank.Params[Editor.Selection._selectedParamName];

            for (int i = 0; i < curParam.Rows.Count; i++)
            {
                var curRow = curParam.Rows[i];

                var rowName = $"{i}:{curRow.ID}";
                if (curRow.Name != null)
                {
                    rowName = $"{rowName} {curRow.Name}";
                }

                var isSelected = Editor.Selection.IsRowSelected(i, curRow);

                if (ImGui.Selectable($"{rowName}##rowEntry{i}", isSelected))
                {
                    Editor.Selection.SelectRow(i, curRow);
                }
            }
        }

        ImGui.End();
    }
}
