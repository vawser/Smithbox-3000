using Smithbox.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomlyn.Model;
using Tomlyn;
using System.Diagnostics;

namespace Smithbox.Core.Utils;

public static class ModEngineUtils
{
    /// <summary>
    /// A bit hacky, but good enough for now
    /// </summary>
    /// <param name="curProject"></param>
    public static void LaunchMod(Project curProject)
    {
        var modEngineInstallFolderPath = Path.GetDirectoryName(CFG.Current.ModEngineInstall);
        var modTomlPath = @$"{modEngineInstallFolderPath}\smithbox_launch_config.toml";

        var looseParams = "false";
        if (CFG.Current.UseLooseParams)
            looseParams = "true";

        string tomlString = $@"[modengine]
debug = false
external_dlls = []

[extension.mod_loader]
enabled = true
loose_params = {looseParams}

mods = [
    {{ enabled = true, name = ""{curProject.ProjectName}"", path = ""{curProject.ProjectPath.Replace("\\", "\\\\")}"" }}
]

[extension.scylla_hide]
enabled = false";

        File.WriteAllText(modTomlPath, tomlString);

        if(File.Exists(modTomlPath))
        {
            var tomlPath = $@"{modEngineInstallFolderPath}\smithbox_launch_config.toml";
            var projectType = $"{curProject.ProjectType}".ToLower();

            var inputStr = $"'-t' '{projectType}' '-c' '{tomlPath}'".Replace("'", "\"");

            TaskLogs.AddLog(inputStr);

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = CFG.Current.ModEngineInstall,
                Arguments = inputStr,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            Process.Start(psi);
        }
    }
}
