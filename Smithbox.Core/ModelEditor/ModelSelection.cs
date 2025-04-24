using HKLib.hk2018.hkaiCollisionAvoidance;
using Microsoft.Extensions.Logging;
using Smithbox.Core.BehaviorEditorNS;
using Smithbox.Core.Editor;
using Smithbox.Core.Utils;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.ModelEditorNS;

public class ModelSelection
{
    public Project Project;
    public ModelEditor Editor;

    public int _selectedFileIndex = -1;
    public string _selectedFileName = "";

    public int _selectedInternalFileIndex = -1;
    public string _selectedInternalFileName = "";

    public IFlver _selectedFlver;

    public ModelSelection(Project curProject, ModelEditor editor)
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

        _selectedInternalFileIndex = -1;
        _selectedInternalFileName = "";

        _selectedFlver = null;
    }

    public bool IsInternalFileSelected(int index, string fileName)
    {
        if (_selectedInternalFileIndex == index)
        {
            return true;
        }

        return false;
    }

    public void SelectInternalFile(int index, string fileName, BinderFile curFile)
    {
        _selectedInternalFileIndex = index;
        _selectedInternalFileName = fileName;

        try
        {
            _selectedFlver = FLVER2.Read(curFile.Bytes.ToArray());
        }
        catch(Exception ex)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Model Editor] Failed to load file: {curFile.Name}", LogLevel.Warning);
        }
    }
}
