using Andre.Formats;
using Hexa.NET.ImGui;
using Smithbox.Core.Editor;
using Smithbox.Core.Interface;
using Smithbox.Core.Interface.Input;
using Smithbox.Core.Resources;
using Smithbox.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

    /// <summary>
    /// Cache for the columns so they are only rebuilt when the order changes
    /// </summary>
    public IEnumerable<Column> PrimaryOrderedColumns;
    private IEnumerable<Column> VanillaOrderedColumns;
    private IEnumerable<Column> AuxOrderedColumns;

    /// <summary>
    /// Visibility state for the fields (based on index)
    /// </summary>
    public Dictionary<int, bool> FieldVisibility;

    public ParamFieldView(Project curProject, ParamEditor editor)
    {
        ID = editor.ID;
        Project = curProject;
        Editor = editor;
    }

    public bool DetectShortcuts = false;

    public unsafe void Draw()
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

        // Done here so if they are changed during the middle of a frame, ImGui doesn't freak out
        var displayVanillaColumns = CFG.Current.DisplayVanillaColumns;
        var displayAuxColumns = CFG.Current.DisplayAuxColumns;
        var displayOffsetColumn = CFG.Current.DisplayOffsetColumn;
        var displayTypeColumn = CFG.Current.DisplayTypeColumn;
        var displayInfoColumn = CFG.Current.DisplayInformationColumn;

        ImGui.Begin($"Fields##ParamRowFieldEditor{ID}", SubWindowFlags);

        if (ImGui.IsWindowFocused())
        {
            DetectShortcuts = true;
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

            var tableColumns = 2;

            if(displayInfoColumn)
            {
                tableColumns += 1;
            }

            if (displayOffsetColumn)
            {
                tableColumns += 1;
            }

            if (displayTypeColumn)
            {
                tableColumns += 1;
            }

            if (vanillaRow != null && displayVanillaColumns)
            {
                tableColumns += 1;
                if (displayInfoColumn)
                {
                    tableColumns += 1;
                }
            }

            if (auxRow != null && displayAuxColumns)
            {
                tableColumns += 1;
                if (displayInfoColumn)
                {
                    tableColumns += 1;
                }
            }

            ParamMeta paramMeta = null;
            ParamFieldMeta fieldMeta = null;

            if (curParam.AppliedParamdef != null)
            {
                paramMeta = Project.ParamData.GetParamMeta(curParam.AppliedParamdef);
            }

            // Handle shortcuts
            Shortcuts(curParam, curRow, paramMeta);

            // Header Area
            DisplayHeader(curParam, curRow, paramMeta);

            ImGui.BeginChild("fieldTableArea");

            // Fields
            if (ImGui.BeginTable($"fieldTable_{ID}", tableColumns, tblFlags))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Primary Value", ImGuiTableColumnFlags.WidthFixed);

                if (displayInfoColumn)
                {
                    ImGui.TableSetupColumn("Primary Info", ImGuiTableColumnFlags.WidthFixed);
                }

                if (vanillaRow != null && displayVanillaColumns)
                {
                    ImGui.TableSetupColumn("Vanilla Value", ImGuiTableColumnFlags.WidthFixed);
                    if (displayInfoColumn)
                    {
                        ImGui.TableSetupColumn("Vanilla Info", ImGuiTableColumnFlags.WidthFixed);
                    }
                }

                if (auxRow != null && displayAuxColumns)
                {
                    ImGui.TableSetupColumn("Aux Value", ImGuiTableColumnFlags.WidthFixed);

                    if (displayInfoColumn)
                    {
                        ImGui.TableSetupColumn("Aux Info", ImGuiTableColumnFlags.WidthFixed);
                    }
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
                    // Visibility State
                    if (FieldVisibility != null)
                    {
                        if(FieldVisibility.ContainsKey(i))
                        {
                            // Skip this field if it set to not be visible
                            if (FieldVisibility[i] == false)
                            {
                                continue;
                            }
                        }
                    }

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

                    // Truncate the name if it exceeds 40 characters, to avoid a pointlessly wide column
                    var finalName = StringUtils.TruncateWithEllipsis(displayName, CFG.Current.ParamFieldColumnTruncationLength);

                    if (ImGui.Selectable($"{finalName}##fieldEntry{i}", isSelected))
                    {
                        Editor.Selection.SelectField(i, curField);
                    }

                    if (fieldMeta != null)
                    {
                        var text = $"{displayName}\n\n{fieldMeta.Wiki}";
                        UIHelper.Tooltip($"{text}");
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

                    // NOTE: a bit fiddly, but this handles the column arrangement nicely
                    // Name = 0
                    var primaryValueIndex = 1;
                    var offsetIndex = 2;
                    var typeIndex = 2;

                    if (displayOffsetColumn)
                    {
                        typeIndex = 3;
                    }

                    // Prefix offset here accounts for the Type and Offset columns that can appear before the name column
                    var prefixOffset = 0;

                    if(displayOffsetColumn)
                    {
                        prefixOffset += 1;
                    }

                    if(displayTypeColumn)
                    {
                        prefixOffset += 1;
                    }

                    var primaryInfoIndex = 2 + prefixOffset;
                    var vanillaInputIndex = 3 + prefixOffset;
                    var vanillaInfoIndex = 4 + prefixOffset;
                    var auxInputIndex = 5 + prefixOffset;
                    var auxInfoIndex = 6 + prefixOffset;

                    // If info columns are displayed, shift vanilla and aux input back
                    if (!displayInfoColumn)
                    {
                        vanillaInputIndex = 2 + prefixOffset;
                        auxInputIndex = 3 + prefixOffset; 
                    }

                    // If vanilla columns aren't displayed, shift aux input/info back
                    if (!displayVanillaColumns)
                    {
                        auxInputIndex = 3 + prefixOffset;
                        auxInfoIndex = 4 + prefixOffset;
                    }

                    ImGui.TableSetColumnIndex(primaryValueIndex);

                    // Primary Value
                    Editor.FieldInput.DisplayFieldInput($"primaryInput_{i}", curParam, curRow, curField, curValue, fieldMeta);

                    if (displayOffsetColumn)
                    {
                        ImGui.TableSetColumnIndex(offsetIndex);

                        var offset = GetFieldOffset(curField);
                        if(offset != null)
                        {
                            ImGui.Text($"0x{offset}");
                            UIHelper.Tooltip($"The data offset for this field.");
                        }
                    }

                    if (displayTypeColumn)
                    {
                        ImGui.TableSetColumnIndex(typeIndex);

                        var typeName = StringUtils.TruncateWithEllipsis($"{curField.Def.InternalType}", CFG.Current.ParamFieldColumnTruncationLength);

                        ImGui.Text($"{typeName}");
                        UIHelper.Tooltip($"The internal type for this field.\n\n{curField.Def.InternalType}");
                    }

                    // Primary Info
                    if (displayInfoColumn)
                    {
                        ImGui.TableSetColumnIndex(primaryInfoIndex);

                        Editor.FieldDecorator.DisplayFieldInfo($"primaryInfo_{i}", Project.ParamData.PrimaryBank, curParam, curRow, curField, curValue, fieldMeta);
                    }

                    // Vanilla Columns
                    if (vanillaRow != null && displayVanillaColumns)
                    {
                        ImGui.TableSetColumnIndex(vanillaInputIndex);

                        if(VanillaOrderedColumns == null)
                            VanillaOrderedColumns = GetOrderedFields(vanillaRow.Columns);

                        var vanillaField = VanillaOrderedColumns.ElementAt(i);
                        var vanillaValue = vanillaField.GetValue(vanillaRow);

                        // Vanilla Value
                        Editor.FieldInput.DisplayFieldInput($"vanillaInput_{i}", vanillaParam, vanillaRow, vanillaField, vanillaValue, fieldMeta, true);


                        if (displayInfoColumn)
                        {
                            ImGui.TableSetColumnIndex(vanillaInfoIndex);

                            // Vanilla Info
                            Editor.FieldDecorator.DisplayFieldInfo($"vanillaInfo_{i}", Project.ParamData.VanillaBank, vanillaParam, vanillaRow, vanillaField, vanillaValue, fieldMeta);
                        }
                    }

                    // Aux Columns
                    if (auxRow != null && displayAuxColumns)
                    {
                        ImGui.TableSetColumnIndex(auxInputIndex);

                        if(AuxOrderedColumns == null)
                            AuxOrderedColumns = GetOrderedFields(auxRow.Columns);

                        var auxField = AuxOrderedColumns.ElementAt(i);
                        var auxValue = auxField.GetValue(auxRow);

                        // Auxiliary Value
                        Editor.FieldInput.DisplayFieldInput($"auxiliaryInput_{i}", auxParam, auxRow, auxField, auxValue, fieldMeta, true);

                        if (displayInfoColumn)
                        {
                            ImGui.TableSetColumnIndex(auxInfoIndex);

                            // Auxiliary Info
                            Editor.FieldDecorator.DisplayFieldInfo($"auxiliaryInfo_{i}", Project.ParamData.AuxBank, auxParam, auxRow, auxField, auxValue, fieldMeta);
                        }
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

                        InvalidateColumns();
                        WriteFieldOrder();
                    }
                }
                ImGui.EndTable();
            }

            ImGui.EndChild();
        }

        ImGui.End();
    }

    private bool FocusFieldSearch = false;

    public void Shortcuts(Param curParam, Row curRow, ParamMeta curMeta)
    {
        if (DetectShortcuts)
        {
            // Focus Field Search
            if (Keyboard.KeyPress(Key.F) && Keyboard.IsDown(Key.LShift))
            {
                FocusFieldSearch = true;
            }

            // Clear Field Search
            if (Keyboard.KeyPress(Key.C) && Keyboard.IsDown(Key.LShift))
            {
                Editor.SearchEngine.FieldFilterInput = "";
                FieldVisibility = null;
            }
        }
    }

    public void DisplayHeader(Param curParam, Row curRow, ParamMeta curMeta)
    {
        // Search Builder
        var searchWidth = ImGui.GetWindowWidth() * 0.5f;

        if (ImGui.Button($"{Icons.ArrowCircleLeft}"))
        {
            Editor.SearchEngine.DisplaySearchTermBuilder = true;
        }
        UIHelper.Tooltip("View the search term builder.");

        // Search Bar
        ImGui.SameLine();

        if (FocusFieldSearch)
        {
            FocusFieldSearch = false;
            ImGui.SetKeyboardFocusHere();
        }

        ImGui.SetNextItemWidth(searchWidth);
        ImGui.InputText($"##fieldSearch_{ID}", ref Editor.SearchEngine.FieldFilterInput, 128);

        // Search
        ImGui.SameLine();

        if(ImGui.Button($"{Icons.Search}"))
        {
            FieldVisibility = Editor.SearchEngine.ProcessFieldVisibility(curParam, curRow, curMeta);
        }
        UIHelper.Tooltip("Filter the field list.");

        // Clear
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Times}"))
        {
            Editor.SearchEngine.FieldFilterInput = "";
            FieldVisibility = null;
        }
        UIHelper.Tooltip("Clear the field list filter.");

        // Regex Match Mode
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Eye}"))
        {
            Editor.SearchEngine.IsRegexLenient = !Editor.SearchEngine.IsRegexLenient;
        }

        var regexMode = "Strict";
        if (Editor.SearchEngine.IsRegexLenient)
            regexMode = "Lenient";

        UIHelper.Tooltip($"Toggle whether regular expressions are run lenient or strict.\nCurrent Mode: {regexMode}");

        // Toggle Community Field Names
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Book}"))
        {
            CFG.Current.DisplayCommunityFieldNames = !CFG.Current.DisplayCommunityFieldNames;
        }

        var communityFieldNameMode = "Internal";
        if (CFG.Current.DisplayCommunityFieldNames)
            communityFieldNameMode = "Community";

        UIHelper.Tooltip($"Toggle field name display type between Internal and Community.\nCurrent Mode: {communityFieldNameMode}");

        // Toggle Vanilla Columns
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.AddressBook}"))
        {
            CFG.Current.DisplayVanillaColumns = !CFG.Current.DisplayVanillaColumns;
        }

        var vanillaColumnMode = "Hidden";
        if (CFG.Current.DisplayVanillaColumns)
            vanillaColumnMode = "Visible";

        UIHelper.Tooltip($"Toggle the display of the vanilla columns.\nCurrent Mode: {vanillaColumnMode}");

        // Toggle Auxiliary Columns
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.AddressBookO}"))
        {
            CFG.Current.DisplayAuxColumns = !CFG.Current.DisplayAuxColumns;
        }


        var auxColumnMode = "Hidden";
        if (CFG.Current.DisplayAuxColumns)
            auxColumnMode = "Visible";

        UIHelper.Tooltip($"Toggle the display of the auxiliary columns.\nCurrent Mode: {auxColumnMode}");

        // Toggle Information Columns
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.InfoCircle}"))
        {
            CFG.Current.DisplayInformationColumn = !CFG.Current.DisplayInformationColumn;
        }

        var infoColumnMode = "Hidden";
        if (CFG.Current.DisplayInformationColumn)
            infoColumnMode = "Visible";

        UIHelper.Tooltip($"Toggle the display of the field information column.\nCurrent Mode: {infoColumnMode}");

        // Toggle Field Type Column
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Cog}"))
        {
            CFG.Current.DisplayTypeColumn = !CFG.Current.DisplayTypeColumn;
        }

        var fieldTypeColumnMode = "Hidden";
        if (CFG.Current.DisplayTypeColumn)
            fieldTypeColumnMode = "Visible";

        UIHelper.Tooltip($"Toggle the display of the field type column.\nCurrent Mode: {fieldTypeColumnMode}");

        // Toggle Field Offset Column
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.MapSigns}"))
        {
            CFG.Current.DisplayOffsetColumn = !CFG.Current.DisplayOffsetColumn;
        }

        var fieldOffsetColumnMode = "Hidden";
        if (CFG.Current.DisplayOffsetColumn)
            fieldOffsetColumnMode = "Visible";

        UIHelper.Tooltip($"Toggle the display of the field offset column.\nCurrent Mode: {fieldOffsetColumnMode}");

        // Toggle Field Padding
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Hubzilla}"))
        {
            CFG.Current.DisplayPaddingFields = !CFG.Current.DisplayPaddingFields;

            // Refresh visibility
            if(Editor.Selection._selectedRows.Count > 0)
            {
                // Just use the first row for this
                UpdateFieldVisibility(Editor.Selection._selectedRows[0].Row);
            }
        }

        var fieldPaddingMode = "Hidden";
        if (CFG.Current.DisplayPaddingFields)
            fieldPaddingMode = "Visible";

        UIHelper.Tooltip($"Toggle the display of padding field.\nCurrent Mode: {fieldPaddingMode}");

        // Reset Field Ordering
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Bars}"))
        {
            ResetCurrentFieldOrder();
        }
        UIHelper.Tooltip($"Reset the field ordering for this set of fields.");

        if (Editor.Selection.IsMultipleRowsSelected())
        {
            // Note if multiple rows are selected
            ImGui.TextColored(UI.Current.ImGui_Highlight_Text, "Multiple rows are selected, edits will be applied to all of them.");
        }
    }

    /// <summary>
    /// Reset the ordering columns
    /// </summary>
    public void InvalidateColumns()
    {
        PrimaryOrderedColumns = null;
        VanillaOrderedColumns = null;
        AuxOrderedColumns = null;
    }

    /// <summary>
    /// Updates the field visibility dictionary on row change
    /// </summary>
    /// <param name="newRow"></param>
    public void UpdateFieldVisibility(Row newRow)
    {
        var search = Editor.SearchEngine;

        if (PrimaryOrderedColumns == null)
            return;

        if (search.StoredParam != null && newRow != null && search.StoredMeta != null)
        {
            FieldVisibility = search.ProcessFieldVisibility(search.StoredParam, newRow, search.StoredMeta);
        }
        // Applied for non filtered switches, e.g. no search is present, but row changes
        else
        {
            // Update visibility for padding
            if (!CFG.Current.DisplayPaddingFields)
            {
                var filterResult = new Dictionary<int, bool>();

                if (Editor.Selection._selectedParam != null && Editor.Selection._selectedRows.Count > 0)
                {
                    var curParam = Editor.Selection._selectedParam;
                    var curMeta = Project.ParamData.GetParamMeta(curParam.AppliedParamdef);

                    for (int i = 0; i < PrimaryOrderedColumns.Count(); i++)
                    {
                        var visible = true;

                        var curField = PrimaryOrderedColumns.ElementAt(i);
                        var curFieldMeta = curMeta.Fields[curField.Def];

                        // Hide padding if it is disabled
                        if (!CFG.Current.DisplayPaddingFields)
                        {
                            if (curFieldMeta.IsPaddingField)
                            {
                                visible = false;
                            }
                        }

                        filterResult.Add(i, visible);
                    }
                }

                FieldVisibility = filterResult;
            }
            // Show all on a non-search row switch with no modifiers
            else
            {
                var filterResult = new Dictionary<int, bool>();

                if (Editor.Selection._selectedParam != null && Editor.Selection._selectedRows.Count > 0)
                {
                    var curParam = Editor.Selection._selectedParam;

                    for (int i = 0; i < PrimaryOrderedColumns.Count(); i++)
                    {
                        var visible = true;

                        filterResult.Add(i, visible);
                    }
                }

                FieldVisibility = filterResult;
            }
        }
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
    /// Reset the field ordering for the currently selected field set
    /// </summary>
    public void ResetCurrentFieldOrder()
    {
        var currentParam = Editor.Selection._selectedParamName;
        var currentOrder = FieldOrder.Entries[currentParam];

        var currentRows = Editor.Selection._selectedRows;

        var fieldOrder = new ParamFieldOrderEntry();

        if (currentRows.Count > 0)
        {
            var currentRow = currentRows[0].Row;

            for (int i = 0; i < currentRow.Columns.Count(); i++)
            {
                var curField = currentRow.Columns.ElementAt(i);

                if (fieldOrder.FieldOrder == null)
                    fieldOrder.FieldOrder = new();

                fieldOrder.FieldOrder.Add(i, curField.Def.InternalName);
            }
        }

        FieldOrder.Entries[currentParam] = fieldOrder;

        InvalidateColumns();

        WriteFieldOrder();
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

        if(!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        var json = JsonSerializer.Serialize(FieldOrder, SmithboxSerializerContext.Default.ParamFieldOrder);

        File.WriteAllText(file, json);
    }

    private string GetFieldOffset(Column curColumn)
    {
        if (curColumn == null)
        {
            return null;
        }

        if (curColumn.Def.BitSize == -1)
        {
            return curColumn.GetByteOffset().ToString("x");
        }

        var offS = curColumn.GetBitOffset();

        if (curColumn.Def.BitSize == 1)
        {
            return $"{curColumn.GetByteOffset().ToString("x")} [{offS}]";
        }

        return $"{curColumn.GetByteOffset().ToString("x")} [{offS}-{offS + curColumn.Def.BitSize - 1}]";
    }
}

