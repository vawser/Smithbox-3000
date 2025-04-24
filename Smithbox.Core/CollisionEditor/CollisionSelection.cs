using HKLib.hk2018;
using HKLib.Serialization.hk2018.Binary;
using Smithbox.Core.BehaviorEditorNS;
using Smithbox.Core.Editor;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.CollisionEditorNS;

public class CollisionSelection
{
    public Project Project;
    public CollisionEditor Editor;

    public int _selectedFileIndex = -1;
    public string _selectedFileName = "";

    public int _selectedInternalFileIndex = -1;
    public string _selectedInternalFileName = "";

    public HavokBinarySerializer _selectedSerializer;
    public hkRootLevelContainer _selectedHavokRoot;

    public IHavokObject SelectedObject;

    public CollisionSelection(Project curProject, CollisionEditor editor)
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
    }

    public void SelectHavokRoot(BinderFile compendiumFile, BinderFile curFile)
    {
        _selectedSerializer = new HavokBinarySerializer();

        // Decompress the files
        var decompCompendiumFile = DCX.Decompress(compendiumFile.Bytes.ToArray());
        var decompCurFile = DCX.Decompress(curFile.Bytes.ToArray());

        using (MemoryStream memoryStream = new MemoryStream(decompCompendiumFile.ToArray()))
        {
            _selectedSerializer.LoadCompendium(memoryStream);
        }

        using (MemoryStream memoryStream = new MemoryStream(decompCurFile.ToArray()))
        {
            _selectedHavokRoot = (hkRootLevelContainer)_selectedSerializer.Read(memoryStream);
        }

        Project.CollisionData.BuildCategories(_selectedHavokRoot);
    }
}
