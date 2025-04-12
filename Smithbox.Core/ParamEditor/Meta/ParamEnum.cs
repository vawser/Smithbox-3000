using Microsoft.Extensions.Logging;
using Smithbox.Core.ParamEditorNS;
using Smithbox.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Smithbox.Core.ParamEditorNS.Meta;

public class ParamEnum
{
    public string Name;

    public Dictionary<string, string> Values = new(); // using string as an intermediate type. first string is value, second is name.

    public ParamEnum(ParamMeta curMeta, XmlNode enumNode)
    {
        Name = "";

        if (enumNode.Attributes["Name"] != null)
        {
            Name = enumNode.Attributes["Name"].InnerText;
        }
        else
        {
            TaskLogs.AddLog($"PARAM META: {curMeta.Name} - Unable to populate ParamEnum Name property for {enumNode.Name}", LogLevel.Error);
        }

        foreach (XmlNode option in enumNode.SelectNodes("Option"))
        {
            if (option.Attributes["Value"] != null)
            {
                Values[option.Attributes["Value"].InnerText] = option.Attributes["Name"].InnerText;
            }
            else
            {
                TaskLogs.AddLog($"PARAM META: {curMeta.Name} - Unable to populate ParamEnum Option Attribute Value property for {enumNode.Name}", LogLevel.Error);
            }
        }
    }
}