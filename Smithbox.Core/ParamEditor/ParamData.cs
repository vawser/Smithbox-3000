using Smithbox.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.ParamEditorNS;

public class ParamData
{
    private Project Project;

    public bool Initialized = false;

    public readonly List<ProjectType> AllowedParamUpgrade = new List<ProjectType>()
    {
        ProjectType.ER,
        ProjectType.AC6
    };

    public ParamData(Project projectOwner)
    {
        Project = projectOwner;
        Initialize();
    }

    public void Initialize()
    {
        if (Initialized)
            return;

        // Start tasks
    }

    public void Update()
    {

    }
}
