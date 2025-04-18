using Andre.Formats;
using Hexa.NET.ImGui;
using HKLib.hk2018.hkHashMapDetail;
using Smithbox.Core.Editor;
using Smithbox.Core.Interface;
using Smithbox.Core.ParamEditorNS.Meta;
using Smithbox.Core.Resources;
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

    public void DisplayFieldInfo(string imguiID, ParamBank targetBank, Param curParam, Row curRow, Column curField, object curValue, ParamFieldMeta fieldMeta, bool isReadOnly = false)
    {
        if (fieldMeta == null)
            return;

        // ParamRef
        ParamRef(imguiID, targetBank, curParam, curRow, curField, curValue, fieldMeta, isReadOnly);

        // ParamTextRef
        ParamTextRef(imguiID, targetBank, curParam, curRow, curField, curValue, fieldMeta, isReadOnly);

        // TODO: ParamFileRef

        // ParamTextureRef
        // Handled in ParamImageView as they are displayed in a separate window

        // ParamEnum

        // ProjectEnum

        // ParamCalcCorrectDef

        // ParamSoulCostDef

        // VirtualRef

        // Alias: Particle
        AliasRef(imguiID, "Particles", curParam, curRow, curField, curValue, fieldMeta, 
            fieldMeta.ShowParticleEnumList, Project.Aliases.Particles, 
            isReadOnly);

        // Alias: Sound
        AliasRef(imguiID, "Sounds", curParam, curRow, curField, curValue, fieldMeta, 
            fieldMeta.ShowSoundEnumList, Project.Aliases.Sounds, 
            isReadOnly);

        // Alias: Event Flag
        ConditionalAliasRef(imguiID, "Event Flags", curParam, curRow, curField, curValue, fieldMeta, 
            fieldMeta.ShowFlagEnumList, Project.Aliases.EventFlags, 
            fieldMeta.FlagAliasEnum_ConditionalField, fieldMeta.FlagAliasEnum_ConditionalValue, 
            isReadOnly);

        // Alias: Cutscene
        AliasRef(imguiID, "Cutscenes", curParam, curRow, curField, curValue, fieldMeta,
            fieldMeta.ShowCutsceneEnumList, Project.Aliases.Cutscenes, 
            isReadOnly);

        // Alias: Movie
        ConditionalAliasRef(imguiID, "Movies", curParam, curRow, curField, curValue, fieldMeta, 
            fieldMeta.ShowMovieEnumList, Project.Aliases.Movies, 
            fieldMeta.MovieAliasEnum_ConditionalField,
            fieldMeta.MovieAliasEnum_ConditionalValue,
            isReadOnly);

        ParamFieldOffset(imguiID, curParam, curRow, curField, curValue, fieldMeta, isReadOnly);
    }

    /// <summary>
    /// Param Reference
    /// </summary>
    /// <param name="imguiID"></param>
    /// <param name="curParam"></param>
    /// <param name="curRow"></param>
    /// <param name="curField"></param>
    /// <param name="curValue"></param>
    /// <param name="fieldMeta"></param>
    /// <param name="isReadOnly"></param>
    public void ParamRef(string imguiID, ParamBank targetBank, Param curParam, Row curRow, Column curField, object curValue, ParamFieldMeta fieldMeta, bool isReadOnly = false)
    {
        if (fieldMeta.RefTypes == null || curRow == null)
            return;

        foreach (ParamRef reference in fieldMeta.RefTypes)
        {
            var conditionalField = curRow[reference.ConditionField];
            if (conditionalField == null)
                continue;

            if (!uint.TryParse($"{conditionalField.Value.Value}", out uint checkFieldValue))
                continue;

            if (checkFieldValue != reference.ConditionValue)
                continue;

            if (!targetBank.Params.TryGetValue(reference.ParamName, out Param targetParam) || targetParam == null)
                continue;

            if (!int.TryParse($"{curValue}", out int targetRowID))
                continue;

            var targetRow = targetParam.Rows.FirstOrDefault(row => row.ID == targetRowID);
            if (targetRow == null)
                continue;

            var rowName = string.IsNullOrWhiteSpace(targetRow.Name) ? "Unnamed Row" : targetRow.Name;
            string displayText = $"{targetRow.ID}: {rowName} <{reference.ParamName}>";
            ImGui.TextColored(UI.Current.ImGui_Highlight_Text, displayText);

            if(ImGui.BeginPopupContextItem($"contextMenu_{imguiID}"))
            {
                if(ImGui.Selectable("Go to row"))
                {

                }

                ImGui.EndPopup();
            }
        }
    }

    /// <summary>
    /// Param Text Reference
    /// </summary>
    /// <param name="imguiID"></param>
    /// <param name="curParam"></param>
    /// <param name="curRow"></param>
    /// <param name="curField"></param>
    /// <param name="curValue"></param>
    /// <param name="fieldMeta"></param>
    /// <param name="isReadOnly"></param>
    public void ParamTextRef(string imguiID, ParamBank targetBank, Param curParam, Row curRow, Column curField, object curValue, ParamFieldMeta fieldMeta, bool isReadOnly = false)
    {
        if (fieldMeta.FmgRef == null || curRow == null)
            return;

        foreach (ParamTextRef r in fieldMeta.FmgRef)
        {
            Param.Cell? conditionalField = curRow?[r.conditionField];

            if (conditionalField == null)
                continue;

            var fieldValue = conditionalField.Value.Value;
            uint checkFieldValue = 0;
            var isValid = uint.TryParse($"{fieldValue}", out checkFieldValue);

            if (!isValid)
                continue;

            if (checkFieldValue != r.conditionValue)
                continue;

            // TODO: add fmg lookup here once Text Editor is implemented
            // r.offset
            // r.fmg
        }
    }

    /// <summary>
    /// Alias
    /// </summary>
    /// <param name="imguiID"></param>
    /// <param name="curParam"></param>
    /// <param name="curRow"></param>
    /// <param name="curField"></param>
    /// <param name="curValue"></param>
    /// <param name="fieldMeta"></param>
    /// <param name="isReadOnly"></param>
    public void AliasRef(string imguiID, string aliasName, Param curParam, Row curRow, Column curField, object curValue, ParamFieldMeta fieldMeta, bool metaCondition, List<AliasEntry> sourceList, bool isReadOnly = false)
    {
        if (!metaCondition)
            return;

        if (!sourceList.Any(e => e.ID == $"{curValue}"))
            return;

        var matchedEntry = sourceList.Where(e => e.ID == $"{curValue}").FirstOrDefault();
        if (matchedEntry == null)
            return;

        ImGui.TextColored(UI.Current.ImGui_Highlight_Text, $"{matchedEntry.Name} <{aliasName}>");
    }

    /// <summary>
    /// Conditional Alias
    /// </summary>
    /// <param name="imguiID"></param>
    /// <param name="curParam"></param>
    /// <param name="curRow"></param>
    /// <param name="curField"></param>
    /// <param name="curValue"></param>
    /// <param name="fieldMeta"></param>
    /// <param name="metaCondition"></param>
    /// <param name="sourceList"></param>
    /// <param name="conditionalField"></param>
    /// <param name="conditionalValue"></param>
    /// <param name="isReadOnly"></param>
    public void ConditionalAliasRef(string imguiID, string aliasName, Param curParam, Row curRow, Column curField, object curValue, ParamFieldMeta fieldMeta, bool metaCondition, List<AliasEntry> sourceList, string conditionalField, string conditionalValue, bool isReadOnly = false)
    {
        if (!metaCondition)
            return;

        var conditionalCol = curRow.Columns.Where(e => e.Def.InternalName == conditionalField).FirstOrDefault();

        if (conditionalCol == null)
            return;

        // If the conditional value is present, then show the alias
        var checkValue = $"{conditionalCol.GetValue(curRow)}";
        if (checkValue != conditionalValue)
            return;

        if (!sourceList.Any(e => e.ID == $"{curValue}"))
            return;

        var matchedEntry = sourceList.Where(e => e.ID == $"{curValue}").FirstOrDefault();
        if (matchedEntry == null)
            return;

        ImGui.TextColored(UI.Current.ImGui_Highlight_Text, $"{matchedEntry.Name} <{aliasName}>");
    }

    /// <summary>
    /// Param Field Offset
    /// </summary>
    /// <param name="imguiID"></param>
    /// <param name="curParam"></param>
    /// <param name="curRow"></param>
    /// <param name="curField"></param>
    /// <param name="curValue"></param>
    /// <param name="fieldMeta"></param>
    /// <param name="isReadOnly"></param>
    public void ParamFieldOffset(string imguiID, Param curParam, Row curRow, Column curField, object curValue, ParamFieldMeta fieldMeta, bool isReadOnly = false)
    {
        if (Editor.Selection._selectedParamName != "MenuPropertySpecParam")
            return;

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

            if (offset != decimalOffset)
                continue;

            internalName = col.Def.InternalName;

            var tempFieldMeta = targetParamMeta.GetField(col.Def);
            displayName = tempFieldMeta.AltName;
        }

        ImGui.TextColored(UI.Current.ImGui_Highlight_Text, paramString);

        if (CFG.Current.DisplayCommunityFieldNames)
        {
            ImGui.TextColored(UI.Current.ImGui_Highlight_Text, $"Drawing from:");
            ImGui.TextColored(UI.Current.ImGui_Highlight_Text, $"{displayName}");
        }
        else
        {
            ImGui.TextColored(UI.Current.ImGui_Highlight_Text, $"Drawing from:");
            ImGui.TextColored(UI.Current.ImGui_Highlight_Text, $"{internalName}");
        }
    }
}
