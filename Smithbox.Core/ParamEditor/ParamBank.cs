using Andre.Formats;
using Andre.IO.VFS;
using Microsoft.Extensions.Logging;
using Smithbox.Core.Editor;
using Smithbox.Core.ParamEditorNS;
using Smithbox.Core.Utils;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        switch (DataParent.Project.ProjectType)
        {
            case ProjectType.DES: LoadParameters_DES(fs, defs); break;
            case ProjectType.DS1: LoadParameters_DS1(fs, defs); break;
            case ProjectType.DS1R: LoadParameters_DS1R(fs, defs); break;
            case ProjectType.DS2: LoadParameters_DS2(fs, defs); break;
            case ProjectType.DS2S: LoadParameters_DS2S(fs, defs); break;
            case ProjectType.DS3: LoadParameters_DS3(fs, defs); break;
            case ProjectType.BB: LoadParameters_BB(fs, defs); break;
            case ProjectType.SDT: LoadParameters_SDT(fs, defs); break;
            case ProjectType.ER: LoadParameters_ER(fs, defs); break;
            case ProjectType.AC6: LoadParameters_AC6(fs, defs); break;
            case ProjectType.ERN: LoadParameters_ERN(fs, defs); break;
            default: break;
        }


        return true;
    }

    /// <summary>
    /// Demons' Souls
    /// </summary>
    private void LoadParameters_DES(VirtualFileSystem fs, Dictionary<string, PARAMDEF> defs)
    {
        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;

        var paramPath = PathUtils.GetGameParam_DES(fs);

        if (!fs.FileExists(paramPath))
        {
            TaskLogs.AddLog($"Failed to find {paramPath}");
        }
        else
        {
            try
            {
                using var bnd = BND3.Read(fs.GetFile(paramPath).GetData());
                FillParamBank(defs, bnd, ref Params, out ParamVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"Failed to load game param: {paramPath}");
            }

            // Draw Params
            foreach (var f in fs.FsRoot.GetDirectory("param")?.GetDirectory("drawparam")?.EnumerateFileNames() ?? [])
            {
                if (f.EndsWith(".parambnd.dcx"))
                {
                    paramPath = $"param/drawparam/{f}";

                    try
                    {
                        using var bnd = BND3.Read(fs.GetFile(paramPath).GetData());
                        FillParamBank(defs, bnd, ref Params, out ParamVersion);
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"Failed to load draw param: {paramPath}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Dark Souls (PTDE)
    /// </summary>
    private void LoadParameters_DS1(VirtualFileSystem fs, Dictionary<string, PARAMDEF> defs)
    {
        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;

        var paramPath = PathUtils.GetGameParam_DS1(fs);

        if (!fs.FileExists(paramPath))
        {
            TaskLogs.AddLog($"Failed to find {paramPath}");
        }
        else
        {
            try
            {
                using var bnd = BND3.Read(fs.GetFile(paramPath).GetData());
                FillParamBank(defs, bnd, ref Params, out ParamVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"Failed to load game param: {paramPath}");
            }

            // Draw Params
            foreach (var f in fs.FsRoot.GetDirectory("param")?.GetDirectory("drawparam")?.EnumerateFileNames() ?? [])
            {
                if (f.EndsWith(".parambnd.dcx"))
                {
                    paramPath = $"param/drawparam/{f}";

                    try
                    {
                        using var bnd = BND3.Read(fs.GetFile(paramPath).GetData());
                        FillParamBank(defs, bnd, ref Params, out ParamVersion);
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"Failed to load draw param: {paramPath}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Dark Souls (Remastered)
    /// </summary>
    private void LoadParameters_DS1R(VirtualFileSystem fs, Dictionary<string, PARAMDEF> defs)
    {
        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;

        var paramPath = PathUtils.GetGameParam_DS1R(fs);

        if (!fs.FileExists(paramPath))
        {
            TaskLogs.AddLog($"Failed to find {paramPath}");
        }
        else
        {
            try
            {
                using var bnd = BND3.Read(fs.GetFile(paramPath).GetData());
                FillParamBank(defs, bnd, ref Params, out ParamVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"Failed to load game param: {paramPath}");
            }

            // Draw Params
            foreach (var f in fs.FsRoot.GetDirectory("param")?.GetDirectory("drawparam")?.EnumerateFileNames() ?? [])
            {
                if (f.EndsWith(".parambnd.dcx"))
                {
                    paramPath = $"param/drawparam/{f}";

                    try
                    {
                        using var bnd = BND3.Read(fs.GetFile(paramPath).GetData());
                        FillParamBank(defs, bnd, ref Params, out ParamVersion);
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"Failed to load draw param: {paramPath}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Dark Souls II
    /// </summary>
    private void LoadParameters_DS2(VirtualFileSystem fs, Dictionary<string, PARAMDEF> defs)
    {
        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;

        var paramPath = PathUtils.GetGameParam_DS2(fs);
        var enemyPath = PathUtils.GetEnemyParam_DS2S(fs);

        if (!fs.FileExists(paramPath))
        {
            TaskLogs.AddLog($"Failed to load game param: {paramPath}");
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
                TaskLogs.AddLog($"Failed to load draw param: {paramPath}");
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
                TaskLogs.AddLog($"Failed to load draw param: {paramPath}");
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
            EnemyParam = Param.Read(fs.GetFile(enemyPath).GetData());
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
                TaskLogs.AddLog($"Could not apply ParamDef for {EnemyParam.ParamType}",
                    LogLevel.Warning, e);
            }
        }

        FillParamBank(defs, paramBnd, ref Params, out ParamVersion);

        foreach (var p in looseParams)
        {
            var name = Path.GetFileNameWithoutExtension(p);
            var lp = Param.Read(fs.GetFile(p).GetData());
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
            }
        }

        paramBnd.Dispose();
    }

    /// <summary>
    /// Dark Souls II: Scholar of the First Sin
    /// </summary>
    private void LoadParameters_DS2S(VirtualFileSystem fs, Dictionary<string, PARAMDEF> defs)
    {
        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;

        var paramPath = PathUtils.GetGameParam_DS2S(fs);
        var enemyPath = PathUtils.GetEnemyParam_DS2S(fs);

        if (!fs.FileExists(paramPath))
        {
            TaskLogs.AddLog($"Failed to load game param: {paramPath}");
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
                TaskLogs.AddLog($"Failed to load draw param: {paramPath}");
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
                TaskLogs.AddLog($"Failed to load draw param: {paramPath}");
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
            EnemyParam = Param.Read(fs.GetFile(enemyPath).GetData());
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
                TaskLogs.AddLog($"Could not apply ParamDef for {EnemyParam.ParamType}",
                    LogLevel.Warning, e);
            }
        }

        FillParamBank(defs, paramBnd, ref Params, out ParamVersion);

        foreach (var p in looseParams)
        {
            var name = Path.GetFileNameWithoutExtension(p);
            var lp = Param.Read(fs.GetFile(p).GetData());
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
            }
        }

        paramBnd.Dispose();
    }

    /// <summary>
    /// Dark Souls III
    /// </summary>
    private void LoadParameters_DS3(VirtualFileSystem fs, Dictionary<string, PARAMDEF> defs)
    {
        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;

        var looseFile = PathUtils.GetGameParamLoose_DS3(fs);
        var packedFile = PathUtils.GetGameParamPacked_DS3(fs);

        if (!fs.FileExists(packedFile))
        {
            TaskLogs.AddLog($"Failed to find {packedFile}");
        }
        else
        {
            if (CFG.Current.UseLooseParams && fs.FileExists(looseFile))
            {
                try
                {
                    using var bnd = BND4.Read(fs.GetFile(looseFile).GetData());
                    FillParamBank(defs, bnd, ref Params, out ParamVersion);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"Failed to load game param: {looseFile}");
                }
            }
            else
            {
                try
                {
                    using var bnd = SFUtil.DecryptDS3Regulation(fs.GetFile(packedFile).GetData().ToArray());
                    FillParamBank(defs, bnd, ref Params, out ParamVersion);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"Failed to load game param: {packedFile}");
                }
            }
        }
    }

    /// <summary>
    /// Bloodborne
    /// </summary>
    private void LoadParameters_BB(VirtualFileSystem fs, Dictionary<string, PARAMDEF> defs)
    {
        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;

        var paramPath = PathUtils.GetGameParam_BB(fs);

        if (!fs.FileExists(paramPath))
        {
            TaskLogs.AddLog($"Failed to find {paramPath}");
        }
        else
        {
            try
            {
                using var bnd = BND4.Read(fs.GetFile(paramPath).GetData());
                FillParamBank(defs, bnd, ref Params, out ParamVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"Failed to load game param: {paramPath}");
            }
        }
    }

    /// <summary>
    /// Sekiro: Shadows Die Twice
    /// </summary>
    private void LoadParameters_SDT(VirtualFileSystem fs, Dictionary<string, PARAMDEF> defs)
    {
        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;

        var paramPath = PathUtils.GetGameParam_SDT(fs);

        if (!fs.FileExists(paramPath))
        {
            TaskLogs.AddLog($"Failed to find {paramPath}");
        }
        else
        {
            try
            {
                using var bnd = BND4.Read(fs.GetFile(paramPath).GetData());
                FillParamBank(defs, bnd, ref Params, out ParamVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"Failed to load game param: {paramPath}");
            }
        }
    }

    /// <summary>
    /// Elden Ring
    /// </summary>
    private void LoadParameters_ER(VirtualFileSystem fs, Dictionary<string, PARAMDEF> defs)
    {
        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;

        var gameParamPath = PathUtils.GetGameParam_ER(fs);
        var systemParamPath = PathUtils.GetSystemParam_ER(fs);

        if (!fs.FileExists(gameParamPath))
        {
            TaskLogs.AddLog($"Failed to find {gameParamPath}");
        }
        else
        {
            try
            {
                using BND4 bnd = SFUtil.DecryptERRegulation(fs.GetFile(gameParamPath).GetData().ToArray());
                FillParamBank(defs, bnd, ref Params, out ParamVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"Failed to load game param: {gameParamPath}");
            }
        }

        if (!fs.FileExists(systemParamPath))
        {
            TaskLogs.AddLog($"Failed to find {systemParamPath}");
        }
        else
        {
            try
            {
                using var bnd = BND4.Read(fs.GetFile(systemParamPath).GetData());
                FillParamBank(defs, bnd, ref Params, out ParamVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"Failed to load game param: {systemParamPath}");
            }
        }
    }

    /// <summary>
    /// Armored Core VI: Fires of Rubicon
    /// </summary>
    private void LoadParameters_AC6(VirtualFileSystem fs, Dictionary<string, PARAMDEF> defs)
    {
        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;

        var gameParamPath = PathUtils.GetGameParam_AC6(fs);
        var systemParamPath = PathUtils.GetSystemParam_AC6(fs);
        var graphicsParamPath = PathUtils.GetGraphicsParam_AC6(fs);
        var eventParamPath = PathUtils.GetEventParam_AC6(fs);

        // Game Param
        if (!fs.FileExists(gameParamPath))
        {
            TaskLogs.AddLog($"Failed to find {gameParamPath}");
        }
        else
        {
            try
            {
                using BND4 bnd = SFUtil.DecryptAC6Regulation(fs.GetFile(gameParamPath).GetData().ToArray());
                FillParamBank(defs, bnd, ref Params, out ParamVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"Failed to load game param: {gameParamPath}");
            }
        }

        // System Param
        if (!fs.FileExists(systemParamPath))
        {
            TaskLogs.AddLog($"Failed to find {systemParamPath}");
        }
        else
        {
            try
            {
                using var bnd = BND4.Read(fs.GetFile(systemParamPath).GetData());
                FillParamBank(defs, bnd, ref Params, out ParamVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"Failed to load game param: {systemParamPath}");
            }
        }

        // Graphics Param
        if (!fs.FileExists(graphicsParamPath))
        {
            TaskLogs.AddLog($"Failed to find {graphicsParamPath}");
        }
        else
        {
            try
            {
                using var bnd = BND4.Read(fs.GetFile(graphicsParamPath).GetData());
                FillParamBank(defs, bnd, ref Params, out ParamVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"Failed to load game param: {graphicsParamPath}");
            }
        }

        // Event Param
        if (!fs.FileExists(eventParamPath))
        {
            TaskLogs.AddLog($"Failed to find {eventParamPath}");
        }
        else
        {
            try
            {
                using var bnd = BND4.Read(fs.GetFile(eventParamPath).GetData());
                FillParamBank(defs, bnd, ref Params, out ParamVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"Failed to load game param: {eventParamPath}");
            }
        }
    }

    /// <summary>
    /// Elden Ring: Nightreign
    /// </summary>
    private void LoadParameters_ERN(VirtualFileSystem fs, Dictionary<string, PARAMDEF> defs)
    {
        var dataPath = DataParent.Project.DataPath;
        var projectPath = DataParent.Project.ProjectPath;

        var gameParamPath = PathUtils.GetGameParam_ERN(fs);
        var systemParamPath = PathUtils.GetSystemParam_ERN(fs);

        if (!fs.FileExists(gameParamPath))
        {
            TaskLogs.AddLog($"Failed to find {gameParamPath}");
        }
        else
        {
            try
            {
                using BND4 bnd = SFUtil.DecryptERNRegulation(fs.GetFile(gameParamPath).GetData().ToArray());
                FillParamBank(defs, bnd, ref Params, out ParamVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"Failed to load game param: {gameParamPath}");
            }
        }

        if (!fs.FileExists(systemParamPath))
        {
            TaskLogs.AddLog($"Failed to find {systemParamPath}");
        }
        else
        {
            try
            {
                using var bnd = BND4.Read(fs.GetFile(systemParamPath).GetData());
                FillParamBank(defs, bnd, ref Params, out ParamVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"Failed to load game param: {systemParamPath}");
            }
        }
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
                            TaskLogs.AddLog($"Couldn't find ParamDef for {paramName}, but tentative ParamType \"{newParamType}\" exists.", LogLevel.Debug);
                        }
                        else
                        {
                            TaskLogs.AddLog($"Couldn't find ParamDef for param {paramName} and no tentative ParamType exists.", LogLevel.Error);

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

                        TaskLogs.AddLog($"Couldn't read ParamType for {paramName}, but tentative ParamType \"{newParamType}\" exists.", LogLevel.Debug);
                    }
                    else
                    {
                        TaskLogs.AddLog($"Couldn't read ParamType for {paramName} and no tentative ParamType exists.", LogLevel.Error);

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
                    TaskLogs.AddLog($"Couldn't find ParamDef for param {paramName} with ParamType \"{curParam.ParamType}\".", LogLevel.Warning);

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
                    TaskLogs.AddLog($"Skipped this param: {paramName}");
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
                var message = $"Could not apply ParamDef for {name}";

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

        switch (DataParent.Project.ProjectType)
        {
            case ProjectType.DES:  SaveParameters_DES(); break;
            case ProjectType.DS1:  SaveParameters_DS1(); break;
            case ProjectType.DS1R: SaveParameters_DS1R(); break;
            case ProjectType.DS2:  SaveParameters_DS2(); break;
            case ProjectType.DS2S: SaveParameters_DS2S(); break;
            case ProjectType.DS3:  SaveParameters_DS3(); break;
            case ProjectType.BB:   SaveParameters_BB(); break;
            case ProjectType.SDT:  SaveParameters_SDT(); break;
            case ProjectType.ER:   SaveParameters_ER(); break;
            case ProjectType.AC6:  SaveParameters_AC6(); break;
            case ProjectType.ERN:  SaveParameters_ERN(); break;
            default: break;
        }

        return true;
    }

    /// <summary>
    /// Demons' Souls
    /// </summary>
    private void SaveParameters_DES()
    {
        var fs = DataParent.Project.FileSystem;
        var toFs = VfsUtils.GetFSForWrites(DataParent.Project);

        var paramPath = PathUtils.GetGameParam_DES(fs);

        if (!fs.FileExists(paramPath))
        {
            TaskLogs.AddLog("Cannot locate param files. Save failed.",
                LogLevel.Error);
            return;
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
    }

    /// <summary>
    /// Dark Souls (PTDE)
    /// </summary>
    private void SaveParameters_DS1()
    {

    }

    /// <summary>
    /// Dark Souls (Remastered)
    /// </summary>
    private void SaveParameters_DS1R()
    {

    }

    /// <summary>
    /// Dark Souls II
    /// </summary>
    private void SaveParameters_DS2()
    {

    }

    /// <summary>
    /// Dark Souls II: Scholar of the First Sin
    /// </summary>
    private void SaveParameters_DS2S()
    {

    }

    /// <summary>
    /// Dark Souls III
    /// </summary>
    private void SaveParameters_DS3()
    {

    }

    /// <summary>
    /// Bloodborne
    /// </summary>
    private void SaveParameters_BB()
    {

    }

    /// <summary>
    /// Sekiro: Shadows Die Twice
    /// </summary>
    private void SaveParameters_SDT()
    {

    }

    /// <summary>
    /// Elden Ring
    /// </summary>
    private void SaveParameters_ER()
    {

    }

    /// <summary>
    /// Armored Core VI: Fires of Rubicon
    /// </summary>
    private void SaveParameters_AC6()
    {

    }

    /// <summary>
    /// Elden Ring: Nightreign
    /// </summary>
    private void SaveParameters_ERN()
    {

    }

    /// <summary>
    /// Upgrade task for this Param bank
    /// </summary>
    public async Task<bool> Upgrade()
    {
        await Task.Delay(1000);

        switch (DataParent.Project.ProjectType)
        {
            case ProjectType.ER: UpgradeParameters_ER(); break;
            case ProjectType.AC6: UpgradeParameters_AC6(); break;
            case ProjectType.ERN: UpgradeParameters_ERN(); break;
            default: break;
        }

        return true;
    }

    /// <summary>
    /// Elden Ring
    /// </summary>
    private void UpgradeParameters_ER()
    {

    }

    /// <summary>
    /// Armored Core VI: Fires of Rubicon
    /// </summary>
    private void UpgradeParameters_AC6()
    {

    }

    /// <summary>
    /// Elden Ring: Nightreign
    /// </summary>
    private void UpgradeParameters_ERN()
    {

    }
}
