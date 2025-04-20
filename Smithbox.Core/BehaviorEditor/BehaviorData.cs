using Andre.IO.VFS;
using Smithbox.Core.Editor;
using Smithbox.Core.FileBrowserNS.Entries;
using Smithbox.Core.ParamEditorNS;
using Smithbox.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace Smithbox.Core.BehaviorEditorNS;

public class BehaviorData
{
    public Project Project;

    public BehaviorBank PrimaryBank;

    public VirtualFileSystemFsEntry FsRoot;

    public BehaviorData(Project projectOwner)
    {
        Project = projectOwner;

        PrimaryBank = new(this, "Primary");
    }

    public async void Initialize()
    {
        // VFS Roots
        Task<bool> vfsRootsTask = LoadVfsRoots();
        bool vfsRootsLoaded = await vfsRootsTask;

        if (vfsRootsLoaded)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Behavior Editor] Loaded VFS roots.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Behavior Editor] Failed to load VFS roots.");
        }

        Traverse(FsRoot);
    }

    private async void Traverse(FsEntry curEntry)
    {
        TaskLogs.AddLog($"{curEntry.Name} -- {curEntry.GetType()}");
        foreach (var entry in curEntry.Children)
        {
            Traverse(entry);
        }
    }

    private async Task<bool> LoadVfsRoots()
    {
        await Task.Delay(1000);

        FsRoot = new VirtualFileSystemFsEntry(Project, Project.FS, "Full Combined FS");

        return true;
    }
}
