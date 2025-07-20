using Smithbox.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.ParamEditorNS;

public class ParamReloader
{
    public Project Project;
    public ParamEditor Editor;

    public ParamReloader(Project project, ParamEditor editor)
    {
        Project = project;
        Editor = editor;
    }

    public void ReloadSelectedParam()
    {

    }

    public void ReloadAllParams()
    {

    }
}
