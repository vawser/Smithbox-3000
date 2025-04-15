using Andre.Formats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.ParamEditorNS;

public class ParamSelection
{
    public ParamEditor Editor;

    public int _selectedParamIndex;
    public string _selectedParamName;
    public Param _selectedParam;

    public int _selectedRowIndex;
    public Param.Row _selectedRow;

    public int _selectedFieldIndex;
    public Param.Column _selectedField;

    public ParamSelection(ParamEditor editor)
    {
        Editor = editor;
    }

    public Param GetSelectedParam(int position = -1)
    {
        return _selectedParam;
    }

    public Param GetSelectedParamFromBank(ParamBank targetBank)
    {
        if(targetBank.Params.ContainsKey(_selectedParamName))
        {
            return targetBank.Params[_selectedParamName];
        }

        return null;
    }

    public Param.Row GetSelectedRow(int position = -1)
    {
        return _selectedRow;
    }

    public Param.Column GetSelectedField(int position = -1)
    {
        return _selectedField;
    }

    public bool IsFieldSelectionValid()
    {
        if (Editor.Selection._selectedParam != null && Editor.Selection._selectedRow != null)
            return true;

        return false;
    }

    public bool IsParamSelected(int index, string paramName, Param param)
    {
        if(paramName == _selectedParamName)
        {
            return true;
        }

        return false;
    }

    public void SelectParam(int index, string paramName, Param param)
    {
        _selectedParamIndex = index;
        _selectedParamName = paramName;
        _selectedParam = param;

        // Reset row
        _selectedRowIndex = -1;
        _selectedRow = null;

        // Reset field
        _selectedFieldIndex = -1;
        _selectedField = null;
    }

    public void SelectRow(int index, Param.Row row)
    {
        _selectedRowIndex = index;
        _selectedRow = row;

        // Reset field
        _selectedFieldIndex = -1;
        _selectedField = null;
    }

    public bool IsRowSelected(int index, Param.Row row)
    {
        if(index == _selectedRowIndex)
        {
            return true;
        }

        return false;
    }

    public void SelectField(int index, Param.Column column)
    {
        _selectedFieldIndex = index;
        _selectedField = column;
    }

    public bool IsFieldSelected(int index, Param.Column column)
    {
        if(index == _selectedFieldIndex)
        {
            return true;
        }

        return false;
    }
}