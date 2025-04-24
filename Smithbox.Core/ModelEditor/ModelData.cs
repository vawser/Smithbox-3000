using Smithbox.Core.BehaviorEditorNS;
using Smithbox.Core.Editor;
using Smithbox.Core.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.ModelEditorNS;

public class ModelData
{
    public Project Project;

    public ModelBank PrimaryBank;

    public FileDictionary CharacterPartFiles;

    public FileDictionary ObjectPartFiles;

    public FileDictionary EquipPartFiles;

    public FileDictionary MapPartFiles;

    /// <summary>
    /// Holds the current user-defined file path (e.g. a loose FLVER)
    /// </summary>
    public string CustomFile;

    public ModelData(Project projectOwner)
    {
        Project = projectOwner;

        PrimaryBank = new(this, "Primary");

        // Characters
        var chrPartFilter = "chrbnd";

        CharacterPartFiles = new();
        CharacterPartFiles.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == chrPartFilter).ToList();

        // Objects/Assets
        var objectPartFilter = "objbnd";

        if (Project.ProjectType is ProjectType.ER or ProjectType.AC6)
            objectPartFilter = "geombnd";

        ObjectPartFiles = new();
        ObjectPartFiles.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == objectPartFilter).ToList();

        // Equip Parts
        var equipPartFilter = "partsbnd";

        EquipPartFiles = new();
        EquipPartFiles.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == equipPartFilter).ToList();

        // Map Parts
        var mapPartFilter = "mapbnd";

        MapPartFiles = new();
        MapPartFiles.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == mapPartFilter).ToList();
    }
}
