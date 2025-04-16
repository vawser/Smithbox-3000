using Smithbox.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Smithbox.Core.ParamEditorNS.Meta;

/// <summary>
/// The display name for the param in the param list
/// </summary>
public class ParamDisplayName
{
    public string Param;
    public string Name;

    public ParamDisplayName(ParamMeta curMeta, XmlNode node)
    {
        Param = "";
        Name = "";

        if (node.Attributes["Param"] != null)
        {
            Param = node.Attributes["Param"].InnerText;
        }
        else
        {
            TaskLogs.AddLog($"[{curMeta.DataParent.Project.ProjectName}:Param Editor] {curMeta.Name} - Unable to populate ParamColorEdit Name property for {Param}");
        }

        if (node.Attributes["Name"] != null)
        {
            Name = node.Attributes["Name"].InnerText;
        }
        else
        {
            TaskLogs.AddLog($"[{curMeta.DataParent.Project.ProjectName}:Param Editor] {curMeta.Name} - Unable to populate ParamColorEdit Name property for {Name}");
        }
    }
}
