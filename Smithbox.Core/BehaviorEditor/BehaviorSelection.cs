using Smithbox.Core.ParamEditorNS;
using Smithbox.Core.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.BehaviorEditorNS;

public class BehaviorSelection
{
    public BehaviorEditor Editor;

    public int _selectedFileIndex;
    public string _selectedFileName;

    public BehaviorSelection(BehaviorEditor editor)
    {
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
    }
}