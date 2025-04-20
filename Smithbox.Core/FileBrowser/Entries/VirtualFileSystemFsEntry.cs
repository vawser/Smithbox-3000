﻿using Andre.IO.VFS;
using Microsoft.Extensions.Logging;
using Smithbox.Core.Editor;
using Smithbox.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.FileBrowserNS.Entries;

public class VirtualFileSystemFsEntry : FsEntry
{
    private Project Project;
    public override bool IsInitialized => inner.IsInitialized;
    private string name;
    public override string Name => name;
    public override bool CanHaveChildren => true;
    public override bool CanView => false;
    private VirtualFileSystemDirectoryFsEntry inner;
    public override List<FsEntry> Children => inner?.Children ?? [];

    private VirtualFileSystem vfs;

    public VirtualFileSystemFsEntry(Project ownerProject, VirtualFileSystem vfs, string name)
    {
        Project = ownerProject;
        this.vfs = vfs;
        this.name = $"{name} ({vfs.GetType().Name})";
        inner = new VirtualFileSystemDirectoryFsEntry(Project, vfs, "", "");
    }

    internal override void Load(Project ownerProject)
    {
        inner.Load(ownerProject);
    }

    internal override void UnloadInner()
    {
        inner.UnloadInner();
    }
}

public class VirtualFileSystemDirectoryFsEntry : FsEntry
{
    private Project Project;

    private bool isInitialized = false;
    public override bool IsInitialized => isInitialized;
    private string name;
    public override string Name => name;
    public override bool CanHaveChildren => true;
    public override bool CanView => false;
    private List<FsEntry> children = [];
    public override List<FsEntry> Children => children;

    private VirtualFileSystem vfs;
    private string path;

    public VirtualFileSystemDirectoryFsEntry(Project ownerProject, VirtualFileSystem vfs, string parentPath, string name)
    {
        Project = ownerProject;
        this.name = name;
        this.vfs = vfs;
        path = $"{parentPath}/{name}";
    }

    internal override void Load(Project ownerProject)
    {
        var dir = vfs.GetDirectory(path);
        if (dir == null)
        {
            TaskLogs.AddVerboseLog($"[File Browser] Failed to load dir {path}", LogLevel.Warning);
            return;
        }

        foreach (var dirname in dir.EnumerateDirectoryNames())
        {
            children.Add(new VirtualFileSystemDirectoryFsEntry(ownerProject, vfs, path, dirname));
        }

        foreach (var filename in dir.EnumerateFileNames())
        {
            var child =
                TryGetFor(Project, filename, () => vfs.ReadFile($"{path}/{filename}").Value, vfs, $"{path}/{filename}")
                ?? new VirtualFileSystemFileFsEntry(ownerProject, filename);
            children.Add(child);
        }

        isInitialized = true;
    }

    internal override void UnloadInner()
    {
        children.ForEach(c => c.UnloadInner());
        children.Clear();
        isInitialized = false;
    }

}

/// <summary>
/// Represents a file in a VirtualFileSystem for which we have no bespoke FsEntry
/// </summary>
public class VirtualFileSystemFileFsEntry : FsEntry
{
    private Project Project;

    public override bool IsInitialized => true;
    private string name;
    public override string Name => name;
    public override bool CanHaveChildren => false;
    public override bool CanView => false;
    private static List<FsEntry> children = [];
    public override List<FsEntry> Children => children;

    public VirtualFileSystemFileFsEntry(Project ownerProject, string name)
    {
        Project = ownerProject;
        this.name = name;
    }

    internal override void Load(Project ownerProject) { }

    internal override void UnloadInner() { }

}
