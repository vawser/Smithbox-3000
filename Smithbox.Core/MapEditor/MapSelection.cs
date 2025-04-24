using Smithbox.Core.Editor;
using Smithbox.Core.ModelEditorNS;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.MapEditorNS;

public class MapSelection
{
    public Project Project;
    public MapEditor Editor;

    public int _selectedFileIndex = -1;
    public string _selectedFileName = "";

    public IMsb _selectedMap;

    public MapSelection(Project curProject, MapEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }
    public bool IsFileSelected(int index, string fileName)
    {
        if (_selectedFileIndex == index)
        {
            return true;
        }

        return false;
    }

    public void SelectFile(int index, string fileName)
    {
        _selectedFileIndex = index;
        _selectedFileName = fileName;

        _selectedMap = null;

        // Load map here
    }
}
