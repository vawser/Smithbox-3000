using Andre.Formats;
using Andre.IO.VFS;
using Microsoft.Extensions.Logging;
using Smithbox.Core.Editor;
using Smithbox.Core.ParamEditorNS;
using Smithbox.Core.Resources;
using Smithbox.Core.Utils;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Smithbox.Core.ParamEditorNS;

/// <summary>
/// Big old file, but it handles three things:
/// 1. Loading
/// 2. Saving
/// 3. Upgrading
/// </summary>
public class ParamBank
{
    private ParamData DataParent;

    private ParamUpgrade Upgrader;

    /// <summary>
    /// This is the source file for the param data
    /// However, if it doesn't exist, Load will fallback to the Project.DataPath.
    /// </summary>
    private string SourcePath;

    public readonly HashSet<int> EMPTYSET = new();

    public Dictionary<string, Param> Params = new();

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

        Upgrader = new(parent);
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

    /// <summary>
    /// Load task for this Param bank
    /// </summary>
    public async Task<bool> Load(VirtualFileSystem fs, Dictionary<string, PARAMDEF> defs)
    {
        await Task.Delay(1000);

        var successfulLoad = false;

        switch (DataParent.Project.ProjectType)
        {
            case ProjectType.DES:
                successfulLoad = LoadParameters_DES(fs, defs); break;
            case ProjectType.DS1:
                successfulLoad = LoadParameters_DS1(fs, defs); break;
            case ProjectType.DS1R:
                successfulLoad = LoadParameters_DS1R(fs, defs); break;
            case ProjectType.DS2:
                successfulLoad = LoadParameters_DS2(fs, defs); break;
            case ProjectType.DS2S:
                successfulLoad = LoadParameters_DS2S(fs, defs); break;
            case ProjectType.DS3:
                successfulLoad = LoadParameters_DS3(fs, defs); break;
            case ProjectType.BB:
                successfulLoad = LoadParameters_BB(fs, defs); break;
            case ProjectType.SDT:
                successfulLoad = LoadParameters_SDT(fs, defs); break;
            case ProjectType.ER:
                successfulLoad = LoadParameters_ER(fs, defs); break;
            case ProjectType.AC6:
                successfulLoad = LoadParameters_AC6(fs, defs); break;
            case ProjectType.ERN:
                successfulLoad = LoadParameters_ERN(fs, defs); break;
            default: break;
        }

        return successfulLoad;
    }

