using Andre.Formats;
using Smithbox.Core.Editor;
using Smithbox.Core.ParamEditorNS;
using Smithbox.Core.Resources;
using Smithbox.Core.Utils;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Smithbox.Core.ParamEditorNS;

public class ParamData
{
    public Project Project;

    public bool Initialized;

    public ParamUICache UICache;

    public Dictionary<PARAMDEF, ParamMeta> ParamMeta = new();

    public ParamBank PrimaryBank;
    public ParamBank VanillaBank;

    /// <summary>
    /// If param != primary param
    /// </summary>
    public Dictionary<string, HashSet<int>> PrimaryDiffCache;

    /// <summary>
    /// If param != vanilla param
    /// </summary>
    public Dictionary<string, HashSet<int>> VanillaDiffCache;

    public Dictionary<string, ParamBank> AuxBanks = new();

    public string Clipboard_Param = null;
    public List<Param.Row> Clipboard_Rows = new();

    /// <summary>
    /// Mapping between ParamType -> PARAMDEF
    /// </summary>
    public Dictionary<string, PARAMDEF> Paramdefs;

    public bool IsParamDefLoaded { get; private set; }
    public bool IsParamMetaLoaded { get; private set; }
    public bool IsPrimaryBankLoaded { get; private set; }
    public bool IsVanillaBankLoaded { get; private set; }

    public ParamTypeInfo ParamTypeInfo;

    public ParamData(Project projectOwner)
    {
        Project = projectOwner;
        UICache = new(projectOwner);

        Initialize();
    }

    public async void Initialize()
    {
        if (Initialized)
            return;

        IsParamDefLoaded = false;
        IsParamMetaLoaded = false;
        IsPrimaryBankLoaded = false;
        IsVanillaBankLoaded = false;

        UICache.ClearCaches();

        // Param Defs
        Task<bool> paramDefsTask = LoadParamDefs();
        bool paramDefsLoaded = await paramDefsTask;

        if (paramDefsLoaded)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Finished param definition setup.");
            IsParamDefLoaded = true;
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to setup param definitions.");
        }

        // Meta
        Task<bool> metaTask = LoadParamMeta(Paramdefs);
        bool paramMetaLoaded = await metaTask;

