using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.Utils;

public class FolderUtils
{
    public static void SetupFolders()
    {
        var folder = $"{AppContext.BaseDirectory}/{Consts.DataFolder}/";
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        folder = $"{AppContext.BaseDirectory}/{Consts.ConfigurationFolder}/";
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        folder = $"{AppContext.BaseDirectory}/{Consts.ProjectFolder}/";
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
    }

    public static string GetConfigurationFolder()
    {
        var folder = $"{AppContext.BaseDirectory}/{Consts.ConfigurationFolder}/";

        return folder;
    }

    public static string GetProjectFolder()
    {
        var folder = $"{AppContext.BaseDirectory}/{Consts.ProjectFolder}/";

        return folder;
    }

    public static List<string> GetStoredProjectJsonList()
    {
        var projectJsonList = new List<string>();
        var projectFolder = GetProjectFolder();

        projectJsonList = Directory.EnumerateFiles(projectFolder, ".json").ToList();

        return projectJsonList;
    }
}
