using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.ParamEditorNS.Meta;

/// <summary>
/// A text reference: pointing to an entry in a FMG file.
/// </summary>
public class ParamTextRef
{
    public string conditionField;
    public int conditionValue;
    public int offset;
    public string fmg;

    internal ParamTextRef(ParamMeta curMeta, string refString)
    {
        var conditionSplit = refString.Split('(', 2, StringSplitOptions.TrimEntries);
        var offsetSplit = conditionSplit[0].Split('+', 2);
        fmg = offsetSplit[0];
        if (offsetSplit.Length > 1)
        {
            offset = int.Parse(offsetSplit[1]);
        }

        if (conditionSplit.Length > 1 && conditionSplit[1].EndsWith(')'))
        {
            var condition = conditionSplit[1].Substring(0, conditionSplit[1].Length - 1)
                .Split('=', 2, StringSplitOptions.TrimEntries);
            conditionField = condition[0];
            conditionValue = int.Parse(condition[1]);
        }
    }

    internal string getStringForm()
    {
        return conditionField != null ? fmg + '(' + conditionField + '=' + conditionValue + ')' : fmg;
    }
}