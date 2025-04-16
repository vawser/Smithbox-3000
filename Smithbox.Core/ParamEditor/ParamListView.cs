using Hexa.NET.ImGui;
using Smithbox.Core.Editor;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.ParamEditorNS;

public class ParamListView
{
    public Project Project;
    public ParamEditor Editor;

    private int ID;

    // Defined here so we can remove NoMove when setting up the imgui.ini
    private ImGuiWindowFlags MainWindowFlags = ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoMove;
    private ImGuiWindowFlags SubWindowFlags = ImGuiWindowFlags.NoMove;

    public bool DetectShortcuts = false;

    public ParamListView(Project curProject, ParamEditor editor)
    {
        ID = editor.ID;
        Project = curProject;
        Editor = editor;
    }

    public void Draw(string[] cmd)
    {
        ImGui.Begin($"Params##ParamList{ID}", SubWindowFlags);

        if (ImGui.IsWindowFocused())
        {
            DetectShortcuts = true;
        }

        for (int i = 0; i < Project.ParamData.PrimaryBank.Params.Count; i++)
        {
            var entry = Project.ParamData.PrimaryBank.Params.ElementAt(i);

            var isSelected = Editor.Selection.IsParamSelected(i, entry.Key, entry.Value);

            if (ImGui.Selectable($"{entry.Key}##paramEntry{i}", isSelected))
            {
                Editor.Selection.SelectParam(i, entry.Key, entry.Value);
            }
        }

        ImGui.End();
    }
}

