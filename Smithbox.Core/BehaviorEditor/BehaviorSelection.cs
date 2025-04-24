﻿using HKLib.hk2018;
using HKLib.hk2018.hk;
using HKLib.Serialization.hk2018.Binary;
using Smithbox.Core.Editor;
using Smithbox.Core.ParamEditorNS;
using Smithbox.Core.Resources;
using SoulsFormats;
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

    public int _selectedFileIndex = -1;
    public string _selectedFileName = "";

    public int _selectedInternalFileIndex = -1;
    public string _selectedInternalFileName = "";
    public HavokCategoryType _selectedInternalFileCategory = HavokCategoryType.None;

    public HavokBinarySerializer _selectedSerializer;
    public hkRootLevelContainer _selectedHavokRoot;

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

        _selectedInternalFileIndex = -1;
        _selectedInternalFileName = "";
        _selectedInternalFileCategory = HavokCategoryType.None;

        _selectedSerializer = null;
        _selectedHavokRoot = null;

        // Undo/Redo is reset on file switch
        Editor.ActionManager.Clear();
    }

    public bool IsInternalFileSelected(int index, string fileName)
    {
        if (_selectedInternalFileIndex == index)
        {
            return true;
        }

        return false;
    }

    public void SelectInternalFile(int index, string fileName)
    {
        _selectedInternalFileIndex = index;
        _selectedInternalFileName = fileName;

        _selectedInternalFileCategory = BehaviorUtils.GetInternalFileCategoryType(fileName);
    }

    public void SelectHavokRoot(BinderFile curFile)
    {
        _selectedSerializer = new HavokBinarySerializer();
        using (MemoryStream memoryStream = new MemoryStream(curFile.Bytes.ToArray()))
        {
            _selectedHavokRoot = (hkRootLevelContainer)_selectedSerializer.Read(memoryStream);
        }

        Project.BehaviorData.BuildCategories(_selectedInternalFileCategory, _selectedHavokRoot);
    }
}
