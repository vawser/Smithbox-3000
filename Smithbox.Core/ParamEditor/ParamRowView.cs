using Hexa.NET.ImGui;
using Smithbox.Core.Editor;
using Smithbox.Core.Interface;
using Smithbox.Core.Interface.Input;
using Smithbox.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.ParamEditorNS;

public class ParamRowView
{
    public Project Project;
    public ParamEditor Editor;

    private int ID;

    public bool DetectShortcuts = false;

    public ParamRowView(Project curProject, ParamEditor editor)
    {
        ID = editor.ID;
        Project = curProject;
        Editor = editor;
    }

    public void Draw()
    {
        ImGui.Begin($"Rows##ParamRowList{ID}", Project.Source.SubWindowFlags);

        DetectShortcuts = ShortcutUtils.UpdateShortcutDetection();

        if (Editor.Selection._selectedParam != null)
        {
            DisplayHeader();

            ImGui.BeginChild("rowListArea");

            CurrentRowSelectionMode = SelectMode.ClearAndSelect;

            ListShortcuts();

            var curParam = Project.ParamData.PrimaryBank.Params[Editor.Selection._selectedParamName];

            ImGuiListClipper clipper = new ImGuiListClipper();
            clipper.Begin(curParam.Rows.Count);

            while (clipper.Step())
            {
                for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var curRow = curParam.Rows[i];

                    var rowName = $"{i}:{curRow.ID}";
                    if (curRow.Name != null)
                    {
                        rowName += $" {curRow.Name}";
                    }

                    bool isSelected = Editor.Selection.IsRowSelected(i, curRow);

                    if (ImGui.Selectable($"{rowName}##rowEntry{i}", isSelected))
                    {
                        Editor.Selection.SelectRow(i, curRow, CurrentRowSelectionMode);
                    }

                    if (CurrentRowSelectionMode is SelectMode.SelectAll)
                    {
                        Editor.Selection.SelectRow(i, curRow, CurrentRowSelectionMode);
                    }
                }
            }

            ImGui.EndChild();
        }

        ImGui.End();

        Shortcuts();
    }

    private SelectMode CurrentRowSelectionMode;
    private bool FocusRowSearch = false;

    public void Shortcuts()
    {
        if (DetectShortcuts)
        {
            // Focus Row Search
            if (Keyboard.KeyPress(Key.F) && ImGui.IsKeyDown(ImGuiKey.LeftCtrl))
            {
                FocusRowSearch = true;
            }

            // Clear Row Search
            if (Keyboard.KeyPress(Key.G) && ImGui.IsKeyDown(ImGuiKey.LeftCtrl))
            {
                Editor.SearchEngine.RowFilterInput = "";
                // TODO: update row visibility
            }
        }
    }
    public void ListShortcuts()
    {
        if (DetectShortcuts)
        {
            // Append
            if (ImGui.IsKeyDown(ImGuiKey.LeftCtrl))
            {
                CurrentRowSelectionMode = SelectMode.SelectAppend;
            }

            // Range Append
            if (ImGui.IsKeyDown(ImGuiKey.LeftShift))
            {
                CurrentRowSelectionMode = SelectMode.SelectRangeAppend;
            }

            // Select All
            if (DetectShortcuts && Keyboard.KeyPress(Key.A) && ImGui.IsKeyDown(ImGuiKey.LeftCtrl))
            {
                CurrentRowSelectionMode = SelectMode.SelectAll;
            }

            // Duplicate
            if (DetectShortcuts && Keyboard.KeyPress(Key.D) && ImGui.IsKeyDown(ImGuiKey.LeftCtrl))
            {
                // Duplicate selection
            }

            // Delete
            if (DetectShortcuts && Keyboard.KeyPress(Key.Delete) && ImGui.IsKeyDown(ImGuiKey.LeftCtrl))
            {
                // Delete selection
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
            Editor.SearchEngine.RowSearch_DisplayTermBuilder = true;
        }
        UIHelper.Tooltip("View the search term builder.");

        // Search Bar
        ImGui.SameLine();

        if (FocusRowSearch)
        {
            FocusRowSearch = false;
            ImGui.SetKeyboardFocusHere();
        }

        ImGui.SetNextItemWidth(searchWidth);
        ImGui.InputText($"##rowSearch_{ID}", ref Editor.SearchEngine.RowFilterInput, 128);

        // Search
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Search}"))
        {
            // TODO: update row visibility
        }
        UIHelper.Tooltip("Filter the row list.");

        // Clear
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Times}"))
        {
            Editor.SearchEngine.RowFilterInput = "";
            // TODO: update row visibility
        }
        UIHelper.Tooltip("Clear the row list filter.");

        // Regex Match Mode
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Eye}"))
        {
            Editor.SearchEngine.RowSearch_IsRegexLenient = !Editor.SearchEngine.RowSearch_IsRegexLenient;
        }

        var regexMode = "Strict";
        if (Editor.SearchEngine.RowSearch_IsRegexLenient)
            regexMode = "Lenient";

        UIHelper.Tooltip($"Toggle whether regular expressions are run lenient or strict.\nCurrent Mode: {regexMode}");
    }
}
