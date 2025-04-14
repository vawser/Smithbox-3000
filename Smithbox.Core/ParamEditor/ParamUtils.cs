using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.ParamEditorNS;

public class ParamUtils
{
    public static string ParseRegulationVersion(ulong version)
    {
        string verStr = version.ToString();
        if (verStr.Length != 8)
        {
            return "Unknown Version";
        }
        char major = verStr[0];
        string minor = verStr[1..3];
        char patch = verStr[3];
        string rev = verStr[4..];

        return $"{major}.{minor}.{patch}.{rev}";
    }
}
