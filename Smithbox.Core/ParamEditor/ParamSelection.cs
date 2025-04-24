using Andre.Formats;
using Smithbox.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.ParamEditorNS;

public class ParamSelection
{
    public Project Project;
    public ParamEditor Editor;

    public int _selectedParamIndex;
    public string _selectedParamName;
    public Param _selectedParam;

    public List<RowSelect> _selectedRows = new();

    public int _selectedFieldIndex;
    public Param.Column _selectedField;

    public ParamSelection(Project curProject, ParamEditor editor)
    {
        Project = curProject;
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

    public Param.Row GetSelectedRow(int position = 0)
    {
        var isSelected = false;

        if(_selectedRows.Count > position)
        {
            return _selectedRows[position].Row;
        }

        return null;
    }

    public Param.Column GetSelectedField(int position = -1)
    {
        return _selectedField;
    }

    public bool IsFieldDisplayValid()
    {
        if (Editor.Selection._selectedParam != null && GetSelectedRow() != null)
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
        _selectedRows = new();

        // Reset field
        _selectedFieldIndex = -1;
        _selectedField = null;

        Editor.FieldView.InvalidateColumns();
    }

    public void SelectRow(int index, Param.Row row, SelectMode selectMode = SelectMode.ClearAndSelect)
    {
        var newRowSelect = new RowSelect(index, row);

        // Clear and Add
        if (selectMode is SelectMode.ClearAndSelect)
        {
            _selectedRows.Clear();
            _selectedRows.Add(newRowSelect);
        }
        // Append
        else if (selectMode is SelectMode.SelectAppend)
        {
            // Only add if not already present
            if (!_selectedRows.Any(e => e.Index == index))
            {
                _selectedRows.Add(newRowSelect);
            }
            // Allow deselect for this action
            else if (_selectedRows.Any(e => e.Index == index))
            {
                var curRowSelect = _selectedRows.Where(e => e.Index == index).FirstOrDefault();
                if (curRowSelect != null)
                {
                    _selectedRows.Remove(curRowSelect);
                }
            }
        }
        // Range Append
        else if (selectMode is SelectMode.SelectRangeAppend)
        {
            var lastRow = _selectedRows.Last();
            var lastRowId = lastRow.Row.ID;
            var curRowID = row.ID;

            if (curRowID < lastRowId)
            {
                for (int i = 0; i < _selectedParam.Rows.Count; i++)
                {
                    var tRow = _selectedParam.Rows[i];

                    if (tRow.ID >= curRowID && tRow.ID <= lastRowId)
                    {
                        if (!_selectedRows.Any(e => e.Index == i))
                        {
                            var tRowSelect = new RowSelect(i, tRow);
                            _selectedRows.Add(tRowSelect);
                        }
                    }
                }
            }
            else if (curRowID > lastRowId)
            {
                for (int i = 0; i < _selectedParam.Rows.Count; i++)
                {
                    var tRow = _selectedParam.Rows[i];

                    if (tRow.ID <= curRowID && tRow.ID >= lastRowId)
                    {
                        if (!_selectedRows.Any(e => e.Index == i))
                        {
                            var tRowSelect = new RowSelect(i, tRow);
                            _selectedRows.Add(tRowSelect);
                        }
                    }
                }
            }
            else
            {
                // Ignore if the curRow is the lastRow
            }
        }
        // All
        else if (selectMode is SelectMode.SelectAll)
        {
            _selectedRows.Clear();

            for (int i = 0; i < _selectedParam.Rows.Count; i++)
            {
                var tRow = _selectedParam.Rows[i];

                var tRowSelect = new RowSelect(i, tRow);
                _selectedRows.Add(tRowSelect);
            }
        }

        // Reset field
        _selectedFieldIndex = -1;
        _selectedField = null;

        // Re-run current field filter on row switch so the field view updates seemlessly
        Editor.FieldView.UpdateFieldVisibility(row);
        Editor.FieldImageView.UpdateIconPreview(row);
    }

    public bool IsMultipleRowsSelected()
    {
        if (_selectedRows.Count > 1)
            return true;

        return false;
    }

    public bool IsRowSelected(int index, Param.Row row)
    {
        foreach(var entry in _selectedRows)
        {
            if (entry.Index == index)
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

public enum SelectMode
{
    ClearAndSelect,
    SelectAppend,
    SelectRangeAppend,
    SelectAll
}

public class RowSelect
{
    public int Index { get; set; }
    public Param.Row Row { get; set; }

    public RowSelect(int index, Param.Row row)
    {
        Index = index;
        Row = row;
    }
}