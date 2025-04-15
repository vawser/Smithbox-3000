using Andre.Formats;
using Andre.IO.VFS;
using HKLib.hk2018.castTest;
using Microsoft.Extensions.Logging;
using Silk.NET.SDL;
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

    private string BankName = "Undefined";

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

    // Difference cache for primary bank
    private Dictionary<string, HashSet<int>> _primaryDiffCache;

    // Difference cache for vanilla bank
    private Dictionary<string, HashSet<int>> _vanillaDiffCache;

    public IReadOnlyDictionary<string, HashSet<int>> VanillaDiffCache
    {
        get
        {
            if (!IsParamBankLoaded)
            {
                return null;
            }
            else
            {
                if (DataParent.VanillaBank == this)
                    return null;
            }

            return _vanillaDiffCache;
        }
    }

    public IReadOnlyDictionary<string, HashSet<int>> PrimaryDiffCache
    {
        get
        {
            if (!IsParamBankLoaded)
            {
                return null;
            }
            else
            {
                if (DataParent.PrimaryBank == this)
                    return null;
            }
            return _primaryDiffCache;
        }
    }

    public ParamBank(ParamData parent, string sourcePath, string bankName)
    {
        DataParent = parent;
        SourcePath = sourcePath;
        BankName = bankName;
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

        IsParamBankLoaded = true;

        return successfulLoad;
    }

    /// <summary>
    /// Demons' Souls
    /// </summary>
    private bool LoadParameters_DES(VirtualFileSystem fs, Dictionary<string, PARAMDEF> defs)
    {
        var successfulLoad = true;

        var paramPath = PathUtils.GetGameParam_DES(fs);

        if (!fs.FileExists(paramPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to find {paramPath}", LogLevel.Warning);
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
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to load game param: {paramPath}", LogLevel.Warning, e);
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
                        TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to load draw param: {paramPath}", LogLevel.Warning, e);
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

        var paramPath = PathUtils.GetGameParam_DS1(fs);

        if (!fs.FileExists(paramPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to find {paramPath}", LogLevel.Warning);
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
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to load game param: {paramPath}", LogLevel.Warning, e);
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
                        TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to load draw param: {paramPath}", LogLevel.Warning, e);
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

        var paramPath = $@"param\GameParam\GameParam.parambnd.dcx";

        if (!fs.FileExists(paramPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to find {paramPath}", LogLevel.Warning);
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
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to load game param: {paramPath}", LogLevel.Warning, e);
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
                        TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to load draw param: {paramPath}", LogLevel.Warning, e);
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

        var paramPath = $@"enc_regulation.bnd.dcx";
        var enemyPath = $@"Param\\EnemyParam.param";

        if (!fs.FileExists(paramPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to load game param: {paramPath}", LogLevel.Warning);
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
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to load draw param: {paramPath}", LogLevel.Warning, e);
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
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to load draw param: {paramPath}", LogLevel.Warning, e);
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
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Could not apply ParamDef for {EnemyParam.ParamType}",
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

        var paramPath = $@"enc_regulation.bnd.dcx";
        var enemyPath = $@"Param\\EnemyParam.param";

        if (!fs.FileExists(paramPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to load game param: {paramPath}", LogLevel.Warning);
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
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to load draw param: {paramPath}", LogLevel.Warning, e);
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
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to load draw param: {paramPath}", LogLevel.Warning, e);
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
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Could not apply ParamDef for {EnemyParam.ParamType}",
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

        var looseFile = $@"param\\gameparam\\gameparam_dlc2.parambnd.dcx";
        var packedFile = $@"Data0.bdt";

        if (!fs.FileExists(packedFile))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to find {packedFile}", LogLevel.Warning);
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

        var paramPath = $@"param\gameparam\gameparam.parambnd.dcx";

        if (!fs.FileExists(paramPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to find {paramPath}", LogLevel.Warning);
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
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to load game param: {paramPath}", LogLevel.Warning, e);
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

        var paramPath = $@"param\gameparam\gameparam.parambnd.dcx";

        if (!fs.FileExists(paramPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to find {paramPath}", LogLevel.Warning);
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
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to load game param: {paramPath}", LogLevel.Warning, e);
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

        var gameParamPath = $@"regulation.bin";
        var systemParamPath = $@"param\systemparam\systemparam.parambnd.dcx";

        if (!fs.FileExists(gameParamPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to find {gameParamPath}", LogLevel.Warning);
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
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to load game param: {gameParamPath}", LogLevel.Warning,
                e.InnerException);
                successfulLoad = false;
            }
        }

        if (!fs.FileExists(systemParamPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to find {systemParamPath}", LogLevel.Warning);
            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = fs.GetFile(systemParamPath).GetData();
                using var bnd = BND4.Read(data);
                FillParamBank(defs, bnd, ref Params, out _);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to load game param: {systemParamPath}", LogLevel.Warning, e);
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

        var gameParamPath = $@"regulation.bin";
        var systemParamPath = $@"param\systemparam\systemparam.parambnd.dcx";
        var graphicsParamPath = $@"param\graphicsconfig\graphicsconfig.parambnd.dcx";
        var eventParamPath = $@"param\eventparam\eventparam.parambnd.dcx";

        // Game Param
        if (!fs.FileExists(gameParamPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to find {gameParamPath}", LogLevel.Warning);
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
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to load game param: {gameParamPath}", LogLevel.Warning, e);
                successfulLoad = false;
            }
        }

        // System Param
        if (!fs.FileExists(systemParamPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to find {systemParamPath}", LogLevel.Warning);
            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = fs.GetFile(systemParamPath).GetData();
                using var bnd = BND4.Read(data);
                FillParamBank(defs, bnd, ref Params, out _);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to load game param: {systemParamPath}", LogLevel.Warning, e);
                successfulLoad = false;
            }
        }

        // Graphics Param
        if (!fs.FileExists(graphicsParamPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to find {graphicsParamPath}", LogLevel.Warning);
            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = fs.GetFile(graphicsParamPath).GetData();
                using var bnd = BND4.Read(data);
                FillParamBank(defs, bnd, ref Params, out _);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to load game param: {graphicsParamPath}", LogLevel.Warning, e);
                successfulLoad = false;
            }
        }

        // Event Param
        if (!fs.FileExists(eventParamPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to find {eventParamPath}", LogLevel.Warning);
            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = fs.GetFile(eventParamPath).GetData();
                using var bnd = BND4.Read(data);
                FillParamBank(defs, bnd, ref Params, out _);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to load game param: {eventParamPath}", LogLevel.Warning, e);
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

        var gameParamPath = $@"regulation.bin";
        var systemParamPath = $@"param\systemparam\systemparam.parambnd.dcx";

        if (!fs.FileExists(gameParamPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to find {gameParamPath}", LogLevel.Warning);
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
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to load game param: {gameParamPath}", LogLevel.Warning, e);
                successfulLoad = false;
            }
        }

        if (!fs.FileExists(systemParamPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to find {systemParamPath}", LogLevel.Warning);
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
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to load game param: {systemParamPath}", LogLevel.Warning, e);
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
                    if (!defs.ContainsKey(curParam.ParamType) || DataParent.ParamTypeInfo.Exceptions.Contains(paramName))
                    {
                        if (DataParent.ParamTypeInfo.Mapping.TryGetValue(paramName, out var newParamType) )
                        {
                            _usedTentativeParamTypes.Add(paramName, curParam.ParamType);
                            curParam.ParamType = newParamType;
                            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Couldn't find ParamDef for {paramName}, but tentative ParamType \"{newParamType}\" exists.", LogLevel.Debug);
                        }
                        else
                        {
                            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Couldn't find ParamDef for param {paramName} and no tentative ParamType exists.", LogLevel.Error);

                            continue;
                        }
                    }
                }
                else
                {
                    if (DataParent.ParamTypeInfo.Mapping.TryGetValue(paramName, out var newParamType))
                    {
                        _usedTentativeParamTypes.Add(paramName, curParam.ParamType);

                        curParam.ParamType = newParamType;

                        TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Couldn't read ParamType for {paramName}, but tentative ParamType \"{newParamType}\" exists.", LogLevel.Debug);
                    }
                    else
                    {
                        TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Couldn't read ParamType for {paramName} and no tentative ParamType exists.", LogLevel.Error);

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
                    TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Couldn't find ParamDef for param {paramName} with ParamType \"{curParam.ParamType}\".", LogLevel.Warning);

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
                    TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Skipped this param: {paramName}");
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
                var message = $"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Could not apply ParamDef for {name}";

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

        var fs = DataParent.Project.FS;
        var toFs = VfsUtils.GetFilesystemForWrite(DataParent.Project);

        var paramPath = PathUtils.GetGameParam_DES(fs);

        if (!fs.FileExists(paramPath))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Cannot locate param files. Save failed.",
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

        var fs = DataParent.Project.FS;
        var toFs = VfsUtils.GetFilesystemForWrite(DataParent.Project);

        string param = @"param\GameParam\GameParam.parambnd";
        if (!fs.FileExists(param))
        {
            param += ".dcx";
            if (!fs.FileExists(param))
            {
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Cannot locate param files. Save failed.", LogLevel.Error);
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

        var fs = DataParent.Project.FS;
        var toFs = VfsUtils.GetFilesystemForWrite(DataParent.Project); ;
        string param = @"param\GameParam\GameParam.parambnd.dcx";

        if (!fs.FileExists(param))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Cannot locate param files. Save failed.", LogLevel.Error);
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

        var fs = DataParent.Project.FS;
        var toFs = VfsUtils.GetFilesystemForWrite(DataParent.Project); ;
        string param = @"enc_regulation.bnd.dcx";

        if (!fs.FileExists(param))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Cannot locate param files. Save failed.", LogLevel.Error);

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

        var fs = DataParent.Project.FS;
        var toFs = VfsUtils.GetFilesystemForWrite(DataParent.Project);
        string param = @"Data0.bdt";
        if (!fs.FileExists(param))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Cannot locate param files. Save failed.", LogLevel.Error);

            return false;
        }

        var data = fs.GetFile(param).GetData().ToArray();
        BND4 paramBnd = SFUtil.DecryptDS3Regulation(data);

        // Replace params with edited ones
        foreach (BinderFile p in paramBnd.Files)
        {
            if (Params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
            {
                p.Bytes = Params[Path.GetFileNameWithoutExtension(p.Name)].Write();
            }
        }

        // If not loose write out the new regulation
        if (!CFG.Current.UseLooseParams)
        {
            VfsUtils.WriteWithBackup(DataParent.Project, fs, toFs, @"Data0.bdt", paramBnd, ProjectType.DS3);
        }
        else
        {
            // Otherwise write them out as parambnds
            BND4 paramBND = new()
            {
                BigEndian = false,
                Compression = DCX.Type.DCX_DFLT_10000_44_9,
                Extended = 0x04,
                Unk04 = false,
                Unk05 = false,
                Format = Binder.Format.Compression | Binder.Format.Flag6 | Binder.Format.LongOffsets |
                         Binder.Format.Names1,
                Unicode = true,
                Files = paramBnd.Files.Where(f => f.Name.EndsWith(".param")).ToList()
            };

            VfsUtils.WriteWithBackup(DataParent.Project, fs, toFs, @"param\gameparam\gameparam_dlc2.parambnd.dcx", paramBND);
        }

        return successfulSave;
    }

    /// <summary>
    /// Bloodborne
    /// </summary>
    private bool SaveParameters_BB()
    {
        var successfulSave = true;

        var fs = DataParent.Project.FS;
        var toFs = VfsUtils.GetFilesystemForWrite(DataParent.Project);
        string param = @"param\gameparam\gameparam.parambnd.dcx";

        if (!fs.FileExists(param))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Cannot locate param files. Save failed.", LogLevel.Error);

            return false;
        }

        var data = fs.GetFile(param).GetData().ToArray();

        var paramBnd = BND4.Read(data);

        // Replace params with edited ones
        foreach (BinderFile p in paramBnd.Files)
        {
            if (Params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
            {
                p.Bytes = Params[Path.GetFileNameWithoutExtension(p.Name)].Write();
            }
        }

        VfsUtils.WriteWithBackup(DataParent.Project, fs, toFs, @"param\gameparam\gameparam.parambnd.dcx", paramBnd);

        return successfulSave;
    }

    /// <summary>
    /// Sekiro: Shadows Die Twice
    /// </summary>
    private bool SaveParameters_SDT()
    {
        var successfulSave = true;

        var fs = DataParent.Project.FS;
        var toFs = VfsUtils.GetFilesystemForWrite(DataParent.Project);
        string param = @"param\gameparam\gameparam.parambnd.dcx";

        if (!fs.FileExists(param))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Cannot locate param files. Save failed.", LogLevel.Error);

            return false;
        }

        var data = fs.GetFile(param).GetData().ToArray();

        var paramBnd = BND4.Read(data);

        // Replace params with edited ones
        foreach (BinderFile p in paramBnd.Files)
        {
            var paramName = Path.GetFileNameWithoutExtension(p.Name);
            if (Params.TryGetValue(paramName, out Param paramFile))
            {
                IReadOnlyList<Param.Row> backup = paramFile.Rows;

                if (_usedTentativeParamTypes.TryGetValue(paramName, out var oldParamType))
                {
                    // This param was given a tentative ParamType, return original ParamType if possible.
                    oldParamType ??= "";
                    var prevParamType = paramFile.ParamType;
                    paramFile.ParamType = oldParamType;

                    p.Bytes = paramFile.Write();
                    paramFile.ParamType = prevParamType;
                    paramFile.Rows = backup;
                    continue;
                }

                p.Bytes = paramFile.Write();
                paramFile.Rows = backup;
            }
        }

        VfsUtils.WriteWithBackup(DataParent.Project, fs, toFs, @"param\gameparam\gameparam.parambnd.dcx", paramBnd);

        return successfulSave;

        return successfulSave;
    }

    /// <summary>
    /// Elden Ring
    /// </summary>
    private bool SaveParameters_ER()
    {
        var successfulSave = true;

        void OverwriteParamsER(BND4 paramBnd)
        {
            // Replace params with edited ones
            foreach (BinderFile p in paramBnd.Files)
            {
                if (Params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
                {
                    Param paramFile = Params[Path.GetFileNameWithoutExtension(p.Name)];
                    IReadOnlyList<Param.Row> backup = paramFile.Rows;

                    p.Bytes = paramFile.Write();
                    paramFile.Rows = backup;
                }
            }
        }

        var sourceFs = DataParent.Project.FS;
        var gameFs = DataParent.Project.VanillaRealFS;
        var writeFs = VfsUtils.GetFilesystemForWrite(DataParent.Project);

        string param = @"regulation.bin";

        if (!sourceFs.FileExists(param))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Cannot locate param files. Save failed.", LogLevel.Error);

            return false;
        }

        var data = sourceFs.GetFile(param).GetData().ToArray();

        // Use the game root version in this case
        if(!sourceFs.FileExists(param) || PendingUpgrade)
        {
            data = gameFs.GetFile(param).GetData().ToArray();
        }

        BND4 regParams = SFUtil.DecryptERRegulation(data);

        OverwriteParamsER(regParams);

        VfsUtils.WriteWithBackup(DataParent.Project, sourceFs, writeFs, @"regulation.bin", regParams, ProjectType.ER);

        var sysParam = @"param\systemparam\systemparam.parambnd.dcx";

        if (!sourceFs.FileExists(sysParam))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Cannot locate system param files. Save failed.", LogLevel.Error);

            return false;
        }

        if (sourceFs.TryGetFile(sysParam, out var sysParamF))
        {
            using var sysParams = BND4.Read(sysParamF.GetData());
            OverwriteParamsER(sysParams);
            VfsUtils.WriteWithBackup(DataParent.Project, sourceFs, writeFs, @"param\systemparam\systemparam.parambnd.dcx", sysParams);
        }

        return successfulSave;
    }

    /// <summary>
    /// Armored Core VI: Fires of Rubicon
    /// </summary>
    private bool SaveParameters_AC6()
    {
        var successfulSave = true;

        void OverwriteParamsAC6(BND4 paramBnd)
        {
            // Replace params with edited ones
            foreach (BinderFile p in paramBnd.Files)
            {
                var paramName = Path.GetFileNameWithoutExtension(p.Name);
                if (Params.TryGetValue(paramName, out Param paramFile))
                {
                    IReadOnlyList<Param.Row> backup = paramFile.Rows;

                    if (_usedTentativeParamTypes.TryGetValue(paramName, out var oldParamType))
                    {
                        // This param was given a tentative ParamType, return original ParamType if possible.
                        oldParamType ??= "";
                        var prevParamType = paramFile.ParamType;
                        paramFile.ParamType = oldParamType;

                        p.Bytes = paramFile.Write();
                        paramFile.ParamType = prevParamType;
                        paramFile.Rows = backup;
                        continue;
                    }

                    p.Bytes = paramFile.Write();
                    paramFile.Rows = backup;
                }
            }
        }

        var sourceFs = DataParent.Project.FS;
        var gameFs = DataParent.Project.VanillaRealFS;
        var writeFs = VfsUtils.GetFilesystemForWrite(DataParent.Project);

        string param = @"regulation.bin";
        if (!sourceFs.FileExists(param))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Cannot locate param files. Save failed.", LogLevel.Error);

            return false;
        }

        var data = sourceFs.GetFile(param).GetData().ToArray();

        // Use the game root version in this case
        if (!sourceFs.FileExists(param) || PendingUpgrade)
        {
            data = gameFs.GetFile(param).GetData().ToArray();
        }

        BND4 regParams = SFUtil.DecryptAC6Regulation(data);
        OverwriteParamsAC6(regParams);
        VfsUtils.WriteWithBackup(DataParent.Project, sourceFs, writeFs, @"regulation.bin", regParams, ProjectType.AC6);

        var sysParam = @"param\systemparam\systemparam.parambnd.dcx";

        if (!sourceFs.FileExists(sysParam))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Cannot locate system param files. Save failed.", LogLevel.Error);

            return false;
        }

        if (sourceFs.TryGetFile(sysParam, out var sysParamF))
        {
            using var sysParams = BND4.Read(sysParamF.GetData());
            OverwriteParamsAC6(sysParams);
            VfsUtils.WriteWithBackup(DataParent.Project, sourceFs, writeFs, sysParam, sysParams);
        }

        var graphicsParam = @"param\graphicsconfig\graphicsconfig.parambnd.dcx";

        if (!sourceFs.FileExists(sysParam))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Cannot locate graphics param files. Save failed.", LogLevel.Error);

            return false;
        }

        if (sourceFs.TryGetFile(graphicsParam, out var graphicsParamF))
        {
            using var graphicsParams = BND4.Read(graphicsParamF.GetData());
            OverwriteParamsAC6(graphicsParams);
            VfsUtils.WriteWithBackup(DataParent.Project, sourceFs, writeFs, graphicsParam, graphicsParams);
        }

        var eventParam = @"param\eventparam\eventparam.parambnd.dcx";

        if (!sourceFs.FileExists(eventParam))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Cannot locate event param files. Save failed.", LogLevel.Error);

            return false;
        }

        if (sourceFs.TryGetFile(eventParam, out var eventParamF))
        {
            using var eventParams = BND4.Read(eventParamF.GetData());
            OverwriteParamsAC6(eventParams);
            VfsUtils.WriteWithBackup(DataParent.Project, sourceFs, writeFs, eventParam, eventParams);
        }

        return successfulSave;
    }

    /// <summary>
    /// Elden Ring: Nightreign
    /// </summary>
    private bool SaveParameters_ERN()
    {
        var successfulSave = true;

        void OverwriteParamsER(BND4 paramBnd)
        {
            // Replace params with edited ones
            foreach (BinderFile p in paramBnd.Files)
            {
                if (Params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
                {
                    Param paramFile = Params[Path.GetFileNameWithoutExtension(p.Name)];
                    IReadOnlyList<Param.Row> backup = paramFile.Rows;

                    p.Bytes = paramFile.Write();
                    paramFile.Rows = backup;
                }
            }
        }

        var fs = DataParent.Project.FS;
        var toFs = VfsUtils.GetFilesystemForWrite(DataParent.Project);
        string param = @"regulation.bin";

        if (!fs.FileExists(param))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Cannot locate param files. Save failed.", LogLevel.Error);

            return false;
        }

        var data = fs.GetFile(param).GetData().ToArray();

        BND4 regParams = SFUtil.DecryptNightreignRegulation(data);

        OverwriteParamsER(regParams);

        VfsUtils.WriteWithBackup(DataParent.Project, fs, toFs, @"regulation.bin", regParams, ProjectType.ERN);

        var sysParam = @"param\systemparam\systemparam.parambnd.dcx";

        if (!fs.FileExists(sysParam))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Cannot locate system param files. Save failed.", LogLevel.Error);

            return false;
        }

        if (fs.TryGetFile(sysParam, out var sysParamF))
        {
            using var sysParams = BND4.Read(sysParamF.GetData());
            OverwriteParamsER(sysParams);
            VfsUtils.WriteWithBackup(DataParent.Project, fs, toFs, @"param\systemparam\systemparam.parambnd.dcx", sysParams);
        }

        return successfulSave;
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

        TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Stripped row names and stored them in {file}");
    }

    public void RowNameRestore()
    {
        RowNameStore store = null;

        var folder = FolderUtils.GetLocalProjectFolder(DataParent.Project);
        var file = Path.Combine(folder, "Stripped Row Names.json");

        if (!File.Exists(file))
        {
            TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to located {file} for row name restore.", LogLevel.Error);
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
                    throw new Exception($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] JsonConvert returned null during RowNameRestore.");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Failed to load {file} for row name restore.", LogLevel.Error);
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

        TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Param Editor:{BankName}] Restored row names from {file}");
    }

    public void ClearParamDiffCaches()
    {
        _vanillaDiffCache = new Dictionary<string, HashSet<int>>();
        _primaryDiffCache = new Dictionary<string, HashSet<int>>();
        foreach (var param in Params.Keys)
        {
            _vanillaDiffCache.Add(param, new HashSet<int>());
            _primaryDiffCache.Add(param, new HashSet<int>());
        }
    }

    public void RefreshParamDiffCaches(bool checkVanillaDiff)
    {
        if (this != DataParent.VanillaBank && checkVanillaDiff)
        {
            _vanillaDiffCache = GetParamDiff(DataParent.VanillaBank);
        }

        if (this == DataParent.VanillaBank && DataParent.PrimaryBank._vanillaDiffCache != null)
        {
            _primaryDiffCache = DataParent.PrimaryBank._vanillaDiffCache;
        }
        else if (this != DataParent.PrimaryBank)
        {
            _primaryDiffCache = GetParamDiff(DataParent.PrimaryBank);
        }

        DataParent.UICache.ClearCaches();
    }

    private Dictionary<string, HashSet<int>> GetParamDiff(ParamBank otherBank)
    {
        if (IsParamBankLoaded || otherBank == null || otherBank.IsParamBankLoaded)
        {
            return null;
        }

        Dictionary<string, HashSet<int>> newCache = new();
        foreach (var param in Params.Keys)
        {
            HashSet<int> cache = new();
            newCache.Add(param, cache);
            Param p = Params[param];

            if (!otherBank.Params.ContainsKey(param))
            {
                Console.WriteLine("Missing vanilla param " + param);
                continue;
            }

            Param.Row[] rows = Params[param].Rows.OrderBy(r => r.ID).ToArray();
            Param.Row[] vrows = otherBank.Params[param].Rows.OrderBy(r => r.ID).ToArray();

            var vanillaIndex = 0;
            var lastID = -1;
            ReadOnlySpan<Param.Row> lastVanillaRows = default;

            for (var i = 0; i < rows.Length; i++)
            {
                var ID = rows[i].ID;
                if (ID == lastID)
                {
                    RefreshParamRowDiffCache(rows[i], lastVanillaRows, cache);
                }
                else
                {
                    lastID = ID;
                    while (vanillaIndex < vrows.Length && vrows[vanillaIndex].ID < ID)
                    {
                        vanillaIndex++;
                    }

                    if (vanillaIndex >= vrows.Length)
                    {
                        RefreshParamRowDiffCache(rows[i], Span<Param.Row>.Empty, cache);
                    }
                    else
                    {
                        var count = 0;
                        while (vanillaIndex + count < vrows.Length && vrows[vanillaIndex + count].ID == ID)
                        {
                            count++;
                        }

                        lastVanillaRows = new ReadOnlySpan<Param.Row>(vrows, vanillaIndex, count);
                        RefreshParamRowDiffCache(rows[i], lastVanillaRows, cache);
                        vanillaIndex += count;
                    }
                }
            }
        }

        return newCache;
    }

    private void RefreshParamRowDiffCache(Param.Row row, ReadOnlySpan<Param.Row> otherBankRows,
        HashSet<int> cache)
    {
        if (IsChanged(row, otherBankRows))
        {
            cache.Add(row.ID);
        }
        else
        {
            cache.Remove(row.ID);
        }
    }

    public void RefreshParamRowDiffs(Param.Row row, string param)
    {
        if (param == null)
        {
            return;
        }

        if (DataParent.VanillaBank.Params.ContainsKey(param) && VanillaDiffCache != null && VanillaDiffCache.ContainsKey(param))
        {
            Param.Row[] otherBankRows = DataParent.VanillaBank.Params[param].Rows.Where(cell => cell.ID == row.ID).ToArray();
            RefreshParamRowDiffCache(row, otherBankRows, VanillaDiffCache[param]);
        }

        if (this != DataParent.PrimaryBank)
        {
            return;
        }

        var aux = DataParent.AuxBank;

        if (aux.Params.ContainsKey(param) && aux.PrimaryDiffCache != null && aux.PrimaryDiffCache.ContainsKey(param))
        {
            Param.Row[] otherBankRows = aux.Params[param].Rows.Where(cell => cell.ID == row.ID).ToArray();
            RefreshParamRowDiffCache(row, otherBankRows, aux.PrimaryDiffCache[param]);
        }
    }

    private bool IsChanged(Param.Row row, ReadOnlySpan<Param.Row> vanillaRows)
    {
        if (vanillaRows.Length == 0)
        {
            return true;
        }

        foreach (Param.Row vrow in vanillaRows)
        {
            if (row.RowMatches(vrow))
            {
                return false; //if we find a matching vanilla row
            }
        }

        return true;
    }
}
