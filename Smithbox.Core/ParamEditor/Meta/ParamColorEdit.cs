using Smithbox.Core.Editor;
using Smithbox.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Smithbox.Core.ParamEditorNS.Meta;

public class ParamColorEdit
{
    public string Name;
    public string Fields;
    public string PlacedField;

    public ParamColorEdit(ParamMeta curMeta, XmlNode colorEditNode)
    {
        Name = "";
        Fields = "";
        PlacedField = "";

        if (colorEditNode.Attributes["Name"] != null)
        {
            Name = colorEditNode.Attributes["Name"].InnerText;
        }
        else
        {
            TaskLogs.AddLog($"[{curMeta.DataParent.Project.ProjectName}:Param Editor] {curMeta.Name} - Unable to populate ParamColorEdit Name property for {colorEditNode.Name}");
        }
        if (colorEditNode.Attributes["Fields"] != null)
        {
            Fields = colorEditNode.Attributes["Fields"].InnerText;
        }
        else
        {
            TaskLogs.AddLog($"[{curMeta.DataParent.Project.ProjectName}:Param Editor] {curMeta.Name} - Unable to populate ParamColorEdit Fields property for {colorEditNode.Name}");
        }
        if (colorEditNode.Attributes["PlacedField"] != null)
        {
            PlacedField = colorEditNode.Attributes["PlacedField"].InnerText;
        }
        else
        {
            TaskLogs.AddLog($"[{curMeta.DataParent.Project.ProjectName}:Param Editor] Unable to populate ParamColorEdit PlacedField property for {colorEditNode.Name}");
        }
    }
}