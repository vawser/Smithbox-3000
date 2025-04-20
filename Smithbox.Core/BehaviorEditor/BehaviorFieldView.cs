using Smithbox.Core.Editor;
using Smithbox.Core.Interface.NodeEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.BehaviorEditorNS;

public class BehaviorFieldView
{
    public Project Project;
    public BehaviorEditor Editor;

    public BehaviorFieldView(Project curProject, BehaviorEditor editor)
    {
        Editor = editor;
        Project = curProject;
    }
    public void Draw()
    {

    }
}