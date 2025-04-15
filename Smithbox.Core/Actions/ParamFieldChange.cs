using Smithbox.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Andre.Formats.Param;

namespace Smithbox.Core.Actions;

public class ParamFieldChange : AtomicAction
{
    private readonly Row Row;
    private readonly Column Column;
    private readonly object NewValue;
    private readonly object OldValue;
    private Action<bool> PostExecutionAction;

    public ParamFieldChange(Row curRow, Column curColumn, object curValue, object newValue)
    {
        Row = curRow;
        Column = curColumn;
        NewValue = newValue;
        OldValue = curValue;
    }

    public override ActionEvent Execute()
    {
        Column.SetValue(Row, NewValue);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Column.SetValue(Row, OldValue);

        return ActionEvent.NoEvent;
    }
}