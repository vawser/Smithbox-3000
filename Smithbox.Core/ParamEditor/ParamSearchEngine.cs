using Andre.Formats;
using Smithbox.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Andre.Formats.Param;

namespace Smithbox.Core.ParamEditorNS;

public class ParamSearchEngine
{
    private Project Project;
    private ParamEditor Editor;

    public string ParamFilterInput = "";
    public string RowFilterInput = "";
    public string FieldFilterInput = "";

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

    /// <summary>
    /// Param Field search filtering
    /// </summary>
    /// <param name="curParam"></param>
    /// <param name="curRow"></param>
    /// <param name="curMeta"></param>
    /// <returns></returns>
    public Dictionary<int, bool> ProcessFieldSearch(Param curParam, Row curRow, ParamMeta curMeta)
    {
        var filterResult = new Dictionary<int, bool>();

        for (int i = 0; i < curRow.Columns.Count(); i++)
        {
            var visible = true;

            var curField = curRow.Columns.ElementAt(i);
            var curValue = curField.GetValue(curRow);

            var truth = GetFieldTruth(curField, curValue, curMeta);

            filterResult.Add(i, visible);
        }

        return filterResult;
    }

    public bool GetFieldTruth(Column curRow, object curValue, ParamMeta curMeta)
    {
        // value: [operator] [value]
        if (FieldFilterInput.Contains("value:"))
        {

        }
        // range: [min] [max]
        else if (FieldFilterInput.Contains("value:"))
        {

        }
        // field: [name]
        else if (FieldFilterInput.Contains("value:"))
        {

        }
        // Default is basic string loose match
        else
        {

        }

        return true;
    }
}
