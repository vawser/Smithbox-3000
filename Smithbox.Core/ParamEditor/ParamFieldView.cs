using Andre.Formats;
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

    private IEnumerable<Column> PrimaryOrderedColumns;
    private IEnumerable<Column> VanillaOrderedColumns;
    private IEnumerable<Column> AuxOrderedColumns;

    public ParamFieldView(Project curProject, ParamEditor editor)
    {
        ID = editor.ID;
        Project = curProject;
        Editor = editor;
    }

    public unsafe void Draw(string[] cmd)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

        ImGui.Begin($"Fields##ParamRowFieldEditor{ID}", SubWindowFlags);

        if (ImGui.IsWindowFocused())
        {
            Editor.DetectShortcuts = true;
        }

        if (Editor.Selection.IsFieldSelectionValid())
        {
            Param curParam = Editor.Selection.GetSelectedParam();
            Row curRow = Editor.Selection.GetSelectedRow();

            Param vanillaParam = Editor.Selection.GetSelectedParamFromBank(Project.ParamData.VanillaBank);
            Row vanillaRow = vanillaParam.Rows.Where(e => e.ID == curRow.ID).FirstOrDefault();

            Param auxParam = null;
            Row auxRow = null;

            if (Project.ParamData.AuxBank != null)
            {
                auxParam = Editor.Selection.GetSelectedParamFromBank(Project.ParamData.AuxBank);
                auxRow = auxParam.Rows.Where(e => e.ID == curRow.ID).FirstOrDefault();
            }

            var tableColumns = 3;

            if (vanillaRow != null)
                tableColumns += 2;

            if (auxRow != null)
                tableColumns += 2;

            ParamMeta paramMeta = null;
            ParamFieldMeta fieldMeta = null;

            if (curParam.AppliedParamdef != null)
            {
                paramMeta = Project.ParamData.GetParamMeta(curParam.AppliedParamdef);
            }

            // Row ID and Name

            // Fields
            if (ImGui.BeginTable($"fieldTable_{ID}", tableColumns, tblFlags))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Primary Value", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Primary Info", ImGuiTableColumnFlags.WidthFixed);

                if (vanillaRow != null)
                {
                    ImGui.TableSetupColumn("Vanilla Value", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Vanilla Info", ImGuiTableColumnFlags.WidthFixed);
                }

                if (auxRow != null)
                {
                    ImGui.TableSetupColumn("Aux Value", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Aux Info", ImGuiTableColumnFlags.WidthFixed);
                }

                // Row ID
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.Text("Row ID");
                UIHelper.Tooltip("ID of the row");

                ImGui.TableSetColumnIndex(1);

                Editor.FieldInput.DisplayRowIDInput($"rowIdInput", curParam, curRow);

                // Row Name
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.Text("Row Name");
                UIHelper.Tooltip("Name of the row");

                ImGui.TableSetColumnIndex(1);

                Editor.FieldInput.DisplayRowNameInput($"rowNameInput", curParam, curRow);

                int dragSourceIndex = -1;
                int dragTargetIndex = -1;

                if(PrimaryOrderedColumns == null)
                    PrimaryOrderedColumns = GetOrderedFields(curRow.Columns);

                // Fields
                for (int i = 0; i < PrimaryOrderedColumns.Count(); i++)
                {
                    var curField = PrimaryOrderedColumns.ElementAt(i);
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

                    // Primary Value
                    Editor.FieldInput.DisplayFieldInput($"primaryInput_{i}", curParam, curRow, curField, curValue, fieldMeta);

                    // Primary Info
                    ImGui.TableSetColumnIndex(2);

                    Editor.FieldDecorator.DisplayFieldInfo($"primaryInfo_{i}", curParam, curRow, curField, curValue, fieldMeta);

                    if (vanillaRow != null)
                    {
                        ImGui.TableSetColumnIndex(3);

                        if(VanillaOrderedColumns == null)
                            VanillaOrderedColumns = GetOrderedFields(vanillaRow.Columns);

                        var vanillaField = VanillaOrderedColumns.ElementAt(i);
                        var vanillaValue = vanillaField.GetValue(vanillaRow);

                        // Vanilla Value
                        Editor.FieldInput.DisplayFieldInput($"vanillaInput_{i}", vanillaParam, vanillaRow, vanillaField, vanillaValue, fieldMeta, true);

                        ImGui.TableSetColumnIndex(4);

                        // Vanilla Info
                        Editor.FieldDecorator.DisplayFieldInfo($"vanillaInfo_{i}", vanillaParam, vanillaRow, vanillaField, vanillaValue, fieldMeta);
                    }

                    if (auxRow != null)
                    {
                        ImGui.TableSetColumnIndex(3);

                        if(AuxOrderedColumns == null)
                            AuxOrderedColumns = GetOrderedFields(auxRow.Columns);

                        var auxField = AuxOrderedColumns.ElementAt(i);
                        var auxValue = auxField.GetValue(vanillaRow);

                        // Auxiliary Value
                        Editor.FieldInput.DisplayFieldInput($"auxiliaryInput_{i}", auxParam, auxRow, auxField, auxValue, fieldMeta, true);

                        ImGui.TableSetColumnIndex(4);

                        // Auxiliary Info
                        Editor.FieldDecorator.DisplayFieldInfo($"auxiliaryInfo_{i}", auxParam, auxRow, auxField, auxValue, fieldMeta);
                    }
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

                        PrimaryOrderedColumns = null;
                        VanillaOrderedColumns = null;
                        AuxOrderedColumns = null;

                        WriteFieldOrder();
                    }
                }

                ImGui.EndTable();
            }
        }

        ImGui.End();
    }

    /// <summary>
    /// Get the ordered fields based on the Param Field Order file.
    /// </summary>
    /// <param name="columns"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Creates a new Param Field Order if a project one does not already exist.
    /// </summary>
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

    /// <summary>
    /// Reads the Param Field Order from the project .smithbox folder.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Writes the Param Field Order out to the project .smithbox folder.
    /// </summary>
    public void WriteFieldOrder()
    {
        var folder = $@"{Project.ProjectPath}\.smithbox\";
        var file = Path.Combine(folder, "Param Field Order.json");

        var json = JsonSerializer.Serialize(FieldOrder, SmithboxSerializerContext.Default.ParamFieldOrder);

        File.WriteAllText(file, json);
    }
}

