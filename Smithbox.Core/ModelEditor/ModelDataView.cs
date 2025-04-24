using Hexa.NET.ImGui;
using Smithbox.Core.Editor;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.ModelEditorNS;

public class ModelDataView
{
    public Project Project;
    public ModelEditor Editor;

    public ModelDataView(Project curProject, ModelEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public void Draw()
    {
        if(Editor.Selection._selectedFlver != null)
        {
            // Assume FLVER2 for now
            var curFlver = (FLVER2)Editor.Selection._selectedFlver;

            ImGui.Text($"{curFlver.Header.Version}");
        }
    }
}
