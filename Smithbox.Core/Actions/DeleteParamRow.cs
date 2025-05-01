using Andre.Formats;
using Smithbox.Core.Editor;
using Smithbox.Core.ParamEditorNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.Actions;

public class DeleteParamRow : AtomicAction
{
    private readonly ParamEditor Editor;

    private readonly List<Param.Row> Deletables = new();
    private readonly Param Param;
    private readonly List<int> RemoveIndices = new();
    private readonly bool SetSelection = false;

    public DeleteParamRow(ParamEditor editor, Param param, List<Param.Row> rows)
    {
        Editor = editor;
        Param = param;
        Deletables.AddRange(rows);
    }

    public override ActionEvent Execute()
    {
        foreach (Param.Row row in Deletables)
        {
            RemoveIndices.Add(Param.IndexOfRow(row));
            Param.RemoveRowAt(RemoveIndices.Last());
        }

        Editor.Selection._selectedRows = new();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (var i = Deletables.Count() - 1; i >= 0; i--)
        {
            Param.InsertRow(RemoveIndices[i], Deletables[i]);
        }

        //ParamBank.RefreshParamDifferenceCacheTask();

        Editor.Selection._selectedRows = new();

        return ActionEvent.NoEvent;
    }
}