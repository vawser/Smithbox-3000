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
    public static void LaunchMod(Project curProject)
    {
        var modEngineInstallFolderPath = Path.GetDirectoryName(CFG.Current.ModEngineInstall);
        var modTomlPath = @$"{modEngineInstallFolderPath}\smithbox_launch_config.toml";

        if (!File.Exists(modTomlPath))
        {
            string tomlString = $@"[modengine]
debug = false
external_dlls = []

[extension.mod_loader]
enabled = true
loose_params = {CFG.Current.UseLooseParams}

mods = [
    {{ enabled = true, name = ""{curProject.ProjectName}"", path = ""{curProject.ProjectPath}"" }}
]

[extension.scylla_hide]
enabled = false";

            File.WriteAllText(modTomlPath, tomlString);
        }

        if(File.Exists(modTomlPath))
        {
            var projectType = $"{curProject.ProjectType}".ToLower();

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = @$"{CFG.Current.ModEngineInstall} -t {projectType} -c {modTomlPath}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        }
    }
}
