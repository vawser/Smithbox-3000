using Andre.Formats;
using Smithbox.Core.Editor;
using Smithbox.Core.ParamEditorNS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.Actions;

public class AddParamRow : AtomicAction
{
    private readonly ParamEditor Editor;

    private readonly List<RowSelect> Clonables = new();

    private readonly Param Param;
    private readonly List<Param.Row> Clones = new();

    private readonly int IdOffset;
    private readonly int InsertIndex;

    private readonly bool IsAppend;
    private readonly bool ApplyInnerEdits;

    public AddParamRow(ParamEditor editor, Param param, List<RowSelect> rows, int idOffset = 1, int index = -1, bool isAppend = false,  bool applyInnerEdits = false)
    {
        Editor = editor;
        Param = param;
        Clonables.AddRange(rows);

        IsAppend = isAppend;
        InsertIndex = index;
        IdOffset = idOffset;

        ApplyInnerEdits = applyInnerEdits;
    }

    public override ActionEvent Execute()
    {
        var newSelection = new List<RowSelect>();

        foreach (var entry in Clonables)
        {
            var curIndex = entry.Index;
            var newRow = new Param.Row(entry.Row);

            // Apply offset
            newRow.ID = newRow.ID + IdOffset;

            if (InsertIndex > -1)
            {
                newRow.Name = entry.Row.Name != null ? entry.Row.Name + "_1" : "";
                Param.InsertRow(InsertIndex, newRow);

                newSelection.Add(new RowSelect(InsertIndex, newRow));
            }
            else
            {
                // If row with this ID already exists, we need to insert it by index after
                if (Param[newRow.ID] != null)
                {
                    int index = -1;
                    Param.Row row = null;

                    for (var i = 0; i < Param.Rows.Count; i++)
                    {
                        if (Param.Rows[i].ID == newRow.ID)
                        {
                            index = i;
                            row = Param.Rows[i];
                        }
                    }

                    if(row != null && index != -1)
                    {
                        Param.InsertRow(index + 1, newRow);
                        newSelection.Add(new RowSelect(index + 1, newRow));
                    }
                }

                // If row doesn't exist 
                if (Param[newRow.ID] == null)
                {
                    // Simply appned it to the current list
                    if (IsAppend)
                    {
                        Param.AddRow(newRow);
                    }
                    // Or insert it at its natural index within the current list (based on ID) 
                    else
                    {
                        var index = 0;
                        foreach (Param.Row r in Param.Rows)
                        {
                            if (r.ID > newRow.ID)
                            {
                                break;
                            }

                            index++;
                        }

                        Param.InsertRow(index, newRow);
                        newSelection.Add(new RowSelect(index, newRow));
                    }
                }
            }

            Clones.Add(newRow);
        }

        //ParamBank.RefreshParamDifferenceCacheTask();

        // Select the new rows
        Editor.Selection._selectedRows = newSelection;

        return ActionEvent.NoEvent;
    }

    public List<Param.Row> GetResultantRows()
    {
        return Clones;
    }

    public override ActionEvent Undo()
    {
        for (var i = 0; i < Clones.Count(); i++)
        {
            Param.RemoveRow(Clones[i]);
        }

        Clones.Clear();

        Editor.Selection._selectedRows.Clear();

        return ActionEvent.NoEvent;
    }
}