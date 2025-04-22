using HKLib.hk2018;
using Smithbox.Core.Editor;
using Smithbox.Core.ParamEditorNS;
using Smithbox.Core.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.BehaviorEditorNS;

public class BehaviorSelection
{
    public Project Project;
    public BehaviorEditor Editor;

    public int _selectedFileIndex;
    public string _selectedFileName;

    // TODO: move the node selection stuff into here

    public BehaviorSelection(Project curProject, BehaviorEditor editor)
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
    }
}