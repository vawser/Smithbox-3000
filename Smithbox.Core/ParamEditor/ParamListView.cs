using Hexa.NET.ImGui;
using Smithbox.Core.Editor;
using Smithbox.Core.Interface;
using Smithbox.Core.Interface.Input;
using Smithbox.Core.Utils;
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

    public bool DetectShortcuts = false;

    public ParamListView(Project curProject, ParamEditor editor)
    {
        ID = editor.ID;
        Project = curProject;
        Editor = editor;
    }

    public void Draw()
    {
        ImGui.Begin($"Params##ParamList{ID}", Project.Source.SubWindowFlags);

        DetectShortcuts = ShortcutUtils.UpdateShortcutDetection();

        DisplayHeader();

        ImGui.BeginChild("paramListArea");

        for (int i = 0; i < Project.ParamData.PrimaryBank.Params.Count; i++)
        {
            var entry = Project.ParamData.PrimaryBank.Params.ElementAt(i);

            var isSelected = Editor.Selection.IsParamSelected(i, entry.Key, entry.Value);

            if (ImGui.Selectable($"{entry.Key}##paramEntry{i}", isSelected))
            {
                Editor.Selection.SelectParam(i, entry.Key, entry.Value);
            }
        }

        ImGui.EndChild();

        ImGui.End();

        Shortcuts();
    }

    private bool FocusParamSearch = false;

    public void Shortcuts()
    {
        if (DetectShortcuts)
        {
            // Focus Param Search
            if (Keyboard.KeyPress(Key.F) && ImGui.IsKeyDown(ImGuiKey.LeftCtrl))
            {
                FocusParamSearch = true;
            }

            // Clear Param Search
            if (Keyboard.KeyPress(Key.G) && ImGui.IsKeyDown(ImGuiKey.LeftCtrl))
            {
                Editor.SearchEngine.ParamFilterInput = "";
                // TODO: update param visibility
            }
        }
    }

    private void DisplayHeader()
    {
        // Search Builder
        var searchWidth = ImGui.GetWindowWidth() * 0.5f;

        if (ImGui.Button($"{Icons.ArrowCircleLeft}"))
        {
            Editor.SearchEngine.WindowPosition = ImGui.GetCursorScreenPos();
            Editor.SearchEngine.ParamSearch_DisplayTermBuilder = true;
        }
        UIHelper.Tooltip("View the search term builder.");

        // Search Bar
        ImGui.SameLine();

        if (FocusParamSearch)
        {
            FocusParamSearch = false;
            ImGui.SetKeyboardFocusHere();
        }

        ImGui.SetNextItemWidth(searchWidth);
        ImGui.InputText($"##paramSearch_{ID}", ref Editor.SearchEngine.ParamFilterInput, 128);

        // Search
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Search}"))
        {
            // TODO: update param visibility
        }
        UIHelper.Tooltip("Filter the param list.");

        // Clear
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Times}"))
        {
            Editor.SearchEngine.ParamFilterInput = "";
            // TODO: update param visibility
        }
        UIHelper.Tooltip("Clear the param list filter.");

        // Regex Match Mode
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Eye}"))
        {
            Editor.SearchEngine.ParamSearch_IsRegexLenient = !Editor.SearchEngine.ParamSearch_IsRegexLenient;
        }

        var regexMode = "Strict";
        if (Editor.SearchEngine.ParamSearch_IsRegexLenient)
            regexMode = "Lenient";

        UIHelper.Tooltip($"Toggle whether regular expressions are run lenient or strict.\nCurrent Mode: {regexMode}");
    }
}

