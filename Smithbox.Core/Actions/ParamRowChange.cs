using HKLib.hk2018.hkFileSystem.Watcher.Change;
using Smithbox.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Andre.Formats.Param;

namespace Smithbox.Core.Actions;

public class ParamRowChange : AtomicAction
{
    private readonly Row Row;
    private readonly object NewValue;
    private readonly object OldValue;
    private readonly RowChangeType ChangeType;
    private Action<bool> PostExecutionAction;

    public ParamRowChange(Row curRow,  object curValue, object newValue, RowChangeType changeType)
    {
        Row = curRow;
        NewValue = newValue;
        OldValue = curValue;
        ChangeType = changeType;
    }

    public override ActionEvent Execute()
    {
        if (ChangeType is RowChangeType.ID)
        {
            Row.ID = (int)NewValue;
        }

        if (ChangeType is RowChangeType.Name)
        {
            Row.Name = $"{NewValue}";
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (ChangeType is RowChangeType.ID)
        {
            Row.ID = (int)OldValue;
        }

        if (ChangeType is RowChangeType.Name)
        {
            Row.Name = $"{OldValue}";
        }

        return ActionEvent.NoEvent;
    }

    public enum RowChangeType
    {
        ID,
        Name
    }
}