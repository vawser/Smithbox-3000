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
}
