﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.Editor;

/// <summary>
/// An action that can be performed by the user in the editor that represents
/// a single atomic editor action that affects the state of the map. Each action
/// should have enough information to apply the action AND undo the action, as
/// these actions get pushed to a stack for undo/redo
/// </summary>
public abstract class AtomicAction
{
    public abstract ActionEvent Execute();
    public abstract ActionEvent Undo();
}
