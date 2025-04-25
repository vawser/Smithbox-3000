using Andre.Formats;
using Hexa.NET.ImGui;
using Smithbox.Core.Actions;
using Smithbox.Core.Editor;
using Smithbox.Core.Interface;
using Smithbox.Core.Utils;
using System.Numerics;
using static Andre.Formats.Param;
using static Smithbox.Core.Actions.ParamRowChange;

namespace Smithbox.Core.ParamEditorNS;

public class ParamFieldInput
{
    private Project Project;
    private ParamEditor Editor;

    public ParamFieldInput(Project curProject, ParamEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public void DisplayRowIDInput(string imguiID, Param curParam, Row curRow)
    {
        var inputWidth = CFG.Current.ParamFieldInputWidth;
        var inputFlags = ImGuiInputTextFlags.None;

        var wasChanged = false;
        var commitChange = false;
        var newValue = curRow.ID;
        var fieldType = curRow.ID.GetType();

        ImGui.SetNextItemWidth(inputWidth);

        if (Project.ParamData.PrimaryBank.EditLock)
        {
            // Signed Integer
            if (fieldType == typeof(int))
            {
                var tempValue = (int)curRow.ID;

                ImGui.BeginDisabled();
                if (ImGui.InputInt($"##value_{imguiID}", ref tempValue, inputFlags))
                {
                    newValue = tempValue;
                    wasChanged = true;
                }
                ImGui.EndDisabled();
                UIHelper.Tooltip("You cannot edit this field currently as a sensitive operation is in process.");
            }
        }
        else
        {
            // Signed Integer
            if (fieldType == typeof(int))
            {
                var tempValue = (int)curRow.ID;

                if (ImGui.InputInt($"##value_{imguiID}", ref tempValue, inputFlags))
                {
                    newValue = tempValue;
                    wasChanged = true;
                }
            }
        }

            commitChange |= ImGui.IsItemDeactivatedAfterEdit();

        // Apply action
        if (commitChange && wasChanged)
        {
            // Apply change to all selected rows if multiple rows are selected.
            if (Editor.Selection.IsMultipleRowsSelected())
            {
                var actions = new List<AtomicAction>();

                foreach (var entry in Editor.Selection._selectedRows)
                {
                    var tRow = entry.Row;

                    actions.Add(new ParamRowChange(tRow, tRow.ID, newValue, RowChangeType.ID));
                }

                var compoundAction = new CompoundAction(actions);
                Editor.ActionManager.ExecuteAction(compoundAction);
            }
            else
            {
                var changeAction = new ParamRowChange(curRow, curRow.ID, newValue, RowChangeType.ID);

                Editor.ActionManager.ExecuteAction(changeAction);
            }
        }
    }
    public void DisplayRowNameInput(string imguiID, Param curParam, Row curRow)
    {
        var inputWidth = CFG.Current.ParamFieldInputWidth;
        var inputFlags = ImGuiInputTextFlags.None;

        var wasChanged = false;
        var commitChange = false;
        var newValue = curRow.Name;
        var fieldType = curRow.Name.GetType();

        ImGui.SetNextItemWidth(inputWidth);

        if (Project.ParamData.PrimaryBank.EditLock)
        {
            // String
            if (fieldType == typeof(string))
            {
                var tempValue = curRow.Name;

                ImGui.BeginDisabled();
                if (ImGui.InputText($"##value_{imguiID}", ref tempValue, 128, inputFlags))
                {
                    newValue = tempValue;
                    wasChanged = true;
                }
                ImGui.EndDisabled();
                UIHelper.Tooltip("You cannot edit this field currently as a sensitive operation is in process.");
            }
        }
        else
        {
            // String
            if (fieldType == typeof(string))
            {
                var tempValue = curRow.Name;

                if (ImGui.InputText($"##value_{imguiID}", ref tempValue, 128, inputFlags))
                {
                    newValue = tempValue;
                    wasChanged = true;
                }
            }
        }

        commitChange |= ImGui.IsItemDeactivatedAfterEdit();

        // Apply action
        if (commitChange && wasChanged)
        {
            // Apply change to all selected rows if multiple rows are selected.
            if (Editor.Selection.IsMultipleRowsSelected())
            {
                var actions = new List<AtomicAction>();

                foreach (var entry in Editor.Selection._selectedRows)
                {
                    var tRow = entry.Row;

                    actions.Add(new ParamRowChange(tRow, tRow.Name, newValue, RowChangeType.Name));
                }

                var compoundAction = new CompoundAction(actions);
                Editor.ActionManager.ExecuteAction(compoundAction);
            }
            else
            {
                var changeAction = new ParamRowChange(curRow, curRow.Name, newValue, RowChangeType.Name);

                Editor.ActionManager.ExecuteAction(changeAction);
            }
        }
    }

    public unsafe void DisplayFieldInput(string imguiID, Param curParam, Row curRow, Column curField, object curValue, ParamFieldMeta fieldMeta, bool isReadOnly = false)
    {
        var inputWidth = CFG.Current.ParamFieldInputWidth;

        var wasChanged = false;
        var commitChange = false;
        var newValue = curValue;
        var fieldType = curValue.GetType();

        var inputFlags = ImGuiInputTextFlags.None;
        if(isReadOnly)
        {
            inputFlags = ImGuiInputTextFlags.ReadOnly;
        }

        ImGui.SetNextItemWidth(inputWidth);

        // Locked inputs for Edit Lock state
        if (Project.ParamData.PrimaryBank.EditLock)
        {
            // Long
            if (fieldType == typeof(long))
            {
                var tempValue = (long)curValue;
                var stringValue = $@"{tempValue}";

                ImGui.BeginDisabled();
                if (ImGui.InputText($"##value_{imguiID}", ref stringValue, 128, inputFlags))
                {
                    var result = long.TryParse(stringValue, out tempValue);
                    if (result)
                    {
                        newValue = tempValue;
                        wasChanged = true;
                    }
                }
                ImGui.EndDisabled();
                UIHelper.Tooltip("You cannot edit this field currently as a sensitive operation is in process.");
            }

            // Signed Integer
            if (fieldType == typeof(int))
            {
                var tempValue = (int)curValue;

                ImGui.BeginDisabled();
                if (ImGui.InputInt($"##value_{imguiID}", ref tempValue, inputFlags))
                {
                    newValue = tempValue;
                    wasChanged = true;
                }
                ImGui.EndDisabled();
                UIHelper.Tooltip("You cannot edit this field currently as a sensitive operation is in process.");
            }

            // Unsigned Integer
            if (fieldType == typeof(uint))
            {
                var tempValue = (uint)curValue;
                var stringValue = $@"{tempValue}";

                ImGui.BeginDisabled();
                if (ImGui.InputText($"##value_{imguiID}", ref stringValue, 128, inputFlags))
                {
                    var result = uint.TryParse(stringValue, out tempValue);
                    if (result)
                    {
                        newValue = tempValue;
                        wasChanged = true;
                    }
                }
                ImGui.EndDisabled();
                UIHelper.Tooltip("You cannot edit this field currently as a sensitive operation is in process.");
            }

            // Signed Short
            if (fieldType == typeof(short))
            {
                int tempValue = (short)curValue;

                ImGui.BeginDisabled();
                if (ImGui.InputInt($"##value_{imguiID}", ref tempValue, inputFlags))
                {
                    newValue = tempValue;
                    wasChanged = true;
                }
                ImGui.EndDisabled();
                UIHelper.Tooltip("You cannot edit this field currently as a sensitive operation is in process.");
            }

            // Unsigned Short
            if (fieldType == typeof(ushort))
            {
                var tempValue = (ushort)curValue;
                var stringValue = $@"{tempValue}";

                ImGui.BeginDisabled();
                if (ImGui.InputText($"##value_{imguiID}", ref stringValue, 128, inputFlags))
                {
                    var result = ushort.TryParse(stringValue, out tempValue);
                    if (result)
                    {
                        newValue = tempValue;
                        wasChanged = true;
                    }
                }
                ImGui.EndDisabled();
                UIHelper.Tooltip("You cannot edit this field currently as a sensitive operation is in process.");
            }

            // Signed Byte
            if (fieldType == typeof(sbyte))
            {
                int tempValue = (sbyte)curValue;

                ImGui.BeginDisabled();
                if (ImGui.InputInt($"##value_{imguiID}", ref tempValue, inputFlags))
                {
                    newValue = tempValue;
                    wasChanged = true;
                }
                ImGui.EndDisabled();
                UIHelper.Tooltip("You cannot edit this field currently as a sensitive operation is in process.");
            }

            // Unsigned Byte
            if (fieldType == typeof(byte))
            {
                var tempValue = (byte)curValue;
                var stringValue = $@"{tempValue}";

                ImGui.BeginDisabled();
                if (ImGui.InputText($"##value_{imguiID}", ref stringValue, 128, inputFlags))
                {
                    var result = byte.TryParse(stringValue, out tempValue);
                    if (result)
                    {
                        newValue = tempValue;
                        wasChanged = true;
                    }
                }
                ImGui.EndDisabled();
                UIHelper.Tooltip("You cannot edit this field currently as a sensitive operation is in process.");
            }

            // Boolean
            if (fieldType == typeof(bool))
            {
                var tempValue = (bool)curValue;

                if (isReadOnly)
                {
                    ImGui.BeginDisabled();
                    ImGui.Checkbox($"##value_{imguiID}", ref tempValue);
                    ImGui.EndDisabled();
                }
                else
                {
                    ImGui.BeginDisabled();
                    if (ImGui.Checkbox($"##value_{imguiID}", ref tempValue))
                    {
                        newValue = tempValue;
                        wasChanged = true;
                    }
                    ImGui.EndDisabled();
                    UIHelper.Tooltip("You cannot edit this field currently as a sensitive operation is in process.");
                }
            }

            // Float
            if (fieldType == typeof(float))
            {
                var tempValue = (float)curValue;
                var format = CreateFloatFormat(tempValue);
                var formatPtr = StringUtils.StringToUtf8(format);

                ImGui.BeginDisabled();
                if (ImGui.InputFloat($"##value_{imguiID}", ref tempValue, 0.1f, 1.0f, formatPtr, inputFlags))
                {
                    newValue = tempValue;
                    wasChanged = true;
                }
                ImGui.EndDisabled();
                UIHelper.Tooltip("You cannot edit this field currently as a sensitive operation is in process.");

                StringUtils.FreeUtf8(formatPtr);
            }

            // Double
            if (fieldType == typeof(double))
            {
                var tempValue = (double)curValue;
                double step = 0.1;
                double stepFast = 1.0;
                var format = CreateFloatFormat((float)tempValue);
                byte* formatPtr = StringUtils.StringToUtf8(format);

                ImGui.BeginDisabled();
                if (ImGui.InputScalar($"##value_{imguiID}", ImGuiDataType.Double, &tempValue, &step, &stepFast, formatPtr, inputFlags))
                {
                    newValue = tempValue;
                    wasChanged = true;
                }
                ImGui.EndDisabled();
                UIHelper.Tooltip("You cannot edit this field currently as a sensitive operation is in process.");
            }

            // String
            if (fieldType == typeof(string))
            {
                var tempValue = (string)curValue;

                ImGui.BeginDisabled();
                if (ImGui.InputText($"##value_{imguiID}", ref tempValue, 128, inputFlags))
                {
                    newValue = tempValue;
                    wasChanged = true;
                }
                ImGui.EndDisabled();
                UIHelper.Tooltip("You cannot edit this field currently as a sensitive operation is in process.");
            }

            // Vector2
            if (fieldType == typeof(Vector2))
            {
                var tempValue = (Vector2)curValue;

                ImGui.BeginDisabled();
                if (ImGui.InputFloat2($"##value_{imguiID}", ref tempValue, inputFlags))
                {
                    newValue = tempValue;
                    wasChanged = true;
                }
                ImGui.EndDisabled();
                UIHelper.Tooltip("You cannot edit this field currently as a sensitive operation is in process.");
            }

            // Vector3
            if (fieldType == typeof(Vector3))
            {
                var tempValue = (Vector3)curValue;

                ImGui.BeginDisabled();
                if (ImGui.InputFloat3($"##value_{imguiID}", ref tempValue, inputFlags))
                {
                    newValue = tempValue;
                    wasChanged = true;
                }
                ImGui.EndDisabled();
                UIHelper.Tooltip("You cannot edit this field currently as a sensitive operation is in process.");
            }

            // Vector4
            if (fieldType == typeof(Vector4))
            {
                var tempValue = (Vector4)curValue;

                ImGui.BeginDisabled();
                if (ImGui.InputFloat4($"##value_{imguiID}", ref tempValue, inputFlags))
                {
                    newValue = tempValue;
                    wasChanged = true;
                }
                ImGui.EndDisabled();
                UIHelper.Tooltip("You cannot edit this field currently as a sensitive operation is in process.");
            }

            // Byte Array
            if (fieldType == typeof(byte[]))
            {
                var bval = (byte[])curValue;
                var tempValue = ConvertToTextualPadding(bval);

                ImGui.BeginDisabled();
                if (ImGui.InputText($"##value_{imguiID}", ref tempValue, 9999, inputFlags))
                {
                    var nVal = ConvertToBytePadding(tempValue, bval.Length);

                    if (nVal != null)
                    {
                        newValue = nVal;
                        wasChanged = true;
                    }
                }
                ImGui.EndDisabled();
                UIHelper.Tooltip("You cannot edit this field currently as a sensitive operation is in process.");
            }

            // Override Bool (for non bool types that act like bools)
            if (fieldMeta != null && fieldMeta.IsBool)
            {
                dynamic tempValue = curValue;
                bool checkValue = tempValue > 0;

                if (isReadOnly)
                {
                    ImGui.SameLine();

                    ImGui.BeginDisabled();
                    ImGui.Checkbox($"##valueBool_{imguiID}", ref checkValue);
                    ImGui.EndDisabled();
                }
                else
                {
                    ImGui.SameLine();

                    ImGui.BeginDisabled();
                    if (ImGui.Checkbox($"##valueBool_{imguiID}", ref checkValue))
                    {
                        try
                        {
                            newValue = Convert.ChangeType(checkValue ? 1 : 0, curValue.GetType());
                            wasChanged = true;
                        }
                        catch (Exception ex) { }
                    }
                    ImGui.EndDisabled();
                    UIHelper.Tooltip("You cannot edit this field currently as a sensitive operation is in process.");
                }

                commitChange = ImGui.IsItemDeactivatedAfterEdit();
            }
        }
        // Normal Inputs
        else
        {
            // Long
            if (fieldType == typeof(long))
            {
                var tempValue = (long)curValue;
                var stringValue = $@"{tempValue}";

                if (ImGui.InputText($"##value_{imguiID}", ref stringValue, 128, inputFlags))
                {
                    var result = long.TryParse(stringValue, out tempValue);
                    if (result)
                    {
                        newValue = tempValue;
                        wasChanged = true;
                    }
                }
            }

            // Signed Integer
            if (fieldType == typeof(int))
            {
                var tempValue = (int)curValue;

                if (ImGui.InputInt($"##value_{imguiID}", ref tempValue, inputFlags))
                {
                    newValue = tempValue;
                    wasChanged = true;
                }
            }

            // Unsigned Integer
            if (fieldType == typeof(uint))
            {
                var tempValue = (uint)curValue;
                var stringValue = $@"{tempValue}";

                if (ImGui.InputText($"##value_{imguiID}", ref stringValue, 128, inputFlags))
                {
                    var result = uint.TryParse(stringValue, out tempValue);
                    if (result)
                    {
                        newValue = tempValue;
                        wasChanged = true;
                    }
                }
            }

            // Signed Short
            if (fieldType == typeof(short))
            {
                int tempValue = (short)curValue;

                if (ImGui.InputInt($"##value_{imguiID}", ref tempValue, inputFlags))
                {
                    newValue = tempValue;
                    wasChanged = true;
                }
            }

            // Unsigned Short
            if (fieldType == typeof(ushort))
            {
                var tempValue = (ushort)curValue;
                var stringValue = $@"{tempValue}";

                if (ImGui.InputText($"##value_{imguiID}", ref stringValue, 128, inputFlags))
                {
                    var result = ushort.TryParse(stringValue, out tempValue);
                    if (result)
                    {
                        newValue = tempValue;
                        wasChanged = true;
                    }
                }
            }

            // Signed Byte
            if (fieldType == typeof(sbyte))
            {
                int tempValue = (sbyte)curValue;

                if (ImGui.InputInt($"##value_{imguiID}", ref tempValue, inputFlags))
                {
                    newValue = tempValue;
                    wasChanged = true;
                }
            }

            // Unsigned Byte
            if (fieldType == typeof(byte))
            {
                var tempValue = (byte)curValue;
                var stringValue = $@"{tempValue}";

                if (ImGui.InputText($"##value_{imguiID}", ref stringValue, 128, inputFlags))
                {
                    var result = byte.TryParse(stringValue, out tempValue);
                    if (result)
                    {
                        newValue = tempValue;
                        wasChanged = true;
                    }
                }
            }

            // Boolean
            if (fieldType == typeof(bool))
            {
                var tempValue = (bool)curValue;

                if (isReadOnly)
                {
                    ImGui.BeginDisabled();
                    ImGui.Checkbox($"##value_{imguiID}", ref tempValue);
                    ImGui.EndDisabled();
                }
                else
                {
                    if (ImGui.Checkbox($"##value_{imguiID}", ref tempValue))
                    {
                        newValue = tempValue;
                        wasChanged = true;
                    }
                }
            }

            // Float
            if (fieldType == typeof(float))
            {
                var tempValue = (float)curValue;
                var format = CreateFloatFormat(tempValue);
                var formatPtr = StringUtils.StringToUtf8(format);

                if (ImGui.InputFloat($"##value_{imguiID}", ref tempValue, 0.1f, 1.0f, formatPtr, inputFlags))
                {
                    newValue = tempValue;
                    wasChanged = true;
                }

                StringUtils.FreeUtf8(formatPtr);
            }

            // Double
            if (fieldType == typeof(double))
            {
                var tempValue = (double)curValue;
                double step = 0.1;
                double stepFast = 1.0;
                var format = CreateFloatFormat((float)tempValue);
                byte* formatPtr = StringUtils.StringToUtf8(format);

                if (ImGui.InputScalar($"##value_{imguiID}", ImGuiDataType.Double, &tempValue, &step, &stepFast, formatPtr, inputFlags))
                {
                    newValue = tempValue;
                    wasChanged = true;
                }
            }

            // String
            if (fieldType == typeof(string))
            {
                var tempValue = (string)curValue;

                if (ImGui.InputText($"##value_{imguiID}", ref tempValue, 128, inputFlags))
                {
                    newValue = tempValue;
                    wasChanged = true;
                }
            }

            // Vector2
            if (fieldType == typeof(Vector2))
            {
                var tempValue = (Vector2)curValue;

                if (ImGui.InputFloat2($"##value_{imguiID}", ref tempValue, inputFlags))
                {
                    newValue = tempValue;
                    wasChanged = true;
                }
            }

            // Vector3
            if (fieldType == typeof(Vector3))
            {
                var tempValue = (Vector3)curValue;

                if (ImGui.InputFloat3($"##value_{imguiID}", ref tempValue, inputFlags))
                {
                    newValue = tempValue;
                    wasChanged = true;
                }
            }

            // Vector4
            if (fieldType == typeof(Vector4))
            {
                var tempValue = (Vector4)curValue;

                if (ImGui.InputFloat4($"##value_{imguiID}", ref tempValue, inputFlags))
                {
                    newValue = tempValue;
                    wasChanged = true;
                }
            }

            // Byte Array
            if (fieldType == typeof(byte[]))
            {
                var bval = (byte[])curValue;
                var tempValue = ConvertToTextualPadding(bval);

                if (ImGui.InputText($"##value_{imguiID}", ref tempValue, 9999, inputFlags))
                {
                    var nVal = ConvertToBytePadding(tempValue, bval.Length);

                    if (nVal != null)
                    {
                        newValue = nVal;
                        wasChanged = true;
                    }
                }
            }

            // Override Bool (for non bool types that act like bools)
            if (fieldMeta != null && fieldMeta.IsBool)
            {
                dynamic tempValue = curValue;
                bool checkValue = tempValue > 0;

                if (isReadOnly)
                {
                    ImGui.SameLine();

                    ImGui.BeginDisabled();
                    ImGui.Checkbox($"##valueBool_{imguiID}", ref checkValue);
                    ImGui.EndDisabled();
                }
                else
                {
                    ImGui.SameLine();

                    if (ImGui.Checkbox($"##valueBool_{imguiID}", ref checkValue))
                    {
                        try
                        {
                            newValue = Convert.ChangeType(checkValue ? 1 : 0, curValue.GetType());
                            wasChanged = true;
                        }
                        catch (Exception ex) { }
                    }
                }

                commitChange = ImGui.IsItemDeactivatedAfterEdit();
            }
        }

        commitChange |= ImGui.IsItemDeactivatedAfterEdit();

        // Apply action
        if(commitChange && wasChanged)
        {
            // Apply change to all selected rows if multiple rows are selected.
            if (Editor.Selection.IsMultipleRowsSelected())
            {
                var actions = new List<AtomicAction>();

                foreach(var entry in Editor.Selection._selectedRows)
                {
                    var tRow = entry.Row;
                    var tField = tRow.Columns.Where(e => e.Def.InternalName == curField.Def.InternalName).FirstOrDefault();
                    var tValue = tField.GetValue(tRow);

                    actions.Add(new ParamFieldChange(tRow, tField, tValue, newValue));
                }

                var compoundAction = new CompoundAction(actions);
                Editor.ActionManager.ExecuteAction(compoundAction);
            }
            else
            {
                var changeAction = new ParamFieldChange(curRow, curField, curValue, newValue);

                Editor.ActionManager.ExecuteAction(changeAction);
            }
        }
    }


    /// <summary>
    /// Helper for the float formatting in the InputFloat input elements
    /// </summary>
    /// <param name="f"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public unsafe string CreateFloatFormat(float f, int min = 3, int max = 6)
    {
        var split = f.ToString("F6").TrimEnd('0').Split('.');
        return $"%.{Math.Clamp(split.Last().Length, min, max)}f";
    }

    /// <summary>
    /// Converts the padding byte array into a textual representation for editing 
    /// </summary>
    /// <param name="dummy8"></param>
    /// <returns></returns>
    public string ConvertToTextualPadding(byte[] dummy8)
    {
        string val = null;
        foreach (var b in dummy8)
        {
            if (val == null)
            {
                val = "[" + b;
            }
            else
            {
                val += "|" + b;
            }
        }

        if (val == null)
        {
            val = "[]";
        }
        else
        {
            val += "]";
        }

        return val;
    }

    /// <summary>
    /// Converts the textual representation of padding back into a byte array
    /// </summary>
    /// <param name="dummy8"></param>
    /// <param name="expectedLength"></param>
    /// <returns></returns>
    public byte[] ConvertToBytePadding(string dummy8, int expectedLength)
    {
        var nval = new byte[expectedLength];
        if (!(dummy8.StartsWith('[') && dummy8.EndsWith(']')))
        {
            return null;
        }

        var spl = dummy8.Substring(1, dummy8.Length - 2).Split('|');
        if (nval.Length != spl.Length)
        {
            return null;
        }

        for (var i = 0; i < nval.Length; i++)
        {
            if (!byte.TryParse(spl[i], out nval[i]))
            {
                return null;
            }
        }

        return nval;
    }
}
