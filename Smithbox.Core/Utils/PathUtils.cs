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

    public static string GetGameParam_DS1R(VirtualFileSystem fs)
    {
        var name = $@"param\\GameParam\\GameParam.parambnd.dcx";
        if (fs.FileExists($@"{name}"))
        {
            return name;
        }
        return "";
    }

    public static string GetGameParam_DS2(VirtualFileSystem fs)
    {
        var name = $@"enc_regulation.bnd.dcx";
        if (fs.FileExists($@"{name}"))
        {
            return name;
        }
        return "";
    }

    public static string GetGameParam_DS2S(VirtualFileSystem fs)
    {
        var name = $@"enc_regulation.bnd.dcx";
        if (fs.FileExists($@"{name}"))
        {
            return name;
        }
        return "";
    }

    public static string GetEnemyParam_DS2S(VirtualFileSystem fs)
    {
        var name = $@"Param\\EnemyParam.param";
        if (fs.FileExists($@"{name}"))
        {
            return name;
        }
        return "";
    }

    public static string GetGameParamLoose_DS3(VirtualFileSystem fs)
    {
        var name = $@"param\\gameparam\\gameparam_dlc2.parambnd.dcx";
        if (fs.FileExists(@$"{name}"))
        {
            return name;
        }
        return "";
    }
    public static string GetGameParamPacked_DS3(VirtualFileSystem fs)
    {
        var name = $@"Data0.bdt";
        if (fs.FileExists(name))
        {
            return name;
        }
        return "";
    }

    public static string GetGameParam_BB(VirtualFileSystem fs)
    {
        var name = $@"param\\gameparam\\gameparam.parambnd.dcx";

        if (fs.FileExists($@"{name}"))
        {
            return name;
        }
        return "";
    }

    public static string GetGameParam_SDT(VirtualFileSystem fs)
    {
        var name = $@"param\gameparam\gameparam.parambnd.dcx";
        if (fs.FileExists($@"{name}"))
        {
            return name;
        }
        return "";
    }

    public static string GetGameParam_ER(VirtualFileSystem fs)
    {
        var name = $@"regulation.bin";
        if (fs.FileExists($@"{name}"))
        {
            return name;
        }
        return "";
    }

    public static string GetSystemParam_ER(VirtualFileSystem fs)
    {
        var name = $@"param\systemparam\systemparam.parambnd.dcx";
        if (fs.FileExists($@"{name}"))
        {
            return name;
        }
        return "";
    }

    public static string GetGameParam_AC6(VirtualFileSystem fs)
    {
        var name = $@"regulation.bin";
        if (fs.FileExists($@"{name}"))
        {
            return name;
        }
        return "";
    }

    public static string GetSystemParam_AC6(VirtualFileSystem fs)
    {
        var name = $@"param\systemparam\systemparam.parambnd.dcx";
        if (fs.FileExists($@"{name}"))
        {
            return name;
        }
        return "";
    }

    public static string GetGraphicsParam_AC6(VirtualFileSystem fs)
    {
        var name = $@"param\graphicsconfig\graphicsconfig.parambnd.dcx";
        if (fs.FileExists($@"{name}"))
        {
            return name;
        }
        return "";
    }

    public static string GetEventParam_AC6(VirtualFileSystem fs)
    {
        var name = $@"param\eventparam\eventparam.parambnd.dcx";
        if (fs.FileExists($@"{name}"))
        {
            return name;
        }
        return "";
    }

    public static string GetGameParam_ERN(VirtualFileSystem fs)
    {
        var name = $@"regulation.bin";
        if (fs.FileExists($@"{name}"))
        {
            return name;
        }
        return "";
    }

    public static string GetSystemParam_ERN(VirtualFileSystem fs)
    {
        var name = $@"param\systemparam\systemparam.parambnd.dcx";
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
