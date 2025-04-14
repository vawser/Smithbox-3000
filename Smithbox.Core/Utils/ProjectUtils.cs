using Smithbox.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.Utils;

public static class ProjectUtils
{
    /// <summary>
    /// Used to filter out project types that aren't fully supported
    /// </summary>
    public static bool IsSupportedProjectType(ProjectType type)
    {
        // ERN -- Unreleased
        // DS2 + BB -- VFS doesn't support them, need to implement fallback to traditional method
        // AC4 + ACFA + ACV + ACVD -- Legacy for now

        if(type is ProjectType.AC4 or ProjectType.DES or ProjectType.ACFA or ProjectType.ACV or ProjectType.ACVD or ProjectType.BB or ProjectType.DS2 or ProjectType.ERN)
        {
            return false;
        }

        return true;
    }
}
