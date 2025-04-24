using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.Editor;

public class FeatureFlags
{
    /// <summary>
    /// These have been implemented so we can easily produce an individual editor release without needing to maintain separate projects.
    /// When these are false, all relevant aspects are either not initialized or accessed.
    /// </summary>
    public static bool IncludeFileBrowser = true;
    public static bool IncludeParamEditor = true;
    public static bool IncludeModelEditor = true;
    public static bool IncludeScriptingConsole = true;
    public static bool IncludeBehaviorEditor = true;
    public static bool IncludeCollisionEditor = true;
}
