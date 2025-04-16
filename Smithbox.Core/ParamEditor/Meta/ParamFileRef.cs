using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.ParamEditorNS.Meta;

/// <summary>
/// Points to a file reference: includes a jump to the File Browser, selecting the file
/// </summary>
public class ParamFileRef
{
    public string name;
    public List<string> paths;

    internal ParamFileRef(ParamMeta curMeta, string refString)
    {
        var parts = refString.Split(",");
        name = parts[0];
        paths = parts.Skip(1).ToList();
    }

    internal string getStringForm()
    {
        return name + ',' + string.Join(',', paths);
    }
}
