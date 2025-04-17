using Andre.Formats;
using Hexa.NET.ImGui;
using HKLib.hk2018.hkHashMapDetail;
using Smithbox.Core.Editor;
using Smithbox.Core.Interface;
using Smithbox.Core.ParamEditorNS.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Andre.Formats.Param;

namespace Smithbox.Core.ParamEditorNS;

public class ParamFieldDecorator
{
    private Project Project;
    private ParamEditor Editor;

    public ParamFieldDecorator(Project curProject, ParamEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public void DisplayFieldInfo(string imguiID, Param curParam, Row curRow, Column curField, object curValue, ParamFieldMeta fieldMeta, bool isReadOnly = false)
    {
        // ParamRef
        ParamRef(imguiID, curParam, curRow, curField, curValue, fieldMeta, isReadOnly);

        // ParamTextRef

        // ParamfileRef

        // ParamTextureRef

        // ParamEnum

        // Project Enum

        // ParamCalcCorrectDef

        // ParamSoulCostDef

        // Virtual Ref

        // Alias: Particle

        // Alias: Sound

        // Alias: Event Flag

        // Alias: Cutscene

        // Alias: Movie

        ParamFieldOffset(imguiID, curParam, curRow, curField, curValue, fieldMeta, isReadOnly);
    }

    public void ParamRef(string imguiID, Param curParam, Row curRow, Column curField, object curValue, ParamFieldMeta fieldMeta, bool isReadOnly = false)
    {
        if(fieldMeta.RefTypes != null)
        {
            foreach (ParamRef r in fieldMeta.RefTypes)
            {
                Param.Cell? c = curRow?[r.ConditionField];

                var inactiveRef = false;

                if (c != null && curRow != null)
                {
                    var fieldValue = c.Value.Value;
                    uint uintValue = 0;
                    var isUintValue = uint.TryParse($"{fieldValue}", out uintValue);

                    // Only check if field value is valid uint
                    if (isUintValue)
                    {
                        inactiveRef = uintValue != r.ConditionValue;
                    }
                }
            }
        }
    }

    public void ParamFieldOffset(string imguiID, Param curParam, Row curRow, Column curField, object curValue, ParamFieldMeta fieldMeta, bool isReadOnly = false)
    {
        if (Editor.Selection._selectedParamName == "MenuPropertySpecParam")
        {
            string target = "";
            string primitiveType = "";
            string operationType = "";
            string fieldOffset = "";

            if (fieldMeta.ParamFieldOffsetIndex == "0")
            {
                target = curRow["extract0_Target"].Value.Value.ToString();
                primitiveType = curRow["extract0_MemberType"].Value.Value.ToString();
                operationType = curRow["extract0_Operation"].Value.Value.ToString();
                fieldOffset = curRow["extract0_MemberTailOffset"].Value.Value.ToString();
            }
            else if (fieldMeta.ParamFieldOffsetIndex == "1")
            {
                target = curRow["extract1_Target"].Value.Value.ToString();
                primitiveType = curRow["extract1_MemberType"].Value.Value.ToString();
                operationType = curRow["extract1_Operation"].Value.Value.ToString();
                fieldOffset = curRow["extract1_MemberTailOffset"].Value.Value.ToString();
            }
            else
            {
                return;
            }

            var decimalOffset = int.Parse($"{fieldOffset}");

            switch (primitiveType)
            {
                case "0": return;

                case "1": // s8
                case "2": // u8
                    decimalOffset = decimalOffset - 1;
                    break;

                case "3": // s16
                case "4": // u16
                    decimalOffset = decimalOffset - 2;
                    break;


                case "5": // s32
                case "6": // u32
                case "7": // f
                    decimalOffset = decimalOffset - 4;
                    break;
            }

            var paramString = "";

            switch (target)
            {
                case "0": return;

                case "1": // Weapon
                    paramString = "EquipParamWeapon";
                    break;
                case "2": // Armor
                    paramString = "EquipParamProtector";
                    break;
                case "3": // Booster
                    paramString = "EquipParamBooster";
                    break;
                case "4": // FCS
                    paramString = "EquipParamFcs";
                    break;
                case "5": // Generator
                    paramString = "EquipParamGenerator";
                    break;
                case "6": // Behavior Paramter
                    paramString = "BehaviorParam_PC";
                    break;
                case "7": // Attack Parameter
                    paramString = "AtkParam_Pc";
                    break;
                case "8": // Bullet Parameter
                    paramString = "Bullet";
                    break;
                case "100": // Child Bullet Parameter
                    paramString = "Bullet";
                    break;
                case "101": // Child Bullet Attack Parameter
                    paramString = "AtkParam_Pc";
                    break;
                case "110": // Parent Bullet Parameter
                    paramString = "Bullet";
                    break;
                case "111": // Parent Bullet Attack Parameter
                    paramString = "AtkParam_Pc";
                    break;
            }

            var targetParam = Project.ParamData.PrimaryBank.Params[paramString];
            var firstRow = targetParam.Rows.First();
            var internalName = "";
            var displayName = "";

            var targetParamMeta = Project.ParamData.GetParamMeta(targetParam.AppliedParamdef);

            foreach (var col in firstRow.Columns)
            {
                var offset = (int)col.GetByteOffset();

                if (offset == decimalOffset)
                {
                    internalName = col.Def.InternalName;

                    var tempFieldMeta = targetParamMeta.GetField(col.Def);
                    displayName = tempFieldMeta.AltName;
                }
            }

            ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color, paramString);

            if (CFG.Current.DisplayCommunityFieldNames)
            {
                ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color, $"Drawing from:");
                ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color, $"{displayName}");
            }
            else
            {
                ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color, $"Drawing from:");
                ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color, $"{internalName}");
            }
        }
    }


    //public void ParamFieldOffset(string imguiID, Param curParam, Row curRow, Column curField, object curValue, ParamFieldMeta fieldMeta, bool isReadOnly = false)
    //{
    //}
}