    /// <summary>
    /// Demons' Souls
    /// </summary>
    private bool LoadParameters_DES(VirtualFileSystem fs, Dictionary<string, PARAMDEF> defs)
    {
        var successfulLoad = true;

        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;

        var paramPath = PathUtils.GetGameParam_DES(fs);

        if (!fs.FileExists(paramPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to find {paramPath}", LogLevel.Warning);
            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = fs.GetFile(paramPath).GetData();
                using var bnd = BND3.Read(data);
                FillParamBank(defs, bnd, ref Params, out ParamVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to load game param: {paramPath}", LogLevel.Warning, e);
                successfulLoad = false;
            }

            // Draw Params
            foreach (var f in fs.FsRoot.GetDirectory("param")?.GetDirectory("drawparam")?.EnumerateFileNames() ?? [])
            {
                if (f.EndsWith(".parambnd.dcx"))
                {
                    paramPath = $"param/drawparam/{f}";

                    try
                    {
                        var data = fs.GetFile(paramPath).GetData();
                        using var bnd = BND3.Read(data);
                        FillParamBank(defs, bnd, ref Params, out ParamVersion);
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to load draw param: {paramPath}", LogLevel.Warning, e);
                        successfulLoad = false;
                    }
                }
            }
        }

        return successfulLoad;
    }

    /// <summary>
    /// Dark Souls (PTDE)
    /// </summary>
    private bool LoadParameters_DS1(VirtualFileSystem fs, Dictionary<string, PARAMDEF> defs)
    {
        var successfulLoad = true;

        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;

        var paramPath = PathUtils.GetGameParam_DS1(fs);

        if (!fs.FileExists(paramPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to find {paramPath}", LogLevel.Warning);
            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = fs.GetFile(paramPath).GetData();
                using var bnd = BND3.Read(data);
                FillParamBank(defs, bnd, ref Params, out ParamVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to load game param: {paramPath}", LogLevel.Warning, e);
                successfulLoad = false;
            }

            // Draw Params
            foreach (var f in fs.FsRoot.GetDirectory("param")?.GetDirectory("drawparam")?.EnumerateFileNames() ?? [])
            {
                if (f.EndsWith(".parambnd.dcx"))
                {
                    paramPath = $"param/drawparam/{f}";

                    try
                    {
                        var data = fs.GetFile(paramPath).GetData();
                        using var bnd = BND3.Read(data);
                        FillParamBank(defs, bnd, ref Params, out ParamVersion);
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to load draw param: {paramPath}", LogLevel.Warning, e);
                        successfulLoad = false;
                    }
                }
            }
        }

        return successfulLoad;
    }

    /// <summary>
    /// Dark Souls (Remastered)
    /// </summary>
    private bool LoadParameters_DS1R(VirtualFileSystem fs, Dictionary<string, PARAMDEF> defs)
    {
        var successfulLoad = true;

        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;

        var paramPath = PathUtils.GetGameParam_DS1R(fs);

        if (!fs.FileExists(paramPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to find {paramPath}", LogLevel.Warning);
            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = fs.GetFile(paramPath).GetData();
                using var bnd = BND3.Read(data);
                FillParamBank(defs, bnd, ref Params, out ParamVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to load game param: {paramPath}", LogLevel.Warning, e);
                successfulLoad = false;
            }

            // Draw Params
            foreach (var f in fs.FsRoot.GetDirectory("param")?.GetDirectory("drawparam")?.EnumerateFileNames() ?? [])
            {
                if (f.EndsWith(".parambnd.dcx"))
                {
                    paramPath = $"param/drawparam/{f}";

                    try
                    {
                        var data = fs.GetFile(paramPath).GetData();
                        using var bnd = BND3.Read(data);
                        FillParamBank(defs, bnd, ref Params, out ParamVersion);
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to load draw param: {paramPath}", LogLevel.Warning, e);
                        successfulLoad = false;
                    }
                }
            }
        }

        return successfulLoad;
    }

    /// <summary>
    /// Dark Souls II
    /// </summary>
    private bool LoadParameters_DS2(VirtualFileSystem fs, Dictionary<string, PARAMDEF> defs)
    {
        var successfulLoad = true;

        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;

        var paramPath = PathUtils.GetGameParam_DS2(fs);
        var enemyPath = PathUtils.GetEnemyParam_DS2S(fs);

        if (!fs.FileExists(paramPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to load game param: {paramPath}", LogLevel.Warning);
            successfulLoad = false;
        }

        // Load loose params (prioritizing ones in mod folder)
        List<string> looseParams = PathUtils.GetLooseParamsInDir(fs, "");

        BND4 paramBnd = null;
        byte[] data = fs.GetFile(paramPath).GetData().ToArray();

        if (!BND4.Is(data))
        {
            try
            {
                paramBnd = SFUtil.DecryptDS2Regulation(data);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to load draw param: {paramPath}", LogLevel.Warning, e);
                successfulLoad = false;
            }
        }
        else
        {
            try
            {
                paramBnd = BND4.Read(data);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to load draw param: {paramPath}", LogLevel.Warning, e);
                successfulLoad = false;
            }
        }

        BinderFile bndfile = paramBnd.Files.Find(x => Path.GetFileName(x.Name) == "EnemyParam.param");

        if (bndfile != null)
        {
            EnemyParam = Param.Read(bndfile.Bytes);
        }

        // Otherwise the param is a loose param
        if (fs.FileExists(enemyPath))
        {
            var paramData = fs.GetFile(enemyPath).GetData();
            EnemyParam = Param.Read(paramData);
        }

        if (EnemyParam is { ParamType: not null })
        {
            try
            {
                PARAMDEF def = defs[EnemyParam.ParamType];
                EnemyParam.ApplyParamdef(def);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Could not apply ParamDef for {EnemyParam.ParamType}",
                    LogLevel.Warning, e);
                successfulLoad = false;
            }
        }

        FillParamBank(defs, paramBnd, ref Params, out ParamVersion);

        foreach (var p in looseParams)
        {
            var name = Path.GetFileNameWithoutExtension(p);
            var paramData = fs.GetFile(p).GetData();
            var lp = Param.Read(paramData);
            var fname = lp.ParamType;

            if (fname is "GENERATOR_DBG_LOCATION_PARAM")
                continue;

            try
            {
                if (CFG.Current.UseLooseParams)
                {
                    // Loose params: override params already loaded via regulation
                    PARAMDEF def = defs[lp.ParamType];
                    lp.ApplyParamdef(def);
                    Params[name] = lp;
                }
                else
                {
                    // Non-loose params: do not override params already loaded via regulation
                    if (!Params.ContainsKey(name))
                    {
                        PARAMDEF def = defs[lp.ParamType];
                        lp.ApplyParamdef(def);
                        Params.Add(name, lp);
                    }
                }
            }
            catch (Exception e)
            {
                var message = $"Could not apply ParamDef for {fname}";
                TaskLogs.AddLog(message, LogLevel.Warning, e);
                successfulLoad = false;
            }
        }

        paramBnd.Dispose();

        return successfulLoad;
    }

    /// <summary>
    /// Dark Souls II: Scholar of the First Sin
    /// </summary>
    private bool LoadParameters_DS2S(VirtualFileSystem fs, Dictionary<string, PARAMDEF> defs)
    {
        var successfulLoad = true;

        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;

        var paramPath = PathUtils.GetGameParam_DS2S(fs);
        var enemyPath = PathUtils.GetEnemyParam_DS2S(fs);

        if (!fs.FileExists(paramPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to load game param: {paramPath}", LogLevel.Warning);
            successfulLoad = false;
        }

        // Load loose params (prioritizing ones in mod folder)
        List<string> looseParams = PathUtils.GetLooseParamsInDir(fs, "");

        BND4 paramBnd = null;
        byte[] data = fs.GetFile(paramPath).GetData().ToArray();

        if (!BND4.Is(data))
        {
            try
            {
                paramBnd = SFUtil.DecryptDS2Regulation(data);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to load draw param: {paramPath}", LogLevel.Warning, e);
                successfulLoad = false;
            }
        }
        else
        {
            try
            {
                paramBnd = BND4.Read(data);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to load draw param: {paramPath}", LogLevel.Warning, e);
                successfulLoad = false;
            }
        }

        BinderFile bndfile = paramBnd.Files.Find(x => Path.GetFileName(x.Name) == "EnemyParam.param");

        if (bndfile != null)
        {
            EnemyParam = Param.Read(bndfile.Bytes);
        }

        // Otherwise the param is a loose param
        if (fs.FileExists(enemyPath))
        {
            var paramData = fs.GetFile(enemyPath).GetData();
            EnemyParam = Param.Read(paramData);
        }

        if (EnemyParam is { ParamType: not null })
        {
            try
            {
                PARAMDEF def = defs[EnemyParam.ParamType];
                EnemyParam.ApplyParamdef(def);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Could not apply ParamDef for {EnemyParam.ParamType}",
                    LogLevel.Warning, e);
                successfulLoad = false;
            }
        }

        FillParamBank(defs, paramBnd, ref Params, out ParamVersion);

        foreach (var p in looseParams)
        {
            var name = Path.GetFileNameWithoutExtension(p);
            var paramData = fs.GetFile(p).GetData();
            var lp = Param.Read(paramData);
            var fname = lp.ParamType;

            if (fname is "GENERATOR_DBG_LOCATION_PARAM")
                continue;

            try
            {
                if (CFG.Current.UseLooseParams)
                {
                    // Loose params: override params already loaded via regulation
                    PARAMDEF def = defs[lp.ParamType];
                    lp.ApplyParamdef(def);
                    Params[name] = lp;
                }
                else
                {
                    // Non-loose params: do not override params already loaded via regulation
                    if (!Params.ContainsKey(name))
                    {
                        PARAMDEF def = defs[lp.ParamType];
                        lp.ApplyParamdef(def);
                        Params.Add(name, lp);
                    }
                }
            }
            catch (Exception e)
            {
                var message = $"Could not apply ParamDef for {fname}";
                TaskLogs.AddLog(message, LogLevel.Warning, e);
                successfulLoad = false;
            }
        }

        paramBnd.Dispose();

        return successfulLoad;
    }

    /// <summary>
    /// Dark Souls III
    /// </summary>
    private bool LoadParameters_DS3(VirtualFileSystem fs, Dictionary<string, PARAMDEF> defs)
    {
        var successfulLoad = true;

        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;

        var looseFile = PathUtils.GetGameParamLoose_DS3(fs);
        var packedFile = PathUtils.GetGameParamPacked_DS3(fs);

        if (!fs.FileExists(packedFile))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to find {packedFile}", LogLevel.Warning);
            successfulLoad = false;
        }
        else
        {
            if (CFG.Current.UseLooseParams && fs.FileExists(looseFile))
            {
                try
                {
                    var data = fs.GetFile(looseFile).GetData();
                    using var bnd = BND4.Read(data);
                    FillParamBank(defs, bnd, ref Params, out ParamVersion);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"Failed to load game param: {looseFile}", LogLevel.Warning, e);
                    successfulLoad = false;
                }
            }
            else
            {
                try
                {
                    var data = fs.GetFile(packedFile).GetData().ToArray();
                    using var bnd = SFUtil.DecryptDS3Regulation(data);
                    FillParamBank(defs, bnd, ref Params, out ParamVersion);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"Failed to load game param: {packedFile}", LogLevel.Warning, e);
                    successfulLoad = false;
                }
            }
        }

        return successfulLoad;
    }

