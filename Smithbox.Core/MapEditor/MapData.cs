using Smithbox.Core.Editor;
using Smithbox.Core.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.MapEditorNS;

public class MapData
{
    public Project Project;

    public MapBank PrimaryBank;

    public FileDictionary MapFiles;

    public MapData(Project projectOwner)
    {
        Project = projectOwner;

        PrimaryBank = new(this, "Primary");

        MapFiles = new();
        MapFiles.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == "msb").ToList();
    }
}
