using Hexa.NET.ImGui;
using HKLib.hk2018.hk;
using Smithbox.Core.Editor;
using Smithbox.Core.Interface;
using Smithbox.Core.Resources;
using Smithbox.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Andre.Formats.Param;

namespace Smithbox.Core.ParamEditorNS;

public class ParamFieldView
{
    public Project Project;
    public ParamEditor Editor;

    private int ID;

    // Defined here so we can remove NoMove when setting up the imgui.ini
    private ImGuiWindowFlags MainWindowFlags = ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoMove;
    private ImGuiWindowFlags SubWindowFlags = ImGuiWindowFlags.NoMove;

    public ParamFieldView(Project curProject, ParamEditor editor)
    {
        ID = editor.ID;
        Project = curProject;
        Editor = editor;
    }

    public unsafe void Draw(string[] cmd)
    {
        ImGui.Begin($"Fields##ParamRowFieldEditor{ID}", SubWindowFlags);

        if (ImGui.IsWindowFocused())
        {
            Editor.DetectShortcuts = true;
        }

        if (Editor.Selection.IsFieldSelectionValid())
        {
            var curParam = Editor.Selection.GetSelectedParam();
            var curRow = Editor.Selection.GetSelectedRow();

            var tableColumns = 2;

            ParamMeta paramMeta = null;
            ParamFieldMeta fieldMeta = null;

            if (curParam.AppliedParamdef != null)
            {
                paramMeta = Project.ParamData.GetParamMeta(curParam.AppliedParamdef);
            }

            // Row ID and Name

            // Fields
            if (ImGui.BeginTable($"fieldTable_{ID}", tableColumns, ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthFixed);

                int dragSourceIndex = -1;
                int dragTargetIndex = -1;

                var curOrderedColumns = GetOrderedFields(curRow.Columns);

                // Fields
                for (int i = 0; i < curOrderedColumns.Count(); i++)
                {
                    var curField = curOrderedColumns.ElementAt(i);
                    var curValue = curField.GetValue(curRow);

                    if (paramMeta != null)
                    {
                        fieldMeta = paramMeta.GetField(curField.Def);
                    }

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    var displayName = curField.Def.InternalName;

                    if (fieldMeta != null && CFG.Current.DisplayCommunityFieldNames)
                    {
                        displayName = fieldMeta.AltName;
                    }

                    var isSelected = Editor.Selection.IsFieldSelected(i, curField);

                    if (ImGui.Selectable($"{displayName}##fieldEntry{i}", isSelected))
                    {
                        Editor.Selection.SelectField(i, curField);
                    }

                    if (fieldMeta != null)
                    {
                        UIHelper.Tooltip($"{fieldMeta.Wiki}");
                    }

                    // Begin drag
                    if (ImGui.BeginDragDropSource())
                    {
                        int payloadIndex = i;
                        ImGui.SetDragDropPayload("FIELD_DRAG", &payloadIndex, sizeof(int));
                        if (displayName == null || displayName == "")
                        {
                            ImGui.Text($"Blank");
                        }
                        else
                        {
                            ImGui.Text($"{displayName}");
                        }
                        ImGui.EndDragDropSource();
                    }

                    // Accept drop
                    if (ImGui.BeginDragDropTarget())
                    {
                        var payload = ImGui.AcceptDragDropPayload("FIELD_DRAG");
                        if (payload.Handle != null)
                        {
                            int* droppedIndex = (int*)payload.Data;
                            dragSourceIndex = *droppedIndex;
                            dragTargetIndex = i;
                        }
                        ImGui.EndDragDropTarget();
                    }

                    ImGui.TableSetColumnIndex(1);

                    ImGui.Text($"{curValue}");
                }

                if (dragSourceIndex >= 0 && dragTargetIndex >= 0 && dragSourceIndex != dragTargetIndex)
                {
                    if (FieldOrder.Entries.ContainsKey(Editor.Selection._selectedParamName))
                    {
                        var curEntry = FieldOrder.Entries[Editor.Selection._selectedParamName];

                        var sourceEntry = curEntry.FieldOrder[dragSourceIndex];
                        var targetEntry = curEntry.FieldOrder[dragTargetIndex];

                        curEntry.FieldOrder.Remove(dragSourceIndex);
                        curEntry.FieldOrder.Remove(dragTargetIndex);
                        curEntry.FieldOrder.Add(dragTargetIndex, sourceEntry);
                        curEntry.FieldOrder.Add(dragSourceIndex, targetEntry);

                        WriteFieldOrder();
                    }
                }

                ImGui.EndTable();
            }
        }

        ImGui.End();
    }

    public IEnumerable<Column> GetOrderedFields(IEnumerable<Column> columns)
    {
        if (FieldOrder == null)
            return columns;

        // If order list doesn't exist, just use default ordering
        if (!FieldOrder.Entries.ContainsKey(Editor.Selection._selectedParamName))
            return columns;

        var orderDict = FieldOrder.Entries[Editor.Selection._selectedParamName];

        var orderedColumns = columns
        .OrderBy(p =>
        {
            foreach (var kvp in orderDict.FieldOrder)
            {
                if (kvp.Value == p.Def.InternalName)
                {
                    return kvp.Key;
                }
            }
            return int.MaxValue;
        })
        .ToList();

        return orderedColumns;
    }

    private bool InitializedFieldOrder = false;
    private ParamFieldOrder FieldOrder;

    public void SetupFieldOrder()
    {
        // Blank
        FieldOrder = new();
        FieldOrder.Entries = new();

        var success = ReadFieldOrder();

        if(!success)
        {
            var primaryBank = Project.ParamData.PrimaryBank;

            foreach (var entry in primaryBank.Params)
            {
                // Ignore if the entry already exists
                if (!FieldOrder.Entries.ContainsKey(entry.Key))
                {
                    var firstRow = entry.Value.Rows.FirstOrDefault();

                    if (firstRow != null)
                    {
                        var fieldOrder = new ParamFieldOrderEntry();

                        for (int i = 0; i < firstRow.Columns.Count(); i++)
                        {
                            var curField = firstRow.Columns.ElementAt(i);

                            if (fieldOrder.FieldOrder == null)
                                fieldOrder.FieldOrder = new();

                            fieldOrder.FieldOrder.Add(i, curField.Def.InternalName);
                        }

                        FieldOrder.Entries.Add(entry.Key, fieldOrder);
                    }
                }
            }

            WriteFieldOrder();
        }

        InitializedFieldOrder = true;
    }

    public bool ReadFieldOrder()
    {
        var folder = $@"{Project.ProjectPath}\.smithbox\";
        var file = Path.Combine(folder, "Param Field Order.json");

        if (!File.Exists(file))
        {
            FieldOrder = new ParamFieldOrder();
            FieldOrder.Entries = new();
        }
        else
        {
            try
            {
                var filestring = File.ReadAllText(file);
                var options = new JsonSerializerOptions();
                FieldOrder = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.ParamFieldOrder);

                if (FieldOrder == null)
                {
                    throw new Exception("[Smithbox] Failed to read Param Field Order.json");
                }

                return true;
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Failed to load Param Field Order.json");

                FieldOrder = new ParamFieldOrder();
                FieldOrder.Entries = new();
            }
        }

        return false;
    }

    public void WriteFieldOrder()
    {
        var folder = $@"{Project.ProjectPath}\.smithbox\";
        var file = Path.Combine(folder, "Param Field Order.json");

        var json = JsonSerializer.Serialize(FieldOrder, SmithboxSerializerContext.Default.ParamFieldOrder);

        File.WriteAllText(file, json);
    }
}

