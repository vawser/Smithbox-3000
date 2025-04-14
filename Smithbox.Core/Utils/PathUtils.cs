using Andre.IO.VFS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.Utils;

public class PathUtils
{
    public static string GetGameParam_DES(VirtualFileSystem fs)
    {
        var name = $@"param\gameparam\gameparamna.parambnd.dcx";

        if (fs.FileExists($@"{name}"))
        {
            return name;
        }

        name = $@"param\gameparam\gameparamna.parambnd";
        if (fs.FileExists($@"{name}"))
        {
            return name;
        }

        name = $@"param\gameparam\gameparam.parambnd.dcx";
        if (fs.FileExists($@"{name}"))
        {
            return name;
        }

        name = $@"param\gameparam\gameparam.parambnd";
        if (fs.FileExists($@"{name}"))
        {
            return name;
        }

        return "";
    }

    public static string GetGameParam_DS1(VirtualFileSystem fs)
    {
        var name = $@"param\GameParam\GameParam.parambnd";
        if (fs.FileExists($@"{name}"))
        {
            return name;
        }

        name = $@"param\GameParam\GameParam.parambnd.dcx";
        if (fs.FileExists($@"{name}"))
        {
            return name;
        }
        return "";
    }

    public static List<string> GetLooseParamsInDir(VirtualFileSystem fs, string dir)
    {
        List<string> looseParams = new();

        string paramDir = Path.Combine(dir, "Param");
        looseParams.AddRange(fs.GetFileNamesWithExtensions(paramDir, ".param"));

        return looseParams;
    }
}