        if (paramMetaLoaded)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Finished param meta setup.");
            IsParamMetaLoaded = true;
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to setup param meta.");
        }

        // Primary Bank
        PrimaryBank = new(this, Project.ProjectPath, "Primary");

        Task<bool> primaryBankTask = PrimaryBank.Load(Project.FS, Paramdefs);
        bool primaryBankLoaded = await primaryBankTask;
        
        if (primaryBankLoaded)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Finished primary param bank setup.");
            IsPrimaryBankLoaded = true;
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to setup primary param bank fully.");
        }

        // Vanilla Bank
        VanillaBank = new(this, Project.DataPath, "Vanilla");

        Task<bool> vanillaBankTask = VanillaBank.Load(Project.VanillaFS, Paramdefs);
        bool vanillaBankLoaded = await vanillaBankTask;

        if (vanillaBankLoaded)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Finished vanilla param bank setup.");
            IsVanillaBankLoaded = true;
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to setup vanilla param bank fully.");
        }

        // Primary Bank: Import Row Names
        if(!Project.ImportedParamRowNames)
        {
            Project.PrimaryParamEditor.ParamActions.ImportDefaultParamRowNames(PrimaryBank);
        }

        // Primary Bank: Stripped Param Row Restore
        if (Project.EnableParamRowStrip)
        {
            PrimaryBank.RowNameRestore();
        }

        Initialized = true;
    }

    /// <summary>
    /// Manual reload of banks
    /// </summary>
    public async void ReloadBanks()
    {
        IsParamDefLoaded = false;
        IsParamMetaLoaded = false;
        IsPrimaryBankLoaded = false;
        IsVanillaBankLoaded = false;

        UICache.ClearCaches();

        // Primary Bank
        PrimaryBank = new(this, Project.ProjectPath, "Primary");

        Task<bool> primaryBankTask = PrimaryBank.Load(Project.FS, Paramdefs);
        bool primaryBankLoaded = await primaryBankTask;

        if (primaryBankLoaded)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Reloaded primary bank.");
            IsPrimaryBankLoaded = true;
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to reload primary bank.");
        }

        // Vanilla Bank
        VanillaBank = new(this, Project.DataPath, "Vanilla");

        Task<bool> vanillaBankTask = VanillaBank.Load(Project.VanillaFS, Paramdefs);
        bool vanillaBankLoaded = await vanillaBankTask;

        if (vanillaBankLoaded)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Reloaded vanilla bank.");
            IsVanillaBankLoaded = true;
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to reload vanilla bank.");
        }

        // Primary Bank: Import Row Names
        if (!Project.ImportedParamRowNames)
        {
            Project.PrimaryParamEditor.ParamActions.ImportDefaultParamRowNames(PrimaryBank);
        }

        // Primary Bank: Stripped Param Row Restore
        if (Project.EnableParamRowStrip)
        {
            PrimaryBank.RowNameRestore();
        }

        Initialized = true;
    }

    public ParamMeta GetParamMeta(PARAMDEF def)
    {
        if(ParamMeta.ContainsKey(def))
        {
            return ParamMeta[def];
        }

        return null;
    }

    public void Update()
    {

    }

    public async Task<bool> LoadParamDefs()
    {
        await Task.Delay(1000);

        Paramdefs = new Dictionary<string, PARAMDEF>();
        ParamTypeInfo = new();
        ParamTypeInfo.Mapping = new();
        ParamTypeInfo.Exceptions = new();

        // Param Defs
        var paramDefDirectory = @$"{AppContext.BaseDirectory}\Assets\PARAM\{LocatorUtils.GetGameDirectory(Project)}\Defs";

        var files = Directory.GetFiles(paramDefDirectory, "*.xml");

        List<(string, PARAMDEF)> defPairs = new();

        foreach (var f in files)
        {
            var pdef = PARAMDEF.XmlDeserialize(f, true);
            Paramdefs.Add(pdef.ParamType, pdef);
            defPairs.Add((f, pdef));
        }

        // Param Type Info
        var paramTypeInfoPath = @$"{AppContext.BaseDirectory}\Assets\PARAM\{LocatorUtils.GetGameDirectory(Project)}\Param Type Info.json";

        if (File.Exists(paramTypeInfoPath))
        {
            var paramTypeInfo = new ParamTypeInfo();
            paramTypeInfo.Mapping = new();
            paramTypeInfo.Exceptions = new();

            ParamTypeInfo = paramTypeInfo;

            try
            {
                var filestring = File.ReadAllText(paramTypeInfoPath);
                var options = new JsonSerializerOptions();

                paramTypeInfo = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.ParamTypeInfo);

                if (paramTypeInfo == null)
                {
                    throw new Exception($"[{Project.ProjectName}:Param Editor] Failed to read Param Type Info.json");
                }

                ParamTypeInfo = paramTypeInfo;
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to load Param Type Info.json");
            }
        }

        return true;
    }

    public async Task<bool> LoadParamMeta(Dictionary<string, PARAMDEF> defs)
    {
        await Task.Delay(1000);

        var rootMetaDir = @$"{AppContext.BaseDirectory}\Assets\PARAM\{LocatorUtils.GetGameDirectory(Project)}\Meta";

        var projectMetaDir = @$"{Project.ProjectPath}\.smithbox\Assets\PARAM\{LocatorUtils.GetGameDirectory(Project)}\Meta";

        if(CFG.Current.UseProjectMeta)
        {
            if (Project.ProjectType != ProjectType.None)
            {
                // Create the project meta copy if it doesn't already exist
                if (!Directory.Exists(projectMetaDir))
                {
                    Directory.CreateDirectory(projectMetaDir);
                    var files = Directory.GetFileSystemEntries(rootMetaDir);

                    foreach (var f in files)
                    {
                        var name = Path.GetFileName(f);
                        var tPath = Path.Combine(rootMetaDir, name);
                        var pPath = Path.Combine(projectMetaDir, name);

                        if (File.Exists(tPath) && !File.Exists(pPath))
                        {
                            File.Copy(tPath, pPath);
                        }
                    }
                }
            }
        }

        foreach ((var f, PARAMDEF pdef) in defs)
        {
            ParamMeta meta = new(this);

            var fName = f.Substring(f.LastIndexOf('\\') + 1);

            if (CFG.Current.UseProjectMeta && Project.ProjectType != ProjectType.None)
            {
                meta.XmlDeserialize($@"{projectMetaDir}\{fName}", pdef);
            }
            else
            {
                meta.XmlDeserialize($@"{rootMetaDir}\{fName}", pdef);
            }

            ParamMeta.Add(pdef, meta);
        }

        return true;
    }

    public void RefreshAllParamDiffCaches(bool checkAuxVanillaDiff)
    {
        PrimaryBank.RefreshParamDiffCaches(true);

        foreach (KeyValuePair<string, ParamBank> bank in Project.ParamData.AuxBanks)
        {
            bank.Value.RefreshParamDiffCaches(checkAuxVanillaDiff);
        }

        UICache.ClearCaches();
    }

    public void RefreshParamDifferenceCacheTask(bool checkAuxVanillaDiff = false)
    {
        // Refresh diff cache
        TaskManager.LiveTask task = new(
            "paramEditor_refreshDifferenceCache",
            "Param Editor",
            "difference cache between param banks has been refreshed.",
            "difference cache refresh has failed.",
            TaskManager.RequeueType.Repeat,
            true,
            () => RefreshAllParamDiffCaches(checkAuxVanillaDiff)
        );

        TaskManager.Run(task);
    }
}
