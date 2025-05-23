﻿using Andre.Formats;
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

    public Dictionary<int, bool> RowVisibility;

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

            // Pre-filter the rows since clipper doens't play nice with in-place filtering
            var sourceRows = new List<Param.Row>();

            if (RowVisibility != null)
            {
                for (int i = 0; i < curParam.Rows.Count; i++)
                {
                    var row = curParam.Rows[i];

                    if (RowVisibility.ContainsKey(i))
                    {
                        if (RowVisibility[i] == true)
                        {
                            sourceRows.Add(row);
                        }
                    }
                }
            }
            else
            {
                sourceRows = curParam.Rows.ToList();
            }

            ImGuiListClipper clipper = new ImGuiListClipper();
            clipper.Begin(sourceRows.Count);

            while (clipper.Step())
            {
                for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var curRow = sourceRows[i];

                    var rowName = $"{i}:{curRow.ID}";

                    if(!CFG.Current.DisplayRowIndices)
                    {
                        rowName = $"{curRow.ID}";
                    }

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

                    // Context Menu - Only for first element in selection
                    if (isSelected)
                    {
                        if (ImGui.BeginPopupContextItem($"Options##ParamRowContext{i}"))
                        {
                            if (ImGui.Selectable($"Duplicate##duplicateAction{i}"))
                            {
                                Editor.ParamActions.DuplicateRowMenu();

                                ImGui.CloseCurrentPopup();
                            }
                            UIHelper.Tooltip("Duplicate the current row selection.");

                            if (ImGui.Selectable($"Delete##deleteAction{i}"))
                            {
                                Editor.ParamActions.DeleteRow();

                                ImGui.CloseCurrentPopup();
                            }
                            UIHelper.Tooltip("Delete the current row selection.");

                            ImGui.EndPopup();
                        }
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
                RowVisibility = null;
                UpdateRowVisibility(Editor.Selection.GetSelectedParam());
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
                Editor.ParamActions.DuplicateRow(CFG.Current.ParamRowDuplicateOffset);
            }

            // Delete
            if (DetectShortcuts && Keyboard.KeyPress(Key.Delete) && ImGui.IsKeyDown(ImGuiKey.LeftCtrl))
            {
                Editor.ParamActions.DeleteRow();
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
            RowVisibility = Editor.SearchEngine.ProcessRowVisibility(Editor.Selection.GetSelectedParam());
        }
        UIHelper.Tooltip("Filter the row list.");

        // Clear
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Times}"))
        {
            Editor.SearchEngine.RowFilterInput = "";
            RowVisibility = null;
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

        // Row Indices
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Bars}"))
        {
            CFG.Current.DisplayRowIndices = !CFG.Current.DisplayRowIndices;
        }

        UIHelper.Tooltip($"Toggle whether row indices are displayed.");
    }

    /// <summary>
    /// Updates the field visibility dictionary on row change
    /// </summary>
    /// <param name="newRow"></param>
    public void UpdateRowVisibility(Param curParam)
    {
        var search = Editor.SearchEngine;

        RowVisibility = search.ProcessRowVisibility(curParam);
    }
}
