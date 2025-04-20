using Smithbox.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.BehaviorEditorNS;

public class BehaviorTreeView
{
    public Project Project;
    public BehaviorEditor Editor;

    public BehaviorTreeView(Project curProject, BehaviorEditor editor)
    {
        Editor = editor;
        Project = curProject;
    }

    public void Draw()
    {

    }
}
