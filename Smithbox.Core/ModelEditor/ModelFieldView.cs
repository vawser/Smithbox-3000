using Smithbox.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.ModelEditorNS;

public class ModelFieldView
{
    public Project Project;
    public ModelEditor Editor;

    public ModelFieldView(Project curProject, ModelEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public void Draw()
    {

    }
}

