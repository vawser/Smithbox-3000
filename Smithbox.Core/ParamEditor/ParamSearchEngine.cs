using Andre.Formats;
using Smithbox.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Andre.Formats.Param;
using static SoulsFormats.PARAMDEF;

namespace Smithbox.Core.ParamEditorNS;

public class ParamSearchEngine
{
    private Project Project;
    private ParamEditor Editor;

    public string ParamFilterInput = "";
    public string RowFilterInput = "";
    public string FieldFilterInput = "";

    public bool IsRegexLenient = false;

    public ParamSearchEngine(Project curProject, ParamEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public bool DisplaySearchTermBuilder = false;

    public void Draw()
    {
        if(DisplaySearchTermBuilder)
        {

        }
    }

    // These are the parameters used for the latest search, set for the field visibility update on row change
    public Param StoredParam;
    public ParamMeta StoredMeta;

    /// <summary>
    /// Param Field search filtering
    /// </summary>
    /// <param name="curParam"></param>
    /// <param name="curRow"></param>
    /// <param name="curMeta"></param>
    /// <returns></returns>
    public Dictionary<int, bool> ProcessFieldVisibility(Param curParam, Row curRow, ParamMeta curMeta)
    {
        StoredParam = curParam;
        StoredMeta = curMeta;

        var filterResult = new Dictionary<int, bool>();

        for (int i = 0; i < Editor.FieldView.PrimaryOrderedColumns.Count(); i++)
        {
            var visible = true;

            var curField = curRow.Columns.ElementAt(i);
            var curFieldMeta = curMeta.Fields[curField.Def];

            var curValue = curField.GetValue(curRow);

            visible = GetFieldTruth(curRow, curField, curValue, curMeta);

            // Hide padding if it is disabled
            if(!CFG.Current.DisplayPaddingFields)
            {
                if(curFieldMeta.IsPaddingField)
                {
                    visible = false;
                }
            }

            filterResult.Add(i, visible);
        }

        return filterResult;
    }

    public bool GetFieldTruth(Row curRow, Column curColumn, object curValue, ParamMeta curMeta)
    {
        // value: [^operator] [value]
        if (FieldFilterInput.Contains("value:"))
        {
            var isStringCheck = curColumn.Def.DisplayType is DefType.fixstr or DefType.fixstrW;
            var args = FieldFilterInput.Replace("value:", "");

            var input = args.Trim();
            var inputArgs = input.Split(" ");

            if(inputArgs.Length >= 2)
            {
                var operation = inputArgs[0];
                var value = inputArgs[1];

                return IsValueMatch($"{curValue}", value, curColumn.Def.DisplayType, operation);
            }
            else if(isStringCheck)
            {
                return IsValueMatch($"{curValue}", input, curColumn.Def.DisplayType, "");
            }

            return false;
        }
        // range: [min] [max]
        else if (FieldFilterInput.Contains("range:"))
        {
            var args = FieldFilterInput.Replace("range:", "");

            var input = args.Trim();
            var inputArgs = input.Split(" ");

            if (inputArgs.Length >= 2)
            {
                var min = inputArgs[0];
                var max = inputArgs[1];

                return IsRangeMatch($"{curValue}", min, max, curColumn.Def.DisplayType);
            }

            return false;
        }
        // field: [name]
        else if (FieldFilterInput.Contains("field:"))
        {
            var args = FieldFilterInput.Replace("field:", "");

            var input = args.Trim();

            Regex reg = new Regex(input, RegexOptions.IgnoreCase);

            if (IsRegexLenient)
            {
                reg = new Regex($@"^{input}$");
            }

            if (reg.IsMatch(curColumn.Def.InternalName))
            {
                return true;
            }

            return false;
        }
        // Default is basic string loose match for value
        else
        {
            var input = FieldFilterInput.Trim();

            Regex reg = new Regex(input, RegexOptions.IgnoreCase);

            if (IsRegexLenient)
            {
                reg = new Regex($@"^{input}$");
            }

            if (reg.IsMatch($"{curValue}"))
            {
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Check a field value against a target value
    /// </summary>
    /// <param name="fieldValue"></param>
    /// <param name="checkValue"></param>
    /// <param name="internalType"></param>
    /// <returns></returns>
    private bool IsValueMatch(string fieldValue, string checkValue, DefType type, string operation)
    {
        var fieldSuccess = false;
        var checkSuccess = false;

        switch (type)
        {
            // Support checking strings here, as it feels natural for a user to use this like that
            case DefType.fixstr:
            case DefType.fixstrW:
                if(fieldValue.Contains(checkValue))
                {
                    return true;
                }
                break;
            case DefType.s8:
                sbyte sbyteFieldVal;
                sbyte sbyteStartVal;

                fieldSuccess = sbyte.TryParse(fieldValue, out sbyteFieldVal);
                checkSuccess = sbyte.TryParse(checkValue, out sbyteStartVal);

                if (fieldSuccess && checkSuccess)
                {
                    switch(operation)
                    {
                        case "=":
                            if ((sbyteFieldVal == sbyteStartVal))
                            {
                                return true;
                            }
                            break;
                        case ">=":
                            if ((sbyteFieldVal >= sbyteStartVal))
                            {
                                return true;
                            }
                            break;
                        case ">":
                            if ((sbyteFieldVal > sbyteStartVal))
                            {
                                return true;
                            }
                            break;
                        case "<=":
                            if ((sbyteFieldVal <= sbyteStartVal))
                            {
                                return true;
                            }
                            break;
                        case "<":
                            if ((sbyteFieldVal < sbyteStartVal))
                            {
                                return true;
                            }
                            break;
                    }
                }
                break;
            case DefType.u8:
                byte byteFieldVal;
                byte byteStartVal;

                fieldSuccess = byte.TryParse(fieldValue, out byteFieldVal);
                checkSuccess = byte.TryParse(checkValue, out byteStartVal);

                if (fieldSuccess && checkSuccess)
                {
                    switch (operation)
                    {
                        case "=":
                            if ((byteFieldVal == byteStartVal))
                            {
                                return true;
                            }
                            break;
                        case ">=":
                            if ((byteFieldVal >= byteStartVal))
                            {
                                return true;
                            }
                            break;
                        case ">":
                            if ((byteFieldVal > byteStartVal))
                            {
                                return true;
                            }
                            break;
                        case "<=":
                            if ((byteFieldVal <= byteStartVal))
                            {
                                return true;
                            }
                            break;
                        case "<":
                            if ((byteFieldVal < byteStartVal))
                            {
                                return true;
                            }
                            break;
                    }
                }
                break;
            case DefType.s16:
                short shortFieldVal;
                short shortStartVal;

                fieldSuccess = short.TryParse(fieldValue, out shortFieldVal);
                checkSuccess = short.TryParse(checkValue, out shortStartVal);

                if (fieldSuccess && checkSuccess)
                {
                    switch (operation)
                    {
                        case "=":
                            if ((shortFieldVal == shortStartVal))
                            {
                                return true;
                            }
                            break;
                        case ">=":
                            if ((shortFieldVal >= shortStartVal))
                            {
                                return true;
                            }
                            break;
                        case ">":
                            if ((shortFieldVal > shortStartVal))
                            {
                                return true;
                            }
                            break;
                        case "<=":
                            if ((shortFieldVal <= shortStartVal))
                            {
                                return true;
                            }
                            break;
                        case "<":
                            if ((shortFieldVal < shortStartVal))
                            {
                                return true;
                            }
                            break;
                    }
                }
                break;
            case DefType.u16:
                ushort ushortFieldVal;
                ushort ushortStartVal;

                fieldSuccess = ushort.TryParse(fieldValue, out ushortFieldVal);
                checkSuccess = ushort.TryParse(checkValue, out ushortStartVal);

                if (fieldSuccess && checkSuccess)
                {
                    switch (operation)
                    {
                        case "=":
                            if ((ushortFieldVal == ushortStartVal))
                            {
                                return true;
                            }
                            break;
                        case ">=":
                            if ((ushortFieldVal >= ushortStartVal))
                            {
                                return true;
                            }
                            break;
                        case ">":
                            if ((ushortFieldVal > ushortStartVal))
                            {
                                return true;
                            }
                            break;
                        case "<=":
                            if ((ushortFieldVal <= ushortStartVal))
                            {
                                return true;
                            }
                            break;
                        case "<":
                            if ((ushortFieldVal < ushortStartVal))
                            {
                                return true;
                            }
                            break;
                    }
                }
                break;
            case DefType.s32:
                int intFieldVal;
                int intStartVal;

                fieldSuccess = int.TryParse(fieldValue, out intFieldVal);
                checkSuccess = int.TryParse(checkValue, out intStartVal);

                if (fieldSuccess && checkSuccess)
                {
                    switch (operation)
                    {
                        case "=":
                            if ((intFieldVal == intStartVal))
                            {
                                return true;
                            }
                            break;
                        case ">=":
                            if ((intFieldVal >= intStartVal))
                            {
                                return true;
                            }
                            break;
                        case ">":
                            if ((intFieldVal > intStartVal))
                            {
                                return true;
                            }
                            break;
                        case "<=":
                            if ((intFieldVal <= intStartVal))
                            {
                                return true;
                            }
                            break;
                        case "<":
                            if ((intFieldVal < intStartVal))
                            {
                                return true;
                            }
                            break;
                    }
                }
                break;
            case DefType.u32:
                uint uintFieldVal;
                uint uintStartVal;

                fieldSuccess = uint.TryParse(fieldValue, out uintFieldVal);
                checkSuccess = uint.TryParse(checkValue, out uintStartVal);

                if (fieldSuccess && checkSuccess)
                {
                    switch (operation)
                    {
                        case "=":
                            if ((uintFieldVal == uintStartVal))
                            {
                                return true;
                            }
                            break;
                        case ">=":
                            if ((uintFieldVal >= uintStartVal))
                            {
                                return true;
                            }
                            break;
                        case ">":
                            if ((uintFieldVal > uintStartVal))
                            {
                                return true;
                            }
                            break;
                        case "<=":
                            if ((uintFieldVal <= uintStartVal))
                            {
                                return true;
                            }
                            break;
                        case "<":
                            if ((uintFieldVal < uintStartVal))
                            {
                                return true;
                            }
                            break;
                    }
                }
                break;
            case DefType.f32:
                double doubleFieldVal;
                double doubleStartVal;

                fieldSuccess = double.TryParse(fieldValue, out doubleFieldVal);
                checkSuccess = double.TryParse(checkValue, out doubleStartVal);

                if (fieldSuccess && checkSuccess)
                {
                    switch (operation)
                    {
                        case "=":
                            if ((doubleFieldVal == doubleStartVal))
                            {
                                return true;
                            }
                            break;
                        case ">=":
                            if ((doubleFieldVal >= doubleStartVal))
                            {
                                return true;
                            }
                            break;
                        case ">":
                            if ((doubleFieldVal > doubleStartVal))
                            {
                                return true;
                            }
                            break;
                        case "<=":
                            if ((doubleFieldVal <= doubleStartVal))
                            {
                                return true;
                            }
                            break;
                        case "<":
                            if ((doubleFieldVal < doubleStartVal))
                            {
                                return true;
                            }
                            break;
                    }
                }
                break;
            default: break;
        }

        return false;
    }

    /// <summary>
    /// Check a field value is within the range of two values
    /// </summary>
    /// <param name="fieldValue"></param>
    /// <param name="startValue"></param>
    /// <param name="endValue"></param>
    /// <param name="internalType"></param>
    /// <returns></returns>
    private bool IsRangeMatch(string fieldValue, string startValue, string endValue, DefType type)
    {
        var fieldSuccess = false;
        var startSuccess = false;
        var endSuccess = false;

        switch (type)
        {
            case DefType.s8:
                sbyte sbyteFieldVal;
                sbyte sbyteStartVal;
                sbyte sbyteEndVal;

                fieldSuccess = sbyte.TryParse(fieldValue, out sbyteFieldVal);
                startSuccess = sbyte.TryParse(startValue, out sbyteStartVal);
                endSuccess = sbyte.TryParse(endValue, out sbyteEndVal);

                if (fieldSuccess && startSuccess && endSuccess)
                {
                    if ((sbyteFieldVal >= sbyteStartVal) &&
                        (sbyteFieldVal <= sbyteEndVal))
                    {
                        return true;
                    }
                }
                break;
            case DefType.u8:
                byte byteFieldVal;
                byte byteStartVal;
                byte byteEndVal;

                fieldSuccess = byte.TryParse(fieldValue, out byteFieldVal);
                startSuccess = byte.TryParse(startValue, out byteStartVal);
                endSuccess = byte.TryParse(endValue, out byteEndVal);

                if (fieldSuccess && startSuccess && endSuccess)
                {
                    if ((byteFieldVal >= byteStartVal) &&
                        (byteFieldVal <= byteEndVal))
                    {
                        return true;
                    }
                }
                break;
            case DefType.s16:
                short shortFieldVal;
                short shortStartVal;
                short shortEndVal;

                fieldSuccess = short.TryParse(fieldValue, out shortFieldVal);
                startSuccess = short.TryParse(startValue, out shortStartVal);
                endSuccess = short.TryParse(endValue, out shortEndVal);

                if (fieldSuccess && startSuccess && endSuccess)
                {
                    if ((shortFieldVal >= shortStartVal) &&
                        (shortFieldVal <= shortEndVal))
                    {
                        return true;
                    }
                }
                break;
            case DefType.u16:
                ushort ushortFieldVal;
                ushort ushortStartVal;
                ushort ushortEndVal;

                fieldSuccess = ushort.TryParse(fieldValue, out ushortFieldVal);
                startSuccess = ushort.TryParse(startValue, out ushortStartVal);
                endSuccess = ushort.TryParse(endValue, out ushortEndVal);

                if (fieldSuccess && startSuccess && endSuccess)
                {
                    if ((ushortFieldVal >= ushortStartVal) &&
                        (ushortFieldVal <= ushortEndVal))
                    {
                        return true;
                    }
                }
                break;
            case DefType.s32:
                int intFieldVal;
                int intStartVal;
                int intEndVal;

                fieldSuccess = int.TryParse(fieldValue, out intFieldVal);
                startSuccess = int.TryParse(startValue, out intStartVal);
                endSuccess = int.TryParse(endValue, out intEndVal);

                if (fieldSuccess && startSuccess && endSuccess)
                {
                    if ((intFieldVal >= intStartVal) &&
                        (intFieldVal <= intEndVal))
                    {
                        return true;
                    }
                }
                break;
            case DefType.u32:
                uint uintFieldVal;
                uint uintStartVal;
                uint uintEndVal;

                fieldSuccess = uint.TryParse(fieldValue, out uintFieldVal);
                startSuccess = uint.TryParse(startValue, out uintStartVal);
                endSuccess = uint.TryParse(endValue, out uintEndVal);

                if (fieldSuccess && startSuccess && endSuccess)
                {
                    if ((uintFieldVal >= uintStartVal) &&
                        (uintFieldVal <= uintEndVal))
                    {
                        return true;
                    }
                }
                break;
            case DefType.f32:
                double floatFieldVal;
                double floatStartVal;
                double floatEndVal;

                fieldSuccess = double.TryParse(fieldValue, out floatFieldVal);
                startSuccess = double.TryParse(startValue, out floatStartVal);
                endSuccess = double.TryParse(endValue, out floatEndVal);

                if (fieldSuccess && startSuccess && endSuccess)
                {
                    if ((floatFieldVal >= floatStartVal) &&
                        (floatFieldVal <= floatEndVal))
                    {
                        return true;
                    }
                }
                break;
            default: break;
        }

        return false;
    }
}
