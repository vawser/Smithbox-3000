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
        if(type is ProjectType.ERN)
        {
            return false;
        }

        return true;
    }
}
