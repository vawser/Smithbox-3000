using Smithbox.Core.ParamEditorNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.BehaviorEditorNS;

public class BehaviorSelection
{
    public BehaviorEditor Editor;

    public BehaviorSelection(BehaviorEditor editor)
    {
        Editor = editor;
    }
}