using Smithbox.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.MapEditorNS;

public class MapFieldView
{
    public Project Project;
    public MapEditor Editor;

    public MapFieldView(Project curProject, MapEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public void Draw()
    {

    }
}
