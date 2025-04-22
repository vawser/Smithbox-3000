﻿using Hexa.NET.ImGui;
using Smithbox.Core.Editor;
using Smithbox.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.BehaviorEditorNS;

public class BehaviorFieldInput
{
    private Project Project;
    private BehaviorEditor Editor;

    public BehaviorFieldInput(Project curProject, BehaviorEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }


    public unsafe void DisplayFieldInput(NodeRepresentation selectedNode, int index, FieldInfo field)
    {
        var inputWidth = CFG.Current.BehaviorFieldInputWidth;

        var imguiID = $"field{index}";
        var inputFlags = ImGuiInputTextFlags.None;

        object? curValue = field.GetValue(selectedNode.Instance);
        string fieldName = field.Name;

        var wasChanged = false;
        var commitChange = false;
        var newValue = curValue;

        ImGui.SetNextItemWidth(inputWidth);

        // Long
        if (field.FieldType == typeof(long))
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
        if (field.FieldType == typeof(int))
        {
            var tempValue = (int)curValue;

            if (ImGui.InputInt($"##value_{imguiID}", ref tempValue, inputFlags))
            {
                newValue = tempValue;
                wasChanged = true;
            }
        }

        // Unsigned Integer
        if (field.FieldType == typeof(uint))
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
        if (field.FieldType == typeof(short))
        {
            int tempValue = (short)curValue;

            if (ImGui.InputInt($"##value_{imguiID}", ref tempValue, inputFlags))
            {
                newValue = tempValue;
                wasChanged = true;
            }
        }

        // Unsigned Short
        if (field.FieldType == typeof(ushort))
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
        if (field.FieldType == typeof(sbyte))
        {
            int tempValue = (sbyte)curValue;

            if (ImGui.InputInt($"##value_{imguiID}", ref tempValue, inputFlags))
            {
                newValue = tempValue;
                wasChanged = true;
            }
        }

        // Unsigned Byte
        if (field.FieldType == typeof(byte))
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
        if (field.FieldType == typeof(bool))
        {
            var tempValue = (bool)curValue;

            if (ImGui.Checkbox($"##value_{imguiID}", ref tempValue))
            {
                newValue = tempValue;
                wasChanged = true;
            }
        }

        // Float
        if (field.FieldType == typeof(float))
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
        if (field.FieldType == typeof(double))
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
        if (field.FieldType == typeof(string))
        {
            var tempValue = (string)curValue;

            if (tempValue == null)
            {
                tempValue = "";
            }

            if (ImGui.InputText($"##value_{imguiID}", ref tempValue, 128, inputFlags))
            {
                newValue = tempValue;
                wasChanged = true;
            }
        }

        // Vector2
        if (field.FieldType == typeof(Vector2))
        {
            var tempValue = (Vector2)curValue;

            if (ImGui.InputFloat2($"##value_{imguiID}", ref tempValue, inputFlags))
            {
                newValue = tempValue;
                wasChanged = true;
            }
        }

        // Vector3
        if (field.FieldType == typeof(Vector3))
        {
            var tempValue = (Vector3)curValue;

            if (ImGui.InputFloat3($"##value_{imguiID}", ref tempValue, inputFlags))
            {
                newValue = tempValue;
                wasChanged = true;
            }
        }

        // Vector4
        if (field.FieldType == typeof(Vector4))
        {
            var tempValue = (Vector4)curValue;

            if (ImGui.InputFloat4($"##value_{imguiID}", ref tempValue, inputFlags))
            {
                newValue = tempValue;
                wasChanged = true;
            }
        }

        commitChange = ImGui.IsItemDeactivatedAfterEdit();

        // Apply action
        if (commitChange && wasChanged)
        {
            // TODO: implement
            //var changeAction = new ParamFieldChange(curRow, curField, curValue, newValue);
            //Editor.ActionManager.ExecuteAction(changeAction);
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
}
