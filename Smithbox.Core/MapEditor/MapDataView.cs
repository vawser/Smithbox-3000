using Smithbox.Core.Editor;

namespace Smithbox.Core.MapEditorNS;

public class MapDataView
{
    public Project Project;
    public MapEditor Editor;

    public MapDataView(Project curProject, MapEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public void Draw()
    {
        if (Editor.Selection._selectedMap != null)
        {

        }
    }
}
