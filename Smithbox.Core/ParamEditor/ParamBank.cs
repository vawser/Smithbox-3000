using Andre.Formats;
using Smithbox.Core.Editor;
using Smithbox.Core.ParamEditorNS;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.ParamEditorNS;

public class ParamBank
{
    private ParamData DataParent;

    /// <summary>
    /// This is the source file for the param data
    /// However, if it doesn't exist, Load will fallback to the Project.DataPath.
    /// </summary>
    private string SourcePath;

    public readonly HashSet<int> EMPTYSET = new();

    public Dictionary<string, Param> Params;

    /// <summary>
    /// Special-case param
    /// </summary>
    public Param EnemyParam;

    public bool IsParamBankLoaded { get; private set; }

    public ulong ParamVersion;

    /// <summary>
    /// Mapping between param and the stripped row name sets
    /// </summary>
    public Dictionary<string, List<string>> StoredStrippedRowNames;

    /// <summary>
    /// Dictionary of param file names that were given a tentative ParamType, and the original ParamType it had.
    /// Used to later restore original ParamType on write (if possible).
    /// </summary>
    private Dictionary<string, string> _usedTentativeParamTypes;

    public ParamBank(ParamData parent, string sourcePath)
    {
        DataParent = parent;
        SourcePath = sourcePath;
    }

    public bool PendingUpgrade;

    public readonly List<ProjectType> AllowedParamUpgrade = new List<ProjectType>()
    {
        ProjectType.ER,
        ProjectType.AC6
    };

    /// <summary>
    /// Map related params for DS2
    /// </summary>
    public readonly List<string> DS2MapParamlist = new()
    {
        "demopointlight",
        "demospotlight",
        "eventlocation",
        "eventparam",
        "GeneralLocationEventParam",
        "generatorparam",
        "generatorregistparam",
        "generatorlocation",
        "generatordbglocation",
        "hitgroupparam",
        "intrudepointparam",
        "mapobjectinstanceparam",
        "maptargetdirparam",
        "npctalkparam",
        "treasureboxparam"
    };

    public async Task<bool> Load(Dictionary<string, PARAMDEF> defs)
    {
        await Task.Delay(1000);

        switch (DataParent.Project.ProjectType)
        {
            case ProjectType.DES:  LoadParameters_DES(defs); break;
            case ProjectType.DS1:  LoadParameters_DS1(defs); break;
            case ProjectType.DS1R: LoadParameters_DS1R(defs); break;
            case ProjectType.DS2:  LoadParameters_DS2(defs); break;
            case ProjectType.DS2S: LoadParameters_DS2S(defs); break;
            case ProjectType.DS3:  LoadParameters_DS3(defs); break;
            case ProjectType.BB:   LoadParameters_BB(defs); break;
            case ProjectType.SDT:  LoadParameters_SDT(defs); break;
            case ProjectType.ER:   LoadParameters_ER(defs); break;
            case ProjectType.AC6:  LoadParameters_AC6(defs); break;
            case ProjectType.ERN:  LoadParameters_ERN(defs); break;
            default: break;
        }


        return true;
    }

    /// <summary>
    /// Demons' Souls
    /// </summary>
    private void LoadParameters_DES(Dictionary<string, PARAMDEF> defs)
    {
        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;


    }

    /// <summary>
    /// Dark Souls (PTDE)
    /// </summary>
    private void LoadParameters_DS1(Dictionary<string, PARAMDEF> defs)
    {
        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;


    }

    /// <summary>
    /// Dark Souls (Remastered
    /// </summary>
    private void LoadParameters_DS1R(Dictionary<string, PARAMDEF> defs)
    {
        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;


    }

    /// <summary>
    /// Dark Souls II
    /// </summary>
    private void LoadParameters_DS2(Dictionary<string, PARAMDEF> defs)
    {
        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;


    }

    /// <summary>
    /// Dark Souls II: Scholar of the First Sin
    /// </summary>
    private void LoadParameters_DS2S(Dictionary<string, PARAMDEF> defs)
    {
        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;


    }

    /// <summary>
    /// Dark Souls III
    /// </summary>
    private void LoadParameters_DS3(Dictionary<string, PARAMDEF> defs)
    {
        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;


    }

    /// <summary>
    /// Bloodborne
    /// </summary>
    private void LoadParameters_BB(Dictionary<string, PARAMDEF> defs)
    {
        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;


    }

    /// <summary>
    /// Sekiro: Shadows Die Twice
    /// </summary>
    private void LoadParameters_SDT(Dictionary<string, PARAMDEF> defs)
    {
        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;


    }

    /// <summary>
    /// Elden Ring
    /// </summary>
    private void LoadParameters_ER(Dictionary<string, PARAMDEF> defs)
    {
        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;


    }

    /// <summary>
    /// Armored Core VI: Fires of Rubicon
    /// </summary>
    private void LoadParameters_AC6(Dictionary<string, PARAMDEF> defs)
    {
        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;


    }

    /// <summary>
    /// Elden Ring: Nightreign
    /// </summary>
    private void LoadParameters_ERN(Dictionary<string, PARAMDEF> defs)
    {
        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;


    }
}