    /// <summary>
    /// Bloodborne
    /// </summary>
    private bool LoadParameters_BB(VirtualFileSystem fs, Dictionary<string, PARAMDEF> defs)
    {
        var successfulLoad = true;

        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;

        var paramPath = PathUtils.GetGameParam_BB(fs);

        if (!fs.FileExists(paramPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to find {paramPath}", LogLevel.Warning);
            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = fs.GetFile(paramPath).GetData();
                using var bnd = BND4.Read(data);
                FillParamBank(defs, bnd, ref Params, out ParamVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to load game param: {paramPath}", LogLevel.Warning, e);
                successfulLoad = false;
            }
        }

        return successfulLoad;
    }

    /// <summary>
    /// Sekiro: Shadows Die Twice
    /// </summary>
    private bool LoadParameters_SDT(VirtualFileSystem fs, Dictionary<string, PARAMDEF> defs)
    {
        var successfulLoad = true;

        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;

        var paramPath = PathUtils.GetGameParam_SDT(fs);

        if (!fs.FileExists(paramPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to find {paramPath}", LogLevel.Warning);
            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = fs.GetFile(paramPath).GetData();
                using var bnd = BND4.Read(data);
                FillParamBank(defs, bnd, ref Params, out ParamVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to load game param: {paramPath}", LogLevel.Warning, e);
                successfulLoad = false;
            }
        }

        return successfulLoad;
    }

    /// <summary>
    /// Elden Ring
    /// </summary>
    private bool LoadParameters_ER(VirtualFileSystem fs, Dictionary<string, PARAMDEF> defs)
    {
        var successfulLoad = true;

        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;

        var gameParamPath = PathUtils.GetGameParam_ER(fs);
        var systemParamPath = PathUtils.GetSystemParam_ER(fs);

        if (!fs.FileExists(gameParamPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to find {gameParamPath}", LogLevel.Warning);
            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = fs.GetFile(gameParamPath).GetData().ToArray();
                using BND4 bnd = SFUtil.DecryptERRegulation(data);
                FillParamBank(defs, bnd, ref Params, out ParamVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to load game param: {gameParamPath}", LogLevel.Warning,
                e.InnerException);
                successfulLoad = false;
            }
        }

        if (!fs.FileExists(systemParamPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to find {systemParamPath}", LogLevel.Warning);
            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = fs.GetFile(systemParamPath).GetData();
                using var bnd = BND4.Read(data);
                FillParamBank(defs, bnd, ref Params, out ParamVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to load game param: {systemParamPath}", LogLevel.Warning, e);
                successfulLoad = false;
            }
        }

        return successfulLoad;
    }

    /// <summary>
    /// Armored Core VI: Fires of Rubicon
    /// </summary>
    private bool LoadParameters_AC6(VirtualFileSystem fs, Dictionary<string, PARAMDEF> defs)
    {
        var successfulLoad = true;

        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;

        var gameParamPath = PathUtils.GetGameParam_AC6(fs);
        var systemParamPath = PathUtils.GetSystemParam_AC6(fs);
        var graphicsParamPath = PathUtils.GetGraphicsParam_AC6(fs);
        var eventParamPath = PathUtils.GetEventParam_AC6(fs);

        // Game Param
        if (!fs.FileExists(gameParamPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to find {gameParamPath}", LogLevel.Warning);
            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = fs.GetFile(gameParamPath).GetData().ToArray();
                using BND4 bnd = SFUtil.DecryptAC6Regulation(data);
                FillParamBank(defs, bnd, ref Params, out ParamVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to load game param: {gameParamPath}", LogLevel.Warning, e);
                successfulLoad = false;
            }
        }

        // System Param
        if (!fs.FileExists(systemParamPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to find {systemParamPath}", LogLevel.Warning);
            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = fs.GetFile(systemParamPath).GetData();
                using var bnd = BND4.Read(data);
                FillParamBank(defs, bnd, ref Params, out ParamVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to load game param: {systemParamPath}", LogLevel.Warning, e);
                successfulLoad = false;
            }
        }

        // Graphics Param
        if (!fs.FileExists(graphicsParamPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to find {graphicsParamPath}", LogLevel.Warning);
            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = fs.GetFile(graphicsParamPath).GetData();
                using var bnd = BND4.Read(data);
                FillParamBank(defs, bnd, ref Params, out ParamVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to load game param: {graphicsParamPath}", LogLevel.Warning, e);
                successfulLoad = false;
            }
        }

        // Event Param
        if (!fs.FileExists(eventParamPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to find {eventParamPath}", LogLevel.Warning);
            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = fs.GetFile(eventParamPath).GetData();
                using var bnd = BND4.Read(data);
                FillParamBank(defs, bnd, ref Params, out ParamVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to load game param: {eventParamPath}", LogLevel.Warning, e);
                successfulLoad = false;
            }
        }

        return successfulLoad;
    }

    /// <summary>
    /// Elden Ring: Nightreign
    /// </summary>
    private bool LoadParameters_ERN(VirtualFileSystem fs, Dictionary<string, PARAMDEF> defs)
    {
        var successfulLoad = true;

        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;

        var gameParamPath = PathUtils.GetGameParam_ERN(fs);
        var systemParamPath = PathUtils.GetSystemParam_ERN(fs);

        if (!fs.FileExists(gameParamPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to find {gameParamPath}", LogLevel.Warning);
            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = fs.GetFile(gameParamPath).GetData().ToArray();
                using BND4 bnd = SFUtil.DecryptNightreignRegulation(data);
                FillParamBank(defs, bnd, ref Params, out ParamVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to load game param: {gameParamPath}", LogLevel.Warning, e);
                successfulLoad = false;
            }
        }

        if (!fs.FileExists(systemParamPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to find {systemParamPath}", LogLevel.Warning);
            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = fs.GetFile(systemParamPath).GetData();
                using var bnd = BND4.Read(data);
                FillParamBank(defs, bnd, ref Params, out ParamVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to load game param: {systemParamPath}", LogLevel.Warning, e);
                successfulLoad = false;
            }
        }

        return successfulLoad;
    }

    private void FillParamBank(Dictionary<string, PARAMDEF> defs, IBinder parambnd, ref Dictionary<string, Param> paramBank, out ulong version,
        bool checkVersion = false)
    {
        var success = ulong.TryParse(parambnd.Version, out version);

        if (checkVersion && !success)
        {
            throw new Exception(@"Failed to get regulation version. Params might be corrupt.");
        }

        // Load every param in the regulation
        foreach (BinderFile f in parambnd.Files)
        {
            var paramName = Path.GetFileNameWithoutExtension(f.Name);

            if (!f.Name.ToUpper().EndsWith(".PARAM"))
            {
                continue;
            }

            if (paramBank.ContainsKey(paramName))
            {
                continue;
            }

            Param curParam;

            // AC6/SDT - Tentative ParamTypes
            if (DataParent.Project.ProjectType is ProjectType.AC6 or ProjectType.SDT)
            {
                _usedTentativeParamTypes = new Dictionary<string, string>();
                curParam = Param.ReadIgnoreCompression(f.Bytes);

                // Missing paramtype
                if (!string.IsNullOrEmpty(curParam.ParamType))
                {
                    if (!defs.ContainsKey(curParam.ParamType))
                    {
                        if (DataParent.TentativeParamTypes.TryGetValue(paramName, out var newParamType))
                        {
                            _usedTentativeParamTypes.Add(paramName, curParam.ParamType);
                            curParam.ParamType = newParamType;
                            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Couldn't find ParamDef for {paramName}, but tentative ParamType \"{newParamType}\" exists.", LogLevel.Debug);
                        }
                        else
                        {
                            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Couldn't find ParamDef for param {paramName} and no tentative ParamType exists.", LogLevel.Error);

                            continue;
                        }
                    }
                }
                else
                {
                    if (DataParent.TentativeParamTypes.TryGetValue(paramName, out var newParamType))
                    {
                        _usedTentativeParamTypes.Add(paramName, curParam.ParamType);

                        curParam.ParamType = newParamType;

                        TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Couldn't read ParamType for {paramName}, but tentative ParamType \"{newParamType}\" exists.", LogLevel.Debug);
                    }
                    else
                    {
                        TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Couldn't read ParamType for {paramName} and no tentative ParamType exists.", LogLevel.Error);

                        continue;
                    }
                }
            }
            // Normal
            else
            {
                curParam = Param.ReadIgnoreCompression(f.Bytes);

                if (!defs.ContainsKey(curParam.ParamType ?? ""))
                {
                    TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Couldn't find ParamDef for param {paramName} with ParamType \"{curParam.ParamType}\".", LogLevel.Warning);

                    continue;
                }
            }

            ApplyParamFixups(curParam);

            if (curParam.ParamType == null)
            {
                throw new Exception("Param type is unexpectedly null");
            }

            // Skip these for DS1 so the param load is not slowed down by the catching
            if (DataParent.Project.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
            {
                if (paramName is "m99_ToneCorrectBank" or "m99_ToneMapBank" or "default_ToneCorrectBank")
                {
                    TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Skipped this param: {paramName}");
                    continue;
                }
            }

            PARAMDEF def = defs[curParam.ParamType];

            try
            {
                curParam.ApplyParamdef(def, version);
                paramBank.Add(paramName, curParam);
            }
            catch (Exception e)
            {
                var name = f.Name.Split("\\").Last();
                var message = $"[{DataParent.Project.ProjectName}:Param Editor] Could not apply ParamDef for {name}";

                TaskLogs.AddLog(message, LogLevel.Warning, e);
            }
        }
    }

    private void ApplyParamFixups(Param p)
    {
        // Try to fixup Elden Ring ChrModelParam for ER 1.06 because many have been saving botched params and
        // it's an easy fixup
        if (DataParent.Project.ProjectType is ProjectType.ER && ParamVersion >= 10601000)
        {
            if (p.ParamType == "CHR_MODEL_PARAM_ST")
            {
                if (p.FixupERField(12, 16))
                    TaskLogs.AddLog($"CHR_MODEL_PARAM_ST fixed up.");
            }
        }

        // Add in the new data for these two params added in 1.12.1
        if (DataParent.Project.ProjectType is ProjectType.ER && ParamVersion >= 11210015)
        {
            if (p.ParamType == "GAME_SYSTEM_COMMON_PARAM_ST")
            {
                if (p.FixupERField(880, 1024))
                    TaskLogs.AddLog($"GAME_SYSTEM_COMMON_PARAM_ST fixed up.");
            }
            if (p.ParamType == "POSTURE_CONTROL_PARAM_WEP_RIGHT_ST")
            {
                if (p.FixupERField(112, 144))
                    TaskLogs.AddLog($"POSTURE_CONTROL_PARAM_WEP_RIGHT_ST fixed up.");
            }
            if (p.ParamType == "SIGN_PUDDLE_PARAM_ST")
            {
                if (p.FixupERField(32, 48))
                    TaskLogs.AddLog($"SIGN_PUDDLE_PARAM_ST fixed up.");
            }
        }
    }

    /// <summary>
    /// Save task for this Param bank
    /// </summary>
    public async Task<bool> Save()
    {
        await Task.Delay(1000);

        var successfulSave = false;

        switch (DataParent.Project.ProjectType)
        {
            case ProjectType.DES:
                successfulSave = SaveParameters_DES(); break;
            case ProjectType.DS1:
                successfulSave = SaveParameters_DS1(); break;
            case ProjectType.DS1R:
                successfulSave = SaveParameters_DS1R(); break;
            case ProjectType.DS2:
                successfulSave = SaveParameters_DS2(); break;
            case ProjectType.DS2S:
                successfulSave = SaveParameters_DS2S(); break;
            case ProjectType.DS3:
                successfulSave = SaveParameters_DS3(); break;
            case ProjectType.BB:
                successfulSave = SaveParameters_BB(); break;
            case ProjectType.SDT:
                successfulSave = SaveParameters_SDT(); break;
            case ProjectType.ER:
                successfulSave = SaveParameters_ER(); break;
            case ProjectType.AC6:
                successfulSave = SaveParameters_AC6(); break;
            case ProjectType.ERN:
                successfulSave = SaveParameters_ERN(); break;
            default: break;
        }

        return successfulSave;
    }

    /// <summary>
    /// Demons' Souls
    /// </summary>
    private bool SaveParameters_DES()
    {
        var successfulSave = true;

        var fs = DataParent.Project.FileSystem;
        var toFs = VfsUtils.GetFSForWrites(DataParent.Project);

        var paramPath = PathUtils.GetGameParam_DES(fs);

        if (!fs.FileExists(paramPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Cannot locate param files. Save failed.",
                LogLevel.Error);
            return false;
        }

        var data = fs.GetFile(paramPath).GetData().ToArray();

        using var paramBnd = BND3.Read(fs.GetFile(paramPath).GetData());

        // Replace params with edited ones
        foreach (BinderFile p in paramBnd.Files)
        {
            if (Params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
            {
                p.Bytes = Params[Path.GetFileNameWithoutExtension(p.Name)].Write();
            }
        }

        // Write all gameparam variations since we don't know which one the the game will use.
        // Compressed
        paramBnd.Compression = DCX.Type.DCX_EDGE;
        var naParamPath = @"param\gameparam\gameparamna.parambnd.dcx";

        if (fs.FileExists(naParamPath))
        {
            VfsUtils.WriteWithBackup(DataParent.Project, fs, toFs, naParamPath, paramBnd);
        }

        VfsUtils.WriteWithBackup(DataParent.Project, fs, toFs, @"param\gameparam\gameparam.parambnd.dcx", paramBnd);

        // Decompressed
        paramBnd.Compression = DCX.Type.None;
        naParamPath = @"param\gameparam\gameparamna.parambnd";
        if (fs.FileExists(naParamPath))
        {
            VfsUtils.WriteWithBackup(DataParent.Project, fs, toFs, naParamPath, paramBnd);
        }

        VfsUtils.WriteWithBackup(DataParent.Project, fs, toFs, @"param\gameparam\gameparam.parambnd", paramBnd);

        // Drawparam
        List<string> drawParambndPaths = new();
        if (fs.DirectoryExists(@"param\drawparam"))
        {
            foreach (var bnd in fs.GetFileNamesWithExtensions($@"param\drawparam", ".parambnd", ".parambnd.dcx"))
            {
                drawParambndPaths.Add(bnd);
            }

            foreach (var bnd in drawParambndPaths)
            {
                using var drawParamBnd = BND3.Read(fs.GetFile(bnd).GetData());

                foreach (BinderFile p in drawParamBnd.Files)
                {
                    if (Params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
                    {
                        p.Bytes = Params[Path.GetFileNameWithoutExtension(p.Name)].Write();
                    }
                }

                VfsUtils.WriteWithBackup(DataParent.Project, fs, toFs, @$"param\drawparam\{Path.GetFileName(bnd)}", drawParamBnd);
            }
        }

        return successfulSave;
    }

    /// <summary>
    /// Dark Souls (PTDE)
    /// </summary>
    private bool SaveParameters_DS1()
    {
        var successfulSave = true;

        var fs = DataParent.Project.FileSystem;
        var toFs = VfsUtils.GetFSForWrites(DataParent.Project);

        string param = @"param\GameParam\GameParam.parambnd";
        if (!fs.FileExists(param))
        {
            param += ".dcx";
            if (!fs.FileExists(param))
            {
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Cannot locate param files. Save failed.", LogLevel.Error);
                return false;
            }
        }

        using var paramBnd = BND3.Read(fs.GetFile(param).GetData());

        foreach (BinderFile p in paramBnd.Files)
        {
            if (Params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
            {
                p.Bytes = Params[Path.GetFileNameWithoutExtension(p.Name)].Write();
            }
        }
        VfsUtils.WriteWithBackup(DataParent.Project, fs, toFs, @"param\GameParam\GameParam.parambnd", paramBnd);

        if (fs.DirectoryExists($@"param\DrawParam"))
        {
            foreach (var bnd in fs.GetFileNamesWithExtensions(@"param\DrawParam", ".parambnd"))
            {
                using var drawParamBnd = BND3.Read(fs.GetFile(bnd).GetData());
                foreach (BinderFile p in drawParamBnd.Files)
                {
                    if (Params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
                    {
                        p.Bytes = Params[Path.GetFileNameWithoutExtension(p.Name)].Write();
                    }
                }

                VfsUtils.WriteWithBackup(DataParent.Project, fs, toFs, @$"param\DrawParam\{Path.GetFileName(bnd)}", drawParamBnd);
            }
        }

        return successfulSave;
    }

    /// <summary>
    /// Dark Souls (Remastered)
    /// </summary>
    private bool SaveParameters_DS1R()
    {
        var successfulSave = true;

        var fs = DataParent.Project.FileSystem;
        var toFs = VfsUtils.GetFSForWrites(DataParent.Project); ;
        string param = @"param\GameParam\GameParam.parambnd.dcx";

        if (!fs.FileExists(param))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Cannot locate param files. Save failed.", LogLevel.Error);
            return false;
        }

        using var paramBnd = BND3.Read(fs.GetFile(param).GetData());
        // Replace params with edited ones
        foreach (BinderFile p in paramBnd.Files)
        {
            if (Params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
            {
                p.Bytes = Params[Path.GetFileNameWithoutExtension(p.Name)].Write();
            }
        }
        VfsUtils.WriteWithBackup(DataParent.Project, fs, toFs, @"param\GameParam\GameParam.parambnd.dcx", paramBnd);

        //DrawParam
        if (fs.DirectoryExists($@"param\DrawParam"))
        {
            foreach (var bnd in fs.GetFileNamesWithExtensions($@"param\DrawParam", ".parambnd.dcx"))
            {
                using var drawParamBnd = BND3.Read(fs.GetFile(bnd).GetData());
                foreach (BinderFile p in drawParamBnd.Files)
                {
                    if (Params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
                    {
                        p.Bytes = Params[Path.GetFileNameWithoutExtension(p.Name)].Write();
                    }
                }

                VfsUtils.WriteWithBackup(DataParent.Project, fs, toFs, @$"param\DrawParam\{Path.GetFileName(bnd)}", drawParamBnd);
            }
        }

        return successfulSave;
    }

    /// <summary>
    /// Dark Souls II
    /// </summary>
    private bool SaveParameters_DS2()
    {
        // No need to duplicate code here
        var successfulSave = SaveParameters_DS2S();

        return successfulSave;
    }

    /// <summary>
    /// Dark Souls II: Scholar of the First Sin
    /// </summary>
    private bool SaveParameters_DS2S()
    {
        var successfulSave = true;

        var fs = DataParent.Project.FileSystem;
        var toFs = VfsUtils.GetFSForWrites(DataParent.Project); ;
        string param = @"enc_regulation.bnd.dcx";

        if (!fs.FileExists(param))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Cannot locate param files. Save failed.", LogLevel.Error);

            return false;
        }

        BND4 paramBnd;
        var data = fs.GetFile(param).GetData().ToArray();
        if (!BND4.Is(data))
        {
            // Decrypt the file
            paramBnd = SFUtil.DecryptDS2Regulation(data);
            // Since the file is encrypted, check for a backup. If it has none, then make one and write a decrypted one.
            if (!toFs.FileExists($"{param}.bak"))
            {
                toFs.WriteFile($"{param}.bak", data);
            }
            toFs.WriteFile(param, paramBnd.Write());
        }
        else
        {
            paramBnd = BND4.Read(data);
        }

        if (!CFG.Current.UseLooseParams)
        {
            // Save params non-loosely: Replace params regulation and write remaining params loosely.
            if (paramBnd.Files.Find(e => e.Name.EndsWith(".param")) == null)
            {
                if (CFG.Current.RepackLooseDS2Params)
                {
                    paramBnd.Dispose();
                    param = $@"enc_regulation.bnd.dcx";
                    data = DataParent.Project.VanillaFS.GetFile(param).GetData().ToArray();

                    if (!BND4.Is(data))
                    {
                        // Decrypt the file.
                        paramBnd = SFUtil.DecryptDS2Regulation(data);

                        // Since the file is encrypted, check for a backup. If it has none, then make one and write a decrypted one.
                        if (!toFs.FileExists($@"{param}.bak"))
                        {
                            toFs.WriteFile($"{param}.bak", data);
                            toFs.WriteFile(param, paramBnd.Write());
                        }
                    }
                    else
                        paramBnd = BND4.Read(data);
                }
            }

            try
            {
                // Strip and store row names before saving, as too many row names can cause DS2 to crash.
                RowNameStrip();

                foreach (KeyValuePair<string, Param> p in Params)
                {
                    BinderFile bnd = paramBnd.Files.Find(e => Path.GetFileNameWithoutExtension(e.Name) == p.Key);

                    if (bnd != null)
                    {
                        // Regulation contains this param, overwrite it.
                        bnd.Bytes = p.Value.Write();
                    }
                    else
                    {
                        // Regulation does not contain this param, write param loosely.
                        VfsUtils.WriteWithBackup(DataParent.Project, fs, toFs, $@"Param\{p.Key}.param", p.Value);
                    }
                }
            }
            catch
            {
                RowNameRestore();
                throw;
            }

            RowNameRestore();
        }
        else
        {
            // Save params loosely: Strip params from regulation and write all params loosely.

            List<BinderFile> newFiles = new();
            foreach (BinderFile p in paramBnd.Files)
            {
                // Strip params from regulation bnd
                if (!p.Name.ToUpper().Contains(".PARAM"))
                {
                    newFiles.Add(p);
                }
            }

            paramBnd.Files = newFiles;

            try
            {
                // Strip and store row names before saving, as too many row names can cause DS2 to crash.
                RowNameStrip();

                // Write params to loose files.
                foreach (KeyValuePair<string, Param> p in Params)
                {
                    VfsUtils.WriteWithBackup(DataParent.Project, fs, toFs, $@"Param\{p.Key}.param", p.Value);
                }
            }
            catch
            {
                RowNameRestore();
                throw;
            }

            RowNameRestore();
        }

        VfsUtils.WriteWithBackup(DataParent.Project, fs, toFs, @"enc_regulation.bnd.dcx", paramBnd);
        paramBnd.Dispose();

        return successfulSave;
    }

    /// <summary>
    /// Dark Souls III
    /// </summary>
    private bool SaveParameters_DS3()
    {
        var successfulSave = true;

        return successfulSave;
    }

    /// <summary>
    /// Bloodborne
    /// </summary>
    private bool SaveParameters_BB()
    {
        var successfulSave = true;

        return successfulSave;
    }

    /// <summary>
    /// Sekiro: Shadows Die Twice
    /// </summary>
    private bool SaveParameters_SDT()
    {
        var successfulSave = true;

        return successfulSave;
    }

    /// <summary>
    /// Elden Ring
    /// </summary>
    private bool SaveParameters_ER()
    {
        var successfulSave = true;

        return successfulSave;
    }

    /// <summary>
    /// Armored Core VI: Fires of Rubicon
    /// </summary>
    private bool SaveParameters_AC6()
    {
        var successfulSave = true;

        return successfulSave;
    }

    /// <summary>
    /// Elden Ring: Nightreign
    /// </summary>
    private bool SaveParameters_ERN()
    {
        var successfulSave = true;

        return successfulSave;
    }

    /// <summary>
    /// Upgrade task for this Param bank
    /// </summary>
    public async Task<bool> Upgrade()
    {
        await Task.Delay(1000);

        var successfulUpgrade = true;

        switch (DataParent.Project.ProjectType)
        {
            case ProjectType.ER:
                successfulUpgrade = UpgradeParameters_ER(); break;
            case ProjectType.AC6:
                successfulUpgrade = UpgradeParameters_AC6(); break;
            case ProjectType.ERN:
                successfulUpgrade = UpgradeParameters_ERN(); break;
            default: break;
        }

        return successfulUpgrade;
    }

    /// <summary>
    /// Elden Ring
    /// </summary>
    private bool UpgradeParameters_ER()
    {
        var successfulUpgrade = true;

        return successfulUpgrade;
    }

    /// <summary>
    /// Armored Core VI: Fires of Rubicon
    /// </summary>
    private bool UpgradeParameters_AC6()
    {
        var successfulUpgrade = true;

        return successfulUpgrade;
    }

    /// <summary>
    /// Elden Ring: Nightreign
    /// </summary>
    private bool UpgradeParameters_ERN()
    {
        var successfulUpgrade = true;

        return successfulUpgrade;
    }

    /// <summary>
    /// Strip and store the row names for this param bank
    /// </summary>
    public void RowNameStrip()
    {
        var store = new RowNameStore();
        store.Params = new();

        foreach (KeyValuePair<string, Param> p in Params)
        {
            var paramEntry = new RowNameParam();
            paramEntry.Name = p.Key;
            paramEntry.Entries = new();

            for (int i = 0; i < p.Value.Rows.Count; i++)
            {
                var row = p.Value.Rows[i];

                // Store
                var rowEntry = new RowNameEntry();

                rowEntry.Index = i;
                rowEntry.ID = row.ID;
                rowEntry.Name = row.Name;

                paramEntry.Entries.Add(rowEntry);

                // Strip
                p.Value.Rows[i].Name = "";
            }

            store.Params.Add(paramEntry);
        }

        var folder = FolderUtils.GetLocalProjectFolder(DataParent.Project);

        if(!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var file = Path.Combine(folder, "Stripped Row Names.json");

        var json = JsonSerializer.Serialize(store, SmithboxSerializerContext.Default.RowNameStore);

        File.WriteAllText(file, json);

        TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Stripped row names and stored them in {file}");
    }

    public void RowNameRestore()
    {
        RowNameStore store = null;

        var folder = FolderUtils.GetLocalProjectFolder(DataParent.Project);
        var file = Path.Combine(folder, "Stripped Row Names.json");

        if (!File.Exists(file))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to located {file} for row name restore.", LogLevel.Error);
        }
        else
        {
            try
            {
                var filestring = File.ReadAllText(file);
                var options = new JsonSerializerOptions();
                store = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.RowNameStore);

                if (store == null)
                {
                    throw new Exception($"[{DataParent.Project.ProjectName}:Param Editor] JsonConvert returned null during RowNameRestore.");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Failed to load {file} for row name restore.", LogLevel.Error);
            }
        }

        // Only proceed if we have row names to work with
        if(store != null)
        {
            var storeDict = store.Params.ToDictionary(e => e.Name);

            foreach (KeyValuePair<string, Param> p in Params)
            {
                var rowNames = storeDict[p.Key];
                var rowNameDict = rowNames.Entries.ToDictionary(e => e.Index);

                for (var i = 0; i < p.Value.Rows.Count; i++)
                {
                    if(CFG.Current.UseIndexMatchForRowNameRestore)
                    {
                        p.Value.Rows[i].Name = rowNameDict[i].Name;
                    }
                    else
                    {
                        // ID may not be unique, so we will manually loop here
                        foreach(var entry in rowNames.Entries)
                        {
                            if(entry.ID == p.Value.Rows[i].ID)
                            {
                                p.Value.Rows[i].Name = entry.Name;
                            }
                        }
                    }
                }
            }
        }


        TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor] Restored row names from {file}");
    }
}
